namespace TSqlMigrations.Core
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Microsoft.SqlServer.Management.Smo;

	public class DirectoryManager
	{
		public static string DirectoryBase = string.Empty;
		public static string DirectoryScriptUpdates = "Updates";
		public static string FileCreateDatabase = MigrationsConfiguration.FileCreateDatabase;
		public static string FileCreateMethods = MigrationsConfiguration.FileCreateMethods;
		public static string FileSchemaChanges = MigrationsConfiguration.FileSchemaChanges;

		/// <summary>
		/// 	Clears the existing directory and files, so we don't persist objects that are deleted.
		/// </summary>
		/// <param name = "directory"></param>
		/// <returns></returns>
		public static void ClearDirectory(string directory)
		{
			if (!Directory.Exists(directory))
			{
				return;
			}

			var info = new DirectoryInfo(directory);
			info.GetFiles("*.sql").OfType<FileInfo>().ToList().ForEach(f => f.Delete());
		}

		public static void EnsureDirectoryBaseExists()
		{
			if (!string.IsNullOrEmpty(DirectoryBase))
			{
				EnsureDirectoryExists(DirectoryBase);
			}
		}

		public static void EnsureDirectoryExists(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}

		public static string GetNextUpdateFile()
		{
			var updateDirectory = GetUpdateDirectory();
			var lastScript = GetUpdateScripts().OrderByDescending(s => s.Version).FirstOrDefault();

			var major = 0;
			var minor = 0;
			var point = 10;
			if (lastScript != null)
			{
				major = lastScript.Version.Major;
				minor = lastScript.Version.Minor;
				point = lastScript.Version.Point + 10;
			}
			var updateFile = string.Format("{0}.{1}.{2}.sql", major, minor, point);
			return Path.Combine(updateDirectory, updateFile);
		}

		public static string GetUpdateDirectory()
		{
			return GetDirectory(DirectoryScriptUpdates);
		}

		public static string GetDirectory(string subDirectory)
		{
			if (string.IsNullOrEmpty(subDirectory))
			{
				throw new ArgumentException("Must specify individual directories for different object types.");
			}

			return Path.Combine(DirectoryBase, subDirectory);
		}

		public static string GetScriptFileName(NamedSmoObject scriptObject, string directory)
		{
			var name = string.Format("{0}.sql", scriptObject.Name);
			if (scriptObject is ScriptSchemaObjectBase)
			{
				name = (scriptObject as ScriptSchemaObjectBase).Schema + "." + name;
			}
			return Path.Combine(directory, name);
		}

		public static string GetDirectoryBaseFile(string fileName)
		{
			return Path.Combine(DirectoryBase, fileName);
		}


		public static List<UpdateScript> GetUpdateScripts()
		{
			EnsureDirectoryExists(GetUpdateDirectory());
			var info = new DirectoryInfo(GetUpdateDirectory());
			return
				info.GetFiles("*.sql").Select(f => new UpdateScript(f)).OrderBy(s => s.Version).ToList();
		}

		public static List<UpdateScript> GetUpdateScripts(SchemaVersion version)
		{
			return GetUpdateScripts().Where(s => s.Version > version).
				ToList();
		}

		public static string GetFileCreateMethods()
		{
			return GetDirectoryBaseFile(FileCreateMethods);
		}

		public static string GetFileCreateDatabase()
		{
			return GetDirectoryBaseFile(FileCreateDatabase);
		}

		public static string GetFileSchemaChanges()
		{
			return GetDirectoryBaseFile(FileSchemaChanges);
		}

		public static string GetScript(string filePath)
		{
			if (!File.Exists(filePath))
			{
				Console.WriteLine("Cannot find script {0}.", filePath);
				return null;
			}
			return File.ReadAllText(filePath);
		}
	}
}