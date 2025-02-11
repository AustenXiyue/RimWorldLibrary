using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.Documents;

namespace MS.Internal.Automation;

internal class TextRangeAdaptor : ITextRangeProvider
{
	private class TextAttributeHelper
	{
		internal delegate object GetValueAtDelegate(ITextPointer textPointer);

		internal delegate bool AreEqualDelegate(object val1, object val2);

		private GetValueAtDelegate _getValueDelegate;

		private AreEqualDelegate _areEqualDelegate;

		internal GetValueAtDelegate GetValueAt => _getValueDelegate;

		internal AreEqualDelegate AreEqual => _areEqualDelegate;

		internal TextAttributeHelper(GetValueAtDelegate getValueDelegate, AreEqualDelegate areEqualDelegate)
		{
			_getValueDelegate = getValueDelegate;
			_areEqualDelegate = areEqualDelegate;
		}
	}

	private ITextPointer _start;

	private ITextPointer _end;

	private TextAdaptor _textAdaptor;

	private AutomationPeer _textPeer;

	private static Hashtable _textPatternAttributes;

	private const string _defaultFamilyName = "Global User Interface";

	static TextRangeAdaptor()
	{
		_textPatternAttributes = new Hashtable();
		_textPatternAttributes.Add(TextPatternIdentifiers.AnimationStyleAttribute, new TextAttributeHelper((ITextPointer tp) => (tp.GetValue(TextElement.TextEffectsProperty) is TextEffectCollection { Count: >0 }) ? AnimationStyle.Other : AnimationStyle.None, (object val1, object val2) => (AnimationStyle)val1 == (AnimationStyle)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.BackgroundColorAttribute, new TextAttributeHelper((ITextPointer tp) => ColorFromBrush(tp.GetValue(TextElement.BackgroundProperty)), (object val1, object val2) => (int)val1 == (int)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.BulletStyleAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			object value = tp.GetValue(List.MarkerStyleProperty);
			return (value is TextMarkerStyle) ? ((TextMarkerStyle)value switch
			{
				TextMarkerStyle.None => BulletStyle.None, 
				TextMarkerStyle.Disc => BulletStyle.FilledRoundBullet, 
				TextMarkerStyle.Circle => BulletStyle.HollowRoundBullet, 
				TextMarkerStyle.Square => BulletStyle.HollowSquareBullet, 
				TextMarkerStyle.Box => BulletStyle.FilledSquareBullet, 
				_ => BulletStyle.Other, 
			}) : ((object)BulletStyle.None);
		}, (object val1, object val2) => (BulletStyle)val1 == (BulletStyle)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.CapStyleAttribute, new TextAttributeHelper((ITextPointer tp) => (FontCapitals)tp.GetValue(Typography.CapitalsProperty) switch
		{
			FontCapitals.Normal => CapStyle.None, 
			FontCapitals.AllSmallCaps => CapStyle.AllCap, 
			FontCapitals.SmallCaps => CapStyle.SmallCap, 
			FontCapitals.AllPetiteCaps => CapStyle.AllPetiteCaps, 
			FontCapitals.PetiteCaps => CapStyle.PetiteCaps, 
			FontCapitals.Unicase => CapStyle.Unicase, 
			FontCapitals.Titling => CapStyle.Titling, 
			_ => CapStyle.Other, 
		}, (object val1, object val2) => (CapStyle)val1 == (CapStyle)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.CultureAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			object value2 = tp.GetValue(FrameworkElement.LanguageProperty);
			return (value2 is XmlLanguage) ? ((XmlLanguage)value2).GetEquivalentCulture().LCID : CultureInfo.InvariantCulture.LCID;
		}, (object val1, object val2) => (int)val1 == (int)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.FontNameAttribute, new TextAttributeHelper((ITextPointer tp) => GetFontFamilyName((FontFamily)tp.GetValue(TextElement.FontFamilyProperty), tp), (object val1, object val2) => val1 as string == val2 as string));
		_textPatternAttributes.Add(TextPatternIdentifiers.FontSizeAttribute, new TextAttributeHelper((ITextPointer tp) => NativeObjectLengthToPoints((double)tp.GetValue(TextElement.FontSizeProperty)), (object val1, object val2) => (double)val1 == (double)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.FontWeightAttribute, new TextAttributeHelper((ITextPointer tp) => ((FontWeight)tp.GetValue(TextElement.FontWeightProperty)).ToOpenTypeWeight(), (object val1, object val2) => (int)val1 == (int)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.ForegroundColorAttribute, new TextAttributeHelper((ITextPointer tp) => ColorFromBrush(tp.GetValue(TextElement.ForegroundProperty)), (object val1, object val2) => (int)val1 == (int)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.HorizontalTextAlignmentAttribute, new TextAttributeHelper((ITextPointer tp) => (TextAlignment)tp.GetValue(Block.TextAlignmentProperty) switch
		{
			TextAlignment.Right => HorizontalTextAlignment.Right, 
			TextAlignment.Center => HorizontalTextAlignment.Centered, 
			TextAlignment.Justify => HorizontalTextAlignment.Justified, 
			_ => HorizontalTextAlignment.Left, 
		}, (object val1, object val2) => (HorizontalTextAlignment)val1 == (HorizontalTextAlignment)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.IndentationFirstLineAttribute, new TextAttributeHelper((ITextPointer tp) => NativeObjectLengthToPoints((double)tp.GetValue(Paragraph.TextIndentProperty)), (object val1, object val2) => (double)val1 == (double)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.IndentationLeadingAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			Thickness thickness = (Thickness)tp.GetValue(Block.PaddingProperty);
			return thickness.IsValid(allowNegative: true, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false) ? NativeObjectLengthToPoints(thickness.Left) : 0.0;
		}, (object val1, object val2) => (double)val1 == (double)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.IndentationTrailingAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			Thickness thickness2 = (Thickness)tp.GetValue(Block.PaddingProperty);
			return thickness2.IsValid(allowNegative: true, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false) ? NativeObjectLengthToPoints(thickness2.Right) : 0.0;
		}, (object val1, object val2) => (double)val1 == (double)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.IsHiddenAttribute, new TextAttributeHelper((ITextPointer tp) => false, (object val1, object val2) => (bool)val1 == (bool)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.IsItalicAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			FontStyle fontStyle = (FontStyle)tp.GetValue(TextElement.FontStyleProperty);
			return fontStyle == FontStyles.Italic || fontStyle == FontStyles.Oblique;
		}, (object val1, object val2) => (bool)val1 == (bool)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.IsReadOnlyAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			bool flag = false;
			if (tp.TextContainer.TextSelection != null)
			{
				flag = tp.TextContainer.TextSelection.TextEditor.IsReadOnly;
			}
			return flag;
		}, (object val1, object val2) => (bool)val1 == (bool)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.IsSubscriptAttribute, new TextAttributeHelper((ITextPointer tp) => (FontVariants)tp.GetValue(Typography.VariantsProperty) == FontVariants.Subscript, (object val1, object val2) => (bool)val1 == (bool)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.IsSuperscriptAttribute, new TextAttributeHelper((ITextPointer tp) => (FontVariants)tp.GetValue(Typography.VariantsProperty) == FontVariants.Superscript, (object val1, object val2) => (bool)val1 == (bool)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.MarginBottomAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			Thickness thickness3 = (Thickness)tp.GetValue(FrameworkElement.MarginProperty);
			return thickness3.IsValid(allowNegative: true, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false) ? NativeObjectLengthToPoints(thickness3.Bottom) : 0.0;
		}, (object val1, object val2) => (double)val1 == (double)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.MarginLeadingAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			Thickness thickness4 = (Thickness)tp.GetValue(FrameworkElement.MarginProperty);
			return thickness4.IsValid(allowNegative: true, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false) ? NativeObjectLengthToPoints(thickness4.Left) : 0.0;
		}, (object val1, object val2) => (double)val1 == (double)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.MarginTopAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			Thickness thickness5 = (Thickness)tp.GetValue(FrameworkElement.MarginProperty);
			return thickness5.IsValid(allowNegative: true, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false) ? NativeObjectLengthToPoints(thickness5.Top) : 0.0;
		}, (object val1, object val2) => (double)val1 == (double)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.MarginTrailingAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			Thickness thickness6 = (Thickness)tp.GetValue(FrameworkElement.MarginProperty);
			return thickness6.IsValid(allowNegative: true, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false) ? NativeObjectLengthToPoints(thickness6.Right) : 0.0;
		}, (object val1, object val2) => (double)val1 == (double)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.OutlineStylesAttribute, new TextAttributeHelper((ITextPointer tp) => OutlineStyles.None, (object val1, object val2) => (OutlineStyles)val1 == (OutlineStyles)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.OverlineColorAttribute, new TextAttributeHelper((ITextPointer tp) => GetTextDecorationColor(tp.GetValue(Inline.TextDecorationsProperty) as TextDecorationCollection, TextDecorationLocation.OverLine), (object val1, object val2) => (int)val1 == (int)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.OverlineStyleAttribute, new TextAttributeHelper((ITextPointer tp) => GetTextDecorationLineStyle(tp.GetValue(Inline.TextDecorationsProperty) as TextDecorationCollection, TextDecorationLocation.OverLine), (object val1, object val2) => (TextDecorationLineStyle)val1 == (TextDecorationLineStyle)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.StrikethroughColorAttribute, new TextAttributeHelper((ITextPointer tp) => GetTextDecorationColor(tp.GetValue(Inline.TextDecorationsProperty) as TextDecorationCollection, TextDecorationLocation.Strikethrough), (object val1, object val2) => (int)val1 == (int)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.StrikethroughStyleAttribute, new TextAttributeHelper((ITextPointer tp) => GetTextDecorationLineStyle(tp.GetValue(Inline.TextDecorationsProperty) as TextDecorationCollection, TextDecorationLocation.Strikethrough), (object val1, object val2) => (TextDecorationLineStyle)val1 == (TextDecorationLineStyle)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.TextFlowDirectionsAttribute, new TextAttributeHelper(delegate(ITextPointer tp)
		{
			FlowDirection flowDirection = (FlowDirection)tp.GetValue(FrameworkElement.FlowDirectionProperty);
			FlowDirections flowDirections = ((flowDirection != 0 && flowDirection == FlowDirection.RightToLeft) ? FlowDirections.RightToLeft : FlowDirections.Default);
			return flowDirections;
		}, (object val1, object val2) => (FlowDirections)val1 == (FlowDirections)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.UnderlineColorAttribute, new TextAttributeHelper((ITextPointer tp) => GetTextDecorationColor(tp.GetValue(Inline.TextDecorationsProperty) as TextDecorationCollection, TextDecorationLocation.Underline), (object val1, object val2) => (int)val1 == (int)val2));
		_textPatternAttributes.Add(TextPatternIdentifiers.UnderlineStyleAttribute, new TextAttributeHelper((ITextPointer tp) => GetTextDecorationLineStyle(tp.GetValue(Inline.TextDecorationsProperty) as TextDecorationCollection, TextDecorationLocation.Underline), (object val1, object val2) => (TextDecorationLineStyle)val1 == (TextDecorationLineStyle)val2));
	}

	internal TextRangeAdaptor(TextAdaptor textAdaptor, ITextPointer start, ITextPointer end, AutomationPeer textPeer)
	{
		Invariant.Assert(textAdaptor != null, "Invalid textAdaptor.");
		Invariant.Assert(textPeer != null, "Invalid textPeer.");
		Invariant.Assert(start != null && end != null, "Invalid range.");
		Invariant.Assert(start.CompareTo(end) <= 0, "Invalid range, end < start.");
		_textAdaptor = textAdaptor;
		_start = start.CreatePointer();
		_end = end.CreatePointer();
		_textPeer = textPeer;
	}

	internal static bool MoveToInsertionPosition(ITextPointer position, LogicalDirection direction)
	{
		if (!position.TextContainer.IsReadOnly || (!TextPointerBase.IsAtNonMergeableInlineStart(position) && !TextPointerBase.IsAtNonMergeableInlineEnd(position)))
		{
			return position.MoveToInsertionPosition(direction);
		}
		return false;
	}

	private TextRangeAdaptor ValidateAndThrow(ITextRangeProvider range)
	{
		if (!(range is TextRangeAdaptor textRangeAdaptor) || textRangeAdaptor._start.TextContainer != _start.TextContainer)
		{
			throw new ArgumentException(SR.TextRangeProvider_WrongTextRange);
		}
		return textRangeAdaptor;
	}

	private void ExpandToEnclosingUnit(TextUnit unit, bool expandStart, bool expandEnd)
	{
		switch (unit)
		{
		case TextUnit.Character:
			if (expandStart && !TextPointerBase.IsAtInsertionPosition(_start))
			{
				TextPointerBase.MoveToNextInsertionPosition(_start, LogicalDirection.Backward);
			}
			if (expandEnd && !TextPointerBase.IsAtInsertionPosition(_end))
			{
				TextPointerBase.MoveToNextInsertionPosition(_end, LogicalDirection.Forward);
			}
			break;
		case TextUnit.Word:
			if (expandStart && !IsAtWordBoundary(_start))
			{
				MoveToNextWordBoundary(_start, LogicalDirection.Backward);
			}
			if (expandEnd && !IsAtWordBoundary(_end))
			{
				MoveToNextWordBoundary(_end, LogicalDirection.Forward);
			}
			break;
		case TextUnit.Format:
			if (expandStart)
			{
				TextPointerContext textPointerContext = _start.GetPointerContext(LogicalDirection.Forward);
				while (true)
				{
					TextPointerContext pointerContext = _start.GetPointerContext(LogicalDirection.Backward);
					if (pointerContext == TextPointerContext.None || (textPointerContext == TextPointerContext.Text && pointerContext != TextPointerContext.Text))
					{
						break;
					}
					textPointerContext = pointerContext;
					_start.MoveToNextContextPosition(LogicalDirection.Backward);
				}
			}
			if (expandEnd)
			{
				TextPointerContext textPointerContext2 = _end.GetPointerContext(LogicalDirection.Backward);
				while (true)
				{
					TextPointerContext pointerContext2 = _end.GetPointerContext(LogicalDirection.Forward);
					switch (pointerContext2)
					{
					case TextPointerContext.Text:
						if (textPointerContext2 == TextPointerContext.Text)
						{
							goto IL_010b;
						}
						break;
					default:
						goto IL_010b;
					case TextPointerContext.None:
						break;
					}
					break;
					IL_010b:
					textPointerContext2 = pointerContext2;
					_end.MoveToNextContextPosition(LogicalDirection.Forward);
				}
			}
			_start.SetLogicalDirection(LogicalDirection.Forward);
			_end.SetLogicalDirection(LogicalDirection.Forward);
			break;
		case TextUnit.Line:
		{
			ITextView updatedTextView = _textAdaptor.GetUpdatedTextView();
			if (updatedTextView == null || !updatedTextView.IsValid)
			{
				break;
			}
			bool flag = true;
			if (expandStart && updatedTextView.Contains(_start))
			{
				TextSegment lineRange = updatedTextView.GetLineRange(_start);
				if (!lineRange.IsNull)
				{
					if (_start.CompareTo(lineRange.Start) != 0)
					{
						_start = lineRange.Start.CreatePointer();
					}
					if (lineRange.Contains(_end))
					{
						flag = false;
						if (_end.CompareTo(lineRange.End) != 0)
						{
							_end = lineRange.End.CreatePointer();
						}
					}
				}
			}
			if (expandEnd && flag && updatedTextView.Contains(_end))
			{
				TextSegment lineRange2 = updatedTextView.GetLineRange(_end);
				if (!lineRange2.IsNull && _end.CompareTo(lineRange2.End) != 0)
				{
					_end = lineRange2.End.CreatePointer();
				}
			}
			break;
		}
		case TextUnit.Paragraph:
		{
			ITextRange textRange = new TextRange(_start, _end);
			TextRangeBase.SelectParagraph(textRange, _start);
			if (expandStart && _start.CompareTo(textRange.Start) != 0)
			{
				_start = textRange.Start.CreatePointer();
			}
			if (expandEnd)
			{
				if (!textRange.Contains(_end))
				{
					TextRangeBase.SelectParagraph(textRange, _end);
				}
				if (_end.CompareTo(textRange.End) != 0)
				{
					_end = textRange.End.CreatePointer();
				}
			}
			break;
		}
		case TextUnit.Page:
		{
			ITextView updatedTextView = _textAdaptor.GetUpdatedTextView();
			if (updatedTextView == null || !updatedTextView.IsValid)
			{
				break;
			}
			if (expandStart && updatedTextView.Contains(_start))
			{
				ITextView textView = updatedTextView;
				if (updatedTextView is MultiPageTextView)
				{
					textView = ((MultiPageTextView)updatedTextView).GetPageTextViewFromPosition(_start);
				}
				ReadOnlyCollection<TextSegment> textSegments = textView.TextSegments;
				if (textSegments != null && textSegments.Count > 0 && _start.CompareTo(textSegments[0].Start) != 0)
				{
					_start = textSegments[0].Start.CreatePointer();
				}
			}
			if (expandEnd && updatedTextView.Contains(_end))
			{
				ITextView textView2 = updatedTextView;
				if (updatedTextView is MultiPageTextView)
				{
					textView2 = ((MultiPageTextView)updatedTextView).GetPageTextViewFromPosition(_end);
				}
				ReadOnlyCollection<TextSegment> textSegments2 = textView2.TextSegments;
				if (textSegments2 != null && textSegments2.Count > 0 && _end.CompareTo(textSegments2[textSegments2.Count - 1].End) != 0)
				{
					_end = textSegments2[textSegments2.Count - 1].End.CreatePointer();
				}
			}
			break;
		}
		case TextUnit.Document:
			if (expandStart && _start.CompareTo(_start.TextContainer.Start) != 0)
			{
				_start = _start.TextContainer.Start.CreatePointer();
			}
			if (expandEnd && _end.CompareTo(_start.TextContainer.End) != 0)
			{
				_end = _start.TextContainer.End.CreatePointer();
			}
			break;
		}
	}

	private bool MoveToUnitBoundary(ITextPointer position, bool isStart, LogicalDirection direction, TextUnit unit)
	{
		bool flag = false;
		switch (unit)
		{
		case TextUnit.Character:
			if (!TextPointerBase.IsAtInsertionPosition(position) && TextPointerBase.MoveToNextInsertionPosition(position, direction))
			{
				flag = true;
			}
			break;
		case TextUnit.Word:
			if (!IsAtWordBoundary(position) && MoveToNextWordBoundary(position, direction))
			{
				flag = true;
			}
			break;
		case TextUnit.Format:
			while (position.GetPointerContext(direction) == TextPointerContext.Text)
			{
				if (position.MoveToNextContextPosition(direction))
				{
					flag = true;
				}
			}
			if (!flag || direction != LogicalDirection.Forward)
			{
				break;
			}
			while (true)
			{
				TextPointerContext pointerContext = position.GetPointerContext(LogicalDirection.Forward);
				if (pointerContext != TextPointerContext.ElementStart && pointerContext != TextPointerContext.ElementEnd)
				{
					break;
				}
				position.MoveToNextContextPosition(LogicalDirection.Forward);
			}
			break;
		case TextUnit.Line:
		{
			ITextView updatedTextView = _textAdaptor.GetUpdatedTextView();
			if (updatedTextView == null || !updatedTextView.IsValid || !updatedTextView.Contains(position))
			{
				break;
			}
			TextSegment lineRange = updatedTextView.GetLineRange(position);
			if (lineRange.IsNull)
			{
				break;
			}
			int linesMoved = 0;
			double newSuggestedX;
			if (direction == LogicalDirection.Forward)
			{
				ITextPointer position2 = null;
				if (isStart)
				{
					position2 = updatedTextView.GetPositionAtNextLine(lineRange.End, double.NaN, 1, out newSuggestedX, out linesMoved);
				}
				position2 = ((linesMoved == 0) ? lineRange.End : updatedTextView.GetLineRange(position2).Start);
				position2 = GetInsertionPosition(position2, LogicalDirection.Forward);
				if (position.CompareTo(position2) != 0)
				{
					position.MoveToPosition(position2);
					position.SetLogicalDirection(isStart ? LogicalDirection.Forward : LogicalDirection.Backward);
					flag = true;
				}
			}
			else
			{
				ITextPointer position3 = null;
				if (!isStart)
				{
					position3 = updatedTextView.GetPositionAtNextLine(lineRange.Start, double.NaN, -1, out newSuggestedX, out linesMoved);
				}
				position3 = ((linesMoved == 0) ? lineRange.Start : updatedTextView.GetLineRange(position3).End);
				position3 = GetInsertionPosition(position3, LogicalDirection.Backward);
				if (position.CompareTo(position3) != 0)
				{
					position.MoveToPosition(position3);
					position.SetLogicalDirection(isStart ? LogicalDirection.Forward : LogicalDirection.Backward);
					flag = true;
				}
			}
			break;
		}
		case TextUnit.Paragraph:
		{
			ITextRange textRange = new TextRange(position, position);
			TextRangeBase.SelectParagraph(textRange, position);
			if (direction == LogicalDirection.Forward)
			{
				ITextPointer textPointer = textRange.End;
				if (isStart)
				{
					textPointer = textPointer.CreatePointer();
					if (textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward))
					{
						TextRangeBase.SelectParagraph(textRange, textPointer);
						textPointer = textRange.Start;
					}
				}
				if (position.CompareTo(textPointer) != 0)
				{
					position.MoveToPosition(textPointer);
					position.SetLogicalDirection(isStart ? LogicalDirection.Forward : LogicalDirection.Backward);
					flag = true;
				}
				break;
			}
			ITextPointer textPointer2 = textRange.Start;
			if (!isStart)
			{
				textPointer2 = textPointer2.CreatePointer();
				if (textPointer2.MoveToNextInsertionPosition(LogicalDirection.Backward))
				{
					TextRangeBase.SelectParagraph(textRange, textPointer2);
					textPointer2 = textRange.End;
				}
			}
			if (position.CompareTo(textPointer2) != 0)
			{
				position.MoveToPosition(textPointer2);
				position.SetLogicalDirection(isStart ? LogicalDirection.Forward : LogicalDirection.Backward);
				flag = true;
			}
			break;
		}
		case TextUnit.Page:
		{
			ITextView updatedTextView = _textAdaptor.GetUpdatedTextView();
			if (updatedTextView == null || !updatedTextView.IsValid || !updatedTextView.Contains(position))
			{
				break;
			}
			ITextView textView = updatedTextView;
			if (updatedTextView is MultiPageTextView)
			{
				textView = ((MultiPageTextView)updatedTextView).GetPageTextViewFromPosition(position);
			}
			ReadOnlyCollection<TextSegment> textSegments = textView.TextSegments;
			if (textSegments == null || textSegments.Count <= 0)
			{
				break;
			}
			if (direction == LogicalDirection.Forward)
			{
				while (position.CompareTo(textSegments[textSegments.Count - 1].End) != 0)
				{
					if (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementEnd)
					{
						position.MoveToPosition(textSegments[textSegments.Count - 1].End);
						flag = true;
						break;
					}
					Invariant.Assert(position.MoveToNextContextPosition(LogicalDirection.Forward));
				}
				MoveToInsertionPosition(position, LogicalDirection.Forward);
				break;
			}
			while (position.CompareTo(textSegments[0].Start) != 0)
			{
				if (position.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.ElementStart)
				{
					position.MoveToPosition(textSegments[0].Start);
					flag = true;
					break;
				}
				Invariant.Assert(position.MoveToNextContextPosition(LogicalDirection.Backward));
			}
			MoveToInsertionPosition(position, LogicalDirection.Backward);
			break;
		}
		case TextUnit.Document:
			if (direction == LogicalDirection.Forward)
			{
				if (position.CompareTo(GetInsertionPosition(position.TextContainer.End, LogicalDirection.Backward)) != 0)
				{
					position.MoveToPosition(position.TextContainer.End);
					flag = true;
				}
			}
			else if (position.CompareTo(GetInsertionPosition(position.TextContainer.Start, LogicalDirection.Forward)) != 0)
			{
				position.MoveToPosition(position.TextContainer.Start);
				flag = true;
			}
			break;
		}
		return flag;
	}

	private int MovePositionByUnits(ITextPointer position, TextUnit unit, int count)
	{
		int i = 0;
		int num = ((count == int.MinValue) ? int.MaxValue : Math.Abs(count));
		LogicalDirection logicalDirection = ((count > 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
		switch (unit)
		{
		case TextUnit.Character:
			for (; i < num; i++)
			{
				if (!TextPointerBase.MoveToNextInsertionPosition(position, logicalDirection))
				{
					break;
				}
			}
			break;
		case TextUnit.Word:
			for (; i < num; i++)
			{
				if (!MoveToNextWordBoundary(position, logicalDirection))
				{
					break;
				}
			}
			break;
		case TextUnit.Format:
			for (; i < num; i++)
			{
				ITextPointer position2 = position.CreatePointer();
				while (position.GetPointerContext(logicalDirection) == TextPointerContext.Text && position.MoveToNextContextPosition(logicalDirection))
				{
				}
				if (!position.MoveToNextContextPosition(logicalDirection))
				{
					break;
				}
				while (position.GetPointerContext(logicalDirection) != TextPointerContext.Text && position.MoveToNextContextPosition(logicalDirection))
				{
				}
				if (logicalDirection == LogicalDirection.Backward)
				{
					while (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text && position.MoveToNextContextPosition(LogicalDirection.Backward))
					{
					}
				}
				if (position.GetPointerContext(logicalDirection) == TextPointerContext.None)
				{
					position.MoveToPosition(position2);
					break;
				}
			}
			position.SetLogicalDirection(LogicalDirection.Forward);
			break;
		case TextUnit.Line:
		{
			ITextView updatedTextView = _textAdaptor.GetUpdatedTextView();
			if (updatedTextView != null && updatedTextView.IsValid && updatedTextView.Contains(position))
			{
				if (TextPointerBase.IsAtRowEnd(position))
				{
					position.MoveToNextInsertionPosition(LogicalDirection.Backward);
				}
				i = position.MoveToLineBoundary(count);
				MoveToInsertionPosition(position, LogicalDirection.Forward);
				if (i < 0)
				{
					i = -i;
				}
			}
			break;
		}
		case TextUnit.Paragraph:
		{
			ITextRange textRange = new TextRange(position, position);
			textRange.SelectParagraph(position);
			while (i < num)
			{
				position.MoveToPosition((logicalDirection == LogicalDirection.Forward) ? textRange.End : textRange.Start);
				if (!position.MoveToNextInsertionPosition(logicalDirection))
				{
					break;
				}
				i++;
				textRange.SelectParagraph(position);
				position.MoveToPosition(textRange.Start);
			}
			break;
		}
		case TextUnit.Page:
		{
			ITextView updatedTextView = _textAdaptor.GetUpdatedTextView();
			if (updatedTextView == null || !updatedTextView.IsValid || !updatedTextView.Contains(position) || !(updatedTextView is MultiPageTextView))
			{
				break;
			}
			ReadOnlyCollection<TextSegment> textSegments = ((MultiPageTextView)updatedTextView).GetPageTextViewFromPosition(position).TextSegments;
			for (; i < num; i++)
			{
				if (textSegments == null)
				{
					break;
				}
				if (textSegments.Count == 0)
				{
					break;
				}
				if (logicalDirection == LogicalDirection.Backward)
				{
					position.MoveToPosition(textSegments[0].Start);
					MoveToInsertionPosition(position, LogicalDirection.Backward);
				}
				else
				{
					position.MoveToPosition(textSegments[textSegments.Count - 1].End);
					MoveToInsertionPosition(position, LogicalDirection.Forward);
				}
				ITextPointer textPointer = position.CreatePointer();
				if (!textPointer.MoveToNextInsertionPosition(logicalDirection))
				{
					break;
				}
				if (logicalDirection == LogicalDirection.Forward)
				{
					if (textPointer.CompareTo(position) <= 0)
					{
						break;
					}
				}
				else if (textPointer.CompareTo(position) >= 0)
				{
					break;
				}
				if (!updatedTextView.Contains(textPointer))
				{
					break;
				}
				textSegments = ((MultiPageTextView)updatedTextView).GetPageTextViewFromPosition(textPointer).TextSegments;
			}
			break;
		}
		}
		if (logicalDirection != LogicalDirection.Forward)
		{
			return -i;
		}
		return i;
	}

	private object GetAttributeValue(TextAttributeHelper attr)
	{
		ITextPointer textPointer = _start.CreatePointer();
		ITextPointer textPointer2 = _end.CreatePointer();
		if (textPointer.CompareTo(textPointer2) < 0)
		{
			while (IsElementBoundary(textPointer.GetPointerContext(LogicalDirection.Forward)) && textPointer.MoveToNextContextPosition(LogicalDirection.Forward) && textPointer.CompareTo(textPointer2) < 0)
			{
			}
			while (IsElementBoundary(textPointer2.GetPointerContext(LogicalDirection.Backward)) && textPointer2.MoveToNextContextPosition(LogicalDirection.Backward) && textPointer.CompareTo(textPointer2) < 0)
			{
			}
			if (textPointer.CompareTo(textPointer2) > 0)
			{
				return AutomationElementIdentifiers.NotSupported;
			}
		}
		object obj = attr.GetValueAt(textPointer2);
		while (textPointer.CompareTo(textPointer2) < 0 && attr.AreEqual(obj, attr.GetValueAt(textPointer)) && textPointer.MoveToNextContextPosition(LogicalDirection.Forward) && textPointer.CompareTo(textPointer2) <= 0)
		{
		}
		if (textPointer.CompareTo(textPointer2) < 0)
		{
			return TextPatternIdentifiers.MixedAttributeValue;
		}
		return obj;
	}

	private bool IsElementBoundary(TextPointerContext symbolType)
	{
		if (symbolType != TextPointerContext.ElementStart)
		{
			return symbolType == TextPointerContext.ElementEnd;
		}
		return true;
	}

	private static int ColorFromBrush(object brush)
	{
		Color color = ((brush is SolidColorBrush solidColorBrush) ? solidColorBrush.Color : Colors.Black);
		return (color.R << 16) + (color.G << 8) + color.B;
	}

	private static string GetFontFamilyName(FontFamily fontFamily, ITextPointer context)
	{
		if (fontFamily != null)
		{
			if (fontFamily.Source != null)
			{
				return fontFamily.Source;
			}
			if (fontFamily.FamilyMaps != null)
			{
				XmlLanguage xmlLanguage = ((context != null) ? ((XmlLanguage)context.GetValue(FrameworkElement.LanguageProperty)) : null);
				foreach (FontFamilyMap familyMap in fontFamily.FamilyMaps)
				{
					if (familyMap.Language == null)
					{
						return familyMap.Target;
					}
					if (xmlLanguage != null && familyMap.Language.RangeIncludes(xmlLanguage))
					{
						return familyMap.Target;
					}
				}
			}
		}
		return "Global User Interface";
	}

	private static int GetTextDecorationColor(TextDecorationCollection decorations, TextDecorationLocation location)
	{
		if (decorations == null)
		{
			return 0;
		}
		int result = 0;
		foreach (TextDecoration decoration in decorations)
		{
			if (decoration.Location == location && decoration.Pen != null)
			{
				result = ColorFromBrush(decoration.Pen.Brush);
				break;
			}
		}
		return result;
	}

	private static TextDecorationLineStyle GetTextDecorationLineStyle(TextDecorationCollection decorations, TextDecorationLocation location)
	{
		if (decorations == null)
		{
			return TextDecorationLineStyle.None;
		}
		TextDecorationLineStyle textDecorationLineStyle = TextDecorationLineStyle.None;
		foreach (TextDecoration decoration in decorations)
		{
			if (decoration.Location == location)
			{
				if (textDecorationLineStyle != 0)
				{
					textDecorationLineStyle = TextDecorationLineStyle.Other;
					break;
				}
				textDecorationLineStyle = ((decoration.Pen == null) ? TextDecorationLineStyle.Single : ((decoration.Pen.DashStyle.Dashes.Count <= 1) ? TextDecorationLineStyle.Single : TextDecorationLineStyle.Dash));
			}
		}
		return textDecorationLineStyle;
	}

	private static double NativeObjectLengthToPoints(double length)
	{
		if (!double.IsNaN(length))
		{
			return length * 72.0 / 96.0;
		}
		return 0.0;
	}

	private AutomationPeer GetEnclosingAutomationPeer(ITextPointer start, ITextPointer end)
	{
		ITextPointer elementStart;
		ITextPointer elementEnd;
		AutomationPeer automationPeer = TextContainerHelper.GetEnclosingAutomationPeer(start, end, out elementStart, out elementEnd);
		if (automationPeer == null)
		{
			automationPeer = _textPeer;
		}
		else
		{
			Invariant.Assert(elementStart != null && elementEnd != null);
			AutomationPeer enclosingAutomationPeer = GetEnclosingAutomationPeer(elementStart, elementEnd);
			GetAutomationPeersFromRange(enclosingAutomationPeer, elementStart, elementEnd);
		}
		return automationPeer;
	}

	private IRawElementProviderSimple ProviderFromPeer(AutomationPeer peer)
	{
		if (_textPeer is TextAutomationPeer)
		{
			return ((TextAutomationPeer)_textPeer).ProviderFromPeer(peer);
		}
		return ((ContentTextAutomationPeer)_textPeer).ProviderFromPeer(peer);
	}

	private List<AutomationPeer> GetAutomationPeersFromRange(AutomationPeer peer, ITextPointer start, ITextPointer end)
	{
		Invariant.Assert(peer is TextAutomationPeer || peer is ContentTextAutomationPeer);
		if (peer is TextAutomationPeer)
		{
			return ((TextAutomationPeer)peer).GetAutomationPeersFromRange(start, end);
		}
		return ((ContentTextAutomationPeer)peer).GetAutomationPeersFromRange(start, end);
	}

	private static bool IsAtWordBoundary(ITextPointer position)
	{
		if (!TextPointerBase.IsAtInsertionPosition(position))
		{
			return false;
		}
		return TextPointerBase.IsAtWordBoundary(position, LogicalDirection.Forward);
	}

	private static bool MoveToNextWordBoundary(ITextPointer position, LogicalDirection direction)
	{
		int num = 0;
		ITextPointer position2 = position.CreatePointer();
		while (position.MoveToNextInsertionPosition(direction))
		{
			num++;
			if (IsAtWordBoundary(position))
			{
				break;
			}
			if (num > 128)
			{
				position.MoveToPosition(position2);
				position.MoveToNextContextPosition(direction);
				break;
			}
		}
		if (num > 0)
		{
			position.SetLogicalDirection(LogicalDirection.Forward);
		}
		return num > 0;
	}

	private void Normalize()
	{
		MoveToInsertionPosition(_start, _start.LogicalDirection);
		MoveToInsertionPosition(_end, _end.LogicalDirection);
		if (_start.CompareTo(_end) > 0)
		{
			_end.MoveToPosition(_start);
		}
	}

	private ITextPointer GetInsertionPosition(ITextPointer position, LogicalDirection direction)
	{
		position = position.CreatePointer();
		MoveToInsertionPosition(position, direction);
		return position;
	}

	ITextRangeProvider ITextRangeProvider.Clone()
	{
		return new TextRangeAdaptor(_textAdaptor, _start, _end, _textPeer);
	}

	bool ITextRangeProvider.Compare(ITextRangeProvider range)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		TextRangeAdaptor textRangeAdaptor = ValidateAndThrow(range);
		Normalize();
		textRangeAdaptor.Normalize();
		if (textRangeAdaptor._start.CompareTo(_start) == 0)
		{
			return textRangeAdaptor._end.CompareTo(_end) == 0;
		}
		return false;
	}

	int ITextRangeProvider.CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
	{
		if (targetRange == null)
		{
			throw new ArgumentNullException("targetRange");
		}
		TextRangeAdaptor textRangeAdaptor = ValidateAndThrow(targetRange);
		Normalize();
		textRangeAdaptor.Normalize();
		ITextPointer obj = ((endpoint == TextPatternRangeEndpoint.Start) ? _start : _end);
		ITextPointer position = ((targetEndpoint == TextPatternRangeEndpoint.Start) ? textRangeAdaptor._start : textRangeAdaptor._end);
		return obj.CompareTo(position);
	}

	void ITextRangeProvider.ExpandToEnclosingUnit(TextUnit unit)
	{
		Normalize();
		_end.MoveToPosition(_start);
		_end.MoveToNextInsertionPosition(LogicalDirection.Forward);
		ExpandToEnclosingUnit(unit, expandStart: true, expandEnd: true);
	}

	ITextRangeProvider ITextRangeProvider.FindAttribute(int attributeId, object value, bool backward)
	{
		AutomationTextAttribute automationTextAttribute = AutomationTextAttribute.LookupById(attributeId);
		if (automationTextAttribute == null)
		{
			throw new ArgumentNullException("attributeId");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!_textPatternAttributes.ContainsKey(automationTextAttribute))
		{
			return null;
		}
		Normalize();
		ITextRangeProvider result = null;
		ITextPointer textPointer = null;
		ITextPointer textPointer2 = null;
		TextAttributeHelper textAttributeHelper = (TextAttributeHelper)_textPatternAttributes[automationTextAttribute];
		if (backward)
		{
			ITextPointer start = _start;
			ITextPointer textPointer3 = _end.CreatePointer(LogicalDirection.Backward);
			textPointer = start;
			while (textPointer3.CompareTo(start) > 0)
			{
				if (textAttributeHelper.AreEqual(value, textAttributeHelper.GetValueAt(textPointer3)))
				{
					if (textPointer2 == null)
					{
						textPointer2 = textPointer3.CreatePointer(LogicalDirection.Backward);
					}
				}
				else if (textPointer2 != null)
				{
					textPointer = textPointer3.CreatePointer(LogicalDirection.Forward);
					break;
				}
				if (!textPointer3.MoveToNextContextPosition(LogicalDirection.Backward))
				{
					break;
				}
			}
		}
		else
		{
			ITextPointer end = _end;
			ITextPointer textPointer4 = _start.CreatePointer(LogicalDirection.Forward);
			textPointer2 = end;
			while (textPointer4.CompareTo(end) < 0)
			{
				if (textAttributeHelper.AreEqual(value, textAttributeHelper.GetValueAt(textPointer4)))
				{
					if (textPointer == null)
					{
						textPointer = textPointer4.CreatePointer(LogicalDirection.Forward);
					}
				}
				else if (textPointer != null)
				{
					textPointer2 = textPointer4.CreatePointer(LogicalDirection.Backward);
					break;
				}
				if (!textPointer4.MoveToNextContextPosition(LogicalDirection.Forward))
				{
					break;
				}
			}
		}
		if (textPointer != null && textPointer2 != null)
		{
			result = new TextRangeAdaptor(_textAdaptor, textPointer, textPointer2, _textPeer);
		}
		return result;
	}

	ITextRangeProvider ITextRangeProvider.FindText(string text, bool backward, bool ignoreCase)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length == 0)
		{
			throw new ArgumentException(SR.Format(SR.TextRangeProvider_EmptyStringParameter, "text"));
		}
		Normalize();
		if (_start.CompareTo(_end) == 0)
		{
			return null;
		}
		TextRangeAdaptor result = null;
		FindFlags findFlags = FindFlags.None;
		if (!ignoreCase)
		{
			findFlags |= FindFlags.MatchCase;
		}
		if (backward)
		{
			findFlags |= FindFlags.FindInReverse;
		}
		ITextRange textRange = TextFindEngine.Find(_start, _end, text, findFlags, CultureInfo.CurrentCulture);
		if (textRange != null && !textRange.IsEmpty)
		{
			result = new TextRangeAdaptor(_textAdaptor, textRange.Start, textRange.End, _textPeer);
		}
		return result;
	}

	object ITextRangeProvider.GetAttributeValue(int attributeId)
	{
		AutomationTextAttribute automationTextAttribute = AutomationTextAttribute.LookupById(attributeId);
		if (automationTextAttribute == null || !_textPatternAttributes.ContainsKey(automationTextAttribute))
		{
			return AutomationElementIdentifiers.NotSupported;
		}
		Normalize();
		return GetAttributeValue((TextAttributeHelper)_textPatternAttributes[automationTextAttribute]);
	}

	double[] ITextRangeProvider.GetBoundingRectangles()
	{
		Normalize();
		Rect[] boundingRectangles = _textAdaptor.GetBoundingRectangles(_start, _end, clipToView: true, transformToScreen: true);
		double[] array = new double[boundingRectangles.Length * 4];
		for (int i = 0; i < boundingRectangles.Length; i++)
		{
			array[4 * i] = boundingRectangles[i].X;
			array[4 * i + 1] = boundingRectangles[i].Y;
			array[4 * i + 2] = boundingRectangles[i].Width;
			array[4 * i + 3] = boundingRectangles[i].Height;
		}
		return array;
	}

	IRawElementProviderSimple ITextRangeProvider.GetEnclosingElement()
	{
		Normalize();
		AutomationPeer enclosingAutomationPeer = GetEnclosingAutomationPeer(_start, _end);
		Invariant.Assert(enclosingAutomationPeer != null);
		IRawElementProviderSimple rawElementProviderSimple = ProviderFromPeer(enclosingAutomationPeer);
		Invariant.Assert(rawElementProviderSimple != null);
		return rawElementProviderSimple;
	}

	string ITextRangeProvider.GetText(int maxLength)
	{
		if (maxLength < 0 && maxLength != -1)
		{
			throw new ArgumentException(SR.Format(SR.TextRangeProvider_InvalidParameterValue, maxLength, "maxLength"));
		}
		Normalize();
		string textInternal = TextRangeBase.GetTextInternal(_start, _end);
		if (textInternal.Length > maxLength && maxLength != -1)
		{
			return textInternal.Substring(0, maxLength);
		}
		return textInternal;
	}

	int ITextRangeProvider.Move(TextUnit unit, int count)
	{
		Normalize();
		int num = 0;
		if (unit != TextUnit.Paragraph)
		{
			ExpandToEnclosingUnit(unit, expandStart: true, expandEnd: true);
		}
		if (count != 0)
		{
			ITextPointer textPointer = _start.CreatePointer();
			num = MovePositionByUnits(textPointer, unit, count);
			if ((textPointer.CompareTo(_start) == 0 && textPointer.LogicalDirection != _start.LogicalDirection) || (count > 0 && textPointer.CompareTo(_start) > 0) || (count < 0 && textPointer.CompareTo(_start) < 0))
			{
				_start = textPointer;
				_end = textPointer.CreatePointer();
				if (unit != TextUnit.Page)
				{
					_end.MoveToNextInsertionPosition(LogicalDirection.Forward);
				}
				ExpandToEnclosingUnit(unit, expandStart: true, expandEnd: true);
				if (num == 0)
				{
					num = ((count > 0) ? 1 : (-1));
				}
			}
		}
		return num;
	}

	int ITextRangeProvider.MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
	{
		Normalize();
		int num = 0;
		if (count != 0)
		{
			bool flag = endpoint == TextPatternRangeEndpoint.Start;
			ITextPointer textPointer = (flag ? _start : _end);
			ITextPointer textPointer2 = textPointer.CreatePointer();
			if (MoveToUnitBoundary(textPointer2, flag, (count >= 0) ? LogicalDirection.Forward : LogicalDirection.Backward, unit))
			{
				num = ((count > 0) ? 1 : (-1));
			}
			if (count != num)
			{
				num += MovePositionByUnits(textPointer2, unit, count - num);
			}
			if ((count > 0 && textPointer2.CompareTo(textPointer) > 0) || (count < 0 && textPointer2.CompareTo(textPointer) < 0) || (textPointer2.CompareTo(textPointer) == 0 && textPointer2.LogicalDirection != textPointer.LogicalDirection))
			{
				if (flag)
				{
					_start = textPointer2;
				}
				else
				{
					_end = textPointer2;
				}
				if (unit != TextUnit.Page)
				{
					ExpandToEnclosingUnit(unit, flag, !flag);
				}
				if (num == 0)
				{
					num = ((count > 0) ? 1 : (-1));
				}
			}
			if (_start.CompareTo(_end) > 0)
			{
				if (flag)
				{
					_end = _start.CreatePointer();
				}
				else
				{
					_start = _end.CreatePointer();
				}
			}
		}
		return num;
	}

	void ITextRangeProvider.MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
	{
		if (targetRange == null)
		{
			throw new ArgumentNullException("targetRange");
		}
		TextRangeAdaptor textRangeAdaptor = ValidateAndThrow(targetRange);
		ITextPointer textPointer = ((targetEndpoint == TextPatternRangeEndpoint.Start) ? textRangeAdaptor._start : textRangeAdaptor._end);
		if (endpoint == TextPatternRangeEndpoint.Start)
		{
			_start = textPointer.CreatePointer();
			if (_start.CompareTo(_end) > 0)
			{
				_end = _start.CreatePointer();
			}
		}
		else
		{
			_end = textPointer.CreatePointer();
			if (_start.CompareTo(_end) > 0)
			{
				_start = _end.CreatePointer();
			}
		}
	}

	void ITextRangeProvider.Select()
	{
		if (((ITextProvider)_textAdaptor).SupportedTextSelection == SupportedTextSelection.None)
		{
			throw new InvalidOperationException(SR.TextProvider_TextSelectionNotSupported);
		}
		Normalize();
		_textAdaptor.Select(_start, _end);
	}

	void ITextRangeProvider.AddToSelection()
	{
		throw new InvalidOperationException();
	}

	void ITextRangeProvider.RemoveFromSelection()
	{
		throw new InvalidOperationException();
	}

	void ITextRangeProvider.ScrollIntoView(bool alignToTop)
	{
		Normalize();
		_textAdaptor.ScrollIntoView(_start, _end, alignToTop);
	}

	IRawElementProviderSimple[] ITextRangeProvider.GetChildren()
	{
		Normalize();
		IRawElementProviderSimple[] array = null;
		AutomationPeer enclosingAutomationPeer = GetEnclosingAutomationPeer(_start, _end);
		Invariant.Assert(enclosingAutomationPeer != null);
		List<AutomationPeer> automationPeersFromRange = GetAutomationPeersFromRange(enclosingAutomationPeer, _start, _end);
		if (automationPeersFromRange.Count > 0)
		{
			array = new IRawElementProviderSimple[automationPeersFromRange.Count];
			for (int i = 0; i < automationPeersFromRange.Count; i++)
			{
				array[i] = ProviderFromPeer(automationPeersFromRange[i]);
			}
		}
		return array;
	}
}
