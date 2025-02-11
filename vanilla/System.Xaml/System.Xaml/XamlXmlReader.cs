using System.Collections.Generic;
using System.IO;
using System.Xaml.Schema;
using System.Xml;
using MS.Internal.Xaml;
using MS.Internal.Xaml.Context;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

/// <summary>Processes XAML markup from XML files by using an <see cref="T:System.Xml.XmlReader" /> intermediary, and produces a XAML node stream.</summary>
public class XamlXmlReader : XamlReader, IXamlLineInfo
{
	private XamlParserContext _context;

	private IEnumerator<XamlNode> _nodeStream;

	private XamlNode _current;

	private LineInfo _currentLineInfo;

	private XamlNode _endOfStreamNode;

	private XamlXmlReaderSettings _mergedSettings;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>A value of the <see cref="T:System.Xaml.XamlNodeType" /> enumeration.</returns>
	public override XamlNodeType NodeType => _current.NodeType;

	/// <summary>Gets a value that reports whether the reader position in the XAML node stream is at end-of-file.</summary>
	/// <returns>true if the position is at the conceptual end-of-file in the node stream; otherwise, false.</returns>
	public override bool IsEof => _current.IsEof;

	/// <summary>Gets the XAML namespace from the current node.</summary>
	/// <returns>The XAML namespace from the current node, if it is available; otherwise, null.</returns>
	public override NamespaceDeclaration Namespace => _current.NamespaceDeclaration;

	/// <summary>Gets the <see cref="T:System.Xaml.XamlType" /> of the current node.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> of the current node; or null, if the position is not on an object.</returns>
	public override XamlType Type => _current.XamlType;

	/// <summary>Gets the value of the current node.</summary>
	/// <returns>The value of the current node; or null, if the position is not on a <see cref="F:System.Xaml.XamlNodeType.Value" /> node type.</returns>
	public override object Value => _current.Value;

	/// <summary>Gets the current member at the reader position, if the current reader position is on a <see cref="F:System.Xaml.XamlNodeType.StartMember" />.</summary>
	/// <returns>The current member; or null, if the current reader position is not on a member.</returns>
	public override XamlMember Member => _current.Member;

	/// <summary>Gets an object that provides schema information for the information set.</summary>
	/// <returns>An object that provides schema information for the information set.</returns>
	public override XamlSchemaContext SchemaContext => _context.SchemaContext;

	/// <summary>Gets a value that specifies whether line information is available.</summary>
	/// <returns>true if line information is available; otherwise, false.</returns>
	public bool HasLineInfo => _mergedSettings.ProvideLineInfo;

	/// <summary>Gets the line number to report.</summary>
	/// <returns>The line number to report.</returns>
	public int LineNumber => _currentLineInfo.LineNumber;

	/// <summary>Gets the line position to report.</summary>
	/// <returns>The line position to report.</returns>
	public int LinePosition => _currentLineInfo.LinePosition;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class using the provided <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="xmlReader">The <see cref="T:System.Xml.XmlReader" /> to use as the intermediary XML processor.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlReader" /> is null.</exception>
	public XamlXmlReader(XmlReader xmlReader)
	{
		ArgumentNullException.ThrowIfNull(xmlReader, "xmlReader");
		Initialize(xmlReader, null, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, using the provided <see cref="T:System.Xml.XmlReader" /> and reader settings.</summary>
	/// <param name="xmlReader">The <see cref="T:System.Xml.XmlReader" /> to use as the intermediary XML processor.</param>
	/// <param name="settings">The specific XAML reader settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlReader" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(XmlReader xmlReader, XamlXmlReaderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(xmlReader, "xmlReader");
		Initialize(xmlReader, null, settings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class using the provided <see cref="T:System.Xml.XmlReader" /> and schema context.</summary>
	/// <param name="xmlReader">The <see cref="T:System.Xml.XmlReader" /> to use as the intermediary XML processor.</param>
	/// <param name="schemaContext">The XAML schema context for XAML processing.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlReader" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext)
	{
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		ArgumentNullException.ThrowIfNull(xmlReader, "xmlReader");
		Initialize(xmlReader, schemaContext, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class using the provided <see cref="T:System.Xml.XmlReader" />, schema context, and reader settings.</summary>
	/// <param name="xmlReader">The <see cref="T:System.Xml.XmlReader" /> to use as the intermediary XML processor.</param>
	/// <param name="schemaContext">The XAML schema context for XAML processing.</param>
	/// <param name="settings">The specific XAML reader settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlReader" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		ArgumentNullException.ThrowIfNull(xmlReader, "xmlReader");
		Initialize(xmlReader, schemaContext, settings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a file name of a file to load into a default XML reader.</summary>
	/// <param name="fileName">The name of the XML file to load.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null.</exception>
	public XamlXmlReader(string fileName)
	{
		ArgumentNullException.ThrowIfNull(fileName, "fileName");
		Initialize(CreateXmlReader(fileName, null), null, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on the file name of a file to load into a default XML reader, and using XAML-specific reader settings.</summary>
	/// <param name="fileName">The name of the XML file to load.</param>
	/// <param name="settings">The specific reader settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null.</exception>
	public XamlXmlReader(string fileName, XamlXmlReaderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(fileName, "fileName");
		Initialize(CreateXmlReader(fileName, settings), null, settings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on the file name of a file to load into a default XML reader, with a supplied XAML schema context.</summary>
	/// <param name="fileName">The name of the file to load.</param>
	/// <param name="schemaContext">The XAML schema context for XAML processing.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(string fileName, XamlSchemaContext schemaContext)
	{
		ArgumentNullException.ThrowIfNull(fileName, "fileName");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(CreateXmlReader(fileName, null), schemaContext, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on the file name of a file to load into a default XML reader, and using a supplied XAML schema context and XAML-specific reader settings.</summary>
	/// <param name="fileName">The name of the XML file to load.</param>
	/// <param name="schemaContext">The XAML schema context for XAML processing.</param>
	/// <param name="settings">The specific reader settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null.-or-<paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(string fileName, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(fileName, "fileName");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(CreateXmlReader(fileName, settings), schemaContext, settings);
	}

	private XmlReader CreateXmlReader(string fileName, XamlXmlReaderSettings settings)
	{
		bool closeInput = settings?.CloseInput ?? true;
		return XmlReader.Create(fileName, new XmlReaderSettings
		{
			CloseInput = closeInput,
			DtdProcessing = DtdProcessing.Prohibit
		});
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a stream.</summary>
	/// <param name="stream">The initial stream to load into the reader.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public XamlXmlReader(Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream, "stream");
		Initialize(CreateXmlReader(stream, null), null, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a stream, with XAML-specific settings.</summary>
	/// <param name="stream">The initial stream to load into the reader.</param>
	/// <param name="settings">The specific reader settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public XamlXmlReader(Stream stream, XamlXmlReaderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(stream, "stream");
		Initialize(CreateXmlReader(stream, settings), null, settings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a stream, and using a supplied XAML schema context.</summary>
	/// <param name="stream">The initial stream to load into the reader.</param>
	/// <param name="schemaContext">The XAML schema context for XAML processing.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(Stream stream, XamlSchemaContext schemaContext)
	{
		ArgumentNullException.ThrowIfNull(stream, "stream");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(CreateXmlReader(stream, null), schemaContext, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a stream, with a supplied XAML schema context and XAML-specific settings.</summary>
	/// <param name="stream">The initial stream to load into the reader.</param>
	/// <param name="schemaContext">The XAML schema context for XAML processing.</param>
	/// <param name="settings">The specific reader settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(Stream stream, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(stream, "stream");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(CreateXmlReader(stream, settings), schemaContext, settings);
	}

	private XmlReader CreateXmlReader(Stream stream, XamlXmlReaderSettings settings)
	{
		bool closeInput = settings?.CloseInput ?? false;
		return XmlReader.Create(stream, new XmlReaderSettings
		{
			CloseInput = closeInput,
			DtdProcessing = DtdProcessing.Prohibit
		});
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a <see cref="T:System.IO.TextReader" />.</summary>
	/// <param name="textReader">The <see cref="T:System.IO.TextReader" /> to use for initialization.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textReader" /> is null.</exception>
	public XamlXmlReader(TextReader textReader)
	{
		ArgumentNullException.ThrowIfNull(textReader, "textReader");
		Initialize(CreateXmlReader(textReader, null), null, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a <see cref="T:System.IO.TextReader" />, and using XAML-specific settings.</summary>
	/// <param name="textReader">The <see cref="T:System.IO.TextReader" /> to use for initialization.</param>
	/// <param name="settings">The specific reader settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textReader" /> is null.</exception>
	public XamlXmlReader(TextReader textReader, XamlXmlReaderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(textReader, "textReader");
		Initialize(CreateXmlReader(textReader, settings), null, settings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a <see cref="T:System.IO.TextReader" />, with a supplied schema context and XAML-specific settings.</summary>
	/// <param name="textReader">The <see cref="T:System.IO.TextReader" /> to use for initialization.</param>
	/// <param name="schemaContext">The XAML schema context for XAML processing.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textReader" /> is null.-or-<paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(TextReader textReader, XamlSchemaContext schemaContext)
	{
		ArgumentNullException.ThrowIfNull(textReader, "textReader");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(CreateXmlReader(textReader, null), schemaContext, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlReader" /> class, based on a <see cref="T:System.IO.TextReader" />, and using a supplied schema context and XAML-specific settings.</summary>
	/// <param name="textReader">The <see cref="T:System.IO.TextReader" /> to use for initialization.</param>
	/// <param name="schemaContext">The XAML schema context for XAML processing.</param>
	/// <param name="settings">The specific reader settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textReader" /> is null.-or-<paramref name="schemaContext" /> is null.</exception>
	public XamlXmlReader(TextReader textReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(textReader, "textReader");
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(CreateXmlReader(textReader, settings), schemaContext, settings);
	}

	private XmlReader CreateXmlReader(TextReader textReader, XamlXmlReaderSettings settings)
	{
		bool closeInput = settings?.CloseInput ?? false;
		return XmlReader.Create(textReader, new XmlReaderSettings
		{
			CloseInput = closeInput,
			DtdProcessing = DtdProcessing.Prohibit
		});
	}

	private void Initialize(XmlReader givenXmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		_mergedSettings = ((settings == null) ? new XamlXmlReaderSettings() : new XamlXmlReaderSettings(settings));
		XmlReader xmlReader = (_mergedSettings.SkipXmlCompatibilityProcessing ? givenXmlReader : new XmlCompatibilityReader(givenXmlReader, IsXmlNamespaceSupported)
		{
			Normalization = true
		});
		if (!string.IsNullOrEmpty(xmlReader.BaseURI))
		{
			_mergedSettings.BaseUri = new Uri(xmlReader.BaseURI);
		}
		if (xmlReader.XmlSpace == XmlSpace.Preserve)
		{
			_mergedSettings.XmlSpacePreserve = true;
		}
		if (!string.IsNullOrEmpty(xmlReader.XmlLang))
		{
			_mergedSettings.XmlLang = xmlReader.XmlLang;
		}
		IXmlNamespaceResolver xmlNamespaceResolver = xmlReader as IXmlNamespaceResolver;
		Dictionary<string, string> dictionary = null;
		if (xmlNamespaceResolver != null)
		{
			IDictionary<string, string> namespacesInScope = xmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope.Local);
			if (namespacesInScope != null)
			{
				foreach (KeyValuePair<string, string> item in namespacesInScope)
				{
					if (dictionary == null)
					{
						dictionary = new Dictionary<string, string>();
					}
					dictionary[item.Key] = item.Value;
				}
			}
		}
		if (schemaContext == null)
		{
			schemaContext = new XamlSchemaContext();
		}
		_endOfStreamNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
		_context = new XamlParserContext(schemaContext, _mergedSettings.LocalAssembly);
		_context.AllowProtectedMembersOnRoot = _mergedSettings.AllowProtectedMembersOnRoot;
		_context.AddNamespacePrefix("xml", "http://www.w3.org/XML/1998/namespace");
		Func<string, string> xmlNamespaceResolver2 = xmlReader.LookupNamespace;
		_context.XmlNamespaceResolver = xmlNamespaceResolver2;
		XamlScanner scanner = new XamlScanner(_context, xmlReader, _mergedSettings);
		XamlPullParser parser = new XamlPullParser(_context, scanner, _mergedSettings);
		_nodeStream = new NodeStreamSorter(_context, parser, _mergedSettings, dictionary);
		_current = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
		_currentLineInfo = new LineInfo(0, 0);
	}

	/// <summary>Provides the next XAML node from the loaded source, if a XAML node is available. </summary>
	/// <returns>true if a node is available; otherwise, false.</returns>
	public override bool Read()
	{
		ThrowIfDisposed();
		do
		{
			if (_nodeStream.MoveNext())
			{
				_current = _nodeStream.Current;
				if (_current.NodeType == XamlNodeType.None)
				{
					if (_current.LineInfo != null)
					{
						_currentLineInfo = _current.LineInfo;
					}
					else if (_current.IsEof)
					{
						break;
					}
				}
				continue;
			}
			_current = _endOfStreamNode;
			break;
		}
		while (_current.NodeType == XamlNodeType.None);
		return !IsEof;
	}

	private void ThrowIfDisposed()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlXmlReader");
		}
	}

	internal bool IsXmlNamespaceSupported(string xmlNamespace, out string newXmlNamespace)
	{
		if (_mergedSettings.LocalAssembly != null && ClrNamespaceUriParser.TryParseUri(xmlNamespace, out var clrNs, out var assemblyName) && string.IsNullOrEmpty(assemblyName))
		{
			assemblyName = _mergedSettings.LocalAssembly.FullName;
			newXmlNamespace = ClrNamespaceUriParser.GetUri(clrNs, assemblyName);
			return true;
		}
		bool result = _context.SchemaContext.TryGetCompatibleXamlNamespace(xmlNamespace, out newXmlNamespace);
		if (newXmlNamespace == null)
		{
			newXmlNamespace = string.Empty;
		}
		return result;
	}
}
