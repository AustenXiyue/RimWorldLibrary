using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.IO;

/// <summary>The exception that is thrown when a managed assembly is found but cannot be loaded.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class FileLoadException : IOException
{
	private string _fileName;

	private string _fusionLog;

	/// <summary>Gets the error message and the name of the file that caused this exception.</summary>
	/// <returns>A string containing the error message and the name of the file that caused this exception.</returns>
	/// <filterpriority>2</filterpriority>
	public override string Message
	{
		get
		{
			SetMessageField();
			return _message;
		}
	}

	/// <summary>Gets the name of the file that causes this exception.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the name of the file with the invalid image, or a null reference if no file name was passed to the constructor for the current instance.</returns>
	/// <filterpriority>2</filterpriority>
	public string FileName => _fileName;

	/// <summary>Gets the log file that describes why an assembly load failed.</summary>
	/// <returns>A string containing errors reported by the assembly cache.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	public string FusionLog
	{
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
		get
		{
			return _fusionLog;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileLoadException" /> class, setting the <see cref="P:System.Exception.Message" /> property of the new instance to a system-supplied message that describes the error, such as "Could not load the specified file." This message takes into account the current system culture.</summary>
	public FileLoadException()
		: base(Environment.GetResourceString("Could not load the specified file."))
	{
		SetErrorCode(-2146232799);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileLoadException" /> class with the specified error message.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	public FileLoadException(string message)
		: base(message)
	{
		SetErrorCode(-2146232799);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileLoadException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public FileLoadException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146232799);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileLoadException" /> class with a specified error message and the name of the file that could not be loaded.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="fileName">A <see cref="T:System.String" /> containing the name of the file that was not loaded. </param>
	public FileLoadException(string message, string fileName)
		: base(message)
	{
		SetErrorCode(-2146232799);
		_fileName = fileName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileLoadException" /> class with a specified error message, the name of the file that could not be loaded, and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="fileName">A <see cref="T:System.String" /> containing the name of the file that was not loaded. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public FileLoadException(string message, string fileName, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146232799);
		_fileName = fileName;
	}

	private void SetMessageField()
	{
		if (_message == null)
		{
			_message = FormatFileLoadExceptionMessage(_fileName, base.HResult);
		}
	}

	/// <summary>Returns the fully qualified name of the current exception, and possibly the error message, the name of the inner exception, and the stack trace.</summary>
	/// <returns>A string containing the fully qualified name of this exception, and possibly the error message, the name of the inner exception, and the stack trace, depending on which <see cref="T:System.IO.FileLoadException" /> constructor is used.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	public override string ToString()
	{
		string text = GetType().FullName + ": " + Message;
		if (_fileName != null && _fileName.Length != 0)
		{
			text = text + Environment.NewLine + Environment.GetResourceString("File name: '{0}'", _fileName);
		}
		if (base.InnerException != null)
		{
			text = text + " ---> " + base.InnerException.ToString();
		}
		if (StackTrace != null)
		{
			text = text + Environment.NewLine + StackTrace;
		}
		try
		{
			if (FusionLog != null)
			{
				if (text == null)
				{
					text = " ";
				}
				text += Environment.NewLine;
				text += Environment.NewLine;
				text += FusionLog;
			}
		}
		catch (SecurityException)
		{
		}
		return text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileLoadException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	protected FileLoadException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_fileName = info.GetString("FileLoad_FileName");
		try
		{
			_fusionLog = info.GetString("FileLoad_FusionLog");
		}
		catch
		{
			_fusionLog = null;
		}
	}

	private FileLoadException(string fileName, string fusionLog, int hResult)
		: base(null)
	{
		SetErrorCode(hResult);
		_fileName = fileName;
		_fusionLog = fusionLog;
		SetMessageField();
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the file name and additional exception information.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("FileLoad_FileName", _fileName, typeof(string));
		try
		{
			info.AddValue("FileLoad_FusionLog", FusionLog, typeof(string));
		}
		catch (SecurityException)
		{
		}
	}

	[SecuritySafeCritical]
	internal static string FormatFileLoadExceptionMessage(string fileName, int hResult)
	{
		return string.Format(CultureInfo.InvariantCulture, "Could not load file or assembly '{0}' or one of its dependencies", fileName);
	}
}
