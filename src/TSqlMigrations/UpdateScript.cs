﻿namespace TSqlMigrations
{
	using System;
	using System.IO;
	using Core;

	public class UpdateScript : IUpdateScript
	{
		public SchemaVersion Version { get; private set; }
		public FileInfo UpdateFile { get; private set; }

		public UpdateScript(FileInfo file)
		{
			if (file.Name == null)
			{
				throw new ArgumentNullException("File must not be null.");
			}

			UpdateFile = file;

			Version = SchemaVersion.CreateVersion(UpdateFile.Name);
		}

		public void ApplyUpdate(IMigrationsService migrationsService)
		{
			var script = File.ReadAllText(UpdateFile.FullName);
			System.Console.WriteLine("Applying update {0}", Version);
			migrationsService.ApplyScript(script, UpdateFile.Name, Version);
		}
	}
}