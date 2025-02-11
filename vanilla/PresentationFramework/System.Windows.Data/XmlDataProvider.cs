using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Threading;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using MS.Internal;
using MS.Internal.Data;
using MS.Internal.Utility;

namespace System.Windows.Data;

/// <summary>Enables declarative access to XML data for data binding.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
[ContentProperty("XmlSerializer")]
public class XmlDataProvider : DataSourceProvider, IUriContext
{
	private class XmlIslandSerializer : IXmlSerializable
	{
		private XmlDataProvider _host;

		internal XmlIslandSerializer(XmlDataProvider host)
		{
			_host = host;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void WriteXml(XmlWriter writer)
		{
			_host.DocumentForSerialization?.Save(writer);
		}

		public void ReadXml(XmlReader reader)
		{
			_host.ParseInline(reader);
		}
	}

	private XmlDocument _document;

	private XmlDocument _domSetDocument;

	private XmlDocument _savedDocument;

	private ManualResetEvent _waitForInlineDoc;

	private XmlNamespaceManager _nsMgr;

	private Uri _source;

	private Uri _baseUri;

	private string _xPath = string.Empty;

	private bool _tryInlineDoc = true;

	private bool _isListening;

	private XmlIslandSerializer _xmlSerializer;

	private bool _isAsynchronous = true;

	private bool _inEndInit;

	private DispatcherOperationCallback _onCompletedCallback;

	private XmlNodeChangedEventHandler _nodeChangedHandler;

	/// <summary>Gets or sets the <see cref="T:System.Uri" /> of the XML data file to use as the binding source.</summary>
	/// <returns>The <see cref="T:System.Uri" /> of the XML data file to use as the binding source. The default value is null.</returns>
	public Uri Source
	{
		get
		{
			return _source;
		}
		set
		{
			if (_domSetDocument != null || _source != value)
			{
				_domSetDocument = null;
				_source = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Source"));
				if (!base.IsRefreshDeferred)
				{
					Refresh();
				}
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.XmlDocument" /> to use as the binding source.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlDocument" /> to use as the binding source. The default value is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public XmlDocument Document
	{
		get
		{
			return _document;
		}
		set
		{
			if (_domSetDocument == null || _source != null || _document != value)
			{
				_domSetDocument = value;
				_source = null;
				OnPropertyChanged(new PropertyChangedEventArgs("Source"));
				ChangeDocument(value);
				if (!base.IsRefreshDeferred)
				{
					Refresh();
				}
			}
		}
	}

	/// <summary>Gets or sets the XPath query used to generate the data collection.</summary>
	/// <returns>The XPath query used to generate the data collection. The default is an empty string.</returns>
	[DesignerSerializationOptions(DesignerSerializationOptions.SerializeAsAttribute)]
	public string XPath
	{
		get
		{
			return _xPath;
		}
		set
		{
			if (_xPath != value)
			{
				_xPath = value;
				OnPropertyChanged(new PropertyChangedEventArgs("XPath"));
				if (!base.IsRefreshDeferred)
				{
					Refresh();
				}
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.XmlNamespaceManager" /> used to run <see cref="P:System.Windows.Data.XmlDataProvider.XPath" /> queries.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNamespaceManager" /> used to run <see cref="P:System.Windows.Data.XmlDataProvider.XPath" /> queries. The default value is null.</returns>
	[DefaultValue(null)]
	public XmlNamespaceManager XmlNamespaceManager
	{
		get
		{
			return _nsMgr;
		}
		set
		{
			if (_nsMgr != value)
			{
				_nsMgr = value;
				OnPropertyChanged(new PropertyChangedEventArgs("XmlNamespaceManager"));
				if (!base.IsRefreshDeferred)
				{
					Refresh();
				}
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether node collection creation will be performed in a worker thread or in the active context.</summary>
	/// <returns>true to perform node collection creation in a worker thread; otherwise, false. The default value is true.</returns>
	[DefaultValue(true)]
	public bool IsAsynchronous
	{
		get
		{
			return _isAsynchronous;
		}
		set
		{
			_isAsynchronous = value;
		}
	}

	/// <summary>Gets the inline XML content.</summary>
	/// <returns>The inline XML content.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public IXmlSerializable XmlSerializer
	{
		get
		{
			if (_xmlSerializer == null)
			{
				_xmlSerializer = new XmlIslandSerializer(this);
			}
			return _xmlSerializer;
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The base URI.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return BaseUri;
		}
		set
		{
			BaseUri = value;
		}
	}

	/// <summary> This type or member supports the WPF infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The base URI.</returns>
	protected virtual Uri BaseUri
	{
		get
		{
			return _baseUri;
		}
		set
		{
			_baseUri = value;
		}
	}

	private XmlDocument DocumentForSerialization
	{
		get
		{
			if (_tryInlineDoc || _savedDocument != null || _domSetDocument != null)
			{
				if (_waitForInlineDoc != null)
				{
					_waitForInlineDoc.WaitOne();
				}
				return _document;
			}
			return null;
		}
	}

	private XmlDataCollection XmlDataCollection => (XmlDataCollection)base.Data;

	private DispatcherOperationCallback CompletedCallback
	{
		get
		{
			if (_onCompletedCallback == null)
			{
				_onCompletedCallback = OnCompletedCallback;
			}
			return _onCompletedCallback;
		}
	}

	private XmlNodeChangedEventHandler NodeChangeHandler
	{
		get
		{
			if (_nodeChangedHandler == null)
			{
				_nodeChangedHandler = OnNodeChanged;
			}
			return _nodeChangedHandler;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.XmlDataProvider" /> class.</summary>
	public XmlDataProvider()
	{
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.XmlDataProvider.Source" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeSource()
	{
		if (_domSetDocument == null)
		{
			return _source != null;
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.XmlDataProvider.XPath" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeXPath()
	{
		if (_xPath != null)
		{
			return _xPath.Length != 0;
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.XmlDataProvider.XmlSerializer" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeXmlSerializer()
	{
		return DocumentForSerialization != null;
	}

	/// <summary>Prepares the loading of either the inline XML or the external XML file to produce a collection of XML nodes.</summary>
	protected override void BeginQuery()
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.BeginQuery(TraceData.Identify(this), IsAsynchronous ? "asynchronous" : "synchronous"));
		}
		if (_source != null)
		{
			DiscardInline();
			LoadFromSource();
			return;
		}
		XmlDocument xmlDocument = null;
		if (_domSetDocument != null)
		{
			DiscardInline();
			xmlDocument = _domSetDocument;
		}
		else
		{
			if (_inEndInit)
			{
				return;
			}
			xmlDocument = _savedDocument;
		}
		if (IsAsynchronous && xmlDocument != null)
		{
			ThreadPool.QueueUserWorkItem(BuildNodeCollectionAsynch, xmlDocument);
		}
		else if (xmlDocument != null || base.Data != null)
		{
			BuildNodeCollection(xmlDocument);
		}
	}

	/// <summary>Indicates that the initialization of this element has completed; this causes a <see cref="M:System.Windows.Data.DataSourceProvider.Refresh" /> if no other <see cref="M:System.Windows.Data.DataSourceProvider.DeferRefresh" /> is outstanding.</summary>
	protected override void EndInit()
	{
		try
		{
			_inEndInit = true;
			base.EndInit();
		}
		finally
		{
			_inEndInit = false;
		}
	}

	private void LoadFromSource()
	{
		Uri uri = Source;
		if (!uri.IsAbsoluteUri)
		{
			uri = MS.Internal.Utility.BindUriHelper.GetResolvedUri((_baseUri != null) ? _baseUri : MS.Internal.Utility.BindUriHelper.BaseUri, uri);
		}
		WebRequest webRequest = PackWebRequestFactory.CreateWebRequest(uri);
		if (webRequest == null)
		{
			throw new Exception(SR.WebRequestCreationFailed);
		}
		if (IsAsynchronous)
		{
			ThreadPool.QueueUserWorkItem(CreateDocFromExternalSourceAsynch, webRequest);
		}
		else
		{
			CreateDocFromExternalSource(webRequest);
		}
	}

	private void ParseInline(XmlReader xmlReader)
	{
		if (_source == null && _domSetDocument == null && _tryInlineDoc)
		{
			if (IsAsynchronous)
			{
				_waitForInlineDoc = new ManualResetEvent(initialState: false);
				ThreadPool.QueueUserWorkItem(CreateDocFromInlineXmlAsync, xmlReader);
			}
			else
			{
				CreateDocFromInlineXml(xmlReader);
			}
		}
	}

	private void CreateDocFromInlineXmlAsync(object arg)
	{
		XmlReader xmlReader = (XmlReader)arg;
		CreateDocFromInlineXml(xmlReader);
	}

	private void CreateDocFromInlineXml(XmlReader xmlReader)
	{
		if (!_tryInlineDoc)
		{
			_savedDocument = null;
			if (_waitForInlineDoc != null)
			{
				_waitForInlineDoc.Set();
			}
			return;
		}
		Exception ex = null;
		XmlDocument xmlDocument;
		try
		{
			xmlDocument = new XmlDocument();
			try
			{
				if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
				{
					TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.XmlLoadInline(TraceData.Identify(this), base.Dispatcher.CheckAccess() ? "synchronous" : "asynchronous"));
				}
				xmlDocument.Load(xmlReader);
			}
			catch (XmlException ex2)
			{
				if (TraceData.IsEnabled)
				{
					TraceData.TraceAndNotify(TraceEventType.Error, TraceData.XmlDPInlineDocError, ex2);
				}
				ex = ex2;
			}
			if (ex == null)
			{
				_savedDocument = (XmlDocument)xmlDocument.Clone();
			}
		}
		finally
		{
			xmlReader.Close();
			if (_waitForInlineDoc != null)
			{
				_waitForInlineDoc.Set();
			}
		}
		if (TraceData.IsEnabled)
		{
			XmlNode documentElement = xmlDocument.DocumentElement;
			if (documentElement != null && documentElement.NamespaceURI == xmlReader.LookupNamespace(string.Empty))
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.XmlNamespaceNotSet);
			}
		}
		if (ex == null)
		{
			BuildNodeCollection(xmlDocument);
			return;
		}
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.QueryFinished(TraceData.Identify(this), base.Dispatcher.CheckAccess() ? "synchronous" : "asynchronous", TraceData.Identify(null), TraceData.IdentifyException(ex)), ex);
		}
		OnQueryFinished(null, ex, CompletedCallback, null);
	}

	private void CreateDocFromExternalSourceAsynch(object arg)
	{
		WebRequest request = (WebRequest)arg;
		CreateDocFromExternalSource(request);
	}

	private void CreateDocFromExternalSource(WebRequest request)
	{
		bool flag = TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer);
		XmlDocument xmlDocument = new XmlDocument();
		Exception ex = null;
		try
		{
			if (flag)
			{
				TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.XmlLoadSource(TraceData.Identify(this), base.Dispatcher.CheckAccess() ? "synchronous" : "asynchronous", TraceData.Identify(request.RequestUri.ToString())));
			}
			Stream responseStream = (WpfWebRequestHelper.GetResponse(request) ?? throw new InvalidOperationException(SR.GetResponseFailed)).GetResponseStream();
			if (flag)
			{
				TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.XmlLoadDoc(TraceData.Identify(this)));
			}
			xmlDocument.Load(responseStream);
			responseStream.Close();
		}
		catch (Exception ex2)
		{
			if (CriticalExceptions.IsCriticalException(ex2))
			{
				throw;
			}
			ex = ex2;
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.XmlDPAsyncDocError, null, new object[2] { Source, ex }, new object[1] { ex });
			}
		}
		catch
		{
			throw;
		}
		if (ex != null)
		{
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
			{
				TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.QueryFinished(TraceData.Identify(this), base.Dispatcher.CheckAccess() ? "synchronous" : "asynchronous", TraceData.Identify(null), TraceData.IdentifyException(ex)), ex);
			}
			OnQueryFinished(null, ex, CompletedCallback, null);
		}
		else
		{
			BuildNodeCollection(xmlDocument);
		}
	}

	private void BuildNodeCollectionAsynch(object arg)
	{
		XmlDocument doc = (XmlDocument)arg;
		BuildNodeCollection(doc);
	}

	private void BuildNodeCollection(XmlDocument doc)
	{
		XmlDataCollection xmlDataCollection = null;
		if (doc != null)
		{
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.CreateExpression))
			{
				TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.XmlBuildCollection(TraceData.Identify(this)));
			}
			XmlNodeList resultNodeList = GetResultNodeList(doc);
			xmlDataCollection = new XmlDataCollection(this);
			if (resultNodeList != null)
			{
				xmlDataCollection.SynchronizeCollection(resultNodeList);
			}
		}
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.QueryFinished(TraceData.Identify(this), base.Dispatcher.CheckAccess() ? "synchronous" : "asynchronous", TraceData.Identify(xmlDataCollection), TraceData.IdentifyException(null)));
		}
		OnQueryFinished(xmlDataCollection, null, CompletedCallback, doc);
	}

	private object OnCompletedCallback(object arg)
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.QueryResult(TraceData.Identify(this), TraceData.Identify(base.Data)));
		}
		ChangeDocument((XmlDocument)arg);
		return null;
	}

	private void ChangeDocument(XmlDocument doc)
	{
		if (_document != doc)
		{
			if (_document != null)
			{
				UnHook();
			}
			_document = doc;
			if (_document != null)
			{
				Hook();
			}
			OnPropertyChanged(new PropertyChangedEventArgs("Document"));
		}
	}

	private void DiscardInline()
	{
		_tryInlineDoc = false;
		_savedDocument = null;
		if (_waitForInlineDoc != null)
		{
			_waitForInlineDoc.Set();
		}
	}

	private void Hook()
	{
		if (!_isListening)
		{
			_document.NodeInserted += NodeChangeHandler;
			_document.NodeRemoved += NodeChangeHandler;
			_document.NodeChanged += NodeChangeHandler;
			_isListening = true;
		}
	}

	private void UnHook()
	{
		if (_isListening)
		{
			_document.NodeInserted -= NodeChangeHandler;
			_document.NodeRemoved -= NodeChangeHandler;
			_document.NodeChanged -= NodeChangeHandler;
			_isListening = false;
		}
	}

	private void OnNodeChanged(object sender, XmlNodeChangedEventArgs e)
	{
		if (XmlDataCollection != null)
		{
			UnHook();
			XmlNodeList resultNodeList = GetResultNodeList((XmlDocument)sender);
			XmlDataCollection.SynchronizeCollection(resultNodeList);
			Hook();
		}
	}

	private XmlNodeList GetResultNodeList(XmlDocument doc)
	{
		XmlNodeList result = null;
		if (doc.DocumentElement != null)
		{
			string text = (string.IsNullOrEmpty(XPath) ? "/" : XPath);
			try
			{
				result = ((XmlNamespaceManager == null) ? doc.SelectNodes(text) : doc.SelectNodes(text, XmlNamespaceManager));
			}
			catch (XPathException ex)
			{
				if (TraceData.IsEnabled)
				{
					TraceData.TraceAndNotify(TraceEventType.Error, TraceData.XmlDPSelectNodesFailed, null, new object[2] { text, ex }, new object[1] { ex });
				}
			}
		}
		return result;
	}
}
