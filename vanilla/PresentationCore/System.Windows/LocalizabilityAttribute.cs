using System.ComponentModel;

namespace System.Windows;

/// <summary>Specifies the localization attributes for a binary XAML (BAML) class or class member.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class LocalizabilityAttribute : Attribute
{
	private LocalizationCategory _category;

	private Readability _readability;

	private Modifiability _modifiability;

	/// <summary>Gets the category setting of the localization attribute's targeted value.</summary>
	/// <returns>The category setting of the localization attribute.</returns>
	public LocalizationCategory Category => _category;

	/// <summary>Gets or sets the readability setting of the localization attribute's targeted value.</summary>
	/// <returns>The readability setting of the localization attribute.</returns>
	public Readability Readability
	{
		get
		{
			return _readability;
		}
		set
		{
			if (value != 0 && value != Readability.Readable && value != Readability.Inherit)
			{
				throw new InvalidEnumArgumentException("Readability", (int)value, typeof(Readability));
			}
			_readability = value;
		}
	}

	/// <summary>Gets or sets the modifiability setting of the localization attribute's targeted value.</summary>
	/// <returns>The modifiability setting of the localization attribute.</returns>
	public Modifiability Modifiability
	{
		get
		{
			return _modifiability;
		}
		set
		{
			if (value != 0 && value != Modifiability.Modifiable && value != Modifiability.Inherit)
			{
				throw new InvalidEnumArgumentException("Modifiability", (int)value, typeof(Modifiability));
			}
			_modifiability = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.LocalizabilityAttribute" /> class with a specified localization category.</summary>
	/// <param name="category">The localization category.</param>
	public LocalizabilityAttribute(LocalizationCategory category)
	{
		if (category < LocalizationCategory.None || category > LocalizationCategory.NeverLocalize)
		{
			throw new InvalidEnumArgumentException("category", (int)category, typeof(LocalizationCategory));
		}
		_category = category;
		_readability = Readability.Readable;
		_modifiability = Modifiability.Modifiable;
	}
}
