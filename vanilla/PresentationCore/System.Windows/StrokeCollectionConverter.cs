using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Ink;

namespace System.Windows;

/// <summary>Converts a <see cref="T:System.Windows.Ink.StrokeCollection" /> to a string.</summary>
public class StrokeCollectionConverter : TypeConverter
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.StrokeCollectionConverter" /> class. </summary>
	public StrokeCollectionConverter()
	{
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.StrokeCollectionConverter" /> can convert an object of a specified type to a <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.StrokeCollectionConverter" /> can convert an object of type <paramref name="sourceType" /> to a <see cref="T:System.Windows.Ink.StrokeCollection" />; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides the format context.</param>
	/// <param name="sourceType">The <see cref="T:System.Type" /> to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.StrokeCollectionConverter" /> can convert a <see cref="T:System.Windows.Ink.StrokeCollection" /> to the specified type.</summary>
	/// <returns>true if the <see cref="T:System.Windows.StrokeCollectionConverter" /> can convert a <see cref="T:System.Windows.Ink.StrokeCollection" /> to the <paramref name="sourceType" />; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides the format context.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> converted from <paramref name="value" />.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			string text2 = text.Trim();
			if (text2.Length != 0)
			{
				using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(text2)))
				{
					return new StrokeCollection(stream);
				}
			}
			return new StrokeCollection();
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts a <see cref="T:System.Windows.Ink.StrokeCollection" /> to a string.</summary>
	/// <returns>An object that represents the specified <see cref="T:System.Windows.Ink.StrokeCollection" />.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value is StrokeCollection strokeCollection)
		{
			if (destinationType == typeof(string))
			{
				using MemoryStream memoryStream = new MemoryStream();
				strokeCollection.Save(memoryStream, compress: true);
				memoryStream.Position = 0L;
				return Convert.ToBase64String(memoryStream.ToArray());
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				ConstructorInfo? constructor = typeof(StrokeCollection).GetConstructor(new Type[1] { typeof(Stream) });
				MemoryStream memoryStream2 = new MemoryStream();
				strokeCollection.Save(memoryStream2, compress: true);
				memoryStream2.Position = 0L;
				return new InstanceDescriptor(constructor, new object[1] { memoryStream2 });
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Returns whether this object supports a standard set of values that can be picked from a list, using the specified context. </summary>
	/// <returns>false in all cases.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return false;
	}
}
