namespace TSqlMigrations
{
	using System;

	public struct SchemaVersion : IComparable
	{
		public int Major { get; private set; }
		public int Minor { get; private set; }
		public int Point { get; private set; }

		public static SchemaVersion CreateVersion(string version)
		{
			var parts = version.Split('.');
			if (parts.Length < 3)
			{
				throw new ArgumentException("Version must have major minor and point sections.", "version");
			}
			return CreateVersion(parts[0], parts[1], parts[2]);
		}

		public static SchemaVersion CreateVersion(string major, string minor, string point)
		{
			int majorParse;
			if (!int.TryParse(major, out majorParse))
			{
				throw new ArgumentException("Invalid major version.", "major");
			}

			int minorParse;
			if (!int.TryParse(minor, out minorParse))
			{
				throw new ArgumentException("Invalid minor version.", "minor");
			}

			int pointParse;
			if (!int.TryParse(point, out pointParse))
			{
				throw new ArgumentException("Invalid point version.", "point");
			}

			return CreateVersion(majorParse, minorParse, pointParse);
		}

		public static SchemaVersion CreateVersion(int major, int minor, int point)
		{
			return new SchemaVersion
			       	{
			       		Major = major,
			       		Minor = minor,
			       		Point = point
			       	};
		}

		#region operator implementations, comparison/equality checks

		public int CompareTo(object compareTarget)
		{
			if (!(compareTarget is SchemaVersion))
			{
				throw new NotSupportedException("Version can only be compared to Version.");
			}
			var comparison = (SchemaVersion) compareTarget;
			if (comparison == this)
			{
				return 0;
			}

			return comparison > this ? -1 : 1;
		}

		public override bool Equals(object comparison)
		{
			return CompareTo(comparison) == 0;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(SchemaVersion source, SchemaVersion target)
		{
			return source.Major == target.Major && source.Minor == target.Minor && source.Point == target.Point;
		}

		public static bool operator !=(SchemaVersion source, SchemaVersion target)
		{
			return !(source == target);
		}

		public static bool operator >(SchemaVersion source, SchemaVersion target)
		{
			if (source == target)
			{
				return false;
			}

			return (source.Major > target.Major)
			       || (source.Major == target.Major && source.Minor > target.Minor)
			       || (source.Major == target.Major && source.Minor == target.Minor && source.Point > target.Point);
		}

		public static bool operator <(SchemaVersion source, SchemaVersion target)
		{
			return source != target && !(source > target);
		}

		public static bool operator <=(SchemaVersion source, SchemaVersion target)
		{
			return source == target || source < target;
		}

		public static bool operator >=(SchemaVersion source, SchemaVersion target)
		{
			return source == target || source > target;
		}

		#endregion

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}", Major, Minor, Point);
		}
	}
}