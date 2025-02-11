using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal sealed class DocumentSequenceTextView : TextViewBase
{
	private readonly FixedDocumentSequenceDocumentPage _docPage;

	private ITextView _childTextView;

	private ReadOnlyCollection<TextSegment> _textSegments;

	private ChildDocumentBlock _childBlock;

	internal override UIElement RenderScope
	{
		get
		{
			Visual visual = _docPage.Visual;
			while (visual != null && !(visual is UIElement))
			{
				visual = VisualTreeHelper.GetParent(visual) as Visual;
			}
			return visual as UIElement;
		}
	}

	internal override ITextContainer TextContainer => _docPage.FixedDocumentSequence.TextContainer;

	internal override bool IsValid
	{
		get
		{
			if (ChildTextView != null)
			{
				return ChildTextView.IsValid;
			}
			return true;
		}
	}

	internal override bool RendersOwnSelection => true;

	internal override ReadOnlyCollection<TextSegment> TextSegments
	{
		get
		{
			if (_textSegments == null)
			{
				ReadOnlyCollection<TextSegment> textSegments = ChildTextView.TextSegments;
				if (textSegments != null)
				{
					List<TextSegment> list = new List<TextSegment>(textSegments.Count);
					foreach (TextSegment item in textSegments)
					{
						DocumentSequenceTextPointer startPosition = _docPage.FixedDocumentSequence.TextContainer.MapChildPositionToParent(item.Start);
						DocumentSequenceTextPointer endPosition = _docPage.FixedDocumentSequence.TextContainer.MapChildPositionToParent(item.End);
						list.Add(new TextSegment(startPosition, endPosition, preserveLogicalDirection: true));
					}
					_textSegments = new ReadOnlyCollection<TextSegment>(list);
				}
			}
			return _textSegments;
		}
	}

	private ITextView ChildTextView
	{
		get
		{
			if (_childTextView == null && _docPage.ChildDocumentPage is IServiceProvider serviceProvider)
			{
				_childTextView = (ITextView)serviceProvider.GetService(typeof(ITextView));
			}
			return _childTextView;
		}
	}

	private ChildDocumentBlock ChildBlock
	{
		get
		{
			if (_childBlock == null)
			{
				_childBlock = _docPage.FixedDocumentSequence.TextContainer.FindChildBlock(_docPage.ChildDocumentReference);
			}
			return _childBlock;
		}
	}

	private DocumentSequenceTextContainer DocumentSequenceTextContainer => _docPage.FixedDocumentSequence.TextContainer;

	internal DocumentSequenceTextView(FixedDocumentSequenceDocumentPage docPage)
	{
		_docPage = docPage;
	}

	internal override ITextPointer GetTextPositionFromPoint(Point point, bool snapToText)
	{
		DocumentSequenceTextPointer documentSequenceTextPointer = null;
		LogicalDirection gravity = LogicalDirection.Forward;
		if (ChildTextView != null)
		{
			ITextPointer textPositionFromPoint = ChildTextView.GetTextPositionFromPoint(point, snapToText);
			if (textPositionFromPoint != null)
			{
				documentSequenceTextPointer = new DocumentSequenceTextPointer(ChildBlock, textPositionFromPoint);
				gravity = textPositionFromPoint.LogicalDirection;
			}
		}
		if (documentSequenceTextPointer != null)
		{
			return DocumentSequenceTextPointer.CreatePointer(documentSequenceTextPointer, gravity);
		}
		return null;
	}

	internal override Rect GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform)
	{
		DocumentSequenceTextPointer documentSequenceTextPointer = null;
		transform = Transform.Identity;
		if (position != null)
		{
			documentSequenceTextPointer = _docPage.FixedDocumentSequence.TextContainer.VerifyPosition(position);
		}
		if (documentSequenceTextPointer != null && ChildTextView != null && ChildTextView.TextContainer == documentSequenceTextPointer.ChildBlock.ChildContainer)
		{
			return ChildTextView.GetRawRectangleFromTextPosition(documentSequenceTextPointer.ChildPointer.CreatePointer(position.LogicalDirection), out transform);
		}
		return Rect.Empty;
	}

	internal override Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		if (startPosition != null && endPosition != null && ChildTextView != null)
		{
			DocumentSequenceTextPointer documentSequenceTextPointer = null;
			DocumentSequenceTextPointer documentSequenceTextPointer2 = null;
			documentSequenceTextPointer = _docPage.FixedDocumentSequence.TextContainer.VerifyPosition(startPosition);
			documentSequenceTextPointer2 = _docPage.FixedDocumentSequence.TextContainer.VerifyPosition(endPosition);
			if (documentSequenceTextPointer != null && documentSequenceTextPointer2 != null)
			{
				return ChildTextView.GetTightBoundingGeometryFromTextPositions(documentSequenceTextPointer.ChildPointer, documentSequenceTextPointer2.ChildPointer);
			}
		}
		return new PathGeometry();
	}

	internal override ITextPointer GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved)
	{
		newSuggestedX = suggestedX;
		linesMoved = count;
		DocumentSequenceTextPointer thisTp = null;
		LogicalDirection gravity = LogicalDirection.Forward;
		DocumentSequenceTextPointer documentSequenceTextPointer = null;
		if (position != null)
		{
			documentSequenceTextPointer = _docPage.FixedDocumentSequence.TextContainer.VerifyPosition(position);
		}
		if (documentSequenceTextPointer != null && ChildTextView != null && ChildTextView.TextContainer == documentSequenceTextPointer.ChildBlock.ChildContainer)
		{
			ITextPointer positionAtNextLine = ChildTextView.GetPositionAtNextLine(documentSequenceTextPointer.ChildPointer.CreatePointer(position.LogicalDirection), suggestedX, count, out newSuggestedX, out linesMoved);
			if (positionAtNextLine != null)
			{
				thisTp = new DocumentSequenceTextPointer(ChildBlock, positionAtNextLine);
				gravity = positionAtNextLine.LogicalDirection;
			}
		}
		return DocumentSequenceTextPointer.CreatePointer(thisTp, gravity);
	}

	internal override bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		Invariant.Assert(position != null);
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		Invariant.Assert(ChildTextView != null);
		DocumentSequenceTextPointer documentSequenceTextPointer = DocumentSequenceTextContainer.VerifyPosition(position);
		return ChildTextView.IsAtCaretUnitBoundary(documentSequenceTextPointer.ChildPointer);
	}

	internal override ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		Invariant.Assert(position != null);
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		Invariant.Assert(ChildTextView != null);
		DocumentSequenceTextPointer documentSequenceTextPointer = DocumentSequenceTextContainer.VerifyPosition(position);
		return ChildTextView.GetNextCaretUnitPosition(documentSequenceTextPointer.ChildPointer, direction);
	}

	internal override ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		Invariant.Assert(position != null);
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		Invariant.Assert(ChildTextView != null);
		DocumentSequenceTextPointer documentSequenceTextPointer = DocumentSequenceTextContainer.VerifyPosition(position);
		return ChildTextView.GetBackspaceCaretUnitPosition(documentSequenceTextPointer.ChildPointer);
	}

	internal override TextSegment GetLineRange(ITextPointer position)
	{
		DocumentSequenceTextPointer documentSequenceTextPointer = null;
		DocumentSequenceTextPointer documentSequenceTextPointer2 = null;
		if (position != null && ChildTextView != null)
		{
			documentSequenceTextPointer2 = _docPage.FixedDocumentSequence.TextContainer.VerifyPosition(position);
			if (ChildTextView.TextContainer == documentSequenceTextPointer2.ChildBlock.ChildContainer)
			{
				TextSegment lineRange = ChildTextView.GetLineRange(documentSequenceTextPointer2.ChildPointer.CreatePointer(position.LogicalDirection));
				if (!lineRange.IsNull)
				{
					DocumentSequenceTextPointer startPosition = new DocumentSequenceTextPointer(ChildBlock, lineRange.Start);
					documentSequenceTextPointer = new DocumentSequenceTextPointer(ChildBlock, lineRange.End);
					return new TextSegment(startPosition, documentSequenceTextPointer, preserveLogicalDirection: true);
				}
			}
		}
		return TextSegment.Null;
	}

	internal override ReadOnlyCollection<GlyphRun> GetGlyphRuns(ITextPointer start, ITextPointer end)
	{
		throw new NotImplementedException();
	}

	internal override bool Contains(ITextPointer position)
	{
		DocumentSequenceTextPointer documentSequenceTextPointer = null;
		if (position != null)
		{
			documentSequenceTextPointer = _docPage.FixedDocumentSequence.TextContainer.VerifyPosition(position);
		}
		if (documentSequenceTextPointer != null && ChildTextView != null && ChildTextView.TextContainer == documentSequenceTextPointer.ChildBlock.ChildContainer)
		{
			return ChildTextView.Contains(documentSequenceTextPointer.ChildPointer.CreatePointer(position.LogicalDirection));
		}
		return false;
	}

	internal override bool Validate()
	{
		if (ChildTextView != null)
		{
			ChildTextView.Validate();
		}
		return ((ITextView)this).IsValid;
	}

	internal override bool Validate(Point point)
	{
		if (ChildTextView != null)
		{
			ChildTextView.Validate(point);
		}
		return ((ITextView)this).IsValid;
	}
}
