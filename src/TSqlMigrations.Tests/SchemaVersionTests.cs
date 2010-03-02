namespace TSqlMigrations.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using NUnit.Framework;

	[TestFixture]
	public class SchemaVersionTests : AssertionHelper
	{
		[Test]
		public void CreateVersion_InvalidMajorNumber_ThrowsArgumentException()
		{
			TestDelegate action = () => SchemaVersion.CreateVersion("A.2.3.sql");

			Assert.Throws<ArgumentException>(action);
		}

		[Test]
		public void CreateVersion_InvalidMinorNumber_ThrowsArgumentException()
		{
			TestDelegate action = () => SchemaVersion.CreateVersion("1.A.3.sql");

			Assert.Throws<ArgumentException>(action);
		}

		[Test]
		public void CreateVersion_InvalidPointNumber_ThrowsArgumentException()
		{
			TestDelegate action = () => SchemaVersion.CreateVersion("1.2.A.sql");

			Assert.Throws<ArgumentException>(action);
		}

		[Test]
		public void CreateVersion_ValidNumbers_ParsesCorrectly()
		{
			var version = SchemaVersion.CreateVersion("001.002.003.sql");

			Expect(version.Major, Is.EqualTo(1));
			Expect(version.Minor, Is.EqualTo(2));
			Expect(version.Point, Is.EqualTo(3));
		}

		[Test]
		public void TestComparisonOperators()
		{
			var lower = SchemaVersion.CreateVersion(1, 2, 3);
			var higher = SchemaVersion.CreateVersion(1, 2, 5);
			var sameAsLower = SchemaVersion.CreateVersion(1, 2, 3);
			Expect(lower < higher);
			Expect(higher > lower);
			Expect(higher != lower);
			Expect(lower == sameAsLower);
		}

		[Test]
		public void TestSortingAListOfSchemas()
		{
			var versions = new List<SchemaVersion>
			               	{
			               		SchemaVersion.CreateVersion(1, 2, 55),
			               		SchemaVersion.CreateVersion(1, 2, 70),
			               		SchemaVersion.CreateVersion(1, 2, 40),
			               		SchemaVersion.CreateVersion(1, 2, 35)
			               	};

			var sorted = versions.OrderBy(v => v).ToList();
			Expect(sorted[0].Point == 35);
			Expect(sorted[1].Point == 40);
			Expect(sorted[2].Point == 55);
			Expect(sorted[3].Point == 70);
		}
	}
}