using System;

namespace UnityEngine.UIElements;

[Serializable]
internal class StyleRule
{
	[SerializeField]
	private StyleProperty[] m_Properties;

	[SerializeField]
	internal int line;

	[NonSerialized]
	internal int customPropertiesCount;

	public StyleProperty[] properties
	{
		get
		{
			return m_Properties;
		}
		internal set
		{
			m_Properties = value;
		}
	}
}
