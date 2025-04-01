using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Instructs obfuscation tools to take the specified actions for an assembly, type, or member.</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
[ComVisible(true)]
public sealed class ObfuscationAttribute : Attribute
{
	private bool m_strip = true;

	private bool m_exclude = true;

	private bool m_applyToMembers = true;

	private string m_feature = "all";

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value indicating whether the obfuscation tool should remove this attribute after processing.</summary>
	/// <returns>true if an obfuscation tool should remove the attribute after processing; otherwise, false. The default is true.</returns>
	public bool StripAfterObfuscation
	{
		get
		{
			return m_strip;
		}
		set
		{
			m_strip = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value indicating whether the obfuscation tool should exclude the type or member from obfuscation.</summary>
	/// <returns>true if the type or member to which this attribute is applied should be excluded from obfuscation; otherwise, false. The default is true.</returns>
	public bool Exclude
	{
		get
		{
			return m_exclude;
		}
		set
		{
			m_exclude = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value indicating whether the attribute of a type is to apply to the members of the type.</summary>
	/// <returns>true if the attribute is to apply to the members of the type; otherwise, false. The default is true.</returns>
	public bool ApplyToMembers
	{
		get
		{
			return m_applyToMembers;
		}
		set
		{
			m_applyToMembers = value;
		}
	}

	/// <summary>Gets or sets a string value that is recognized by the obfuscation tool, and which specifies processing options. </summary>
	/// <returns>A string value that is recognized by the obfuscation tool, and which specifies processing options. The default is "all".</returns>
	public string Feature
	{
		get
		{
			return m_feature;
		}
		set
		{
			m_feature = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.ObfuscationAttribute" /> class.</summary>
	public ObfuscationAttribute()
	{
	}
}
