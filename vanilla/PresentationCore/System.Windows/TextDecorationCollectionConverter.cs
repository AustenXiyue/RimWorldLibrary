using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Converts instances of <see cref="T:System.Windows.TextDecorationCollection" /> from other data types.</summary>
public sealed class TextDecorationCollectionConverter : TypeConverter
{
	private const string None = "NONE";

	private const char Separator = ',';

	private static readonly string[] TextDecorationNames = new string[4] { "OVERLINE", "BASELINE", "UNDERLINE", "STRIKETHROUGH" };

	private static readonly TextDecorationCollection[] PredefinedTextDecorations = new TextDecorationCollection[4]
	{
		TextDecorations.OverLine,
		TextDecorations.Baseline,
		TextDecorations.Underline,
		TextDecorations.Strikethrough
	};

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.TextDecorationCollection" /> can be converted to a different type.</summary>
	/// <returns>false is always returned because the <see cref="T:System.Windows.TextDecorationCollection" /> cannot be converted to another type.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="destinationType">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether this converter can convert an object of the given type to an instance of <see cref="T:System.Windows.TextDecorationCollection" />.</summary>
	/// <returns>true if the converter can convert the provided type to an instance of <see cref="T:System.Windows.TextDecorationCollection" />; otherwise, false.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="sourceType">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Attempts to convert a specified object to an instance of <see cref="T:System.Windows.TextDecorationCollection" />.</summary>
	/// <returns>The instance of <see cref="T:System.Windows.FontWeight" /> created from the converted <paramref name="input" />.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted. </param>
	/// <param name="input">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">Occurs if <paramref name="input" /> is null or is not a valid type for conversion.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object input)
	{
		if (input == null)
		{
			throw GetConvertFromException(input);
		}
		if (!(input is string text))
		{
			throw new ArgumentException(SR.Format(SR.General_BadType, "ConvertFrom"), "input");
		}
		return ConvertFromString(text);
	}

	/// <summary>Attempts to convert a specified string to an instance of <see cref="T:System.Windows.TextDecorationCollection" />.</summary>
	/// <returns>The instance of <see cref="T:System.Windows.TextDecorationCollection" /> created from the converted <paramref name="text" />.</returns>
	/// <param name="text">The <see cref="T:System.String" /> to be converted into the <see cref="T:System.Windows.TextDecorationCollection" /> object.</param>
	public new static TextDecorationCollection ConvertFromString(string text)
	{
		if (text == null)
		{
			return null;
		}
		TextDecorationCollection textDecorationCollection = new TextDecorationCollection();
		byte b = 0;
		int num = AdvanceToNextNonWhiteSpace(text, 0);
		while (num >= 0 && num < text.Length)
		{
			if (Match("NONE", text, num))
			{
				num = AdvanceToNextNonWhiteSpace(text, num + "NONE".Length);
				if (textDecorationCollection.Count > 0 || num < text.Length)
				{
					num = -1;
				}
				continue;
			}
			int i;
			for (i = 0; i < TextDecorationNames.Length && !Match(TextDecorationNames[i], text, num); i++)
			{
			}
			if (i < TextDecorationNames.Length)
			{
				if ((b & (1 << i)) > 0)
				{
					num = -1;
					continue;
				}
				textDecorationCollection.Add(PredefinedTextDecorations[i]);
				b |= (byte)(1 << i);
				num = AdvanceToNextNameStart(text, num + TextDecorationNames[i].Length);
			}
			else
			{
				num = -1;
			}
		}
		if (num < 0)
		{
			throw new ArgumentException(SR.Format(SR.InvalidTextDecorationCollectionString, text));
		}
		return textDecorationCollection;
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.TextDecorationCollection" /> to a specified type.</summary>
	/// <returns>null is always returned because <see cref="T:System.Windows.TextDecorationCollection" /> cannot be converted to any other type.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The instance of <see cref="T:System.Windows.TextDecorationCollection" /> to convert.</param>
	/// <param name="destinationType">The type this instance of <see cref="T:System.Windows.TextDecorationCollection" /> is converted to.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) && value is IEnumerable<TextDecoration>)
		{
			return new InstanceDescriptor(typeof(TextDecorationCollection).GetConstructor(new Type[1] { typeof(IEnumerable<TextDecoration>) }), new object[1] { value });
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private static bool Match(string pattern, string input, int index)
	{
		int i;
		for (i = 0; i < pattern.Length && index + i < input.Length && pattern[i] == char.ToUpperInvariant(input[index + i]); i++)
		{
		}
		return i == pattern.Length;
	}

	private static int AdvanceToNextNameStart(string input, int index)
	{
		int num = AdvanceToNextNonWhiteSpace(input, index);
		int num2;
		if (num >= input.Length)
		{
			num2 = input.Length;
		}
		else if (input[num] == ',')
		{
			num2 = AdvanceToNextNonWhiteSpace(input, num + 1);
			if (num2 >= input.Length)
			{
				num2 = -1;
			}
		}
		else
		{
			num2 = -1;
		}
		return num2;
	}

	private static int AdvanceToNextNonWhiteSpace(string input, int index)
	{
		while (index < input.Length && char.IsWhiteSpace(input[index]))
		{
			index++;
		}
		if (index <= input.Length)
		{
			return index;
		}
		return input.Length;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TextDecorationCollectionConverter" /> class.</summary>
	public TextDecorationCollectionConverter()
	{
	}
}
