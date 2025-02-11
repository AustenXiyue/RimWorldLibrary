using System.Collections;
using System.ComponentModel;
using System.Globalization;
using MS.Internal;

namespace System.Windows.Data;

/// <summary>Describes the grouping of items using a property name as the criteria.</summary>
public class PropertyGroupDescription : GroupDescription
{
	private class NameComparer : IComparer
	{
		private ListSortDirection _direction;

		public NameComparer(ListSortDirection direction)
		{
			_direction = direction;
		}

		int IComparer.Compare(object x, object y)
		{
			object a = (x as CollectionViewGroup)?.Name ?? x;
			object b = (y as CollectionViewGroup)?.Name ?? y;
			int num = Comparer.DefaultInvariant.Compare(a, b);
			if (_direction != 0)
			{
				return -num;
			}
			return num;
		}
	}

	private string _propertyName;

	private PropertyPath _propertyPath;

	private IValueConverter _converter;

	private StringComparison _stringComparison = StringComparison.Ordinal;

	private static readonly IComparer _compareNameAscending = new NameComparer(ListSortDirection.Ascending);

	private static readonly IComparer _compareNameDescending = new NameComparer(ListSortDirection.Descending);

	/// <summary>Gets or sets the name of the property that is used to determine which group(s) an item belongs to.</summary>
	/// <returns>The default value is null.</returns>
	[DefaultValue(null)]
	public string PropertyName
	{
		get
		{
			return _propertyName;
		}
		set
		{
			UpdatePropertyName(value);
			OnPropertyChanged("PropertyName");
		}
	}

	/// <summary>Gets or sets a converter to apply to the property value or the item to produce the final value that is used to determine which group(s) an item belongs to.</summary>
	/// <returns>The default value is null.</returns>
	[DefaultValue(null)]
	public IValueConverter Converter
	{
		get
		{
			return _converter;
		}
		set
		{
			_converter = value;
			OnPropertyChanged("Converter");
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.StringComparison" /> value that specifies the comparison between the value of an item (as determined by <see cref="P:System.Windows.Data.PropertyGroupDescription.PropertyName" /> and <see cref="P:System.Windows.Data.PropertyGroupDescription.Converter" />) and the name of a group.</summary>
	/// <returns>The default value is <see cref="T:System.StringComparison" />.Ordinal.</returns>
	[DefaultValue(StringComparison.Ordinal)]
	public StringComparison StringComparison
	{
		get
		{
			return _stringComparison;
		}
		set
		{
			_stringComparison = value;
			OnPropertyChanged("StringComparison");
		}
	}

	public static IComparer CompareNameAscending => _compareNameAscending;

	public static IComparer CompareNameDescending => _compareNameDescending;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.PropertyGroupDescription" /> class.</summary>
	public PropertyGroupDescription()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.PropertyGroupDescription" /> class with the specified property name.</summary>
	/// <param name="propertyName">The name of the property that specifies which group an item belongs to.</param>
	public PropertyGroupDescription(string propertyName)
	{
		UpdatePropertyName(propertyName);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.PropertyGroupDescription" /> class with the specified property name and converter.</summary>
	/// <param name="propertyName">The name of the property that specifies which group an item belongs to. If this is null, the item itself is passed to the value converter.</param>
	/// <param name="converter">An <see cref="T:System.Windows.Data.IValueConverter" /> object to apply to the property value or the item to produce the final value that is used to determine which group(s) an item belongs to. The converter may return a collection, which indicates the items can appear in more than one group.</param>
	public PropertyGroupDescription(string propertyName, IValueConverter converter)
	{
		UpdatePropertyName(propertyName);
		_converter = converter;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.PropertyGroupDescription" /> class with the specified parameters.</summary>
	/// <param name="propertyName">The name of the property that specifies which group an item belongs to. If this is null, the item itself is passed to the value converter.</param>
	/// <param name="converter">An <see cref="T:System.Windows.Data.IValueConverter" /> object to apply to the property value or the item to produce the final value that is used to determine which group(s) an item belongs to. The converter may return a collection, which indicates the items can appear in more than one group.</param>
	/// <param name="stringComparison">A <see cref="T:System.StringComparison" /> value that specifies the comparison between the value of an item and the name of a group.</param>
	public PropertyGroupDescription(string propertyName, IValueConverter converter, StringComparison stringComparison)
	{
		UpdatePropertyName(propertyName);
		_converter = converter;
		_stringComparison = stringComparison;
	}

	/// <summary>Returns the group name(s) for the given item. </summary>
	/// <returns>The group name(s) for the given item.</returns>
	/// <param name="item">The item to return group names for.</param>
	/// <param name="level">The level of grouping.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to supply to the converter.</param>
	public override object GroupNameFromItem(object item, int level, CultureInfo culture)
	{
		object obj;
		object value;
		if (string.IsNullOrEmpty(PropertyName))
		{
			obj = item;
		}
		else if (SystemXmlHelper.TryGetValueFromXmlNode(item, PropertyName, out value))
		{
			obj = value;
		}
		else if (item != null)
		{
			using (_propertyPath.SetContext(item))
			{
				obj = _propertyPath.GetValue();
			}
		}
		else
		{
			obj = null;
		}
		if (Converter != null)
		{
			obj = Converter.Convert(obj, typeof(object), level, culture);
		}
		return obj;
	}

	/// <summary>Returns a value that indicates whether the group name and the item name match so that the item belongs to the group.</summary>
	/// <returns>true if the names match and the item belongs to the group; otherwise, false.</returns>
	/// <param name="groupName">The name of the group to check.</param>
	/// <param name="itemName">The name of the item to check.</param>
	public override bool NamesMatch(object groupName, object itemName)
	{
		string text = groupName as string;
		string text2 = itemName as string;
		if (text != null && text2 != null)
		{
			return string.Equals(text, text2, StringComparison);
		}
		return object.Equals(groupName, itemName);
	}

	private void UpdatePropertyName(string propertyName)
	{
		_propertyName = propertyName;
		_propertyPath = ((!string.IsNullOrEmpty(propertyName)) ? new PropertyPath(propertyName) : null);
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}
}
