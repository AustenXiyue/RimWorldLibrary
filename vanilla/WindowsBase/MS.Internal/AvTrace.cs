#define TRACE
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using MS.Internal.WindowsBase;
using MS.Win32;

namespace MS.Internal;

internal class AvTrace
{
	private bool _isEnabled;

	private bool _enabledByDebugger;

	private bool _suppressGeneratedParameters;

	private static bool _hasBeenRefreshed = false;

	private GetTraceSourceDelegate _getTraceSourceDelegate;

	private ClearTraceSourceDelegate _clearTraceSourceDelegate;

	private TraceSource _traceSource;

	private static bool? _enabledInRegistry = null;

	private static char[] FormatChars = new char[2] { '{', '}' };

	public bool IsEnabled => _isEnabled;

	public bool SuppressGeneratedParameters
	{
		get
		{
			return _suppressGeneratedParameters;
		}
		set
		{
			_suppressGeneratedParameters = value;
		}
	}

	public bool IsEnabledOverride => _traceSource != null;

	public bool EnabledByDebugger
	{
		get
		{
			return _enabledByDebugger;
		}
		set
		{
			_enabledByDebugger = value;
			if (_enabledByDebugger)
			{
				if (!IsEnabled && IsDebuggerAttached())
				{
					_isEnabled = true;
				}
			}
			else if (IsEnabled && !IsWpfTracingEnabledInRegistry() && !_hasBeenRefreshed)
			{
				_isEnabled = false;
			}
		}
	}

	public event AvTraceEventHandler TraceExtraMessages;

	public AvTrace(GetTraceSourceDelegate getTraceSourceDelegate, ClearTraceSourceDelegate clearTraceSourceDelegate)
	{
		_getTraceSourceDelegate = getTraceSourceDelegate;
		_clearTraceSourceDelegate = clearTraceSourceDelegate;
		PresentationTraceSources.TraceRefresh += Refresh;
		Initialize();
	}

	public void Refresh()
	{
		_enabledInRegistry = null;
		Initialize();
	}

	public static void OnRefresh()
	{
		_hasBeenRefreshed = true;
	}

	private void Initialize()
	{
		if (ShouldCreateTraceSources())
		{
			_traceSource = _getTraceSourceDelegate();
			_isEnabled = IsWpfTracingEnabledInRegistry() || _hasBeenRefreshed || _enabledByDebugger;
		}
		else
		{
			_clearTraceSourceDelegate();
			_traceSource = null;
			_isEnabled = false;
		}
	}

	private static bool ShouldCreateTraceSources()
	{
		if (IsWpfTracingEnabledInRegistry() || IsDebuggerAttached() || _hasBeenRefreshed)
		{
			return true;
		}
		return false;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool IsWpfTracingEnabledInRegistry()
	{
		if (!_enabledInRegistry.HasValue)
		{
			bool value = false;
			object obj = SecurityHelper.ReadRegistryValue(Registry.CurrentUser, "Software\\Microsoft\\Tracing\\WPF", "ManagedTracing");
			if (obj is int && (int)obj == 1)
			{
				value = true;
			}
			_enabledInRegistry = value;
		}
		return _enabledInRegistry.Value;
	}

	internal static bool IsDebuggerAttached()
	{
		if (!Debugger.IsAttached)
		{
			return SafeNativeMethods.IsDebuggerPresent();
		}
		return true;
	}

	public string Trace(TraceEventType type, int eventId, string message, string[] labels, object[] parameters)
	{
		if (_traceSource == null || !_traceSource.Switch.ShouldTrace(type))
		{
			return null;
		}
		AvTraceBuilder avTraceBuilder = new AvTraceBuilder(AntiFormat(message));
		ArrayList arrayList = new ArrayList();
		int num = 0;
		if (parameters != null && labels != null && labels.Length != 0)
		{
			int num2 = 1;
			int num3 = 0;
			while (num2 < labels.Length && num3 < parameters.Length)
			{
				avTraceBuilder.Append("; {" + num++ + "}='{" + num++ + "}'");
				if (parameters[num3] == null)
				{
					parameters[num3] = "<null>";
				}
				else if (!SuppressGeneratedParameters && parameters[num3].GetType() != typeof(string) && !(parameters[num3] is ValueType) && !(parameters[num3] is Type) && !(parameters[num3] is DependencyProperty))
				{
					avTraceBuilder.Append("; " + labels[num2].ToString() + ".HashCode='" + GetHashCodeHelper(parameters[num3]) + "'");
					avTraceBuilder.Append("; " + labels[num2].ToString() + ".Type='" + GetTypeHelper(parameters[num3]).ToString() + "'");
				}
				arrayList.Add(labels[num2]);
				arrayList.Add(parameters[num3]);
				num2++;
				num3++;
			}
			if (this.TraceExtraMessages != null && num3 < parameters.Length)
			{
				this.TraceExtraMessages(avTraceBuilder, parameters, num3);
			}
		}
		string text = avTraceBuilder.ToString();
		_traceSource.TraceEvent(type, eventId, text, arrayList.ToArray());
		if (IsDebuggerAttached())
		{
			_traceSource.Flush();
		}
		return text;
	}

	public void TraceStartStop(int eventID, string message, string[] labels, object[] parameters)
	{
		Trace(TraceEventType.Start, eventID, message, labels, parameters);
		_traceSource.TraceEvent(TraceEventType.Stop, eventID);
	}

	public static string ToStringHelper(object value)
	{
		if (value == null)
		{
			return "<null>";
		}
		string s;
		try
		{
			s = value.ToString();
		}
		catch
		{
			s = "<unprintable>";
		}
		return AntiFormat(s);
	}

	public static string AntiFormat(string s)
	{
		int num = s.IndexOfAny(FormatChars);
		if (num < 0)
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num2 = 0;
		int num3 = s.Length - 1;
		while (num >= 0)
		{
			if (num < num3 && s[num] == s[num + 1])
			{
				num = s.IndexOfAny(FormatChars, num + 2);
				continue;
			}
			stringBuilder.Append(s.Substring(num2, num - num2 + 1));
			stringBuilder.Append(s[num]);
			num2 = num + 1;
			num = s.IndexOfAny(FormatChars, num2);
		}
		if (num2 <= num3)
		{
			stringBuilder.Append(s.Substring(num2));
		}
		return stringBuilder.ToString();
	}

	public static string TypeName(object value)
	{
		if (value == null)
		{
			return "<null>";
		}
		return value.GetType().Name;
	}

	public static int GetHashCodeHelper(object value)
	{
		try
		{
			return value?.GetHashCode() ?? 0;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex))
			{
				throw;
			}
			return 0;
		}
	}

	public static Type GetTypeHelper(object value)
	{
		if (value == null)
		{
			return typeof(ValueType);
		}
		return value.GetType();
	}
}
