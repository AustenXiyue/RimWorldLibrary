using System.Runtime.InteropServices;

namespace System;

/// <summary>Specifies the usage of another attribute class. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
[ComVisible(true)]
public sealed class AttributeUsageAttribute : Attribute
{
	internal AttributeTargets m_attributeTarget = AttributeTargets.All;

	internal bool m_allowMultiple;

	internal bool m_inherited = true;

	internal static AttributeUsageAttribute Default = new AttributeUsageAttribute(AttributeTargets.All);

	/// <summary>Gets a set of values identifying which program elements that the indicated attribute can be applied to.</summary>
	/// <returns>One or several <see cref="T:System.AttributeTargets" /> values. The default is All.</returns>
	/// <filterpriority>2</filterpriority>
	public AttributeTargets ValidOn => m_attributeTarget;

	/// <summary>Gets or sets a Boolean value indicating whether more than one instance of the indicated attribute can be specified for a single program element.</summary>
	/// <returns>true if more than one instance is allowed to be specified; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool AllowMultiple
	{
		get
		{
			return m_allowMultiple;
		}
		set
		{
			m_allowMultiple = value;
		}
	}

	/// <summary>Gets or sets a Boolean value indicating whether the indicated attribute can be inherited by derived classes and overriding members.</summary>
	/// <returns>true if the attribute can be inherited by derived classes and overriding members; otherwise, false. The default is true.</returns>
	/// <filterpriority>2</filterpriority>
	public bool Inherited
	{
		get
		{
			return m_inherited;
		}
		set
		{
			m_inherited = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.AttributeUsageAttribute" /> class with the specified list of <see cref="T:System.AttributeTargets" />, the <see cref="P:System.AttributeUsageAttribute.AllowMultiple" /> value, and the <see cref="P:System.AttributeUsageAttribute.Inherited" /> value.</summary>
	/// <param name="validOn">The set of values combined using a bitwise OR operation to indicate which program elements are valid. </param>
	public AttributeUsageAttribute(AttributeTargets validOn)
	{
		m_attributeTarget = validOn;
	}

	internal AttributeUsageAttribute(AttributeTargets validOn, bool allowMultiple, bool inherited)
	{
		m_attributeTarget = validOn;
		m_allowMultiple = allowMultiple;
		m_inherited = inherited;
	}
}
