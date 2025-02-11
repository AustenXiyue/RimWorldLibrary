using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Baml2006;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xaml;
using System.Xaml.Permissions;
using System.Xml;
using MS.Internal;
using MS.Internal.Utility;
using MS.Internal.WindowsBase;
using MS.Internal.Xaml.Context;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Markup;

/// <summary>Reads XAML input and creates an object graph, using the WPF default XAML reader and an associated XAML object writer. </summary>
public class XamlReader
{
	private const int AsyncLoopTimeout = 200;

	private Uri _baseUri;

	private System.Xaml.XamlReader _textReader;

	private XmlReader _xmlReader;

	private XamlObjectWriter _objectWriter;

	private Stream _stream;

	private bool _parseCancelled;

	private Exception _parseException;

	private int _persistId = 1;

	private bool _skipJournaledProperties;

	private MS.Internal.Xaml.Context.XamlContextStack<WpfXamlFrame> _stack;

	private int _maxAsynxRecords = -1;

	private IStyleConnector _styleConnector;

	private static readonly Lazy<WpfSharedBamlSchemaContext> _bamlSharedContext = new Lazy<WpfSharedBamlSchemaContext>(() => CreateBamlSchemaContext());

	private static readonly Lazy<WpfSharedXamlSchemaContext> _xamlSharedContext = new Lazy<WpfSharedXamlSchemaContext>(() => CreateXamlSchemaContext(useV3Rules: false));

	private static readonly Lazy<WpfSharedXamlSchemaContext> _xamlV3SharedContext = new Lazy<WpfSharedXamlSchemaContext>(() => CreateXamlSchemaContext(useV3Rules: true));

	internal static WpfSharedBamlSchemaContext BamlSharedSchemaContext => _bamlSharedContext.Value;

	internal static WpfSharedBamlSchemaContext XamlV3SharedSchemaContext => _xamlV3SharedContext.Value;

	/// <summary>Occurs when an asynchronous load operation completes. </summary>
	public event AsyncCompletedEventHandler LoadCompleted;

	/// <summary>Reads the XAML input in the specified text string and returns an object that corresponds to the root of the specified markup.</summary>
	/// <returns>The root of the created object tree.</returns>
	/// <param name="xamlText">The input XAML, as a single text string.</param>
	public static object Parse(string xamlText)
	{
		return Parse(xamlText, useRestrictiveXamlReader: false);
	}

	public static object Parse(string xamlText, bool useRestrictiveXamlReader)
	{
		return Load(XmlReader.Create(new StringReader(xamlText)), useRestrictiveXamlReader);
	}

	/// <summary>Reads the XAML markup in the specified text string (using a specified <see cref="T:System.Windows.Markup.ParserContext" />) and returns an object that corresponds to the root of the specified markup.</summary>
	/// <returns>The root of the created object tree.</returns>
	/// <param name="xamlText">The input XAML, as a single text string.</param>
	/// <param name="parserContext">Context information used by the parser.</param>
	public static object Parse(string xamlText, ParserContext parserContext)
	{
		return Parse(xamlText, parserContext, useRestrictiveXamlReader: false);
	}

	public static object Parse(string xamlText, ParserContext parserContext, bool useRestrictiveXamlReader)
	{
		return Load(new MemoryStream(Encoding.Default.GetBytes(xamlText)), parserContext, useRestrictiveXamlReader);
	}

	/// <summary>Reads the XAML input in the specified <see cref="T:System.IO.Stream" /> and returns an <see cref="T:System.Object" /> that is the root of the corresponding object tree.</summary>
	/// <returns>The object at the root of the created object tree.</returns>
	/// <param name="stream">The XAML to load, in stream form.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public static object Load(Stream stream)
	{
		return Load(stream, null, useRestrictiveXamlReader: false);
	}

	public static object Load(Stream stream, bool useRestrictiveXamlReader)
	{
		return Load(stream, null, useRestrictiveXamlReader);
	}

	/// <summary>Reads the XAML input in the specified <see cref="T:System.Xml.XmlReader" /> and returns an object that is the root of the corresponding object tree.</summary>
	/// <returns>The object that is the root of the created object tree.</returns>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> that has already loaded the XAML input to load in XML form.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	public static object Load(XmlReader reader)
	{
		return Load(reader, useRestrictiveXamlReader: false);
	}

	public static object Load(XmlReader reader, bool useRestrictiveXamlReader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return Load(reader, null, XamlParseMode.Synchronous, useRestrictiveXamlReader);
	}

	/// <summary>Reads the XAML input in the specified <see cref="T:System.IO.Stream" /> and returns an object that is the root of the corresponding object tree.</summary>
	/// <returns>The object that is the root of the created object tree.</returns>
	/// <param name="stream">The stream that contains the XAML input to load.</param>
	/// <param name="parserContext">Context information used by the parser.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.-or-<paramref name="parserContext" /> is null.</exception>
	public static object Load(Stream stream, ParserContext parserContext)
	{
		return Load(stream, parserContext, useRestrictiveXamlReader: false);
	}

	public static object Load(Stream stream, ParserContext parserContext, bool useRestrictiveXamlReader)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (parserContext == null)
		{
			parserContext = new ParserContext();
		}
		object result = Load(XmlReader.Create(stream, null, parserContext), parserContext, XamlParseMode.Synchronous, useRestrictiveXamlReader || parserContext.FromRestrictiveReader);
		stream.Close();
		return result;
	}

	/// <summary>Reads the XAML input in the specified <see cref="T:System.IO.Stream" /> and returns the root of the corresponding object tree.</summary>
	/// <returns>The object that is the root of the created object tree.</returns>
	/// <param name="stream">The stream containing the XAML input to load.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Multiple load operations are pending concurrently with the same <see cref="T:System.Windows.Markup.XamlReader" />.</exception>
	public object LoadAsync(Stream stream)
	{
		return LoadAsync(stream, useRestrictiveXamlReader: false);
	}

	public object LoadAsync(Stream stream, bool useRestrictiveXamlReader)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		_stream = stream;
		if (_objectWriter != null)
		{
			throw new InvalidOperationException(SR.ParserCannotReuseXamlReader);
		}
		return LoadAsync(stream, null, useRestrictiveXamlReader);
	}

	/// <summary>Reads the XAML input in the specified <see cref="T:System.Xml.XmlReader" /> and returns the root of the corresponding object tree. </summary>
	/// <returns>The root of the created object tree.</returns>
	/// <param name="reader">An existing  <see cref="T:System.Xml.XmlReader" /> that has already loaded/read the XAML input.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Multiple load operations are performed concurrently with the same <see cref="T:System.Windows.Markup.XamlReader" />.</exception>
	public object LoadAsync(XmlReader reader)
	{
		return LoadAsync(reader, null, useRestrictiveXamlReader: false);
	}

	public object LoadAsync(XmlReader reader, bool useRestrictiveXamlReader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return LoadAsync(reader, null, useRestrictiveXamlReader);
	}

	/// <summary>Reads the XAML input in the specified <see cref="T:System.IO.Stream" /> and returns the root of the corresponding object tree. </summary>
	/// <returns>The root of the created object tree.</returns>
	/// <param name="stream">A stream containing the XAML input to load.</param>
	/// <param name="parserContext">Context information used by the parser.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Multiple load operations are performed concurrently with the same <see cref="T:System.Windows.Markup.XamlReader" />.</exception>
	public object LoadAsync(Stream stream, ParserContext parserContext)
	{
		return LoadAsync(stream, parserContext, useRestrictiveXamlReader: false);
	}

	public object LoadAsync(Stream stream, ParserContext parserContext, bool useRestrictiveXamlReader)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		_stream = stream;
		if (_objectWriter != null)
		{
			throw new InvalidOperationException(SR.ParserCannotReuseXamlReader);
		}
		if (parserContext == null)
		{
			parserContext = new ParserContext();
		}
		XmlTextReader xmlTextReader = new XmlTextReader(stream, XmlNodeType.Document, parserContext);
		xmlTextReader.DtdProcessing = DtdProcessing.Prohibit;
		return LoadAsync(xmlTextReader, parserContext, useRestrictiveXamlReader);
	}

	internal static bool ShouldReWrapException(Exception e, Uri baseUri)
	{
		if (e is XamlParseException ex)
		{
			if (ex.BaseUri == null)
			{
				return baseUri != null;
			}
			return false;
		}
		return true;
	}

	private object LoadAsync(XmlReader reader, ParserContext parserContext)
	{
		return LoadAsync(reader, parserContext, useRestrictiveXamlReader: false);
	}

	private object LoadAsync(XmlReader reader, ParserContext parserContext, bool useRestrictiveXamlReader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (parserContext == null)
		{
			parserContext = new ParserContext();
		}
		_xmlReader = reader;
		object rootObject = null;
		if (parserContext.BaseUri == null || string.IsNullOrEmpty(parserContext.BaseUri.ToString()))
		{
			if (reader.BaseURI == null || string.IsNullOrEmpty(reader.BaseURI.ToString()))
			{
				parserContext.BaseUri = BaseUriHelper.PackAppBaseUri;
			}
			else
			{
				parserContext.BaseUri = new Uri(reader.BaseURI);
			}
		}
		_baseUri = parserContext.BaseUri;
		XamlXmlReaderSettings xamlXmlReaderSettings = new XamlXmlReaderSettings();
		xamlXmlReaderSettings.IgnoreUidsOnPropertyElements = true;
		xamlXmlReaderSettings.BaseUri = parserContext.BaseUri;
		xamlXmlReaderSettings.ProvideLineInfo = true;
		XamlSchemaContext schemaContext = ((parserContext.XamlTypeMapper != null) ? parserContext.XamlTypeMapper.SchemaContext : GetWpfSchemaContext());
		try
		{
			_textReader = ((useRestrictiveXamlReader || parserContext.FromRestrictiveReader) ? new RestrictiveXamlXmlReader(reader, schemaContext, xamlXmlReaderSettings) : new XamlXmlReader(reader, schemaContext, xamlXmlReaderSettings));
			_stack = new MS.Internal.Xaml.Context.XamlContextStack<WpfXamlFrame>(() => new WpfXamlFrame());
			XamlObjectWriterSettings xamlObjectWriterSettings = CreateObjectWriterSettings();
			xamlObjectWriterSettings.AfterBeginInitHandler = delegate(object sender, XamlObjectEventArgs args)
			{
				if (rootObject == null)
				{
					rootObject = args.Instance;
					_styleConnector = rootObject as IStyleConnector;
				}
				if (args.Instance is UIElement uIElement)
				{
					XamlReader xamlReader = this;
					int persistId = _persistId;
					xamlReader._persistId = persistId + 1;
					uIElement.SetPersistId(persistId);
				}
				if (args.Instance is DependencyObject dependencyObject && _stack.CurrentFrame.XmlnsDictionary != null)
				{
					XmlnsDictionary xmlnsDictionary = _stack.CurrentFrame.XmlnsDictionary;
					xmlnsDictionary.Seal();
					XmlAttributeProperties.SetXmlnsDictionary(dependencyObject, xmlnsDictionary);
				}
			};
			_objectWriter = new XamlObjectWriter(_textReader.SchemaContext, xamlObjectWriterSettings);
			_parseCancelled = false;
			_skipJournaledProperties = parserContext.SkipJournaledProperties;
			XamlMember xamlDirective = _textReader.SchemaContext.GetXamlDirective("http://schemas.microsoft.com/winfx/2006/xaml", "SynchronousMode");
			XamlMember xamlDirective2 = _textReader.SchemaContext.GetXamlDirective("http://schemas.microsoft.com/winfx/2006/xaml", "AsyncRecords");
			System.Xaml.XamlReader textReader = _textReader;
			IXamlLineInfo xamlLineInfo = textReader as IXamlLineInfo;
			IXamlLineInfoConsumer objectWriter = _objectWriter;
			bool shouldPassLineNumberInfo = false;
			if (xamlLineInfo != null && xamlLineInfo.HasLineInfo && objectWriter != null && objectWriter.ShouldProvideLineInfo)
			{
				shouldPassLineNumberInfo = true;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			while (!_textReader.IsEof)
			{
				WpfXamlLoader.TransformNodes(textReader, _objectWriter, onlyLoadOneNode: true, _skipJournaledProperties, shouldPassLineNumberInfo, xamlLineInfo, objectWriter, _stack, _styleConnector);
				if (textReader.NodeType == System.Xaml.XamlNodeType.StartMember)
				{
					if (textReader.Member == xamlDirective)
					{
						flag2 = true;
					}
					else if (textReader.Member == xamlDirective2)
					{
						flag3 = true;
					}
				}
				else if (textReader.NodeType == System.Xaml.XamlNodeType.Value)
				{
					if (flag2)
					{
						if (textReader.Value as string == "Async")
						{
							flag = true;
						}
					}
					else if (flag3)
					{
						if (textReader.Value is int)
						{
							_maxAsynxRecords = (int)textReader.Value;
						}
						else if (textReader.Value is string)
						{
							_maxAsynxRecords = int.Parse(textReader.Value as string, TypeConverterHelper.InvariantEnglishUS);
						}
					}
				}
				else if (textReader.NodeType == System.Xaml.XamlNodeType.EndMember)
				{
					flag2 = false;
					flag3 = false;
				}
				if (flag && rootObject != null)
				{
					break;
				}
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || !ShouldReWrapException(ex, parserContext.BaseUri))
			{
				throw;
			}
			RewrapException(ex, parserContext.BaseUri);
		}
		if (!_textReader.IsEof)
		{
			Post();
		}
		else
		{
			TreeBuildComplete();
		}
		if (rootObject is DependencyObject)
		{
			if (parserContext.BaseUri != null && !string.IsNullOrEmpty(parserContext.BaseUri.ToString()))
			{
				(rootObject as DependencyObject).SetValue(BaseUriHelper.BaseUriProperty, parserContext.BaseUri);
			}
			WpfXamlLoader.EnsureXmlNamespaceMaps(rootObject, schemaContext);
		}
		if (rootObject is Application application)
		{
			application.ApplicationMarkupBaseUri = GetBaseUri(xamlXmlReaderSettings.BaseUri);
		}
		return rootObject;
	}

	internal static void RewrapException(Exception e, Uri baseUri)
	{
		RewrapException(e, null, baseUri);
	}

	internal static void RewrapException(Exception e, IXamlLineInfo lineInfo, Uri baseUri)
	{
		throw WrapException(e, lineInfo, baseUri);
	}

	internal static XamlParseException WrapException(Exception e, IXamlLineInfo lineInfo, Uri baseUri)
	{
		Exception ex = ((e.InnerException == null) ? e : e.InnerException);
		if (ex is XamlParseException)
		{
			XamlParseException ex2 = (XamlParseException)ex;
			ex2.BaseUri = ex2.BaseUri ?? baseUri;
			if (lineInfo != null && ex2.LinePosition == 0 && ex2.LineNumber == 0)
			{
				ex2.LinePosition = lineInfo.LinePosition;
				ex2.LineNumber = lineInfo.LineNumber;
			}
			return ex2;
		}
		if (e is XamlException)
		{
			XamlException ex3 = (XamlException)e;
			return new XamlParseException(ex3.Message, ex3.LineNumber, ex3.LinePosition, baseUri, ex);
		}
		if (e is XmlException)
		{
			XmlException ex4 = (XmlException)e;
			return new XamlParseException(ex4.Message, ex4.LineNumber, ex4.LinePosition, baseUri, ex);
		}
		if (lineInfo != null)
		{
			return new XamlParseException(e.Message, lineInfo.LineNumber, lineInfo.LinePosition, baseUri, ex);
		}
		return new XamlParseException(e.Message, ex);
	}

	internal void Post()
	{
		Post(DispatcherPriority.Background);
	}

	internal void Post(DispatcherPriority priority)
	{
		DispatcherOperationCallback method = Dispatch;
		Dispatcher.CurrentDispatcher.BeginInvoke(priority, method, this);
	}

	private object Dispatch(object o)
	{
		DispatchParserQueueEvent((XamlReader)o);
		return null;
	}

	private void DispatchParserQueueEvent(XamlReader xamlReader)
	{
		xamlReader.HandleAsyncQueueItem();
	}

	internal virtual void HandleAsyncQueueItem()
	{
		try
		{
			int num = SafeNativeMethods.GetTickCount();
			int num2 = _maxAsynxRecords;
			System.Xaml.XamlReader textReader = _textReader;
			IXamlLineInfo xamlLineInfo = textReader as IXamlLineInfo;
			IXamlLineInfoConsumer objectWriter = _objectWriter;
			bool shouldPassLineNumberInfo = false;
			if (xamlLineInfo != null && xamlLineInfo.HasLineInfo && objectWriter != null && objectWriter.ShouldProvideLineInfo)
			{
				shouldPassLineNumberInfo = true;
			}
			XamlMember xamlDirective = _textReader.SchemaContext.GetXamlDirective("http://schemas.microsoft.com/winfx/2006/xaml", "AsyncRecords");
			while (!textReader.IsEof && !_parseCancelled)
			{
				WpfXamlLoader.TransformNodes(textReader, _objectWriter, onlyLoadOneNode: true, _skipJournaledProperties, shouldPassLineNumberInfo, xamlLineInfo, objectWriter, _stack, _styleConnector);
				if (textReader.NodeType == System.Xaml.XamlNodeType.Value && _stack.CurrentFrame.Property == xamlDirective)
				{
					if (textReader.Value is int)
					{
						_maxAsynxRecords = (int)textReader.Value;
					}
					else if (textReader.Value is string)
					{
						_maxAsynxRecords = int.Parse(textReader.Value as string, TypeConverterHelper.InvariantEnglishUS);
					}
					num2 = _maxAsynxRecords;
				}
				int num3 = SafeNativeMethods.GetTickCount() - num;
				if (num3 < 0)
				{
					num = 0;
				}
				else if (num3 > 200)
				{
					break;
				}
				if (--num2 == 0)
				{
					break;
				}
			}
		}
		catch (XamlParseException parseException)
		{
			_parseException = parseException;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || !ShouldReWrapException(ex, _baseUri))
			{
				_parseException = ex;
			}
			else
			{
				_parseException = WrapException(ex, null, _baseUri);
			}
		}
		finally
		{
			if (_parseException != null || _parseCancelled)
			{
				TreeBuildComplete();
			}
			else if (!_textReader.IsEof)
			{
				Post();
			}
			else
			{
				TreeBuildComplete();
			}
		}
	}

	internal void TreeBuildComplete()
	{
		if (this.LoadCompleted != null)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
			{
				this.LoadCompleted(this, new AsyncCompletedEventArgs(_parseException, _parseCancelled, null));
				return (object)null;
			}, null);
		}
		_xmlReader.Close();
		_objectWriter = null;
		_stream = null;
		_textReader = null;
		_stack = null;
	}

	/// <summary>Aborts the current asynchronous load operation, if there is an asynchronous load operation pending.</summary>
	public void CancelAsync()
	{
		_parseCancelled = true;
	}

	internal static XamlObjectWriterSettings CreateObjectWriterSettings()
	{
		return new XamlObjectWriterSettings
		{
			IgnoreCanConvert = true,
			PreferUnconvertedDictionaryKeys = true
		};
	}

	internal static XamlObjectWriterSettings CreateObjectWriterSettings(XamlObjectWriterSettings parentSettings)
	{
		XamlObjectWriterSettings xamlObjectWriterSettings = CreateObjectWriterSettings();
		if (parentSettings != null)
		{
			xamlObjectWriterSettings.SkipDuplicatePropertyCheck = parentSettings.SkipDuplicatePropertyCheck;
			xamlObjectWriterSettings.AccessLevel = parentSettings.AccessLevel;
			xamlObjectWriterSettings.SkipProvideValueOnRoot = parentSettings.SkipProvideValueOnRoot;
			xamlObjectWriterSettings.SourceBamlUri = parentSettings.SourceBamlUri;
		}
		return xamlObjectWriterSettings;
	}

	internal static XamlObjectWriterSettings CreateObjectWriterSettingsForBaml()
	{
		XamlObjectWriterSettings xamlObjectWriterSettings = CreateObjectWriterSettings();
		xamlObjectWriterSettings.SkipDuplicatePropertyCheck = true;
		return xamlObjectWriterSettings;
	}

	internal static Baml2006ReaderSettings CreateBamlReaderSettings()
	{
		return new Baml2006ReaderSettings
		{
			IgnoreUidsOnPropertyElements = true
		};
	}

	internal static XamlSchemaContextSettings CreateSchemaContextSettings()
	{
		return new XamlSchemaContextSettings
		{
			SupportMarkupExtensionsWithDuplicateArity = true
		};
	}

	/// <summary>Returns a <see cref="T:System.Xaml.XamlSchemaContext" /> object that represents the WPF schema context settings for a <see cref="T:System.Windows.Markup.XamlReader" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.XamlSchemaContext" /> object that represents the WPF schema context settings for a <see cref="T:System.Windows.Markup.XamlReader" />.</returns>
	public static XamlSchemaContext GetWpfSchemaContext()
	{
		return _xamlSharedContext.Value;
	}

	internal static object Load(XmlReader reader, ParserContext parserContext, XamlParseMode parseMode)
	{
		return Load(reader, parserContext, parseMode, useRestrictiveXamlReader: false);
	}

	internal static object Load(XmlReader reader, ParserContext parserContext, XamlParseMode parseMode, bool useRestrictiveXamlReader)
	{
		return Load(reader, parserContext, parseMode, useRestrictiveXamlReader, null);
	}

	internal static object Load(XmlReader reader, ParserContext parserContext, XamlParseMode parseMode, bool useRestrictiveXamlReader, List<Type> safeTypes)
	{
		if (parseMode == XamlParseMode.Uninitialized || parseMode == XamlParseMode.Asynchronous)
		{
			return new XamlReader().LoadAsync(reader, parserContext, useRestrictiveXamlReader);
		}
		if (parserContext == null)
		{
			parserContext = new ParserContext();
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Event.WClientParseXmlBegin, parserContext.BaseUri);
		if (TraceMarkup.IsEnabled)
		{
			TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.Load);
		}
		object obj = null;
		try
		{
			if (parserContext.BaseUri == null || string.IsNullOrEmpty(parserContext.BaseUri.ToString()))
			{
				if (reader.BaseURI == null || string.IsNullOrEmpty(reader.BaseURI.ToString()))
				{
					parserContext.BaseUri = BaseUriHelper.PackAppBaseUri;
				}
				else
				{
					parserContext.BaseUri = new Uri(reader.BaseURI);
				}
			}
			XamlXmlReaderSettings xamlXmlReaderSettings = new XamlXmlReaderSettings();
			xamlXmlReaderSettings.IgnoreUidsOnPropertyElements = true;
			xamlXmlReaderSettings.BaseUri = parserContext.BaseUri;
			xamlXmlReaderSettings.ProvideLineInfo = true;
			XamlSchemaContext schemaContext = ((parserContext.XamlTypeMapper != null) ? parserContext.XamlTypeMapper.SchemaContext : GetWpfSchemaContext());
			obj = Load((useRestrictiveXamlReader || parserContext.FromRestrictiveReader) ? new RestrictiveXamlXmlReader(reader, schemaContext, xamlXmlReaderSettings, safeTypes) : new XamlXmlReader(reader, schemaContext, xamlXmlReaderSettings), parserContext);
			reader.Close();
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || !ShouldReWrapException(ex, parserContext.BaseUri))
			{
				throw;
			}
			RewrapException(ex, parserContext.BaseUri);
		}
		finally
		{
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.Load, obj);
			}
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Event.WClientParseXmlEnd, parserContext.BaseUri);
		}
		return obj;
	}

	internal static object Load(System.Xaml.XamlReader xamlReader, ParserContext parserContext)
	{
		if (parserContext == null)
		{
			parserContext = new ParserContext();
		}
		MS.Internal.WindowsBase.SecurityHelper.RunClassConstructor(typeof(Application));
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Event.WClientParseXamlBegin, parserContext.BaseUri);
		object obj = WpfXamlLoader.Load(xamlReader, parserContext.SkipJournaledProperties, parserContext.BaseUri);
		if (obj is DependencyObject dependencyObject && parserContext.BaseUri != null && !string.IsNullOrEmpty(parserContext.BaseUri.ToString()))
		{
			dependencyObject.SetValue(BaseUriHelper.BaseUriProperty, parserContext.BaseUri);
		}
		if (obj is Application application)
		{
			application.ApplicationMarkupBaseUri = GetBaseUri(parserContext.BaseUri);
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Event.WClientParseXamlEnd, parserContext.BaseUri);
		return obj;
	}

	/// <summary>Reads the XAML input through a provided <see cref="T:System.Xaml.XamlReader" /> and returns an object that is the root of the corresponding object tree.</summary>
	/// <returns>The object that is the root of the created object tree.</returns>
	/// <param name="reader">A <see cref="T:System.Xaml.XamlReader" /> object. This is expected to be initialized with input XAML.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	public static object Load(System.Xaml.XamlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		object obj = null;
		try
		{
			obj = Load(reader, null);
		}
		catch (Exception ex)
		{
			Uri baseUri = ((reader is IUriContext uriContext) ? uriContext.BaseUri : null);
			if (CriticalExceptions.IsCriticalException(ex) || !ShouldReWrapException(ex, baseUri))
			{
				throw;
			}
			RewrapException(ex, baseUri);
		}
		finally
		{
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.Load, obj);
			}
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Event.WClientParseXmlEnd);
		}
		return obj;
	}

	internal static object LoadBaml(Stream stream, ParserContext parserContext, object parent, bool closeStream)
	{
		object obj = null;
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Event.WClientParseBamlBegin, parserContext.BaseUri);
		if (TraceMarkup.IsEnabled)
		{
			TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.Load);
		}
		try
		{
			IStreamInfo streamInfo = stream as IStreamInfo;
			if (streamInfo != null)
			{
				parserContext.StreamCreatedAssembly = streamInfo.Assembly;
			}
			Baml2006ReaderSettings baml2006ReaderSettings = CreateBamlReaderSettings();
			baml2006ReaderSettings.BaseUri = parserContext.BaseUri;
			baml2006ReaderSettings.LocalAssembly = streamInfo.Assembly;
			if (baml2006ReaderSettings.BaseUri == null || string.IsNullOrEmpty(baml2006ReaderSettings.BaseUri.ToString()))
			{
				baml2006ReaderSettings.BaseUri = BaseUriHelper.PackAppBaseUri;
			}
			Baml2006ReaderInternal xamlReader = new Baml2006ReaderInternal(stream, new Baml2006SchemaContext(baml2006ReaderSettings.LocalAssembly), baml2006ReaderSettings, parent);
			Type type = null;
			if (streamInfo.Assembly != null)
			{
				try
				{
					type = XamlTypeMapper.GetInternalTypeHelperTypeFromAssembly(parserContext);
				}
				catch (Exception ex)
				{
					if (CriticalExceptions.IsCriticalException(ex))
					{
						throw;
					}
				}
			}
			if (type != null)
			{
				XamlAccessLevel accessLevel = XamlAccessLevel.AssemblyAccessTo(streamInfo.Assembly);
				obj = WpfXamlLoader.LoadBaml(xamlReader, parserContext.SkipJournaledProperties, parent, accessLevel, parserContext.BaseUri);
			}
			else
			{
				obj = WpfXamlLoader.LoadBaml(xamlReader, parserContext.SkipJournaledProperties, parent, null, parserContext.BaseUri);
			}
			if (obj is DependencyObject dependencyObject)
			{
				dependencyObject.SetValue(BaseUriHelper.BaseUriProperty, baml2006ReaderSettings.BaseUri);
			}
			if (obj is Application application)
			{
				application.ApplicationMarkupBaseUri = GetBaseUri(baml2006ReaderSettings.BaseUri);
			}
		}
		finally
		{
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.Load, obj);
			}
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Event.WClientParseBamlEnd, parserContext.BaseUri);
			if (closeStream)
			{
				stream?.Close();
			}
		}
		return obj;
	}

	private static Uri GetBaseUri(Uri uri)
	{
		if (uri == null)
		{
			return MS.Internal.Utility.BindUriHelper.BaseUri;
		}
		if (!uri.IsAbsoluteUri)
		{
			return new Uri(MS.Internal.Utility.BindUriHelper.BaseUri, uri);
		}
		return uri;
	}

	private static WpfSharedBamlSchemaContext CreateBamlSchemaContext()
	{
		return new WpfSharedBamlSchemaContext(new XamlSchemaContextSettings
		{
			SupportMarkupExtensionsWithDuplicateArity = true
		});
	}

	private static WpfSharedXamlSchemaContext CreateXamlSchemaContext(bool useV3Rules)
	{
		return new WpfSharedXamlSchemaContext(new XamlSchemaContextSettings
		{
			SupportMarkupExtensionsWithDuplicateArity = true
		}, useV3Rules);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlReader" /> class.</summary>
	public XamlReader()
	{
	}
}
