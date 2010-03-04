namespace TSqlMigrations.Core
{
	using System;
	using System.Linq;
	using Diff;

	public class MigrationsService : IMigrationsService
	{
		protected MigrationsServer _migrationsServer;

		public MigrationsService()
		{
			_migrationsServer = new MigrationsServer();
		}

		#region IMigrationsService Members

		public void LoadSchema()
		{
			Console.WriteLine("Loading schema");
			using (var manager = new TransactionManager(_migrationsServer))
			{
				Console.WriteLine("Creating base");
				RunScript(DirectoryManager.GetFileCreateDatabase());
				Console.WriteLine("Creating methods");
				RunScript(DirectoryManager.GetFileCreateMethods());
				Console.WriteLine("Populating schema changes");
				RunScript(DirectoryManager.GetFileSchemaChanges());
				manager.CommitTransaction();
			}
		}

		public void CreateSchemaLogTable()
		{
			_migrationsServer.Database.CreateSchemaChangeLog();
		}

		public void UpdateDatabase()
		{
			DirectoryManager.GetUpdateScripts(GetVersion()).ForEach(s => s.ApplyUpdate(this));
			_migrationsServer.Database.DropMethods();
			CreateMethods();
		}

		public void UpdateDatabase(SchemaVersion version)
		{
			Console.WriteLine(
				"This is not suggested to be used as the methods might be newer, make sure you have the correct methods version checked out when not updating to the latest schema version.");
			DirectoryManager.GetUpdateScripts(GetVersion()).Where(v => v.Version <= version).ToList().ForEach(
				s => s.ApplyUpdate(this));
		}

		public void ScriptIndividualObjects()
		{
			var scripter = new MigrationsScripter(_migrationsServer);
			scripter.ScriptIndividualObjects();
		}

		public void ScriptCreateDatabase()
		{
			var scripter = new MigrationsScripter(_migrationsServer);
			scripter.ScriptCreateDatabase();
		}

		public void ScriptTableData(string fileName, string tableList)
		{
			var file = DirectoryManager.GetDirectoryBaseFile(fileName);
			var scripter = new MigrationsScripter(_migrationsServer);
			scripter.ScriptDataToFile(file, tableList);
		}

		public void ApplyScript(string script, string name, SchemaVersion version)
		{
			_migrationsServer.Database.ApplyScript(script, name, version);
		}

		public void RunScript(string fileName)
		{
			var script = DirectoryManager.GetScript(fileName);
			_migrationsServer.Database.RunScript(script);
		}

		public void BackupDatabase(string fileName)
		{
			_migrationsServer.Database.Backup(fileName);
		}

		public void RestoreDatabase(string fileName)
		{
			_migrationsServer.RestoreDatabase(fileName);
		}

		public void SetDirectoryBase(string value)
		{
			DirectoryManager.DirectoryBase = value;
		}

		public void GenerateUpdate(string generateUpdateCompareDatabase)
		{
			var diff = new MigrationsDiff(_migrationsServer, generateUpdateCompareDatabase);
			diff.GenerateUpdateFile();
		}

		public void MarkSchemaChangesWithLatestVersion()
		{
			var currentVersion = _migrationsServer.Database.Version;
			var updates = DirectoryManager.GetUpdateScripts(currentVersion);
			updates.ForEach(u => _migrationsServer.Database.UpdateSchemaChangesTable(u.Version, u.UpdateFile.Name));
		}

		public SchemaVersion GetVersion()
		{
			return _migrationsServer.Database.Version;
		}

		public string DatabaseName
		{
			get { return _migrationsServer.Database.Name; }
			set { _migrationsServer.SetDatabase(value); }
		}

		public void SetConnectionString(string connectionString)
		{
			_migrationsServer.ChangeConnection(connectionString);
		}

		public void DropAndCreateDatabase()
		{
			_migrationsServer.DropDatabase();
			_migrationsServer.CreateDatabase();
		}

		#endregion

		private void CreateMethods()
		{
			Console.WriteLine("Creating methods");
			RunScript(DirectoryManager.GetFileCreateMethods());
		}
	}
}