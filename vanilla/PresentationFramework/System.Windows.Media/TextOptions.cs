using MS.Internal.Media;

namespace System.Windows.Media;

/// <summary>Defines a set of attached properties that affect the way text is displayed in an element.</summary>
public static class TextOptions
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.TextOptions.TextFormattingMode" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TextOptions.TextFormattingMode" /> attached property.</returns>
	public static readonly DependencyProperty TextFormattingModeProperty = DependencyProperty.RegisterAttached("TextFormattingMode", typeof(TextFormattingMode), typeof(TextOptions), new FrameworkPropertyMetadata(TextFormattingMode.Ideal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits), IsTextFormattingModeValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TextOptions.TextRenderingMode" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TextOptions.TextRenderingMode" /> attached property.</returns>
	public static readonly DependencyProperty TextRenderingModeProperty = DependencyProperty.RegisterAttached("TextRenderingMode", typeof(TextRenderingMode), typeof(TextOptions), new FrameworkPropertyMetadata(TextRenderingMode.Auto, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits), ValidateEnums.IsTextRenderingModeValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TextOptions.TextHintingMode" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TextOptions.TextHintingMode" /> attached property.</returns>
	public static readonly DependencyProperty TextHintingModeProperty = TextOptionsInternal.TextHintingModeProperty.AddOwner(typeof(TextOptions));

	internal static bool IsTextFormattingModeValid(object valueObject)
	{
		TextFormattingMode textFormattingMode = (TextFormattingMode)valueObject;
		if (textFormattingMode != 0)
		{
			return textFormattingMode == TextFormattingMode.Display;
		}
		return true;
	}

	/// <summary>Sets the <see cref="T:System.Windows.Media.TextFormattingMode" /> for the specified element. </summary>
	/// <param name="element">The element to set the <see cref="T:System.Windows.Media.TextFormattingMode" /> for.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextFormattingMode" /> to set on <paramref name="element" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	public static void SetTextFormattingMode(DependencyObject element, TextFormattingMode value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TextFormattingModeProperty, value);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Media.TextFormattingMode" /> for the specified element.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextFormattingMode" /> for <paramref name="element" />.</returns>
	/// <param name="element">The element to get the <see cref="T:System.Windows.Media.TextFormattingMode" /> for.</param>
	public static TextFormattingMode GetTextFormattingMode(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (TextFormattingMode)element.GetValue(TextFormattingModeProperty);
	}

	/// <summary>Sets the <see cref="T:System.Windows.Media.TextRenderingMode" /> for the specified element.</summary>
	/// <param name="element">The element to set the <see cref="T:System.Windows.Media.TextRenderingMode" /> for.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextRenderingMode" /> to set on <paramref name="element" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	public static void SetTextRenderingMode(DependencyObject element, TextRenderingMode value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TextRenderingModeProperty, value);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Media.TextRenderingMode" /> for the specified element.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextRenderingMode" /> for <paramref name="element" />.</returns>
	/// <param name="element">The element to get the <see cref="T:System.Windows.Media.TextRenderingMode" /> for.</param>
	public static TextRenderingMode GetTextRenderingMode(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (TextRenderingMode)element.GetValue(TextRenderingModeProperty);
	}

	/// <summary>Sets the <see cref="T:System.Windows.Media.TextHintingMode" /> for the specified element.</summary>
	/// <param name="element">The element to set the <see cref="T:System.Windows.Media.TextHintingMode" /> for.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextHintingMode" /> to set on <paramref name="element" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	public static void SetTextHintingMode(DependencyObject element, TextHintingMode value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TextHintingModeProperty, value);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Media.TextHintingMode" /> for the specified element.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextHintingMode" /> for <paramref name="element" />.</returns>
	/// <param name="element">The element to get the <see cref="T:System.Windows.Media.TextHintingMode" />  for.</param>
	public static TextHintingMode GetTextHintingMode(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (TextHintingMode)element.GetValue(TextHintingModeProperty);
	}
}
