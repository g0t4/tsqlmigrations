namespace TSqlMigrations.Console
{
	using System;
	using System.Diagnostics;
	using Core;

	public class CommandDispatcher
	{
		public IMigrationsService MigrationsService { get; set; }

		public CommandDispatcher(IMigrationsService database)
		{
			MigrationsService = database;
		}

		public CommandResult DispatchCommand(string[] arguments)
		{
			var watch = new Stopwatch();
			watch.Start();

			var parser = new ArgumentParser(arguments);
			var result = new CommandResult
			             	{
			             		Success = true
			             	};

			try
			{
				InternalDispatch(parser);
			}
			catch (Exception exception)
			{
				var handler = new ExceptionHandler(exception);
				handler.Display();
				result.Success = false;
			}
			Console.WriteLine("Done {0}", watch.Elapsed.TotalSeconds);

			result.Wait = parser.Wait;
			return result;
		}

		private void InternalDispatch(ArgumentParser parser)
		{
			if (!string.IsNullOrEmpty(parser.ConnectionString))
			{
				MigrationsService.SetConnectionString(parser.ConnectionString);
			}
			MigrationsService.DatabaseName = parser.Database;

			if (!string.IsNullOrEmpty(parser.DirectoryBase))
			{
				MigrationsService.SetDirectoryBase(parser.DirectoryBase);
			}
			if (parser.ScriptCreate)
			{
				MigrationsService.ScriptCreateDatabase();
			}
			if (parser.ScriptIndividual)
			{
				MigrationsService.ScriptIndividualObjects();
			}
			if (parser.GetSchemaVersion)
			{
				var version = MigrationsService.GetVersion();
				Console.WriteLine(version.ToString());
			}
			if (parser.CreateDatabase)
			{
				if (parser.ResetDatabase)
				{
					MigrationsService.DropAndCreateDatabase();
				}
				MigrationsService.LoadSchema();
			}
			if (parser.Backup)
			{
				//Backup must be before updates!
				MigrationsService.BackupDatabase(parser.BackupFile);
			}
			if (parser.UpdateDatabase)
			{
				if (!string.IsNullOrEmpty(parser.UpdateDatabaseVersion))
				{
					MigrationsService.UpdateDatabase(SchemaVersion.CreateVersion(parser.UpdateDatabaseVersion));
				}
				else
				{
					MigrationsService.UpdateDatabase();
				}
			}
			if (parser.CreateSchemaLogTable)
			{
				MigrationsService.CreateSchemaLogTable();
			}
			if (!string.IsNullOrEmpty(parser.TableData))
			{
				MigrationsService.ScriptTableData(parser.TableDataFile, parser.TableData);
			}
			if (parser.RunScript)
			{
				MigrationsService.RunScript(parser.ScriptToRun);
			}
			if (parser.Restore)
			{
				MigrationsService.RestoreDatabase(parser.RestoreFile);
			}
			if(parser.GenerateUpdate)
			{
				MigrationsService.GenerateUpdate(parser.GenerateUpdateCompareDatabase);
			}
		}
	}
}