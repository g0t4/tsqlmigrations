namespace TSqlMigrations.Tests
{
	using System;
	using Core;
	using NUnit.Framework;

	[TestFixture]
	public class PostScriptProcessorTests : AssertionHelper
	{
		[Test]
		public void RemoveSequentialBlankLinesBeforeGo_LineAfterParenthesisBeforeGo_RemovesLine()
		{
			var script = new[]
			             	{
			             		"asdf",
			             		")",
			             		string.Empty,
			             		"GO"
			             	};

			var result = new[]
			             	{
			             		"asdf",
			             		")",
			             		"GO"
			             	};
			var processor = new PostScriptProcessor(null)
			                	{
			                		_Script = string.Join("\r\n", script)
			                	};

			processor.RemoveSequentialBlankLinesBeforeGo();

			var output = processor._Script;
			var expectedResult = String.Join("\r\n", result);
			Expect(output, Is.EqualTo(expectedResult));
		}

		[Test]
		public void RemoveSequentialBlankLinesBeforeGo_LinesAfterGo_LeavesLineAfterGo()
		{
			var script = new[]
			             	{
			             		"asdf",
			             		")",
			             		string.Empty,
			             		string.Empty,
			             		string.Empty,
			             		string.Empty,
			             		"GO",
			             		string.Empty,
			             		")",
			             		string.Empty,
			             		"ASDF"
			             	};
			var result = new[]
			             	{
			             		"asdf",
			             		")",
			             		"GO",
			             		string.Empty,
			             		")",
			             		string.Empty,
			             		"ASDF"
			             	};
			var processor = new PostScriptProcessor(null)
			                	{
			                		_Script = string.Join("\r\n", script)
			                	};

			processor.RemoveSequentialBlankLinesBeforeGo();

			var output = processor._Script;
			var expectedResult = String.Join("\r\n", result);
			Expect(output, Is.EqualTo(expectedResult));
		}
	}
}