namespace TSqlMigrations.Core
{
	public interface IMigrationsService
	{
		SchemaVersion GetVersion();
		string DatabaseName { get; set; }
		void ApplyScript(string script, string name, SchemaVersion version);
		void ScriptCreateDatabase();
		void DropAndCreateDatabase();
		void LoadSchema();
		void UpdateDatabase();
		void UpdateDatabase(SchemaVersion version);
		void ScriptIndividualObjects();
		void CreateSchemaLogTable();
		void SetConnectionString(string connectionString);
		void ScriptTableData(string fileName, string tableList);
		void RunScript(string script);
		void BackupDatabase(string fileName);
		void RestoreDatabase(string fileName);
		void SetDirectoryBase(string value);
	}
}