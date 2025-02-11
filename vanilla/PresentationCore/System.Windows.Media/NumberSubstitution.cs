using System.ComponentModel;
using System.Globalization;
using MS.Internal.FontCache;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Specifies how numbers in text are displayed in different cultures.</summary>
public class NumberSubstitution
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.NumberSubstitution.CultureSource" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.NumberSubstitution.CultureSource" /> dependency property.</returns>
	public static readonly DependencyProperty CultureSourceProperty = DependencyProperty.RegisterAttached("CultureSource", typeof(NumberCultureSource), typeof(NumberSubstitution));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.NumberSubstitution.CultureOverride" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.NumberSubstitution.CultureOverride" /> dependency property.</returns>
	public static readonly DependencyProperty CultureOverrideProperty = DependencyProperty.RegisterAttached("CultureOverride", typeof(CultureInfo), typeof(NumberSubstitution), null, IsValidCultureOverrideValue);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.NumberSubstitution.Substitution" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.NumberSubstitution.Substitution" /> dependency property.</returns>
	public static readonly DependencyProperty SubstitutionProperty = DependencyProperty.RegisterAttached("Substitution", typeof(NumberSubstitutionMethod), typeof(NumberSubstitution));

	private NumberCultureSource _source;

	private CultureInfo _cultureOverride;

	private NumberSubstitutionMethod _substitution;

	/// <summary>Gets or sets a value which identifies the source of the culture value that is used to determine number substitution.  </summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.Media.NumberCultureSource" />.</returns>
	public NumberCultureSource CultureSource
	{
		get
		{
			return _source;
		}
		set
		{
			if ((uint)value > 2u)
			{
				throw new InvalidEnumArgumentException("CultureSource", (int)value, typeof(NumberCultureSource));
			}
			_source = value;
		}
	}

	/// <summary>Gets or sets a value which identifies which culture to use when the value of the <see cref="P:System.Windows.Media.NumberSubstitution.CultureSource" /> property is set to <see cref="F:System.Windows.Media.NumberCultureSource.Override" />.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> value that represents the culture that is used as an override.</returns>
	[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
	public CultureInfo CultureOverride
	{
		get
		{
			return _cultureOverride;
		}
		set
		{
			_cultureOverride = ThrowIfInvalidCultureOverride(value);
		}
	}

	/// <summary>Gets or sets a value which identifies the substitution method that is used to determine number substitution.  </summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.Media.NumberSubstitutionMethod" />.</returns>
	public NumberSubstitutionMethod Substitution
	{
		get
		{
			return _substitution;
		}
		set
		{
			if ((uint)value > 4u)
			{
				throw new InvalidEnumArgumentException("Substitution", (int)value, typeof(NumberSubstitutionMethod));
			}
			_substitution = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.NumberSubstitution" /> class.</summary>
	public NumberSubstitution()
	{
		_source = NumberCultureSource.Text;
		_cultureOverride = null;
		_substitution = NumberSubstitutionMethod.AsCulture;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.NumberSubstitution" /> class with explicit property values.</summary>
	/// <param name="source">Identifies the source of the culture value that is used to determine number substitution.</param>
	/// <param name="cultureOverride">Identifies which culture to use when the value of the <see cref="P:System.Windows.Media.NumberSubstitution.CultureSource" /> property is set to <see cref="F:System.Windows.Media.NumberCultureSource.Override" />.</param>
	/// <param name="substitution">Identifies the substitution method that is used to determine number substitution.</param>
	public NumberSubstitution(NumberCultureSource source, CultureInfo cultureOverride, NumberSubstitutionMethod substitution)
	{
		_source = source;
		_cultureOverride = ThrowIfInvalidCultureOverride(cultureOverride);
		_substitution = substitution;
	}

	private static CultureInfo ThrowIfInvalidCultureOverride(CultureInfo culture)
	{
		if (!IsValidCultureOverride(culture))
		{
			throw new ArgumentException(SR.SpecificNumberCultureRequired);
		}
		return culture;
	}

	private static bool IsValidCultureOverride(CultureInfo culture)
	{
		if (culture != null)
		{
			if (!culture.IsNeutralCulture)
			{
				return !culture.Equals(CultureInfo.InvariantCulture);
			}
			return false;
		}
		return true;
	}

	private static bool IsValidCultureOverrideValue(object value)
	{
		return IsValidCultureOverride((CultureInfo)value);
	}

	/// <summary>Sets the value of <see cref="P:System.Windows.Media.NumberSubstitution.CultureSource" /> for a provided element.</summary>
	/// <param name="target">Element that is specifying a culture override.</param>
	/// <param name="value">A culture source value of type <see cref="T:System.Windows.Media.NumberCultureSource" />.</param>
	public static void SetCultureSource(DependencyObject target, NumberCultureSource value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(CultureSourceProperty, value);
	}

	/// <summary>Returns the value of <see cref="P:System.Windows.Media.NumberSubstitution.CultureSource" /> from the provided element.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.Media.NumberCultureSource" />.</returns>
	/// <param name="target">The element to return a <see cref="P:System.Windows.Media.NumberSubstitution.CultureSource" /> value for.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static NumberCultureSource GetCultureSource(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (NumberCultureSource)target.GetValue(CultureSourceProperty);
	}

	/// <summary>Sets the value of <see cref="P:System.Windows.Media.NumberSubstitution.CultureOverride" /> for a provided element.</summary>
	/// <param name="target">Element that is specifying a culture override.</param>
	/// <param name="value">A culture override value of type <see cref="T:System.Globalization.CultureInfo" />.</param>
	public static void SetCultureOverride(DependencyObject target, CultureInfo value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(CultureOverrideProperty, value);
	}

	/// <summary>Returns the value of <see cref="P:System.Windows.Media.NumberSubstitution.CultureOverride" /> from the provided element.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> value that represents the culture that is used as an override.</returns>
	/// <param name="target">The element to return a <see cref="P:System.Windows.Media.NumberSubstitution.CultureOverride" /> value for.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
	public static CultureInfo GetCultureOverride(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (CultureInfo)target.GetValue(CultureOverrideProperty);
	}

	/// <summary>Sets the value of <see cref="P:System.Windows.Media.NumberSubstitution.Substitution" /> for a provided element.</summary>
	/// <param name="target">Element that is specifying a substitution method.</param>
	/// <param name="value">A substitution method value of type <see cref="T:System.Windows.Media.NumberSubstitutionMethod" />.</param>
	public static void SetSubstitution(DependencyObject target, NumberSubstitutionMethod value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(SubstitutionProperty, value);
	}

	/// <summary>Returns the value of <see cref="P:System.Windows.Media.NumberSubstitution.Substitution" /> from the provided element.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.Media.NumberSubstitutionMethod" />.</returns>
	/// <param name="target">The element to return a <see cref="P:System.Windows.Media.NumberSubstitution.Substitution" /> value for.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static NumberSubstitutionMethod GetSubstitution(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (NumberSubstitutionMethod)target.GetValue(SubstitutionProperty);
	}

	/// <summary>Serves as a hash function for <see cref="T:System.Windows.Media.NumberSubstitution" />. It is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the hash code for the current object.</returns>
	public override int GetHashCode()
	{
		int hash = (int)(HashFn.HashMultiply((int)_source) + _substitution);
		if (_cultureOverride != null)
		{
			hash = HashFn.HashMultiply(hash) + _cultureOverride.GetHashCode();
		}
		return HashFn.HashScramble(hash);
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.Media.NumberSubstitution" /> object.</summary>
	/// <returns>true if <paramref name="o" /> is equal to the current <see cref="T:System.Windows.Media.NumberSubstitution" /> object; otherwise, false. If <paramref name="o" /> is not a <see cref="T:System.Windows.Media.NumberSubstitution" /> object, false is returned.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Windows.Media.NumberSubstitution" /> object.</param>
	public override bool Equals(object obj)
	{
		if (obj is NumberSubstitution numberSubstitution && _source == numberSubstitution._source && _substitution == numberSubstitution._substitution)
		{
			if (_cultureOverride != null)
			{
				return _cultureOverride.Equals(numberSubstitution._cultureOverride);
			}
			return numberSubstitution._cultureOverride == null;
		}
		return false;
	}
}
