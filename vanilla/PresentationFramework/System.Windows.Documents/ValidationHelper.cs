using System.ComponentModel;
using MS.Internal;

namespace System.Windows.Documents;

internal static class ValidationHelper
{
	internal static void VerifyPosition(ITextContainer tree, ITextPointer position)
	{
		VerifyPosition(tree, position, "position");
	}

	internal static void VerifyPosition(ITextContainer container, ITextPointer position, string paramName)
	{
		if (position == null)
		{
			throw new ArgumentNullException(paramName);
		}
		if (position.TextContainer != container)
		{
			throw new ArgumentException(SR.Format(SR.NotInAssociatedTree, paramName));
		}
	}

	internal static void VerifyPositionPair(ITextPointer startPosition, ITextPointer endPosition)
	{
		if (startPosition == null)
		{
			throw new ArgumentNullException("startPosition");
		}
		if (endPosition == null)
		{
			throw new ArgumentNullException("endPosition");
		}
		if (startPosition.TextContainer != endPosition.TextContainer)
		{
			throw new ArgumentException(SR.Format(SR.InDifferentTextContainers, "startPosition", "endPosition"));
		}
		if (startPosition.CompareTo(endPosition) > 0)
		{
			throw new ArgumentException(SR.Format(SR.BadTextPositionOrder, "startPosition", "endPosition"));
		}
	}

	internal static void VerifyDirection(LogicalDirection direction, string argumentName)
	{
		if (direction != LogicalDirection.Forward && direction != 0)
		{
			throw new InvalidEnumArgumentException(argumentName, (int)direction, typeof(LogicalDirection));
		}
	}

	internal static void VerifyElementEdge(ElementEdge edge, string param)
	{
		if (edge != ElementEdge.BeforeStart && edge != ElementEdge.AfterStart && edge != ElementEdge.BeforeEnd && edge != ElementEdge.AfterEnd)
		{
			throw new InvalidEnumArgumentException(param, (int)edge, typeof(ElementEdge));
		}
	}

	internal static void ValidateChild(TextPointer position, object child, string paramName)
	{
		Invariant.Assert(position != null);
		if (child == null)
		{
			throw new ArgumentNullException(paramName);
		}
		if (!TextSchema.IsValidChild(position, child.GetType()))
		{
			throw new ArgumentException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, position.Parent.GetType().Name, child.GetType().Name));
		}
		if (child is TextElement)
		{
			if (((TextElement)child).Parent != null)
			{
				throw new ArgumentException(SR.Format(SR.TextSchema_TheChildElementBelongsToAnotherTreeAlready, child.GetType().Name));
			}
		}
		else
		{
			Invariant.Assert(child is UIElement);
		}
	}
}
