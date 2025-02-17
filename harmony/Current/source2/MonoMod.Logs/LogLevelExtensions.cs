using System;

namespace MonoMod.Logs;

internal static class LogLevelExtensions
{
	public const LogLevel MaxLevel = LogLevel.Assert;

	public static string FastToString(this LogLevel level, IFormatProvider? provider = null)
	{
		switch (level)
		{
		case LogLevel.Spam:
			return "Spam";
		case LogLevel.Trace:
			return "Trace";
		case LogLevel.Info:
			return "Info";
		case LogLevel.Warning:
			return "Warning";
		case LogLevel.Error:
			return "Error";
		case LogLevel.Assert:
			return "Assert";
		default:
		{
			int num = (int)level;
			return num.ToString(provider);
		}
		}
	}
}
