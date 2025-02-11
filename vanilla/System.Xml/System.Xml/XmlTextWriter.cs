using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Xml;

/// <summary>Represents a writer that provides a fast, non-cached, forward-only way of generating streams or files containing XML data that conforms to the W3C Extensible Markup Language (XML) 1.0 and the Namespaces in XML recommendations. </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class XmlTextWriter : XmlWriter
{
	private enum NamespaceState
	{
		Uninitialized,
		NotDeclaredButInScope,
		DeclaredButNotWrittenOut,
		DeclaredAndWrittenOut
	}

	private struct TagInfo
	{
		internal string name;

		internal string prefix;

		internal string defaultNs;

		internal NamespaceState defaultNsState;

		internal XmlSpace xmlSpace;

		internal string xmlLang;

		internal int prevNsTop;

		internal int prefixCount;

		internal bool mixed;

		internal void Init(int nsTop)
		{
			name = null;
			defaultNs = string.Empty;
			defaultNsState = NamespaceState.Uninitialized;
			xmlSpace = XmlSpace.None;
			xmlLang = null;
			prevNsTop = nsTop;
			prefixCount = 0;
			mixed = false;
		}
	}

	private struct Namespace
	{
		internal string prefix;

		internal string ns;

		internal bool declared;

		internal int prevNsIndex;

		internal void Set(string prefix, string ns, bool declared)
		{
			this.prefix = prefix;
			this.ns = ns;
			this.declared = declared;
			prevNsIndex = -1;
		}
	}

	private enum SpecialAttr
	{
		None,
		XmlSpace,
		XmlLang,
		XmlNs
	}

	private enum State
	{
		Start,
		Prolog,
		PostDTD,
		Element,
		Attribute,
		Content,
		AttrOnly,
		Epilog,
		Error,
		Closed
	}

	private enum Token
	{
		PI,
		Doctype,
		Comment,
		CData,
		StartElement,
		EndElement,
		LongEndElement,
		StartAttribute,
		EndAttribute,
		Content,
		Base64,
		RawData,
		Whitespace,
		Empty
	}

	private TextWriter textWriter;

	private XmlTextEncoder xmlEncoder;

	private Encoding encoding;

	private Formatting formatting;

	private bool indented;

	private int indentation;

	private char indentChar;

	private TagInfo[] stack;

	private int top;

	private State[] stateTable;

	private State currentState;

	private Token lastToken;

	private XmlTextWriterBase64Encoder base64Encoder;

	private char quoteChar;

	private char curQuoteChar;

	private bool namespaces;

	private SpecialAttr specialAttr;

	private string prefixForXmlNs;

	private bool flush;

	private Namespace[] nsStack;

	private int nsTop;

	private Dictionary<string, int> nsHashtable;

	private bool useNsHashtable;

	private XmlCharType xmlCharType = XmlCharType.Instance;

	private const int NamespaceStackInitialSize = 8;

	private const int MaxNamespacesWalkCount = 16;

	private static string[] stateName = new string[10] { "Start", "Prolog", "PostDTD", "Element", "Attribute", "Content", "AttrOnly", "Epilog", "Error", "Closed" };

	private static string[] tokenName = new string[14]
	{
		"PI", "Doctype", "Comment", "CData", "StartElement", "EndElement", "LongEndElement", "StartAttribute", "EndAttribute", "Content",
		"Base64", "RawData", "Whitespace", "Empty"
	};

	private static readonly State[] stateTableDefault = new State[104]
	{
		State.Prolog,
		State.Prolog,
		State.PostDTD,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Epilog,
		State.PostDTD,
		State.PostDTD,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Prolog,
		State.Prolog,
		State.PostDTD,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Epilog,
		State.Content,
		State.Content,
		State.Error,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Epilog,
		State.Element,
		State.Element,
		State.Element,
		State.Element,
		State.Element,
		State.Element,
		State.Error,
		State.Element,
		State.Error,
		State.Error,
		State.Error,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Error,
		State.AttrOnly,
		State.Error,
		State.Error,
		State.Attribute,
		State.Attribute,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Element,
		State.Error,
		State.Epilog,
		State.Error,
		State.Content,
		State.Content,
		State.Error,
		State.Content,
		State.Attribute,
		State.Content,
		State.Attribute,
		State.Epilog,
		State.Content,
		State.Content,
		State.Error,
		State.Content,
		State.Attribute,
		State.Content,
		State.Attribute,
		State.Epilog,
		State.Prolog,
		State.Prolog,
		State.PostDTD,
		State.Content,
		State.Attribute,
		State.Content,
		State.Attribute,
		State.Epilog,
		State.Prolog,
		State.Prolog,
		State.PostDTD,
		State.Content,
		State.Attribute,
		State.Content,
		State.Attribute,
		State.Epilog
	};

	private static readonly State[] stateTableDocument = new State[104]
	{
		State.Error,
		State.Prolog,
		State.PostDTD,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Epilog,
		State.Error,
		State.PostDTD,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Prolog,
		State.PostDTD,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Epilog,
		State.Error,
		State.Error,
		State.Error,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Error,
		State.Error,
		State.Element,
		State.Element,
		State.Element,
		State.Element,
		State.Element,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Content,
		State.Content,
		State.Content,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Attribute,
		State.Attribute,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Element,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Content,
		State.Attribute,
		State.Content,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Error,
		State.Content,
		State.Attribute,
		State.Content,
		State.Error,
		State.Error,
		State.Error,
		State.Prolog,
		State.PostDTD,
		State.Content,
		State.Attribute,
		State.Content,
		State.Error,
		State.Epilog,
		State.Error,
		State.Prolog,
		State.PostDTD,
		State.Content,
		State.Attribute,
		State.Content,
		State.Error,
		State.Epilog
	};

	/// <summary>Gets the underlying stream object.</summary>
	/// <returns>The stream to which the XmlTextWriter is writing or null if the XmlTextWriter was constructed using a <see cref="T:System.IO.TextWriter" /> that does not inherit from the <see cref="T:System.IO.StreamWriter" /> class.</returns>
	public Stream BaseStream
	{
		get
		{
			if (textWriter is StreamWriter streamWriter)
			{
				return streamWriter.BaseStream;
			}
			return null;
		}
	}

	/// <summary>Gets or sets a value indicating whether to do namespace support.</summary>
	/// <returns>true to support namespaces; otherwise, false.The default is true.</returns>
	/// <exception cref="T:System.InvalidOperationException">You can only change this property when in the WriteState.Start state. </exception>
	public bool Namespaces
	{
		get
		{
			return namespaces;
		}
		set
		{
			if (currentState != 0)
			{
				throw new InvalidOperationException(Res.GetString("NotInWriteState."));
			}
			namespaces = value;
		}
	}

	/// <summary>Indicates how the output is formatted.</summary>
	/// <returns>One of the <see cref="T:System.Xml.Formatting" /> values. The default is Formatting.None (no special formatting).</returns>
	public Formatting Formatting
	{
		get
		{
			return formatting;
		}
		set
		{
			formatting = value;
			indented = value == Formatting.Indented;
		}
	}

	/// <summary>Gets or sets how many IndentChars to write for each level in the hierarchy when <see cref="P:System.Xml.XmlTextWriter.Formatting" /> is set to Formatting.Indented.</summary>
	/// <returns>Number of IndentChars for each level. The default is 2.</returns>
	/// <exception cref="T:System.ArgumentException">Setting this property to a negative value. </exception>
	public int Indentation
	{
		get
		{
			return indentation;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException(Res.GetString("Indentation value must be greater than 0."));
			}
			indentation = value;
		}
	}

	/// <summary>Gets or sets which character to use for indenting when <see cref="P:System.Xml.XmlTextWriter.Formatting" /> is set to Formatting.Indented.</summary>
	/// <returns>The character to use for indenting. The default is space.NoteThe XmlTextWriter allows you to set this property to any character. To ensure valid XML, you must specify a valid white space character, 0x9, 0x10, 0x13 or 0x20.</returns>
	public char IndentChar
	{
		get
		{
			return indentChar;
		}
		set
		{
			indentChar = value;
		}
	}

	/// <summary>Gets or sets which character to use to quote attribute values.</summary>
	/// <returns>The character to use to quote attribute values. This must be a single quote (&amp;#39;) or a double quote (&amp;#34;). The default is a double quote.</returns>
	/// <exception cref="T:System.ArgumentException">Setting this property to something other than either a single or double quote. </exception>
	public char QuoteChar
	{
		get
		{
			return quoteChar;
		}
		set
		{
			if (value != '"' && value != '\'')
			{
				throw new ArgumentException(Res.GetString("Invalid XML attribute quote character. Valid attribute quote characters are ' and \"."));
			}
			quoteChar = value;
			xmlEncoder.QuoteChar = value;
		}
	}

	/// <summary>Gets the state of the writer.</summary>
	/// <returns>One of the <see cref="T:System.Xml.WriteState" /> values.</returns>
	public override WriteState WriteState
	{
		get
		{
			switch (currentState)
			{
			case State.Start:
				return WriteState.Start;
			case State.Prolog:
			case State.PostDTD:
				return WriteState.Prolog;
			case State.Element:
				return WriteState.Element;
			case State.Attribute:
			case State.AttrOnly:
				return WriteState.Attribute;
			case State.Content:
			case State.Epilog:
				return WriteState.Content;
			case State.Error:
				return WriteState.Error;
			case State.Closed:
				return WriteState.Closed;
			default:
				return WriteState.Error;
			}
		}
	}

	/// <summary>Gets an <see cref="T:System.Xml.XmlSpace" /> representing the current xml:space scope.</summary>
	/// <returns>An XmlSpace representing the current xml:space scope.Value Meaning None This is the default if no xml:space scope exists. Default The current scope is xml:space="default". Preserve The current scope is xml:space="preserve". </returns>
	public override XmlSpace XmlSpace
	{
		get
		{
			for (int num = top; num > 0; num--)
			{
				XmlSpace xmlSpace = stack[num].xmlSpace;
				if (xmlSpace != 0)
				{
					return xmlSpace;
				}
			}
			return XmlSpace.None;
		}
	}

	/// <summary>Gets the current xml:lang scope.</summary>
	/// <returns>The current xml:lang or null if there is no xml:lang in the current scope.</returns>
	public override string XmlLang
	{
		get
		{
			for (int num = top; num > 0; num--)
			{
				string xmlLang = stack[num].xmlLang;
				if (xmlLang != null)
				{
					return xmlLang;
				}
			}
			return null;
		}
	}

	internal XmlTextWriter()
	{
		namespaces = true;
		formatting = Formatting.None;
		indentation = 2;
		indentChar = ' ';
		nsStack = new Namespace[8];
		nsTop = -1;
		stack = new TagInfo[10];
		top = 0;
		stack[top].Init(-1);
		quoteChar = '"';
		stateTable = stateTableDefault;
		currentState = State.Start;
		lastToken = Token.Empty;
	}

	/// <summary>Creates an instance of the XmlTextWriter class using the specified stream and encoding.</summary>
	/// <param name="w">The stream to which you want to write. </param>
	/// <param name="encoding">The encoding to generate. If encoding is null it writes out the stream as UTF-8 and omits the encoding attribute from the ProcessingInstruction. </param>
	/// <exception cref="T:System.ArgumentException">The encoding is not supported or the stream cannot be written to. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="w" /> is null. </exception>
	public XmlTextWriter(Stream w, Encoding encoding)
		: this()
	{
		this.encoding = encoding;
		if (encoding != null)
		{
			textWriter = new StreamWriter(w, encoding);
		}
		else
		{
			textWriter = new StreamWriter(w);
		}
		xmlEncoder = new XmlTextEncoder(textWriter);
		xmlEncoder.QuoteChar = quoteChar;
	}

	/// <summary>Creates an instance of the <see cref="T:System.Xml.XmlTextWriter" /> class using the specified file.</summary>
	/// <param name="filename">The filename to write to. If the file exists, it truncates it and overwrites it with the new content. </param>
	/// <param name="encoding">The encoding to generate. If encoding is null it writes the file out as UTF-8, and omits the encoding attribute from the ProcessingInstruction. </param>
	/// <exception cref="T:System.ArgumentException">The encoding is not supported; the filename is empty, contains only white space, or contains one or more invalid characters. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.ArgumentNullException">The filename is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The directory to write to is not found. </exception>
	/// <exception cref="T:System.IO.IOException">The filename includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public XmlTextWriter(string filename, Encoding encoding)
		: this(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read), encoding)
	{
	}

	/// <summary>Creates an instance of the XmlTextWriter class using the specified <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="w">The TextWriter to write to. It is assumed that the TextWriter is already set to the correct encoding. </param>
	public XmlTextWriter(TextWriter w)
		: this()
	{
		textWriter = w;
		encoding = w.Encoding;
		xmlEncoder = new XmlTextEncoder(w);
		xmlEncoder.QuoteChar = quoteChar;
	}

	/// <summary>Writes the XML declaration with the version "1.0".</summary>
	/// <exception cref="T:System.InvalidOperationException">This is not the first write method called after the constructor. </exception>
	public override void WriteStartDocument()
	{
		StartDocument(-1);
	}

	/// <summary>Writes the XML declaration with the version "1.0" and the standalone attribute.</summary>
	/// <param name="standalone">If true, it writes "standalone=yes"; if false, it writes "standalone=no". </param>
	/// <exception cref="T:System.InvalidOperationException">This is not the first write method called after the constructor. </exception>
	public override void WriteStartDocument(bool standalone)
	{
		StartDocument(standalone ? 1 : 0);
	}

	/// <summary>Closes any open elements or attributes and puts the writer back in the Start state.</summary>
	/// <exception cref="T:System.ArgumentException">The XML document is invalid. </exception>
	public override void WriteEndDocument()
	{
		try
		{
			AutoCompleteAll();
			if (currentState != State.Epilog)
			{
				if (currentState == State.Closed)
				{
					throw new ArgumentException(Res.GetString("The Writer is closed or in error state."));
				}
				throw new ArgumentException(Res.GetString("Document does not have a root element."));
			}
			stateTable = stateTableDefault;
			currentState = State.Start;
			lastToken = Token.Empty;
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes the DOCTYPE declaration with the specified name and optional attributes.</summary>
	/// <param name="name">The name of the DOCTYPE. This must be non-empty. </param>
	/// <param name="pubid">If non-null it also writes PUBLIC "pubid" "sysid" where <paramref name="pubid" /> and <paramref name="sysid" /> are replaced with the value of the given arguments. </param>
	/// <param name="sysid">If <paramref name="pubid" /> is null and <paramref name="sysid" /> is non-null it writes SYSTEM "sysid" where <paramref name="sysid" /> is replaced with the value of this argument. </param>
	/// <param name="subset">If non-null it writes [subset] where subset is replaced with the value of this argument. </param>
	/// <exception cref="T:System.InvalidOperationException">This method was called outside the prolog (after the root element). </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is null or String.Empty-or- the value for <paramref name="name" /> would result in invalid XML. </exception>
	public override void WriteDocType(string name, string pubid, string sysid, string subset)
	{
		try
		{
			ValidateName(name, isNCName: false);
			AutoComplete(Token.Doctype);
			textWriter.Write("<!DOCTYPE ");
			textWriter.Write(name);
			if (pubid != null)
			{
				textWriter.Write(" PUBLIC " + quoteChar);
				textWriter.Write(pubid);
				textWriter.Write(quoteChar + " " + quoteChar);
				textWriter.Write(sysid);
				textWriter.Write(quoteChar);
			}
			else if (sysid != null)
			{
				textWriter.Write(" SYSTEM " + quoteChar);
				textWriter.Write(sysid);
				textWriter.Write(quoteChar);
			}
			if (subset != null)
			{
				textWriter.Write("[");
				textWriter.Write(subset);
				textWriter.Write("]");
			}
			textWriter.Write('>');
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes the specified start tag and associates it with the given namespace and prefix.</summary>
	/// <param name="prefix">The namespace prefix of the element. </param>
	/// <param name="localName">The local name of the element. </param>
	/// <param name="ns">The namespace URI to associate with the element. If this namespace is already in scope and has an associated prefix then the writer automatically writes that prefix also. </param>
	/// <exception cref="T:System.InvalidOperationException">The writer is closed. </exception>
	public override void WriteStartElement(string prefix, string localName, string ns)
	{
		try
		{
			AutoComplete(Token.StartElement);
			PushStack();
			textWriter.Write('<');
			if (namespaces)
			{
				stack[top].defaultNs = stack[top - 1].defaultNs;
				if (stack[top - 1].defaultNsState != 0)
				{
					stack[top].defaultNsState = NamespaceState.NotDeclaredButInScope;
				}
				stack[top].mixed = stack[top - 1].mixed;
				if (ns == null)
				{
					if (prefix != null && prefix.Length != 0 && LookupNamespace(prefix) == -1)
					{
						throw new ArgumentException(Res.GetString("An undefined prefix is in use."));
					}
				}
				else if (prefix == null)
				{
					string text = FindPrefix(ns);
					if (text != null)
					{
						prefix = text;
					}
					else
					{
						PushNamespace(null, ns, declared: false);
					}
				}
				else if (prefix.Length == 0)
				{
					PushNamespace(null, ns, declared: false);
				}
				else
				{
					if (ns.Length == 0)
					{
						prefix = null;
					}
					VerifyPrefixXml(prefix, ns);
					PushNamespace(prefix, ns, declared: false);
				}
				stack[top].prefix = null;
				if (prefix != null && prefix.Length != 0)
				{
					stack[top].prefix = prefix;
					textWriter.Write(prefix);
					textWriter.Write(':');
				}
			}
			else if ((ns != null && ns.Length != 0) || (prefix != null && prefix.Length != 0))
			{
				throw new ArgumentException(Res.GetString("Cannot set the namespace if Namespaces is 'false'."));
			}
			stack[top].name = localName;
			textWriter.Write(localName);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Closes one element and pops the corresponding namespace scope.</summary>
	public override void WriteEndElement()
	{
		InternalWriteEndElement(longFormat: false);
	}

	/// <summary>Closes one element and pops the corresponding namespace scope.</summary>
	public override void WriteFullEndElement()
	{
		InternalWriteEndElement(longFormat: true);
	}

	/// <summary>Writes the start of an attribute.</summary>
	/// <param name="prefix">Namespace prefix of the attribute. </param>
	/// <param name="localName">LocalName of the attribute. </param>
	/// <param name="ns">NamespaceURI of the attribute </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="localName" /> is either null or String.Empty. </exception>
	public override void WriteStartAttribute(string prefix, string localName, string ns)
	{
		try
		{
			AutoComplete(Token.StartAttribute);
			specialAttr = SpecialAttr.None;
			if (namespaces)
			{
				if (prefix != null && prefix.Length == 0)
				{
					prefix = null;
				}
				if (ns == "http://www.w3.org/2000/xmlns/" && prefix == null && localName != "xmlns")
				{
					prefix = "xmlns";
				}
				if (prefix == "xml")
				{
					if (localName == "lang")
					{
						specialAttr = SpecialAttr.XmlLang;
					}
					else if (localName == "space")
					{
						specialAttr = SpecialAttr.XmlSpace;
					}
				}
				else if (prefix == "xmlns")
				{
					if ("http://www.w3.org/2000/xmlns/" != ns && ns != null)
					{
						throw new ArgumentException(Res.GetString("The 'xmlns' attribute is bound to the reserved namespace 'http://www.w3.org/2000/xmlns/'."));
					}
					if (localName == null || localName.Length == 0)
					{
						localName = prefix;
						prefix = null;
						prefixForXmlNs = null;
					}
					else
					{
						prefixForXmlNs = localName;
					}
					specialAttr = SpecialAttr.XmlNs;
				}
				else if (prefix == null && localName == "xmlns")
				{
					if ("http://www.w3.org/2000/xmlns/" != ns && ns != null)
					{
						throw new ArgumentException(Res.GetString("The 'xmlns' attribute is bound to the reserved namespace 'http://www.w3.org/2000/xmlns/'."));
					}
					specialAttr = SpecialAttr.XmlNs;
					prefixForXmlNs = null;
				}
				else if (ns == null)
				{
					if (prefix != null && LookupNamespace(prefix) == -1)
					{
						throw new ArgumentException(Res.GetString("An undefined prefix is in use."));
					}
				}
				else if (ns.Length == 0)
				{
					prefix = string.Empty;
				}
				else
				{
					VerifyPrefixXml(prefix, ns);
					if (prefix != null && LookupNamespaceInCurrentScope(prefix) != -1)
					{
						prefix = null;
					}
					string text = FindPrefix(ns);
					if (text != null && (prefix == null || prefix == text))
					{
						prefix = text;
					}
					else
					{
						if (prefix == null)
						{
							prefix = GeneratePrefix();
						}
						PushNamespace(prefix, ns, declared: false);
					}
				}
				if (prefix != null && prefix.Length != 0)
				{
					textWriter.Write(prefix);
					textWriter.Write(':');
				}
			}
			else
			{
				if ((ns != null && ns.Length != 0) || (prefix != null && prefix.Length != 0))
				{
					throw new ArgumentException(Res.GetString("Cannot set the namespace if Namespaces is 'false'."));
				}
				if (localName == "xml:lang")
				{
					specialAttr = SpecialAttr.XmlLang;
				}
				else if (localName == "xml:space")
				{
					specialAttr = SpecialAttr.XmlSpace;
				}
			}
			xmlEncoder.StartAttribute(specialAttr != SpecialAttr.None);
			textWriter.Write(localName);
			textWriter.Write('=');
			if (curQuoteChar != quoteChar)
			{
				curQuoteChar = quoteChar;
				xmlEncoder.QuoteChar = quoteChar;
			}
			textWriter.Write(curQuoteChar);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Closes the previous <see cref="M:System.Xml.XmlTextWriter.WriteStartAttribute(System.String,System.String,System.String)" /> call.</summary>
	public override void WriteEndAttribute()
	{
		try
		{
			AutoComplete(Token.EndAttribute);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes out a &lt;![CDATA[...]]&gt; block containing the specified text.</summary>
	/// <param name="text">Text to place inside the CDATA block. </param>
	/// <exception cref="T:System.ArgumentException">The text would result in a non-well formed XML document. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Xml.XmlTextWriter.WriteState" /> is Closed. </exception>
	public override void WriteCData(string text)
	{
		try
		{
			AutoComplete(Token.CData);
			if (text != null && text.IndexOf("]]>", StringComparison.Ordinal) >= 0)
			{
				throw new ArgumentException(Res.GetString("Cannot have ']]>' inside an XML CDATA block."));
			}
			textWriter.Write("<![CDATA[");
			if (text != null)
			{
				xmlEncoder.WriteRawWithSurrogateChecking(text);
			}
			textWriter.Write("]]>");
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes out a comment &lt;!--...--&gt; containing the specified text.</summary>
	/// <param name="text">Text to place inside the comment. </param>
	/// <exception cref="T:System.ArgumentException">The text would result in a non-well formed XML document </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Xml.XmlTextWriter.WriteState" /> is Closed. </exception>
	public override void WriteComment(string text)
	{
		try
		{
			if (text != null && (text.IndexOf("--", StringComparison.Ordinal) >= 0 || (text.Length != 0 && text[text.Length - 1] == '-')))
			{
				throw new ArgumentException(Res.GetString("An XML comment cannot contain '--', and '-' cannot be the last character."));
			}
			AutoComplete(Token.Comment);
			textWriter.Write("<!--");
			if (text != null)
			{
				xmlEncoder.WriteRawWithSurrogateChecking(text);
			}
			textWriter.Write("-->");
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes out a processing instruction with a space between the name and text as follows: &lt;?name text?&gt;.</summary>
	/// <param name="name">Name of the processing instruction. </param>
	/// <param name="text">Text to include in the processing instruction. </param>
	/// <exception cref="T:System.ArgumentException">The text would result in a non-well formed XML document.<paramref name="name" /> is either null or String.Empty.This method is being used to create an XML declaration after <see cref="M:System.Xml.XmlTextWriter.WriteStartDocument" /> has already been called. </exception>
	public override void WriteProcessingInstruction(string name, string text)
	{
		try
		{
			if (text != null && text.IndexOf("?>", StringComparison.Ordinal) >= 0)
			{
				throw new ArgumentException(Res.GetString("Cannot have '?>' inside an XML processing instruction."));
			}
			if (string.Compare(name, "xml", StringComparison.OrdinalIgnoreCase) == 0 && stateTable == stateTableDocument)
			{
				throw new ArgumentException(Res.GetString("Cannot write XML declaration. WriteStartDocument method has already written it."));
			}
			AutoComplete(Token.PI);
			InternalWriteProcessingInstruction(name, text);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes out an entity reference as &amp;name;.</summary>
	/// <param name="name">Name of the entity reference. </param>
	/// <exception cref="T:System.ArgumentException">The text would result in a non-well formed XML document or <paramref name="name" /> is either null or String.Empty. </exception>
	public override void WriteEntityRef(string name)
	{
		try
		{
			ValidateName(name, isNCName: false);
			AutoComplete(Token.Content);
			xmlEncoder.WriteEntityRef(name);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Forces the generation of a character entity for the specified Unicode character value.</summary>
	/// <param name="ch">Unicode character for which to generate a character entity. </param>
	/// <exception cref="T:System.ArgumentException">The character is in the surrogate pair character range, 0xd800 - 0xdfff; or the text would result in a non-well formed XML document. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Xml.XmlTextWriter.WriteState" /> is Closed. </exception>
	public override void WriteCharEntity(char ch)
	{
		try
		{
			AutoComplete(Token.Content);
			xmlEncoder.WriteCharEntity(ch);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes out the given white space.</summary>
	/// <param name="ws">The string of white space characters. </param>
	/// <exception cref="T:System.ArgumentException">The string contains non-white space characters. </exception>
	public override void WriteWhitespace(string ws)
	{
		try
		{
			if (ws == null)
			{
				ws = string.Empty;
			}
			if (!xmlCharType.IsOnlyWhitespace(ws))
			{
				throw new ArgumentException(Res.GetString("Only white space characters should be used."));
			}
			AutoComplete(Token.Whitespace);
			xmlEncoder.Write(ws);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes the given text content.</summary>
	/// <param name="text">Text to write. </param>
	/// <exception cref="T:System.ArgumentException">The text string contains an invalid surrogate pair. </exception>
	public override void WriteString(string text)
	{
		try
		{
			if (text != null && text.Length != 0)
			{
				AutoComplete(Token.Content);
				xmlEncoder.Write(text);
			}
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Generates and writes the surrogate character entity for the surrogate character pair.</summary>
	/// <param name="lowChar">The low surrogate. This must be a value between 0xDC00 and 0xDFFF. </param>
	/// <param name="highChar">The high surrogate. This must be a value between 0xD800 and 0xDBFF. </param>
	/// <exception cref="T:System.Exception">An invalid surrogate character pair was passed. </exception>
	public override void WriteSurrogateCharEntity(char lowChar, char highChar)
	{
		try
		{
			AutoComplete(Token.Content);
			xmlEncoder.WriteSurrogateCharEntity(lowChar, highChar);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes text one buffer at a time.</summary>
	/// <param name="buffer">Character array containing the text to write. </param>
	/// <param name="index">The position in the buffer indicating the start of the text to write. </param>
	/// <param name="count">The number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero. -or-The buffer length minus <paramref name="index" /> is less than <paramref name="count" />; the call results in surrogate pair characters being split or an invalid surrogate pair being written.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Xml.XmlTextWriter.WriteState" /> is Closed. </exception>
	public override void WriteChars(char[] buffer, int index, int count)
	{
		try
		{
			AutoComplete(Token.Content);
			xmlEncoder.Write(buffer, index, count);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes raw markup manually from a character buffer.</summary>
	/// <param name="buffer">Character array containing the text to write. </param>
	/// <param name="index">The position within the buffer indicating the start of the text to write. </param>
	/// <param name="count">The number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or-The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	public override void WriteRaw(char[] buffer, int index, int count)
	{
		try
		{
			AutoComplete(Token.RawData);
			xmlEncoder.WriteRaw(buffer, index, count);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes raw markup manually from a string.</summary>
	/// <param name="data">String containing the text to write. </param>
	public override void WriteRaw(string data)
	{
		try
		{
			AutoComplete(Token.RawData);
			xmlEncoder.WriteRawWithSurrogateChecking(data);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Encodes the specified binary bytes as base64 and writes out the resulting text.</summary>
	/// <param name="buffer">Byte array to encode. </param>
	/// <param name="index">The position within the buffer indicating the start of the bytes to write. </param>
	/// <param name="count">The number of bytes to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Xml.XmlTextWriter.WriteState" /> is Closed. </exception>
	public override void WriteBase64(byte[] buffer, int index, int count)
	{
		try
		{
			if (!flush)
			{
				AutoComplete(Token.Base64);
			}
			flush = true;
			if (base64Encoder == null)
			{
				base64Encoder = new XmlTextWriterBase64Encoder(xmlEncoder);
			}
			base64Encoder.Encode(buffer, index, count);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Encodes the specified binary bytes as binhex and writes out the resulting text.</summary>
	/// <param name="buffer">Byte array to encode. </param>
	/// <param name="index">The position in the buffer indicating the start of the bytes to write. </param>
	/// <param name="count">The number of bytes to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Xml.XmlTextWriter.WriteState" /> is Closed. </exception>
	public override void WriteBinHex(byte[] buffer, int index, int count)
	{
		try
		{
			AutoComplete(Token.Content);
			BinHexEncoder.Encode(buffer, index, count, this);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Closes this stream and the underlying stream.</summary>
	public override void Close()
	{
		try
		{
			AutoCompleteAll();
		}
		catch
		{
		}
		finally
		{
			currentState = State.Closed;
			textWriter.Close();
		}
	}

	/// <summary>Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.</summary>
	public override void Flush()
	{
		textWriter.Flush();
	}

	/// <summary>Writes out the specified name, ensuring it is a valid name according to the W3C XML 1.0 recommendation (http://www.w3.org/TR/1998/REC-xml-19980210#NT-Name).</summary>
	/// <param name="name">Name to write. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not a valid XML name; or <paramref name="name" /> is either null or String.Empty. </exception>
	public override void WriteName(string name)
	{
		try
		{
			AutoComplete(Token.Content);
			InternalWriteName(name, isNCName: false);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Writes out the namespace-qualified name. This method looks up the prefix that is in scope for the given namespace.</summary>
	/// <param name="localName">The local name to write. </param>
	/// <param name="ns">The namespace URI to associate with the name. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="localName" /> is either null or String.Empty.<paramref name="localName" /> is not a valid name according to the W3C Namespaces spec. </exception>
	public override void WriteQualifiedName(string localName, string ns)
	{
		try
		{
			AutoComplete(Token.Content);
			if (namespaces)
			{
				if (ns != null && ns.Length != 0 && ns != stack[top].defaultNs)
				{
					string text = FindPrefix(ns);
					if (text == null)
					{
						if (currentState != State.Attribute)
						{
							throw new ArgumentException(Res.GetString("The '{0}' namespace is not defined.", ns));
						}
						text = GeneratePrefix();
						PushNamespace(text, ns, declared: false);
					}
					if (text.Length != 0)
					{
						InternalWriteName(text, isNCName: true);
						textWriter.Write(':');
					}
				}
			}
			else if (ns != null && ns.Length != 0)
			{
				throw new ArgumentException(Res.GetString("Cannot set the namespace if Namespaces is 'false'."));
			}
			InternalWriteName(localName, isNCName: true);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	/// <summary>Returns the closest prefix defined in the current namespace scope for the namespace URI.</summary>
	/// <returns>The matching prefix. Or null if no matching namespace URI is found in the current scope.</returns>
	/// <param name="ns">Namespace URI whose prefix you want to find. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ns" /> is either null or String.Empty. </exception>
	public override string LookupPrefix(string ns)
	{
		if (ns == null || ns.Length == 0)
		{
			throw new ArgumentException(Res.GetString("The empty string '' is not a valid name."));
		}
		string text = FindPrefix(ns);
		if (text == null && ns == stack[top].defaultNs)
		{
			text = string.Empty;
		}
		return text;
	}

	/// <summary>Writes out the specified name, ensuring it is a valid NmToken according to the W3C XML 1.0 recommendation (http://www.w3.org/TR/1998/REC-xml-19980210#NT-Name).</summary>
	/// <param name="name">Name to write. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not a valid NmToken; or <paramref name="name" /> is either null or String.Empty. </exception>
	public override void WriteNmToken(string name)
	{
		try
		{
			AutoComplete(Token.Content);
			if (name == null || name.Length == 0)
			{
				throw new ArgumentException(Res.GetString("The empty string '' is not a valid name."));
			}
			if (!ValidateNames.IsNmtokenNoNamespaces(name))
			{
				throw new ArgumentException(Res.GetString("Invalid name character in '{0}'.", name));
			}
			textWriter.Write(name);
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	private void StartDocument(int standalone)
	{
		try
		{
			if (currentState != 0)
			{
				throw new InvalidOperationException(Res.GetString("WriteStartDocument needs to be the first call."));
			}
			stateTable = stateTableDocument;
			currentState = State.Prolog;
			StringBuilder stringBuilder = new StringBuilder(128);
			stringBuilder.Append("version=" + quoteChar + "1.0" + quoteChar);
			if (encoding != null)
			{
				stringBuilder.Append(" encoding=");
				stringBuilder.Append(quoteChar);
				stringBuilder.Append(encoding.WebName);
				stringBuilder.Append(quoteChar);
			}
			if (standalone >= 0)
			{
				stringBuilder.Append(" standalone=");
				stringBuilder.Append(quoteChar);
				stringBuilder.Append((standalone == 0) ? "no" : "yes");
				stringBuilder.Append(quoteChar);
			}
			InternalWriteProcessingInstruction("xml", stringBuilder.ToString());
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	private void AutoComplete(Token token)
	{
		if (currentState == State.Closed)
		{
			throw new InvalidOperationException(Res.GetString("The Writer is closed."));
		}
		if (currentState == State.Error)
		{
			throw new InvalidOperationException(Res.GetString("Token {0} in state {1} would result in an invalid XML document.", tokenName[(int)token], stateName[8]));
		}
		State state = stateTable[(int)((int)token * 8 + currentState)];
		if (state == State.Error)
		{
			throw new InvalidOperationException(Res.GetString("Token {0} in state {1} would result in an invalid XML document.", tokenName[(int)token], stateName[(int)currentState]));
		}
		switch (token)
		{
		case Token.Doctype:
			if (indented && currentState != 0)
			{
				Indent(beforeEndElement: false);
			}
			break;
		case Token.PI:
		case Token.Comment:
		case Token.CData:
		case Token.StartElement:
			if (currentState == State.Attribute)
			{
				WriteEndAttributeQuote();
				WriteEndStartTag(empty: false);
			}
			else if (currentState == State.Element)
			{
				WriteEndStartTag(empty: false);
			}
			if (token == Token.CData)
			{
				stack[top].mixed = true;
			}
			else if (indented && currentState != 0)
			{
				Indent(beforeEndElement: false);
			}
			break;
		case Token.EndElement:
		case Token.LongEndElement:
			if (flush)
			{
				FlushEncoders();
			}
			if (currentState == State.Attribute)
			{
				WriteEndAttributeQuote();
			}
			if (currentState == State.Content)
			{
				token = Token.LongEndElement;
			}
			else
			{
				WriteEndStartTag(token == Token.EndElement);
			}
			if (stateTableDocument == stateTable && top == 1)
			{
				state = State.Epilog;
			}
			break;
		case Token.StartAttribute:
			if (flush)
			{
				FlushEncoders();
			}
			if (currentState == State.Attribute)
			{
				WriteEndAttributeQuote();
				textWriter.Write(' ');
			}
			else if (currentState == State.Element)
			{
				textWriter.Write(' ');
			}
			break;
		case Token.EndAttribute:
			if (flush)
			{
				FlushEncoders();
			}
			WriteEndAttributeQuote();
			break;
		case Token.Content:
		case Token.RawData:
		case Token.Whitespace:
			if (flush)
			{
				FlushEncoders();
			}
			goto case Token.Base64;
		case Token.Base64:
			if (currentState == State.Element && lastToken != Token.Content)
			{
				WriteEndStartTag(empty: false);
			}
			if (state == State.Content)
			{
				stack[top].mixed = true;
			}
			break;
		default:
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current state of the object."));
		}
		currentState = state;
		lastToken = token;
	}

	private void AutoCompleteAll()
	{
		if (flush)
		{
			FlushEncoders();
		}
		while (top > 0)
		{
			WriteEndElement();
		}
	}

	private void InternalWriteEndElement(bool longFormat)
	{
		try
		{
			if (top <= 0)
			{
				throw new InvalidOperationException(Res.GetString("There was no XML start tag open."));
			}
			AutoComplete(longFormat ? Token.LongEndElement : Token.EndElement);
			if (lastToken == Token.LongEndElement)
			{
				if (indented)
				{
					Indent(beforeEndElement: true);
				}
				textWriter.Write('<');
				textWriter.Write('/');
				if (namespaces && stack[top].prefix != null)
				{
					textWriter.Write(stack[top].prefix);
					textWriter.Write(':');
				}
				textWriter.Write(stack[top].name);
				textWriter.Write('>');
			}
			int prevNsTop = stack[top].prevNsTop;
			if (useNsHashtable && prevNsTop < nsTop)
			{
				PopNamespaces(prevNsTop + 1, nsTop);
			}
			nsTop = prevNsTop;
			top--;
		}
		catch
		{
			currentState = State.Error;
			throw;
		}
	}

	private void WriteEndStartTag(bool empty)
	{
		xmlEncoder.StartAttribute(cacheAttrValue: false);
		for (int num = nsTop; num > stack[top].prevNsTop; num--)
		{
			if (!nsStack[num].declared)
			{
				textWriter.Write(" xmlns");
				textWriter.Write(':');
				textWriter.Write(nsStack[num].prefix);
				textWriter.Write('=');
				textWriter.Write(quoteChar);
				xmlEncoder.Write(nsStack[num].ns);
				textWriter.Write(quoteChar);
			}
		}
		if (stack[top].defaultNs != stack[top - 1].defaultNs && stack[top].defaultNsState == NamespaceState.DeclaredButNotWrittenOut)
		{
			textWriter.Write(" xmlns");
			textWriter.Write('=');
			textWriter.Write(quoteChar);
			xmlEncoder.Write(stack[top].defaultNs);
			textWriter.Write(quoteChar);
			stack[top].defaultNsState = NamespaceState.DeclaredAndWrittenOut;
		}
		xmlEncoder.EndAttribute();
		if (empty)
		{
			textWriter.Write(" /");
		}
		textWriter.Write('>');
	}

	private void WriteEndAttributeQuote()
	{
		if (specialAttr != 0)
		{
			HandleSpecialAttribute();
		}
		xmlEncoder.EndAttribute();
		textWriter.Write(curQuoteChar);
	}

	private void Indent(bool beforeEndElement)
	{
		if (top == 0)
		{
			textWriter.WriteLine();
		}
		else if (!stack[top].mixed)
		{
			textWriter.WriteLine();
			int num = (beforeEndElement ? (top - 1) : top);
			for (num *= indentation; num > 0; num--)
			{
				textWriter.Write(indentChar);
			}
		}
	}

	private void PushNamespace(string prefix, string ns, bool declared)
	{
		if ("http://www.w3.org/2000/xmlns/" == ns)
		{
			throw new ArgumentException(Res.GetString("Cannot bind to the reserved namespace."));
		}
		if (prefix == null)
		{
			switch (stack[top].defaultNsState)
			{
			default:
				return;
			case NamespaceState.Uninitialized:
			case NamespaceState.NotDeclaredButInScope:
				stack[top].defaultNs = ns;
				break;
			case NamespaceState.DeclaredButNotWrittenOut:
				break;
			}
			stack[top].defaultNsState = (declared ? NamespaceState.DeclaredAndWrittenOut : NamespaceState.DeclaredButNotWrittenOut);
			return;
		}
		if (prefix.Length != 0 && ns.Length == 0)
		{
			throw new ArgumentException(Res.GetString("Cannot use a prefix with an empty namespace."));
		}
		int num = LookupNamespace(prefix);
		if (num != -1 && nsStack[num].ns == ns)
		{
			if (declared)
			{
				nsStack[num].declared = true;
			}
			return;
		}
		if (declared && num != -1 && num > stack[top].prevNsTop)
		{
			nsStack[num].declared = true;
		}
		AddNamespace(prefix, ns, declared);
	}

	private void AddNamespace(string prefix, string ns, bool declared)
	{
		int num = ++nsTop;
		if (num == nsStack.Length)
		{
			Namespace[] destinationArray = new Namespace[num * 2];
			Array.Copy(nsStack, destinationArray, num);
			nsStack = destinationArray;
		}
		nsStack[num].Set(prefix, ns, declared);
		if (useNsHashtable)
		{
			AddToNamespaceHashtable(num);
		}
		else if (num == 16)
		{
			nsHashtable = new Dictionary<string, int>(new SecureStringHasher());
			for (int i = 0; i <= num; i++)
			{
				AddToNamespaceHashtable(i);
			}
			useNsHashtable = true;
		}
	}

	private void AddToNamespaceHashtable(int namespaceIndex)
	{
		string prefix = nsStack[namespaceIndex].prefix;
		if (nsHashtable.TryGetValue(prefix, out var value))
		{
			nsStack[namespaceIndex].prevNsIndex = value;
		}
		nsHashtable[prefix] = namespaceIndex;
	}

	private void PopNamespaces(int indexFrom, int indexTo)
	{
		for (int num = indexTo; num >= indexFrom; num--)
		{
			if (nsStack[num].prevNsIndex == -1)
			{
				nsHashtable.Remove(nsStack[num].prefix);
			}
			else
			{
				nsHashtable[nsStack[num].prefix] = nsStack[num].prevNsIndex;
			}
		}
	}

	private string GeneratePrefix()
	{
		return string.Concat(str3: (stack[top].prefixCount++ + 1).ToString("d", CultureInfo.InvariantCulture), str0: "d", str1: top.ToString("d", CultureInfo.InvariantCulture), str2: "p");
	}

	private void InternalWriteProcessingInstruction(string name, string text)
	{
		textWriter.Write("<?");
		ValidateName(name, isNCName: false);
		textWriter.Write(name);
		textWriter.Write(' ');
		if (text != null)
		{
			xmlEncoder.WriteRawWithSurrogateChecking(text);
		}
		textWriter.Write("?>");
	}

	private int LookupNamespace(string prefix)
	{
		if (useNsHashtable)
		{
			if (nsHashtable.TryGetValue(prefix, out var value))
			{
				return value;
			}
		}
		else
		{
			for (int num = nsTop; num >= 0; num--)
			{
				if (nsStack[num].prefix == prefix)
				{
					return num;
				}
			}
		}
		return -1;
	}

	private int LookupNamespaceInCurrentScope(string prefix)
	{
		if (useNsHashtable)
		{
			if (nsHashtable.TryGetValue(prefix, out var value) && value > stack[top].prevNsTop)
			{
				return value;
			}
		}
		else
		{
			for (int num = nsTop; num > stack[top].prevNsTop; num--)
			{
				if (nsStack[num].prefix == prefix)
				{
					return num;
				}
			}
		}
		return -1;
	}

	private string FindPrefix(string ns)
	{
		for (int num = nsTop; num >= 0; num--)
		{
			if (nsStack[num].ns == ns && LookupNamespace(nsStack[num].prefix) == num)
			{
				return nsStack[num].prefix;
			}
		}
		return null;
	}

	private void InternalWriteName(string name, bool isNCName)
	{
		ValidateName(name, isNCName);
		textWriter.Write(name);
	}

	private void ValidateName(string name, bool isNCName)
	{
		if (name == null || name.Length == 0)
		{
			throw new ArgumentException(Res.GetString("The empty string '' is not a valid name."));
		}
		int length = name.Length;
		if (namespaces)
		{
			int num = -1;
			int num2 = ValidateNames.ParseNCName(name);
			while (true)
			{
				if (num2 == length)
				{
					return;
				}
				if (name[num2] != ':' || isNCName || num != -1 || num2 <= 0 || num2 + 1 >= length)
				{
					break;
				}
				num = num2;
				num2++;
				num2 += ValidateNames.ParseNmtoken(name, num2);
			}
		}
		else if (ValidateNames.IsNameNoNamespaces(name))
		{
			return;
		}
		throw new ArgumentException(Res.GetString("Invalid name character in '{0}'.", name));
	}

	private void HandleSpecialAttribute()
	{
		string attributeValue = xmlEncoder.AttributeValue;
		switch (specialAttr)
		{
		case SpecialAttr.XmlLang:
			stack[top].xmlLang = attributeValue;
			break;
		case SpecialAttr.XmlSpace:
			attributeValue = XmlConvert.TrimString(attributeValue);
			if (attributeValue == "default")
			{
				stack[top].xmlSpace = XmlSpace.Default;
				break;
			}
			if (attributeValue == "preserve")
			{
				stack[top].xmlSpace = XmlSpace.Preserve;
				break;
			}
			throw new ArgumentException(Res.GetString("'{0}' is an invalid xml:space value.", attributeValue));
		case SpecialAttr.XmlNs:
			VerifyPrefixXml(prefixForXmlNs, attributeValue);
			PushNamespace(prefixForXmlNs, attributeValue, declared: true);
			break;
		}
	}

	private void VerifyPrefixXml(string prefix, string ns)
	{
		if (prefix != null && prefix.Length == 3 && (prefix[0] == 'x' || prefix[0] == 'X') && (prefix[1] == 'm' || prefix[1] == 'M') && (prefix[2] == 'l' || prefix[2] == 'L') && "http://www.w3.org/XML/1998/namespace" != ns)
		{
			throw new ArgumentException(Res.GetString("Prefixes beginning with \"xml\" (regardless of whether the characters are uppercase, lowercase, or some combination thereof) are reserved for use by XML."));
		}
	}

	private void PushStack()
	{
		if (top == stack.Length - 1)
		{
			TagInfo[] destinationArray = new TagInfo[stack.Length + 10];
			if (top > 0)
			{
				Array.Copy(stack, destinationArray, top + 1);
			}
			stack = destinationArray;
		}
		top++;
		stack[top].Init(nsTop);
	}

	private void FlushEncoders()
	{
		if (base64Encoder != null)
		{
			base64Encoder.Flush();
		}
		flush = false;
	}
}
