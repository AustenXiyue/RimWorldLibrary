using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;

namespace System.Configuration;

/// <summary>The exception that is thrown when a configuration system error has occurred.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public class ConfigurationException : SystemException
{
	private readonly string filename;

	private readonly int line;

	/// <summary>Gets a description of why this configuration exception was thrown.</summary>
	/// <returns>A description of why this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual string BareMessage => base.Message;

	/// <summary>Gets the path to the configuration file that caused this configuration exception to be thrown.</summary>
	/// <returns>The path to the configuration file that caused this <see cref="T:System.Configuration.ConfigurationException" /> exception to be thrown.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual string Filename => filename;

	/// <summary>Gets the line number within the configuration file at which this configuration exception was thrown.</summary>
	/// <returns>The line number within the configuration file at which this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual int Line => line;

	/// <summary>Gets an extended description of why this configuration exception was thrown.</summary>
	/// <returns>An extended description of why this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string Message
	{
		get
		{
			if (filename != null && filename.Length != 0)
			{
				if (line != 0)
				{
					return BareMessage + " (" + filename + " line " + line + ")";
				}
				return BareMessage + " (" + filename + ")";
			}
			if (line != 0)
			{
				return BareMessage + " (line " + line + ")";
			}
			return BareMessage;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationException" /> class. </summary>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public ConfigurationException()
		: this(null)
	{
		filename = null;
		line = 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationException" /> class. </summary>
	/// <param name="message">A message describing why this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</param>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public ConfigurationException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationException" /> class. </summary>
	/// <param name="info">The object that holds the information to deserialize.</param>
	/// <param name="context">Contextual information about the source or destination.</param>
	protected ConfigurationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		filename = info.GetString("filename");
		line = info.GetInt32("line");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationException" /> class. </summary>
	/// <param name="message">A message describing why this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</param>
	/// <param name="inner">The inner exception that caused this <see cref="T:System.Configuration.ConfigurationException" /> to be thrown, if any.</param>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public ConfigurationException(string message, Exception inner)
		: base(message, inner)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationException" /> class. </summary>
	/// <param name="message">A message describing why this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</param>
	/// <param name="node">The <see cref="T:System.Xml.XmlNode" /> that caused this <see cref="T:System.Configuration.ConfigurationException" /> to be thrown.</param>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public ConfigurationException(string message, XmlNode node)
		: base(message)
	{
		filename = GetXmlNodeFilename(node);
		line = GetXmlNodeLineNumber(node);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationException" /> class. </summary>
	/// <param name="message">A message describing why this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</param>
	/// <param name="inner">The inner exception that caused this <see cref="T:System.Configuration.ConfigurationException" /> to be thrown, if any.</param>
	/// <param name="node">The <see cref="T:System.Xml.XmlNode" /> that caused this <see cref="T:System.Configuration.ConfigurationException" /> to be thrown.</param>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public ConfigurationException(string message, Exception inner, XmlNode node)
		: base(message, inner)
	{
		filename = GetXmlNodeFilename(node);
		line = GetXmlNodeLineNumber(node);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationException" /> class. </summary>
	/// <param name="message">A message describing why this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</param>
	/// <param name="filename">The path to the configuration file that caused this <see cref="T:System.Configuration.ConfigurationException" /> to be thrown.</param>
	/// <param name="line">The line number within the configuration file at which this <see cref="T:System.Configuration.ConfigurationException" /> was thrown.</param>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public ConfigurationException(string message, string filename, int line)
		: base(message)
	{
		this.filename = filename;
		this.line = line;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationException" /> class. </summary>
	/// <param name="message">A message describing why this <see cref="T:System.Configuration.ConfigurationException" /> exception was thrown.</param>
	/// <param name="inner">The inner exception that caused this <see cref="T:System.Configuration.ConfigurationException" /> to be thrown, if any.</param>
	/// <param name="filename">The path to the configuration file that caused this <see cref="T:System.Configuration.ConfigurationException" /> to be thrown.</param>
	/// <param name="line">The line number within the configuration file at which this <see cref="T:System.Configuration.ConfigurationException" /> was thrown.</param>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public ConfigurationException(string message, Exception inner, string filename, int line)
		: base(message, inner)
	{
		this.filename = filename;
		this.line = line;
	}

	/// <summary>Gets the path to the configuration file from which the internal <see cref="T:System.Xml.XmlNode" /> object was loaded when this configuration exception was thrown.</summary>
	/// <returns>A string representing the node file name.</returns>
	/// <param name="node">The <see cref="T:System.Xml.XmlNode" /> that caused this <see cref="T:System.Configuration.ConfigurationException" /> exception to be thrown.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public static string GetXmlNodeFilename(XmlNode node)
	{
		if (!(node is System.Configuration.IConfigXmlNode))
		{
			return string.Empty;
		}
		return ((System.Configuration.IConfigXmlNode)node).Filename;
	}

	/// <summary>Gets the line number within the configuration file that the internal <see cref="T:System.Xml.XmlNode" /> object represented when this configuration exception was thrown.</summary>
	/// <returns>An int representing the node line number.</returns>
	/// <param name="node">The <see cref="T:System.Xml.XmlNode" /> that caused this <see cref="T:System.Configuration.ConfigurationException" /> exception to be thrown.</param>
	/// <filterpriority>1</filterpriority>
	[Obsolete("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
	public static int GetXmlNodeLineNumber(XmlNode node)
	{
		if (!(node is System.Configuration.IConfigXmlNode))
		{
			return 0;
		}
		return ((System.Configuration.IConfigXmlNode)node).LineNumber;
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the file name and line number at which this configuration exception occurred.</summary>
	/// <param name="info">The object that holds the information to be serialized.</param>
	/// <param name="context">The contextual information about the source or destination.</param>
	/// <filterpriority>2</filterpriority>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("filename", filename);
		info.AddValue("line", line);
	}
}
