using System.Runtime.InteropServices;

namespace System.Runtime.Serialization;

/// <summary>Holds the value, <see cref="T:System.Type" />, and name of a serialized object. </summary>
[ComVisible(true)]
public struct SerializationEntry
{
	private Type m_type;

	private object m_value;

	private string m_name;

	/// <summary>Gets the value contained in the object.</summary>
	/// <returns>The value contained in the object.</returns>
	public object Value => m_value;

	/// <summary>Gets the name of the object.</summary>
	/// <returns>The name of the object.</returns>
	public string Name => m_name;

	/// <summary>Gets the <see cref="T:System.Type" /> of the object.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the object.</returns>
	public Type ObjectType => m_type;

	internal SerializationEntry(string entryName, object entryValue, Type entryType)
	{
		m_value = entryValue;
		m_name = entryName;
		m_type = entryType;
	}
}
