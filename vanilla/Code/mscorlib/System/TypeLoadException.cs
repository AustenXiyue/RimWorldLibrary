using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>The exception that is thrown when type-loading failures occur.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class TypeLoadException : SystemException, ISerializable
{
	private string ClassName;

	private string AssemblyName;

	private string MessageArg;

	internal int ResourceId;

	/// <summary>Gets the error message for this exception.</summary>
	/// <returns>The error message string.</returns>
	/// <filterpriority>2</filterpriority>
	public override string Message
	{
		[SecuritySafeCritical]
		get
		{
			SetMessageField();
			return _message;
		}
	}

	/// <summary>Gets the fully qualified name of the type that causes the exception.</summary>
	/// <returns>The fully qualified type name.</returns>
	/// <filterpriority>2</filterpriority>
	public string TypeName
	{
		get
		{
			if (ClassName == null)
			{
				return string.Empty;
			}
			return ClassName;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TypeLoadException" /> class.</summary>
	public TypeLoadException()
		: base(Environment.GetResourceString("Failure has occurred while loading a type."))
	{
		SetErrorCode(-2146233054);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TypeLoadException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public TypeLoadException(string message)
		: base(message)
	{
		SetErrorCode(-2146233054);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TypeLoadException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public TypeLoadException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233054);
	}

	[SecurityCritical]
	private void SetMessageField()
	{
		if (_message != null)
		{
			return;
		}
		if (ClassName == null && ResourceId == 0)
		{
			_message = Environment.GetResourceString("Failure has occurred while loading a type.");
			return;
		}
		if (AssemblyName == null)
		{
			AssemblyName = Environment.GetResourceString("[Unknown]");
		}
		if (ClassName == null)
		{
			ClassName = Environment.GetResourceString("[Unknown]");
		}
		string text = null;
		text = "Could not load type '{0}' from assembly '{1}'.";
		_message = string.Format(CultureInfo.CurrentCulture, text, ClassName, AssemblyName, MessageArg);
	}

	private TypeLoadException(string className, string assemblyName)
		: this(className, assemblyName, null, 0)
	{
	}

	[SecurityCritical]
	private TypeLoadException(string className, string assemblyName, string messageArg, int resourceId)
		: base(null)
	{
		SetErrorCode(-2146233054);
		ClassName = className;
		AssemblyName = assemblyName;
		MessageArg = messageArg;
		ResourceId = resourceId;
		SetMessageField();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TypeLoadException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> object is null. </exception>
	protected TypeLoadException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		ClassName = info.GetString("TypeLoadClassName");
		AssemblyName = info.GetString("TypeLoadAssemblyName");
		MessageArg = info.GetString("TypeLoadMessageArg");
		ResourceId = info.GetInt32("TypeLoadResourceID");
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the class name, method name, resource ID, and additional exception information.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> object is null. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("TypeLoadClassName", ClassName, typeof(string));
		info.AddValue("TypeLoadAssemblyName", AssemblyName, typeof(string));
		info.AddValue("TypeLoadMessageArg", MessageArg, typeof(string));
		info.AddValue("TypeLoadResourceID", ResourceId);
	}
}
