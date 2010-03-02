namespace TSqlMigrations.Console
{
	using net.sf.dotnetcli;

	public class ArgumentParser
	{
		private const string OptionDatabase = "d";
		private const string OptionScriptCreate = "sc";
		private const string OptionScriptIndividual = "si";
		private const string OptionSchemaVersion = "sv";
		private const string OptionCreateDatabase = "cd";
		private const string OptionResetDatabase = "r";
		private const string OptionUpdateDatabase = "u";
		private const string OptionCreateSchemaLogTable = "createSchemaLogTable";
		private const string OptionSetDirectoryBase = "b";
		private const string OptionSetConnectionString = "cs";
		private const string OptionTableData = "td";
		private const string OptionNoWait = "nw";
		private const string OptionRunScript = "rs";
		private const string OptionBackup = "bak";
		private const string OptionRestore = "rbak";


		private GnuParser _Parser;

		private CommandLine _Line;

		public ArgumentParser(string[] arguments)
		{
			_Parser = new GnuParser();

			var options = new Options();
			options.AddOption(OptionBuilder.Factory
			                  	.IsRequired()
			                  	.WithLongOpt("database")
			                  	.HasArg()
			                  	.Create(OptionDatabase));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("scriptCreate")
			                  	.WithArgName(OptionScriptCreate)
			                  	.Create(OptionScriptCreate));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("scriptIndividual")
			                  	.Create(OptionScriptIndividual));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("schemaVersion")
			                  	.Create(OptionSchemaVersion));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("createDatabase")
			                  	.Create(OptionCreateDatabase));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("resetDatabase")
			                  	.Create(OptionResetDatabase));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("noWait")
			                  	.Create(OptionNoWait));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("setDirectoryBase")
			                  	.HasArg()
			                  	.Create(OptionSetDirectoryBase));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("setConnectionString")
			                  	.HasArg()
			                  	.Create(OptionSetConnectionString));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("tableData")
			                  	.HasArgs(2)
			                  	.Create(OptionTableData));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt(OptionCreateSchemaLogTable)
			                  	.Create(OptionCreateSchemaLogTable));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("update")
			                  	.HasOptionalArg()
			                  	.Create(OptionUpdateDatabase));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("runScript")
			                  	.HasArg()
			                  	.Create(OptionRunScript));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("backup")
			                  	.HasArg()
			                  	.Create(OptionBackup));
			options.AddOption(OptionBuilder.Factory
			                  	.WithLongOpt("restore")
			                  	.HasArg()
			                  	.Create(OptionRestore));

			_Line = _Parser.Parse(options, arguments);
		}

		public string Database
		{
			get { return _Line.GetOptionValue(OptionDatabase); }
		}

		public bool ScriptCreate
		{
			get { return _Line.HasOption(OptionScriptCreate); }
		}

		public bool ScriptIndividual
		{
			get { return _Line.HasOption(OptionScriptIndividual); }
		}

		public bool GetSchemaVersion
		{
			get { return _Line.HasOption(OptionSchemaVersion); }
		}

		public bool CreateDatabase
		{
			get { return _Line.HasOption(OptionCreateDatabase); }
		}

		public bool UpdateDatabase
		{
			get { return _Line.HasOption(OptionUpdateDatabase); }
		}

		public string UpdateDatabaseVersion
		{
			get { return GetOptionValue(OptionUpdateDatabase); }
		}

		public bool ResetDatabase
		{
			get { return _Line.HasOption(OptionResetDatabase); }
		}

		public bool CreateSchemaLogTable
		{
			get { return _Line.HasOption(OptionCreateSchemaLogTable); }
		}

		public string[] Arguments
		{
			get { return _Line.Args; }
		}

		public string DirectoryBase
		{
			get { return GetOptionValue(OptionSetDirectoryBase); }
		}

		public string ConnectionString
		{
			get { return GetOptionValue(OptionSetConnectionString); }
		}

		public string TableData
		{
			get { return GetOptionValue(OptionTableData, 1); }
		}

		public string TableDataFile
		{
			get { return GetOptionValue(OptionTableData, 0); }
		}

		public bool Wait
		{
			get { return !_Line.HasOption(OptionNoWait); }
		}

		public bool RunScript
		{
			get { return _Line.HasOption(OptionRunScript); }
		}

		public string ScriptToRun
		{
			get { return GetOptionValue(OptionRunScript, 0); }
		}

		private string GetOptionValue(string option, int position)
		{
			if (_Line.HasOption(option) && _Line.GetOptionValues(option).Length > position)
			{
				return _Line.GetOptionValues(option)[position];
			}

			return null;
		}

		private string GetOptionValue(string option)
		{
			if (_Line.HasOption(option))
			{
				return _Line.GetOptionValue(option);
			}

			return null;
		}

		public bool Backup
		{
			get { return _Line.HasOption(OptionBackup); }
		}

		public string BackupFile
		{
			get { return GetOptionValue(OptionBackup, 0); }
		}

		public bool Restore
		{
			get { return _Line.HasOption(OptionRestore); }
		}

		public string RestoreFile
		{
			get { return GetOptionValue(OptionRestore, 0); }
		}
	}
}