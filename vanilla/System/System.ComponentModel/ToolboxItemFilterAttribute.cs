namespace System.ComponentModel;

/// <summary>Specifies the filter string and filter type to use for a toolbox item.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ToolboxItemFilterAttribute : Attribute
{
	private ToolboxItemFilterType filterType;

	private string filterString;

	private string typeId;

	/// <summary>Gets the filter string for the toolbox item.</summary>
	/// <returns>The filter string for the toolbox item.</returns>
	public string FilterString => filterString;

	/// <summary>Gets the type of the filter.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.ToolboxItemFilterType" /> that indicates the type of the filter.</returns>
	public ToolboxItemFilterType FilterType => filterType;

	/// <summary>Gets the type ID for the attribute.</summary>
	/// <returns>The type ID for this attribute. All <see cref="T:System.ComponentModel.ToolboxItemFilterAttribute" /> objects with the same filter string return the same type ID.</returns>
	public override object TypeId
	{
		get
		{
			if (typeId == null)
			{
				typeId = GetType().FullName + filterString;
			}
			return typeId;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ToolboxItemFilterAttribute" /> class using the specified filter string.</summary>
	/// <param name="filterString">The filter string for the toolbox item. </param>
	public ToolboxItemFilterAttribute(string filterString)
		: this(filterString, ToolboxItemFilterType.Allow)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ToolboxItemFilterAttribute" /> class using the specified filter string and type.</summary>
	/// <param name="filterString">The filter string for the toolbox item. </param>
	/// <param name="filterType">A <see cref="T:System.ComponentModel.ToolboxItemFilterType" /> indicating the type of the filter. </param>
	public ToolboxItemFilterAttribute(string filterString, ToolboxItemFilterType filterType)
	{
		if (filterString == null)
		{
			filterString = string.Empty;
		}
		this.filterString = filterString;
		this.filterType = filterType;
	}

	/// <param name="obj">The object to compare.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is ToolboxItemFilterAttribute { FilterType: var toolboxItemFilterType } toolboxItemFilterAttribute && toolboxItemFilterType.Equals(FilterType))
		{
			return toolboxItemFilterAttribute.FilterString.Equals(FilterString);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return filterString.GetHashCode();
	}

	/// <summary>Indicates whether the specified object has a matching filter string.</summary>
	/// <returns>true if the specified object has a matching filter string; otherwise, false.</returns>
	/// <param name="obj">The object to test for a matching filter string. </param>
	public override bool Match(object obj)
	{
		if (!(obj is ToolboxItemFilterAttribute toolboxItemFilterAttribute))
		{
			return false;
		}
		if (!toolboxItemFilterAttribute.FilterString.Equals(FilterString))
		{
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return filterString + "," + Enum.GetName(typeof(ToolboxItemFilterType), filterType);
	}
}
