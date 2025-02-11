using System.ComponentModel;
using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Converts a <see cref="T:System.Windows.Input.MouseAction" /> object to and from other types.</summary>
public class MouseActionConverter : TypeConverter
{
	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.Input.MouseAction" />,using the specified context.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="sourceType">The type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Input.MouseAction" /> can be converted to the specified type, using the specified context.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string) && context != null && context.Instance != null)
		{
			return IsDefinedMouseAction((MouseAction)context.Instance);
		}
		return false;
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Input.MouseAction" />, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> does not map to a valid <see cref="T:System.Windows.Input.MouseAction" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> cannot be converted.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		if (source != null && source is string)
		{
			string text = ((string)source).Trim();
			text = text.ToUpper(CultureInfo.InvariantCulture);
			if (text == string.Empty)
			{
				return MouseAction.None;
			}
			MouseAction mouseAction = MouseAction.None;
			return text switch
			{
				"LEFTCLICK" => MouseAction.LeftClick, 
				"RIGHTCLICK" => MouseAction.RightClick, 
				"MIDDLECLICK" => MouseAction.MiddleClick, 
				"WHEELCLICK" => MouseAction.WheelClick, 
				"LEFTDOUBLECLICK" => MouseAction.LeftDoubleClick, 
				"RIGHTDOUBLECLICK" => MouseAction.RightDoubleClick, 
				"MIDDLEDOUBLECLICK" => MouseAction.MiddleDoubleClick, 
				_ => throw new NotSupportedException(SR.Format(SR.Unsupported_MouseAction, text)), 
			};
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.Input.MouseAction" /> to the specified type, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="value" /> does not map to a valid <see cref="T:System.Windows.Input.MouseAction" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted.  </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string) && value != null)
		{
			MouseAction mouseAction = (MouseAction)value;
			if (IsDefinedMouseAction(mouseAction))
			{
				string text = null;
				switch (mouseAction)
				{
				case MouseAction.None:
					text = string.Empty;
					break;
				case MouseAction.LeftClick:
					text = "LeftClick";
					break;
				case MouseAction.RightClick:
					text = "RightClick";
					break;
				case MouseAction.MiddleClick:
					text = "MiddleClick";
					break;
				case MouseAction.WheelClick:
					text = "WheelClick";
					break;
				case MouseAction.LeftDoubleClick:
					text = "LeftDoubleClick";
					break;
				case MouseAction.RightDoubleClick:
					text = "RightDoubleClick";
					break;
				case MouseAction.MiddleDoubleClick:
					text = "MiddleDoubleClick";
					break;
				}
				if (text != null)
				{
					return text;
				}
			}
			throw new InvalidEnumArgumentException("value", (int)mouseAction, typeof(MouseAction));
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static bool IsDefinedMouseAction(MouseAction mouseAction)
	{
		if ((int)mouseAction >= 0)
		{
			return (int)mouseAction <= 7;
		}
		return false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseActionConverter" /> class.</summary>
	public MouseActionConverter()
	{
	}
}
