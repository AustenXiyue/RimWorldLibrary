using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Converts instances of other types to and from a <see cref="T:System.Windows.Media.Animation.KeySpline" />. </summary>
public class KeySplineConverter : TypeConverter
{
	/// <summary>Determines whether an object can be converted from a given type to an instance of a <see cref="T:System.Windows.Media.Animation.KeySpline" />.  </summary>
	/// <returns>true if the type can be converted to a <see cref="T:System.Windows.Media.Animation.KeySpline" />; otherwise, false.</returns>
	/// <param name="typeDescriptor">Describes the context information of a type.</param>
	/// <param name="destinationType">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptor, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary> Determines whether an instance of a <see cref="T:System.Windows.Media.Animation.KeySpline" /> can be converted to a different type. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.Animation.KeySpline" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.Media.Animation.KeySpline" /> is being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Media.Animation.KeySpline" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Animation.KeySpline" /> created from converting <paramref name="value" />.</returns>
	/// <param name="context">Provides contextual information required for conversion.</param>
	/// <param name="cultureInfo">Cultural information to respect during conversion.</param>
	/// <param name="value">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">Thrown if the specified object is NULL or is a type that cannot be converted to a <see cref="T:System.Windows.Media.Animation.KeySpline" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
	{
		string str = value as string;
		if (value == null)
		{
			throw new NotSupportedException(SR.Converter_ConvertFromNotSupported);
		}
		TokenizerHelper tokenizerHelper = new TokenizerHelper(str, cultureInfo);
		return new KeySpline(Convert.ToDouble(tokenizerHelper.NextTokenRequired(), cultureInfo), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), cultureInfo), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), cultureInfo), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), cultureInfo));
	}

	/// <summary> Attempts to convert a <see cref="T:System.Windows.Media.Animation.KeySpline" /> to a specified type. </summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.Media.Animation.KeySpline" />.</returns>
	/// <param name="context">Provides contextual information required for conversion.</param>
	/// <param name="cultureInfo">Cultural information to respect during conversion.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Animation.KeySpline" /> to convert.</param>
	/// <param name="destinationType">The type to convert this <see cref="T:System.Windows.Media.Animation.KeySpline" /> to.</param>
	/// <exception cref="T:System.NotSupportedException">Thrown if <paramref name="value" /> is null or is not a <see cref="T:System.Windows.Media.Animation.KeySpline" />, or if the <paramref name="destinationType" /> is not one of the valid types for conversion.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (value is KeySpline keySpline && destinationType != null)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(KeySpline).GetConstructor(new Type[4]
				{
					typeof(double),
					typeof(double),
					typeof(double),
					typeof(double)
				}), new object[4]
				{
					keySpline.ControlPoint1.X,
					keySpline.ControlPoint1.Y,
					keySpline.ControlPoint2.X,
					keySpline.ControlPoint2.Y
				});
			}
			if (destinationType == typeof(string))
			{
				return string.Format(cultureInfo, "{0}{4}{1}{4}{2}{4}{3}", keySpline.ControlPoint1.X, keySpline.ControlPoint1.Y, keySpline.ControlPoint2.X, keySpline.ControlPoint2.Y, (cultureInfo != null) ? cultureInfo.TextInfo.ListSeparator : CultureInfo.InvariantCulture.TextInfo.ListSeparator);
			}
		}
		return base.ConvertTo(context, cultureInfo, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.KeySplineConverter" /> class. </summary>
	public KeySplineConverter()
	{
	}
}
