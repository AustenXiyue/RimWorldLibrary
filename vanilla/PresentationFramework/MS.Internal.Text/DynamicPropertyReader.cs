using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace MS.Internal.Text;

internal static class DynamicPropertyReader
{
	internal static Typeface GetTypeface(DependencyObject element)
	{
		FontFamily fontFamily = (FontFamily)element.GetValue(TextElement.FontFamilyProperty);
		FontStyle style = (FontStyle)element.GetValue(TextElement.FontStyleProperty);
		FontWeight weight = (FontWeight)element.GetValue(TextElement.FontWeightProperty);
		FontStretch stretch = (FontStretch)element.GetValue(TextElement.FontStretchProperty);
		return new Typeface(fontFamily, style, weight, stretch);
	}

	internal static Typeface GetModifiedTypeface(DependencyObject element, FontFamily fontFamily)
	{
		FontStyle style = (FontStyle)element.GetValue(TextElement.FontStyleProperty);
		FontWeight weight = (FontWeight)element.GetValue(TextElement.FontWeightProperty);
		FontStretch stretch = (FontStretch)element.GetValue(TextElement.FontStretchProperty);
		return new Typeface(fontFamily, style, weight, stretch);
	}

	internal static TextDecorationCollection GetTextDecorationsForInlineObject(DependencyObject element, TextDecorationCollection textDecorations)
	{
		DependencyObject parent = LogicalTreeHelper.GetParent(element);
		TextDecorationCollection textDecorationCollection = null;
		if (parent != null)
		{
			textDecorationCollection = GetTextDecorations(parent);
		}
		if (!(textDecorations?.ValueEquals(textDecorationCollection) ?? (textDecorationCollection == null)))
		{
			if (textDecorationCollection == null)
			{
				textDecorations = null;
			}
			else
			{
				textDecorations = new TextDecorationCollection();
				int count = textDecorationCollection.Count;
				for (int i = 0; i < count; i++)
				{
					textDecorations.Add(textDecorationCollection[i]);
				}
			}
		}
		return textDecorations;
	}

	internal static TextDecorationCollection GetTextDecorations(DependencyObject element)
	{
		return GetCollectionValue(element, Inline.TextDecorationsProperty) as TextDecorationCollection;
	}

	internal static TextEffectCollection GetTextEffects(DependencyObject element)
	{
		return GetCollectionValue(element, TextElement.TextEffectsProperty) as TextEffectCollection;
	}

	private static object GetCollectionValue(DependencyObject element, DependencyProperty property)
	{
		if (element.GetValueSource(property, null, out var hasModifiers) != BaseValueSourceInternal.Default || hasModifiers)
		{
			return element.GetValue(property);
		}
		return null;
	}

	internal static bool GetKeepTogether(DependencyObject element)
	{
		if (!(element is Paragraph paragraph))
		{
			return false;
		}
		return paragraph.KeepTogether;
	}

	internal static bool GetKeepWithNext(DependencyObject element)
	{
		if (!(element is Paragraph paragraph))
		{
			return false;
		}
		return paragraph.KeepWithNext;
	}

	internal static int GetMinWidowLines(DependencyObject element)
	{
		if (!(element is Paragraph paragraph))
		{
			return 0;
		}
		return paragraph.MinWidowLines;
	}

	internal static int GetMinOrphanLines(DependencyObject element)
	{
		if (!(element is Paragraph paragraph))
		{
			return 0;
		}
		return paragraph.MinOrphanLines;
	}

	internal static double GetLineHeightValue(DependencyObject d)
	{
		double num = (double)d.GetValue(Block.LineHeightProperty);
		if (double.IsNaN(num))
		{
			FontFamily obj = (FontFamily)d.GetValue(TextElement.FontFamilyProperty);
			double num2 = (double)d.GetValue(TextElement.FontSizeProperty);
			num = obj.LineSpacing * num2;
		}
		return Math.Max(TextDpi.MinWidth, Math.Min(TextDpi.MaxWidth, num));
	}

	internal static Brush GetBackgroundBrush(DependencyObject element)
	{
		Brush brush = null;
		while (brush == null && CanApplyBackgroundBrush(element))
		{
			brush = (Brush)element.GetValue(TextElement.BackgroundProperty);
			Invariant.Assert(element is FrameworkContentElement);
			element = ((FrameworkContentElement)element).Parent;
		}
		return brush;
	}

	internal static Brush GetBackgroundBrushForInlineObject(StaticTextPointer position)
	{
		if (position.TextContainer.Highlights.GetHighlightValue(position, LogicalDirection.Forward, typeof(TextSelection)) == DependencyProperty.UnsetValue)
		{
			return (Brush)position.GetValue(TextElement.BackgroundProperty);
		}
		return SelectionHighlightInfo.BackgroundBrush;
	}

	internal static BaselineAlignment GetBaselineAlignment(DependencyObject element)
	{
		Inline inline = element as Inline;
		BaselineAlignment result = inline?.BaselineAlignment ?? BaselineAlignment.Baseline;
		while (inline != null && BaselineAlignmentIsDefault(inline))
		{
			inline = inline.Parent as Inline;
		}
		if (inline != null)
		{
			result = inline.BaselineAlignment;
		}
		return result;
	}

	internal static BaselineAlignment GetBaselineAlignmentForInlineObject(DependencyObject element)
	{
		return GetBaselineAlignment(LogicalTreeHelper.GetParent(element));
	}

	internal static CultureInfo GetCultureInfo(DependencyObject element)
	{
		XmlLanguage xmlLanguage = (XmlLanguage)element.GetValue(FrameworkElement.LanguageProperty);
		try
		{
			return xmlLanguage.GetSpecificCulture();
		}
		catch (InvalidOperationException)
		{
			return System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		}
	}

	internal static NumberSubstitution GetNumberSubstitution(DependencyObject element)
	{
		return new NumberSubstitution
		{
			CultureSource = (NumberCultureSource)element.GetValue(NumberSubstitution.CultureSourceProperty),
			CultureOverride = (CultureInfo)element.GetValue(NumberSubstitution.CultureOverrideProperty),
			Substitution = (NumberSubstitutionMethod)element.GetValue(NumberSubstitution.SubstitutionProperty)
		};
	}

	private static bool CanApplyBackgroundBrush(DependencyObject element)
	{
		if (!(element is Inline) || element is AnchoredBlock)
		{
			return false;
		}
		return true;
	}

	private static bool BaselineAlignmentIsDefault(DependencyObject element)
	{
		Invariant.Assert(element != null);
		if (element.GetValueSource(Inline.BaselineAlignmentProperty, null, out var hasModifiers) != BaseValueSourceInternal.Default || hasModifiers)
		{
			return false;
		}
		return true;
	}
}
