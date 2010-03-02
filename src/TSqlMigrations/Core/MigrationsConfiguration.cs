namespace TSqlMigrations.Core
{
	using System.Collections.Specialized;
	using Properties;

	public static class MigrationsConfiguration
	{
		public static string FileCreateDatabase
		{
			get { return Settings.Default.FileCreateDatabase; }
		}

		public static string FileCreateMethods
		{
			get { return Settings.Default.FileCreateMethods; }
		}

		public static string FileSchemaChanges
		{
			get { return "SchemaChanges.sql"; }
		}

		public static string ServerConnection
		{
			get { return Settings.Default.ServerConnection; }
		}

		public static StringCollection SchemasToIgnore
		{
			get { return Settings.Default.SchemasToIgnore ?? new StringCollection(); }
		}
	}
}