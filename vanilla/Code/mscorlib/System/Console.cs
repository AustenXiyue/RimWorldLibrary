using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace System;

/// <summary>Represents the standard input, output, and error streams for console applications. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public static class Console
{
	private class WindowsConsole
	{
		private delegate bool WindowsCancelHandler(int keyCode);

		public static bool ctrlHandlerAdded = false;

		private static WindowsCancelHandler cancelHandler = DoWindowsConsoleCancelEvent;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern int GetConsoleCP();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern int GetConsoleOutputCP();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern bool SetConsoleCtrlHandler(WindowsCancelHandler handler, bool addHandler);

		private static bool DoWindowsConsoleCancelEvent(int keyCode)
		{
			if (keyCode == 0)
			{
				DoConsoleCancelEvent();
			}
			return keyCode == 0;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int GetInputCodePage()
		{
			return GetConsoleCP();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int GetOutputCodePage()
		{
			return GetConsoleOutputCP();
		}

		public static void AddCtrlHandler()
		{
			SetConsoleCtrlHandler(cancelHandler, addHandler: true);
			ctrlHandlerAdded = true;
		}

		public static void RemoveCtrlHandler()
		{
			SetConsoleCtrlHandler(cancelHandler, addHandler: false);
			ctrlHandlerAdded = false;
		}
	}

	private delegate void InternalCancelHandler();

	internal static TextWriter stdout;

	private static TextWriter stderr;

	private static TextReader stdin;

	private static Encoding inputEncoding;

	private static Encoding outputEncoding;

	private static ConsoleCancelEventHandler cancel_event;

	private static readonly InternalCancelHandler cancel_handler;

	/// <summary>Gets the standard error output stream.</summary>
	/// <returns>A <see cref="T:System.IO.TextWriter" /> that represents the standard error output stream.</returns>
	/// <filterpriority>1</filterpriority>
	public static TextWriter Error => stderr;

	/// <summary>Gets the standard output stream.</summary>
	/// <returns>A <see cref="T:System.IO.TextWriter" /> that represents the standard output stream.</returns>
	/// <filterpriority>1</filterpriority>
	public static TextWriter Out => stdout;

	/// <summary>Gets the standard input stream.</summary>
	/// <returns>A <see cref="T:System.IO.TextReader" /> that represents the standard input stream.</returns>
	/// <filterpriority>1</filterpriority>
	public static TextReader In => stdin;

	/// <summary>Gets or sets the encoding the console uses to read input. </summary>
	/// <returns>The encoding used to read console input.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property value in a set operation is null.</exception>
	/// <exception cref="T:System.IO.IOException">An error occurred during the execution of this operation.</exception>
	/// <exception cref="T:System.Security.SecurityException">Your application does not have permission to perform this operation.</exception>
	/// <filterpriority>1</filterpriority>
	public static Encoding InputEncoding
	{
		get
		{
			return inputEncoding;
		}
		set
		{
			inputEncoding = value;
			SetupStreams(inputEncoding, outputEncoding);
		}
	}

	/// <summary>Gets or sets the encoding the console uses to write output. </summary>
	/// <returns>The encoding used to write console output.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property value in a set operation is null.</exception>
	/// <exception cref="T:System.IO.IOException">An error occurred during the execution of this operation.</exception>
	/// <exception cref="T:System.Security.SecurityException">Your application does not have permission to perform this operation.</exception>
	/// <filterpriority>1</filterpriority>
	public static Encoding OutputEncoding
	{
		get
		{
			return outputEncoding;
		}
		set
		{
			outputEncoding = value;
			SetupStreams(inputEncoding, outputEncoding);
		}
	}

	/// <summary>Gets or sets the background color of the console.</summary>
	/// <returns>A value that specifies the background color of the console; that is, the color that appears behind each character. The default is black.</returns>
	/// <exception cref="T:System.ArgumentException">The color specified in a set operation is not a valid member of <see cref="T:System.ConsoleColor" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static ConsoleColor BackgroundColor
	{
		get
		{
			return ConsoleDriver.BackgroundColor;
		}
		set
		{
			ConsoleDriver.BackgroundColor = value;
		}
	}

	/// <summary>Gets or sets the height of the buffer area.</summary>
	/// <returns>The current height, in rows, of the buffer area.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value in a set operation is less than or equal to zero.-or- The value in a set operation is greater than or equal to <see cref="F:System.Int16.MaxValue" />.-or- The value in a set operation is less than <see cref="P:System.Console.WindowTop" /> + <see cref="P:System.Console.WindowHeight" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static int BufferHeight
	{
		get
		{
			return ConsoleDriver.BufferHeight;
		}
		[MonoLimitation("Implemented only on Windows")]
		set
		{
			ConsoleDriver.BufferHeight = value;
		}
	}

	/// <summary>Gets or sets the width of the buffer area.</summary>
	/// <returns>The current width, in columns, of the buffer area.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value in a set operation is less than or equal to zero.-or- The value in a set operation is greater than or equal to <see cref="F:System.Int16.MaxValue" />.-or- The value in a set operation is less than <see cref="P:System.Console.WindowLeft" /> + <see cref="P:System.Console.WindowWidth" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static int BufferWidth
	{
		get
		{
			return ConsoleDriver.BufferWidth;
		}
		[MonoLimitation("Implemented only on Windows")]
		set
		{
			ConsoleDriver.BufferWidth = value;
		}
	}

	/// <summary>Gets a value indicating whether the CAPS LOCK keyboard toggle is turned on or turned off.</summary>
	/// <returns>true if CAPS LOCK is turned on; false if CAPS LOCK is turned off.</returns>
	/// <filterpriority>1</filterpriority>
	[MonoLimitation("Implemented only on Windows")]
	public static bool CapsLock => ConsoleDriver.CapsLock;

	/// <summary>Gets or sets the column position of the cursor within the buffer area.</summary>
	/// <returns>The current position, in columns, of the cursor.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value in a set operation is less than zero.-or- The value in a set operation is greater than or equal to <see cref="P:System.Console.BufferWidth" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static int CursorLeft
	{
		get
		{
			return ConsoleDriver.CursorLeft;
		}
		set
		{
			ConsoleDriver.CursorLeft = value;
		}
	}

	/// <summary>Gets or sets the row position of the cursor within the buffer area.</summary>
	/// <returns>The current position, in rows, of the cursor.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value in a set operation is less than zero.-or- The value in a set operation is greater than or equal to <see cref="P:System.Console.BufferHeight" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	public static int CursorTop
	{
		get
		{
			return ConsoleDriver.CursorTop;
		}
		set
		{
			ConsoleDriver.CursorTop = value;
		}
	}

	/// <summary>Gets or sets the height of the cursor within a character cell.</summary>
	/// <returns>The size of the cursor expressed as a percentage of the height of a character cell. The property value ranges from 1 to 100.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified in a set operation is less than 1 or greater than 100. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static int CursorSize
	{
		get
		{
			return ConsoleDriver.CursorSize;
		}
		set
		{
			ConsoleDriver.CursorSize = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the cursor is visible.</summary>
	/// <returns>true if the cursor is visible; otherwise, false.</returns>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static bool CursorVisible
	{
		get
		{
			return ConsoleDriver.CursorVisible;
		}
		set
		{
			ConsoleDriver.CursorVisible = value;
		}
	}

	/// <summary>Gets or sets the foreground color of the console.</summary>
	/// <returns>A <see cref="T:System.ConsoleColor" /> that specifies the foreground color of the console; that is, the color of each character that is displayed. The default is gray.</returns>
	/// <exception cref="T:System.ArgumentException">The color specified in a set operation is not a valid member of <see cref="T:System.ConsoleColor" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static ConsoleColor ForegroundColor
	{
		get
		{
			return ConsoleDriver.ForegroundColor;
		}
		set
		{
			ConsoleDriver.ForegroundColor = value;
		}
	}

	/// <summary>Gets a value indicating whether a key press is available in the input stream.</summary>
	/// <returns>true if a key press is available; otherwise, false.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.InvalidOperationException">Standard input is redirected to a file instead of the keyboard. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool KeyAvailable => ConsoleDriver.KeyAvailable;

	/// <summary>Gets the largest possible number of console window rows, based on the current font and screen resolution.</summary>
	/// <returns>The height of the largest possible console window measured in rows.</returns>
	/// <filterpriority>1</filterpriority>
	public static int LargestWindowHeight => ConsoleDriver.LargestWindowHeight;

	/// <summary>Gets the largest possible number of console window columns, based on the current font and screen resolution.</summary>
	/// <returns>The width of the largest possible console window measured in columns.</returns>
	/// <filterpriority>1</filterpriority>
	public static int LargestWindowWidth => ConsoleDriver.LargestWindowWidth;

	/// <summary>Gets a value indicating whether the NUM LOCK keyboard toggle is turned on or turned off.</summary>
	/// <returns>true if NUM LOCK is turned on; false if NUM LOCK is turned off.</returns>
	/// <filterpriority>1</filterpriority>
	[MonoLimitation("Only works on windows")]
	public static bool NumberLock => ConsoleDriver.NumberLock;

	/// <summary>Gets or sets the title to display in the console title bar.</summary>
	/// <returns>The string to be displayed in the title bar of the console. The maximum length of the title string is 24500 characters.</returns>
	/// <exception cref="T:System.InvalidOperationException">In a get operation, the retrieved title is longer than 24500 characters. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">In a set operation, the specified title is longer than 24500 characters. </exception>
	/// <exception cref="T:System.ArgumentNullException">In a set operation, the specified title is null. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static string Title
	{
		get
		{
			return ConsoleDriver.Title;
		}
		set
		{
			ConsoleDriver.Title = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the combination of the <see cref="F:System.ConsoleModifiers.Control" /> modifier key and <see cref="F:System.ConsoleKey.C" /> console key (Ctrl+C) is treated as ordinary input or as an interruption that is handled by the operating system.</summary>
	/// <returns>true if Ctrl+C is treated as ordinary input; otherwise, false.</returns>
	/// <exception cref="T:System.IO.IOException">Unable to get or set the input mode of the console input buffer. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static bool TreatControlCAsInput
	{
		get
		{
			return ConsoleDriver.TreatControlCAsInput;
		}
		set
		{
			ConsoleDriver.TreatControlCAsInput = value;
		}
	}

	/// <summary>Gets or sets the height of the console window area.</summary>
	/// <returns>The height of the console window measured in rows.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <see cref="P:System.Console.WindowWidth" /> property or the value of the <see cref="P:System.Console.WindowHeight" /> property is less than or equal to 0.-or-The value of the <see cref="P:System.Console.WindowHeight" /> property plus the value of the <see cref="P:System.Console.WindowTop" /> property is greater than or equal to <see cref="F:System.Int16.MaxValue" />.-or-The value of the <see cref="P:System.Console.WindowWidth" /> property or the value of the <see cref="P:System.Console.WindowHeight" /> property is greater than the largest possible window width or height for the current screen resolution and console font.</exception>
	/// <exception cref="T:System.IO.IOException">Error reading or writing information.</exception>
	/// <filterpriority>1</filterpriority>
	[MonoLimitation("Only works on windows")]
	public static int WindowHeight
	{
		get
		{
			return ConsoleDriver.WindowHeight;
		}
		set
		{
			ConsoleDriver.WindowHeight = value;
		}
	}

	/// <summary>Gets or sets the leftmost position of the console window area relative to the screen buffer.</summary>
	/// <returns>The leftmost console window position measured in columns.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">In a set operation, the value to be assigned is less than zero.-or-As a result of the assignment, <see cref="P:System.Console.WindowLeft" /> plus <see cref="P:System.Console.WindowWidth" /> would exceed <see cref="P:System.Console.BufferWidth" />. </exception>
	/// <exception cref="T:System.IO.IOException">Error reading or writing information.</exception>
	/// <filterpriority>1</filterpriority>
	[MonoLimitation("Only works on windows")]
	public static int WindowLeft
	{
		get
		{
			return ConsoleDriver.WindowLeft;
		}
		set
		{
			ConsoleDriver.WindowLeft = value;
		}
	}

	/// <summary>Gets or sets the top position of the console window area relative to the screen buffer.</summary>
	/// <returns>The uppermost console window position measured in rows.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">In a set operation, the value to be assigned is less than zero.-or-As a result of the assignment, <see cref="P:System.Console.WindowTop" /> plus <see cref="P:System.Console.WindowHeight" /> would exceed <see cref="P:System.Console.BufferHeight" />.</exception>
	/// <exception cref="T:System.IO.IOException">Error reading or writing information.</exception>
	/// <filterpriority>1</filterpriority>
	[MonoLimitation("Only works on windows")]
	public static int WindowTop
	{
		get
		{
			return ConsoleDriver.WindowTop;
		}
		set
		{
			ConsoleDriver.WindowTop = value;
		}
	}

	/// <summary>Gets or sets the width of the console window.</summary>
	/// <returns>The width of the console window measured in columns.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <see cref="P:System.Console.WindowWidth" /> property or the value of the <see cref="P:System.Console.WindowHeight" /> property is less than or equal to 0.-or-The value of the <see cref="P:System.Console.WindowHeight" /> property plus the value of the <see cref="P:System.Console.WindowTop" /> property is greater than or equal to <see cref="F:System.Int16.MaxValue" />.-or-The value of the <see cref="P:System.Console.WindowWidth" /> property or the value of the <see cref="P:System.Console.WindowHeight" /> property is greater than the largest possible window width or height for the current screen resolution and console font.</exception>
	/// <exception cref="T:System.IO.IOException">Error reading or writing information.</exception>
	/// <filterpriority>1</filterpriority>
	[MonoLimitation("Only works on windows")]
	public static int WindowWidth
	{
		get
		{
			return ConsoleDriver.WindowWidth;
		}
		set
		{
			ConsoleDriver.WindowWidth = value;
		}
	}

	/// <summary>Gets a value that indicates whether the error output stream has been redirected from the standard error stream.</summary>
	/// <returns>true if error output is redirected; otherwise, false.</returns>
	public static bool IsErrorRedirected => ConsoleDriver.IsErrorRedirected;

	/// <summary>Gets a value that indicates whether output has been redirected from the standard output stream.</summary>
	/// <returns>true if output is redirected; otherwise, false.</returns>
	public static bool IsOutputRedirected => ConsoleDriver.IsOutputRedirected;

	/// <summary>Gets a value that indicates whether input has been redirected from the standard input stream.</summary>
	/// <returns>true if input is redirected; otherwise, false.</returns>
	public static bool IsInputRedirected => ConsoleDriver.IsInputRedirected;

	/// <summary>Occurs when the <see cref="F:System.ConsoleModifiers.Control" /> modifier key (Ctrl) and either the <see cref="F:System.ConsoleKey.C" /> console key (C) or the Break key are pressed simultaneously (Ctrl+C or Ctrl+Break).</summary>
	/// <filterpriority>1</filterpriority>
	public static event ConsoleCancelEventHandler CancelKeyPress
	{
		add
		{
			if (!ConsoleDriver.Initialized)
			{
				ConsoleDriver.Init();
			}
			cancel_event = (ConsoleCancelEventHandler)Delegate.Combine(cancel_event, value);
			if (Environment.IsRunningOnWindows && !WindowsConsole.ctrlHandlerAdded)
			{
				WindowsConsole.AddCtrlHandler();
			}
		}
		remove
		{
			if (!ConsoleDriver.Initialized)
			{
				ConsoleDriver.Init();
			}
			cancel_event = (ConsoleCancelEventHandler)Delegate.Remove(cancel_event, value);
			if (cancel_event == null && Environment.IsRunningOnWindows && WindowsConsole.ctrlHandlerAdded)
			{
				WindowsConsole.RemoveCtrlHandler();
			}
		}
	}

	static Console()
	{
		cancel_handler = DoConsoleCancelEvent;
		if (Environment.IsRunningOnWindows)
		{
			try
			{
				inputEncoding = Encoding.GetEncoding(WindowsConsole.GetInputCodePage());
				outputEncoding = Encoding.GetEncoding(WindowsConsole.GetOutputCodePage());
			}
			catch
			{
				inputEncoding = (outputEncoding = Encoding.Default);
			}
		}
		else
		{
			int code_page = 0;
			EncodingHelper.InternalCodePage(ref code_page);
			if (code_page != -1 && ((code_page & 0xFFFFFFF) == 3 || (code_page & 0x10000000) != 0))
			{
				inputEncoding = (outputEncoding = EncodingHelper.UTF8Unmarked);
			}
			else
			{
				inputEncoding = (outputEncoding = Encoding.Default);
			}
		}
		SetupStreams(inputEncoding, outputEncoding);
	}

	private static void SetupStreams(Encoding inputEncoding, Encoding outputEncoding)
	{
		if (!Environment.IsRunningOnWindows && ConsoleDriver.IsConsole)
		{
			stdin = new CStreamReader(OpenStandardInput(0), inputEncoding);
			stdout = TextWriter.Synchronized(new CStreamWriter(OpenStandardOutput(0), outputEncoding, leaveOpen: true)
			{
				AutoFlush = true
			});
			stderr = TextWriter.Synchronized(new CStreamWriter(OpenStandardError(0), outputEncoding, leaveOpen: true)
			{
				AutoFlush = true
			});
		}
		else
		{
			stdin = TextReader.Synchronized(new UnexceptionalStreamReader(OpenStandardInput(0), inputEncoding));
			stdout = TextWriter.Synchronized(new UnexceptionalStreamWriter(OpenStandardOutput(0), outputEncoding)
			{
				AutoFlush = true
			});
			stderr = TextWriter.Synchronized(new UnexceptionalStreamWriter(OpenStandardError(0), outputEncoding)
			{
				AutoFlush = true
			});
		}
		GC.SuppressFinalize(stdout);
		GC.SuppressFinalize(stderr);
		GC.SuppressFinalize(stdin);
	}

	private static Stream Open(IntPtr handle, FileAccess access, int bufferSize)
	{
		try
		{
			return new FileStream(handle, access, ownsHandle: false, bufferSize, isAsync: false, isConsoleWrapper: true);
		}
		catch (IOException)
		{
			return Stream.Null;
		}
	}

	/// <summary>Acquires the standard error stream.</summary>
	/// <returns>The standard error stream.</returns>
	/// <filterpriority>1</filterpriority>
	public static Stream OpenStandardError()
	{
		return OpenStandardError(0);
	}

	/// <summary>Acquires the standard error stream, which is set to a specified buffer size.</summary>
	/// <returns>The standard error stream.</returns>
	/// <param name="bufferSize">The internal stream buffer size. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is less than or equal to zero. </exception>
	/// <filterpriority>1</filterpriority>
	[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
	public static Stream OpenStandardError(int bufferSize)
	{
		return Open(MonoIO.ConsoleError, FileAccess.Write, bufferSize);
	}

	/// <summary>Acquires the standard input stream.</summary>
	/// <returns>The standard input stream.</returns>
	/// <filterpriority>1</filterpriority>
	public static Stream OpenStandardInput()
	{
		return OpenStandardInput(0);
	}

	/// <summary>Acquires the standard input stream, which is set to a specified buffer size.</summary>
	/// <returns>The standard input stream.</returns>
	/// <param name="bufferSize">The internal stream buffer size. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is less than or equal to zero. </exception>
	/// <filterpriority>1</filterpriority>
	[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
	public static Stream OpenStandardInput(int bufferSize)
	{
		return Open(MonoIO.ConsoleInput, FileAccess.Read, bufferSize);
	}

	/// <summary>Acquires the standard output stream.</summary>
	/// <returns>The standard output stream.</returns>
	/// <filterpriority>1</filterpriority>
	public static Stream OpenStandardOutput()
	{
		return OpenStandardOutput(0);
	}

	/// <summary>Acquires the standard output stream, which is set to a specified buffer size.</summary>
	/// <returns>The standard output stream.</returns>
	/// <param name="bufferSize">The internal stream buffer size. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is less than or equal to zero. </exception>
	/// <filterpriority>1</filterpriority>
	[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
	public static Stream OpenStandardOutput(int bufferSize)
	{
		return Open(MonoIO.ConsoleOutput, FileAccess.Write, bufferSize);
	}

	/// <summary>Sets the <see cref="P:System.Console.Error" /> property to the specified <see cref="T:System.IO.TextWriter" /> object.</summary>
	/// <param name="newError">A stream that is the new standard error output. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="newError" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public static void SetError(TextWriter newError)
	{
		if (newError == null)
		{
			throw new ArgumentNullException("newError");
		}
		stderr = newError;
	}

	/// <summary>Sets the <see cref="P:System.Console.In" /> property to the specified <see cref="T:System.IO.TextReader" /> object.</summary>
	/// <param name="newIn">A stream that is the new standard input. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="newIn" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public static void SetIn(TextReader newIn)
	{
		if (newIn == null)
		{
			throw new ArgumentNullException("newIn");
		}
		stdin = newIn;
	}

	/// <summary>Sets the <see cref="P:System.Console.Out" /> property to the specified <see cref="T:System.IO.TextWriter" /> object.</summary>
	/// <param name="newOut">A stream that is the new standard output. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="newOut" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public static void SetOut(TextWriter newOut)
	{
		if (newOut == null)
		{
			throw new ArgumentNullException("newOut");
		}
		stdout = newOut;
	}

	/// <summary>Writes the text representation of the specified Boolean value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(bool value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the specified Unicode character value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(char value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the specified array of Unicode characters to the standard output stream.</summary>
	/// <param name="buffer">A Unicode character array. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(char[] buffer)
	{
		stdout.Write(buffer);
	}

	/// <summary>Writes the text representation of the specified <see cref="T:System.Decimal" /> value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(decimal value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the text representation of the specified double-precision floating-point value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(double value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the text representation of the specified 32-bit signed integer value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(int value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the text representation of the specified 64-bit signed integer value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(long value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the text representation of the specified object to the standard output stream.</summary>
	/// <param name="value">The value to write, or null. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(object value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the text representation of the specified single-precision floating-point value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(float value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the specified string value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(string value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the text representation of the specified 32-bit unsigned integer value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static void Write(uint value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the text representation of the specified 64-bit unsigned integer value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static void Write(ulong value)
	{
		stdout.Write(value);
	}

	/// <summary>Writes the text representation of the specified object to the standard output stream using the specified format information.</summary>
	/// <param name="format">A composite format string (see Remarks). </param>
	/// <param name="arg0">An object to write using <paramref name="format" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format specification in <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(string format, object arg0)
	{
		stdout.Write(format, arg0);
	}

	/// <summary>Writes the text representation of the specified array of objects to the standard output stream using the specified format information.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg">An array of objects to write using <paramref name="format" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> or <paramref name="arg" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format specification in <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(string format, params object[] arg)
	{
		if (arg == null)
		{
			stdout.Write(format);
		}
		else
		{
			stdout.Write(format, arg);
		}
	}

	/// <summary>Writes the specified subarray of Unicode characters to the standard output stream.</summary>
	/// <param name="buffer">An array of Unicode characters. </param>
	/// <param name="index">The starting position in <paramref name="buffer" />. </param>
	/// <param name="count">The number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> plus <paramref name="count" /> specify a position that is not within <paramref name="buffer" />. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(char[] buffer, int index, int count)
	{
		stdout.Write(buffer, index, count);
	}

	/// <summary>Writes the text representation of the specified objects to the standard output stream using the specified format information.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg0">The first object to write using <paramref name="format" />. </param>
	/// <param name="arg1">The second object to write using <paramref name="format" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format specification in <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(string format, object arg0, object arg1)
	{
		stdout.Write(format, arg0, arg1);
	}

	/// <summary>Writes the text representation of the specified objects to the standard output stream using the specified format information.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg0">The first object to write using <paramref name="format" />. </param>
	/// <param name="arg1">The second object to write using <paramref name="format" />. </param>
	/// <param name="arg2">The third object to write using <paramref name="format" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format specification in <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Write(string format, object arg0, object arg1, object arg2)
	{
		stdout.Write(format, arg0, arg1, arg2);
	}

	[CLSCompliant(false)]
	public static void Write(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
	{
		ArgIterator argIterator = new ArgIterator(__arglist);
		int remainingCount = argIterator.GetRemainingCount();
		object[] array = new object[remainingCount + 4];
		array[0] = arg0;
		array[1] = arg1;
		array[2] = arg2;
		array[3] = arg3;
		for (int i = 0; i < remainingCount; i++)
		{
			TypedReference nextArg = argIterator.GetNextArg();
			array[i + 4] = TypedReference.ToObject(nextArg);
		}
		stdout.Write(string.Format(format, array));
	}

	/// <summary>Writes the current line terminator to the standard output stream.</summary>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine()
	{
		stdout.WriteLine();
	}

	/// <summary>Writes the text representation of the specified Boolean value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(bool value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the specified Unicode character, followed by the current line terminator, value to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(char value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the specified array of Unicode characters, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="buffer">A Unicode character array. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(char[] buffer)
	{
		stdout.WriteLine(buffer);
	}

	/// <summary>Writes the text representation of the specified <see cref="T:System.Decimal" /> value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(decimal value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the text representation of the specified double-precision floating-point value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(double value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the text representation of the specified 32-bit signed integer value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(int value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the text representation of the specified 64-bit signed integer value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(long value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the text representation of the specified object, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(object value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the text representation of the specified single-precision floating-point value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(float value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the specified string value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(string value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the text representation of the specified 32-bit unsigned integer value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static void WriteLine(uint value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the text representation of the specified 64-bit unsigned integer value, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static void WriteLine(ulong value)
	{
		stdout.WriteLine(value);
	}

	/// <summary>Writes the text representation of the specified object, followed by the current line terminator, to the standard output stream using the specified format information.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg0">An object to write using <paramref name="format" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format specification in <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(string format, object arg0)
	{
		stdout.WriteLine(format, arg0);
	}

	/// <summary>Writes the text representation of the specified array of objects, followed by the current line terminator, to the standard output stream using the specified format information.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg">An array of objects to write using <paramref name="format" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> or <paramref name="arg" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format specification in <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(string format, params object[] arg)
	{
		if (arg == null)
		{
			stdout.WriteLine(format);
		}
		else
		{
			stdout.WriteLine(format, arg);
		}
	}

	/// <summary>Writes the specified subarray of Unicode characters, followed by the current line terminator, to the standard output stream.</summary>
	/// <param name="buffer">An array of Unicode characters. </param>
	/// <param name="index">The starting position in <paramref name="buffer" />. </param>
	/// <param name="count">The number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> plus <paramref name="count" /> specify a position that is not within <paramref name="buffer" />. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(char[] buffer, int index, int count)
	{
		stdout.WriteLine(buffer, index, count);
	}

	/// <summary>Writes the text representation of the specified objects, followed by the current line terminator, to the standard output stream using the specified format information.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg0">The first object to write using <paramref name="format" />. </param>
	/// <param name="arg1">The second object to write using <paramref name="format" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format specification in <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(string format, object arg0, object arg1)
	{
		stdout.WriteLine(format, arg0, arg1);
	}

	/// <summary>Writes the text representation of the specified objects, followed by the current line terminator, to the standard output stream using the specified format information.</summary>
	/// <param name="format">A composite format string (see Remarks).</param>
	/// <param name="arg0">The first object to write using <paramref name="format" />. </param>
	/// <param name="arg1">The second object to write using <paramref name="format" />. </param>
	/// <param name="arg2">The third object to write using <paramref name="format" />. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format specification in <paramref name="format" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void WriteLine(string format, object arg0, object arg1, object arg2)
	{
		stdout.WriteLine(format, arg0, arg1, arg2);
	}

	[CLSCompliant(false)]
	public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
	{
		ArgIterator argIterator = new ArgIterator(__arglist);
		int remainingCount = argIterator.GetRemainingCount();
		object[] array = new object[remainingCount + 4];
		array[0] = arg0;
		array[1] = arg1;
		array[2] = arg2;
		array[3] = arg3;
		for (int i = 0; i < remainingCount; i++)
		{
			TypedReference nextArg = argIterator.GetNextArg();
			array[i + 4] = TypedReference.ToObject(nextArg);
		}
		stdout.WriteLine(string.Format(format, array));
	}

	/// <summary>Reads the next character from the standard input stream.</summary>
	/// <returns>The next character from the input stream, or negative one (-1) if there are currently no more characters to be read.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	public static int Read()
	{
		if (stdin is CStreamReader && ConsoleDriver.IsConsole)
		{
			return ConsoleDriver.Read();
		}
		return stdin.Read();
	}

	/// <summary>Reads the next line of characters from the standard input stream.</summary>
	/// <returns>The next line of characters from the input stream, or null if no more lines are available.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters in the next line of characters is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static string ReadLine()
	{
		if (stdin is CStreamReader && ConsoleDriver.IsConsole)
		{
			return ConsoleDriver.ReadLine();
		}
		return stdin.ReadLine();
	}

	/// <summary>Plays the sound of a beep through the console speaker.</summary>
	/// <exception cref="T:System.Security.HostProtectionException">This method was executed on a server, such as SQL Server, that does not permit access to a user interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Beep()
	{
		Beep(1000, 500);
	}

	/// <summary>Plays the sound of a beep of a specified frequency and duration through the console speaker.</summary>
	/// <param name="frequency">The frequency of the beep, ranging from 37 to 32767 hertz.</param>
	/// <param name="duration">The duration of the beep measured in milliseconds.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="frequency" /> is less than 37 or more than 32767 hertz.-or-<paramref name="duration" /> is less than or equal to zero.</exception>
	/// <exception cref="T:System.Security.HostProtectionException">This method was executed on a server, such as SQL Server, that does not permit access to the console.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Beep(int frequency, int duration)
	{
		if (frequency < 37 || frequency > 32767)
		{
			throw new ArgumentOutOfRangeException("frequency");
		}
		if (duration <= 0)
		{
			throw new ArgumentOutOfRangeException("duration");
		}
		ConsoleDriver.Beep(frequency, duration);
	}

	/// <summary>Clears the console buffer and corresponding console window of display information.</summary>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Clear()
	{
		ConsoleDriver.Clear();
	}

	/// <summary>Copies a specified source area of the screen buffer to a specified destination area.</summary>
	/// <param name="sourceLeft">The leftmost column of the source area. </param>
	/// <param name="sourceTop">The topmost row of the source area. </param>
	/// <param name="sourceWidth">The number of columns in the source area. </param>
	/// <param name="sourceHeight">The number of rows in the source area. </param>
	/// <param name="targetLeft">The leftmost column of the destination area. </param>
	/// <param name="targetTop">The topmost row of the destination area. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">One or more of the parameters is less than zero.-or- <paramref name="sourceLeft" /> or <paramref name="targetLeft" /> is greater than or equal to <see cref="P:System.Console.BufferWidth" />.-or- <paramref name="sourceTop" /> or <paramref name="targetTop" /> is greater than or equal to <see cref="P:System.Console.BufferHeight" />.-or- <paramref name="sourceTop" /> + <paramref name="sourceHeight" /> is greater than or equal to <see cref="P:System.Console.BufferHeight" />.-or- <paramref name="sourceLeft" /> + <paramref name="sourceWidth" /> is greater than or equal to <see cref="P:System.Console.BufferWidth" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	[MonoLimitation("Implemented only on Windows")]
	public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
	{
		ConsoleDriver.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
	}

	/// <summary>Copies a specified source area of the screen buffer to a specified destination area.</summary>
	/// <param name="sourceLeft">The leftmost column of the source area. </param>
	/// <param name="sourceTop">The topmost row of the source area. </param>
	/// <param name="sourceWidth">The number of columns in the source area. </param>
	/// <param name="sourceHeight">The number of rows in the source area. </param>
	/// <param name="targetLeft">The leftmost column of the destination area. </param>
	/// <param name="targetTop">The topmost row of the destination area. </param>
	/// <param name="sourceChar">The character used to fill the source area. </param>
	/// <param name="sourceForeColor">The foreground color used to fill the source area. </param>
	/// <param name="sourceBackColor">The background color used to fill the source area. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">One or more of the parameters is less than zero.-or- <paramref name="sourceLeft" /> or <paramref name="targetLeft" /> is greater than or equal to <see cref="P:System.Console.BufferWidth" />.-or- <paramref name="sourceTop" /> or <paramref name="targetTop" /> is greater than or equal to <see cref="P:System.Console.BufferHeight" />.-or- <paramref name="sourceTop" /> + <paramref name="sourceHeight" /> is greater than or equal to <see cref="P:System.Console.BufferHeight" />.-or- <paramref name="sourceLeft" /> + <paramref name="sourceWidth" /> is greater than or equal to <see cref="P:System.Console.BufferWidth" />. </exception>
	/// <exception cref="T:System.ArgumentException">One or both of the color parameters is not a member of the <see cref="T:System.ConsoleColor" /> enumeration. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	[MonoLimitation("Implemented only on Windows")]
	public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
	{
		ConsoleDriver.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, sourceChar, sourceForeColor, sourceBackColor);
	}

	/// <summary>Obtains the next character or function key pressed by the user. The pressed key is displayed in the console window.</summary>
	/// <returns>A <see cref="T:System.ConsoleKeyInfo" /> object that describes the <see cref="T:System.ConsoleKey" /> constant and Unicode character, if any, that correspond to the pressed console key. The <see cref="T:System.ConsoleKeyInfo" /> object also describes, in a bitwise combination of <see cref="T:System.ConsoleModifiers" /> values, whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously with the console key.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Console.In" /> property is redirected from some stream other than the console.</exception>
	/// <filterpriority>1</filterpriority>
	public static ConsoleKeyInfo ReadKey()
	{
		return ReadKey(intercept: false);
	}

	/// <summary>Obtains the next character or function key pressed by the user. The pressed key is optionally displayed in the console window.</summary>
	/// <returns>A <see cref="T:System.ConsoleKeyInfo" /> object that describes the <see cref="T:System.ConsoleKey" /> constant and Unicode character, if any, that correspond to the pressed console key. The <see cref="T:System.ConsoleKeyInfo" /> object also describes, in a bitwise combination of <see cref="T:System.ConsoleModifiers" /> values, whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously with the console key.</returns>
	/// <param name="intercept">Determines whether to display the pressed key in the console window. true to not display the pressed key; otherwise, false. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Console.In" /> property is redirected from some stream other than the console.</exception>
	/// <filterpriority>1</filterpriority>
	public static ConsoleKeyInfo ReadKey(bool intercept)
	{
		return ConsoleDriver.ReadKey(intercept);
	}

	/// <summary>Sets the foreground and background console colors to their defaults.</summary>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static void ResetColor()
	{
		ConsoleDriver.ResetColor();
	}

	/// <summary>Sets the height and width of the screen buffer area to the specified values.</summary>
	/// <param name="width">The width of the buffer area measured in columns. </param>
	/// <param name="height">The height of the buffer area measured in rows. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="height" /> or <paramref name="width" /> is less than or equal to zero.-or- <paramref name="height" /> or <paramref name="width" /> is greater than or equal to <see cref="F:System.Int16.MaxValue" />.-or- <paramref name="width" /> is less than <see cref="P:System.Console.WindowLeft" /> + <see cref="P:System.Console.WindowWidth" />.-or- <paramref name="height" /> is less than <see cref="P:System.Console.WindowTop" /> + <see cref="P:System.Console.WindowHeight" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	[MonoLimitation("Only works on windows")]
	public static void SetBufferSize(int width, int height)
	{
		ConsoleDriver.SetBufferSize(width, height);
	}

	/// <summary>Sets the position of the cursor.</summary>
	/// <param name="left">The column position of the cursor. Columns are numbered from left to right starting at 0. </param>
	/// <param name="top">The row position of the cursor. Rows are numbered from top to bottom starting at 0. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="left" /> or <paramref name="top" /> is less than zero.-or- <paramref name="left" /> is greater than or equal to <see cref="P:System.Console.BufferWidth" />.-or- <paramref name="top" /> is greater than or equal to <see cref="P:System.Console.BufferHeight" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static void SetCursorPosition(int left, int top)
	{
		ConsoleDriver.SetCursorPosition(left, top);
	}

	/// <summary>Sets the position of the console window relative to the screen buffer.</summary>
	/// <param name="left">The column position of the upper left  corner of the console window. </param>
	/// <param name="top">The row position of the upper left corner of the console window. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="left" /> or <paramref name="top" /> is less than zero.-or- <paramref name="left" /> + <see cref="P:System.Console.WindowWidth" /> is greater than <see cref="P:System.Console.BufferWidth" />.-or- <paramref name="top" /> + <see cref="P:System.Console.WindowHeight" /> is greater than <see cref="P:System.Console.BufferHeight" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static void SetWindowPosition(int left, int top)
	{
		ConsoleDriver.SetWindowPosition(left, top);
	}

	/// <summary>Sets the height and width of the console window to the specified values.</summary>
	/// <param name="width">The width of the console window measured in columns. </param>
	/// <param name="height">The height of the console window measured in rows. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="width" /> or <paramref name="height" /> is less than or equal to zero.-or- <paramref name="width" /> plus <see cref="P:System.Console.WindowLeft" /> or <paramref name="height" /> plus <see cref="P:System.Console.WindowTop" /> is greater than or equal to <see cref="F:System.Int16.MaxValue" />. -or-<paramref name="width" /> or <paramref name="height" /> is greater than the largest possible window width or height for the current screen resolution and console font.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have permission to perform this action. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public static void SetWindowSize(int width, int height)
	{
		ConsoleDriver.SetWindowSize(width, height);
	}

	internal static void DoConsoleCancelEvent()
	{
		bool flag = true;
		if (cancel_event != null)
		{
			ConsoleCancelEventArgs consoleCancelEventArgs = new ConsoleCancelEventArgs(ConsoleSpecialKey.ControlC);
			Delegate[] invocationList = cancel_event.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				ConsoleCancelEventHandler consoleCancelEventHandler = (ConsoleCancelEventHandler)invocationList[i];
				try
				{
					consoleCancelEventHandler(null, consoleCancelEventArgs);
				}
				catch
				{
				}
			}
			flag = !consoleCancelEventArgs.Cancel;
		}
		if (flag)
		{
			Environment.Exit(58);
		}
	}
}
