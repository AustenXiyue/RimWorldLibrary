using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

[Serializable]
internal class VisualElementAsset : IUxmlAttributes, ISerializationCallbackReceiver
{
	[SerializeField]
	private string m_Name;

	[SerializeField]
	private int m_Id;

	[SerializeField]
	private int m_OrderInDocument;

	[SerializeField]
	private int m_ParentId;

	[SerializeField]
	private int m_RuleIndex;

	[SerializeField]
	private string m_Text;

	[SerializeField]
	private PickingMode m_PickingMode;

	[SerializeField]
	private string m_FullTypeName;

	[SerializeField]
	private string[] m_Classes;

	[SerializeField]
	private List<string> m_StylesheetPaths;

	[SerializeField]
	private List<StyleSheet> m_Stylesheets;

	[SerializeField]
	private List<string> m_Properties;

	public int id
	{
		get
		{
			return m_Id;
		}
		set
		{
			m_Id = value;
		}
	}

	public int orderInDocument
	{
		get
		{
			return m_OrderInDocument;
		}
		set
		{
			m_OrderInDocument = value;
		}
	}

	public int parentId
	{
		get
		{
			return m_ParentId;
		}
		set
		{
			m_ParentId = value;
		}
	}

	public int ruleIndex
	{
		get
		{
			return m_RuleIndex;
		}
		set
		{
			m_RuleIndex = value;
		}
	}

	public string fullTypeName
	{
		get
		{
			return m_FullTypeName;
		}
		set
		{
			m_FullTypeName = value;
		}
	}

	public string[] classes
	{
		get
		{
			return m_Classes;
		}
		set
		{
			m_Classes = value;
		}
	}

	public List<string> stylesheetPaths
	{
		get
		{
			return (m_StylesheetPaths == null) ? (m_StylesheetPaths = new List<string>()) : m_StylesheetPaths;
		}
		set
		{
			m_StylesheetPaths = value;
		}
	}

	public List<StyleSheet> stylesheets
	{
		get
		{
			return (m_Stylesheets == null) ? (m_Stylesheets = new List<StyleSheet>()) : m_Stylesheets;
		}
		set
		{
			m_Stylesheets = value;
		}
	}

	public VisualElementAsset(string fullTypeName)
	{
		m_FullTypeName = fullTypeName;
		m_Name = string.Empty;
		m_Text = string.Empty;
		m_PickingMode = PickingMode.Position;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (!string.IsNullOrEmpty(m_Name) && !m_Properties.Contains("name"))
		{
			AddProperty("name", m_Name);
		}
		if (!string.IsNullOrEmpty(m_Text) && !m_Properties.Contains("text"))
		{
			AddProperty("text", m_Text);
		}
		if (m_PickingMode != 0 && !m_Properties.Contains("picking-mode") && !m_Properties.Contains("pickingMode"))
		{
			AddProperty("picking-mode", m_PickingMode.ToString());
		}
	}

	public void AddProperty(string propertyName, string propertyValue)
	{
		SetOrAddProperty(propertyName, propertyValue);
	}

	private void SetOrAddProperty(string propertyName, string propertyValue)
	{
		if (m_Properties == null)
		{
			m_Properties = new List<string>();
		}
		for (int i = 0; i < m_Properties.Count - 1; i += 2)
		{
			if (m_Properties[i] == propertyName)
			{
				m_Properties[i + 1] = propertyValue;
				return;
			}
		}
		m_Properties.Add(propertyName);
		m_Properties.Add(propertyValue);
	}

	public bool TryGetAttributeValue(string propertyName, out string value)
	{
		if (m_Properties == null)
		{
			value = null;
			return false;
		}
		for (int i = 0; i < m_Properties.Count - 1; i += 2)
		{
			if (m_Properties[i] == propertyName)
			{
				value = m_Properties[i + 1];
				return true;
			}
		}
		value = null;
		return false;
	}
}
