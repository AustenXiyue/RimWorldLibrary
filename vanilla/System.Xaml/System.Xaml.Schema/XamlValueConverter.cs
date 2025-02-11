using System.ComponentModel;
using System.Threading;

namespace System.Xaml.Schema;

/// <summary>Provides a common API surface for techniques that generate initialization or serialization values for XAML based on input other than the eventual destination type. This includes markup extensions and type converters.</summary>
/// <typeparam name="TConverterBase">The CLR base class for the particular converter that this <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> represents. Typically this is one of the following: <see cref="T:System.ComponentModel.TypeConverter" />; <see cref="T:System.Windows.Markup.MarkupExtension" />; <see cref="T:System.Windows.Markup.ValueSerializer" />; </typeparam>
public class XamlValueConverter<TConverterBase> : IEquatable<XamlValueConverter<TConverterBase>> where TConverterBase : class
{
	private TConverterBase _instance;

	private ThreeValuedBool _isPublic;

	private volatile bool _instanceIsSet;

	/// <summary>Gets a string name for this <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />.</summary>
	/// <returns>A string name for this <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />.</returns>
	public string Name { get; }

	/// <summary>Gets the <see cref="T:System.Type" /> for the class that implements the converter behavior.</summary>
	/// <returns>The <see cref="T:System.Type" /> for the class that implements the converter behavior.</returns>
	public Type ConverterType { get; }

	/// <summary>Gets the target/destination <see cref="T:System.Xaml.XamlType" /> of the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />.</summary>
	/// <returns>The target/destination <see cref="T:System.Xaml.XamlType" /> of the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />.</returns>
	public XamlType TargetType { get; }

	/// <summary>Gets a created instance of the converter implementation.</summary>
	/// <returns>A created instance of the converter implementation, or null.</returns>
	public TConverterBase ConverterInstance
	{
		get
		{
			if (!_instanceIsSet)
			{
				Interlocked.CompareExchange(ref _instance, CreateInstance(), null);
				_instanceIsSet = true;
			}
			return _instance;
		}
	}

	internal virtual bool IsPublic
	{
		get
		{
			if (_isPublic == ThreeValuedBool.NotSet)
			{
				_isPublic = ((!(ConverterType == null) && !ConverterType.IsVisible) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isPublic == ThreeValuedBool.True;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> class, based on a converter implementing <see cref="T:System.Type" /> and the target/destination type of the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />.</summary>
	/// <param name="converterType">The <see cref="T:System.Type" /> that implements the converter behavior.</param>
	/// <param name="targetType">The target/destination <see cref="T:System.Xaml.XamlType" /> of the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />.</param>
	public XamlValueConverter(Type converterType, XamlType targetType)
		: this(converterType, targetType, (string)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> class, based on a converter implementing <see cref="T:System.Type" /> the target/destination type of the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />, and a string name.</summary>
	/// <param name="converterType">The <see cref="T:System.Type" /> that implements the converter behavior.</param>
	/// <param name="targetType">The target/destination <see cref="T:System.Xaml.XamlType" /> of the <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />.</param>
	/// <param name="name">The string name.</param>
	/// <exception cref="T:System.ArgumentException">All three parameters are null (at least one is required to be non-null).</exception>
	public XamlValueConverter(Type converterType, XamlType targetType, string name)
	{
		if (converterType == null && targetType == null && name == null)
		{
			throw new ArgumentException(System.SR.Format(System.SR.ArgumentRequired, "converterType, targetType, name"));
		}
		ConverterType = converterType;
		TargetType = targetType;
		Name = name ?? GetDefaultName();
	}

	/// <summary>Returns a <see cref="T:System.String" /> that represents this <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />. </summary>
	/// <returns>A <see cref="T:System.String" /> that represents this <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />. </returns>
	public override string ToString()
	{
		return Name;
	}

	/// <summary>Returns an instance of the converter implementation.</summary>
	/// <returns>An instance of the converter implementation, or null.</returns>
	/// <exception cref="T:System.Xaml.XamlSchemaException">Converter did not implement the correct base type.</exception>
	protected virtual TConverterBase CreateInstance()
	{
		if (ConverterType == typeof(EnumConverter) && TargetType.UnderlyingType != null && TargetType.UnderlyingType.IsEnum)
		{
			return (TConverterBase)(object)new EnumConverter(TargetType.UnderlyingType);
		}
		if (ConverterType != null)
		{
			if (!typeof(TConverterBase).IsAssignableFrom(ConverterType))
			{
				throw new XamlSchemaException(System.SR.Format(System.SR.ConverterMustDeriveFromBase, ConverterType, typeof(TConverterBase)));
			}
			return (TConverterBase)Activator.CreateInstance(ConverterType, null);
		}
		return null;
	}

	private string GetDefaultName()
	{
		if (ConverterType != null)
		{
			if (TargetType != null)
			{
				return ConverterType.Name + "(" + TargetType.Name + ")";
			}
			return ConverterType.Name;
		}
		return TargetType.Name;
	}

	/// <summary>Determines whether this instance of <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> and a specified object, which must also be a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> object, have the same value.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> and its value is the same as this instance; otherwise, false. </returns>
	/// <param name="obj">The object to compare.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is XamlValueConverter<TConverterBase> xamlValueConverter))
		{
			return false;
		}
		return this == xamlValueConverter;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />. </summary>
	/// <returns>An integer hash code. </returns>
	public override int GetHashCode()
	{
		int num = Name.GetHashCode();
		if (ConverterType != null)
		{
			num ^= ConverterType.GetHashCode();
		}
		if (TargetType != null)
		{
			num ^= TargetType.GetHashCode();
		}
		return num;
	}

	/// <summary>Determines whether this instance of <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> and another <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> object have the same value.</summary>
	/// <returns>true if <paramref name="other" /> is a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> and its value is the same as this instance; otherwise, false. </returns>
	/// <param name="other">The <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />  to compare.</param>
	public bool Equals(XamlValueConverter<TConverterBase> other)
	{
		return this == other;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> objects have the same value. </summary>
	/// <returns>true if the value of <paramref name="converter1" /> is the same as the value of <paramref name="converter2" />; otherwise, false. </returns>
	/// <param name="converter1">A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />, or null.</param>
	/// <param name="converter2">A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />, or null.</param>
	public static bool operator ==(XamlValueConverter<TConverterBase> converter1, XamlValueConverter<TConverterBase> converter2)
	{
		if ((object)converter1 == null)
		{
			return (object)converter2 == null;
		}
		if ((object)converter2 == null)
		{
			return false;
		}
		if (converter1.ConverterType == converter2.ConverterType && converter1.TargetType == converter2.TargetType)
		{
			return converter1.Name == converter2.Name;
		}
		return false;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> objects have different values. </summary>
	/// <returns>true if the value of <paramref name="converter1" /> is different than the value of <paramref name="converter2" />; otherwise, false. </returns>
	/// <param name="converter1">A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />, or null.</param>
	/// <param name="converter2">A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" />, or null.</param>
	public static bool operator !=(XamlValueConverter<TConverterBase> converter1, XamlValueConverter<TConverterBase> converter2)
	{
		return !(converter1 == converter2);
	}
}
