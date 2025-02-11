using Unity;

namespace System.Configuration;

/// <summary>Represents a collection of <see cref="T:System.Configuration.SchemeSettingElement" /> objects.</summary>
[ConfigurationCollection(typeof(SchemeSettingElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap, AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
public sealed class SchemeSettingElementCollection : ConfigurationElementCollection
{
	/// <summary>Gets an item at the specified index in the <see cref="T:System.Configuration.SchemeSettingElementCollection" /> collection.</summary>
	/// <returns>The specified <see cref="T:System.Configuration.SchemeSettingElement" />.</returns>
	/// <param name="index">The index of the <see cref="T:System.Configuration.SchemeSettingElement" /> to return.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The <paramref name="index" /> parameter is less than zero.-or- The item specified by the parameter is null or has been removed.</exception>
	public SchemeSettingElement this[int index]
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SchemeSettingElementCollection" /> class. </summary>
	public SchemeSettingElementCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	protected override ConfigurationElement CreateNewElement()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>The index of the specified <see cref="T:System.Configuration.SchemeSettingElement" />.</summary>
	/// <returns>The index of the specified <see cref="T:System.Configuration.SchemeSettingElement" />; otherwise, -1.</returns>
	/// <param name="element">The <see cref="T:System.Configuration.SchemeSettingElement" /> for the specified index location.</param>
	public int IndexOf(SchemeSettingElement element)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(int);
	}
}
