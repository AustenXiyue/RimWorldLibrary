using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using Mono.Security.Cryptography;
using Mono.Xml;

namespace System.Security.Permissions;

/// <summary>Allows security actions for a <see cref="T:System.Security.PermissionSet" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
[ComVisible(true)]
public sealed class PermissionSetAttribute : CodeAccessSecurityAttribute
{
	private string file;

	private string name;

	private bool isUnicodeEncoded;

	private string xml;

	private string hex;

	/// <summary>Gets or sets a file containing the XML representation of a custom permission set to be declared.</summary>
	/// <returns>The physical path to the file containing the XML representation of the permission set.</returns>
	public string File
	{
		get
		{
			return file;
		}
		set
		{
			file = value;
		}
	}

	/// <summary>Gets or sets the hexadecimal representation of the XML encoded permission set.</summary>
	/// <returns>The hexadecimal representation of the XML encoded permission set.</returns>
	public string Hex
	{
		get
		{
			return hex;
		}
		set
		{
			hex = value;
		}
	}

	/// <summary>Gets or sets the name of the permission set.</summary>
	/// <returns>The name of an immutable <see cref="T:System.Security.NamedPermissionSet" /> (one of several permission sets that are contained in the default policy and cannot be altered).</returns>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the file specified by <see cref="P:System.Security.Permissions.PermissionSetAttribute.File" /> is Unicode or ASCII encoded.</summary>
	/// <returns>true if the file is Unicode encoded; otherwise, false.</returns>
	public bool UnicodeEncoded
	{
		get
		{
			return isUnicodeEncoded;
		}
		set
		{
			isUnicodeEncoded = value;
		}
	}

	/// <summary>Gets or sets the XML representation of a permission set.</summary>
	/// <returns>The XML representation of a permission set.</returns>
	public string XML
	{
		get
		{
			return xml;
		}
		set
		{
			xml = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.PermissionSetAttribute" /> class with the specified security action.</summary>
	/// <param name="action">One of the enumeration values that specifies a security action. </param>
	public PermissionSetAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>This method is not used.</summary>
	/// <returns>A null reference (nothing in Visual Basic) in all cases.</returns>
	public override IPermission CreatePermission()
	{
		return null;
	}

	private PermissionSet CreateFromXml(string xml)
	{
		SecurityParser securityParser = new SecurityParser();
		try
		{
			securityParser.LoadXml(xml);
		}
		catch (SmallXmlParserException ex)
		{
			throw new XmlSyntaxException(ex.Line, ex.ToString());
		}
		SecurityElement securityElement = securityParser.ToXml();
		string text = securityElement.Attribute("class");
		if (text == null)
		{
			return null;
		}
		PermissionState state = PermissionState.None;
		if (CodeAccessPermission.IsUnrestricted(securityElement))
		{
			state = PermissionState.Unrestricted;
		}
		if (text.EndsWith("NamedPermissionSet"))
		{
			NamedPermissionSet namedPermissionSet = new NamedPermissionSet(securityElement.Attribute("Name"), state);
			namedPermissionSet.FromXml(securityElement);
			return namedPermissionSet;
		}
		if (text.EndsWith("PermissionSet"))
		{
			PermissionSet permissionSet = new PermissionSet(state);
			permissionSet.FromXml(securityElement);
			return permissionSet;
		}
		return null;
	}

	/// <summary>Creates and returns a new permission set based on this permission set attribute object.</summary>
	/// <returns>A new permission set.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public PermissionSet CreatePermissionSet()
	{
		PermissionSet permissionSet = null;
		if (base.Unrestricted)
		{
			permissionSet = new PermissionSet(PermissionState.Unrestricted);
		}
		else
		{
			permissionSet = new PermissionSet(PermissionState.None);
			if (name != null)
			{
				return PolicyLevel.CreateAppDomainLevel().GetNamedPermissionSet(name);
			}
			if (file != null)
			{
				Encoding encoding = (isUnicodeEncoded ? Encoding.Unicode : Encoding.ASCII);
				using StreamReader streamReader = new StreamReader(file, encoding);
				permissionSet = CreateFromXml(streamReader.ReadToEnd());
			}
			else if (xml != null)
			{
				permissionSet = CreateFromXml(xml);
			}
			else if (hex != null)
			{
				Encoding aSCII = Encoding.ASCII;
				byte[] array = CryptoConvert.FromHex(hex);
				permissionSet = CreateFromXml(aSCII.GetString(array, 0, array.Length));
			}
		}
		return permissionSet;
	}
}
