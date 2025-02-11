using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Xml;

namespace MS.Internal.Annotations;

internal sealed class XmlElementCollection : ObservableCollection<XmlElement>
{
	private Dictionary<XmlDocument, int> _xmlDocsRefCounts;

	public XmlElementCollection()
	{
		_xmlDocsRefCounts = new Dictionary<XmlDocument, int>();
	}

	protected override void ClearItems()
	{
		using (IEnumerator<XmlElement> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XmlElement current = enumerator.Current;
				UnregisterForElement(current);
			}
		}
		base.ClearItems();
	}

	protected override void RemoveItem(int index)
	{
		XmlElement element = base[index];
		UnregisterForElement(element);
		base.RemoveItem(index);
	}

	protected override void InsertItem(int index, XmlElement item)
	{
		if (item != null && Contains(item))
		{
			throw new ArgumentException(SR.Format(SR.XmlNodeAlreadyOwned, "change", "change"), "item");
		}
		base.InsertItem(index, item);
		RegisterForElement(item);
	}

	protected override void SetItem(int index, XmlElement item)
	{
		if (item != null && Contains(item))
		{
			throw new ArgumentException(SR.Format(SR.XmlNodeAlreadyOwned, "change", "change"), "item");
		}
		XmlElement element = base[index];
		UnregisterForElement(element);
		base.Items[index] = item;
		OnCollectionReset();
		RegisterForElement(item);
	}

	private void UnregisterForElement(XmlElement element)
	{
		if (element != null)
		{
			Invariant.Assert(_xmlDocsRefCounts.ContainsKey(element.OwnerDocument), "Not registered on XmlElement");
			_xmlDocsRefCounts[element.OwnerDocument]--;
			if (_xmlDocsRefCounts[element.OwnerDocument] == 0)
			{
				element.OwnerDocument.NodeChanged -= OnNodeChanged;
				element.OwnerDocument.NodeInserted -= OnNodeChanged;
				element.OwnerDocument.NodeRemoved -= OnNodeChanged;
				_xmlDocsRefCounts.Remove(element.OwnerDocument);
			}
		}
	}

	private void RegisterForElement(XmlElement element)
	{
		if (element != null)
		{
			if (!_xmlDocsRefCounts.ContainsKey(element.OwnerDocument))
			{
				_xmlDocsRefCounts[element.OwnerDocument] = 1;
				XmlNodeChangedEventHandler value = OnNodeChanged;
				element.OwnerDocument.NodeChanged += value;
				element.OwnerDocument.NodeInserted += value;
				element.OwnerDocument.NodeRemoved += value;
			}
			else
			{
				_xmlDocsRefCounts[element.OwnerDocument]++;
			}
		}
	}

	private void OnNodeChanged(object sender, XmlNodeChangedEventArgs args)
	{
		XmlAttribute xmlAttribute = null;
		XmlElement xmlElement = null;
		Invariant.Assert(_xmlDocsRefCounts.ContainsKey(sender as XmlDocument), "Not expecting a notification from this sender");
		for (XmlNode xmlNode = args.Node; xmlNode != null; xmlNode = ((!(xmlNode is XmlAttribute xmlAttribute2)) ? xmlNode.ParentNode : xmlAttribute2.OwnerElement))
		{
			if (xmlNode is XmlElement item && Contains(item))
			{
				OnCollectionReset();
				break;
			}
		}
	}

	private void OnCollectionReset()
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}
}
