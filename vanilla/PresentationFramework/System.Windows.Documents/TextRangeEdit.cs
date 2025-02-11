using MS.Internal;

namespace System.Windows.Documents;

internal static class TextRangeEdit
{
	internal static class DoublePropertyBounds
	{
		private struct DoublePropertyRange
		{
			private DependencyProperty _property;

			private double _lowerBound;

			private double _upperBound;

			internal DependencyProperty Property => _property;

			internal DoublePropertyRange(DependencyProperty property, double lowerBound, double upperBound)
			{
				Invariant.Assert(lowerBound < upperBound);
				_lowerBound = lowerBound;
				_upperBound = upperBound;
				_property = property;
			}

			internal double GetClosestValue(double value)
			{
				return Math.Min(Math.Max(_lowerBound, value), _upperBound);
			}
		}

		private static readonly DoublePropertyRange[] _ranges = new DoublePropertyRange[2]
		{
			new DoublePropertyRange(null, 0.0, double.MaxValue),
			new DoublePropertyRange(Paragraph.TextIndentProperty, -Math.Min(1000000, 3500000), Math.Min(1000000, 3500000))
		};

		private static DoublePropertyRange DefaultRange => _ranges[0];

		internal static double GetClosestValidValue(DependencyProperty property, double value)
		{
			return GetValueRange(property).GetClosestValue(value);
		}

		private static DoublePropertyRange GetValueRange(DependencyProperty property)
		{
			for (int i = 0; i < _ranges.Length; i++)
			{
				if (property == _ranges[i].Property)
				{
					return _ranges[i];
				}
			}
			return DefaultRange;
		}
	}

	internal static TextElement InsertElementClone(TextPointer start, TextPointer end, TextElement element)
	{
		TextElement textElement = (TextElement)Activator.CreateInstance(element.GetType());
		textElement.TextContainer.SetValues(textElement.ContentStart, element.GetLocalValueEnumerator());
		textElement.Reposition(start, end);
		return textElement;
	}

	internal static TextPointer SplitFormattingElements(TextPointer splitPosition, bool keepEmptyFormatting)
	{
		return SplitFormattingElements(splitPosition, keepEmptyFormatting, null);
	}

	internal static TextPointer SplitFormattingElement(TextPointer splitPosition, bool keepEmptyFormatting)
	{
		Invariant.Assert(splitPosition.Parent != null && TextSchema.IsMergeableInline(splitPosition.Parent.GetType()));
		Inline inline = (Inline)splitPosition.Parent;
		if (splitPosition.IsFrozen)
		{
			splitPosition = new TextPointer(splitPosition);
		}
		if (!keepEmptyFormatting && splitPosition.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
		{
			splitPosition.MoveToPosition(inline.ElementStart);
		}
		else if (!keepEmptyFormatting && splitPosition.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
		{
			splitPosition.MoveToPosition(inline.ElementEnd);
		}
		else
		{
			splitPosition = SplitElement(splitPosition);
		}
		return splitPosition;
	}

	private static bool InheritablePropertiesAreEqual(Inline firstInline, Inline secondInline)
	{
		Invariant.Assert(firstInline != null, "null check: firstInline");
		Invariant.Assert(secondInline != null, "null check: secondInline");
		DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(typeof(Inline));
		foreach (DependencyProperty dependencyProperty in inheritableProperties)
		{
			if (TextSchema.IsStructuralCharacterProperty(dependencyProperty))
			{
				if (firstInline.ReadLocalValue(dependencyProperty) != DependencyProperty.UnsetValue || secondInline.ReadLocalValue(dependencyProperty) != DependencyProperty.UnsetValue)
				{
					return false;
				}
			}
			else if (!TextSchema.ValuesAreEqual(firstInline.GetValue(dependencyProperty), secondInline.GetValue(dependencyProperty)))
			{
				return false;
			}
		}
		return true;
	}

	private static bool CharacterPropertiesAreEqual(Inline firstElement, Inline secondElement)
	{
		Invariant.Assert(firstElement != null, "null check: firstElement");
		if (secondElement == null)
		{
			return false;
		}
		DependencyProperty[] noninheritableProperties = TextSchema.GetNoninheritableProperties(typeof(Span));
		foreach (DependencyProperty dp in noninheritableProperties)
		{
			if (!TextSchema.ValuesAreEqual(firstElement.GetValue(dp), secondElement.GetValue(dp)))
			{
				return false;
			}
		}
		if (!InheritablePropertiesAreEqual(firstElement, secondElement))
		{
			return false;
		}
		return true;
	}

	private static bool ExtractEmptyFormattingElements(TextPointer position)
	{
		bool result = false;
		Inline inline = position.Parent as Inline;
		if (inline != null && inline.IsEmpty)
		{
			while (inline != null && inline.IsEmpty && !TextSchema.IsFormattingType(inline.GetType()))
			{
				inline.Reposition(null, null);
				result = true;
				inline = position.Parent as Inline;
			}
			while (inline != null && inline.IsEmpty && (inline.GetType() == typeof(Run) || inline.GetType() == typeof(Span)) && !HasWriteableLocalPropertyValues(inline))
			{
				inline.Reposition(null, null);
				result = true;
				inline = position.Parent as Inline;
			}
			while (inline != null && inline.IsEmpty && ((inline.NextInline != null && TextSchema.IsFormattingType(inline.NextInline.GetType())) || (inline.PreviousInline != null && TextSchema.IsFormattingType(inline.PreviousInline.GetType()))))
			{
				inline.Reposition(null, null);
				result = true;
				inline = position.Parent as Inline;
			}
		}
		return result;
	}

	internal static void SetInlineProperty(TextPointer start, TextPointer end, DependencyProperty formattingProperty, object value, PropertyValueAction propertyValueAction)
	{
		if (start.CompareTo(end) < 0 && (propertyValueAction != 0 || !(start.Parent is Run) || start.Parent != end.Parent || !TextSchema.ValuesAreEqual(start.Parent.GetValue(formattingProperty), value)))
		{
			RemoveUnnecessarySpans(start);
			RemoveUnnecessarySpans(end);
			if (TextSchema.IsStructuralCharacterProperty(formattingProperty))
			{
				SetStructuralInlineProperty(start, end, formattingProperty, value);
			}
			else
			{
				SetNonStructuralInlineProperty(start, end, formattingProperty, value, propertyValueAction);
			}
		}
	}

	internal static bool MergeFormattingInlines(TextPointer position)
	{
		RemoveUnnecessarySpans(position);
		ExtractEmptyFormattingElements(position);
		while (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && TextSchema.IsMergeableInline(position.Parent.GetType()))
		{
			position = ((Inline)position.Parent).ElementStart;
		}
		while (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd && TextSchema.IsMergeableInline(position.Parent.GetType()))
		{
			position = ((Inline)position.Parent).ElementEnd;
		}
		bool flag = false;
		while (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementEnd && position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart && position.GetAdjacentElement(LogicalDirection.Backward) is Inline inline && position.GetAdjacentElement(LogicalDirection.Forward) is Inline inline2)
		{
			if (TextSchema.IsFormattingType(inline.GetType()) && inline.TextRange.IsEmpty)
			{
				inline.RepositionWithContent(null);
				flag = true;
				continue;
			}
			if (TextSchema.IsFormattingType(inline2.GetType()) && inline2.TextRange.IsEmpty)
			{
				inline2.RepositionWithContent(null);
				flag = true;
				continue;
			}
			if (!TextSchema.IsKnownType(inline.GetType()) || !TextSchema.IsKnownType(inline2.GetType()) || ((!(inline is Run) || !(inline2 is Run)) && (!(inline is Span) || !(inline2 is Span))) || !TextSchema.IsMergeableInline(inline.GetType()) || !TextSchema.IsMergeableInline(inline2.GetType()) || !CharacterPropertiesAreEqual(inline, inline2))
			{
				break;
			}
			inline.Reposition(inline.ElementStart, inline2.ElementEnd);
			inline2.Reposition(null, null);
			flag = true;
		}
		if (flag)
		{
			RemoveUnnecessarySpans(position);
		}
		return flag;
	}

	private static void RemoveUnnecessarySpans(TextPointer position)
	{
		Inline inline = position.Parent as Inline;
		while (inline != null)
		{
			if (inline.Parent != null && TextSchema.IsMergeableInline(inline.Parent.GetType()) && TextSchema.IsKnownType(inline.Parent.GetType()) && inline.ElementStart.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && inline.ElementEnd.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
			{
				Span span = (Span)inline.Parent;
				if (span.Parent == null)
				{
					break;
				}
				DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(typeof(Span));
				foreach (DependencyProperty dp in inheritableProperties)
				{
					object value = inline.GetValue(dp);
					object value2 = span.GetValue(dp);
					if (TextSchema.ValuesAreEqual(value, value2))
					{
						object value3 = span.Parent.GetValue(dp);
						if (!TextSchema.ValuesAreEqual(value, value3))
						{
							inline.SetValue(dp, value2);
						}
					}
				}
				DependencyProperty[] noninheritableProperties = TextSchema.GetNoninheritableProperties(typeof(Span));
				foreach (DependencyProperty dp2 in noninheritableProperties)
				{
					bool hasModifiers;
					bool flag = span.GetValueSource(dp2, null, out hasModifiers) == BaseValueSourceInternal.Default && !hasModifiers;
					if (inline.GetValueSource(dp2, null, out hasModifiers) == BaseValueSourceInternal.Default && !hasModifiers && !flag)
					{
						inline.SetValue(dp2, span.GetValue(dp2));
					}
				}
				span.Reposition(null, null);
			}
			else
			{
				inline = inline.Parent as Inline;
			}
		}
	}

	internal static void CharacterResetFormatting(TextPointer start, TextPointer end)
	{
		if (start.CompareTo(end) >= 0)
		{
			return;
		}
		start = SplitFormattingElements(start, keepEmptyFormatting: false, preserveStructuralFormatting: true, null);
		end = SplitFormattingElements(end, keepEmptyFormatting: false, preserveStructuralFormatting: true, null);
		while (start.CompareTo(end) < 0)
		{
			if (start.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
			{
				TextElement textElement = (TextElement)start.Parent;
				if (!(textElement is Span) || textElement.ContentEnd.CompareTo(end) <= 0)
				{
					if (textElement is Span && TextSchema.IsKnownType(textElement.GetType()))
					{
						TextPointer elementStart = textElement.ElementStart;
						Span span = TransferNonFormattingInlineProperties((Span)textElement);
						if (span != null)
						{
							span.Reposition(textElement.ElementStart, textElement.ElementEnd);
							elementStart = span.ElementStart;
						}
						textElement.Reposition(null, null);
						MergeFormattingInlines(elementStart);
					}
					else if (textElement is Inline)
					{
						ClearFormattingInlineProperties((Inline)textElement);
						MergeFormattingInlines(textElement.ElementStart);
					}
				}
			}
			start = start.GetNextContextPosition(LogicalDirection.Forward);
		}
		MergeFormattingInlines(end);
	}

	private static void ClearFormattingInlineProperties(Inline inline)
	{
		LocalValueEnumerator localValueEnumerator = inline.GetLocalValueEnumerator();
		while (localValueEnumerator.MoveNext())
		{
			DependencyProperty property = localValueEnumerator.Current.Property;
			if (!property.ReadOnly && !TextSchema.IsNonFormattingCharacterProperty(property))
			{
				inline.ClearValue(localValueEnumerator.Current.Property);
			}
		}
	}

	private static Span TransferNonFormattingInlineProperties(Span source)
	{
		Span span = null;
		DependencyProperty[] nonFormattingCharacterProperties = TextSchema.GetNonFormattingCharacterProperties();
		for (int i = 0; i < nonFormattingCharacterProperties.Length; i++)
		{
			object value = source.GetValue(nonFormattingCharacterProperties[i]);
			object value2 = ((ITextPointer)source.ElementStart).GetValue(nonFormattingCharacterProperties[i]);
			if (!TextSchema.ValuesAreEqual(value, value2))
			{
				if (span == null)
				{
					span = new Span();
				}
				span.SetValue(nonFormattingCharacterProperties[i], value);
			}
		}
		return span;
	}

	internal static TextPointer SplitElement(TextPointer position)
	{
		TextElement textElement = (TextElement)position.Parent;
		if (position.IsFrozen)
		{
			position = new TextPointer(position);
		}
		if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
		{
			TextElement textElement2 = InsertElementClone(textElement.ElementEnd, textElement.ElementEnd, textElement);
			position.MoveToPosition(textElement.ElementEnd);
		}
		else if (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
		{
			TextElement textElement2 = InsertElementClone(textElement.ElementStart, textElement.ElementStart, textElement);
			position.MoveToPosition(textElement.ElementStart);
		}
		else
		{
			TextElement textElement2 = InsertElementClone(position, textElement.ContentEnd, textElement);
			textElement.Reposition(textElement.ContentStart, textElement2.ElementStart);
			position.MoveToPosition(textElement.ElementEnd);
		}
		Invariant.Assert(position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementEnd, "position must be after ElementEnd");
		Invariant.Assert(position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart, "position must be before ElementStart");
		return position;
	}

	internal static TextPointer InsertParagraphBreak(TextPointer position, bool moveIntoSecondParagraph)
	{
		Invariant.Assert(position.TextContainer.Parent == null || TextSchema.IsValidChildOfContainer(position.TextContainer.Parent.GetType(), typeof(Paragraph)));
		bool num = TextPointerBase.IsAtRowEnd(position) || TextPointerBase.IsBeforeFirstTable(position) || TextPointerBase.IsInBlockUIContainer(position);
		if (position.Paragraph == null)
		{
			position = TextRangeEditTables.EnsureInsertionPosition(position);
		}
		Inline nonMergeableInlineAncestor = position.GetNonMergeableInlineAncestor();
		if (nonMergeableInlineAncestor != null)
		{
			Invariant.Assert(TextPointerBase.IsPositionAtNonMergeableInlineBoundary(position), "Position must be at hyperlink boundary!");
			position = (position.IsAtNonMergeableInlineStart ? nonMergeableInlineAncestor.ElementStart : nonMergeableInlineAncestor.ElementEnd);
		}
		Paragraph paragraph = position.Paragraph;
		if (paragraph == null)
		{
			Invariant.Assert(position.TextContainer.Parent == null);
			paragraph = new Paragraph();
			paragraph.Reposition(position.DocumentStart, position.DocumentEnd);
		}
		if (num)
		{
			return position;
		}
		TextPointer splitPosition = position;
		splitPosition = SplitFormattingElements(splitPosition, keepEmptyFormatting: true);
		Invariant.Assert(splitPosition.Parent == paragraph, "breakPosition must be in paragraph scope after splitting formatting elements");
		bool num2 = TextPointerBase.GetImmediateListItem(paragraph.ContentStart) != null;
		splitPosition = SplitElement(splitPosition);
		if (num2)
		{
			Invariant.Assert(splitPosition.Parent is ListItem, "breakPosition must be in ListItem scope");
			splitPosition = SplitElement(splitPosition);
		}
		if (moveIntoSecondParagraph)
		{
			while (!(splitPosition.Parent is Paragraph) && splitPosition.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
			{
				splitPosition = splitPosition.GetNextContextPosition(LogicalDirection.Forward);
			}
			splitPosition = splitPosition.GetInsertionPosition(LogicalDirection.Forward);
		}
		return splitPosition;
	}

	internal static TextPointer InsertLineBreak(TextPointer position)
	{
		if (!TextSchema.IsValidChild(position, typeof(LineBreak)))
		{
			position = TextRangeEditTables.EnsureInsertionPosition(position);
		}
		if (TextSchema.IsInTextContent(position))
		{
			position = SplitElement(position);
		}
		Invariant.Assert(TextSchema.IsValidChild(position, typeof(LineBreak)), "position must be in valid scope now to insert a LineBreak element");
		LineBreak lineBreak = new LineBreak();
		position.InsertTextElement(lineBreak);
		return lineBreak.ElementEnd.GetInsertionPosition(LogicalDirection.Forward);
	}

	internal static void SetParagraphProperty(TextPointer start, TextPointer end, DependencyProperty property, object value)
	{
		SetParagraphProperty(start, end, property, value, PropertyValueAction.SetValue);
	}

	internal static void SetParagraphProperty(TextPointer start, TextPointer end, DependencyProperty property, object value, PropertyValueAction propertyValueAction)
	{
		Invariant.Assert(start != null, "null check: start");
		Invariant.Assert(end != null, "null check: end");
		Invariant.Assert(start.CompareTo(end) <= 0, "expecting: start <= end");
		Invariant.Assert(property != null, "null check: property");
		end = (TextPointer)GetAdjustedRangeEnd(start, end);
		Block paragraphOrBlockUIContainer = start.ParagraphOrBlockUIContainer;
		if (paragraphOrBlockUIContainer != null)
		{
			start = paragraphOrBlockUIContainer.ContentStart;
		}
		if (property == Block.FlowDirectionProperty)
		{
			if (!TextRangeEditLists.SplitListsForFlowDirectionChange(start, end, value))
			{
				return;
			}
			ListItem listAncestor = start.GetListAncestor();
			if (listAncestor != null && listAncestor.List != null)
			{
				start = listAncestor.List.ElementStart;
			}
		}
		SetParagraphPropertyWorker(start, end, property, value, propertyValueAction);
	}

	private static void SetParagraphPropertyWorker(TextPointer start, TextPointer end, DependencyProperty property, object value, PropertyValueAction propertyValueAction)
	{
		for (Block nextBlock = GetNextBlock(start, end); nextBlock != null; nextBlock = GetNextBlock(start, end))
		{
			if (TextSchema.IsParagraphOrBlockUIContainer(nextBlock.GetType()))
			{
				SetPropertyOnParagraphOrBlockUIContainer(start.TextContainer.Parent, nextBlock, property, value, propertyValueAction);
				start = nextBlock.ElementEnd.GetPositionAtOffset(0, LogicalDirection.Forward);
			}
			else if (nextBlock is List)
			{
				TextPointer nextContextPosition = nextBlock.ContentStart.GetPositionAtOffset(0, LogicalDirection.Forward).GetNextContextPosition(LogicalDirection.Forward);
				TextPointer contentEnd = nextBlock.ContentEnd;
				SetParagraphPropertyWorker(nextContextPosition, contentEnd, property, value, propertyValueAction);
				if (property == Block.FlowDirectionProperty)
				{
					object value2 = nextBlock.GetValue(property);
					SetPropertyValue(nextBlock, property, value2, value);
					if (!object.Equals(value2, value))
					{
						SwapBlockLeftAndRightMargins(nextBlock);
					}
				}
				start = nextBlock.ElementEnd.GetPositionAtOffset(0, LogicalDirection.Forward);
			}
		}
	}

	private static void SetPropertyOnParagraphOrBlockUIContainer(DependencyObject parent, Block block, DependencyProperty property, object value, PropertyValueAction propertyValueAction)
	{
		FlowDirection parentFlowDirection = ((parent == null) ? ((FlowDirection)FrameworkElement.FlowDirectionProperty.GetDefaultValue(typeof(FrameworkElement))) : ((FlowDirection)parent.GetValue(FrameworkElement.FlowDirectionProperty)));
		FlowDirection flowDirection = (FlowDirection)block.GetValue(Block.FlowDirectionProperty);
		object value2 = block.GetValue(property);
		object obj = value;
		PreserveBlockContentStructuralProperty(block, property, value2, value);
		if (property.PropertyType == typeof(Thickness))
		{
			Invariant.Assert(value2 is Thickness, "Expecting the currentValue to be of Thinkness type");
			Invariant.Assert(obj is Thickness, "Expecting the newValue to be of Thinkness type");
			obj = ComputeNewThicknessValue((Thickness)value2, (Thickness)obj, parentFlowDirection, flowDirection, propertyValueAction);
		}
		else if (property == Block.TextAlignmentProperty)
		{
			Invariant.Assert(value is TextAlignment, "Expecting TextAlignment as a value of a Paragraph.TextAlignmentProperty");
			obj = ComputeNewTextAlignmentValue((TextAlignment)value, flowDirection);
			if (block is BlockUIContainer)
			{
				UIElement child = ((BlockUIContainer)block).Child;
				if (child != null)
				{
					HorizontalAlignment horizontalAlignmentFromTextAlignment = GetHorizontalAlignmentFromTextAlignment((TextAlignment)obj);
					UIElementPropertyUndoUnit.Add(block.TextContainer, child, FrameworkElement.HorizontalAlignmentProperty, horizontalAlignmentFromTextAlignment);
					child.SetValue(FrameworkElement.HorizontalAlignmentProperty, horizontalAlignmentFromTextAlignment);
				}
			}
		}
		else if (value2 is double)
		{
			obj = GetNewDoubleValue(property, (double)value2, (double)obj, propertyValueAction);
		}
		SetPropertyValue(block, property, value2, obj);
		if (property == Block.FlowDirectionProperty && !object.Equals(value2, obj))
		{
			SwapBlockLeftAndRightMargins(block);
		}
	}

	private static void PreserveBlockContentStructuralProperty(Block block, DependencyProperty property, object currentValue, object newValue)
	{
		if (!(block is Paragraph paragraph) || !TextSchema.IsStructuralCharacterProperty(property) || TextSchema.ValuesAreEqual(currentValue, newValue))
		{
			return;
		}
		Inline inline = paragraph.Inlines.FirstInline;
		Inline lastInline = paragraph.Inlines.LastInline;
		while (inline != null && inline == lastInline && inline is Span && !HasLocalPropertyValue(inline, property))
		{
			inline = ((Span)inline).Inlines.FirstInline;
			lastInline = ((Span)lastInline).Inlines.LastInline;
		}
		if (inline != lastInline)
		{
			do
			{
				object value = inline.GetValue(property);
				lastInline = inline;
				Inline inline2;
				while (true)
				{
					inline2 = (Inline)lastInline.NextElement;
					if (inline2 == null || !TextSchema.ValuesAreEqual(inline2.GetValue(property), value))
					{
						break;
					}
					lastInline = inline2;
				}
				if (TextSchema.ValuesAreEqual(value, currentValue))
				{
					if (inline != lastInline)
					{
						TextPointer frozenPointer = inline.ElementStart.GetFrozenPointer(LogicalDirection.Backward);
						TextPointer frozenPointer2 = lastInline.ElementEnd.GetFrozenPointer(LogicalDirection.Forward);
						SetStructuralInlineProperty(frozenPointer, frozenPointer2, property, currentValue);
						inline = (Inline)frozenPointer.GetAdjacentElement(LogicalDirection.Forward);
						lastInline = (Inline)frozenPointer2.GetAdjacentElement(LogicalDirection.Backward);
						if (inline != lastInline)
						{
							Span span = inline.Parent as Span;
							if (span == null || span.Inlines.FirstInline != inline || span.Inlines.LastInline != lastInline)
							{
								span = new Span(inline.ElementStart, lastInline.ElementEnd);
							}
							span.SetValue(property, currentValue);
						}
					}
					if (inline == lastInline)
					{
						SetStructuralPropertyOnInline(inline, property, currentValue);
					}
				}
				inline = inline2;
			}
			while (inline != null);
		}
		else
		{
			SetStructuralPropertyOnInline(inline, property, currentValue);
		}
	}

	private static void SetStructuralPropertyOnInline(Inline inline, DependencyProperty property, object value)
	{
		if (inline is Run && !inline.IsEmpty && !HasLocalPropertyValue(inline, property))
		{
			inline.SetValue(property, value);
		}
	}

	private static Block GetNextBlock(TextPointer pointer, TextPointer limit)
	{
		Block block = null;
		while (pointer != null && pointer.CompareTo(limit) <= 0)
		{
			if (pointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
			{
				block = pointer.Parent as Block;
				if (block is Paragraph || block is BlockUIContainer || block is List)
				{
					break;
				}
			}
			if (TextPointerBase.IsAtPotentialParagraphPosition(pointer))
			{
				pointer = TextRangeEditTables.EnsureInsertionPosition(pointer);
				block = pointer.Paragraph;
				Invariant.Assert(block != null);
				break;
			}
			pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
		}
		return block;
	}

	private static Thickness ComputeNewThicknessValue(Thickness currentThickness, Thickness newThickness, FlowDirection parentFlowDirection, FlowDirection flowDirection, PropertyValueAction propertyValueAction)
	{
		double top = ((newThickness.Top < 0.0) ? currentThickness.Top : GetNewDoubleValue(null, currentThickness.Top, newThickness.Top, propertyValueAction));
		double bottom = ((newThickness.Bottom < 0.0) ? currentThickness.Bottom : GetNewDoubleValue(null, currentThickness.Bottom, newThickness.Bottom, propertyValueAction));
		double left;
		double right;
		if (parentFlowDirection != flowDirection)
		{
			left = ((newThickness.Right < 0.0) ? currentThickness.Left : GetNewDoubleValue(null, currentThickness.Left, newThickness.Right, propertyValueAction));
			right = ((newThickness.Left < 0.0) ? currentThickness.Right : GetNewDoubleValue(null, currentThickness.Right, newThickness.Left, propertyValueAction));
		}
		else
		{
			left = ((newThickness.Left < 0.0) ? currentThickness.Left : GetNewDoubleValue(null, currentThickness.Left, newThickness.Left, propertyValueAction));
			right = ((newThickness.Right < 0.0) ? currentThickness.Right : GetNewDoubleValue(null, currentThickness.Right, newThickness.Right, propertyValueAction));
		}
		return new Thickness(left, top, right, bottom);
	}

	private static TextAlignment ComputeNewTextAlignmentValue(TextAlignment textAlignment, FlowDirection flowDirection)
	{
		switch (textAlignment)
		{
		case TextAlignment.Left:
			textAlignment = ((flowDirection != 0) ? TextAlignment.Right : TextAlignment.Left);
			break;
		case TextAlignment.Right:
			textAlignment = ((flowDirection == FlowDirection.LeftToRight) ? TextAlignment.Right : TextAlignment.Left);
			break;
		}
		return textAlignment;
	}

	private static double GetNewDoubleValue(DependencyProperty property, double currentValue, double newValue, PropertyValueAction propertyValueAction)
	{
		double value = NewValue(currentValue, newValue, propertyValueAction);
		return DoublePropertyBounds.GetClosestValidValue(property, value);
	}

	private static double NewValue(double currentValue, double newValue, PropertyValueAction propertyValueAction)
	{
		if (double.IsNaN(newValue))
		{
			return newValue;
		}
		if (double.IsNaN(currentValue))
		{
			currentValue = 0.0;
		}
		newValue = propertyValueAction switch
		{
			PropertyValueAction.DecreaseByPercentageValue => currentValue * (1.0 - newValue / 100.0), 
			PropertyValueAction.IncreaseByPercentageValue => currentValue * (1.0 + newValue / 100.0), 
			PropertyValueAction.DecreaseByAbsoluteValue => currentValue - newValue, 
			PropertyValueAction.IncreaseByAbsoluteValue => currentValue + newValue, 
			_ => newValue, 
		};
		return newValue;
	}

	internal static HorizontalAlignment GetHorizontalAlignmentFromTextAlignment(TextAlignment textAlignment)
	{
		return textAlignment switch
		{
			TextAlignment.Center => HorizontalAlignment.Center, 
			TextAlignment.Right => HorizontalAlignment.Right, 
			TextAlignment.Justify => HorizontalAlignment.Stretch, 
			_ => HorizontalAlignment.Left, 
		};
	}

	internal static TextAlignment GetTextAlignmentFromHorizontalAlignment(HorizontalAlignment horizontalAlignment)
	{
		return horizontalAlignment switch
		{
			HorizontalAlignment.Left => TextAlignment.Left, 
			HorizontalAlignment.Center => TextAlignment.Center, 
			HorizontalAlignment.Right => TextAlignment.Right, 
			_ => TextAlignment.Justify, 
		};
	}

	private static void SetPropertyValue(TextElement element, DependencyProperty property, object currentValue, object newValue)
	{
		if (!TextSchema.ValuesAreEqual(newValue, currentValue))
		{
			element.ClearValue(property);
			if (!TextSchema.ValuesAreEqual(newValue, element.GetValue(property)))
			{
				element.SetValue(property, newValue);
			}
		}
	}

	private static void SwapBlockLeftAndRightMargins(Block block)
	{
		object value = block.GetValue(Block.MarginProperty);
		if (value is Thickness && !Paragraph.IsMarginAuto((Thickness)value))
		{
			object newValue = new Thickness(((Thickness)value).Right, ((Thickness)value).Top, ((Thickness)value).Left, ((Thickness)value).Bottom);
			SetPropertyValue(block, Block.MarginProperty, value, newValue);
		}
	}

	internal static ITextPointer GetAdjustedRangeEnd(ITextPointer rangeStart, ITextPointer rangeEnd)
	{
		if (rangeStart.CompareTo(rangeEnd) < 0 && rangeEnd.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
		{
			rangeEnd = rangeEnd.GetNextInsertionPosition(LogicalDirection.Backward);
			if (rangeEnd == null)
			{
				rangeEnd = rangeStart;
			}
		}
		else if (TextPointerBase.IsAfterLastParagraph(rangeEnd))
		{
			rangeEnd = rangeEnd.GetInsertionPosition(LogicalDirection.Backward);
		}
		return rangeEnd;
	}

	internal static void MergeFlowDirection(TextPointer position)
	{
		TextPointerContext pointerContext = position.GetPointerContext(LogicalDirection.Backward);
		TextPointerContext pointerContext2 = position.GetPointerContext(LogicalDirection.Forward);
		if (pointerContext != TextPointerContext.ElementStart && pointerContext != TextPointerContext.ElementEnd && pointerContext2 != TextPointerContext.ElementStart && pointerContext2 != TextPointerContext.ElementEnd)
		{
			return;
		}
		while (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && TextSchema.IsMergeableInline(position.Parent.GetType()))
		{
			position = ((Inline)position.Parent).ElementStart;
		}
		while (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd && TextSchema.IsMergeableInline(position.Parent.GetType()))
		{
			position = ((Inline)position.Parent).ElementEnd;
		}
		TextElement textElement = position.Parent as TextElement;
		if (!(textElement is Span) && !(textElement is Paragraph))
		{
			return;
		}
		TextPointer textPointer = position.CreatePointer();
		while (textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementEnd && TextSchema.IsMergeableInline(textPointer.GetAdjacentElement(LogicalDirection.Backward).GetType()))
		{
			textPointer = ((Inline)textPointer.GetAdjacentElement(LogicalDirection.Backward)).ContentEnd;
		}
		Run run = textPointer.Parent as Run;
		TextPointer textPointer2 = position.CreatePointer();
		while (textPointer2.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart && TextSchema.IsMergeableInline(textPointer2.GetAdjacentElement(LogicalDirection.Forward).GetType()))
		{
			textPointer2 = ((Inline)textPointer2.GetAdjacentElement(LogicalDirection.Forward)).ContentStart;
		}
		Run run2 = textPointer2.Parent as Run;
		if (run != null && !run.IsEmpty && run2 != null && !run2.IsEmpty)
		{
			FlowDirection flowDirection = (FlowDirection)textElement.GetValue(FrameworkElement.FlowDirectionProperty);
			FlowDirection flowDirection2 = (FlowDirection)run.GetValue(FrameworkElement.FlowDirectionProperty);
			FlowDirection flowDirection3 = (FlowDirection)run2.GetValue(FrameworkElement.FlowDirectionProperty);
			if (flowDirection2 == flowDirection3 && flowDirection2 != flowDirection)
			{
				Inline scopingFlowDirectionInline = GetScopingFlowDirectionInline(run);
				SetStructuralInlineProperty(end: GetScopingFlowDirectionInline(run2).ElementEnd, start: scopingFlowDirectionInline.ElementStart, formattingProperty: FrameworkElement.FlowDirectionProperty, value: flowDirection2);
			}
		}
	}

	internal static bool CanApplyStructuralInlineProperty(TextPointer start, TextPointer end)
	{
		return ValidateApplyStructuralInlineProperty(start, end, TextPointer.GetCommonAncestor(start, end), null);
	}

	internal static void IncrementParagraphLeadingMargin(TextRange range, double increment, PropertyValueAction propertyValueAction)
	{
		Invariant.Assert(increment >= 0.0);
		Invariant.Assert(propertyValueAction != PropertyValueAction.SetValue);
		if (increment != 0.0)
		{
			SetParagraphProperty(value: new Thickness(increment, -1.0, -1.0, -1.0), start: range.Start, end: range.End, property: Block.MarginProperty, propertyValueAction: propertyValueAction);
		}
	}

	internal static void DeleteInlineContent(ITextPointer start, ITextPointer end)
	{
		DeleteParagraphContent(start, end);
	}

	internal static void DeleteParagraphContent(ITextPointer start, ITextPointer end)
	{
		Invariant.Assert(start != null, "null check: start");
		Invariant.Assert(end != null, "null check: end");
		Invariant.Assert(start.CompareTo(end) <= 0, "expecting: start <= end");
		if (!(start is TextPointer))
		{
			start.DeleteContentToPosition(end);
			return;
		}
		TextPointer textPointer = (TextPointer)start;
		TextPointer textPointer2 = (TextPointer)end;
		DeleteEquiScopedContent(textPointer, textPointer2);
		DeleteEquiScopedContent(textPointer2, textPointer);
		if (textPointer.CompareTo(textPointer2) < 0)
		{
			if (TextPointerBase.IsAfterLastParagraph(textPointer2))
			{
				while (textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
				{
					TextElement textElement = (TextElement)textPointer.Parent;
					if (!(textElement is Inline) && !TextSchema.AllowsParagraphMerging(textElement.GetType()))
					{
						break;
					}
					textElement.RepositionWithContent(null);
				}
			}
			else
			{
				Block block = textPointer.ParagraphOrBlockUIContainer;
				Block block2 = textPointer2.ParagraphOrBlockUIContainer;
				if (block == null && TextPointerBase.IsInEmptyListItem(textPointer))
				{
					textPointer = TextRangeEditTables.EnsureInsertionPosition(textPointer);
					block = textPointer.Paragraph;
					Invariant.Assert(block != null, "EnsureInsertionPosition must create a paragraph inside list item - 1");
				}
				if (block2 == null && TextPointerBase.IsInEmptyListItem(textPointer2))
				{
					textPointer2 = TextRangeEditTables.EnsureInsertionPosition(textPointer2);
					block2 = textPointer2.Paragraph;
					Invariant.Assert(block2 != null, "EnsureInsertionPosition must create a paragraph inside list item - 2");
				}
				if (block != null && block2 != null)
				{
					TextRangeEditLists.MergeParagraphs(block, block2);
				}
				else
				{
					MergeEmptyParagraphsAndBlockUIContainers(textPointer, textPointer2);
				}
			}
		}
		MergeFormattingInlines(textPointer);
		MergeFormattingInlines(textPointer2);
		if (textPointer.Parent is BlockUIContainer && ((BlockUIContainer)textPointer.Parent).IsEmpty)
		{
			((BlockUIContainer)textPointer.Parent).Reposition(null, null);
		}
		else if (textPointer.Parent is Hyperlink && ((Hyperlink)textPointer.Parent).IsEmpty)
		{
			((Hyperlink)textPointer.Parent).Reposition(null, null);
			MergeFormattingInlines(textPointer);
		}
	}

	private static void MergeEmptyParagraphsAndBlockUIContainers(TextPointer startPosition, TextPointer endPosition)
	{
		Block paragraphOrBlockUIContainer = startPosition.ParagraphOrBlockUIContainer;
		Block paragraphOrBlockUIContainer2 = endPosition.ParagraphOrBlockUIContainer;
		if (paragraphOrBlockUIContainer is BlockUIContainer)
		{
			if (paragraphOrBlockUIContainer.IsEmpty)
			{
				paragraphOrBlockUIContainer.Reposition(null, null);
				return;
			}
			if (paragraphOrBlockUIContainer2 is Paragraph && Paragraph.HasNoTextContent((Paragraph)paragraphOrBlockUIContainer2))
			{
				paragraphOrBlockUIContainer2.RepositionWithContent(null);
				return;
			}
		}
		if (paragraphOrBlockUIContainer2 is BlockUIContainer)
		{
			if (paragraphOrBlockUIContainer2.IsEmpty)
			{
				paragraphOrBlockUIContainer2.Reposition(null, null);
			}
			else if (paragraphOrBlockUIContainer2 is Paragraph && Paragraph.HasNoTextContent((Paragraph)paragraphOrBlockUIContainer))
			{
				paragraphOrBlockUIContainer.RepositionWithContent(null);
			}
		}
	}

	private static void DeleteEquiScopedContent(TextPointer start, TextPointer end)
	{
		Invariant.Assert(start != null, "null check: start");
		Invariant.Assert(end != null, "null check: end");
		if (start.CompareTo(end) == 0)
		{
			return;
		}
		if (start.Parent == end.Parent)
		{
			DeleteContentBetweenPositions(start, end);
			return;
		}
		LogicalDirection logicalDirection;
		LogicalDirection direction;
		TextPointerContext textPointerContext;
		TextPointerContext textPointerContext2;
		ElementEdge edge;
		ElementEdge edge2;
		if (start.CompareTo(end) < 0)
		{
			logicalDirection = LogicalDirection.Forward;
			direction = LogicalDirection.Backward;
			textPointerContext = TextPointerContext.ElementStart;
			textPointerContext2 = TextPointerContext.ElementEnd;
			edge = ElementEdge.BeforeStart;
			edge2 = ElementEdge.AfterEnd;
		}
		else
		{
			logicalDirection = LogicalDirection.Backward;
			direction = LogicalDirection.Forward;
			textPointerContext = TextPointerContext.ElementEnd;
			textPointerContext2 = TextPointerContext.ElementStart;
			edge = ElementEdge.AfterEnd;
			edge2 = ElementEdge.BeforeStart;
		}
		TextPointer textPointer = new TextPointer(start);
		TextPointer textPointer2 = new TextPointer(start);
		while (textPointer2.CompareTo(end) != 0)
		{
			Invariant.Assert((logicalDirection == LogicalDirection.Forward && textPointer2.CompareTo(end) < 0) || (logicalDirection == LogicalDirection.Backward && textPointer2.CompareTo(end) > 0), "Inappropriate position ordering");
			Invariant.Assert(textPointer.Parent == textPointer2.Parent, "inconsistent position Parents: previous and next");
			TextPointerContext pointerContext = textPointer2.GetPointerContext(logicalDirection);
			if (pointerContext == TextPointerContext.Text || pointerContext == TextPointerContext.EmbeddedElement)
			{
				textPointer2.MoveToNextContextPosition(logicalDirection);
				if ((logicalDirection == LogicalDirection.Forward && textPointer2.CompareTo(end) > 0) || (logicalDirection == LogicalDirection.Backward && textPointer2.CompareTo(end) < 0))
				{
					Invariant.Assert(textPointer2.Parent == end.Parent, "inconsistent poaition Parents: next and end");
					textPointer2.MoveToPosition(end);
					break;
				}
				continue;
			}
			if (pointerContext == textPointerContext)
			{
				textPointer2.MoveToNextContextPosition(logicalDirection);
				((ITextPointer)textPointer2).MoveToElementEdge(edge2);
				if ((logicalDirection == LogicalDirection.Forward && textPointer2.CompareTo(end) >= 0) || (logicalDirection == LogicalDirection.Backward && textPointer2.CompareTo(end) <= 0))
				{
					textPointer2.MoveToNextContextPosition(direction);
					((ITextPointer)textPointer2).MoveToElementEdge(edge);
					break;
				}
				continue;
			}
			if (pointerContext == textPointerContext2)
			{
				DeleteContentBetweenPositions(textPointer, textPointer2);
				if (!ExtractEmptyFormattingElements(textPointer))
				{
					Invariant.Assert(textPointer2.GetPointerContext(logicalDirection) == textPointerContext2, "Unexpected context of nextPosition");
					textPointer2.MoveToNextContextPosition(logicalDirection);
				}
				textPointer.MoveToPosition(textPointer2);
				continue;
			}
			Invariant.Assert(condition: false, "Not expecting None context here");
			Invariant.Assert(pointerContext == TextPointerContext.None, "Unknown pointer context");
			break;
		}
		Invariant.Assert(textPointer.Parent == textPointer2.Parent, "inconsistent Parents: previousPosition, nextPosition");
		DeleteContentBetweenPositions(textPointer, textPointer2);
	}

	private static bool DeleteContentBetweenPositions(TextPointer one, TextPointer two)
	{
		Invariant.Assert(one.Parent == two.Parent, "inconsistent Parents: one and two");
		if (one.CompareTo(two) < 0)
		{
			one.TextContainer.DeleteContentInternal(one, two);
		}
		else if (one.CompareTo(two) > 0)
		{
			two.TextContainer.DeleteContentInternal(two, one);
		}
		Invariant.Assert(one.CompareTo(two) == 0, "Positions one and two must be equal now");
		return false;
	}

	private static TextPointer SplitFormattingElements(TextPointer splitPosition, bool keepEmptyFormatting, TextElement limitingAncestor)
	{
		return SplitFormattingElements(splitPosition, keepEmptyFormatting, preserveStructuralFormatting: false, limitingAncestor);
	}

	private static TextPointer SplitFormattingElements(TextPointer splitPosition, bool keepEmptyFormatting, bool preserveStructuralFormatting, TextElement limitingAncestor)
	{
		if (preserveStructuralFormatting && splitPosition.Parent is Run run && run != limitingAncestor && ((run.Parent != null && HasLocalInheritableStructuralPropertyValue(run)) || (run.Parent == null && HasLocalStructuralPropertyValue(run))))
		{
			Span destination = new Span(run.ElementStart, run.ElementEnd);
			TransferStructuralProperties(run, destination);
		}
		while (splitPosition.Parent != null && TextSchema.IsMergeableInline(splitPosition.Parent.GetType()) && splitPosition.Parent != limitingAncestor && (!preserveStructuralFormatting || (((Inline)splitPosition.Parent).Parent != null && !HasLocalInheritableStructuralPropertyValue((Inline)splitPosition.Parent)) || (((Inline)splitPosition.Parent).Parent == null && !HasLocalStructuralPropertyValue((Inline)splitPosition.Parent))))
		{
			splitPosition = SplitFormattingElement(splitPosition, keepEmptyFormatting);
		}
		return splitPosition;
	}

	private static void TransferStructuralProperties(Inline source, Inline destination)
	{
		bool flag = source.Parent == destination;
		for (int i = 0; i < TextSchema.StructuralCharacterProperties.Length; i++)
		{
			DependencyProperty dp = TextSchema.StructuralCharacterProperties[i];
			if ((flag && HasLocalInheritableStructuralPropertyValue(source)) || (!flag && HasLocalStructuralPropertyValue(source)))
			{
				object value = source.GetValue(dp);
				source.ClearValue(dp);
				destination.SetValue(dp, value);
			}
		}
	}

	private static bool HasWriteableLocalPropertyValues(Inline inline)
	{
		LocalValueEnumerator localValueEnumerator = inline.GetLocalValueEnumerator();
		bool flag = false;
		while (!flag && localValueEnumerator.MoveNext())
		{
			flag = !localValueEnumerator.Current.Property.ReadOnly;
		}
		return flag;
	}

	private static bool HasLocalInheritableStructuralPropertyValue(Inline inline)
	{
		int i;
		for (i = 0; i < TextSchema.StructuralCharacterProperties.Length; i++)
		{
			DependencyProperty dp = TextSchema.StructuralCharacterProperties[i];
			if (!TextSchema.ValuesAreEqual(inline.GetValue(dp), inline.Parent.GetValue(dp)))
			{
				break;
			}
		}
		return i < TextSchema.StructuralCharacterProperties.Length;
	}

	private static bool HasLocalStructuralPropertyValue(Inline inline)
	{
		int i;
		for (i = 0; i < TextSchema.StructuralCharacterProperties.Length; i++)
		{
			DependencyProperty property = TextSchema.StructuralCharacterProperties[i];
			if (HasLocalPropertyValue(inline, property))
			{
				break;
			}
		}
		return i < TextSchema.StructuralCharacterProperties.Length;
	}

	private static bool HasLocalPropertyValue(Inline inline, DependencyProperty property)
	{
		bool hasModifiers;
		BaseValueSourceInternal valueSource = inline.GetValueSource(property, null, out hasModifiers);
		if (valueSource != 0 && valueSource != BaseValueSourceInternal.Default)
		{
			return valueSource != BaseValueSourceInternal.Inherited;
		}
		return false;
	}

	private static Inline GetScopingFlowDirectionInline(Run run)
	{
		FlowDirection flowDirection = run.FlowDirection;
		Inline inline = run;
		while ((FlowDirection)inline.Parent.GetValue(FrameworkElement.FlowDirectionProperty) == flowDirection)
		{
			inline = (Span)inline.Parent;
		}
		return inline;
	}

	private static void SetNonStructuralInlineProperty(TextPointer start, TextPointer end, DependencyProperty formattingProperty, object value, PropertyValueAction propertyValueAction)
	{
		start = SplitFormattingElements(start, keepEmptyFormatting: false, preserveStructuralFormatting: true, null);
		end = SplitFormattingElements(end, keepEmptyFormatting: false, preserveStructuralFormatting: true, null);
		Run nextRun = GetNextRun(start, end);
		while (nextRun != null)
		{
			object value2 = nextRun.GetValue(formattingProperty);
			object newValue = value;
			if (propertyValueAction != 0)
			{
				Invariant.Assert(formattingProperty == TextElement.FontSizeProperty, "Only FontSize can be incremented/decremented among character properties");
				newValue = GetNewFontSizeValue((double)value2, (double)value, propertyValueAction);
			}
			SetPropertyValue(nextRun, formattingProperty, value2, newValue);
			TextPointer textPointer = nextRun.ElementEnd.GetPositionAtOffset(0, LogicalDirection.Forward);
			if (TextPointerBase.IsAtPotentialRunPosition(nextRun))
			{
				textPointer = textPointer.GetNextContextPosition(LogicalDirection.Forward);
			}
			MergeFormattingInlines(nextRun.ContentStart);
			nextRun = GetNextRun(textPointer, end);
		}
		MergeFormattingInlines(end);
	}

	private static double GetNewFontSizeValue(double currentValue, double value, PropertyValueAction propertyValueAction)
	{
		double num = value;
		switch (propertyValueAction)
		{
		case PropertyValueAction.IncreaseByAbsoluteValue:
			num = currentValue + value;
			break;
		case PropertyValueAction.DecreaseByAbsoluteValue:
			num = currentValue - value;
			break;
		}
		if (num < 0.75)
		{
			num = 0.75;
		}
		else if (num > 1638.0)
		{
			num = 1638.0;
		}
		return num;
	}

	private static void SetStructuralInlineProperty(TextPointer start, TextPointer end, DependencyProperty formattingProperty, object value)
	{
		DependencyObject commonAncestor = TextPointer.GetCommonAncestor(start, end);
		ValidateApplyStructuralInlineProperty(start, end, commonAncestor, formattingProperty);
		if (commonAncestor is Run)
		{
			ApplyStructuralInlinePropertyAcrossRun(start, end, (Run)commonAncestor, formattingProperty, value);
		}
		else if ((commonAncestor is Inline && !(commonAncestor is AnchoredBlock)) || commonAncestor is Paragraph)
		{
			Invariant.Assert(!(commonAncestor is InlineUIContainer));
			ApplyStructuralInlinePropertyAcrossInline(start, end, (TextElement)commonAncestor, formattingProperty, value);
		}
		else
		{
			ApplyStructuralInlinePropertyAcrossParagraphs(start, end, formattingProperty, value);
		}
	}

	private static void FixupStructuralPropertyEnvironment(Inline inline, DependencyProperty property)
	{
		ClearParentStructuralPropertyValue(inline, property);
		for (Inline inline2 = inline; inline2 != null; inline2 = inline2.Parent as Span)
		{
			Inline inline3 = (Inline)inline2.PreviousElement;
			if (inline3 != null)
			{
				FlattenStructuralProperties(inline3);
				break;
			}
		}
		for (Inline inline4 = inline; inline4 != null; inline4 = inline4.Parent as Span)
		{
			Inline inline5 = (Inline)inline4.NextElement;
			if (inline5 != null)
			{
				FlattenStructuralProperties(inline5);
				break;
			}
		}
	}

	private static void FlattenStructuralProperties(Inline inline)
	{
		Span span = inline as Span;
		Span span2 = inline.Parent as Span;
		while (span2 != null && span2.Inlines.FirstInline == span2.Inlines.LastInline)
		{
			span = span2;
			span2 = span2.Parent as Span;
		}
		while (span != null && span.Inlines.FirstInline == span.Inlines.LastInline)
		{
			Inline firstInline = span.Inlines.FirstInline;
			TransferStructuralProperties(span, firstInline);
			if (TextSchema.IsMergeableInline(span.GetType()) && TextSchema.IsKnownType(span.GetType()) && !HasWriteableLocalPropertyValues(span))
			{
				span.Reposition(null, null);
			}
			span = firstInline as Span;
		}
	}

	private static void ClearParentStructuralPropertyValue(Inline child, DependencyProperty property)
	{
		Span span = null;
		Span span2 = child.Parent as Span;
		while (span2 != null && TextSchema.IsMergeableInline(span2.GetType()))
		{
			if (HasLocalPropertyValue(span2, property))
			{
				span = span2;
			}
			span2 = span2.Parent as Span;
		}
		if (span == null)
		{
			return;
		}
		TextElement limitingAncestor = (TextElement)span.Parent;
		SplitFormattingElements(child.ElementStart, keepEmptyFormatting: false, limitingAncestor);
		Span span3 = (Span)SplitFormattingElements(child.ElementEnd, keepEmptyFormatting: false, limitingAncestor).GetAdjacentElement(LogicalDirection.Backward);
		while (span3 != null && span3 != child)
		{
			span3.ClearValue(property);
			Span obj = span3.Inlines.FirstInline as Span;
			if (!HasWriteableLocalPropertyValues(span3))
			{
				span3.Reposition(null, null);
			}
			span3 = obj;
		}
	}

	private static Run GetNextRun(TextPointer pointer, TextPointer limit)
	{
		Run result = null;
		while (pointer != null && pointer.CompareTo(limit) < 0 && (pointer.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementStart || (result = pointer.GetAdjacentElement(LogicalDirection.Forward) as Run) == null))
		{
			if (TextPointerBase.IsAtPotentialRunPosition(pointer))
			{
				pointer = TextRangeEditTables.EnsureInsertionPosition(pointer);
				Invariant.Assert(pointer.Parent is Run);
				result = pointer.Parent as Run;
				break;
			}
			pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
		}
		return result;
	}

	private static void ClearPropertyValueFromSpansAndRuns(TextPointer start, TextPointer end, DependencyProperty formattingProperty)
	{
		start = start.GetPositionAtOffset(0, LogicalDirection.Forward);
		start = start.GetNextContextPosition(LogicalDirection.Forward);
		while (start != null && start.CompareTo(end) < 0)
		{
			if (start.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && TextSchema.IsFormattingType(start.Parent.GetType()))
			{
				start.Parent.ClearValue(formattingProperty);
				MergeFormattingInlines(start);
			}
			start = start.GetNextContextPosition(LogicalDirection.Forward);
		}
	}

	private static void ApplyStructuralInlinePropertyAcrossRun(TextPointer start, TextPointer end, Run run, DependencyProperty formattingProperty, object value)
	{
		if (start.CompareTo(end) == 0)
		{
			if (run.IsEmpty)
			{
				run.SetValue(formattingProperty, value);
			}
		}
		else
		{
			start = SplitFormattingElements(start, keepEmptyFormatting: false, run.Parent as TextElement);
			end = SplitFormattingElements(end, keepEmptyFormatting: false, run.Parent as TextElement);
			run = (Run)start.GetAdjacentElement(LogicalDirection.Forward);
			run.SetValue(formattingProperty, value);
		}
		FixupStructuralPropertyEnvironment(run, formattingProperty);
	}

	private static void ApplyStructuralInlinePropertyAcrossInline(TextPointer start, TextPointer end, TextElement commonAncestor, DependencyProperty formattingProperty, object value)
	{
		start = SplitFormattingElements(start, keepEmptyFormatting: false, commonAncestor);
		end = SplitFormattingElements(end, keepEmptyFormatting: false, commonAncestor);
		DependencyObject adjacentElement = start.GetAdjacentElement(LogicalDirection.Forward);
		DependencyObject adjacentElement2 = end.GetAdjacentElement(LogicalDirection.Backward);
		if (adjacentElement == adjacentElement2 && (adjacentElement is Run || adjacentElement is Span))
		{
			Inline inline = (Inline)start.GetAdjacentElement(LogicalDirection.Forward);
			inline.SetValue(formattingProperty, value);
			FixupStructuralPropertyEnvironment(inline, formattingProperty);
			if (adjacentElement is Span)
			{
				ClearPropertyValueFromSpansAndRuns(inline.ContentStart, inline.ContentEnd, formattingProperty);
			}
			return;
		}
		Span span;
		if (commonAncestor is Span && start.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && end.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd && start.GetAdjacentElement(LogicalDirection.Backward) == commonAncestor)
		{
			span = (Span)commonAncestor;
		}
		else
		{
			span = new Span();
			span.Reposition(start, end);
		}
		span.SetValue(formattingProperty, value);
		FixupStructuralPropertyEnvironment(span, formattingProperty);
		ClearPropertyValueFromSpansAndRuns(span.ContentStart, span.ContentEnd, formattingProperty);
	}

	private static void ApplyStructuralInlinePropertyAcrossParagraphs(TextPointer start, TextPointer end, DependencyProperty formattingProperty, object value)
	{
		Invariant.Assert(start.Paragraph != null);
		Invariant.Assert(start.Paragraph.ContentEnd.CompareTo(end) < 0);
		SetStructuralInlineProperty(start, start.Paragraph.ContentEnd, formattingProperty, value);
		start = start.Paragraph.ElementEnd;
		if (end.Paragraph != null)
		{
			SetStructuralInlineProperty(end.Paragraph.ContentStart, end, formattingProperty, value);
			end = end.Paragraph.ElementStart;
		}
		while (start != null && start.CompareTo(end) < 0)
		{
			if (start.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && start.Parent is Paragraph)
			{
				Paragraph paragraph = (Paragraph)start.Parent;
				SetStructuralInlineProperty(paragraph.ContentStart, paragraph.ContentEnd, formattingProperty, value);
				start = paragraph.ElementEnd;
			}
			start = start.GetNextContextPosition(LogicalDirection.Forward);
		}
	}

	private static bool ValidateApplyStructuralInlineProperty(TextPointer start, TextPointer end, DependencyObject commonAncestor, DependencyProperty property)
	{
		if (!(commonAncestor is Inline))
		{
			return true;
		}
		Inline p = null;
		Inline inline;
		for (inline = (Inline)start.Parent; inline != commonAncestor; inline = (Inline)inline.Parent)
		{
			if (!TextSchema.IsMergeableInline(inline.GetType()))
			{
				p = inline;
				commonAncestor = inline;
				break;
			}
		}
		for (inline = (Inline)end.Parent; inline != commonAncestor; inline = (Inline)inline.Parent)
		{
			if (!TextSchema.IsMergeableInline(inline.GetType()))
			{
				p = inline;
				break;
			}
		}
		if (property != null && inline != commonAncestor)
		{
			throw new InvalidOperationException(SR.Format(SR.TextRangeEdit_InvalidStructuralPropertyApply, property, p));
		}
		return inline == commonAncestor;
	}
}
