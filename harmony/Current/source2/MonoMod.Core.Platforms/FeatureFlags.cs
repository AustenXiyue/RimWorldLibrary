using System;

namespace MonoMod.Core.Platforms;

internal readonly struct FeatureFlags : IEquatable<FeatureFlags>
{
	public ArchitectureFeature Architecture { get; }

	public SystemFeature System { get; }

	public RuntimeFeature Runtime { get; }

	public FeatureFlags(ArchitectureFeature archFlags, SystemFeature sysFlags, RuntimeFeature runtimeFlags)
	{
		Runtime = runtimeFlags;
		Architecture = archFlags;
		System = sysFlags;
	}

	public bool Has(RuntimeFeature feature)
	{
		return (Runtime & feature) == feature;
	}

	public bool Has(ArchitectureFeature feature)
	{
		return (Architecture & feature) == feature;
	}

	public bool Has(SystemFeature feature)
	{
		return (System & feature) == feature;
	}

	public override bool Equals(object? obj)
	{
		if (obj is FeatureFlags other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(FeatureFlags other)
	{
		if (Runtime == other.Runtime && Architecture == other.Architecture)
		{
			return System == other.System;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Runtime, Architecture, System);
	}

	public override string ToString()
	{
		return $"({Architecture})({System})({Runtime})";
	}

	public static bool operator ==(FeatureFlags left, FeatureFlags right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FeatureFlags left, FeatureFlags right)
	{
		return !(left == right);
	}
}
