using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MonoMod.Utils;

namespace MonoMod.Logs;

internal sealed class DebugLog
{
	public delegate void OnLogMessage(string source, DateTime time, LogLevel level, string message);

	public delegate void OnLogMessageDetailed(string source, DateTime time, LogLevel level, string formattedMessage, ReadOnlyMemory<MessageHole> holes);

	private sealed class LogMessage
	{
		public string Source { get; private set; }

		public DateTime Time { get; private set; }

		public LogLevel Level { get; private set; }

		public string FormattedMessage { get; private set; }

		public ReadOnlyMemory<MessageHole> FormatHoles { get; private set; }

		public LogMessage(string source, DateTime time, LogLevel level, string formatted, ReadOnlyMemory<MessageHole> holes)
		{
			Source = source;
			Time = time;
			Level = level;
			FormattedMessage = formatted;
			FormatHoles = holes;
		}

		public void Clear()
		{
			Source = "";
			Time = default(DateTime);
			Level = LogLevel.Spam;
			FormattedMessage = "";
			FormatHoles = default(ReadOnlyMemory<MessageHole>);
		}

		public void Init(string source, DateTime time, LogLevel level, string formatted, ReadOnlyMemory<MessageHole> holes)
		{
			Source = source;
			Time = time;
			Level = level;
			FormattedMessage = formatted;
			FormatHoles = holes;
		}

		public void ReportTo(OnLogMessage del)
		{
			try
			{
				del(Source, Time, Level, FormattedMessage);
			}
			catch (Exception ex)
			{
				Debugger.Log(int.MaxValue, "MonoMod.DebugLog", "Exception caught while reporting to message handler");
				Debugger.Log(int.MaxValue, "MonoMod.DebugLog", ex.ToString());
			}
		}

		public void ReportTo(OnLogMessageDetailed del)
		{
			try
			{
				del(Source, Time, Level, FormattedMessage, FormatHoles);
			}
			catch (Exception ex)
			{
				Debugger.Log(int.MaxValue, "MonoMod.DebugLog", "Exception caught while reporting to message handler");
				Debugger.Log(int.MaxValue, "MonoMod.DebugLog", ex.ToString());
			}
		}
	}

	private sealed class LevelSubscriptions
	{
		public LogLevelFilter ActiveLevels;

		public LogLevelFilter DetailLevels;

		public readonly OnLogMessage?[] SimpleRegs;

		public readonly OnLogMessageDetailed?[] DetailedRegs;

		private const LogLevelFilter ValidFilter = LogLevelFilter.Spam | LogLevelFilter.Trace | LogLevelFilter.Info | LogLevelFilter.Warning | LogLevelFilter.Error | LogLevelFilter.Assert;

		public static readonly LevelSubscriptions None = new LevelSubscriptions();

		private LevelSubscriptions(LogLevelFilter active, LogLevelFilter detail, OnLogMessage?[] simple, OnLogMessageDetailed?[] detailed)
		{
			ActiveLevels = active | detail;
			DetailLevels = detail;
			SimpleRegs = simple;
			DetailedRegs = detailed;
		}

		private LevelSubscriptions()
		{
			ActiveLevels = LogLevelFilter.None;
			DetailLevels = LogLevelFilter.None;
			SimpleRegs = new OnLogMessage[6];
			DetailedRegs = new OnLogMessageDetailed[SimpleRegs.Length];
		}

		private LevelSubscriptions Clone(bool changingDetail)
		{
			OnLogMessage[] array = SimpleRegs;
			OnLogMessageDetailed[] array2 = DetailedRegs;
			if (!changingDetail)
			{
				array = new OnLogMessage[SimpleRegs.Length];
				Array.Copy(SimpleRegs, array, array.Length);
			}
			else
			{
				array2 = new OnLogMessageDetailed[DetailedRegs.Length];
				Array.Copy(DetailedRegs, array2, array2.Length);
			}
			return new LevelSubscriptions(ActiveLevels, DetailLevels, array, array2);
		}

		private void FixFilters()
		{
			ActiveLevels &= LogLevelFilter.Spam | LogLevelFilter.Trace | LogLevelFilter.Info | LogLevelFilter.Warning | LogLevelFilter.Error | LogLevelFilter.Assert;
			DetailLevels &= LogLevelFilter.Spam | LogLevelFilter.Trace | LogLevelFilter.Info | LogLevelFilter.Warning | LogLevelFilter.Error | LogLevelFilter.Assert;
		}

		public LevelSubscriptions AddSimple(LogLevelFilter filter, OnLogMessage del)
		{
			LevelSubscriptions levelSubscriptions = Clone(changingDetail: false);
			levelSubscriptions.ActiveLevels |= filter;
			for (int i = 0; i < levelSubscriptions.SimpleRegs.Length; i++)
			{
				if (((uint)filter & (uint)(1 << i)) != 0)
				{
					Helpers.EventAdd(ref levelSubscriptions.SimpleRegs[i], del);
				}
			}
			levelSubscriptions.FixFilters();
			return levelSubscriptions;
		}

		public LevelSubscriptions RemoveSimple(LogLevelFilter filter, OnLogMessage del)
		{
			LevelSubscriptions levelSubscriptions = Clone(changingDetail: false);
			for (int i = 0; i < levelSubscriptions.SimpleRegs.Length; i++)
			{
				if (((uint)filter & (uint)(1 << i)) != 0 && Helpers.EventRemove(ref levelSubscriptions.SimpleRegs[i], del) == null)
				{
					levelSubscriptions.ActiveLevels &= (LogLevelFilter)(~(1 << i));
				}
			}
			levelSubscriptions.ActiveLevels |= levelSubscriptions.DetailLevels;
			levelSubscriptions.FixFilters();
			return levelSubscriptions;
		}

		public LevelSubscriptions AddDetailed(LogLevelFilter filter, OnLogMessageDetailed del)
		{
			LevelSubscriptions levelSubscriptions = Clone(changingDetail: true);
			levelSubscriptions.DetailLevels |= filter;
			for (int i = 0; i < levelSubscriptions.DetailedRegs.Length; i++)
			{
				if (((uint)filter & (uint)(1 << i)) != 0)
				{
					Helpers.EventAdd(ref levelSubscriptions.DetailedRegs[i], del);
				}
			}
			levelSubscriptions.ActiveLevels |= levelSubscriptions.DetailLevels;
			levelSubscriptions.FixFilters();
			return levelSubscriptions;
		}

		public LevelSubscriptions RemoveDetailed(LogLevelFilter filter, OnLogMessageDetailed del)
		{
			LevelSubscriptions levelSubscriptions = Clone(changingDetail: true);
			for (int i = 0; i < levelSubscriptions.DetailedRegs.Length; i++)
			{
				if (((uint)filter & (uint)(1 << i)) != 0 && Helpers.EventRemove(ref levelSubscriptions.DetailedRegs[i], del) == null)
				{
					levelSubscriptions.DetailLevels &= (LogLevelFilter)(~(1 << i));
				}
			}
			levelSubscriptions.ActiveLevels |= levelSubscriptions.DetailLevels;
			levelSubscriptions.FixFilters();
			return levelSubscriptions;
		}
	}

	private sealed class LogSubscriptionSimple : IDisposable
	{
		private readonly DebugLog log;

		private readonly OnLogMessage del;

		private readonly LogLevelFilter filter;

		public LogSubscriptionSimple(DebugLog log, OnLogMessage del, LogLevelFilter filter)
		{
			this.log = log;
			this.del = del;
			this.filter = filter;
		}

		public void Dispose()
		{
			LevelSubscriptions subscriptions;
			LevelSubscriptions value;
			do
			{
				subscriptions = log.subscriptions;
				value = subscriptions.RemoveSimple(filter, del);
			}
			while (Interlocked.CompareExchange(ref log.subscriptions, value, subscriptions) != subscriptions);
		}
	}

	private sealed class LogSubscriptionDetailed : IDisposable
	{
		private readonly DebugLog log;

		private readonly OnLogMessageDetailed del;

		private readonly LogLevelFilter filter;

		public LogSubscriptionDetailed(DebugLog log, OnLogMessageDetailed del, LogLevelFilter filter)
		{
			this.log = log;
			this.del = del;
			this.filter = filter;
		}

		public void Dispose()
		{
			LevelSubscriptions subscriptions;
			LevelSubscriptions value;
			do
			{
				subscriptions = log.subscriptions;
				value = subscriptions.RemoveDetailed(filter, del);
			}
			while (Interlocked.CompareExchange(ref log.subscriptions, value, subscriptions) != subscriptions);
		}
	}

	internal static readonly DebugLog Instance = new DebugLog();

	private static readonly ConcurrentBag<WeakReference<LogMessage>> weakRefCache = new ConcurrentBag<WeakReference<LogMessage>>();

	private static readonly ConcurrentBag<WeakReference<LogMessage>> messageObjectCache = new ConcurrentBag<WeakReference<LogMessage>>();

	private static readonly char[] listEnvSeparator = new char[3] { ' ', ';', ',' };

	private readonly bool recordHoles;

	private readonly int replayQueueLength;

	private readonly ConcurrentQueue<LogMessage>? replayQueue;

	private LogLevelFilter globalFilter = LogLevelFilter.DefaultFilter;

	private static byte[]? memlog;

	private static int memlogPos;

	private LevelSubscriptions subscriptions = LevelSubscriptions.None;

	private static readonly ConcurrentDictionary<OnLogMessage, IDisposable> simpleRegDict = new ConcurrentDictionary<OnLogMessage, IDisposable>();

	public static bool IsFinalizing
	{
		get
		{
			if (!Environment.HasShutdownStarted)
			{
				return AppDomain.CurrentDomain.IsFinalizingForUnload();
			}
			return true;
		}
	}

	public static bool IsWritingLog => Instance.ShouldLog;

	internal bool AlwaysLog
	{
		get
		{
			if (replayQueue == null)
			{
				return Debugger.IsAttached;
			}
			return true;
		}
	}

	internal bool ShouldLog
	{
		get
		{
			if (subscriptions.ActiveLevels == LogLevelFilter.None)
			{
				return AlwaysLog;
			}
			return true;
		}
	}

	internal bool RecordHoles
	{
		get
		{
			if (!recordHoles)
			{
				return subscriptions.DetailLevels != LogLevelFilter.None;
			}
			return true;
		}
	}

	public static event OnLogMessage OnLog
	{
		add
		{
			IDisposable res = Subscribe(Instance.globalFilter, value);
			simpleRegDict.AddOrUpdate(value, res, delegate(OnLogMessage _, IDisposable d)
			{
				d.Dispose();
				return res;
			});
		}
		remove
		{
			if (simpleRegDict.TryRemove(value, out IDisposable value2))
			{
				value2.Dispose();
			}
		}
	}

	private LogMessage MakeMessage(string source, DateTime time, LogLevel level, string formatted, ReadOnlyMemory<MessageHole> holes)
	{
		try
		{
			if (replayQueue == null && !IsFinalizing)
			{
				WeakReference<LogMessage> result;
				while (messageObjectCache.TryTake(out result))
				{
					if (result.TryGetTarget(out var target))
					{
						target.Init(source, time, level, formatted, holes);
						weakRefCache.Add(result);
						return target;
					}
					weakRefCache.Add(result);
				}
			}
		}
		catch
		{
		}
		return new LogMessage(source, time, level, formatted, holes);
	}

	private void ReturnMessage(LogMessage message)
	{
		message.Clear();
		try
		{
			if (replayQueue == null && !IsFinalizing)
			{
				if (weakRefCache.TryTake(out WeakReference<LogMessage> result))
				{
					result.SetTarget(message);
					messageObjectCache.Add(result);
				}
				else
				{
					messageObjectCache.Add(new WeakReference<LogMessage>(message));
				}
			}
		}
		catch
		{
		}
	}

	private void PostMessage(LogMessage message)
	{
		if (Debugger.IsAttached)
		{
			try
			{
				LogLevel level = message.Level;
				string source = message.Source;
				FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(6, 3);
				handler.AppendLiteral("[");
				handler.AppendFormatted(message.Source);
				handler.AppendLiteral("] ");
				handler.AppendFormatted(message.Level.FastToString());
				handler.AppendLiteral(": ");
				handler.AppendFormatted(message.FormattedMessage);
				handler.AppendLiteral("\n");
				Debugger.Log((int)level, source, DebugFormatter.Format(ref handler));
			}
			catch
			{
			}
		}
		try
		{
			LevelSubscriptions levelSubscriptions = subscriptions;
			int level2 = (int)message.Level;
			OnLogMessage onLogMessage = levelSubscriptions.SimpleRegs[level2];
			if (onLogMessage != null)
			{
				message.ReportTo(onLogMessage);
			}
			OnLogMessageDetailed onLogMessageDetailed = levelSubscriptions.DetailedRegs[level2];
			if (onLogMessageDetailed != null)
			{
				message.ReportTo(onLogMessageDetailed);
			}
			if (IsFinalizing)
			{
				return;
			}
			ConcurrentQueue<LogMessage> concurrentQueue = replayQueue;
			if (concurrentQueue != null)
			{
				concurrentQueue.Enqueue(message);
				LogMessage result;
				while (concurrentQueue.Count > replayQueueLength && concurrentQueue.TryDequeue(out result))
				{
				}
			}
			else
			{
				ReturnMessage(message);
			}
		}
		catch
		{
		}
	}

	internal bool ShouldLogLevel(LogLevel level)
	{
		if (((uint)(1 << (int)level) & (uint)subscriptions.ActiveLevels) == 0)
		{
			if (((uint)(1 << (int)level) & (uint)globalFilter) != 0)
			{
				return AlwaysLog;
			}
			return false;
		}
		return true;
	}

	internal bool ShouldLevelRecordHoles(LogLevel level)
	{
		if (!recordHoles)
		{
			return ((uint)(1 << (int)level) & (uint)subscriptions.DetailLevels) != 0;
		}
		return true;
	}

	public void Write(string source, DateTime time, LogLevel level, string message)
	{
		if (ShouldLogLevel(level))
		{
			PostMessage(MakeMessage(source, time, level, message, default(ReadOnlyMemory<MessageHole>)));
		}
	}

	public void Write(string source, DateTime time, LogLevel level, [InterpolatedStringHandlerArgument("level")] ref DebugLogInterpolatedStringHandler message)
	{
		if (message.enabled && ShouldLogLevel(level))
		{
			ReadOnlyMemory<MessageHole> holes;
			string formatted = message.ToStringAndClear(out holes);
			PostMessage(MakeMessage(source, time, level, formatted, holes));
		}
	}

	internal void LogCore(string source, LogLevel level, string message)
	{
		if (ShouldLogLevel(level))
		{
			Write(source, DateTime.UtcNow, level, message);
		}
	}

	internal void LogCore(string source, LogLevel level, [InterpolatedStringHandlerArgument("level")] ref DebugLogInterpolatedStringHandler message)
	{
		if (message.enabled && ShouldLogLevel(level))
		{
			Write(source, DateTime.UtcNow, level, ref message);
		}
	}

	public static void Log(string source, LogLevel level, string message)
	{
		DebugLog instance = Instance;
		if (instance.ShouldLogLevel(level))
		{
			instance.Write(source, DateTime.UtcNow, level, message);
		}
	}

	public static void Log(string source, LogLevel level, [InterpolatedStringHandlerArgument("level")] ref DebugLogInterpolatedStringHandler message)
	{
		DebugLog instance = Instance;
		if (message.enabled && instance.ShouldLogLevel(level))
		{
			instance.Write(source, DateTime.UtcNow, level, ref message);
		}
	}

	private static string[]? GetListEnvVar(string text)
	{
		string text2 = text.Trim();
		if (string.IsNullOrEmpty(text2))
		{
			return null;
		}
		string[] array = text2.Split(listEnvSeparator, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		return array;
	}

	private DebugLog()
	{
		recordHoles = Switches.TryGetSwitchEnabled("LogRecordHoles", out var isEnabled) && isEnabled;
		replayQueueLength = 0;
		if (Switches.TryGetSwitchValue("LogReplayQueueLength", out object value))
		{
			replayQueueLength = (value as int?).GetValueOrDefault();
		}
		if (Switches.TryGetSwitchEnabled("LogSpam", out isEnabled) && isEnabled)
		{
			globalFilter |= LogLevelFilter.Spam;
		}
		if (replayQueueLength > 0)
		{
			replayQueue = new ConcurrentQueue<LogMessage>();
		}
		string text = (Switches.TryGetSwitchValue("LogToFile", out value) ? (value as string) : null);
		string[] sourceFilter = null;
		if (Switches.TryGetSwitchValue("LogToFileFilter", out value))
		{
			sourceFilter = ((value is string[] array) ? array : ((!(value is string text2)) ? null : GetListEnvVar(text2)));
		}
		if (text != null)
		{
			TryInitializeLogToFile(text, sourceFilter, globalFilter);
		}
		if (Switches.TryGetSwitchEnabled("LogInMemory", out isEnabled) && isEnabled)
		{
			TryInitializeMemoryLog(globalFilter);
		}
	}

	private void TryInitializeLogToFile(string file, string[]? sourceFilter, LogLevelFilter filter)
	{
		try
		{
			StringComparer comparer = StringComparerEx.FromComparison(StringComparison.OrdinalIgnoreCase);
			if (sourceFilter != null)
			{
				Array.Sort<string>(sourceFilter, (IComparer<string>)comparer);
			}
			object sync = new object();
			TextWriter writer;
			if (file == "-")
			{
				writer = Console.Out;
			}
			else
			{
				FileStream stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write);
				writer = new StreamWriter(stream, Encoding.UTF8)
				{
					AutoFlush = true
				};
			}
			SubscribeCore(filter, delegate(string source, DateTime time, LogLevel level, string msg)
			{
				if (sourceFilter != null && sourceFilter.AsSpan().BinarySearch(source, comparer) < 0)
				{
					return;
				}
				DateTime value = time.ToLocalTime();
				string value2 = $"[{source}]({value}) {level.FastToString()}: {msg}";
				lock (sync)
				{
					writer.WriteLine(value2);
				}
			});
		}
		catch (Exception value3)
		{
			LogLevel logLevel = LogLevel.Error;
			LogLevel level2 = logLevel;
			bool isEnabled;
			DebugLogInterpolatedStringHandler message = new DebugLogInterpolatedStringHandler(61, 1, logLevel, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Exception while trying to initialize writing logs to a file: ");
				message.AppendFormatted(value3);
			}
			Instance.LogCore("DebugLog", level2, ref message);
		}
	}

	private void TryInitializeMemoryLog(LogLevelFilter filter)
	{
		try
		{
			memlogPos = 0;
			memlog = new byte[4096];
			object sync = new object();
			_ = Encoding.UTF8;
			SubscribeCore(filter, delegate(string source, DateTime time, LogLevel level, string msg)
			{
				byte value = (byte)level;
				long ticks = time.Ticks;
				if (source.Length > 255)
				{
					source = source.Substring(0, 255);
				}
				byte b = (byte)source.Length;
				int length = msg.Length;
				int num = 14 + b * 2 + length * 2;
				lock (sync)
				{
					if (memlog.Length - memlogPos < num)
					{
						int num2 = memlog.Length * 4;
						while (num2 - memlogPos < num)
						{
							num2 *= 4;
						}
						Array.Resize(ref memlog, num2);
					}
					ref byte reference = ref MemoryMarshal.GetReference(memlog.AsSpan().Slice(memlogPos));
					int num3 = 0;
					Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference, num3), value);
					num3++;
					Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference, num3), ticks);
					num3 += 8;
					Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference, num3), b);
					num3++;
					Unsafe.CopyBlock(ref Unsafe.Add(ref reference, num3), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(source.AsSpan())), (uint)(b * 2));
					num3 += b * 2;
					Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference, num3), length);
					num3 += 4;
					Unsafe.CopyBlock(ref Unsafe.Add(ref reference, num3), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(msg.AsSpan())), (uint)(length * 2));
					num3 += length * 2;
					memlogPos += num3;
				}
			});
		}
		catch (Exception value2)
		{
			LogLevel logLevel = LogLevel.Error;
			LogLevel level2 = logLevel;
			bool isEnabled;
			DebugLogInterpolatedStringHandler message = new DebugLogInterpolatedStringHandler(45, 1, logLevel, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Exception while initializing the memory log: ");
				message.AppendFormatted(value2);
			}
			Instance.LogCore("DebugLog", level2, ref message);
		}
	}

	private void MaybeReplayTo(LogLevelFilter filter, OnLogMessage del)
	{
		if (replayQueue == null || filter == LogLevelFilter.None)
		{
			return;
		}
		LogMessage[] array = replayQueue.ToArray();
		foreach (LogMessage logMessage in array)
		{
			if (((uint)(1 << (int)logMessage.Level) & (uint)filter) != 0)
			{
				logMessage.ReportTo(del);
			}
		}
	}

	private void MaybeReplayTo(LogLevelFilter filter, OnLogMessageDetailed del)
	{
		if (replayQueue == null || filter == LogLevelFilter.None)
		{
			return;
		}
		LogMessage[] array = replayQueue.ToArray();
		foreach (LogMessage logMessage in array)
		{
			if (((uint)(1 << (int)logMessage.Level) & (uint)filter) != 0)
			{
				logMessage.ReportTo(del);
			}
		}
	}

	public static IDisposable Subscribe(LogLevelFilter filter, OnLogMessage value)
	{
		return Instance.SubscribeCore(filter, value);
	}

	private IDisposable SubscribeCore(LogLevelFilter filter, OnLogMessage value)
	{
		LevelSubscriptions levelSubscriptions;
		LevelSubscriptions value2;
		do
		{
			levelSubscriptions = subscriptions;
			value2 = levelSubscriptions.AddSimple(filter, value);
		}
		while (Interlocked.CompareExchange(ref subscriptions, value2, levelSubscriptions) != levelSubscriptions);
		MaybeReplayTo(filter, value);
		return new LogSubscriptionSimple(this, value, filter);
	}

	public static IDisposable Subscribe(LogLevelFilter filter, OnLogMessageDetailed value)
	{
		return Instance.SubscribeCore(filter, value);
	}

	private IDisposable SubscribeCore(LogLevelFilter filter, OnLogMessageDetailed value)
	{
		LevelSubscriptions levelSubscriptions;
		LevelSubscriptions value2;
		do
		{
			levelSubscriptions = subscriptions;
			value2 = levelSubscriptions.AddDetailed(filter, value);
		}
		while (Interlocked.CompareExchange(ref subscriptions, value2, levelSubscriptions) != levelSubscriptions);
		MaybeReplayTo(filter, value);
		return new LogSubscriptionDetailed(this, value, filter);
	}
}
