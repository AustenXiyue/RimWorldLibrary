namespace System.Configuration;

/// <summary>Declaratively instructs the .NET Framework to create an instance of a configuration element collection. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class ConfigurationCollectionAttribute : Attribute
{
	private string addItemName = "add";

	private string clearItemsName = "clear";

	private string removeItemName = "remove";

	private ConfigurationElementCollectionType collectionType;

	private Type itemType;

	/// <summary>Gets or sets the name of the &lt;add&gt; configuration element.</summary>
	/// <returns>The name that substitutes the standard name "add" for the configuration item.</returns>
	public string AddItemName
	{
		get
		{
			return addItemName;
		}
		set
		{
			addItemName = value;
		}
	}

	/// <summary>Gets or sets the name for the &lt;clear&gt; configuration element.</summary>
	/// <returns>The name that replaces the standard name "clear" for the configuration item.</returns>
	public string ClearItemsName
	{
		get
		{
			return clearItemsName;
		}
		set
		{
			clearItemsName = value;
		}
	}

	/// <summary>Gets or sets the name for the &lt;remove&gt; configuration element.</summary>
	/// <returns>The name that replaces the standard name "remove" for the configuration element.</returns>
	public string RemoveItemName
	{
		get
		{
			return removeItemName;
		}
		set
		{
			removeItemName = value;
		}
	}

	/// <summary>Gets or sets the type of the <see cref="T:System.Configuration.ConfigurationCollectionAttribute" /> attribute.</summary>
	/// <returns>The type of the <see cref="T:System.Configuration.ConfigurationCollectionAttribute" />.</returns>
	public ConfigurationElementCollectionType CollectionType
	{
		get
		{
			return collectionType;
		}
		set
		{
			collectionType = value;
		}
	}

	/// <summary>Gets the type of the collection element.</summary>
	/// <returns>The type of the collection element.</returns>
	[System.MonoInternalNote("Do something with this in ConfigurationElementCollection")]
	public Type ItemType => itemType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationCollectionAttribute" /> class.</summary>
	/// <param name="itemType">The type of the property collection to create.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="itemType" /> is null.</exception>
	public ConfigurationCollectionAttribute(Type itemType)
	{
		this.itemType = itemType;
	}
}
