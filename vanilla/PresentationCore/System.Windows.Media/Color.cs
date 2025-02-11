using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Describes a color in terms of alpha, red, green, and blue channels. </summary>
[TypeConverter(typeof(ColorConverter))]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public struct Color : IFormattable, IEquatable<Color>
{
	private struct MILColorF
	{
		public float a;

		public float r;

		public float g;

		public float b;

		public override int GetHashCode()
		{
			return a.GetHashCode() ^ r.GetHashCode() ^ g.GetHashCode() ^ b.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
	}

	private struct MILColor
	{
		public byte a;

		public byte r;

		public byte g;

		public byte b;
	}

	[MarshalAs(UnmanagedType.Interface)]
	private ColorContext context;

	private MILColorF scRgbColor;

	private MILColor sRgbColor;

	private float[] nativeColorValue;

	private bool isFromScRgb;

	private const string c_scRgbFormat = "R";

	/// <summary>Gets the International Color Consortium (ICC) or Image Color Management (ICM) color profile of the color.</summary>
	/// <returns>The International Color Consortium (ICC) or Image Color Management (ICM) color profile of the color.</returns>
	public ColorContext ColorContext => context;

	/// <summary>Gets or sets the sRGB alpha channel value of the color. </summary>
	/// <returns>The sRGB alpha channel value of the color.</returns>
	public byte A
	{
		get
		{
			return sRgbColor.a;
		}
		set
		{
			scRgbColor.a = (float)(int)value / 255f;
			sRgbColor.a = value;
		}
	}

	/// <summary>Gets or sets the sRGB red channel value of the color. </summary>
	/// <returns>The sRGB red channel value of the current <see cref="T:System.Windows.Media.Color" /> structure.</returns>
	public byte R
	{
		get
		{
			return sRgbColor.r;
		}
		set
		{
			if (context == null || context.ColorSpaceFamily == ColorContext.StandardColorSpace.Srgb || context.ColorSpaceFamily == ColorContext.StandardColorSpace.ScRgb)
			{
				scRgbColor.r = sRgbToScRgb(value);
				sRgbColor.r = value;
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.Color_ColorContextNotsRGB_or_scRGB, null));
		}
	}

	/// <summary>Gets or sets the sRGB green channel value of the color. </summary>
	/// <returns>The sRGB green channel value of the current <see cref="T:System.Windows.Media.Color" /> structure.</returns>
	public byte G
	{
		get
		{
			return sRgbColor.g;
		}
		set
		{
			if (context == null || context.ColorSpaceFamily == ColorContext.StandardColorSpace.Srgb || context.ColorSpaceFamily == ColorContext.StandardColorSpace.ScRgb)
			{
				scRgbColor.g = sRgbToScRgb(value);
				sRgbColor.g = value;
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.Color_ColorContextNotsRGB_or_scRGB, null));
		}
	}

	/// <summary>Gets or sets the sRGB blue channel value of the color. </summary>
	/// <returns>The sRGB blue channel value of the current <see cref="T:System.Windows.Media.Color" /> structure.</returns>
	public byte B
	{
		get
		{
			return sRgbColor.b;
		}
		set
		{
			if (context == null || context.ColorSpaceFamily == ColorContext.StandardColorSpace.Srgb || context.ColorSpaceFamily == ColorContext.StandardColorSpace.ScRgb)
			{
				scRgbColor.b = sRgbToScRgb(value);
				sRgbColor.b = value;
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.Color_ColorContextNotsRGB_or_scRGB, null));
		}
	}

	/// <summary>Gets or sets the ScRGB alpha channel value of the color. </summary>
	/// <returns>The ScRGB alpha channel value of the current <see cref="T:System.Windows.Media.Color" /> structure.</returns>
	public float ScA
	{
		get
		{
			return scRgbColor.a;
		}
		set
		{
			scRgbColor.a = value;
			if (value < 0f)
			{
				sRgbColor.a = 0;
			}
			else if (value > 1f)
			{
				sRgbColor.a = byte.MaxValue;
			}
			else
			{
				sRgbColor.a = (byte)(value * 255f);
			}
		}
	}

	/// <summary>Gets or sets the ScRGB red channel value of the color. </summary>
	/// <returns>The ScRGB red channel value of the current <see cref="T:System.Windows.Media.Color" /> structure.</returns>
	public float ScR
	{
		get
		{
			return scRgbColor.r;
		}
		set
		{
			if (context == null || context.ColorSpaceFamily == ColorContext.StandardColorSpace.Srgb || context.ColorSpaceFamily == ColorContext.StandardColorSpace.ScRgb)
			{
				scRgbColor.r = value;
				sRgbColor.r = ScRgbTosRgb(value);
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.Color_ColorContextNotsRGB_or_scRGB, null));
		}
	}

	/// <summary>Gets or sets the ScRGB green channel value of the color. </summary>
	/// <returns>The ScRGB green channel value of the current <see cref="T:System.Windows.Media.Color" /> structure.</returns>
	public float ScG
	{
		get
		{
			return scRgbColor.g;
		}
		set
		{
			if (context == null || context.ColorSpaceFamily == ColorContext.StandardColorSpace.Srgb || context.ColorSpaceFamily == ColorContext.StandardColorSpace.ScRgb)
			{
				scRgbColor.g = value;
				sRgbColor.g = ScRgbTosRgb(value);
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.Color_ColorContextNotsRGB_or_scRGB, null));
		}
	}

	/// <summary>Gets or sets the ScRGB blue channel value of the color. </summary>
	/// <returns>The ScRGB red channel value of the current <see cref="T:System.Windows.Media.Color" /> structure.</returns>
	public float ScB
	{
		get
		{
			return scRgbColor.b;
		}
		set
		{
			if (context == null || context.ColorSpaceFamily == ColorContext.StandardColorSpace.Srgb || context.ColorSpaceFamily == ColorContext.StandardColorSpace.ScRgb)
			{
				scRgbColor.b = value;
				sRgbColor.b = ScRgbTosRgb(value);
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.Color_ColorContextNotsRGB_or_scRGB, null));
		}
	}

	private static Color FromProfile(Uri profileUri)
	{
		Color result = default(Color);
		result.context = new ColorContext(profileUri);
		result.scRgbColor.a = 1f;
		result.scRgbColor.r = 0f;
		result.scRgbColor.g = 0f;
		result.scRgbColor.b = 0f;
		result.sRgbColor.a = byte.MaxValue;
		result.sRgbColor.r = 0;
		result.sRgbColor.g = 0;
		result.sRgbColor.b = 0;
		if (result.context != null)
		{
			result.nativeColorValue = new float[result.context.NumChannels];
			for (int i = 0; i < result.nativeColorValue.GetLength(0); i++)
			{
				result.nativeColorValue[i] = 0f;
			}
		}
		result.isFromScRgb = false;
		return result;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Color" /> structure by using the specified alpha channel, color channel values, and color profile.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Color" /> structure with the specified values.</returns>
	/// <param name="a">The alpha channel for the new color.</param>
	/// <param name="values">A collection of values that specify the color channels for the new color. These values map to the <paramref name="profileUri" />.</param>
	/// <param name="profileUri">The International Color Consortium (ICC) or Image Color Management (ICM) color profile for the new color. </param>
	public static Color FromAValues(float a, float[] values, Uri profileUri)
	{
		Color result = FromProfile(profileUri);
		if (values == null)
		{
			throw new ArgumentException(SR.Format(SR.Color_DimensionMismatch, null));
		}
		if (values.GetLength(0) != result.nativeColorValue.GetLength(0))
		{
			throw new ArgumentException(SR.Format(SR.Color_DimensionMismatch, null));
		}
		for (int i = 0; i < values.GetLength(0); i++)
		{
			result.nativeColorValue[i] = values[i];
		}
		result.ComputeScRgbValues();
		result.scRgbColor.a = a;
		if (a < 0f)
		{
			a = 0f;
		}
		else if (a > 1f)
		{
			a = 1f;
		}
		result.sRgbColor.a = (byte)(a * 255f + 0.5f);
		result.sRgbColor.r = ScRgbTosRgb(result.scRgbColor.r);
		result.sRgbColor.g = ScRgbTosRgb(result.scRgbColor.g);
		result.sRgbColor.b = ScRgbTosRgb(result.scRgbColor.b);
		return result;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Color" /> structure by using the specified color channel values and color profile.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Color" /> structure with the specified values and an alpha channel value of 1.</returns>
	/// <param name="values">A collection of values that specify the color channels for the new color. These values map to the <paramref name="profileUri" />.</param>
	/// <param name="profileUri">The International Color Consortium (ICC) or Image Color Management (ICM) color profile for the new color.</param>
	public static Color FromValues(float[] values, Uri profileUri)
	{
		return FromAValues(1f, values, profileUri);
	}

	internal static Color FromUInt32(uint argb)
	{
		Color result = default(Color);
		result.sRgbColor.a = (byte)((argb & 0xFF000000u) >> 24);
		result.sRgbColor.r = (byte)((argb & 0xFF0000) >> 16);
		result.sRgbColor.g = (byte)((argb & 0xFF00) >> 8);
		result.sRgbColor.b = (byte)(argb & 0xFF);
		result.scRgbColor.a = (float)(int)result.sRgbColor.a / 255f;
		result.scRgbColor.r = sRgbToScRgb(result.sRgbColor.r);
		result.scRgbColor.g = sRgbToScRgb(result.sRgbColor.g);
		result.scRgbColor.b = sRgbToScRgb(result.sRgbColor.b);
		result.context = null;
		result.isFromScRgb = false;
		return result;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Color" /> structure by using the specified ScRGB alpha channel and color channel values. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Color" /> structure with the specified values.</returns>
	/// <param name="a">The ScRGB alpha channel, <see cref="P:System.Windows.Media.Color.ScA" />, of the new color.</param>
	/// <param name="r">The ScRGB red channel, <see cref="P:System.Windows.Media.Color.ScR" />, of the new color.</param>
	/// <param name="g">The ScRGB green channel, <see cref="P:System.Windows.Media.Color.ScG" />, of the new color.</param>
	/// <param name="b">The ScRGB blue channel, <see cref="P:System.Windows.Media.Color.ScB" />, of the new color.</param>
	public static Color FromScRgb(float a, float r, float g, float b)
	{
		Color result = default(Color);
		result.scRgbColor.r = r;
		result.scRgbColor.g = g;
		result.scRgbColor.b = b;
		result.scRgbColor.a = a;
		if (a < 0f)
		{
			a = 0f;
		}
		else if (a > 1f)
		{
			a = 1f;
		}
		result.sRgbColor.a = (byte)(a * 255f + 0.5f);
		result.sRgbColor.r = ScRgbTosRgb(result.scRgbColor.r);
		result.sRgbColor.g = ScRgbTosRgb(result.scRgbColor.g);
		result.sRgbColor.b = ScRgbTosRgb(result.scRgbColor.b);
		result.context = null;
		result.isFromScRgb = true;
		return result;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Color" /> structure by using the specified sRGB alpha channel and color channel values. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Color" /> structure with the specified values.</returns>
	/// <param name="a">The alpha channel, <see cref="P:System.Windows.Media.Color.A" />, of the new color.</param>
	/// <param name="r">The red channel, <see cref="P:System.Windows.Media.Color.R" />, of the new color.</param>
	/// <param name="g">The green channel, <see cref="P:System.Windows.Media.Color.G" />, of the new color.</param>
	/// <param name="b">The blue channel, <see cref="P:System.Windows.Media.Color.B" />, of the new color.</param>
	public static Color FromArgb(byte a, byte r, byte g, byte b)
	{
		Color result = default(Color);
		result.scRgbColor.a = (float)(int)a / 255f;
		result.scRgbColor.r = sRgbToScRgb(r);
		result.scRgbColor.g = sRgbToScRgb(g);
		result.scRgbColor.b = sRgbToScRgb(b);
		result.context = null;
		result.sRgbColor.a = a;
		result.sRgbColor.r = ScRgbTosRgb(result.scRgbColor.r);
		result.sRgbColor.g = ScRgbTosRgb(result.scRgbColor.g);
		result.sRgbColor.b = ScRgbTosRgb(result.scRgbColor.b);
		result.isFromScRgb = false;
		return result;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Color" /> structure by using the specified sRGB color channel values. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Color" /> structure with the specified values and an alpha channel value of 1.</returns>
	/// <param name="r">The sRGB red channel, <see cref="P:System.Windows.Media.Color.R" />, of the new color.</param>
	/// <param name="g">The sRGB green channel, <see cref="P:System.Windows.Media.Color.G" />, of the new color.</param>
	/// <param name="b">The sRGB blue channel, <see cref="P:System.Windows.Media.Color.B" />, of the new color.</param>
	public static Color FromRgb(byte r, byte g, byte b)
	{
		return FromArgb(byte.MaxValue, r, g, b);
	}

	/// <summary>Gets a hash code for the current <see cref="T:System.Windows.Media.Color" /> structure. </summary>
	/// <returns>A hash code for the current <see cref="T:System.Windows.Media.Color" /> structure.</returns>
	public override int GetHashCode()
	{
		return scRgbColor.GetHashCode();
	}

	/// <summary>Creates a string representation of the color using the ScRGB channels. </summary>
	/// <returns>The string representation of the color.</returns>
	public override string ToString()
	{
		string format = (isFromScRgb ? "R" : null);
		return ConvertToString(format, null);
	}

	/// <summary>Creates a string representation of the color by using the ScRGB channels and the specified format provider. </summary>
	/// <returns>The string representation of the color.</returns>
	/// <param name="provider">Culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		string format = (isFromScRgb ? "R" : null);
		return ConvertToString(format, provider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		return ConvertToString(format, provider);
	}

	internal string ConvertToString(string format, IFormatProvider provider)
	{
		if (context == null)
		{
			DefaultInterpolatedStringHandler handler;
			if (format == null)
			{
				Span<char> initialBuffer = stackalloc char[128];
				handler = new DefaultInterpolatedStringHandler(1, 4, provider, initialBuffer);
				handler.AppendLiteral("#");
				handler.AppendFormatted(sRgbColor.a, "X2");
				handler.AppendFormatted(sRgbColor.r, "X2");
				handler.AppendFormatted(sRgbColor.g, "X2");
				handler.AppendFormatted(sRgbColor.b, "X2");
				return string.Create(provider, initialBuffer, ref handler);
			}
			char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
			handler = new DefaultInterpolatedStringHandler(31, 4);
			handler.AppendLiteral("sc#{1:");
			handler.AppendFormatted(format);
			handler.AppendLiteral("}{0} {2:");
			handler.AppendFormatted(format);
			handler.AppendLiteral("}{0} {3:");
			handler.AppendFormatted(format);
			handler.AppendLiteral("}{0} {4:");
			handler.AppendFormatted(format);
			handler.AppendLiteral("}");
			return string.Format(provider, handler.ToStringAndClear(), numericListSeparator, scRgbColor.a, scRgbColor.r, scRgbColor.g, scRgbColor.b);
		}
		char numericListSeparator2 = TokenizerHelper.GetNumericListSeparator(provider);
		format = "R";
		string components = new Uri(context.ProfileUri.GetComponents(UriComponents.SerializationInfoString, UriFormat.SafeUnescaped), context.ProfileUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative).GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(provider, "{0}{1} ", "ContextColor ", components);
		stringBuilder.AppendFormat(provider, "{1:" + format + "}{0}", numericListSeparator2, scRgbColor.a);
		for (int i = 0; i < nativeColorValue.GetLength(0); i++)
		{
			stringBuilder.AppendFormat(provider, "{0:" + format + "}", nativeColorValue[i]);
			if (i < nativeColorValue.GetLength(0) - 1)
			{
				stringBuilder.AppendFormat(provider, "{0}", numericListSeparator2);
			}
		}
		return stringBuilder.ToString();
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Color" /> structures for fuzzy equality. </summary>
	/// <returns>true if <paramref name="color1" /> and <paramref name="color2" /> are nearly identical; otherwise, false.</returns>
	/// <param name="color1">The first color to compare.</param>
	/// <param name="color2">The second color to compare.</param>
	public static bool AreClose(Color color1, Color color2)
	{
		return color1.IsClose(color2);
	}

	private bool IsClose(Color color)
	{
		bool flag = true;
		if (context == null || color.nativeColorValue == null)
		{
			flag = flag && FloatUtil.AreClose(scRgbColor.r, color.scRgbColor.r) && FloatUtil.AreClose(scRgbColor.g, color.scRgbColor.g) && FloatUtil.AreClose(scRgbColor.b, color.scRgbColor.b);
		}
		else
		{
			for (int i = 0; i < color.nativeColorValue.GetLength(0); i++)
			{
				flag = flag && FloatUtil.AreClose(nativeColorValue[i], color.nativeColorValue[i]);
			}
		}
		if (flag)
		{
			return FloatUtil.AreClose(scRgbColor.a, color.scRgbColor.a);
		}
		return false;
	}

	/// <summary>Sets the ScRGB channels of the color to within the gamut of 0 to 1, if they are outside that range. </summary>
	public void Clamp()
	{
		scRgbColor.r = ((scRgbColor.r < 0f) ? 0f : ((scRgbColor.r > 1f) ? 1f : scRgbColor.r));
		scRgbColor.g = ((scRgbColor.g < 0f) ? 0f : ((scRgbColor.g > 1f) ? 1f : scRgbColor.g));
		scRgbColor.b = ((scRgbColor.b < 0f) ? 0f : ((scRgbColor.b > 1f) ? 1f : scRgbColor.b));
		scRgbColor.a = ((scRgbColor.a < 0f) ? 0f : ((scRgbColor.a > 1f) ? 1f : scRgbColor.a));
		sRgbColor.a = (byte)(scRgbColor.a * 255f);
		sRgbColor.r = ScRgbTosRgb(scRgbColor.r);
		sRgbColor.g = ScRgbTosRgb(scRgbColor.g);
		sRgbColor.b = ScRgbTosRgb(scRgbColor.b);
	}

	/// <summary>Gets the color channel values of the color.</summary>
	/// <returns>An array of color channel values.</returns>
	public float[] GetNativeColorValues()
	{
		if (context != null)
		{
			return (float[])nativeColorValue.Clone();
		}
		throw new InvalidOperationException(SR.Format(SR.Color_NullColorContext, null));
	}

	/// <summary>Adds two <see cref="T:System.Windows.Media.Color" /> structures. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Color" /> structure whose color values are the results of the addition operation.</returns>
	/// <param name="color1">The first <see cref="T:System.Windows.Media.Color" /> structure to add.</param>
	/// <param name="color2">The second <see cref="T:System.Windows.Media.Color" /> structure to add.</param>
	public static Color operator +(Color color1, Color color2)
	{
		if (color1.context == null && color2.context == null)
		{
			return FromScRgb(color1.scRgbColor.a + color2.scRgbColor.a, color1.scRgbColor.r + color2.scRgbColor.r, color1.scRgbColor.g + color2.scRgbColor.g, color1.scRgbColor.b + color2.scRgbColor.b);
		}
		if (color1.context == color2.context)
		{
			Color result = default(Color);
			result.context = color1.context;
			result.nativeColorValue = new float[result.context.NumChannels];
			for (int i = 0; i < result.nativeColorValue.GetLength(0); i++)
			{
				result.nativeColorValue[i] = color1.nativeColorValue[i] + color2.nativeColorValue[i];
			}
			Color color3 = FromRgb(0, 0, 0);
			color3.context = new ColorContext(PixelFormats.Bgra32);
			ColorTransform colorTransform = new ColorTransform(result.context, color3.context);
			float[] array = new float[3];
			colorTransform.Translate(result.nativeColorValue, array);
			if (array[0] < 0f)
			{
				result.sRgbColor.r = 0;
			}
			else if (array[0] > 1f)
			{
				result.sRgbColor.r = byte.MaxValue;
			}
			else
			{
				result.sRgbColor.r = (byte)(array[0] * 255f + 0.5f);
			}
			if (array[1] < 0f)
			{
				result.sRgbColor.g = 0;
			}
			else if (array[1] > 1f)
			{
				result.sRgbColor.g = byte.MaxValue;
			}
			else
			{
				result.sRgbColor.g = (byte)(array[1] * 255f + 0.5f);
			}
			if (array[2] < 0f)
			{
				result.sRgbColor.b = 0;
			}
			else if (array[2] > 1f)
			{
				result.sRgbColor.b = byte.MaxValue;
			}
			else
			{
				result.sRgbColor.b = (byte)(array[2] * 255f + 0.5f);
			}
			result.scRgbColor.r = sRgbToScRgb(result.sRgbColor.r);
			result.scRgbColor.g = sRgbToScRgb(result.sRgbColor.g);
			result.scRgbColor.b = sRgbToScRgb(result.sRgbColor.b);
			result.scRgbColor.a = color1.scRgbColor.a + color2.scRgbColor.a;
			if (result.scRgbColor.a < 0f)
			{
				result.scRgbColor.a = 0f;
				result.sRgbColor.a = 0;
			}
			else if (result.scRgbColor.a > 1f)
			{
				result.scRgbColor.a = 1f;
				result.sRgbColor.a = byte.MaxValue;
			}
			else
			{
				result.sRgbColor.a = (byte)(result.scRgbColor.a * 255f + 0.5f);
			}
			return result;
		}
		throw new ArgumentException(SR.Format(SR.Color_ColorContextTypeMismatch, null));
	}

	/// <summary>Adds two <see cref="T:System.Windows.Media.Color" /> structures. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Color" /> structure whose color values are the results of the addition operation.</returns>
	/// <param name="color1">The first <see cref="T:System.Windows.Media.Color" /> structure to add.</param>
	/// <param name="color2">The second <see cref="T:System.Windows.Media.Color" /> structure to add.</param>
	public static Color Add(Color color1, Color color2)
	{
		return color1 + color2;
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Color" /> structure from a <see cref="T:System.Windows.Media.Color" /> structure. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Color" /> structure whose color values are the results of the subtraction operation.</returns>
	/// <param name="color1">The <see cref="T:System.Windows.Media.Color" /> structure to be subtracted from.</param>
	/// <param name="color2">The <see cref="T:System.Windows.Media.Color" /> structure to subtract from <paramref name="color1" />.</param>
	public static Color operator -(Color color1, Color color2)
	{
		if (color1.context == null && color2.context == null)
		{
			return FromScRgb(color1.scRgbColor.a - color2.scRgbColor.a, color1.scRgbColor.r - color2.scRgbColor.r, color1.scRgbColor.g - color2.scRgbColor.g, color1.scRgbColor.b - color2.scRgbColor.b);
		}
		if (color1.context == null || color2.context == null)
		{
			throw new ArgumentException(SR.Format(SR.Color_ColorContextTypeMismatch, null));
		}
		if (color1.context == color2.context)
		{
			Color result = default(Color);
			result.context = color1.context;
			result.nativeColorValue = new float[result.context.NumChannels];
			for (int i = 0; i < result.nativeColorValue.GetLength(0); i++)
			{
				result.nativeColorValue[i] = color1.nativeColorValue[i] - color2.nativeColorValue[i];
			}
			Color color3 = FromRgb(0, 0, 0);
			color3.context = new ColorContext(PixelFormats.Bgra32);
			ColorTransform colorTransform = new ColorTransform(result.context, color3.context);
			float[] array = new float[3];
			colorTransform.Translate(result.nativeColorValue, array);
			if (array[0] < 0f)
			{
				result.sRgbColor.r = 0;
			}
			else if (array[0] > 1f)
			{
				result.sRgbColor.r = byte.MaxValue;
			}
			else
			{
				result.sRgbColor.r = (byte)(array[0] * 255f + 0.5f);
			}
			if (array[1] < 0f)
			{
				result.sRgbColor.g = 0;
			}
			else if (array[1] > 1f)
			{
				result.sRgbColor.g = byte.MaxValue;
			}
			else
			{
				result.sRgbColor.g = (byte)(array[1] * 255f + 0.5f);
			}
			if (array[2] < 0f)
			{
				result.sRgbColor.b = 0;
			}
			else if (array[2] > 1f)
			{
				result.sRgbColor.b = byte.MaxValue;
			}
			else
			{
				result.sRgbColor.b = (byte)(array[2] * 255f + 0.5f);
			}
			result.scRgbColor.r = sRgbToScRgb(result.sRgbColor.r);
			result.scRgbColor.g = sRgbToScRgb(result.sRgbColor.g);
			result.scRgbColor.b = sRgbToScRgb(result.sRgbColor.b);
			result.scRgbColor.a = color1.scRgbColor.a - color2.scRgbColor.a;
			if (result.scRgbColor.a < 0f)
			{
				result.scRgbColor.a = 0f;
				result.sRgbColor.a = 0;
			}
			else if (result.scRgbColor.a > 1f)
			{
				result.scRgbColor.a = 1f;
				result.sRgbColor.a = byte.MaxValue;
			}
			else
			{
				result.sRgbColor.a = (byte)(result.scRgbColor.a * 255f + 0.5f);
			}
			return result;
		}
		throw new ArgumentException(SR.Format(SR.Color_ColorContextTypeMismatch, null));
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Color" /> structure from a <see cref="T:System.Windows.Media.Color" /> structure. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Color" /> structure whose color values are the results of the subtraction operation.</returns>
	/// <param name="color1">The <see cref="T:System.Windows.Media.Color" /> structure to be subtracted from.</param>
	/// <param name="color2">The <see cref="T:System.Windows.Media.Color" /> structure to subtract from <paramref name="color1" />.</param>
	public static Color Subtract(Color color1, Color color2)
	{
		return color1 - color2;
	}

	/// <summary>Multiplies the alpha, red, blue, and green channels of the specified <see cref="T:System.Windows.Media.Color" /> structure by the specified value. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Color" /> structure whose color values are the results of the multiplication operation.</returns>
	/// <param name="color">The <see cref="T:System.Windows.Media.Color" /> to be multiplied.</param>
	/// <param name="coefficient">The value to multiply by.</param>
	public static Color operator *(Color color, float coefficient)
	{
		Color result = FromScRgb(color.scRgbColor.a * coefficient, color.scRgbColor.r * coefficient, color.scRgbColor.g * coefficient, color.scRgbColor.b * coefficient);
		if (color.context == null)
		{
			return result;
		}
		result.context = color.context;
		result.ComputeNativeValues(result.context.NumChannels);
		return result;
	}

	/// <summary>Multiplies the alpha, red, blue, and green channels of the specified <see cref="T:System.Windows.Media.Color" /> structure by the specified value. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Color" /> structure whose color values are the results of the multiplication operation.</returns>
	/// <param name="color">The <see cref="T:System.Windows.Media.Color" /> to be multiplied.</param>
	/// <param name="coefficient">The value to multiply by.</param>
	public static Color Multiply(Color color, float coefficient)
	{
		return color * coefficient;
	}

	/// <summary>Tests whether two <see cref="T:System.Windows.Media.Color" /> structures are identical. </summary>
	/// <returns>true if <paramref name="color1" /> and <paramref name="color2" /> are exactly identical; otherwise, false.</returns>
	/// <param name="color1">The first <see cref="T:System.Windows.Media.Color" /> structure to compare.</param>
	/// <param name="color2">The second <see cref="T:System.Windows.Media.Color" /> structure to compare.</param>
	public static bool Equals(Color color1, Color color2)
	{
		return color1 == color2;
	}

	/// <summary>Tests whether the specified <see cref="T:System.Windows.Media.Color" /> structure is identical to the current color.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Media.Color" /> structure is identical to the current <see cref="T:System.Windows.Media.Color" /> structure; otherwise, false.</returns>
	/// <param name="color">The <see cref="T:System.Windows.Media.Color" /> structure to compare to the current <see cref="T:System.Windows.Media.Color" /> structure.</param>
	public bool Equals(Color color)
	{
		return this == color;
	}

	/// <summary>Tests whether the specified object is a <see cref="T:System.Windows.Media.Color" /> structure and is equivalent to the current color. </summary>
	/// <returns>true if the specified object is a <see cref="T:System.Windows.Media.Color" /> structure and is identical to the current <see cref="T:System.Windows.Media.Color" /> structure; otherwise, false.</returns>
	/// <param name="o">The object to compare to the current <see cref="T:System.Windows.Media.Color" /> structure.</param>
	public override bool Equals(object o)
	{
		if (o is Color color)
		{
			return this == color;
		}
		return false;
	}

	/// <summary>Tests whether two <see cref="T:System.Windows.Media.Color" /> structures are identical. </summary>
	/// <returns>true if <paramref name="color1" /> and <paramref name="color2" /> are exactly identical; otherwise, false.</returns>
	/// <param name="color1">The first <see cref="T:System.Windows.Media.Color" /> structure to compare.</param>
	/// <param name="color2">The second <see cref="T:System.Windows.Media.Color" /> structure to compare.</param>
	public static bool operator ==(Color color1, Color color2)
	{
		if (color1.context == null && color2.context == null)
		{
			if (color1.scRgbColor.r != color2.scRgbColor.r)
			{
				return false;
			}
			if (color1.scRgbColor.g != color2.scRgbColor.g)
			{
				return false;
			}
			if (color1.scRgbColor.b != color2.scRgbColor.b)
			{
				return false;
			}
			if (color1.scRgbColor.a != color2.scRgbColor.a)
			{
				return false;
			}
			return true;
		}
		if (color1.context == null || color2.context == null)
		{
			return false;
		}
		if (color1.context.ColorSpaceFamily == color2.context.ColorSpaceFamily)
		{
			if (color1.nativeColorValue == null && color2.nativeColorValue == null)
			{
				return true;
			}
			if (color1.nativeColorValue == null || color2.nativeColorValue == null)
			{
				return false;
			}
			if (color1.nativeColorValue.GetLength(0) != color2.nativeColorValue.GetLength(0))
			{
				return false;
			}
			for (int i = 0; i < color1.nativeColorValue.GetLength(0); i++)
			{
				if (color1.nativeColorValue[i] != color2.nativeColorValue[i])
				{
					return false;
				}
			}
			if (color1.scRgbColor.a != color2.scRgbColor.a)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>Tests whether two <see cref="T:System.Windows.Media.Color" /> structures are not identical. </summary>
	/// <returns>true if <paramref name="color1" /> and <paramref name="color2" /> are not equal; otherwise, false.</returns>
	/// <param name="color1">The first <see cref="T:System.Windows.Media.Color" /> structure to compare.</param>
	/// <param name="color2">The second <see cref="T:System.Windows.Media.Color" /> structure to compare.</param>
	public static bool operator !=(Color color1, Color color2)
	{
		return !(color1 == color2);
	}

	private static float sRgbToScRgb(byte bval)
	{
		float num = (float)(int)bval / 255f;
		if (!((double)num > 0.0))
		{
			return 0f;
		}
		if ((double)num <= 0.04045)
		{
			return num / 12.92f;
		}
		if (num < 1f)
		{
			return (float)Math.Pow(((double)num + 0.055) / 1.055, 2.4);
		}
		return 1f;
	}

	private static byte ScRgbTosRgb(float val)
	{
		if (!((double)val > 0.0))
		{
			return 0;
		}
		if ((double)val <= 0.0031308)
		{
			return (byte)(255f * val * 12.92f + 0.5f);
		}
		if ((double)val < 1.0)
		{
			return (byte)(255f * (1.055f * (float)Math.Pow(val, 5.0 / 12.0) - 0.055f) + 0.5f);
		}
		return byte.MaxValue;
	}

	private void ComputeScRgbValues()
	{
		if (context != null)
		{
			Color color = FromRgb(0, 0, 0);
			color.context = new ColorContext(PixelFormats.Bgra32);
			ColorTransform colorTransform = new ColorTransform(context, color.context);
			float[] array = new float[3];
			colorTransform.Translate(nativeColorValue, array);
			scRgbColor.r = sRgbToScRgb((byte)(255f * array[0] + 0.5f));
			scRgbColor.g = sRgbToScRgb((byte)(255f * array[1] + 0.5f));
			scRgbColor.b = sRgbToScRgb((byte)(255f * array[2] + 0.5f));
		}
	}

	private void ComputeNativeValues(int numChannels)
	{
		nativeColorValue = new float[numChannels];
		if (nativeColorValue.GetLength(0) > 0)
		{
			float[] srcValue = new float[3]
			{
				(float)(int)sRgbColor.r / 255f,
				(float)(int)sRgbColor.g / 255f,
				(float)(int)sRgbColor.b / 255f
			};
			new ColorTransform(context, new ColorContext(PixelFormats.Bgra32)).Translate(srcValue, nativeColorValue);
		}
	}
}
