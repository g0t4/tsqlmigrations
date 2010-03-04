namespace TSqlMigrations.Core
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.IO;
	using System.Linq;
	using Microsoft.SqlServer.Management.Smo;

	public class MigrationsDatabase
	{
		public static string SchemaChangesTableName = "SchemaChanges";
		public static string SchemaChangesTableSchema = "dbo";
		private readonly Database _Database;
		private readonly MigrationsServer _migrationsServer;

		private SchemaVersion? _Version;

		public MigrationsDatabase(Database database, MigrationsServer migrationsServer)
		{
			_Database = database;
			_migrationsServer = migrationsServer;
		}

		public SchemaVersion Version
		{
			get
			{
				if (!_Version.HasValue)
				{
					// Get schema version of database
					LoadSchemaVersion();
				}

				return _Version.Value;
			}
		}

		public bool Exists
		{
			get { return _Database != null; }
		}

		public string Name
		{
			get { return _Database.Name; }
		}

		public List<UserDefinedAggregate> UserDefinedAggregates
		{
			get { return _Database.UserDefinedAggregates.OfType<UserDefinedAggregate>().ToList(); }
		}

		public List<UserDefinedFunction> UserDefinedFunctions
		{
			get { return _Database.UserDefinedFunctions.OfType<UserDefinedFunction>().Where(p => !p.IsSystemObject).ToList(); }
		}

		public List<View> Views
		{
			get { return _Database.Views.OfType<View>().Where(p => !p.IsSystemObject).ToList(); }
		}

		public List<Trigger> TableTriggers
		{
			get { return Tables.SelectMany(t => t.Triggers.OfType<Trigger>()).ToList(); }
		}

		public List<Table> Tables
		{
			get { return _Database.Tables.OfType<Table>().Where(p => !p.IsSystemObject).ToList(); }
		}

		public List<StoredProcedure> StoredProcedures
		{
			get { return _Database.StoredProcedures.OfType<StoredProcedure>().Where(p => !p.IsSystemObject).ToList(); }
		}

		public List<SqlAssembly> Assemblies
		{
			get { return _Database.Assemblies.OfType<SqlAssembly>().Where(p => !p.IsSystemObject).ToList(); }
		}

		public List<UserDefinedDataType> UserDefinedDataTypes
		{
			get { return _Database.UserDefinedDataTypes.OfType<UserDefinedDataType>().ToList(); }
		}

		public List<UserDefinedTableType> UserDefinedTableTypes
		{
			get { return _Database.UserDefinedTableTypes.OfType<UserDefinedTableType>().ToList(); }
		}

		public List<UserDefinedType> UserDefinedTypes
		{
			get { return _Database.UserDefinedTypes.OfType<UserDefinedType>().ToList(); }
		}

		public List<Schema> Schemas
		{
			get
			{
				return _Database.Schemas.OfType<Schema>()
					.Where(s => s.Owner == "dbo")
					.Where(
						s => MigrationsConfiguration.SchemasToIgnore.Contains(s.Name))
					.ToList();
			}
		}

		private void LoadSchemaVersion()
		{
			var schemaChanges = GetTable(SchemaChangesTableName, SchemaChangesTableSchema);
			if (schemaChanges == null)
			{
				throw new ApplicationException("Schema changes table does not exist.");
			}
			if (schemaChanges.RowCount < 1)
			{
				throw new ApplicationException("No schema changes records");
			}

			var script =
				string.Format("SELECT TOP 1 Major, Minor, Point FROM {0}.{1} ORDER BY Major DESC, Minor DESC, Point DESC",
				              SchemaChangesTableSchema, SchemaChangesTableName);
			var set = _Database.ExecuteWithResults(script);

			var row = set.Tables[0].Rows[0];
			_Version = SchemaVersion.CreateVersion(
				row["Major"].ToString(),
				row["Minor"].ToString(),
				row["Point"].ToString());
		}


		public void RunScript(string script)
		{
			if (script == null)
			{
				Console.WriteLine("No script to run, returning");
				return;
			}
			using (var manager = new TransactionManager(_migrationsServer))
			{
				_Database.ExecuteNonQuery(script);
				manager.CommitTransaction();
			}
		}

		public void ApplyScript(string script, string name, SchemaVersion version)
		{
			if (version <= Version)
			{
				throw new ArgumentOutOfRangeException("version", "Cannot update with an older version");
			}

			using (var manager = new TransactionManager(_migrationsServer))
			{
				RunScript(script);
				UpdateSchemaChangesTable(version, name);
				manager.CommitTransaction();
			}
		}

		public void UpdateSchemaChangesTable(SchemaVersion version, string name)
		{
			Console.WriteLine("Adding version {0} {1} to schema changes table", version, name);
			// Todo error handling here? Create schema table if it no exists?
			var command =
				new SqlCommand(
					"INSERT INTO SchemaChanges (Major, Minor, Point, ScriptName,DateApplied) VALUES (@Major, @Minor, @Point, @ScriptName, GETDATE())");
			command.Parameters.AddWithValue("Major", version.Major);
			command.Parameters.AddWithValue("Minor", version.Minor);
			command.Parameters.AddWithValue("Point", version.Point);
			command.Parameters.AddWithValue("ScriptName", name);
			command.Connection = _migrationsServer.GetConnection();
			command.ExecuteNonQuery();

			_Version = version;
		}

		// Todo how about a back up class that is created and a save method?
		public void Backup(string fileName)
		{
			var backup = new Backup
			             	{
			             		Database = _Database.Name,
			             		Action = BackupActionType.Database,
			             		Incremental = false,
			             		CompressionOption = BackupCompressionOptions.On,
			             		Initialize = true,
			             		FormatMedia = true,
			             		SkipTapeHeader = true
			             	};

			var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

			var device = new BackupDeviceItem(path, DeviceType.File);
			backup.Devices.Add(device);

			backup.PercentCompleteNotification = 10;
			backup.PercentComplete += (sender, e) => Console.WriteLine(e.Percent + "%");

			backup.SqlBackup(_migrationsServer.GetServer());
		}

		public void DropMethods()
		{
			Console.WriteLine("Dropping methods");

			Console.WriteLine("Dropping procedures");
			StoredProcedures.ForEach(p => p.Drop());

			Console.WriteLine("Dropping views");
			Views.ForEach(v => v.Drop());

			Console.WriteLine("Dropping user defined functions");
			UserDefinedFunctions.ForEach(f => f.Drop());

			Console.WriteLine("Dropping triggers");
			TableTriggers.ForEach(t => t.Drop());

			Console.WriteLine("Dropping user defined functions");
			UserDefinedAggregates.ForEach(t => t.Drop());
		}

		public Table GetTable(string name, string schema)
		{
			if (!_Database.Tables.Contains(name, schema))
			{
				return null;
			}
			return _Database.Tables[name, schema];
		}

		public void CreateSchemaChangeLog()
		{
			var schemaChangesTable = GetTable(SchemaChangesTableSchema, SchemaChangesTableName);
			if (schemaChangesTable != null)
			{
				Console.WriteLine("Schema changes table already exists, not creating.");
			}
			schemaChangesTable = new Table(_Database, SchemaChangesTableName, SchemaChangesTableSchema);
			var idColumn = new Column(schemaChangesTable, "Id", DataType.Int) {Identity = true, Nullable = false};
			schemaChangesTable.Columns.Add(idColumn);
			schemaChangesTable.Columns.Add(new Column(schemaChangesTable, "Major", DataType.Int) {Nullable = false});
			schemaChangesTable.Columns.Add(new Column(schemaChangesTable, "Minor", DataType.Int) {Nullable = false});
			schemaChangesTable.Columns.Add(new Column(schemaChangesTable, "Point", DataType.Int) {Nullable = false});
			schemaChangesTable.Columns.Add(new Column(schemaChangesTable, "ScriptName", DataType.NVarChar(50)) {Nullable = false});
			schemaChangesTable.Columns.Add(new Column(schemaChangesTable, "DateApplied", DataType.DateTime) {Nullable = false});
			schemaChangesTable.Create();
		}
	}
}