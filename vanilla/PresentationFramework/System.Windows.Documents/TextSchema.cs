using System.Windows.Controls;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Documents;

internal static class TextSchema
{
	private static readonly DependencyProperty[] _inheritableTextElementProperties;

	private static readonly DependencyProperty[] _inheritableBlockProperties;

	private static readonly DependencyProperty[] _inheritableTableCellProperties;

	private static readonly DependencyProperty[] _hyperlinkProperties;

	private static readonly DependencyProperty[] _inlineProperties;

	private static readonly DependencyProperty[] _paragraphProperties;

	private static readonly DependencyProperty[] _listProperties;

	private static readonly DependencyProperty[] _listItemProperties;

	private static readonly DependencyProperty[] _tableProperties;

	private static readonly DependencyProperty[] _tableColumnProperties;

	private static readonly DependencyProperty[] _tableRowGroupProperties;

	private static readonly DependencyProperty[] _tableRowProperties;

	private static readonly DependencyProperty[] _tableCellProperties;

	private static readonly DependencyProperty[] _floaterProperties;

	private static readonly DependencyProperty[] _figureProperties;

	private static readonly DependencyProperty[] _blockProperties;

	private static readonly DependencyProperty[] _textElementPropertyList;

	private static readonly DependencyProperty[] _imagePropertyList;

	private static readonly DependencyProperty[] _behavioralPropertyList;

	private static readonly DependencyProperty[] _emptyPropertyList;

	private static readonly DependencyProperty[] _structuralCharacterProperties;

	private static readonly DependencyProperty[] _nonFormattingCharacterProperties;

	internal static DependencyProperty[] BehavioralProperties => _behavioralPropertyList;

	internal static DependencyProperty[] ImageProperties => _imagePropertyList;

	internal static DependencyProperty[] StructuralCharacterProperties => _structuralCharacterProperties;

	static TextSchema()
	{
		_hyperlinkProperties = new DependencyProperty[9]
		{
			Hyperlink.NavigateUriProperty,
			Hyperlink.TargetNameProperty,
			Hyperlink.CommandProperty,
			Hyperlink.CommandParameterProperty,
			Hyperlink.CommandTargetProperty,
			Inline.BaselineAlignmentProperty,
			Inline.TextDecorationsProperty,
			TextElement.BackgroundProperty,
			FrameworkContentElement.ToolTipProperty
		};
		_inlineProperties = new DependencyProperty[3]
		{
			Inline.BaselineAlignmentProperty,
			Inline.TextDecorationsProperty,
			TextElement.BackgroundProperty
		};
		_paragraphProperties = new DependencyProperty[11]
		{
			Paragraph.MinWidowLinesProperty,
			Paragraph.MinOrphanLinesProperty,
			Paragraph.TextIndentProperty,
			Paragraph.KeepWithNextProperty,
			Paragraph.KeepTogetherProperty,
			Paragraph.TextDecorationsProperty,
			Block.MarginProperty,
			Block.PaddingProperty,
			Block.BorderThicknessProperty,
			Block.BorderBrushProperty,
			TextElement.BackgroundProperty
		};
		_listProperties = new DependencyProperty[8]
		{
			List.MarkerStyleProperty,
			List.MarkerOffsetProperty,
			List.StartIndexProperty,
			Block.MarginProperty,
			Block.PaddingProperty,
			Block.BorderThicknessProperty,
			Block.BorderBrushProperty,
			TextElement.BackgroundProperty
		};
		_listItemProperties = new DependencyProperty[5]
		{
			ListItem.MarginProperty,
			ListItem.PaddingProperty,
			ListItem.BorderThicknessProperty,
			ListItem.BorderBrushProperty,
			TextElement.BackgroundProperty
		};
		_tableProperties = new DependencyProperty[6]
		{
			Table.CellSpacingProperty,
			Block.MarginProperty,
			Block.PaddingProperty,
			Block.BorderThicknessProperty,
			Block.BorderBrushProperty,
			TextElement.BackgroundProperty
		};
		_tableColumnProperties = new DependencyProperty[2]
		{
			TableColumn.WidthProperty,
			TableColumn.BackgroundProperty
		};
		_tableRowGroupProperties = new DependencyProperty[1] { TextElement.BackgroundProperty };
		_tableRowProperties = new DependencyProperty[1] { TextElement.BackgroundProperty };
		_tableCellProperties = new DependencyProperty[6]
		{
			TableCell.ColumnSpanProperty,
			TableCell.RowSpanProperty,
			TableCell.PaddingProperty,
			TableCell.BorderThicknessProperty,
			TableCell.BorderBrushProperty,
			TextElement.BackgroundProperty
		};
		_floaterProperties = new DependencyProperty[7]
		{
			Floater.HorizontalAlignmentProperty,
			Floater.WidthProperty,
			AnchoredBlock.MarginProperty,
			AnchoredBlock.PaddingProperty,
			AnchoredBlock.BorderThicknessProperty,
			AnchoredBlock.BorderBrushProperty,
			TextElement.BackgroundProperty
		};
		_figureProperties = new DependencyProperty[13]
		{
			Figure.HorizontalAnchorProperty,
			Figure.VerticalAnchorProperty,
			Figure.HorizontalOffsetProperty,
			Figure.VerticalOffsetProperty,
			Figure.CanDelayPlacementProperty,
			Figure.WrapDirectionProperty,
			Figure.WidthProperty,
			Figure.HeightProperty,
			AnchoredBlock.MarginProperty,
			AnchoredBlock.PaddingProperty,
			AnchoredBlock.BorderThicknessProperty,
			AnchoredBlock.BorderBrushProperty,
			TextElement.BackgroundProperty
		};
		_blockProperties = new DependencyProperty[9]
		{
			Block.MarginProperty,
			Block.PaddingProperty,
			Block.BorderThicknessProperty,
			Block.BorderBrushProperty,
			Block.BreakPageBeforeProperty,
			Block.BreakColumnBeforeProperty,
			Block.ClearFloatersProperty,
			Block.IsHyphenationEnabledProperty,
			TextElement.BackgroundProperty
		};
		_textElementPropertyList = new DependencyProperty[1] { TextElement.BackgroundProperty };
		_imagePropertyList = new DependencyProperty[28]
		{
			Image.SourceProperty,
			Image.StretchProperty,
			Image.StretchDirectionProperty,
			FrameworkElement.LanguageProperty,
			FrameworkElement.LayoutTransformProperty,
			FrameworkElement.WidthProperty,
			FrameworkElement.MinWidthProperty,
			FrameworkElement.MaxWidthProperty,
			FrameworkElement.HeightProperty,
			FrameworkElement.MinHeightProperty,
			FrameworkElement.MaxHeightProperty,
			FrameworkElement.MarginProperty,
			FrameworkElement.HorizontalAlignmentProperty,
			FrameworkElement.VerticalAlignmentProperty,
			FrameworkElement.CursorProperty,
			FrameworkElement.ForceCursorProperty,
			FrameworkElement.ToolTipProperty,
			UIElement.RenderTransformProperty,
			UIElement.RenderTransformOriginProperty,
			UIElement.OpacityProperty,
			UIElement.OpacityMaskProperty,
			UIElement.BitmapEffectProperty,
			UIElement.BitmapEffectInputProperty,
			UIElement.VisibilityProperty,
			UIElement.ClipToBoundsProperty,
			UIElement.ClipProperty,
			UIElement.SnapsToDevicePixelsProperty,
			TextBlock.BaselineOffsetProperty
		};
		_behavioralPropertyList = new DependencyProperty[1] { UIElement.AllowDropProperty };
		_emptyPropertyList = new DependencyProperty[0];
		_structuralCharacterProperties = new DependencyProperty[1] { Inline.FlowDirectionProperty };
		_nonFormattingCharacterProperties = new DependencyProperty[3]
		{
			FrameworkElement.FlowDirectionProperty,
			FrameworkElement.LanguageProperty,
			Run.TextProperty
		};
		DependencyProperty[] array = new DependencyProperty[11]
		{
			FrameworkElement.LanguageProperty,
			FrameworkElement.FlowDirectionProperty,
			NumberSubstitution.CultureSourceProperty,
			NumberSubstitution.SubstitutionProperty,
			NumberSubstitution.CultureOverrideProperty,
			TextElement.FontFamilyProperty,
			TextElement.FontStyleProperty,
			TextElement.FontWeightProperty,
			TextElement.FontStretchProperty,
			TextElement.FontSizeProperty,
			TextElement.ForegroundProperty
		};
		_inheritableTextElementProperties = new DependencyProperty[array.Length + Typography.TypographyPropertiesList.Length];
		Array.Copy(array, 0, _inheritableTextElementProperties, 0, array.Length);
		Array.Copy(Typography.TypographyPropertiesList, 0, _inheritableTextElementProperties, array.Length, Typography.TypographyPropertiesList.Length);
		DependencyProperty[] array2 = new DependencyProperty[3]
		{
			Block.TextAlignmentProperty,
			Block.LineHeightProperty,
			Block.IsHyphenationEnabledProperty
		};
		_inheritableBlockProperties = new DependencyProperty[array2.Length + _inheritableTextElementProperties.Length];
		Array.Copy(array2, 0, _inheritableBlockProperties, 0, array2.Length);
		Array.Copy(_inheritableTextElementProperties, 0, _inheritableBlockProperties, array2.Length, _inheritableTextElementProperties.Length);
		DependencyProperty[] array3 = new DependencyProperty[1] { Block.TextAlignmentProperty };
		_inheritableTableCellProperties = new DependencyProperty[array3.Length + _inheritableTextElementProperties.Length];
		Array.Copy(array3, _inheritableTableCellProperties, array3.Length);
		Array.Copy(_inheritableTextElementProperties, 0, _inheritableTableCellProperties, array3.Length, _inheritableTextElementProperties.Length);
	}

	internal static bool IsInTextContent(ITextPointer position)
	{
		return IsValidChild(position, typeof(string));
	}

	internal static bool ValidateChild(TextElement parent, TextElement child, bool throwIfIllegalChild, bool throwIfIllegalHyperlinkDescendent)
	{
		if (HasHyperlinkAncestor(parent) && HasIllegalHyperlinkDescendant(child, throwIfIllegalHyperlinkDescendent))
		{
			return false;
		}
		bool num = IsValidChild(parent.GetType(), child.GetType());
		if (!num && throwIfIllegalChild)
		{
			throw new InvalidOperationException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, parent.GetType().Name, child.GetType().Name));
		}
		return num;
	}

	internal static bool IsValidChild(TextElement parent, Type childType)
	{
		return ValidateChild(parent, childType, throwIfIllegalChild: false, throwIfIllegalHyperlinkDescendent: false);
	}

	internal static bool ValidateChild(TextElement parent, Type childType, bool throwIfIllegalChild, bool throwIfIllegalHyperlinkDescendent)
	{
		if (HasHyperlinkAncestor(parent) && (typeof(Hyperlink).IsAssignableFrom(childType) || typeof(AnchoredBlock).IsAssignableFrom(childType)))
		{
			if (throwIfIllegalHyperlinkDescendent)
			{
				throw new InvalidOperationException(SR.Format(SR.TextSchema_IllegalHyperlinkChild, childType));
			}
			return false;
		}
		bool num = IsValidChild(parent.GetType(), childType);
		if (!num && throwIfIllegalChild)
		{
			throw new InvalidOperationException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, parent.GetType().Name, childType.Name));
		}
		return num;
	}

	internal static bool IsValidChild(TextPointer position, Type childType)
	{
		return ValidateChild(position, childType, throwIfIllegalChild: false, throwIfIllegalHyperlinkDescendent: false);
	}

	internal static bool ValidateChild(TextPointer position, Type childType, bool throwIfIllegalChild, bool throwIfIllegalHyperlinkDescendent)
	{
		DependencyObject parent = position.Parent;
		if (parent == null)
		{
			TextElement adjacentElementFromOuterPosition = position.GetAdjacentElementFromOuterPosition(LogicalDirection.Backward);
			TextElement adjacentElementFromOuterPosition2 = position.GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
			if (adjacentElementFromOuterPosition == null || IsValidSibling(adjacentElementFromOuterPosition.GetType(), childType))
			{
				if (adjacentElementFromOuterPosition2 != null)
				{
					return IsValidSibling(adjacentElementFromOuterPosition2.GetType(), childType);
				}
				return true;
			}
			return false;
		}
		if (parent is TextElement)
		{
			return ValidateChild((TextElement)parent, childType, throwIfIllegalChild, throwIfIllegalHyperlinkDescendent);
		}
		bool num = IsValidChild(parent.GetType(), childType);
		if (!num && throwIfIllegalChild)
		{
			throw new InvalidOperationException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, parent.GetType().Name, childType.Name));
		}
		return num;
	}

	internal static bool IsValidSibling(Type siblingType, Type newType)
	{
		if (typeof(Inline).IsAssignableFrom(newType))
		{
			return typeof(Inline).IsAssignableFrom(siblingType);
		}
		if (typeof(Block).IsAssignableFrom(newType))
		{
			return typeof(Block).IsAssignableFrom(siblingType);
		}
		if (typeof(TableRowGroup).IsAssignableFrom(newType))
		{
			return typeof(TableRowGroup).IsAssignableFrom(siblingType);
		}
		if (typeof(TableRow).IsAssignableFrom(newType))
		{
			return typeof(TableRow).IsAssignableFrom(siblingType);
		}
		if (typeof(TableCell).IsAssignableFrom(newType))
		{
			return typeof(TableCell).IsAssignableFrom(siblingType);
		}
		if (typeof(ListItem).IsAssignableFrom(newType))
		{
			return typeof(ListItem).IsAssignableFrom(siblingType);
		}
		Invariant.Assert(condition: false, "unexpected value for newType");
		return false;
	}

	internal static bool IsValidChild(ITextPointer position, Type childType)
	{
		if (typeof(TextElement).IsAssignableFrom(position.ParentType) && TextPointerBase.IsInHyperlinkScope(position) && (typeof(Hyperlink).IsAssignableFrom(childType) || typeof(AnchoredBlock).IsAssignableFrom(childType)))
		{
			return false;
		}
		return IsValidChild(position.ParentType, childType);
	}

	internal static bool IsValidChildOfContainer(Type parentType, Type childType)
	{
		Invariant.Assert(!typeof(TextElement).IsAssignableFrom(parentType));
		return IsValidChild(parentType, childType);
	}

	internal static bool HasHyperlinkAncestor(TextElement element)
	{
		Inline inline = element as Inline;
		while (inline != null && !(inline is Hyperlink))
		{
			inline = inline.Parent as Inline;
		}
		return inline != null;
	}

	internal static bool IsFormattingType(Type elementType)
	{
		if (!typeof(Run).IsAssignableFrom(elementType))
		{
			return typeof(Span).IsAssignableFrom(elementType);
		}
		return true;
	}

	internal static bool IsKnownType(Type elementType)
	{
		if (!(elementType.Module == typeof(TextElement).Module))
		{
			return elementType.Module == typeof(UIElement).Module;
		}
		return true;
	}

	internal static bool IsNonFormattingInline(Type elementType)
	{
		if (typeof(Inline).IsAssignableFrom(elementType))
		{
			return !IsFormattingType(elementType);
		}
		return false;
	}

	internal static bool IsMergeableInline(Type elementType)
	{
		if (IsFormattingType(elementType))
		{
			return !IsNonMergeableInline(elementType);
		}
		return false;
	}

	internal static bool IsNonMergeableInline(Type elementType)
	{
		TextElementEditingBehaviorAttribute textElementEditingBehaviorAttribute = (TextElementEditingBehaviorAttribute)Attribute.GetCustomAttribute(elementType, typeof(TextElementEditingBehaviorAttribute));
		if (textElementEditingBehaviorAttribute != null && !textElementEditingBehaviorAttribute.IsMergeable)
		{
			return true;
		}
		return false;
	}

	internal static bool AllowsParagraphMerging(Type elementType)
	{
		if (!typeof(Paragraph).IsAssignableFrom(elementType) && !typeof(ListItem).IsAssignableFrom(elementType) && !typeof(List).IsAssignableFrom(elementType))
		{
			return typeof(Section).IsAssignableFrom(elementType);
		}
		return true;
	}

	internal static bool IsParagraphOrBlockUIContainer(Type elementType)
	{
		if (!typeof(Paragraph).IsAssignableFrom(elementType))
		{
			return typeof(BlockUIContainer).IsAssignableFrom(elementType);
		}
		return true;
	}

	internal static bool IsBlock(Type type)
	{
		return typeof(Block).IsAssignableFrom(type);
	}

	internal static bool IsBreak(Type type)
	{
		return typeof(LineBreak).IsAssignableFrom(type);
	}

	internal static bool HasTextDecorations(object value)
	{
		if (value is TextDecorationCollection)
		{
			return ((TextDecorationCollection)value).Count > 0;
		}
		return false;
	}

	internal static Type GetStandardElementType(Type type, bool reduceElement)
	{
		if (typeof(Run).IsAssignableFrom(type))
		{
			return typeof(Run);
		}
		if (typeof(Hyperlink).IsAssignableFrom(type))
		{
			return typeof(Hyperlink);
		}
		if (typeof(Span).IsAssignableFrom(type))
		{
			return typeof(Span);
		}
		if (typeof(InlineUIContainer).IsAssignableFrom(type))
		{
			if (!reduceElement)
			{
				return typeof(InlineUIContainer);
			}
			return typeof(Run);
		}
		if (typeof(LineBreak).IsAssignableFrom(type))
		{
			return typeof(LineBreak);
		}
		if (typeof(Floater).IsAssignableFrom(type))
		{
			return typeof(Floater);
		}
		if (typeof(Figure).IsAssignableFrom(type))
		{
			return typeof(Figure);
		}
		if (typeof(Paragraph).IsAssignableFrom(type))
		{
			return typeof(Paragraph);
		}
		if (typeof(Section).IsAssignableFrom(type))
		{
			return typeof(Section);
		}
		if (typeof(List).IsAssignableFrom(type))
		{
			return typeof(List);
		}
		if (typeof(Table).IsAssignableFrom(type))
		{
			return typeof(Table);
		}
		if (typeof(BlockUIContainer).IsAssignableFrom(type))
		{
			if (!reduceElement)
			{
				return typeof(BlockUIContainer);
			}
			return typeof(Paragraph);
		}
		if (typeof(ListItem).IsAssignableFrom(type))
		{
			return typeof(ListItem);
		}
		if (typeof(TableColumn).IsAssignableFrom(type))
		{
			return typeof(TableColumn);
		}
		if (typeof(TableRowGroup).IsAssignableFrom(type))
		{
			return typeof(TableRowGroup);
		}
		if (typeof(TableRow).IsAssignableFrom(type))
		{
			return typeof(TableRow);
		}
		if (typeof(TableCell).IsAssignableFrom(type))
		{
			return typeof(TableCell);
		}
		Invariant.Assert(condition: false, "We do not expect any unknown elements derived directly from TextElement, Block or Inline. Schema must have been checking for that");
		return null;
	}

	internal static DependencyProperty[] GetInheritableProperties(Type type)
	{
		if (typeof(TableCell).IsAssignableFrom(type))
		{
			return _inheritableTableCellProperties;
		}
		if (typeof(Block).IsAssignableFrom(type) || typeof(FlowDocument).IsAssignableFrom(type))
		{
			return _inheritableBlockProperties;
		}
		Invariant.Assert(typeof(TextElement).IsAssignableFrom(type) || typeof(TableColumn).IsAssignableFrom(type), "type must be one of TextElement, FlowDocument or TableColumn");
		return _inheritableTextElementProperties;
	}

	internal static DependencyProperty[] GetNoninheritableProperties(Type type)
	{
		if (typeof(Run).IsAssignableFrom(type))
		{
			return _inlineProperties;
		}
		if (typeof(Hyperlink).IsAssignableFrom(type))
		{
			return _hyperlinkProperties;
		}
		if (typeof(Span).IsAssignableFrom(type))
		{
			return _inlineProperties;
		}
		if (typeof(InlineUIContainer).IsAssignableFrom(type))
		{
			return _inlineProperties;
		}
		if (typeof(LineBreak).IsAssignableFrom(type))
		{
			return _emptyPropertyList;
		}
		if (typeof(Floater).IsAssignableFrom(type))
		{
			return _floaterProperties;
		}
		if (typeof(Figure).IsAssignableFrom(type))
		{
			return _figureProperties;
		}
		if (typeof(Paragraph).IsAssignableFrom(type))
		{
			return _paragraphProperties;
		}
		if (typeof(Section).IsAssignableFrom(type))
		{
			return _blockProperties;
		}
		if (typeof(List).IsAssignableFrom(type))
		{
			return _listProperties;
		}
		if (typeof(Table).IsAssignableFrom(type))
		{
			return _tableProperties;
		}
		if (typeof(BlockUIContainer).IsAssignableFrom(type))
		{
			return _blockProperties;
		}
		if (typeof(ListItem).IsAssignableFrom(type))
		{
			return _listItemProperties;
		}
		if (typeof(TableColumn).IsAssignableFrom(type))
		{
			return _tableColumnProperties;
		}
		if (typeof(TableRowGroup).IsAssignableFrom(type))
		{
			return _tableRowGroupProperties;
		}
		if (typeof(TableRow).IsAssignableFrom(type))
		{
			return _tableRowProperties;
		}
		if (typeof(TableCell).IsAssignableFrom(type))
		{
			return _tableCellProperties;
		}
		Invariant.Assert(condition: false, "We do not expect any unknown elements derived directly from TextElement. Schema must have been checking for that");
		return _emptyPropertyList;
	}

	internal static bool ValuesAreEqual(object value1, object value2)
	{
		if (value1 == value2)
		{
			return true;
		}
		if (value1 == null)
		{
			if (value2 is TextDecorationCollection)
			{
				return ((TextDecorationCollection)value2).Count == 0;
			}
			if (value2 is TextEffectCollection)
			{
				return ((TextEffectCollection)value2).Count == 0;
			}
			return false;
		}
		if (value2 == null)
		{
			if (value1 is TextDecorationCollection)
			{
				return ((TextDecorationCollection)value1).Count == 0;
			}
			if (value1 is TextEffectCollection)
			{
				return ((TextEffectCollection)value1).Count == 0;
			}
			return false;
		}
		if (value1.GetType() != value2.GetType())
		{
			return false;
		}
		if (value1 is TextDecorationCollection)
		{
			TextDecorationCollection obj = (TextDecorationCollection)value1;
			TextDecorationCollection textDecorations = (TextDecorationCollection)value2;
			return obj.ValueEquals(textDecorations);
		}
		if (value1 is FontFamily)
		{
			FontFamily obj2 = (FontFamily)value1;
			FontFamily obj3 = (FontFamily)value2;
			return obj2.Equals(obj3);
		}
		if (value1 is Brush)
		{
			return AreBrushesEqual((Brush)value1, (Brush)value2);
		}
		string? text = value1.ToString();
		string text2 = value2.ToString();
		return text == text2;
	}

	internal static bool IsParagraphProperty(DependencyProperty formattingProperty)
	{
		for (int i = 0; i < _inheritableBlockProperties.Length; i++)
		{
			if (formattingProperty == _inheritableBlockProperties[i])
			{
				return true;
			}
		}
		for (int j = 0; j < _paragraphProperties.Length; j++)
		{
			if (formattingProperty == _paragraphProperties[j])
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsCharacterProperty(DependencyProperty formattingProperty)
	{
		for (int i = 0; i < _inheritableTextElementProperties.Length; i++)
		{
			if (formattingProperty == _inheritableTextElementProperties[i])
			{
				return true;
			}
		}
		for (int j = 0; j < _inlineProperties.Length; j++)
		{
			if (formattingProperty == _inlineProperties[j])
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsNonFormattingCharacterProperty(DependencyProperty property)
	{
		for (int i = 0; i < _nonFormattingCharacterProperties.Length; i++)
		{
			if (property == _nonFormattingCharacterProperties[i])
			{
				return true;
			}
		}
		return false;
	}

	internal static DependencyProperty[] GetNonFormattingCharacterProperties()
	{
		return _nonFormattingCharacterProperties;
	}

	internal static bool IsStructuralCharacterProperty(DependencyProperty formattingProperty)
	{
		int i;
		for (i = 0; i < _structuralCharacterProperties.Length && formattingProperty != _structuralCharacterProperties[i]; i++)
		{
		}
		return i < _structuralCharacterProperties.Length;
	}

	internal static bool IsPropertyIncremental(DependencyProperty property)
	{
		if (property == null)
		{
			return false;
		}
		Type propertyType = property.PropertyType;
		if (!typeof(double).IsAssignableFrom(propertyType) && !typeof(long).IsAssignableFrom(propertyType) && !typeof(int).IsAssignableFrom(propertyType))
		{
			return typeof(Thickness).IsAssignableFrom(propertyType);
		}
		return true;
	}

	private static bool IsValidChild(Type parentType, Type childType)
	{
		if (parentType == null || typeof(Run).IsAssignableFrom(parentType) || typeof(TextBox).IsAssignableFrom(parentType) || typeof(PasswordBox).IsAssignableFrom(parentType))
		{
			return childType == typeof(string);
		}
		if (typeof(TextBlock).IsAssignableFrom(parentType))
		{
			if (typeof(Inline).IsAssignableFrom(childType))
			{
				return !typeof(AnchoredBlock).IsAssignableFrom(childType);
			}
			return false;
		}
		if (typeof(Hyperlink).IsAssignableFrom(parentType))
		{
			if (typeof(Inline).IsAssignableFrom(childType) && !typeof(Hyperlink).IsAssignableFrom(childType))
			{
				return !typeof(AnchoredBlock).IsAssignableFrom(childType);
			}
			return false;
		}
		if (typeof(Span).IsAssignableFrom(parentType) || typeof(Paragraph).IsAssignableFrom(parentType) || typeof(AccessText).IsAssignableFrom(parentType))
		{
			return typeof(Inline).IsAssignableFrom(childType);
		}
		if (typeof(InlineUIContainer).IsAssignableFrom(parentType))
		{
			return typeof(UIElement).IsAssignableFrom(childType);
		}
		if (typeof(List).IsAssignableFrom(parentType))
		{
			return typeof(ListItem).IsAssignableFrom(childType);
		}
		if (typeof(Table).IsAssignableFrom(parentType))
		{
			return typeof(TableRowGroup).IsAssignableFrom(childType);
		}
		if (typeof(TableRowGroup).IsAssignableFrom(parentType))
		{
			return typeof(TableRow).IsAssignableFrom(childType);
		}
		if (typeof(TableRow).IsAssignableFrom(parentType))
		{
			return typeof(TableCell).IsAssignableFrom(childType);
		}
		if (typeof(Section).IsAssignableFrom(parentType) || typeof(ListItem).IsAssignableFrom(parentType) || typeof(TableCell).IsAssignableFrom(parentType) || typeof(Floater).IsAssignableFrom(parentType) || typeof(Figure).IsAssignableFrom(parentType) || typeof(FlowDocument).IsAssignableFrom(parentType))
		{
			return typeof(Block).IsAssignableFrom(childType);
		}
		if (typeof(BlockUIContainer).IsAssignableFrom(parentType))
		{
			return typeof(UIElement).IsAssignableFrom(childType);
		}
		return false;
	}

	private static bool HasIllegalHyperlinkDescendant(TextElement element, bool throwIfIllegalDescendent)
	{
		TextPointer textPointer = element.ElementStart;
		TextPointer elementEnd = element.ElementEnd;
		while (textPointer.CompareTo(elementEnd) < 0)
		{
			if (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
			{
				TextElement textElement = (TextElement)textPointer.GetAdjacentElement(LogicalDirection.Forward);
				if (textElement is Hyperlink || textElement is AnchoredBlock)
				{
					if (throwIfIllegalDescendent)
					{
						throw new InvalidOperationException(SR.Format(SR.TextSchema_IllegalHyperlinkChild, textElement.GetType()));
					}
					return true;
				}
			}
			textPointer = textPointer.GetNextContextPosition(LogicalDirection.Forward);
		}
		return false;
	}

	private static bool AreBrushesEqual(Brush brush1, Brush brush2)
	{
		if (brush1 is SolidColorBrush { Color: var color })
		{
			return color.Equals(((SolidColorBrush)brush2).Color);
		}
		string stringValue = DPTypeDescriptorContext.GetStringValue(TextElement.BackgroundProperty, brush1);
		string stringValue2 = DPTypeDescriptorContext.GetStringValue(TextElement.BackgroundProperty, brush2);
		if (stringValue == null || stringValue2 == null)
		{
			return false;
		}
		return stringValue == stringValue2;
	}
}
