using System;
using System.Runtime.CompilerServices;
using MonoMod.Logs;

namespace MonoMod;

internal static class MMDbgLog
{
	[InterpolatedStringHandler]
	internal ref struct DebugLogSpamStringHandler
	{
		internal DebugLogInterpolatedStringHandler handler;

		public DebugLogSpamStringHandler(int literalLen, int formattedCount, out bool isEnabled)
		{
			handler = new DebugLogInterpolatedStringHandler(literalLen, formattedCount, LogLevel.Spam, out isEnabled);
		}

		public override string ToString()
		{
			return handler.ToString();
		}

		public string ToStringAndClear()
		{
			return handler.ToStringAndClear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLiteral(string s)
		{
			handler.AppendLiteral(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value)
		{
			handler.AppendFormatted(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment)
		{
			handler.AppendFormatted(value, alignment);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, string? format)
		{
			handler.AppendFormatted(value, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment, string? format)
		{
			handler.AppendFormatted(value, alignment, format);
		}
	}

	[InterpolatedStringHandler]
	internal ref struct DebugLogTraceStringHandler
	{
		internal DebugLogInterpolatedStringHandler handler;

		public DebugLogTraceStringHandler(int literalLen, int formattedCount, out bool isEnabled)
		{
			handler = new DebugLogInterpolatedStringHandler(literalLen, formattedCount, LogLevel.Trace, out isEnabled);
		}

		public override string ToString()
		{
			return handler.ToString();
		}

		public string ToStringAndClear()
		{
			return handler.ToStringAndClear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLiteral(string s)
		{
			handler.AppendLiteral(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value)
		{
			handler.AppendFormatted(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment)
		{
			handler.AppendFormatted(value, alignment);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, string? format)
		{
			handler.AppendFormatted(value, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment, string? format)
		{
			handler.AppendFormatted(value, alignment, format);
		}
	}

	[InterpolatedStringHandler]
	internal ref struct DebugLogInfoStringHandler
	{
		internal DebugLogInterpolatedStringHandler handler;

		public DebugLogInfoStringHandler(int literalLen, int formattedCount, out bool isEnabled)
		{
			handler = new DebugLogInterpolatedStringHandler(literalLen, formattedCount, LogLevel.Info, out isEnabled);
		}

		public override string ToString()
		{
			return handler.ToString();
		}

		public string ToStringAndClear()
		{
			return handler.ToStringAndClear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLiteral(string s)
		{
			handler.AppendLiteral(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value)
		{
			handler.AppendFormatted(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment)
		{
			handler.AppendFormatted(value, alignment);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, string? format)
		{
			handler.AppendFormatted(value, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment, string? format)
		{
			handler.AppendFormatted(value, alignment, format);
		}
	}

	[InterpolatedStringHandler]
	internal ref struct DebugLogWarningStringHandler
	{
		internal DebugLogInterpolatedStringHandler handler;

		public DebugLogWarningStringHandler(int literalLen, int formattedCount, out bool isEnabled)
		{
			handler = new DebugLogInterpolatedStringHandler(literalLen, formattedCount, LogLevel.Warning, out isEnabled);
		}

		public override string ToString()
		{
			return handler.ToString();
		}

		public string ToStringAndClear()
		{
			return handler.ToStringAndClear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLiteral(string s)
		{
			handler.AppendLiteral(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value)
		{
			handler.AppendFormatted(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment)
		{
			handler.AppendFormatted(value, alignment);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, string? format)
		{
			handler.AppendFormatted(value, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment, string? format)
		{
			handler.AppendFormatted(value, alignment, format);
		}
	}

	[InterpolatedStringHandler]
	internal ref struct DebugLogErrorStringHandler
	{
		internal DebugLogInterpolatedStringHandler handler;

		public DebugLogErrorStringHandler(int literalLen, int formattedCount, out bool isEnabled)
		{
			handler = new DebugLogInterpolatedStringHandler(literalLen, formattedCount, LogLevel.Error, out isEnabled);
		}

		public override string ToString()
		{
			return handler.ToString();
		}

		public string ToStringAndClear()
		{
			return handler.ToStringAndClear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendLiteral(string s)
		{
			handler.AppendLiteral(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(string? s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s)
		{
			handler.AppendFormatted(s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted(ReadOnlySpan<char> s, int alignment = 0, string? format = null)
		{
			handler.AppendFormatted(s, alignment, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value)
		{
			handler.AppendFormatted(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment)
		{
			handler.AppendFormatted(value, alignment);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, string? format)
		{
			handler.AppendFormatted(value, format);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendFormatted<T>(T value, int alignment, string? format)
		{
			handler.AppendFormatted(value, alignment, format);
		}
	}

	public static bool IsWritingLog => DebugLog.IsWritingLog;

	[ModuleInitializer]
	internal static void LogVersion()
	{
		Info("Version 25.0.4");
	}

	public static void Log(LogLevel level, string message)
	{
		DebugLog.Log("MonoMod.Utils", level, message);
	}

	public static void Log(LogLevel level, [InterpolatedStringHandlerArgument("level")] ref DebugLogInterpolatedStringHandler message)
	{
		DebugLog.Log("MonoMod.Utils", level, ref message);
	}

	public static void Spam(string message)
	{
		Log(LogLevel.Spam, message);
	}

	public static void Spam(ref DebugLogSpamStringHandler message)
	{
		Log(LogLevel.Spam, ref message.handler);
	}

	public static void Trace(string message)
	{
		Log(LogLevel.Trace, message);
	}

	public static void Trace(ref DebugLogTraceStringHandler message)
	{
		Log(LogLevel.Trace, ref message.handler);
	}

	public static void Info(string message)
	{
		Log(LogLevel.Info, message);
	}

	public static void Info(ref DebugLogInfoStringHandler message)
	{
		Log(LogLevel.Info, ref message.handler);
	}

	public static void Warning(string message)
	{
		Log(LogLevel.Warning, message);
	}

	public static void Warning(ref DebugLogWarningStringHandler message)
	{
		Log(LogLevel.Warning, ref message.handler);
	}

	public static void Error(string message)
	{
		Log(LogLevel.Error, message);
	}

	public static void Error(ref DebugLogErrorStringHandler message)
	{
		Log(LogLevel.Error, ref message.handler);
	}
}
