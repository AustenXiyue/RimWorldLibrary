using System.Diagnostics;

namespace System;

internal static class BCLDebug
{
	[Conditional("_DEBUG")]
	public static void Assert(bool condition, string message)
	{
	}

	[Conditional("_DEBUG")]
	internal static void Correctness(bool expr, string msg)
	{
	}

	[Conditional("_DEBUG")]
	public static void Log(string message)
	{
	}

	[Conditional("_DEBUG")]
	public static void Log(string switchName, string message)
	{
	}

	[Conditional("_DEBUG")]
	public static void Log(string switchName, LogLevel level, params object[] messages)
	{
	}

	[Conditional("_DEBUG")]
	internal static void Perf(bool expr, string msg)
	{
	}

	[Conditional("_LOGGING")]
	public static void Trace(string switchName, params object[] messages)
	{
	}

	internal static bool CheckEnabled(string switchName)
	{
		return false;
	}
}
