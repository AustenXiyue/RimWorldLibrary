using System;
using System.Windows;
using System.Xml;

namespace MS.Internal.Data;

internal class XmlNodeChangedEventManager : WeakEventManager
{
	private static XmlNodeChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(XmlNodeChangedEventManager);
			XmlNodeChangedEventManager xmlNodeChangedEventManager = (XmlNodeChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (xmlNodeChangedEventManager == null)
			{
				xmlNodeChangedEventManager = new XmlNodeChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, xmlNodeChangedEventManager);
			}
			return xmlNodeChangedEventManager;
		}
	}

	private XmlNodeChangedEventManager()
	{
	}

	public static void AddListener(XmlDocument source, IWeakEventListener listener)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.ProtectedAddListener(source, listener);
	}

	public static void RemoveListener(XmlDocument source, IWeakEventListener listener)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.ProtectedRemoveListener(source, listener);
	}

	public static void AddHandler(XmlDocument source, EventHandler<XmlNodeChangedEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedAddHandler(source, handler);
	}

	public static void RemoveHandler(XmlDocument source, EventHandler<XmlNodeChangedEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.ProtectedRemoveHandler(source, handler);
	}

	protected override ListenerList NewListenerList()
	{
		return new ListenerList<XmlNodeChangedEventArgs>();
	}

	protected override void StartListening(object source)
	{
		XmlNodeChangedEventHandler value = OnXmlNodeChanged;
		XmlDocument obj = (XmlDocument)source;
		obj.NodeInserted += value;
		obj.NodeRemoved += value;
		obj.NodeChanged += value;
	}

	protected override void StopListening(object source)
	{
		XmlNodeChangedEventHandler value = OnXmlNodeChanged;
		XmlDocument obj = (XmlDocument)source;
		obj.NodeInserted -= value;
		obj.NodeRemoved -= value;
		obj.NodeChanged -= value;
	}

	private void OnXmlNodeChanged(object sender, XmlNodeChangedEventArgs args)
	{
		DeliverEvent(sender, args);
	}
}
