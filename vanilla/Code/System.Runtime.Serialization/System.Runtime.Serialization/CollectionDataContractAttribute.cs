namespace System.Runtime.Serialization;

/// <summary>When applied to a collection type, enables custom specification of the collection item elements. This attribute can be applied only to types that are recognized by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" /> as valid, serializable collections. </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class CollectionDataContractAttribute : Attribute
{
	private string name;

	private string ns;

	private string itemName;

	private string keyName;

	private string valueName;

	private bool isReference;

	private bool isNameSetExplicitly;

	private bool isNamespaceSetExplicitly;

	private bool isReferenceSetExplicitly;

	private bool isItemNameSetExplicitly;

	private bool isKeyNameSetExplicitly;

	private bool isValueNameSetExplicitly;

	/// <summary>Gets or sets the namespace for the data contract.</summary>
	/// <returns>The namespace of the data contract.</returns>
	public string Namespace
	{
		get
		{
			return ns;
		}
		set
		{
			ns = value;
			isNamespaceSetExplicitly = true;
		}
	}

	public bool IsNamespaceSetExplicitly => isNamespaceSetExplicitly;

	/// <summary>Gets or sets the data contract name for the collection type.</summary>
	/// <returns>The data contract name for the collection type.</returns>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
			isNameSetExplicitly = true;
		}
	}

	public bool IsNameSetExplicitly => isNameSetExplicitly;

	/// <summary>Gets or sets a custom name for a collection element.</summary>
	/// <returns>The name to apply to collection elements.</returns>
	public string ItemName
	{
		get
		{
			return itemName;
		}
		set
		{
			itemName = value;
			isItemNameSetExplicitly = true;
		}
	}

	public bool IsItemNameSetExplicitly => isItemNameSetExplicitly;

	/// <summary>Gets or sets the custom name for a dictionary key name.</summary>
	/// <returns>The name to use instead of the default dictionary key name.</returns>
	public string KeyName
	{
		get
		{
			return keyName;
		}
		set
		{
			keyName = value;
			isKeyNameSetExplicitly = true;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to preserve object reference data.</summary>
	/// <returns>true to keep object reference data; otherwise, false. The default is false.</returns>
	public bool IsReference
	{
		get
		{
			return isReference;
		}
		set
		{
			isReference = value;
			isReferenceSetExplicitly = true;
		}
	}

	public bool IsReferenceSetExplicitly => isReferenceSetExplicitly;

	public bool IsKeyNameSetExplicitly => isKeyNameSetExplicitly;

	/// <summary>Gets or sets the custom name for a dictionary value name.</summary>
	/// <returns>The name to use instead of the default dictionary value name.</returns>
	public string ValueName
	{
		get
		{
			return valueName;
		}
		set
		{
			valueName = value;
			isValueNameSetExplicitly = true;
		}
	}

	public bool IsValueNameSetExplicitly => isValueNameSetExplicitly;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.CollectionDataContractAttribute" /> class. </summary>
	public CollectionDataContractAttribute()
	{
	}
}
