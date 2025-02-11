using System.Collections.Generic;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Xml.XPath;
using MS.Internal;
using MS.Internal.Annotations;
using MS.Internal.Annotations.Storage;
using MS.Utility;

namespace System.Windows.Annotations.Storage;

/// <summary>Represents an XML data store for writing and reading user annotations.</summary>
public sealed class XmlStreamStore : AnnotationStore
{
	private bool _dirty;

	private bool _autoFlush;

	private XmlDocument _document;

	private XmlNamespaceManager _namespaceManager;

	private Stream _stream;

	private XPathNavigator _rootNavigator;

	private StoreAnnotationsMap _storeAnnotationsMap;

	private List<Uri> _ignoredNamespaces = new List<Uri>();

	private XmlCompatibilityReader _xmlCompatibilityReader;

	private static readonly Dictionary<Uri, IList<Uri>> _predefinedNamespaces;

	private static readonly Serializer _serializer;

	/// <summary>Gets or sets a value that indicates whether data in annotation buffers is to be written immediately to the physical data store.</summary>
	/// <returns>true if data in annotation buffers is to be written immediately to the physical data store for each operation; otherwise, false if data in the annotation buffers is to be written when the application explicitly calls <see cref="M:System.Windows.Annotations.Storage.XmlStreamStore.Flush" />.</returns>
	public override bool AutoFlush
	{
		get
		{
			lock (base.SyncRoot)
			{
				return _autoFlush;
			}
		}
		set
		{
			lock (base.SyncRoot)
			{
				_autoFlush = value;
				if (_autoFlush)
				{
					Flush();
				}
			}
		}
	}

	/// <summary>Gets a list of the namespaces that were ignored when the XML stream was loaded.</summary>
	/// <returns>The list of the namespaces that were ignored when the XML stream was loaded.</returns>
	public IList<Uri> IgnoredNamespaces => _ignoredNamespaces;

	/// <summary>Gets a list of all namespaces that are predefined by the Annotations Framework.</summary>
	/// <returns>The list of namespaces that are predefined by the Microsoft Annotations Framework.</returns>
	public static IList<Uri> WellKnownNamespaces
	{
		get
		{
			Uri[] array = new Uri[_predefinedNamespaces.Keys.Count];
			_predefinedNamespaces.Keys.CopyTo(array, 0);
			return array;
		}
	}

	static XmlStreamStore()
	{
		_serializer = new Serializer(typeof(Annotation));
		_predefinedNamespaces = new Dictionary<Uri, IList<Uri>>(6);
		_predefinedNamespaces.Add(new Uri("http://schemas.microsoft.com/windows/annotations/2003/11/core"), null);
		_predefinedNamespaces.Add(new Uri("http://schemas.microsoft.com/windows/annotations/2003/11/base"), null);
		_predefinedNamespaces.Add(new Uri("http://schemas.microsoft.com/winfx/2006/xaml/presentation"), null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.Storage.XmlStreamStore" /> class with a specified I/O <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="stream">The I/O stream for reading and writing user annotations.</param>
	public XmlStreamStore(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanSeek)
		{
			throw new ArgumentException(SR.StreamDoesNotSupportSeek);
		}
		SetStream(stream, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.Storage.XmlStreamStore" /> class with a specified I/O <see cref="T:System.IO.Stream" /> and dictionary of known compatible namespaces.</summary>
	/// <param name="stream">The I/O stream for reading and writing user annotations.</param>
	/// <param name="knownNamespaces">A dictionary with a list of known compatible namespaces.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="stream" /> contains invalid XML.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="knownNamespaces" /> dictionary contains a duplicate namespace.-or-The <paramref name="knownNamespaces" /> dictionary contains an element that has a null key.</exception>
	public XmlStreamStore(Stream stream, IDictionary<Uri, IList<Uri>> knownNamespaces)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SetStream(stream, knownNamespaces);
	}

	/// <summary>Adds a new <see cref="T:System.Windows.Annotations.Annotation" /> to the store.</summary>
	/// <param name="newAnnotation">The annotation to add to the store.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="newAnnotation" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">An <see cref="T:System.Windows.Annotations.Annotation" /> with the same <see cref="P:System.Windows.Annotations.Annotation.Id" /> already is in the store.</exception>
	/// <exception cref="T:System.InvalidOperationException">An I/O <see cref="T:System.IO.Stream" /> has not been set for the store.</exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	public override void AddAnnotation(Annotation newAnnotation)
	{
		if (newAnnotation == null)
		{
			throw new ArgumentNullException("newAnnotation");
		}
		lock (base.SyncRoot)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AddAnnotationBegin);
			try
			{
				CheckStatus();
				if (GetAnnotationNodeForId(newAnnotation.Id) != null)
				{
					throw new ArgumentException(SR.AnnotationAlreadyExists, "newAnnotation");
				}
				if (_storeAnnotationsMap.FindAnnotation(newAnnotation.Id) != null)
				{
					throw new ArgumentException(SR.AnnotationAlreadyExists, "newAnnotation");
				}
				_storeAnnotationsMap.AddAnnotation(newAnnotation, dirty: true);
			}
			finally
			{
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AddAnnotationEnd);
			}
		}
		OnStoreContentChanged(new StoreContentChangedEventArgs(StoreContentAction.Added, newAnnotation));
	}

	/// <summary>Deletes the annotation with the specified <see cref="P:System.Windows.Annotations.Annotation.Id" /> from the store. </summary>
	/// <returns>The annotation that was deleted; otherwise, null if an annotation with the specified <paramref name="annotationId" /> was not found in the store.</returns>
	/// <param name="annotationId">The globally unique identifier (GUID) <see cref="P:System.Windows.Annotations.Annotation.Id" /> property of the annotation to be deleted.</param>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	/// <exception cref="T:System.InvalidOperationException">An I/O <see cref="T:System.IO.Stream" /> has not been set for the store.</exception>
	public override Annotation DeleteAnnotation(Guid annotationId)
	{
		Annotation annotation = null;
		lock (base.SyncRoot)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.DeleteAnnotationBegin);
			try
			{
				CheckStatus();
				annotation = _storeAnnotationsMap.FindAnnotation(annotationId);
				XPathNavigator annotationNodeForId = GetAnnotationNodeForId(annotationId);
				if (annotationNodeForId != null)
				{
					if (annotation == null)
					{
						annotation = (Annotation)_serializer.Deserialize(annotationNodeForId.ReadSubtree());
					}
					annotationNodeForId.DeleteSelf();
				}
				_storeAnnotationsMap.RemoveAnnotation(annotationId);
			}
			finally
			{
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.DeleteAnnotationEnd);
			}
		}
		if (annotation != null)
		{
			OnStoreContentChanged(new StoreContentChangedEventArgs(StoreContentAction.Deleted, annotation));
		}
		return annotation;
	}

	/// <summary>Returns a list of annotations that have <see cref="P:System.Windows.Annotations.Annotation.Anchors" /> with locators that begin with a matching <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> sequence.</summary>
	/// <returns>The list of annotations that have <see cref="P:System.Windows.Annotations.Annotation.Anchors" /> with locators that start and match the given <paramref name="anchorLocator" />; otherwise, null if no matching annotations were found.</returns>
	/// <param name="anchorLocator">The starting <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> sequence to return matching annotations for.</param>
	public override IList<Annotation> GetAnnotations(ContentLocator anchorLocator)
	{
		if (anchorLocator == null)
		{
			throw new ArgumentNullException("anchorLocator");
		}
		if (anchorLocator.Parts == null)
		{
			throw new ArgumentNullException("anchorLocator.Parts");
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.GetAnnotationByLocBegin);
		IList<Annotation> list = null;
		try
		{
			string text = "//anc:ContentLocator";
			if (anchorLocator.Parts.Count > 0)
			{
				text += "/child::*[1]/self::";
				for (int i = 0; i < anchorLocator.Parts.Count; i++)
				{
					if (anchorLocator.Parts[i] != null)
					{
						if (i > 0)
						{
							text += "/following-sibling::";
						}
						string queryFragment = anchorLocator.Parts[i].GetQueryFragment(_namespaceManager);
						text = ((queryFragment == null) ? (text + "*") : (text + queryFragment));
					}
				}
			}
			text += "/ancestor::anc:Anchors/ancestor::anc:Annotation";
			return InternalGetAnnotations(text, anchorLocator);
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.GetAnnotationByLocEnd);
		}
	}

	/// <summary>Returns a list of all the annotations in the store.</summary>
	/// <returns>The list of all annotations that are currently in the store.</returns>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	public override IList<Annotation> GetAnnotations()
	{
		IList<Annotation> list = null;
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.GetAnnotationsBegin);
		try
		{
			string query = "//anc:Annotation";
			return InternalGetAnnotations(query, null);
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.GetAnnotationsEnd);
		}
	}

	/// <summary>Returns the annotation with the specified <see cref="P:System.Windows.Annotations.Annotation.Id" /> from the store.</summary>
	/// <returns>The annotation with the given <paramref name="annotationId" />; otherwise, null if an annotation with the specified <paramref name="annotationId" /> was not found in the store.</returns>
	/// <param name="annotationId">The globally unique identifier (GUID) <see cref="P:System.Windows.Annotations.Annotation.Id" /> property of the annotation to be returned.</param>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	public override Annotation GetAnnotation(Guid annotationId)
	{
		lock (base.SyncRoot)
		{
			Annotation annotation = null;
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.GetAnnotationByIdBegin);
			try
			{
				CheckStatus();
				annotation = _storeAnnotationsMap.FindAnnotation(annotationId);
				if (annotation != null)
				{
					return annotation;
				}
				XPathNavigator annotationNodeForId = GetAnnotationNodeForId(annotationId);
				if (annotationNodeForId != null)
				{
					annotation = (Annotation)_serializer.Deserialize(annotationNodeForId.ReadSubtree());
					_storeAnnotationsMap.AddAnnotation(annotation, dirty: false);
				}
			}
			finally
			{
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.GetAnnotationByIdEnd);
			}
			return annotation;
		}
	}

	/// <summary>Forces any annotation data retained in internal buffers to be written to the underlying storage device.</summary>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	/// <exception cref="T:System.InvalidOperationException">An I/O <see cref="T:System.IO.Stream" /> has not been set for the store.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The store I/O <see cref="T:System.IO.Stream" /> is read-only and cannot be accessed for output.</exception>
	public override void Flush()
	{
		lock (base.SyncRoot)
		{
			CheckStatus();
			if (!_stream.CanWrite)
			{
				throw new UnauthorizedAccessException(SR.StreamCannotBeWritten);
			}
			if (_dirty)
			{
				SerializeAnnotations();
				_stream.Position = 0L;
				_stream.SetLength(0L);
				_document.PreserveWhitespace = true;
				_document.Save(_stream);
				_stream.Flush();
				_dirty = false;
			}
		}
	}

	/// <summary>Returns a list of namespaces that are compatible as an input namespace.</summary>
	/// <returns>A list of compatible namespaces that match <paramref name="name" />; otherwise, null if there are no compatible namespaces found.</returns>
	/// <param name="name">The starting URI sequence to return the list of namespaces for.</param>
	public static IList<Uri> GetWellKnownCompatibleNamespaces(Uri name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_predefinedNamespaces.ContainsKey(name))
		{
			return _predefinedNamespaces[name];
		}
		return null;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			Cleanup();
		}
	}

	protected override void OnStoreContentChanged(StoreContentChangedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			_dirty = true;
		}
		base.OnStoreContentChanged(e);
	}

	private List<Guid> FindAnnotationIds(string queryExpression)
	{
		Invariant.Assert(queryExpression != null && queryExpression.Length > 0, "Invalid query expression");
		List<Guid> list = null;
		lock (base.SyncRoot)
		{
			CheckStatus();
			XPathNodeIterator xPathNodeIterator = _document.CreateNavigator().Select(queryExpression, _namespaceManager);
			if (xPathNodeIterator != null && xPathNodeIterator.Count > 0)
			{
				list = new List<Guid>(xPathNodeIterator.Count);
				foreach (XPathNavigator item2 in xPathNodeIterator)
				{
					string attribute = item2.GetAttribute("Id", "");
					if (string.IsNullOrEmpty(attribute))
					{
						throw new XmlException(SR.Format(SR.RequiredAttributeMissing, "Id", "Annotation"));
					}
					Guid item;
					try
					{
						item = XmlConvert.ToGuid(attribute);
					}
					catch (FormatException innerException)
					{
						throw new InvalidOperationException(SR.CannotParseId, innerException);
					}
					list.Add(item);
				}
			}
			else
			{
				list = new List<Guid>(0);
			}
		}
		return list;
	}

	private void HandleAuthorChanged(object sender, AnnotationAuthorChangedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			_dirty = true;
		}
		OnAuthorChanged(e);
	}

	private void HandleAnchorChanged(object sender, AnnotationResourceChangedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			_dirty = true;
		}
		OnAnchorChanged(e);
	}

	private void HandleCargoChanged(object sender, AnnotationResourceChangedEventArgs e)
	{
		lock (base.SyncRoot)
		{
			_dirty = true;
		}
		OnCargoChanged(e);
	}

	private IList<Annotation> MergeAndCacheAnnotations(Dictionary<Guid, Annotation> mapAnnotations, List<Guid> storeAnnotationsId)
	{
		List<Annotation> list = new List<Annotation>(mapAnnotations.Values);
		foreach (Guid item in storeAnnotationsId)
		{
			if (!mapAnnotations.TryGetValue(item, out var value))
			{
				value = GetAnnotation(item);
				list.Add(value);
			}
		}
		return list;
	}

	private IList<Annotation> InternalGetAnnotations(string query, ContentLocator anchorLocator)
	{
		Invariant.Assert(query != null, "Parameter 'query' is null.");
		lock (base.SyncRoot)
		{
			CheckStatus();
			List<Guid> storeAnnotationsId = FindAnnotationIds(query);
			Dictionary<Guid, Annotation> dictionary = null;
			dictionary = ((anchorLocator != null) ? _storeAnnotationsMap.FindAnnotations(anchorLocator) : _storeAnnotationsMap.FindAnnotations());
			return MergeAndCacheAnnotations(dictionary, storeAnnotationsId);
		}
	}

	private void LoadStream(IDictionary<Uri, IList<Uri>> knownNamespaces)
	{
		CheckKnownNamespaces(knownNamespaces);
		lock (base.SyncRoot)
		{
			_document = new XmlDocument();
			_document.PreserveWhitespace = false;
			if (_stream.Length == 0L)
			{
				_document.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?> <anc:Annotations xmlns:anc=\"http://schemas.microsoft.com/windows/annotations/2003/11/core\" xmlns:anb=\"http://schemas.microsoft.com/windows/annotations/2003/11/base\" />");
			}
			else
			{
				_xmlCompatibilityReader = SetupReader(knownNamespaces);
				_document.Load(_xmlCompatibilityReader);
			}
			_namespaceManager = new XmlNamespaceManager(_document.NameTable);
			_namespaceManager.AddNamespace("anc", "http://schemas.microsoft.com/windows/annotations/2003/11/core");
			_namespaceManager.AddNamespace("anb", "http://schemas.microsoft.com/windows/annotations/2003/11/base");
			XPathNodeIterator xPathNodeIterator = _document.CreateNavigator().Select("//anc:Annotations", _namespaceManager);
			Invariant.Assert(xPathNodeIterator.Count == 1, "More than one annotation returned for the query");
			xPathNodeIterator.MoveNext();
			_rootNavigator = xPathNodeIterator.Current;
		}
	}

	private void CheckKnownNamespaces(IDictionary<Uri, IList<Uri>> knownNamespaces)
	{
		if (knownNamespaces == null)
		{
			return;
		}
		IList<Uri> list = new List<Uri>();
		foreach (Uri key in _predefinedNamespaces.Keys)
		{
			list.Add(key);
		}
		foreach (Uri key2 in knownNamespaces.Keys)
		{
			if (key2 == null)
			{
				throw new ArgumentException(SR.NullUri, "knownNamespaces");
			}
			if (list.Contains(key2))
			{
				throw new ArgumentException(SR.DuplicatedUri, "knownNamespaces");
			}
			list.Add(key2);
		}
		foreach (KeyValuePair<Uri, IList<Uri>> knownNamespace in knownNamespaces)
		{
			if (knownNamespace.Value == null)
			{
				continue;
			}
			foreach (Uri item in knownNamespace.Value)
			{
				if (item == null)
				{
					throw new ArgumentException(SR.NullUri, "knownNamespaces");
				}
				if (list.Contains(item))
				{
					throw new ArgumentException(SR.DuplicatedCompatibleUri, "knownNamespaces");
				}
				list.Add(item);
			}
		}
	}

	private XmlCompatibilityReader SetupReader(IDictionary<Uri, IList<Uri>> knownNamespaces)
	{
		IList<string> list = new List<string>();
		foreach (Uri key in _predefinedNamespaces.Keys)
		{
			list.Add(key.ToString());
		}
		if (knownNamespaces != null)
		{
			foreach (Uri key2 in knownNamespaces.Keys)
			{
				list.Add(key2.ToString());
			}
		}
		XmlCompatibilityReader xmlCompatibilityReader = new XmlCompatibilityReader(new XmlTextReader(_stream), IsXmlNamespaceSupported, list);
		if (knownNamespaces != null)
		{
			foreach (KeyValuePair<Uri, IList<Uri>> knownNamespace in knownNamespaces)
			{
				if (knownNamespace.Value == null)
				{
					continue;
				}
				foreach (Uri item in knownNamespace.Value)
				{
					xmlCompatibilityReader.DeclareNamespaceCompatibility(knownNamespace.Key.ToString(), item.ToString());
				}
			}
		}
		_ignoredNamespaces.Clear();
		return xmlCompatibilityReader;
	}

	private bool IsXmlNamespaceSupported(string xmlNamespace, out string newXmlNamespace)
	{
		if (!string.IsNullOrEmpty(xmlNamespace))
		{
			if (!Uri.IsWellFormedUriString(xmlNamespace, UriKind.RelativeOrAbsolute))
			{
				throw new ArgumentException(SR.Format(SR.InvalidNamespace, xmlNamespace), "xmlNamespace");
			}
			Uri item = new Uri(xmlNamespace, UriKind.RelativeOrAbsolute);
			if (!_ignoredNamespaces.Contains(item))
			{
				_ignoredNamespaces.Add(item);
			}
		}
		newXmlNamespace = null;
		return false;
	}

	private XPathNavigator GetAnnotationNodeForId(Guid id)
	{
		XPathNavigator result = null;
		lock (base.SyncRoot)
		{
			XPathNodeIterator xPathNodeIterator = _document.CreateNavigator().Select("//anc:Annotation[@Id=\"" + XmlConvert.ToString(id) + "\"]", _namespaceManager);
			if (xPathNodeIterator.MoveNext())
			{
				result = xPathNodeIterator.Current;
			}
		}
		return result;
	}

	private void CheckStatus()
	{
		lock (base.SyncRoot)
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(null, SR.ObjectDisposed_StoreClosed);
			}
			if (_stream == null)
			{
				throw new InvalidOperationException(SR.StreamNotSet);
			}
		}
	}

	private void SerializeAnnotations()
	{
		foreach (Annotation item in _storeAnnotationsMap.FindDirtyAnnotations())
		{
			XPathNavigator annotationNodeForId = GetAnnotationNodeForId(item.Id);
			if (annotationNodeForId == null)
			{
				annotationNodeForId = _rootNavigator.CreateNavigator();
				XmlWriter xmlWriter = annotationNodeForId.AppendChild();
				_serializer.Serialize(xmlWriter, item);
				xmlWriter.Close();
			}
			else
			{
				XmlWriter xmlWriter2 = annotationNodeForId.InsertBefore();
				_serializer.Serialize(xmlWriter2, item);
				xmlWriter2.Close();
				annotationNodeForId.DeleteSelf();
			}
		}
		_storeAnnotationsMap.ValidateDirtyAnnotations();
	}

	private void Cleanup()
	{
		lock (base.SyncRoot)
		{
			_xmlCompatibilityReader = null;
			_ignoredNamespaces = null;
			_stream = null;
			_document = null;
			_rootNavigator = null;
			_storeAnnotationsMap = null;
		}
	}

	private void SetStream(Stream stream, IDictionary<Uri, IList<Uri>> knownNamespaces)
	{
		try
		{
			lock (base.SyncRoot)
			{
				_storeAnnotationsMap = new StoreAnnotationsMap(HandleAuthorChanged, HandleAnchorChanged, HandleCargoChanged);
				_stream = stream;
				LoadStream(knownNamespaces);
			}
		}
		catch
		{
			Cleanup();
			throw;
		}
	}
}
