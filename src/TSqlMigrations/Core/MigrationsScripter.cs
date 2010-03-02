namespace TSqlMigrations.Core
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using Microsoft.SqlServer.Management.Sdk.Sfc;
	using Microsoft.SqlServer.Management.Smo;

	public class MigrationsScripter
	{
		private readonly MigrationsServer _server;
		public string DirectoryAssemblies = "Assemblies";
		public string DirectoryStoredProcedures = "StoredProcedures";
		public string DirectoryTable = "Table";
		public string DirectoryUserDefinedAggregates = "UserDefinedAggregates";
		public string DirectoryUserDefinedDataTypes = "UserDefinedDataTypes";
		public string DirectoryUserDefinedFunctions = "UserDefinedFunctions";
		public string DirectoryUserDefinedTableTypes = "UserDefinedTableTypes";
		public string DirectoryUserDefinedTypes = "UserDefinedTypes";
		public string DirectoryViews = "Views";
		private MigrationsDatabase _database;

		public MigrationsScripter(MigrationsServer server)
		{
			_server = server;
			_database = server.Database;
		}

		public void ScriptIndividualObjects()
		{
			ScriptAssemblies();
			ScriptUserDefinedDataTypes();
			ScriptUserDefinedAggregates();
			ScriptUserDefinedTableTypes();
			ScriptUserDefinedTypes();
			ScriptStoredProcedures();
			ScriptViews();
			ScriptUserDefinedFunctions();
			ScriptTables();
		}

		private ScriptingOptions CreateOptions()
		{
			return new ScriptingOptions
			       	{
			       		IncludeHeaders = false,
			       		AllowSystemObjects = false,
			       		ToFileOnly = true,
			       		Triggers = false,
			       		DriAll = true,
			       		Indexes = true,
			       		Statistics = false,
			       	};
		}

		public void ScriptCreateDatabase()
		{
			Console.WriteLine("Scripting database schema {0}", _database.Name);

			DirectoryManager.EnsureDirectoryBaseExists();

			var watch = new Stopwatch();
			watch.Start();

			var baseUrns = GetBaseUrns();
			ScriptBase(baseUrns);
			PostScriptProcessor processor;
			Console.WriteLine("Done with base {0}", watch.Elapsed.TotalSeconds);

			var methodUrns = GetMethodUrns();
			Console.WriteLine("Done making dependency list {0}", watch.Elapsed.TotalSeconds);

			var methodsName = DirectoryManager.GetFileCreateMethods();
			if (File.Exists(methodsName))
			{
				File.Delete(methodsName);
			}
			if (methodUrns.Count() == 0)
			{
				Console.WriteLine("No methods to script");
			}
			else
			{
				var ordered = WalkMethodDependencies(baseUrns, methodUrns);
				Console.WriteLine("Done walking dependency list {0}", watch.Elapsed.TotalSeconds);

				ScriptMethods(methodsName, ordered);
				Console.WriteLine("Done scripting methods {0}", watch.Elapsed.TotalSeconds);
			}

			ScriptDataToFile(DirectoryManager.GetFileSchemaChanges(), "SchemaChanges");
		}

		private void ScriptMethods(string methodsName, List<Urn> ordered)
		{
			_server.Scripter.Options.FileName = methodsName;
			_server.Scripter.Options.ScriptDrops = false;
			_server.Scripter.Options.IncludeIfNotExists = false;
			_server.Scripter.Script(ordered.ToArray());
			var processor = new PostScriptProcessor(methodsName);
			processor.Execute();
		}

		private List<Urn> WalkMethodDependencies(List<Urn> baseUrns, List<Urn> methodUrns)
		{
			var tree = _server.Scripter.DiscoverDependencies(methodUrns.ToArray(), DependencyType.Parents);
			var values = baseUrns.Select(o => o.Value).ToList();
			var ordered = _server.Scripter.WalkDependencies(tree).Select(n => n.Urn).ToList();
			ordered.RemoveAll(d => values.Contains(d.Value, StringComparer.InvariantCultureIgnoreCase));
			return ordered;
		}

		private List<Urn> GetMethodUrns()
		{
			var others = new List<Urn>();
			others.AddRange(_database.TableTriggers.Select(t => t.Urn));
			others.AddRange(_database.StoredProcedures.Select(a => a.Urn));
			others.AddRange(_database.UserDefinedFunctions.Select(a => a.Urn));
			others.AddRange(_database.UserDefinedAggregates.Select(a => a.Urn));
			others.AddRange(_database.Views.Select(a => a.Urn));
			return others;
		}

		private void ScriptBase(List<Urn> objects)
		{
			_server.Scripter.Options = CreateOptions();
			var fileName = DirectoryManager.GetFileCreateDatabase();
			_server.Scripter.Options.FileName = fileName;
			_server.Scripter.Options.AppendToFile = false;
			_server.Scripter.Script(objects.ToArray());
			var processor = new PostScriptProcessor(fileName);
			processor.Execute();
		}

		private List<Urn> GetBaseUrns()
		{
			var objects = new List<Urn>();
			objects.AddRange(_database.Schemas.Select(a => a.Urn));
			objects.AddRange(_database.Assemblies.Select(a => a.Urn));
			objects.AddRange(_database.UserDefinedTypes.Select(a => a.Urn));
			objects.AddRange(_database.UserDefinedDataTypes.Select(a => a.Urn));
			objects.AddRange(_database.UserDefinedTableTypes.Select(a => a.Urn));
			objects.AddRange(_database.Tables.Select(a => a.Urn));
			return objects;
		}

		public void ScriptDataToFile(string fileName, string tableList)
		{
			Console.WriteLine("Scripting data for {0}", tableList);
			var tables = GetTables(tableList);
			if (tables.Count == 0)
			{
				Console.WriteLine("Warning: No tables to script");
				return;
			}

			tables.ForEach(t => Console.WriteLine(t.Schema + "." + t.Name));
			var orderedTables = WalkDependencies(tables);

			Console.WriteLine("Scripting {0} tables", orderedTables.Count);
			_server.Scripter.Options = CreateOptions();
			_server.Scripter.Options.FileName = fileName;
			ScriptDropTableData(orderedTables);
			ScriptInsertTableData(orderedTables);
		}

		private void ScriptInsertTableData(DependencyCollection orderedTables)
		{
			_server.Scripter.Options.AppendToFile = true;
			_server.Scripter.Options.ScriptData = true;
			_server.Scripter.Options.ScriptDrops = false;
			_server.Scripter.Options.ScriptSchema = false;
			_server.Scripter.Options.IncludeIfNotExists = true;
			_server.Scripter.EnumScriptWithList(orderedTables);
		}

		private void ScriptDropTableData(DependencyCollection orderedTables)
		{
			_server.Scripter.Options.AppendToFile = false;
			_server.Scripter.Options.ScriptData = true;
			_server.Scripter.Options.ScriptDrops = true;
			_server.Scripter.Options.ScriptSchema = false;
			_server.Scripter.EnumScriptWithList(orderedTables);
		}

		private DependencyCollection WalkDependencies(List<Table> tables)
		{
			var tree = _server.Scripter.DiscoverDependencies(tables.ToArray(), DependencyType.Parents);
			var orderedTables = _server.Scripter.WalkDependencies(tree);
			for (var i = 0; i < orderedTables.Count; i++)
			{
				if (!tables.Any(t => string.Compare(t.Urn, orderedTables[i].Urn, true, CultureInfo.InvariantCulture) == 0))
				{
					orderedTables.RemoveAt(i);
					i--;
				}
			}
			return orderedTables;
		}

		private List<Table> GetTables(string tableList)
		{
			var tables = new List<Table>();
			foreach (var table in tableList.Split(','))
			{
				var parts = table.Split('.');
				string tableName;
				string schema;
				switch (parts.Count())
				{
					case 1:
						tableName = table;
						schema = "dbo";
						break;
					case 2:
						tableName = parts[1];
						schema = parts[0];
						break;
					default:
						Console.WriteLine("Invalid table to script {0}", table);
						continue;
				}
				var tableObject = _database.GetTable(tableName, schema);
				if (tableObject == null)
				{
					Console.WriteLine("Warning: Cannot script {0}.{1}, table doesn't exist", schema, tableName);
					continue;
				}
				tables.Add(tableObject);
			}
			return tables;
		}

		public void ScriptAssemblies()
		{
			Console.WriteLine("Scripting assemblies");
			ScriptObjects(DirectoryAssemblies, _database.Assemblies.OfType<NamedSmoObject>(), null);
		}

		public void ScriptUserDefinedDataTypes()
		{
			Console.WriteLine("Scripting user defined data types");
			ScriptObjects(DirectoryUserDefinedDataTypes, _database.UserDefinedDataTypes.OfType<NamedSmoObject>());
		}

		public void ScriptUserDefinedAggregates()
		{
			Console.WriteLine("Scripting user defined aggregates");
			ScriptObjects(DirectoryUserDefinedAggregates, _database.UserDefinedAggregates.OfType<NamedSmoObject>(), null);
		}

		public void ScriptUserDefinedTableTypes()
		{
			Console.WriteLine("Scripting user defined table types");
			ScriptObjects(DirectoryUserDefinedTableTypes, _database.UserDefinedTableTypes.OfType<NamedSmoObject>(), null);
		}

		public void ScriptUserDefinedTypes()
		{
			Console.WriteLine("Scripting user defined types");
			ScriptObjects(DirectoryUserDefinedTypes, _database.UserDefinedTypes.OfType<NamedSmoObject>(), null);
		}

		public void ScriptStoredProcedures()
		{
			Console.WriteLine("Scripting stored procedures");
			ScriptObjects(DirectoryStoredProcedures, _database.StoredProcedures.OfType<NamedSmoObject>(), null);
		}

		public void ScriptViews()
		{
			Console.WriteLine("Scripting views");
			var options = CreateOptions();
			options.DriAll = true;
			options.Indexes = true;
			options.Triggers = true;

			ScriptObjects(DirectoryViews, _database.Views.OfType<NamedSmoObject>(), options);
		}

		public void ScriptUserDefinedFunctions()
		{
			Console.WriteLine("Scripting user defined functions");
			ScriptObjects(DirectoryUserDefinedFunctions, _database.UserDefinedFunctions.OfType<NamedSmoObject>(), null);
		}

		public void ScriptTables()
		{
			Console.WriteLine("Scripting tables");
			var options = CreateOptions();
			options.ClusteredIndexes = true;
			options.DriAll = true;
			options.DriDefaults = true;
			options.DriIncludeSystemNames = false;
			options.Indexes = true;
			options.Triggers = true;

			ScriptObjects(DirectoryTable, _database.Tables.OfType<NamedSmoObject>(), options);
		}

		private void ScriptObjects(string subDirectory, IEnumerable<NamedSmoObject> objects, ScriptingOptions options = null)
		{
			var directory = DirectoryManager.GetDirectory(subDirectory);
			DirectoryManager.ClearDirectory(directory);
			if (objects.Count() == 0)
			{
				return;
			}
			DirectoryManager.EnsureDirectoryExists(directory);

			_server.Scripter.Options = options ?? CreateOptions();
			foreach (var scriptObject in objects)
			{
				ScriptObject(directory, scriptObject);
			}
		}

		private void ScriptObject(string directory, NamedSmoObject scriptObject)
		{
			var fileName = DirectoryManager.GetScriptFileName(scriptObject, directory);

			AppendDropScript(scriptObject, fileName);

			AppendCreateScript(scriptObject);

			var processor = new PostScriptProcessor(fileName);
			processor.Execute();
		}


		private void AppendDropScript(NamedSmoObject scriptObject, string fileName)
		{
			_server.Scripter.Options.FileName = fileName;
			_server.Scripter.Options.IncludeIfNotExists = true;
			_server.Scripter.Options.ScriptDrops = true;
			_server.Scripter.Options.AppendToFile = false;
			_server.Scripter.Script(new[] {scriptObject});
		}

		private void AppendCreateScript(NamedSmoObject scriptObject)
		{
			_server.Scripter.Options.IncludeIfNotExists = false;
			_server.Scripter.Options.ScriptDrops = false;
			_server.Scripter.Options.AppendToFile = true;
			_server.Scripter.Script(new[] {scriptObject});
		}
	}
}