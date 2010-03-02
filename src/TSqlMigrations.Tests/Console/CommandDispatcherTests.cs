namespace TSqlMigrations.Tests.Console
{
	using Core;
	using net.sf.dotnetcli;
	using NUnit.Framework;
	using Rhino.Mocks;
	using TSqlMigrations.Console;

	[TestFixture]
	public class CommandDispatcherTests : AssertionHelper
	{
		private const string DatabaseName = "DatabaseName";

		[Test]
		public void DispatchCommand_WithoutDatabase_ThrowsMissingOptionException()
		{
			var dispatcher = NewDispatcher();

			TestDelegate action = () => dispatcher.DispatchCommand(new[] {"-sv"});

			Assert.Throws<MissingOptionException>(action);
		}

		private static CommandDispatcher NewDispatcher()
		{
			var migrationsService = MockRepository.GenerateStub<IMigrationsService>();
			return new CommandDispatcher(migrationsService);
		}

		[Test]
		public void DispatchCommand_WithScriptCreateOption_CallsScriptCreateDatabase()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-sc"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.ScriptCreateDatabase());
		}

		[Test]
		public void DispatchCommand_WithScriptIndividualOption_CallsScriptIndividualObjects()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-si"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.ScriptIndividualObjects());
		}

		[Test]
		public void DispatchCommand_WithGetSchemaVersionOption_CallsGetVersion()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-sv"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.GetVersion());
		}

		[Test]
		public void DispatchCommand_WithLoadSchemaOption_CallsLoadSchema()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-cd"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.LoadSchema());
		}

		[Test]
		public void DispatchCommand_WithCreateDatabaseOption_CallsDropAndCreateDatabaseThenLoadSchema()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-cd", "-r"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.DropAndCreateDatabase());
			dispatcher.MigrationsService.AssertWasCalled(s => s.LoadSchema());
		}

		[Test]
		public void DispatchCommand_WithUpdateDatabaseOption_CallsUpdateDatabase()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-u"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.UpdateDatabase());
		}

		[Test]
		public void DispatchCommand_WithUpdateDatabaseToVersionOption_CallsUpdateDatabaseToVersion()
		{
			var version = SchemaVersion.CreateVersion("1.2.3");
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-u", "1.2.3"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.UpdateDatabase(version));
		}

		[Test]
		public void DispatchCommand_WithCreateSchemaLogTableOption_CallsCreateSchemaLogTable()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-createSchemaLogTable"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.CreateSchemaLogTable());
		}

		[Test]
		public void DispatchCommand_WithDirectoryBaseOption_SetsDirectoryBase()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-b", "base"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.SetDirectoryBase("base"));
		}

		[Test]
		public void DispatchCommand_WithSetConnectionStringOption_CallsSetConnectionString()
		{
			const string connectionString = "test";
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-cs", connectionString});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.SetConnectionString(connectionString));
		}

		[Test]
		public void DispatchCommand_WithScriptTableDataOption_CallsScriptTableData()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-td", "TestScript.sql", "table1,table2"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.ScriptTableData("TestScript.sql", "table1,table2"));
		}

		[Test]
		public void DispatchCommand_WithRunScriptOption_CallsRunScript()
		{
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-rs", "Lookups.sql"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			dispatcher.MigrationsService.AssertWasCalled(s => s.RunScript("Lookups.sql"));
		}

		[Test]
		public void DispatchCommand_WithNoWaitOption_WaitIsFalse()
		{
			var dispatcher = NewDispatcher();

			var result = dispatcher.DispatchCommand(new[] {"-d", DatabaseName, "-nw"});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			Expect(result.Wait, Is.False);
		}

		[Test]
		public void DispatchCommand_WithOutNoWaitOption_WaitIsTrue()
		{
			var dispatcher = NewDispatcher();

			var result = dispatcher.DispatchCommand(new[] {"-d", DatabaseName});

			Expect(dispatcher.MigrationsService.DatabaseName, Is.EqualTo(DatabaseName));
			Expect(result.Wait);
		}

		[Test]
		public void DispatchCommand_BackupFlag_CallsBackup()
		{
			var backupFlag = "-bak";
			var backupFile = "backupTo";
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, backupFlag, backupFile});

			dispatcher.MigrationsService.AssertWasCalled(g => g.BackupDatabase(backupFile));
		}

		[Test]
		public void DispatchCommand_RestoreFlag_CallsRestore()
		{
			var restoreFlag = "-rbak";
			var restoreFile = "restoreFrom";
			var dispatcher = NewDispatcher();

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, restoreFlag, restoreFile});

			dispatcher.MigrationsService.AssertWasCalled(g => g.RestoreDatabase(restoreFile));
		}

		[Test]
		public void DispatchCommand_BackupAndUpdate_BackupHappensFirst()
		{
			var backupFlag = "-bak";
			var backupFile = "backupTo";
			var updateFlag = "-u";
			var mockRepository = new MockRepository();
			var service = mockRepository.DynamicMock<IMigrationsService>();
			var dispatcher = new CommandDispatcher(service);
			using (mockRepository.Ordered())
			{
				service.Expect(g => g.BackupDatabase(backupFile));
				service.Expect(g => g.UpdateDatabase());
			}
			mockRepository.Replay(service);

			dispatcher.DispatchCommand(new[] {"-d", DatabaseName, updateFlag, backupFlag, backupFile});

			mockRepository.Verify(service);
		}
	}
}