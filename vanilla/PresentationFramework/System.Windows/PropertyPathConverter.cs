using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Provides a type converter for <see cref="T:System.Windows.PropertyPath" /> objects. </summary>
public sealed class PropertyPathConverter : TypeConverter
{
	/// <summary>Returns whether this converter can convert an object of one type to the <see cref="T:System.Windows.PropertyPath" /> type.</summary>
	/// <returns>true if <paramref name="sourceType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns whether this converter can convert the object to the <see cref="T:System.Windows.PropertyPath" /> type.</summary>
	/// <returns>true if <paramref name="destinationType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to. </param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts the specified value to the <see cref="T:System.Windows.PropertyPath" /> type.</summary>
	/// <returns>The converted <see cref="T:System.Windows.PropertyPath" />.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="source">The object to convert to a <see cref="T:System.Windows.PropertyPath" />. This is expected to be a string.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="source" /> was provided as null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="source" /> was not null, but was not of the expected <see cref="T:System.String" /> type.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source is string)
		{
			return new PropertyPath((string)source, typeDescriptorContext);
		}
		throw new ArgumentException(SR.Format(SR.CannotConvertType, source.GetType().FullName, typeof(PropertyPath)));
	}

	/// <summary>Converts the specified value object to the <see cref="T:System.Windows.PropertyPath" /> type.</summary>
	/// <returns>The converted destination <see cref="T:System.String" />.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
	/// <param name="value">The <see cref="T:System.Windows.PropertyPath" /> to convert.</param>
	/// <param name="destinationType">The destination type. This is expected to be the <see cref="T:System.String" /> type.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> was provided as null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> was not null, but was not of the expected <see cref="T:System.Windows.PropertyPath" /> type.- or -The <paramref name="destinationType" /> was not the <see cref="T:System.String" /> type.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (null == destinationType)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType != typeof(string))
		{
			throw new ArgumentException(SR.Format(SR.CannotConvertType, typeof(PropertyPath), destinationType.FullName));
		}
		if (!(value is PropertyPath propertyPath))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(PropertyPath)), "value");
		}
		if (propertyPath.PathParameters.Count == 0)
		{
			return propertyPath.Path;
		}
		string path = propertyPath.Path;
		Collection<object> pathParameters = propertyPath.PathParameters;
		XamlDesignerSerializationManager obj = ((typeDescriptorContext == null) ? null : (typeDescriptorContext.GetService(typeof(XamlDesignerSerializationManager)) as XamlDesignerSerializationManager));
		ValueSerializer valueSerializer = null;
		IValueSerializerContext valueSerializerContext = null;
		if (obj == null)
		{
			valueSerializerContext = typeDescriptorContext as IValueSerializerContext;
			if (valueSerializerContext != null)
			{
				valueSerializer = ValueSerializer.GetSerializerFor(typeof(Type), valueSerializerContext);
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int i = 0; i < path.Length; i++)
		{
			if (path[i] != '(')
			{
				continue;
			}
			int j;
			for (j = i + 1; j < path.Length && path[j] != ')'; j++)
			{
			}
			if (!int.TryParse(path.AsSpan(i + 1, j - i - 1), NumberStyles.Integer, TypeConverterHelper.InvariantEnglishUS.NumberFormat, out var result))
			{
				continue;
			}
			stringBuilder.Append(path.AsSpan(num, i - num + 1));
			object obj2 = pathParameters[result];
			PropertyPath.DowncastAccessor(obj2, out var dp, out var pi, out var pd, out var doa);
			Type type;
			string text;
			if (dp != null)
			{
				type = dp.OwnerType;
				text = dp.Name;
			}
			else if (pi != null)
			{
				type = pi.DeclaringType;
				text = pi.Name;
			}
			else if (pd != null)
			{
				type = pd.ComponentType;
				text = pd.Name;
			}
			else if (doa != null)
			{
				type = doa.OwnerType;
				text = doa.PropertyName;
			}
			else
			{
				type = obj2.GetType();
				text = null;
			}
			if (valueSerializer != null)
			{
				stringBuilder.Append(valueSerializer.ConvertToString(type, valueSerializerContext));
			}
			else
			{
				string text2 = null;
				if (text2 != null && text2 != string.Empty)
				{
					stringBuilder.Append(text2);
					stringBuilder.Append(':');
				}
				stringBuilder.Append(type.Name);
			}
			if (text != null)
			{
				stringBuilder.Append('.');
				stringBuilder.Append(text);
				stringBuilder.Append(')');
			}
			else
			{
				stringBuilder.Append(')');
				text = obj2 as string;
				if (text == null)
				{
					TypeConverter converter = TypeDescriptor.GetConverter(type);
					if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
					{
						try
						{
							text = converter.ConvertToString(obj2);
						}
						catch (NotSupportedException)
						{
						}
					}
				}
				stringBuilder.Append(text);
			}
			i = j;
			num = j + 1;
		}
		if (num < path.Length)
		{
			stringBuilder.Append(path.AsSpan(num));
		}
		return stringBuilder.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.PropertyPathConverter" /> class.</summary>
	public PropertyPathConverter()
	{
	}
}
