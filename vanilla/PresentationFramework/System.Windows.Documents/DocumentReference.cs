using System.IO;
using System.Windows.Markup;
using System.Windows.Navigation;
using MS.Internal;
using MS.Internal.Utility;

namespace System.Windows.Documents;

/// <summary>Provides access to reference a <see cref="T:System.Windows.Documents.FixedDocument" />.</summary>
[UsableDuringInitialization(false)]
public sealed class DocumentReference : FrameworkElement, IUriContext
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.DocumentReference.Source" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.DocumentReference.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(DocumentReference), new FrameworkPropertyMetadata(null, OnSourceChanged));

	private FixedDocument _doc;

	private FixedDocument _docIdentity;

	/// <summary>Gets or sets the uniform resource identifier (URI) for this document reference. </summary>
	/// <returns>A <see cref="T:System.Uri" /> representing the document reference.</returns>
	public Uri Source
	{
		get
		{
			return (Uri)GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Windows.Markup.IUriContext.BaseUri" />.</summary>
	/// <returns>The base URI of the current context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return (Uri)GetValue(BaseUriHelper.BaseUriProperty);
		}
		set
		{
			SetValue(BaseUriHelper.BaseUriProperty, value);
		}
	}

	internal FixedDocument CurrentlyLoadedDoc => _docIdentity;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentReference" /> class.</summary>
	public DocumentReference()
	{
		_Init();
	}

	/// <summary>Synchronously loads and parses the document specified by the <see cref="P:System.Windows.Documents.DocumentReference.Source" /> property location.</summary>
	/// <returns>The document that was loaded.</returns>
	/// <param name="forceReload">true to force a new load of the <see cref="P:System.Windows.Documents.DocumentReference.Source" /> document, even if it was previously loaded.</param>
	public FixedDocument GetDocument(bool forceReload)
	{
		VerifyAccess();
		FixedDocument fixedDocument = null;
		if (_doc != null)
		{
			fixedDocument = _doc;
		}
		else
		{
			if (!forceReload)
			{
				fixedDocument = CurrentlyLoadedDoc;
			}
			if (fixedDocument == null)
			{
				FixedDocument fixedDocument2 = _LoadDocument();
				if (fixedDocument2 != null)
				{
					_docIdentity = fixedDocument2;
					fixedDocument = fixedDocument2;
				}
			}
		}
		if (fixedDocument != null)
		{
			LogicalTreeHelper.AddLogicalChild(base.Parent, fixedDocument);
		}
		return fixedDocument;
	}

	/// <summary>Attaches a <see cref="T:System.Windows.Documents.FixedDocument" /> to the <see cref="T:System.Windows.Documents.DocumentReference" />.</summary>
	/// <param name="doc">The document that is attached.</param>
	public void SetDocument(FixedDocument doc)
	{
		VerifyAccess();
		_docIdentity = null;
		_doc = doc;
	}

	private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentReference documentReference = (DocumentReference)d;
		if (!object.Equals(e.OldValue, e.NewValue))
		{
			_ = (Uri)e.OldValue;
			_ = (Uri)e.NewValue;
			documentReference._doc = null;
		}
	}

	private void _Init()
	{
		base.InheritanceBehavior = InheritanceBehavior.SkipToAppNow;
	}

	private Uri _ResolveUri()
	{
		Uri uri = Source;
		if (uri != null)
		{
			uri = MS.Internal.Utility.BindUriHelper.GetUriToNavigate(this, ((IUriContext)this).BaseUri, uri);
		}
		return uri;
	}

	private FixedDocument _LoadDocument()
	{
		FixedDocument fixedDocument = null;
		Uri uri = _ResolveUri();
		if (uri != null)
		{
			ContentType contentType = null;
			Stream stream = null;
			stream = WpfWebRequestHelper.CreateRequestAndGetResponseStream(uri, out contentType);
			if (stream == null)
			{
				throw new ApplicationException(SR.DocumentReferenceNotFound);
			}
			ParserContext parserContext = new ParserContext();
			parserContext.BaseUri = uri;
			if (MS.Internal.Utility.BindUriHelper.IsXamlMimeType(contentType))
			{
				fixedDocument = new XpsValidatingLoader().Load(stream, ((IUriContext)this).BaseUri, parserContext, contentType) as FixedDocument;
			}
			else
			{
				if (!MimeTypeMapper.BamlMime.AreTypeAndSubTypeEqual(contentType))
				{
					throw new ApplicationException(SR.DocumentReferenceUnsupportedMimeType);
				}
				fixedDocument = XamlReader.LoadBaml(stream, parserContext, null, closeStream: true) as FixedDocument;
			}
			fixedDocument.DocumentReference = this;
		}
		return fixedDocument;
	}
}
