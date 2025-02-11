using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>Converts an integer to and from an object by applying the integer as an index to a list of objects.</summary>
[ContentProperty("Values")]
public class AlternationConverter : IValueConverter
{
	private List<object> _values = new List<object>();

	/// <summary>Gets a list of objects that the <see cref="T:System.Windows.Controls.AlternationConverter" /> returns when an integer is passed to the <see cref="M:System.Windows.Controls.AlternationConverter.Convert(System.Object,System.Type,System.Object,System.Globalization.CultureInfo)" /> method.</summary>
	/// <returns>A list of objects that the <see cref="T:System.Windows.Controls.AlternationConverter" /> returns when an integer is passed to the <see cref="M:System.Windows.Controls.AlternationConverter.Convert(System.Object,System.Type,System.Object,System.Globalization.CultureInfo)" /> method.</returns>
	public IList Values => _values;

	/// <summary>Converts an integer to an object in the <see cref="P:System.Windows.Controls.AlternationConverter.Values" /> list.</summary>
	/// <returns>The object that is in the position of <paramref name="o" /> modulo the number of items in <see cref="P:System.Windows.Controls.AlternationConverter.Values" />.</returns>
	/// <param name="o">The integer to use to find an object in the <see cref="P:System.Windows.Controls.AlternationConverter.Values" /> property. </param>
	/// <param name="targetType">The type of the binding target property.</param>
	/// <param name="parameter">The converter parameter to use.</param>
	/// <param name="culture">The culture to use in the converter.</param>
	public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
	{
		if (_values.Count > 0 && o is int)
		{
			int num = (int)o % _values.Count;
			if (num < 0)
			{
				num += _values.Count;
			}
			return _values[num];
		}
		return DependencyProperty.UnsetValue;
	}

	/// <summary>Converts an object in the <see cref="P:System.Windows.Controls.AlternationConverter.Values" /> list to an integer.</summary>
	/// <returns>The index of <paramref name="o" /> if it is in <see cref="P:System.Windows.Controls.AlternationConverter.Values" />, or –1 if o does not exist in <see cref="P:System.Windows.Controls.AlternationConverter.Values" />.</returns>
	/// <param name="o">The object to find in the <see cref="P:System.Windows.Controls.AlternationConverter.Values" /> property. </param>
	/// <param name="targetType">The type of the binding target property.</param>
	/// <param name="parameter">The converter parameter to use.</param>
	/// <param name="culture">The culture to use in the converter.</param>
	public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
	{
		return _values.IndexOf(o);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.AlternationConverter" /> class. </summary>
	public AlternationConverter()
	{
	}
}
