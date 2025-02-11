using System;

namespace Unity.Burst;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
public class BurstCompileAttribute : Attribute
{
	public FloatMode FloatMode { get; set; }

	public FloatPrecision FloatPrecision { get; set; }

	public bool CompileSynchronously { get; set; }

	public bool Debug { get; set; }

	public bool DisableSafetyChecks { get; set; }

	internal string[] Options { get; set; }

	public BurstCompileAttribute()
	{
	}

	public BurstCompileAttribute(FloatPrecision floatPrecision, FloatMode floatMode)
	{
		FloatMode = floatMode;
		FloatPrecision = floatPrecision;
	}

	internal BurstCompileAttribute(string[] options)
	{
		Options = options;
	}
}
