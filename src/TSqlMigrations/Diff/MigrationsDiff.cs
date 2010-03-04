namespace TSqlMigrations.Diff
{
	using System;
	using System.Data.SqlClient;
	using System.IO;
	using Core;
	using DBDiff.Schema;
	using DBDiff.Schema.SQLServer.Generates.Generates;
	using DBDiff.Schema.SQLServer.Generates.Model;
	using DBDiff.Schema.SQLServer.Generates.Options;

	public class MigrationsDiff
	{
		private readonly MigrationsServer _Server;
		private string _ChangedConnectionString;
		private readonly string _OriginalConnectionString;

		public MigrationsDiff(MigrationsServer server, string changedDatabaseName)
		{
			_Server = server;
			_OriginalConnectionString = AddDatabaseToConnectionString(server.Database.Name);
			_ChangedConnectionString = AddDatabaseToConnectionString(changedDatabaseName);
		}

		private string AddDatabaseToConnectionString(string databaseName)
		{
			var builder = new SqlConnectionStringBuilder(_Server.GetConnection().ConnectionString)
			              	{InitialCatalog = databaseName};
			return builder.ToString();
		}

		public void GenerateUpdateFile()
		{
			var changeScript = GenerateChangeScript();
			if (string.IsNullOrEmpty(changeScript))
			{
				Console.WriteLine("No changes to generate in update script.");
				return;
			}
			WriteUpdateFile(changeScript);
		}

		private void WriteUpdateFile(string changeScript)
		{
			var changeFile = DirectoryManager.GetNextUpdateFile();
			Console.WriteLine("Writing update {0} ", changeFile);
			DirectoryManager.EnsureDirectoryExists(DirectoryManager.GetUpdateDirectory());
			var writer = new StreamWriter(changeFile, false);
			try
			{
				writer.Write(changeScript);
			}
			finally
			{
				writer.Close();
			}
		}

		public string GenerateChangeScript()
		{
			var sql = new Generate();
			var original = ProcessOriginalDatabase(sql);
			var changed = ProcessChangedDatabase(sql);
			Diff(original, changed);
			return GetChangeScript(original);
		}

		private string GetChangeScript(Database original)
		{
			Console.WriteLine("Generating SQL file");
			var listDiff = new SQLScriptList();
			listDiff.AddRange(original.UserTypes.ToSqlDiff());
			listDiff.AddRange(original.TablesTypes.ToSqlDiff());
			listDiff.AddRange(original.Tables.ToSqlDiff());
			return listDiff.ToSQL();
		}

		private void Diff(Database original, Database changed)
		{
			Console.WriteLine("Comparing databases schemas...");
			Generate.Compare(original, changed);
		}

		private Database ProcessChangedDatabase(Generate sql)
		{
			sql.ConnectionString = _ChangedConnectionString;
			Console.WriteLine("Reading second database...");
			return sql.Process();
		}

		private Database ProcessOriginalDatabase(Generate sql)
		{
			sql.ConnectionString = _OriginalConnectionString;
			Console.WriteLine("Reading first database...");
			sql.Options = new SqlOption();
			return sql.Process();
		}
	}
}