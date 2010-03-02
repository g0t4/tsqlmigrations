namespace TSqlMigrations.Core
{
	using System;
	using System.Data.SqlClient;
	using System.Linq;

	public class ExceptionHandler
	{
		private Exception _Exception;

		public ExceptionHandler(Exception exception)
		{
			_Exception = exception;
		}

		// Todo this goes elsewhere, AOP? :)
		public void Display()
		{
			if (_Exception == null)
			{
				return;
			}
#if DEBUG
			Console.WriteLine("Exception message {0}", _Exception);
#else 
			Console.WriteLine("Exception message {0}", _Exception.Message);
#endif
			if (_Exception is SqlException)
			{
				var sqlException = _Exception as SqlException;
				sqlException.Errors.OfType<SqlError>().ToList().ForEach(ShowSqlError);
			}
			DisplayInner();
		}

		private void DisplayInner()
		{
			var exception = _Exception;
			_Exception = _Exception.InnerException;
			Display();
			_Exception = exception;
		}

		public void ShowSqlError(SqlError error)
		{
			Console.WriteLine(error.ToString());
		}
	}
}