using System.Windows.Media;

namespace System.Windows.Documents;

internal sealed class FixedDocumentSequenceDocumentPage : DocumentPage, IServiceProvider
{
	private readonly FixedDocumentSequence _fixedDocumentSequence;

	private readonly DynamicDocumentPaginator _documentPaginator;

	private readonly DocumentPage _documentPage;

	private bool _layedOut;

	private DocumentSequenceTextView _textView;

	public override Visual Visual
	{
		get
		{
			if (!_layedOut)
			{
				_layedOut = true;
				if (base.Visual is UIElement uIElement)
				{
					uIElement.Measure(base.Size);
					uIElement.Arrange(new Rect(base.Size));
				}
			}
			return base.Visual;
		}
	}

	internal ContentPosition ContentPosition
	{
		get
		{
			if (_documentPaginator.GetPagePosition(_documentPage) is ITextPointer childPosition)
			{
				return new DocumentSequenceTextPointer(new ChildDocumentBlock(_fixedDocumentSequence.TextContainer, ChildDocumentReference), childPosition);
			}
			return null;
		}
	}

	internal DocumentReference ChildDocumentReference
	{
		get
		{
			foreach (DocumentReference reference in _fixedDocumentSequence.References)
			{
				if (reference.CurrentlyLoadedDoc == _documentPaginator.Source)
				{
					return reference;
				}
			}
			return null;
		}
	}

	internal DocumentPage ChildDocumentPage => _documentPage;

	internal FixedDocumentSequence FixedDocumentSequence => _fixedDocumentSequence;

	internal FixedDocumentSequenceDocumentPage(FixedDocumentSequence documentSequence, DynamicDocumentPaginator documentPaginator, DocumentPage documentPage)
		: base((documentPage is FixedDocumentPage) ? ((FixedDocumentPage)documentPage).FixedPage : documentPage.Visual, documentPage.Size, documentPage.BleedBox, documentPage.ContentBox)
	{
		_fixedDocumentSequence = documentSequence;
		_documentPaginator = documentPaginator;
		_documentPage = documentPage;
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(ITextView))
		{
			if (_textView == null)
			{
				_textView = new DocumentSequenceTextView(this);
			}
			return _textView;
		}
		return null;
	}
}
