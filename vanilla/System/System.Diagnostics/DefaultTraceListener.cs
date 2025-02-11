using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Diagnostics;

/// <summary>Provides the default output methods and behavior for tracing.</summary>
/// <filterpriority>1</filterpriority>
public class DefaultTraceListener : TraceListener
{
	private enum DialogResult
	{
		None,
		Retry,
		Ignore,
		Abort
	}

	private static readonly bool OnWin32;

	private const string ConsoleOutTrace = "Console.Out";

	private const string ConsoleErrorTrace = "Console.Error";

	private static readonly string MonoTracePrefix;

	private static readonly string MonoTraceFile;

	private string logFileName;

	private bool assertUiEnabled;

	/// <summary>Gets or sets a value indicating whether the application is running in user-interface mode.</summary>
	/// <returns>true if user-interface mode is enabled; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO("AssertUiEnabled defaults to False; should follow Environment.UserInteractive.")]
	public bool AssertUiEnabled
	{
		get
		{
			return assertUiEnabled;
		}
		set
		{
			assertUiEnabled = value;
		}
	}

	/// <summary>Gets or sets the name of a log file to write trace or debug messages to.</summary>
	/// <returns>The name of a log file to write trace or debug messages to.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public string LogFileName
	{
		get
		{
			return logFileName;
		}
		set
		{
			logFileName = value;
		}
	}

	static DefaultTraceListener()
	{
		OnWin32 = Path.DirectorySeparatorChar == '\\';
		if (OnWin32)
		{
			return;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("MONO_TRACE_LISTENER");
		if (environmentVariable != null)
		{
			string text = null;
			string text2 = null;
			if (environmentVariable.StartsWith("Console.Out"))
			{
				text = "Console.Out";
				text2 = GetPrefix(environmentVariable, "Console.Out");
			}
			else if (environmentVariable.StartsWith("Console.Error"))
			{
				text = "Console.Error";
				text2 = GetPrefix(environmentVariable, "Console.Error");
			}
			else
			{
				text = environmentVariable;
				text2 = "";
			}
			MonoTraceFile = text;
			MonoTracePrefix = text2;
		}
	}

	private static string GetPrefix(string var, string target)
	{
		if (var.Length > target.Length)
		{
			return var.Substring(target.Length + 1);
		}
		return "";
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DefaultTraceListener" /> class with "Default" as its <see cref="P:System.Diagnostics.TraceListener.Name" /> property value.</summary>
	public DefaultTraceListener()
		: base("Default")
	{
	}

	/// <summary>Emits or displays a message and a stack trace for an assertion that always fails.</summary>
	/// <param name="message">The message to emit or display. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Fail(string message)
	{
		base.Fail(message);
	}

	/// <summary>Emits or displays detailed messages and a stack trace for an assertion that always fails.</summary>
	/// <param name="message">The message to emit or display. </param>
	/// <param name="detailMessage">The detailed message to emit or display. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Fail(string message, string detailMessage)
	{
		base.Fail(message, detailMessage);
		if (ProcessUI(message, detailMessage) == DialogResult.Abort)
		{
			Thread.CurrentThread.Abort();
		}
		WriteLine(new StackTrace().ToString());
	}

	private DialogResult ProcessUI(string message, string detailMessage)
	{
		if (!AssertUiEnabled)
		{
			return DialogResult.None;
		}
		object obj;
		MethodInfo method;
		try
		{
			Assembly assembly = Assembly.Load("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			if (assembly == null)
			{
				return DialogResult.None;
			}
			Type type = assembly.GetType("System.Windows.Forms.MessageBoxButtons");
			obj = Enum.Parse(type, "AbortRetryIgnore");
			method = assembly.GetType("System.Windows.Forms.MessageBox").GetMethod("Show", new Type[3]
			{
				typeof(string),
				typeof(string),
				type
			});
		}
		catch
		{
			return DialogResult.None;
		}
		if (method == null || obj == null)
		{
			return DialogResult.None;
		}
		string text = string.Format("Assertion Failed: {0} to quit, {1} to debug, {2} to continue", "Abort", "Retry", "Ignore");
		string text2 = string.Format("{0}{1}{2}{1}{1}{3}", message, Environment.NewLine, detailMessage, new StackTrace());
		string text3 = method.Invoke(null, new object[3] { text2, text, obj }).ToString();
		if (!(text3 == "Ignore"))
		{
			if (text3 == "Abort")
			{
				return DialogResult.Abort;
			}
			return DialogResult.Retry;
		}
		return DialogResult.Ignore;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void WriteWindowsDebugString(string message);

	private void WriteDebugString(string message)
	{
		if (OnWin32)
		{
			WriteWindowsDebugString(message);
		}
		else
		{
			WriteMonoTrace(message);
		}
	}

	private void WriteMonoTrace(string message)
	{
		string monoTraceFile = MonoTraceFile;
		if (!(monoTraceFile == "Console.Out"))
		{
			if (monoTraceFile == "Console.Error")
			{
				Console.Error.Write(message);
			}
			else
			{
				WriteLogFile(message, MonoTraceFile);
			}
		}
		else
		{
			Console.Out.Write(message);
		}
	}

	private void WritePrefix()
	{
		if (!OnWin32)
		{
			WriteMonoTrace(MonoTracePrefix);
		}
	}

	private void WriteImpl(string message)
	{
		if (base.NeedIndent)
		{
			WriteIndent();
			WritePrefix();
		}
		if (Debugger.IsLogging())
		{
			Debugger.Log(0, null, message);
		}
		else
		{
			WriteDebugString(message);
		}
		WriteLogFile(message, LogFileName);
	}

	private void WriteLogFile(string message, string logFile)
	{
		if (logFile != null && logFile.Length != 0)
		{
			FileInfo fileInfo = new FileInfo(logFile);
			StreamWriter streamWriter = null;
			try
			{
				streamWriter = ((!fileInfo.Exists) ? fileInfo.CreateText() : fileInfo.AppendText());
			}
			catch
			{
				return;
			}
			using (streamWriter)
			{
				streamWriter.Write(message);
				streamWriter.Flush();
			}
		}
	}

	/// <summary>Writes the output to the OutputDebugString function and to the <see cref="M:System.Diagnostics.Debugger.Log(System.Int32,System.String,System.String)" /> method.</summary>
	/// <param name="message">The message to write to OutputDebugString and <see cref="M:System.Diagnostics.Debugger.Log(System.Int32,System.String,System.String)" />. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public override void Write(string message)
	{
		WriteImpl(message);
	}

	/// <summary>Writes the output to the OutputDebugString function and to the <see cref="M:System.Diagnostics.Debugger.Log(System.Int32,System.String,System.String)" /> method, followed by a carriage return and line feed (\r\n).</summary>
	/// <param name="message">The message to write to OutputDebugString and <see cref="M:System.Diagnostics.Debugger.Log(System.Int32,System.String,System.String)" />. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public override void WriteLine(string message)
	{
		string message2 = message + Environment.NewLine;
		WriteImpl(message2);
		base.NeedIndent = true;
	}
}
