using Unity;

namespace System.Configuration;

/// <summary>Represents an element in a <see cref="T:System.Configuration.SchemeSettingElementCollection" /> class.</summary>
public sealed class SchemeSettingElement : ConfigurationElement
{
	/// <summary>Gets the value of the GenericUriParserOptions entry from a <see cref="T:System.Configuration.SchemeSettingElement" /> instance.</summary>
	/// <returns>The value of GenericUriParserOptions entry.</returns>
	public GenericUriParserOptions GenericUriParserOptions
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(GenericUriParserOptions);
		}
	}

	/// <summary>Gets the value of the Name entry from a <see cref="T:System.Configuration.SchemeSettingElement" /> instance.</summary>
	/// <returns>The protocol used by this schema setting. </returns>
	public string Name
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SchemeSettingElement" /> class.</summary>
	public SchemeSettingElement()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
