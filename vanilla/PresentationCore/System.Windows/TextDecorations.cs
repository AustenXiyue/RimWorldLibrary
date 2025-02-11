namespace System.Windows;

/// <summary>Provides a set of static predefined text decorations.</summary>
public static class TextDecorations
{
	private static readonly TextDecorationCollection underline;

	private static readonly TextDecorationCollection strikethrough;

	private static readonly TextDecorationCollection overLine;

	private static readonly TextDecorationCollection baseline;

	/// <summary>Specifies an underline <see cref="T:System.Windows.TextDecoration" />.</summary>
	/// <returns>A value that represents an underline <see cref="T:System.Windows.TextDecoration" />.</returns>
	public static TextDecorationCollection Underline => underline;

	/// <summary>Specifies a strikethrough <see cref="T:System.Windows.TextDecoration" />.</summary>
	/// <returns>A value that represents a strikethrough <see cref="T:System.Windows.TextDecoration" />.</returns>
	public static TextDecorationCollection Strikethrough => strikethrough;

	/// <summary>Specifies an overline <see cref="T:System.Windows.TextDecoration" />.</summary>
	/// <returns>A value that represents an overline <see cref="T:System.Windows.TextDecoration" />.</returns>
	public static TextDecorationCollection OverLine => overLine;

	/// <summary>Specifies a baseline <see cref="T:System.Windows.TextDecoration" />.</summary>
	/// <returns>A value that represents a baseline <see cref="T:System.Windows.TextDecoration" />.</returns>
	public static TextDecorationCollection Baseline => baseline;

	static TextDecorations()
	{
		TextDecoration value = new TextDecoration
		{
			Location = TextDecorationLocation.Underline
		};
		underline = new TextDecorationCollection();
		underline.Add(value);
		underline.Freeze();
		value = new TextDecoration
		{
			Location = TextDecorationLocation.Strikethrough
		};
		strikethrough = new TextDecorationCollection();
		strikethrough.Add(value);
		strikethrough.Freeze();
		value = new TextDecoration
		{
			Location = TextDecorationLocation.OverLine
		};
		overLine = new TextDecorationCollection();
		overLine.Add(value);
		overLine.Freeze();
		value = new TextDecoration
		{
			Location = TextDecorationLocation.Baseline
		};
		baseline = new TextDecorationCollection();
		baseline.Add(value);
		baseline.Freeze();
	}
}
