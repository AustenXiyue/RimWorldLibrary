using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;
using System.Xml.XPath;

namespace MS.Internal.Data;

internal class XmlBindingWorker : BindingWorker, IWeakEventListener
{
	private enum XPathType : byte
	{
		Default,
		SimpleName,
		SimpleAttribute
	}

	private bool _collectionMode;

	private XPathType _xpathType;

	private XmlNode _contextNode;

	private XmlDataCollection _queriedCollection;

	private ICollectionView _collectionView;

	private XmlDataProvider _xmlDataProvider;

	private ClrBindingWorker _hostWorker;

	private string _xpath;

	private XmlDataCollection QueriedCollection
	{
		get
		{
			return _queriedCollection;
		}
		set
		{
			_queriedCollection = value;
		}
	}

	private ICollectionView CollectionView
	{
		get
		{
			return _collectionView;
		}
		set
		{
			_collectionView = value;
		}
	}

	private XmlNode ContextNode
	{
		get
		{
			return _contextNode;
		}
		set
		{
			if (_contextNode != value && TraceData.IsExtendedTraceEnabled(base.ParentBindingExpression, TraceDataLevel.Transfer))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.XmlContextNode(TraceData.Identify(base.ParentBindingExpression), IdentifyNode(value)), base.ParentBindingExpression);
			}
			_contextNode = value;
		}
	}

	private string XPath => _xpath;

	private XmlNamespaceManager NamespaceManager
	{
		get
		{
			DependencyObject targetElement = base.TargetElement;
			if (targetElement == null)
			{
				return null;
			}
			XmlNamespaceManager xmlNamespaceManager = Binding.GetXmlNamespaceManager(targetElement);
			if (xmlNamespaceManager == null && XmlDataProvider != null)
			{
				xmlNamespaceManager = XmlDataProvider.XmlNamespaceManager;
			}
			return xmlNamespaceManager;
		}
	}

	private XmlDataProvider XmlDataProvider
	{
		get
		{
			if (_xmlDataProvider == null && (_xmlDataProvider = base.ParentBindingExpression.DataSource as XmlDataProvider) == null)
			{
				if (base.DataItem is XmlDataCollection xmlDataCollection)
				{
					_xmlDataProvider = xmlDataCollection.ParentXmlDataProvider;
				}
				else if (CollectionView != null && CollectionView.SourceCollection is XmlDataCollection xmlDataCollection2)
				{
					_xmlDataProvider = xmlDataCollection2.ParentXmlDataProvider;
				}
				else if (base.TargetProperty == BindingExpressionBase.NoTargetProperty && base.TargetElement is ItemsControl itemsControl)
				{
					object itemsSource = itemsControl.ItemsSource;
					XmlDataCollection xmlDataCollection3;
					if ((xmlDataCollection3 = itemsSource as XmlDataCollection) == null)
					{
						xmlDataCollection3 = ((itemsSource is ICollectionView collectionView) ? collectionView.SourceCollection : null) as XmlDataCollection;
					}
					if (xmlDataCollection3 != null)
					{
						_xmlDataProvider = xmlDataCollection3.ParentXmlDataProvider;
					}
				}
				else
				{
					_xmlDataProvider = Helper.XmlDataProviderForElement(base.TargetElement);
				}
			}
			return _xmlDataProvider;
		}
	}

	internal XmlBindingWorker(ClrBindingWorker worker, bool collectionMode)
		: base(worker.ParentBindingExpression)
	{
		_hostWorker = worker;
		_xpath = base.ParentBinding.XPath;
		_collectionMode = collectionMode;
		_xpathType = GetXPathType(_xpath);
	}

	internal override void AttachDataItem()
	{
		if (XPath.Length > 0)
		{
			CollectionView = base.DataItem as CollectionView;
			if (CollectionView == null && base.DataItem is ICollection)
			{
				CollectionView = CollectionViewSource.GetDefaultCollectionView(base.DataItem, base.TargetElement);
			}
		}
		if (CollectionView != null)
		{
			CurrentChangedEventManager.AddHandler(CollectionView, base.ParentBindingExpression.OnCurrentChanged);
			if (base.IsReflective)
			{
				CurrentChangingEventManager.AddHandler(CollectionView, base.ParentBindingExpression.OnCurrentChanging);
			}
		}
		UpdateContextNode(hookNotifications: true);
	}

	internal override void DetachDataItem()
	{
		if (CollectionView != null)
		{
			CurrentChangedEventManager.RemoveHandler(CollectionView, base.ParentBindingExpression.OnCurrentChanged);
			if (base.IsReflective)
			{
				CurrentChangingEventManager.RemoveHandler(CollectionView, base.ParentBindingExpression.OnCurrentChanging);
			}
		}
		UpdateContextNode(hookNotifications: false);
		CollectionView = null;
	}

	internal override void OnCurrentChanged(ICollectionView collectionView, EventArgs args)
	{
		if (collectionView == CollectionView)
		{
			using (base.ParentBindingExpression.ChangingValue())
			{
				UpdateContextNode(hookNotifications: true);
				_hostWorker.UseNewXmlItem(RawValue());
			}
		}
	}

	internal override object RawValue()
	{
		if (XPath.Length == 0)
		{
			return base.DataItem;
		}
		if (ContextNode == null)
		{
			QueriedCollection = null;
			return null;
		}
		XmlNodeList xmlNodeList = SelectNodes();
		if (xmlNodeList == null)
		{
			QueriedCollection = null;
			return DependencyProperty.UnsetValue;
		}
		return BuildQueriedCollection(xmlNodeList);
	}

	internal void ReportBadXPath(TraceEventType traceType)
	{
		if (TraceData.IsEnabled)
		{
			TraceData.TraceAndNotifyWithNoParameters(traceType, TraceData.BadXPath(XPath, IdentifyNode(ContextNode)), base.ParentBindingExpression);
		}
	}

	private void UpdateContextNode(bool hookNotifications)
	{
		UnHookNotifications();
		if (base.DataItem == BindingExpressionBase.DisconnectedItem)
		{
			ContextNode = null;
			return;
		}
		if (CollectionView != null)
		{
			ContextNode = CollectionView.CurrentItem as XmlNode;
			if (ContextNode != CollectionView.CurrentItem && TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.XmlBindingToNonXmlCollection, base.ParentBindingExpression, new object[3] { XPath, base.ParentBindingExpression, base.DataItem });
			}
		}
		else
		{
			ContextNode = base.DataItem as XmlNode;
			if (ContextNode != base.DataItem && TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.XmlBindingToNonXml, base.ParentBindingExpression, new object[3] { XPath, base.ParentBindingExpression, base.DataItem });
			}
		}
		if (hookNotifications)
		{
			HookNotifications();
		}
	}

	private void HookNotifications()
	{
		if (base.IsDynamic && ContextNode != null)
		{
			XmlDocument xmlDocument = DocumentFor(ContextNode);
			if (xmlDocument != null)
			{
				XmlNodeChangedEventManager.AddHandler(xmlDocument, OnXmlNodeChanged);
			}
		}
	}

	private void UnHookNotifications()
	{
		if (base.IsDynamic && ContextNode != null)
		{
			XmlDocument xmlDocument = DocumentFor(ContextNode);
			if (xmlDocument != null)
			{
				XmlNodeChangedEventManager.RemoveHandler(xmlDocument, OnXmlNodeChanged);
			}
		}
	}

	private XmlDocument DocumentFor(XmlNode node)
	{
		XmlDocument xmlDocument = node.OwnerDocument;
		if (xmlDocument == null)
		{
			xmlDocument = node as XmlDocument;
		}
		return xmlDocument;
	}

	private XmlDataCollection BuildQueriedCollection(XmlNodeList nodes)
	{
		if (TraceData.IsExtendedTraceEnabled(base.ParentBindingExpression, TraceDataLevel.CreateExpression))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.XmlNewCollection(TraceData.Identify(base.ParentBindingExpression), IdentifyNodeList(nodes)), base.ParentBindingExpression);
		}
		QueriedCollection = new XmlDataCollection(XmlDataProvider);
		QueriedCollection.XmlNamespaceManager = NamespaceManager;
		QueriedCollection.SynchronizeCollection(nodes);
		return QueriedCollection;
	}

	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs args)
	{
		return false;
	}

	private void OnXmlNodeChanged(object sender, XmlNodeChangedEventArgs e)
	{
		if (TraceData.IsExtendedTraceEnabled(base.ParentBindingExpression, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(base.ParentBindingExpression), "XmlNodeChanged", TraceData.Identify(sender)), base.ParentBindingExpression);
		}
		ProcessXmlNodeChanged(e);
	}

	private void ProcessXmlNodeChanged(EventArgs args)
	{
		DependencyObject targetElement = base.ParentBindingExpression.TargetElement;
		if (targetElement == null || base.IgnoreSourcePropertyChange || base.DataItem == BindingExpressionBase.DisconnectedItem || !IsChangeRelevant(args))
		{
			return;
		}
		if (XPath.Length == 0)
		{
			_hostWorker.OnXmlValueChanged();
		}
		else if (QueriedCollection == null)
		{
			_hostWorker.UseNewXmlItem(RawValue());
		}
		else
		{
			XmlNodeList xmlNodeList = SelectNodes();
			if (xmlNodeList == null)
			{
				QueriedCollection = null;
				_hostWorker.UseNewXmlItem(DependencyProperty.UnsetValue);
			}
			else if (_collectionMode)
			{
				if (TraceData.IsExtendedTraceEnabled(base.ParentBindingExpression, TraceDataLevel.CreateExpression))
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.XmlSynchronizeCollection(TraceData.Identify(base.ParentBindingExpression), IdentifyNodeList(xmlNodeList)), base.ParentBindingExpression);
				}
				QueriedCollection.SynchronizeCollection(xmlNodeList);
			}
			else if (QueriedCollection.CollectionHasChanged(xmlNodeList))
			{
				_hostWorker.UseNewXmlItem(BuildQueriedCollection(xmlNodeList));
			}
			else
			{
				_hostWorker.OnXmlValueChanged();
			}
		}
		GC.KeepAlive(targetElement);
	}

	private XmlNodeList SelectNodes()
	{
		XmlNamespaceManager namespaceManager = NamespaceManager;
		XmlNodeList xmlNodeList = null;
		try
		{
			xmlNodeList = ((namespaceManager == null) ? ContextNode.SelectNodes(XPath) : ContextNode.SelectNodes(XPath, namespaceManager));
		}
		catch (XPathException ex)
		{
			base.Status = BindingStatusInternal.PathError;
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.CannotGetXmlNodeCollection, base.ParentBindingExpression, new object[4]
				{
					(ContextNode != null) ? ContextNode.Name : null,
					XPath,
					base.ParentBindingExpression,
					ex
				}, new object[1] { ex });
			}
		}
		if (TraceData.IsExtendedTraceEnabled(base.ParentBindingExpression, TraceDataLevel.CreateExpression))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.SelectNodes(TraceData.Identify(base.ParentBindingExpression), IdentifyNode(ContextNode), TraceData.Identify(XPath), IdentifyNodeList(xmlNodeList)), base.ParentBindingExpression);
		}
		return xmlNodeList;
	}

	private string IdentifyNode(XmlNode node)
	{
		if (node == null)
		{
			return "<null>";
		}
		return string.Format(TypeConverterHelper.InvariantEnglishUS, "{0} ({1})", node.GetType().Name, node.Name);
	}

	private string IdentifyNodeList(XmlNodeList nodeList)
	{
		if (nodeList == null)
		{
			return "<null>";
		}
		return string.Format(TypeConverterHelper.InvariantEnglishUS, "{0} (hash={1} Count={2})", nodeList.GetType().Name, AvTrace.GetHashCodeHelper(nodeList), nodeList.Count);
	}

	private static XPathType GetXPathType(string xpath)
	{
		int length = xpath.Length;
		if (length == 0)
		{
			return XPathType.SimpleName;
		}
		bool flag = xpath[0] == '@';
		int num = (flag ? 1 : 0);
		if (num >= length)
		{
			return XPathType.Default;
		}
		char c = xpath[num];
		if (!char.IsLetter(c) && c != '_' && c != ':')
		{
			return XPathType.Default;
		}
		for (num++; num < length; num++)
		{
			c = xpath[num];
			if (!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_' && c != ':')
			{
				return XPathType.Default;
			}
		}
		if (!flag)
		{
			return XPathType.SimpleName;
		}
		return XPathType.SimpleAttribute;
	}

	private bool IsChangeRelevant(EventArgs rawArgs)
	{
		if (_xpathType == XPathType.Default)
		{
			return true;
		}
		XmlNodeChangedEventArgs xmlNodeChangedEventArgs = (XmlNodeChangedEventArgs)rawArgs;
		XmlNode xmlNode = null;
		XmlNode xmlNode2 = null;
		switch (xmlNodeChangedEventArgs.Action)
		{
		case XmlNodeChangedAction.Insert:
			xmlNode = xmlNodeChangedEventArgs.NewParent;
			break;
		case XmlNodeChangedAction.Remove:
			xmlNode = xmlNodeChangedEventArgs.OldParent;
			break;
		case XmlNodeChangedAction.Change:
			xmlNode2 = xmlNodeChangedEventArgs.Node;
			break;
		}
		if (_collectionMode)
		{
			return xmlNode == ContextNode;
		}
		if (xmlNode == ContextNode)
		{
			return true;
		}
		if (!(_hostWorker.GetResultNode() is XmlNode xmlNode3))
		{
			return false;
		}
		if (xmlNode2 != null)
		{
			xmlNode = xmlNode2;
		}
		while (xmlNode != null)
		{
			if (xmlNode == xmlNode3)
			{
				return true;
			}
			xmlNode = xmlNode.ParentNode;
		}
		return false;
	}
}
