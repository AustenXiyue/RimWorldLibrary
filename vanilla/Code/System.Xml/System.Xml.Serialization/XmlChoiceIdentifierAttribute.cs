using System.Reflection;

namespace System.Xml.Serialization;

/// <summary>Specifies that the member can be further detected by using an enumeration.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false)]
public class XmlChoiceIdentifierAttribute : Attribute
{
	private string name;

	private MemberInfo memberInfo;

	/// <summary>Gets or sets the name of the field that returns the enumeration to use when detecting types.</summary>
	/// <returns>The name of a field that returns an enumeration.</returns>
	public string MemberName
	{
		get
		{
			if (name != null)
			{
				return name;
			}
			return string.Empty;
		}
		set
		{
			name = value;
		}
	}

	internal MemberInfo MemberInfo
	{
		get
		{
			return memberInfo;
		}
		set
		{
			memberInfo = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlChoiceIdentifierAttribute" /> class.</summary>
	public XmlChoiceIdentifierAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlChoiceIdentifierAttribute" /> class.</summary>
	/// <param name="name">The member name that returns the enumeration used to detect a choice. </param>
	public XmlChoiceIdentifierAttribute(string name)
	{
		this.name = name;
	}
}
