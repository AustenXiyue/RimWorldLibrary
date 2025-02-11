using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.IO;

/// <summary>The exception that is thrown when an attempt to access a file that does not exist on disk fails.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class FileNotFoundException : IOException
{
	private string _fileName;

	private string _fusionLog;

	/// <summary>Gets the error message that explains the reason for the exception.</summary>
	/// <returns>The error message.</returns>
	/// <filterpriority>2</filterpriority>
	public override string Message
	{
		get
		{
			SetMessageField();
			return _message;
		}
	}

	/// <summary>Gets the name of the file that cannot be found.</summary>
	/// <returns>The name of the file, or null if no file name was passed to the constructor for this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public string FileName => _fileName;

	/// <summary>Gets the log file that describes why loading of an assembly failed.</summary>
	/// <returns>The errors reported by the assembly cache.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileNotFoundException" /> class with its message string set to a system-supplied message and its HRESULT set to COR_E_FILENOTFOUND.</summary>
	public FileNotFoundException()
		: base(Environment.GetResourceString("Unable to find the specified file."))
	{
		SetErrorCode(-2147024894);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileNotFoundException" /> class with its message string set to <paramref name="message" /> and its HRESULT set to COR_E_FILENOTFOUND.</summary>
	/// <param name="message">A description of the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	public FileNotFoundException(string message)
		: base(message)
	{
		SetErrorCode(-2147024894);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileNotFoundException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">A description of the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public FileNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2147024894);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileNotFoundException" /> class with its message string set to <paramref name="message" />, specifying the file name that cannot be found, and its HRESULT set to COR_E_FILENOTFOUND.</summary>
	/// <param name="message">A description of the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="fileName">The full name of the file with the invalid image. </param>
	public FileNotFoundException(string message, string fileName)
		: base(message)
	{
		SetErrorCode(-2147024894);
		_fileName = fileName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileNotFoundException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="fileName">The full name of the file with the invalid image. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public FileNotFoundException(string message, string fileName, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2147024894);
		_fileName = fileName;
	}

	private void SetMessageField()
	{
		if (_message == null)
		{
			if (_fileName == null && base.HResult == -2146233088)
			{
				_message = Environment.GetResourceString("Unable to find the specified file.");
			}
			else if (_fileName != null)
			{
				_message = FileLoadException.FormatFileLoadExceptionMessage(_fileName, base.HResult);
			}
		}
	}

	/// <summary>Returns the fully qualified name of this exception and possibly the error message, the name of the inner exception, and the stack trace.</summary>
	/// <returns>The fully qualified name of this exception and possibly the error message, the name of the inner exception, and the stack trace.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.FileNotFoundException" /> class with the specified serialization and context information.</summary>
	/// <param name="info">An object that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">An object that contains contextual information about the source or destination. </param>
	protected FileNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_fileName = info.GetString("FileNotFound_FileName");
		try
		{
			_fusionLog = info.GetString("FileNotFound_FusionLog");
		}
		catch
		{
			_fusionLog = null;
		}
	}

	private FileNotFoundException(string fileName, string fusionLog, int hResult)
		: base(null)
	{
		SetErrorCode(hResult);
		_fileName = fileName;
		_fusionLog = fusionLog;
		SetMessageField();
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the file name and additional exception information.</summary>
	/// <param name="info">The object that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The object that contains contextual information about the source or destination. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("FileNotFound_FileName", _fileName, typeof(string));
		try
		{
			info.AddValue("FileNotFound_FusionLog", FusionLog, typeof(string));
		}
		catch (SecurityException)
		{
		}
	}
}
