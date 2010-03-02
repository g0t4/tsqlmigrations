namespace TSqlMigrations.Core
{
	using System;
	using System.Data.SqlClient;
	using System.IO;
	using Microsoft.SqlServer.Management.Common;
	using Microsoft.SqlServer.Management.Smo;

	public class MigrationsServer
	{
		private string _DatabaseName;

		private Scripter _Scripter;
		private Server _Server;
		private MigrationsDatabase _database;

		public MigrationsServer()
		{
			ChangeConnection(MigrationsConfiguration.ServerConnection);
		}

		public Scripter Scripter
		{
			get { return _Scripter; }
		}

		public MigrationsDatabase Database
		{
			get { return _database; }
		}

		public void ChangeConnection(string connectionString)
		{
			_Server = new Server(new ServerConnection(new SqlConnection(connectionString)));

			// W00t these weren't being run cuz resetting the connection used to not do them, only the initial connection!
			// Load IsSystemObject property to speed up filtering out system objects.
			_Server.SetDefaultInitFields(typeof (StoredProcedure), "IsSystemObject");
			_Server.SetDefaultInitFields(typeof (SqlAssembly), "IsSystemObject");
			_Server.SetDefaultInitFields(typeof (View), "IsSystemObject");
			_Server.SetDefaultInitFields(typeof (UserDefinedFunction), "IsSystemObject");
			_Server.SetDefaultInitFields(typeof (Table), "IsSystemObject");

			_Scripter = new Scripter(_Server);

			SetDatabase(_DatabaseName);
		}

		public void SetDatabase(string name)
		{
			_DatabaseName = name;
			if (name == null)
			{
				_database = new MigrationsDatabase(null, this);
				return;
			}
			_database = new MigrationsDatabase(_Server.Databases[name], this);
			if (!_database.Exists)
			{
				Console.WriteLine("Database not found {0}", name);
			}
		}

		public void CreateDatabase()
		{
			if (_Server.Databases.Contains(_DatabaseName))
			{
				Console.WriteLine("Database {0} already exists, can't create.", _DatabaseName);
				return;
			}

			Console.WriteLine("Creating database {0}", _DatabaseName);
			var database = new Database(_Server, _DatabaseName);
			database.Create();
			_database = new MigrationsDatabase(database, this);
		}

		public void DropDatabase()
		{
			if (!_Server.Databases.Contains(_DatabaseName))
			{
				Console.WriteLine("Database doesn't exist {0}, can't delete.", _DatabaseName);
				return;
			}

			Console.WriteLine("Deleting database {0}", _DatabaseName);
			SetDatabaseSingleUserMode();
			_Server.Databases[_DatabaseName].Drop();

			_database = new MigrationsDatabase(null, this);
		}

		private void SetDatabaseSingleUserMode()
		{
			_Server.Databases["master"].ExecuteNonQuery(
				string.Format("ALTER DATABASE [{0}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE", _DatabaseName));
		}

		public void RollBack()
		{
			Console.WriteLine("Rolling back transaction");
			_Server.ConnectionContext.RollBackTransaction();
		}

		public void CommitTransaction()
		{
			_Server.ConnectionContext.CommitTransaction();
		}

		public void BeingTransaction()
		{
			_Server.ConnectionContext.BeginTransaction();
		}

		public SqlConnection GetConnection()
		{
			return _Server.ConnectionContext.SqlConnectionObject;
		}

		public Server GetServer()
		{
			return _Server;
		}

		public void RestoreDatabase(string fileName)
		{
			CreateDatabase();

			var restore = new Restore
			              	{
			              		Database = _DatabaseName,
			              		Action = RestoreActionType.Database,
			              		NoRecovery = false,
			              		ReplaceDatabase = true
			              	};

			var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

			var device = new BackupDeviceItem(path, DeviceType.File);
			restore.Devices.Add(device);
			var backupHeader = restore.ReadBackupHeader(_Server);
			var originalDatabaseName = backupHeader.Rows[0]["DatabaseName"].ToString();

			if (originalDatabaseName != _DatabaseName)
			{
				var dataFile = new RelocateFile
				               	{
				               		PhysicalFileName = Path.Combine(_Server.DefaultFile, _DatabaseName + ".mdb"),
				               		LogicalFileName = originalDatabaseName
				               	};
				Console.WriteLine("Relocating {0} to {1}", dataFile.LogicalFileName, dataFile.PhysicalFileName);
				restore.RelocateFiles.Add(dataFile);

				var logFile = new RelocateFile
				              	{
				              		PhysicalFileName = Path.Combine(_Server.DefaultLog, _DatabaseName + "_log.ldb"),
				              		LogicalFileName = originalDatabaseName + "_log"
				              	};
				Console.WriteLine("Relocating {0} to {1}", logFile.LogicalFileName, logFile.PhysicalFileName);
				restore.RelocateFiles.Add(logFile);
			}

			_Server.KillAllProcesses(_DatabaseName);
			restore.PercentCompleteNotification = 10;
			restore.PercentComplete += (sender, e) => Console.WriteLine(e.Percent + "%");
			restore.SqlRestore(_Server);
		}

		public int TransactionDepth()
		{
			return _Server.ConnectionContext.TransactionDepth;
		}
	}
}