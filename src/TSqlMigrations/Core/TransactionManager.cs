namespace TSqlMigrations.Core
{
	using System;

	public class TransactionManager : IDisposable
	{
		private readonly MigrationsServer _server;
		private readonly int _transactionDepth;

		public TransactionManager(MigrationsServer server)
		{
			_server = server;
			server.BeingTransaction();
			_transactionDepth = _server.TransactionDepth();
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_transactionDepth == _server.TransactionDepth())
			{
				_server.RollBack();
			}
		}

		#endregion

		public void CommitTransaction()
		{
			_server.CommitTransaction();
		}
	}
}