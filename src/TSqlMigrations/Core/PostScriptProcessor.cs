namespace TSqlMigrations.Core
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;

	public class PostScriptProcessor
	{
		private readonly string _FileName;
		public string _Script;

		public PostScriptProcessor(string fileName)
		{
			_FileName = fileName;
		}

		public void Execute()
		{
			LoadFile();
			CheckForFailures();
			RemoveSequentialBlankLinesBeforeGo();
			SaveFile();
		}

		private void SaveFile()
		{
			File.WriteAllText(_FileName, _Script);
		}

		public void CheckForFailures()
		{
			var matches = Regex.Matches(_Script, @"^.*Cannot script Unresolved Entities.*$",
			                            RegexOptions.Multiline | RegexOptions.IgnoreCase);
			if (matches.Count <= 0)
			{
				return;
			}

			Console.WriteLine("\nUnresolved entities during scripting:");
			matches.OfType<Match>().ToList().ForEach(m => Console.WriteLine(m.Value));
		}

		public void RemoveSequentialBlankLinesBeforeGo()
		{
			_Script = Regex.Replace(_Script, @"[\r\n]{2,}GO", "\r\nGO", RegexOptions.Multiline | RegexOptions.IgnoreCase);
		}


		private void LoadFile()
		{
			_Script = File.ReadAllText(_FileName);
		}
	}
}