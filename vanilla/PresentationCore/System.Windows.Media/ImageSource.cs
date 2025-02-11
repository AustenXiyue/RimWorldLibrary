using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Represents a object type that has a width, height, and <see cref="T:System.Windows.Media.ImageMetadata" /> such as a  <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> and a <see cref="T:System.Windows.Media.DrawingImage" />. This is an abstract class.</summary>
[TypeConverter(typeof(ImageSourceConverter))]
[ValueSerializer(typeof(ImageSourceValueSerializer))]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class ImageSource : Animatable, IFormattable, DUCE.IResource
{
	/// <summary>When overridden in a derived class, gets the width of the image in measure units (96ths of an inch). </summary>
	/// <returns>The width of the image in measure units (96ths of an inch).</returns>
	public abstract double Width { get; }

	/// <summary>When overridden in a derived class, gets the height of the image in measure units (96ths of an inch). </summary>
	/// <returns>The height of the image.</returns>
	public abstract double Height { get; }

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Windows.Media.ImageMetadata" /> associated with the image.</summary>
	/// <returns>The metadata associated with the image.</returns>
	public abstract ImageMetadata Metadata { get; }

	internal virtual Size Size => new Size(Width, Height);

	internal ImageSource()
	{
	}

	internal virtual bool CanSerializeToString()
	{
		return false;
	}

	/// <summary>Converts pixels to DIPs in a way that is consistent with MIL.</summary>
	/// <returns>The natural size of the bitmap in MIL Device Independent Pixels (DIPs, or 1/96").</returns>
	/// <param name="dpi">Width of the bitmap, in dots per inch. </param>
	/// <param name="pixels">Width of the bitmap, in pixels.</param>
	protected static double PixelsToDIPs(double dpi, int pixels)
	{
		float num = (float)dpi;
		if (num < 0f || FloatUtil.IsCloseToDivideByZero(96f, num))
		{
			return pixels;
		}
		return (float)pixels * (96f / num);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ImageSource" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ImageSource Clone()
	{
		return (ImageSource)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ImageSource" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ImageSource CloneCurrentValue()
	{
		return (ImageSource)base.CloneCurrentValue();
	}

	internal abstract DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return AddRefOnChannelCore(channel);
		}
	}

	internal abstract void ReleaseOnChannelCore(DUCE.Channel channel);

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			ReleaseOnChannelCore(channel);
		}
	}

	internal abstract DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return GetHandleCore(channel);
		}
	}

	internal abstract int GetChannelCountCore();

	int DUCE.IResource.GetChannelCount()
	{
		return GetChannelCountCore();
	}

	internal abstract DUCE.Channel GetChannelCore(int index);

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return GetChannelCore(index);
	}

	/// <summary> Creates a string representation of this object based on the current culture. </summary>
	/// <returns>A string representation of this object.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of this object based on the <see cref="T:System.IFormatProvider" /> passed in. If the provider is null, the <see cref="P:System.Globalization.CultureInfo.CurrentCulture" /> is used. </summary>
	/// <returns>A string representation of this object.</returns>
	/// <param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information.  </param>
	public string ToString(IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(null, provider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(format, provider);
	}

	internal virtual string ConvertToString(string format, IFormatProvider provider)
	{
		return base.ToString();
	}
}
