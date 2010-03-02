namespace TSqlMigrations
{
	using System.IO;
	using Core;

	public interface IUpdateScript
	{
		SchemaVersion Version { get; }
		FileInfo UpdateFile { get; }
		void ApplyUpdate(IMigrationsService migrationsService);
	}
}