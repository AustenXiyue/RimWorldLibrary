using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Xml;

namespace MS.Internal.Data;

internal class XmlDataCollection : ReadOnlyObservableCollection<XmlNode>
{
	private XmlDataProvider _xds;

	private XmlNamespaceManager _nsMgr;

	internal XmlNamespaceManager XmlNamespaceManager
	{
		get
		{
			if (_nsMgr == null && _xds != null)
			{
				_nsMgr = _xds.XmlNamespaceManager;
			}
			return _nsMgr;
		}
		set
		{
			_nsMgr = value;
		}
	}

	internal XmlDataProvider ParentXmlDataProvider => _xds;

	internal XmlDataCollection(XmlDataProvider xmlDataProvider)
		: base(new ObservableCollection<XmlNode>())
	{
		_xds = xmlDataProvider;
	}

	internal bool CollectionHasChanged(XmlNodeList nodes)
	{
		int count = base.Count;
		if (count != nodes.Count)
		{
			return true;
		}
		for (int i = 0; i < count; i++)
		{
			if (base[i] != nodes[i])
			{
				return true;
			}
		}
		return false;
	}

	internal void SynchronizeCollection(XmlNodeList nodes)
	{
		if (nodes == null)
		{
			base.Items.Clear();
			return;
		}
		int i = 0;
		while (i < base.Count && i < nodes.Count)
		{
			if (base[i] != nodes[i])
			{
				int j;
				for (j = i + 1; j < nodes.Count && base[i] != nodes[j]; j++)
				{
				}
				if (j < nodes.Count)
				{
					for (; i < j; i++)
					{
						base.Items.Insert(i, nodes[i]);
					}
					i++;
				}
				else
				{
					base.Items.RemoveAt(i);
				}
			}
			else
			{
				i++;
			}
		}
		while (i < base.Count)
		{
			base.Items.RemoveAt(i);
		}
		for (; i < nodes.Count; i++)
		{
			base.Items.Insert(i, nodes[i]);
		}
	}
}
