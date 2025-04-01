using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents information about an operating system, such as the version and platform identifier. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class OperatingSystem : ICloneable, ISerializable
{
	private PlatformID _platform;

	private Version _version;

	private string _servicePack = string.Empty;

	/// <summary>Gets a <see cref="T:System.PlatformID" /> enumeration value that identifies the operating system platform.</summary>
	/// <returns>One of the <see cref="T:System.PlatformID" /> values.</returns>
	/// <filterpriority>2</filterpriority>
	public PlatformID Platform => _platform;

	/// <summary>Gets a <see cref="T:System.Version" /> object that identifies the operating system.</summary>
	/// <returns>A <see cref="T:System.Version" /> object that describes the major version, minor version, build, and revision numbers for the operating system.</returns>
	/// <filterpriority>2</filterpriority>
	public Version Version => _version;

	/// <summary>Gets the service pack version represented by this <see cref="T:System.OperatingSystem" /> object.</summary>
	/// <returns>The service pack version, if service packs are supported and at least one is installed; otherwise, an empty string (""). </returns>
	/// <filterpriority>2</filterpriority>
	public string ServicePack => _servicePack;

	/// <summary>Gets the concatenated string representation of the platform identifier, version, and service pack that are currently installed on the operating system. </summary>
	/// <returns>The string representation of the values returned by the <see cref="P:System.OperatingSystem.Platform" />, <see cref="P:System.OperatingSystem.Version" />, and <see cref="P:System.OperatingSystem.ServicePack" /> properties.</returns>
	/// <filterpriority>2</filterpriority>
	public string VersionString => ToString();

	/// <summary>Initializes a new instance of the <see cref="T:System.OperatingSystem" /> class, using the specified platform identifier value and version object.</summary>
	/// <param name="platform">One of the <see cref="T:System.PlatformID" /> values that indicates the operating system platform. </param>
	/// <param name="version">A <see cref="T:System.Version" /> object that indicates the version of the operating system. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="version" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="platform" /> is not a <see cref="T:System.PlatformID" /> enumeration value.</exception>
	public OperatingSystem(PlatformID platform, Version version)
	{
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		_platform = platform;
		_version = version;
		if (platform == PlatformID.Win32NT && version.Revision != 0)
		{
			_servicePack = "Service Pack " + (version.Revision >> 16);
		}
	}

	private OperatingSystem(SerializationInfo information, StreamingContext context)
	{
		_platform = (PlatformID)information.GetValue("_platform", typeof(PlatformID));
		_version = (Version)information.GetValue("_version", typeof(Version));
		_servicePack = information.GetString("_servicePack");
	}

	/// <summary>Creates an <see cref="T:System.OperatingSystem" /> object that is identical to this instance.</summary>
	/// <returns>An <see cref="T:System.OperatingSystem" /> object that is a copy of this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new OperatingSystem(_platform, _version);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data necessary to deserialize this instance.</summary>
	/// <param name="info">The object to populate with serialization information.</param>
	/// <param name="context">The place to store and retrieve serialized data. Reserved for future use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("_platform", _platform);
		info.AddValue("_version", _version);
		info.AddValue("_servicePack", _servicePack);
	}

	/// <summary>Converts the value of this <see cref="T:System.OperatingSystem" /> object to its equivalent string representation.</summary>
	/// <returns>The string representation of the values returned by the <see cref="P:System.OperatingSystem.Platform" />, <see cref="P:System.OperatingSystem.Version" />, and <see cref="P:System.OperatingSystem.ServicePack" /> properties.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		string text;
		switch ((int)_platform)
		{
		case 2:
			text = "Microsoft Windows NT";
			break;
		case 0:
			text = "Microsoft Win32S";
			break;
		case 1:
			text = "Microsoft Windows 98";
			break;
		case 3:
			text = "Microsoft Windows CE";
			break;
		case 4:
		case 128:
			text = "Unix";
			break;
		case 5:
			text = "XBox";
			break;
		case 6:
			text = "OSX";
			break;
		default:
			text = Locale.GetText("<unknown>");
			break;
		}
		string text2 = "";
		if (ServicePack != string.Empty)
		{
			text2 = " " + ServicePack;
		}
		return text + " " + _version.ToString() + text2;
	}
}
