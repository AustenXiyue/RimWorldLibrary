using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents errors that occur during application execution.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_Exception))]
[ComVisible(true)]
public class Exception : ISerializable, _Exception
{
	[Serializable]
	internal class __RestrictedErrorObject
	{
		[NonSerialized]
		private object _realErrorObject;

		public object RealErrorObject => _realErrorObject;

		internal __RestrictedErrorObject(object errorObject)
		{
			_realErrorObject = errorObject;
		}
	}

	internal enum ExceptionMessageKind
	{
		ThreadAbort = 1,
		ThreadInterrupted,
		OutOfMemory
	}

	[OptionalField]
	private static object s_EDILock = new object();

	private string _className;

	internal string _message;

	private IDictionary _data;

	private Exception _innerException;

	private string _helpURL;

	private object _stackTrace;

	private string _stackTraceString;

	private string _remoteStackTraceString;

	private int _remoteStackIndex;

	private object _dynamicMethods;

	internal int _HResult;

	private string _source;

	[OptionalField(VersionAdded = 4)]
	private SafeSerializationManager _safeSerializationManager;

	internal StackTrace[] captured_traces;

	private IntPtr[] native_trace_ips;

	private const int _COMPlusExceptionCode = -532462766;

	/// <summary>Gets a message that describes the current exception.</summary>
	/// <returns>The error message that explains the reason for the exception, or an empty string ("").</returns>
	/// <filterpriority>1</filterpriority>
	public virtual string Message
	{
		get
		{
			if (_message == null)
			{
				if (_className == null)
				{
					_className = GetClassName();
				}
				return Environment.GetResourceString("Exception of type '{0}' was thrown.", _className);
			}
			return _message;
		}
	}

	/// <summary>Gets a collection of key/value pairs that provide additional user-defined information about the exception.</summary>
	/// <returns>An object that implements the <see cref="T:System.Collections.IDictionary" /> interface and contains a collection of user-defined key/value pairs. The default is an empty collection.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IDictionary Data
	{
		[SecuritySafeCritical]
		get
		{
			if (_data == null)
			{
				if (IsImmutableAgileException(this))
				{
					_data = new EmptyReadOnlyDictionaryInternal();
				}
				else
				{
					_data = new ListDictionaryInternal();
				}
			}
			return _data;
		}
	}

	/// <summary>Gets the <see cref="T:System.Exception" /> instance that caused the current exception.</summary>
	/// <returns>An instance of Exception that describes the error that caused the current exception. The InnerException property returns the same value as was passed into the constructor, or a null reference (Nothing in Visual Basic) if the inner exception value was not supplied to the constructor. This property is read-only.</returns>
	/// <filterpriority>1</filterpriority>
	public Exception InnerException => _innerException;

	/// <summary>Gets the method that throws the current exception.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodBase" /> that threw the current exception.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public MethodBase TargetSite
	{
		[SecuritySafeCritical]
		get
		{
			StackTrace stackTrace = new StackTrace(this, fNeedFileInfo: true);
			if (stackTrace.FrameCount > 0)
			{
				return stackTrace.GetFrame(0).GetMethod();
			}
			return null;
		}
	}

	/// <summary>Gets a string representation of the immediate frames on the call stack.</summary>
	/// <returns>A string that describes the immediate frames of the call stack.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual string StackTrace => GetStackTrace(needFileInfo: true);

	/// <summary>Gets or sets a link to the help file associated with this exception.</summary>
	/// <returns>The Uniform Resource Name (URN) or Uniform Resource Locator (URL).</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string HelpLink
	{
		get
		{
			return _helpURL;
		}
		set
		{
			_helpURL = value;
		}
	}

	/// <summary>Gets or sets the name of the application or the object that causes the error.</summary>
	/// <returns>The name of the application or the object that causes the error.</returns>
	/// <exception cref="T:System.ArgumentException">The object must be a runtime <see cref="N:System.Reflection" /> object</exception>
	/// <filterpriority>2</filterpriority>
	public virtual string Source
	{
		get
		{
			if (_source == null)
			{
				StackTrace stackTrace = new StackTrace(this, fNeedFileInfo: true);
				if (stackTrace.FrameCount > 0)
				{
					MethodBase method = stackTrace.GetFrame(0).GetMethod();
					if (method != null)
					{
						_source = method.DeclaringType.Assembly.GetName().Name;
					}
				}
			}
			return _source;
		}
		set
		{
			_source = value;
		}
	}

	internal string RemoteStackTrace => _remoteStackTraceString;

	/// <summary>Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.</summary>
	/// <returns>The HRESULT value.</returns>
	public int HResult
	{
		get
		{
			return _HResult;
		}
		protected set
		{
			_HResult = value;
		}
	}

	internal bool IsTransient
	{
		[SecuritySafeCritical]
		get
		{
			return nIsTransient(_HResult);
		}
	}

	/// <summary>Occurs when an exception is serialized to create an exception state object that contains serialized data about the exception.</summary>
	protected event EventHandler<SafeSerializationEventArgs> SerializeObjectState
	{
		add
		{
			_safeSerializationManager.SerializeObjectState += value;
		}
		remove
		{
			_safeSerializationManager.SerializeObjectState -= value;
		}
	}

	private void Init()
	{
		_message = null;
		_stackTrace = null;
		_dynamicMethods = null;
		HResult = -2146233088;
		_safeSerializationManager = new SafeSerializationManager();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class.</summary>
	public Exception()
	{
		Init();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public Exception(string message)
	{
		Init();
		_message = message;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
	public Exception(string message, Exception innerException)
	{
		Init();
		_message = message;
		_innerException = innerException;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
	[SecuritySafeCritical]
	protected Exception(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		_className = info.GetString("ClassName");
		_message = info.GetString("Message");
		_data = (IDictionary)info.GetValueNoThrow("Data", typeof(IDictionary));
		_innerException = (Exception)info.GetValue("InnerException", typeof(Exception));
		_helpURL = info.GetString("HelpURL");
		_stackTraceString = info.GetString("StackTraceString");
		_remoteStackTraceString = info.GetString("RemoteStackTraceString");
		_remoteStackIndex = info.GetInt32("RemoteStackIndex");
		HResult = info.GetInt32("HResult");
		_source = info.GetString("Source");
		_safeSerializationManager = info.GetValueNoThrow("SafeSerializationManager", typeof(SafeSerializationManager)) as SafeSerializationManager;
		if (_className == null || HResult == 0)
		{
			throw new SerializationException(Environment.GetResourceString("Insufficient state to return the real object."));
		}
		if (context.State == StreamingContextStates.CrossAppDomain)
		{
			_remoteStackTraceString += _stackTraceString;
			_stackTraceString = null;
		}
	}

	private static bool IsImmutableAgileException(Exception e)
	{
		return false;
	}

	[FriendAccessAllowed]
	internal void AddExceptionDataForRestrictedErrorInfo(string restrictedError, string restrictedErrorReference, string restrictedCapabilitySid, object restrictedErrorObject, bool hasrestrictedLanguageErrorObject = false)
	{
		IDictionary data = Data;
		if (data != null)
		{
			data.Add("RestrictedDescription", restrictedError);
			data.Add("RestrictedErrorReference", restrictedErrorReference);
			data.Add("RestrictedCapabilitySid", restrictedCapabilitySid);
			data.Add("__RestrictedErrorObject", (restrictedErrorObject == null) ? null : new __RestrictedErrorObject(restrictedErrorObject));
			data.Add("__HasRestrictedLanguageErrorObject", hasrestrictedLanguageErrorObject);
		}
	}

	internal bool TryGetRestrictedLanguageErrorObject(out object restrictedErrorObject)
	{
		restrictedErrorObject = null;
		if (Data != null && Data.Contains("__HasRestrictedLanguageErrorObject"))
		{
			if (Data.Contains("__RestrictedErrorObject") && Data["__RestrictedErrorObject"] is __RestrictedErrorObject _RestrictedErrorObject)
			{
				restrictedErrorObject = _RestrictedErrorObject.RealErrorObject;
			}
			return (bool)Data["__HasRestrictedLanguageErrorObject"];
		}
		return false;
	}

	private string GetClassName()
	{
		if (_className == null)
		{
			_className = GetType().ToString();
		}
		return _className;
	}

	/// <summary>When overridden in a derived class, returns the <see cref="T:System.Exception" /> that is the root cause of one or more subsequent exceptions.</summary>
	/// <returns>The first exception thrown in a chain of exceptions. If the <see cref="P:System.Exception.InnerException" /> property of the current exception is a null reference (Nothing in Visual Basic), this property returns the current exception.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Exception GetBaseException()
	{
		Exception innerException = InnerException;
		Exception result = this;
		while (innerException != null)
		{
			result = innerException;
			innerException = innerException.InnerException;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern IRuntimeMethodInfo GetMethodFromStackTrace(object stackTrace);

	private string GetStackTrace(bool needFileInfo)
	{
		string text = _stackTraceString;
		string text2 = _remoteStackTraceString;
		if (!needFileInfo)
		{
			text = StripFileInfo(text, isRemoteStackTrace: false);
			text2 = StripFileInfo(text2, isRemoteStackTrace: true);
		}
		if (text != null)
		{
			return text2 + text;
		}
		if (_stackTrace == null)
		{
			return text2;
		}
		string stackTrace = Environment.GetStackTrace(this, needFileInfo);
		return text2 + stackTrace;
	}

	[FriendAccessAllowed]
	internal void SetErrorCode(int hr)
	{
		HResult = hr;
	}

	/// <summary>Creates and returns a string representation of the current exception.</summary>
	/// <returns>A string representation of the current exception.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string ToString()
	{
		return ToString(needFileLineInfo: true, needMessage: true);
	}

	private string ToString(bool needFileLineInfo, bool needMessage)
	{
		string text = (needMessage ? Message : null);
		string text2 = ((text != null && text.Length > 0) ? (GetClassName() + ": " + text) : GetClassName());
		if (_innerException != null)
		{
			text2 = text2 + " ---> " + _innerException.ToString(needFileLineInfo, needMessage) + Environment.NewLine + "   " + Environment.GetResourceString("--- End of inner exception stack trace ---");
		}
		string stackTrace = GetStackTrace(needFileLineInfo);
		if (stackTrace != null)
		{
			text2 = text2 + Environment.NewLine + stackTrace;
		}
		return text2;
	}

	/// <summary>When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic). </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		string text = _stackTraceString;
		if (_stackTrace != null && text == null)
		{
			text = Environment.GetStackTrace(this, needFileInfo: true);
		}
		if (_source == null)
		{
			_source = Source;
		}
		info.AddValue("ClassName", GetClassName(), typeof(string));
		info.AddValue("Message", _message, typeof(string));
		info.AddValue("Data", _data, typeof(IDictionary));
		info.AddValue("InnerException", _innerException, typeof(Exception));
		info.AddValue("HelpURL", _helpURL, typeof(string));
		info.AddValue("StackTraceString", text, typeof(string));
		info.AddValue("RemoteStackTraceString", _remoteStackTraceString, typeof(string));
		info.AddValue("RemoteStackIndex", _remoteStackIndex, typeof(int));
		info.AddValue("ExceptionMethod", null);
		info.AddValue("HResult", HResult);
		info.AddValue("Source", _source, typeof(string));
		if (_safeSerializationManager != null && _safeSerializationManager.IsActive)
		{
			info.AddValue("SafeSerializationManager", _safeSerializationManager, typeof(SafeSerializationManager));
			_safeSerializationManager.CompleteSerialization(this, info, context);
		}
	}

	internal Exception PrepForRemoting()
	{
		string text = null;
		text = ((_remoteStackIndex != 0) ? (StackTrace + Environment.NewLine + Environment.NewLine + "Exception rethrown at [" + _remoteStackIndex + "]: " + Environment.NewLine) : (Environment.NewLine + "Server stack trace: " + Environment.NewLine + StackTrace + Environment.NewLine + Environment.NewLine + "Exception rethrown at [" + _remoteStackIndex + "]: " + Environment.NewLine));
		_remoteStackTraceString = text;
		_remoteStackIndex++;
		return this;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext context)
	{
		_stackTrace = null;
		if (_safeSerializationManager == null)
		{
			_safeSerializationManager = new SafeSerializationManager();
		}
		else
		{
			_safeSerializationManager.CompleteDeserialization(this);
		}
	}

	internal void InternalPreserveStackTrace()
	{
		string stackTrace = StackTrace;
		if (stackTrace != null && stackTrace.Length > 0)
		{
			_remoteStackTraceString = stackTrace + Environment.NewLine;
		}
		_stackTrace = null;
		_stackTraceString = null;
	}

	private string StripFileInfo(string stackTrace, bool isRemoteStackTrace)
	{
		return stackTrace;
	}

	[SecuritySafeCritical]
	internal void RestoreExceptionDispatchInfo(ExceptionDispatchInfo exceptionDispatchInfo)
	{
		captured_traces = (StackTrace[])exceptionDispatchInfo.BinaryStackTraceArray;
		_stackTrace = null;
		_stackTraceString = null;
	}

	[SecurityCritical]
	internal virtual string InternalToString()
	{
		bool flag = true;
		return ToString(flag, needMessage: true);
	}

	/// <summary>Gets the runtime type of the current instance.</summary>
	/// <returns>A <see cref="T:System.Type" /> object that represents the exact runtime type of the current instance.</returns>
	/// <filterpriority>2</filterpriority>
	public new Type GetType()
	{
		return base.GetType();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern bool nIsTransient(int hr);

	[SecuritySafeCritical]
	internal static string GetMessageFromNativeResources(ExceptionMessageKind kind)
	{
		return kind switch
		{
			ExceptionMessageKind.ThreadAbort => "", 
			ExceptionMessageKind.ThreadInterrupted => "", 
			ExceptionMessageKind.OutOfMemory => "Out of memory", 
			_ => "", 
		};
	}

	internal void SetMessage(string s)
	{
		_message = s;
	}

	internal void SetStackTrace(string s)
	{
		_stackTraceString = s;
	}

	internal Exception FixRemotingException()
	{
		string remoteStackTraceString = string.Format((_remoteStackIndex == 0) ? Locale.GetText("{0}{0}Server stack trace: {0}{1}{0}{0}Exception rethrown at [{2}]: {0}") : Locale.GetText("{1}{0}{0}Exception rethrown at [{2}]: {0}"), Environment.NewLine, StackTrace, _remoteStackIndex);
		_remoteStackTraceString = remoteStackTraceString;
		_remoteStackIndex++;
		_stackTraceString = null;
		return this;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void ReportUnhandledException(Exception exception);
}
