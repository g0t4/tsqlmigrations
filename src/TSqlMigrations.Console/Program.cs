namespace TSqlMigrations.Console
{
	using System;
	using Core;

	public class Program
	{
		private static void Main(string[] args)
		{
			var gateway = new MigrationsService();
			var dispatcher = new CommandDispatcher(gateway);

			var result = dispatcher.DispatchCommand(args);
			if (result.Wait)
			{
				Console.ReadKey();
			}

			Environment.ExitCode = result.Success ? 0 : 1;
		}
	}
}