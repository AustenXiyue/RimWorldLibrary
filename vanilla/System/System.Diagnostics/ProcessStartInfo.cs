using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace System.Diagnostics;

/// <summary>Specifies a set of values that are used when you start a process.</summary>
/// <filterpriority>2</filterpriority>
[StructLayout(LayoutKind.Sequential)]
[TypeConverter(typeof(ExpandableObjectConverter))]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true, SelfAffectingProcessMgmt = true)]
public sealed class ProcessStartInfo
{
	private string fileName;

	private string arguments;

	private string directory;

	private string verb;

	private ProcessWindowStyle windowStyle;

	private bool errorDialog;

	private IntPtr errorDialogParentHandle;

	private bool useShellExecute = true;

	private string userName;

	private string domain;

	private SecureString password;

	private string passwordInClearText;

	private bool loadUserProfile;

	private bool redirectStandardInput;

	private bool redirectStandardOutput;

	private bool redirectStandardError;

	private Encoding standardOutputEncoding;

	private Encoding standardErrorEncoding;

	private bool createNoWindow;

	private WeakReference weakParentProcess;

	internal StringDictionary environmentVariables;

	private IDictionary<string, string> environment;

	private static readonly string[] empty = new string[0];

	/// <summary>Gets or sets the verb to use when opening the application or document specified by the <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property.</summary>
	/// <returns>The action to take with the file that the process opens. The default is an empty string (""), which signifies no action.</returns>
	/// <filterpriority>2</filterpriority>
	[TypeConverter("System.Diagnostics.Design.VerbConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[MonitoringDescription("The verb to apply to the document specified by the FileName property.")]
	[NotifyParentProperty(true)]
	[DefaultValue("")]
	public string Verb
	{
		get
		{
			if (verb == null)
			{
				return string.Empty;
			}
			return verb;
		}
		set
		{
			verb = value;
		}
	}

	/// <summary>Gets or sets the set of command-line arguments to use when starting the application.</summary>
	/// <returns>File type–specific arguments that the system can associate with the application specified in the <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property. The default is an empty string (""). On Windows Vista and earlier versions of the Windows operating system, the length of the arguments added to the length of the full path to the process must be less than 2080. On Windows 7 and later versions, the length must be less than 32699.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue("")]
	[SettingsBindable(true)]
	[MonitoringDescription("Command line arguments that will be passed to the application specified by the FileName property.")]
	[NotifyParentProperty(true)]
	[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public string Arguments
	{
		get
		{
			if (arguments == null)
			{
				return string.Empty;
			}
			return arguments;
		}
		set
		{
			arguments = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to start the process in a new window.</summary>
	/// <returns>true if the process should be started without creating a new window to contain it; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[NotifyParentProperty(true)]
	[MonitoringDescription("Whether to start the process without creating a new window to contain it.")]
	[DefaultValue(false)]
	public bool CreateNoWindow
	{
		get
		{
			return createNoWindow;
		}
		set
		{
			createNoWindow = value;
		}
	}

	/// <summary>Gets search paths for files, directories for temporary files, application-specific options, and other similar information.</summary>
	/// <returns>A string dictionary that provides environment variables that apply to this process and child processes. The default is null.</returns>
	/// <filterpriority>1</filterpriority>
	[Editor("System.Diagnostics.Design.StringDictionaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[DefaultValue(null)]
	[MonitoringDescription("Set of environment variables that apply to this process and child processes.")]
	[NotifyParentProperty(true)]
	public StringDictionary EnvironmentVariables
	{
		get
		{
			if (environmentVariables == null)
			{
				environmentVariables = new CaseSensitiveStringDictionary();
				if (weakParentProcess == null || !weakParentProcess.IsAlive || ((Component)weakParentProcess.Target).Site == null || !((Component)weakParentProcess.Target).Site.DesignMode)
				{
					foreach (DictionaryEntry environmentVariable in System.Environment.GetEnvironmentVariables())
					{
						environmentVariables.Add((string)environmentVariable.Key, (string)environmentVariable.Value);
					}
				}
			}
			return environmentVariables;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DefaultValue(null)]
	[NotifyParentProperty(true)]
	public IDictionary<string, string> Environment
	{
		get
		{
			if (environment == null)
			{
				environment = EnvironmentVariables.AsGenericDictionary();
			}
			return environment;
		}
	}

	/// <summary>Gets or sets a value indicating whether the input for an application is read from the <see cref="P:System.Diagnostics.Process.StandardInput" /> stream.</summary>
	/// <returns>true if input should be read from <see cref="P:System.Diagnostics.Process.StandardInput" />; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[DefaultValue(false)]
	[MonitoringDescription("Whether the process command input is read from the Process instance's StandardInput member.")]
	[NotifyParentProperty(true)]
	public bool RedirectStandardInput
	{
		get
		{
			return redirectStandardInput;
		}
		set
		{
			redirectStandardInput = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the output of an application is written to the <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream.</summary>
	/// <returns>true if output should be written to <see cref="P:System.Diagnostics.Process.StandardOutput" />; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("Whether the process output is written to the Process instance's StandardOutput member.")]
	[DefaultValue(false)]
	[NotifyParentProperty(true)]
	public bool RedirectStandardOutput
	{
		get
		{
			return redirectStandardOutput;
		}
		set
		{
			redirectStandardOutput = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the error output of an application is written to the <see cref="P:System.Diagnostics.Process.StandardError" /> stream.</summary>
	/// <returns>true if error output should be written to <see cref="P:System.Diagnostics.Process.StandardError" />; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[DefaultValue(false)]
	[MonitoringDescription("Whether the process's error output is written to the Process instance's StandardError member.")]
	[NotifyParentProperty(true)]
	public bool RedirectStandardError
	{
		get
		{
			return redirectStandardError;
		}
		set
		{
			redirectStandardError = value;
		}
	}

	/// <summary>Gets or sets the preferred encoding for error output.</summary>
	/// <returns>An object that represents the preferred encoding for error output. The default is null.</returns>
	public Encoding StandardErrorEncoding
	{
		get
		{
			return standardErrorEncoding;
		}
		set
		{
			standardErrorEncoding = value;
		}
	}

	/// <summary>Gets or sets the preferred encoding for standard output.</summary>
	/// <returns>An object that represents the preferred encoding for standard output. The default is null.</returns>
	public Encoding StandardOutputEncoding
	{
		get
		{
			return standardOutputEncoding;
		}
		set
		{
			standardOutputEncoding = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to use the operating system shell to start the process.</summary>
	/// <returns>true if the shell should be used when starting the process; false if the process should be created directly from the executable file. The default is true.</returns>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("Whether to use the operating system shell to start the process.")]
	[DefaultValue(true)]
	[NotifyParentProperty(true)]
	public bool UseShellExecute
	{
		get
		{
			return useShellExecute;
		}
		set
		{
			useShellExecute = value;
		}
	}

	/// <summary>Gets or sets the user name to be used when starting the process.</summary>
	/// <returns>The user name to use when starting the process.</returns>
	/// <filterpriority>1</filterpriority>
	[NotifyParentProperty(true)]
	public string UserName
	{
		get
		{
			if (userName == null)
			{
				return string.Empty;
			}
			return userName;
		}
		set
		{
			userName = value;
		}
	}

	/// <summary>Gets or sets a secure string that contains the user password to use when starting the process.</summary>
	/// <returns>The user password to use when starting the process.</returns>
	/// <filterpriority>1</filterpriority>
	public SecureString Password
	{
		get
		{
			return password;
		}
		set
		{
			password = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string PasswordInClearText
	{
		get
		{
			return passwordInClearText;
		}
		set
		{
			passwordInClearText = value;
		}
	}

	/// <summary>Gets or sets a value that identifies the domain to use when starting the process. </summary>
	/// <returns>The Active Directory domain to use when starting the process. The domain property is primarily of interest to users within enterprise environments that use Active Directory.</returns>
	/// <filterpriority>1</filterpriority>
	[NotifyParentProperty(true)]
	public string Domain
	{
		get
		{
			if (domain == null)
			{
				return string.Empty;
			}
			return domain;
		}
		set
		{
			domain = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the Windows user profile is to be loaded from the registry. </summary>
	/// <returns>true if the Windows user profile should be loaded; otherwise, false. The default is false.</returns>
	/// <filterpriority>1</filterpriority>
	[NotifyParentProperty(true)]
	public bool LoadUserProfile
	{
		get
		{
			return loadUserProfile;
		}
		set
		{
			loadUserProfile = value;
		}
	}

	/// <summary>Gets or sets the application or document to start.</summary>
	/// <returns>The name of the application to start, or the name of a document of a file type that is associated with an application and that has a default open action available to it. The default is an empty string ("").</returns>
	/// <filterpriority>1</filterpriority>
	[Editor("System.Diagnostics.Design.StartFileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[MonitoringDescription("The name of the application, document or URL to start.")]
	[SettingsBindable(true)]
	[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[NotifyParentProperty(true)]
	[DefaultValue("")]
	public string FileName
	{
		get
		{
			if (fileName == null)
			{
				return string.Empty;
			}
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	/// <summary>When the <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property is false, gets or sets the working directory for the process to be started. When <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> is true, gets or sets the directory that contains the process to be started.</summary>
	/// <returns>When <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> is true, the fully qualified name of the directory that contains the process to be started. When the <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property is false, the working directory for the process to be started. The default is an empty string ("").</returns>
	/// <filterpriority>1</filterpriority>
	[SettingsBindable(true)]
	[DefaultValue("")]
	[MonitoringDescription("The initial working directory for the process.")]
	[Editor("System.Diagnostics.Design.WorkingDirectoryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[NotifyParentProperty(true)]
	public string WorkingDirectory
	{
		get
		{
			if (directory == null)
			{
				return string.Empty;
			}
			return directory;
		}
		set
		{
			directory = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether an error dialog box is displayed to the user if the process cannot be started.</summary>
	/// <returns>true if an error dialog box should be displayed on the screen if the process cannot be started; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[NotifyParentProperty(true)]
	[MonitoringDescription("Whether to show an error dialog to the user if there is an error.")]
	[DefaultValue(false)]
	public bool ErrorDialog
	{
		get
		{
			return errorDialog;
		}
		set
		{
			errorDialog = value;
		}
	}

	/// <summary>Gets or sets the window handle to use when an error dialog box is shown for a process that cannot be started.</summary>
	/// <returns>A pointer to the handle of the error dialog box that results from a process start failure.</returns>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public IntPtr ErrorDialogParentHandle
	{
		get
		{
			return errorDialogParentHandle;
		}
		set
		{
			errorDialogParentHandle = value;
		}
	}

	/// <summary>Gets or sets the window state to use when the process is started.</summary>
	/// <returns>One of the enumeration values that indicates whether the process is started in a window that is maximized, minimized, normal (neither maximized nor minimized), or not visible. The default is Normal.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The window style is not one of the <see cref="T:System.Diagnostics.ProcessWindowStyle" /> enumeration members. </exception>
	/// <filterpriority>2</filterpriority>
	[MonitoringDescription("How the main window should be created when the process starts.")]
	[DefaultValue(ProcessWindowStyle.Normal)]
	[NotifyParentProperty(true)]
	public ProcessWindowStyle WindowStyle
	{
		get
		{
			return windowStyle;
		}
		set
		{
			if (!Enum.IsDefined(typeof(ProcessWindowStyle), value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(ProcessWindowStyle));
			}
			windowStyle = value;
		}
	}

	internal bool HaveEnvVars => environmentVariables != null;

	/// <summary>Gets the set of verbs associated with the type of file specified by the <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property.</summary>
	/// <returns>The actions that the system can apply to the file indicated by the <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property.</returns>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public string[] Verbs
	{
		get
		{
			PlatformID platform = System.Environment.OSVersion.Platform;
			if (platform == PlatformID.Unix || platform == PlatformID.MacOSX || platform == (PlatformID)128)
			{
				return empty;
			}
			string text = (string.IsNullOrEmpty(fileName) ? null : Path.GetExtension(fileName));
			if (text == null)
			{
				return empty;
			}
			RegistryKey registryKey = null;
			RegistryKey registryKey2 = null;
			RegistryKey registryKey3 = null;
			try
			{
				registryKey = Registry.ClassesRoot.OpenSubKey(text);
				string text2 = ((registryKey != null) ? (registryKey.GetValue(null) as string) : null);
				registryKey2 = ((text2 != null) ? Registry.ClassesRoot.OpenSubKey(text2) : null);
				registryKey3 = registryKey2?.OpenSubKey("shell");
				return registryKey3?.GetSubKeyNames();
			}
			finally
			{
				registryKey3?.Close();
				registryKey2?.Close();
				registryKey?.Close();
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.ProcessStartInfo" /> class without specifying a file name with which to start the process.</summary>
	public ProcessStartInfo()
	{
	}

	internal ProcessStartInfo(Process parent)
	{
		weakParentProcess = new WeakReference(parent);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.ProcessStartInfo" /> class and specifies a file name such as an application or document with which to start the process.</summary>
	/// <param name="fileName">An application or document with which to start a process. </param>
	public ProcessStartInfo(string fileName)
	{
		this.fileName = fileName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.ProcessStartInfo" /> class, specifies an application file name with which to start the process, and specifies a set of command-line arguments to pass to the application.</summary>
	/// <param name="fileName">An application with which to start a process. </param>
	/// <param name="arguments">Command-line arguments to pass to the application when the process starts. </param>
	public ProcessStartInfo(string fileName, string arguments)
	{
		this.fileName = fileName;
		this.arguments = arguments;
	}
}
