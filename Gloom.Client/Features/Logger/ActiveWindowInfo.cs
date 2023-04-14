using System.Diagnostics.CodeAnalysis;

namespace Feckdoor
{
	internal struct ActiveWindowInfo
	{
		public string Name;
		public string Executable;

		public static bool operator ==(ActiveWindowInfo first, ActiveWindowInfo second) => first.Equals(second);

		public static bool operator !=(ActiveWindowInfo first, ActiveWindowInfo second) => !(first == second);

		public bool Equals(ActiveWindowInfo other) => Name == other.Name && Executable == other.Executable;

		public override bool Equals([NotNullWhen(true)] object? obj) => obj is ActiveWindowInfo other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(Name, Executable);
	}
}
