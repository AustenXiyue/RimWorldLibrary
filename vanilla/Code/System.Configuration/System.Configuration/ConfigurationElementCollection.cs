using System.Collections;
using System.Diagnostics;
using System.Xml;

namespace System.Configuration;

/// <summary>Represents a configuration element containing a collection of child elements.</summary>
[DebuggerDisplay("Count = {Count}")]
public abstract class ConfigurationElementCollection : ConfigurationElement, ICollection, IEnumerable
{
	private sealed class ConfigurationRemoveElement : ConfigurationElement
	{
		private readonly ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

		private readonly ConfigurationElement _origElement;

		private readonly ConfigurationElementCollection _origCollection;

		internal object KeyValue
		{
			get
			{
				foreach (ConfigurationProperty property in Properties)
				{
					_origElement[property] = base[property];
				}
				return _origCollection.GetElementKey(_origElement);
			}
		}

		protected internal override ConfigurationPropertyCollection Properties => properties;

		internal ConfigurationRemoveElement(ConfigurationElement origElement, ConfigurationElementCollection origCollection)
		{
			_origElement = origElement;
			_origCollection = origCollection;
			foreach (ConfigurationProperty property in origElement.Properties)
			{
				if (property.IsKey)
				{
					properties.Add(property);
				}
			}
		}
	}

	private ArrayList list = new ArrayList();

	private ArrayList removed;

	private ArrayList inherited;

	private bool emitClear;

	private bool modified;

	private IComparer comparer;

	private int inheritedLimitIndex;

	private string addElementName = "add";

	private string clearElementName = "clear";

	private string removeElementName = "remove";

	/// <summary>Gets the type of the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationElementCollectionType" /> of this collection.</returns>
	public virtual ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.AddRemoveClearMap;

	private bool IsBasic
	{
		get
		{
			if (CollectionType != 0)
			{
				return CollectionType == ConfigurationElementCollectionType.BasicMapAlternate;
			}
			return true;
		}
	}

	private bool IsAlternate
	{
		get
		{
			if (CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate)
			{
				return CollectionType == ConfigurationElementCollectionType.BasicMapAlternate;
			}
			return true;
		}
	}

	/// <summary>Gets the number of elements in the collection.</summary>
	/// <returns>The number of elements in the collection.</returns>
	public int Count => list.Count;

	/// <summary>Gets the name used to identify this collection of elements in the configuration file when overridden in a derived class.</summary>
	/// <returns>The name of the collection; otherwise, an empty string. The default is an empty string.</returns>
	protected virtual string ElementName => string.Empty;

	/// <summary>Gets or sets a value that specifies whether the collection has been cleared.</summary>
	/// <returns>true if the collection has been cleared; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration is read-only.</exception>
	public bool EmitClear
	{
		get
		{
			return emitClear;
		}
		set
		{
			emitClear = value;
		}
	}

	/// <summary>Gets a value indicating whether access to the collection is synchronized.</summary>
	/// <returns>true if access to the <see cref="T:System.Configuration.ConfigurationElementCollection" /> is synchronized; otherwise, false.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets an object used to synchronize access to the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</summary>
	/// <returns>An object used to synchronize access to the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</returns>
	public object SyncRoot => this;

	/// <summary>Gets a value indicating whether an attempt to add a duplicate <see cref="T:System.Configuration.ConfigurationElement" /> to the <see cref="T:System.Configuration.ConfigurationElementCollection" /> will cause an exception to be thrown.</summary>
	/// <returns>true if an attempt to add a duplicate <see cref="T:System.Configuration.ConfigurationElement" /> to this <see cref="T:System.Configuration.ConfigurationElementCollection" /> will cause an exception to be thrown; otherwise, false. </returns>
	protected virtual bool ThrowOnDuplicate
	{
		get
		{
			if (CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap && CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate)
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>Gets or sets the name of the <see cref="T:System.Configuration.ConfigurationElement" /> to associate with the add operation in the <see cref="T:System.Configuration.ConfigurationElementCollection" /> when overridden in a derived class. </summary>
	/// <returns>The name of the element.</returns>
	/// <exception cref="T:System.ArgumentException">The selected value starts with the reserved prefix "config" or "lock".</exception>
	protected internal string AddElementName
	{
		get
		{
			return addElementName;
		}
		set
		{
			addElementName = value;
		}
	}

	/// <summary>Gets or sets the name for the <see cref="T:System.Configuration.ConfigurationElement" /> to associate with the clear operation in the <see cref="T:System.Configuration.ConfigurationElementCollection" /> when overridden in a derived class. </summary>
	/// <returns>The name of the element.</returns>
	/// <exception cref="T:System.ArgumentException">The selected value starts with the reserved prefix "config" or "lock".</exception>
	protected internal string ClearElementName
	{
		get
		{
			return clearElementName;
		}
		set
		{
			clearElementName = value;
		}
	}

	/// <summary>Gets or sets the name of the <see cref="T:System.Configuration.ConfigurationElement" /> to associate with the remove operation in the <see cref="T:System.Configuration.ConfigurationElementCollection" /> when overridden in a derived class. </summary>
	/// <returns>The name of the element.</returns>
	/// <exception cref="T:System.ArgumentException">The selected value starts with the reserved prefix "config" or "lock".</exception>
	protected internal string RemoveElementName
	{
		get
		{
			return removeElementName;
		}
		set
		{
			removeElementName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationElementCollection" /> class.</summary>
	protected ConfigurationElementCollection()
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Configuration.ConfigurationElementCollection" /> class.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> comparer to use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="comparer" /> is null.</exception>
	protected ConfigurationElementCollection(IComparer comparer)
	{
		this.comparer = comparer;
	}

	internal override void InitFromProperty(PropertyInformation propertyInfo)
	{
		ConfigurationCollectionAttribute configurationCollectionAttribute = propertyInfo.Property.CollectionAttribute;
		if (configurationCollectionAttribute == null)
		{
			configurationCollectionAttribute = Attribute.GetCustomAttribute(propertyInfo.Type, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
		}
		if (configurationCollectionAttribute != null)
		{
			addElementName = configurationCollectionAttribute.AddItemName;
			clearElementName = configurationCollectionAttribute.ClearItemsName;
			removeElementName = configurationCollectionAttribute.RemoveItemName;
		}
		base.InitFromProperty(propertyInfo);
	}

	/// <summary>Adds a configuration element to the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</summary>
	/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to add.</param>
	protected virtual void BaseAdd(ConfigurationElement element)
	{
		BaseAdd(element, ThrowOnDuplicate);
	}

	/// <summary>Adds a configuration element to the configuration element collection.</summary>
	/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to add.</param>
	/// <param name="throwIfExists">true to throw an exception if the <see cref="T:System.Configuration.ConfigurationElement" /> specified is already contained in the <see cref="T:System.Configuration.ConfigurationElementCollection" />; otherwise, false. </param>
	/// <exception cref="T:System.Exception">The <see cref="T:System.Configuration.ConfigurationElement" /> to add already exists in the <see cref="T:System.Configuration.ConfigurationElementCollection" /> and the <paramref name="throwIfExists" /> parameter is true. </exception>
	protected void BaseAdd(ConfigurationElement element, bool throwIfExists)
	{
		if (IsReadOnly())
		{
			throw new ConfigurationErrorsException("Collection is read only.");
		}
		if (IsAlternate)
		{
			list.Insert(inheritedLimitIndex, element);
			inheritedLimitIndex++;
		}
		else
		{
			int num = IndexOfKey(GetElementKey(element));
			if (num >= 0)
			{
				if (element.Equals(list[num]))
				{
					return;
				}
				if (throwIfExists)
				{
					throw new ConfigurationErrorsException("Duplicate element in collection");
				}
				list.RemoveAt(num);
			}
			list.Add(element);
		}
		modified = true;
	}

	/// <summary>Adds a configuration element to the configuration element collection.</summary>
	/// <param name="index">The index location at which to add the specified <see cref="T:System.Configuration.ConfigurationElement" />. </param>
	/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to add. </param>
	protected virtual void BaseAdd(int index, ConfigurationElement element)
	{
		if (ThrowOnDuplicate && BaseIndexOf(element) != -1)
		{
			throw new ConfigurationErrorsException("Duplicate element in collection");
		}
		if (IsReadOnly())
		{
			throw new ConfigurationErrorsException("Collection is read only.");
		}
		if (IsAlternate && index > inheritedLimitIndex)
		{
			throw new ConfigurationErrorsException("Can't insert new elements below the inherited elements.");
		}
		if (!IsAlternate && index <= inheritedLimitIndex)
		{
			throw new ConfigurationErrorsException("Can't insert new elements above the inherited elements.");
		}
		list.Insert(index, element);
		modified = true;
	}

	/// <summary>Removes all configuration element objects from the collection.</summary>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration is read-only.- or -A collection item has been locked in a higher-level configuration.</exception>
	protected internal void BaseClear()
	{
		if (IsReadOnly())
		{
			throw new ConfigurationErrorsException("Collection is read only.");
		}
		list.Clear();
		modified = true;
	}

	/// <summary>Gets the configuration element at the specified index location.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationElement" /> at the specified index.</returns>
	/// <param name="index">The index location of the <see cref="T:System.Configuration.ConfigurationElement" /> to return. </param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">
	///   <paramref name="index" /> is less than 0.- or -There is no <see cref="T:System.Configuration.ConfigurationElement" /> at the specified <paramref name="index" />.</exception>
	protected internal ConfigurationElement BaseGet(int index)
	{
		return (ConfigurationElement)list[index];
	}

	/// <summary>Returns the configuration element with the specified key.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationElement" /> with the specified key; otherwise, null.</returns>
	/// <param name="key">The key of the element to return. </param>
	protected internal ConfigurationElement BaseGet(object key)
	{
		int num = IndexOfKey(key);
		if (num != -1)
		{
			return (ConfigurationElement)list[num];
		}
		return null;
	}

	/// <summary>Returns an array of the keys for all of the configuration elements contained in the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</summary>
	/// <returns>An array that contains the keys for all of the <see cref="T:System.Configuration.ConfigurationElement" /> objects contained in the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</returns>
	protected internal object[] BaseGetAllKeys()
	{
		object[] array = new object[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = BaseGetKey(i);
		}
		return array;
	}

	/// <summary>Gets the key for the <see cref="T:System.Configuration.ConfigurationElement" /> at the specified index location.</summary>
	/// <returns>The key for the specified <see cref="T:System.Configuration.ConfigurationElement" />.</returns>
	/// <param name="index">The index location for the <see cref="T:System.Configuration.ConfigurationElement" />.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">
	///   <paramref name="index" /> is less than 0.- or -There is no <see cref="T:System.Configuration.ConfigurationElement" /> at the specified <paramref name="index" />.</exception>
	protected internal object BaseGetKey(int index)
	{
		if (index < 0 || index >= list.Count)
		{
			throw new ConfigurationErrorsException($"Index {index} is out of range");
		}
		return GetElementKey((ConfigurationElement)list[index]).ToString();
	}

	/// <summary>Indicates the index of the specified <see cref="T:System.Configuration.ConfigurationElement" />.</summary>
	/// <returns>The index of the specified <see cref="T:System.Configuration.ConfigurationElement" />; otherwise, -1.</returns>
	/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> for the specified index location.</param>
	protected int BaseIndexOf(ConfigurationElement element)
	{
		return list.IndexOf(element);
	}

	private int IndexOfKey(object key)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (CompareKeys(GetElementKey((ConfigurationElement)list[i]), key))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Indicates whether the <see cref="T:System.Configuration.ConfigurationElement" /> with the specified key has been removed from the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.ConfigurationElement" /> with the specified key has been removed; otherwise, false. The default is false.</returns>
	/// <param name="key">The key of the element to check.</param>
	protected internal bool BaseIsRemoved(object key)
	{
		if (removed == null)
		{
			return false;
		}
		foreach (ConfigurationElement item in removed)
		{
			if (CompareKeys(GetElementKey(item), key))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Removes a <see cref="T:System.Configuration.ConfigurationElement" /> from the collection.</summary>
	/// <param name="key">The key of the <see cref="T:System.Configuration.ConfigurationElement" /> to remove.</param>
	/// <exception cref="T:System.Exception">No <see cref="T:System.Configuration.ConfigurationElement" /> with the specified key exists in the collection, the element has already been removed, or the element cannot be removed because the value of its <see cref="P:System.Configuration.ConfigurationProperty.Type" /> is not <see cref="F:System.Configuration.ConfigurationElementCollectionType.AddRemoveClearMap" />. </exception>
	protected internal void BaseRemove(object key)
	{
		if (IsReadOnly())
		{
			throw new ConfigurationErrorsException("Collection is read only.");
		}
		int num = IndexOfKey(key);
		if (num != -1)
		{
			BaseRemoveAt(num);
			modified = true;
		}
	}

	/// <summary>Removes the <see cref="T:System.Configuration.ConfigurationElement" /> at the specified index location.</summary>
	/// <param name="index">The index location of the <see cref="T:System.Configuration.ConfigurationElement" /> to remove.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration is read-only.- or -<paramref name="index" /> is less than 0 or greater than the number of <see cref="T:System.Configuration.ConfigurationElement" /> objects in the collection.- or -The <see cref="T:System.Configuration.ConfigurationElement" /> object has already been removed.- or -The value of the <see cref="T:System.Configuration.ConfigurationElement" /> object has been locked at a higher level.- or -The <see cref="T:System.Configuration.ConfigurationElement" /> object was inherited.- or -The value of the <see cref="T:System.Configuration.ConfigurationElement" /> object's <see cref="P:System.Configuration.ConfigurationProperty.Type" /> is not <see cref="F:System.Configuration.ConfigurationElementCollectionType.AddRemoveClearMap" /> or <see cref="F:System.Configuration.ConfigurationElementCollectionType.AddRemoveClearMapAlternate" />.</exception>
	protected internal void BaseRemoveAt(int index)
	{
		if (IsReadOnly())
		{
			throw new ConfigurationErrorsException("Collection is read only.");
		}
		ConfigurationElement configurationElement = (ConfigurationElement)list[index];
		if (!IsElementRemovable(configurationElement))
		{
			throw new ConfigurationErrorsException("Element can't be removed from element collection.");
		}
		if (inherited != null && inherited.Contains(configurationElement))
		{
			throw new ConfigurationErrorsException("Inherited items can't be removed.");
		}
		list.RemoveAt(index);
		if (IsAlternate && inheritedLimitIndex > 0)
		{
			inheritedLimitIndex--;
		}
		modified = true;
	}

	private bool CompareKeys(object key1, object key2)
	{
		if (comparer != null)
		{
			return comparer.Compare(key1, key2) == 0;
		}
		return object.Equals(key1, key2);
	}

	/// <summary>Copies the contents of the <see cref="T:System.Configuration.ConfigurationElementCollection" /> to an array.</summary>
	/// <param name="array">Array to which to copy the contents of the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</param>
	/// <param name="index">Index location at which to begin copying.</param>
	public void CopyTo(ConfigurationElement[] array, int index)
	{
		list.CopyTo(array, index);
	}

	/// <summary>When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement" />.</summary>
	/// <returns>A newly created <see cref="T:System.Configuration.ConfigurationElement" />.</returns>
	protected abstract ConfigurationElement CreateNewElement();

	/// <summary>Creates a new <see cref="T:System.Configuration.ConfigurationElement" /> when overridden in a derived class.</summary>
	/// <returns>A new <see cref="T:System.Configuration.ConfigurationElement" /> with a specified name.</returns>
	/// <param name="elementName">The name of the <see cref="T:System.Configuration.ConfigurationElement" /> to create. </param>
	protected virtual ConfigurationElement CreateNewElement(string elementName)
	{
		return CreateNewElement();
	}

	private ConfigurationElement CreateNewElementInternal(string elementName)
	{
		ConfigurationElement configurationElement = ((elementName != null) ? CreateNewElement(elementName) : CreateNewElement());
		configurationElement.Init();
		return configurationElement;
	}

	/// <summary>Compares the <see cref="T:System.Configuration.ConfigurationElementCollection" /> to the specified object.</summary>
	/// <returns>true if the object to compare with is equal to the current <see cref="T:System.Configuration.ConfigurationElementCollection" /> instance; otherwise, false. The default is false.</returns>
	/// <param name="compareTo">The object to compare. </param>
	public override bool Equals(object compareTo)
	{
		if (!(compareTo is ConfigurationElementCollection configurationElementCollection))
		{
			return false;
		}
		if (GetType() != configurationElementCollection.GetType())
		{
			return false;
		}
		if (Count != configurationElementCollection.Count)
		{
			return false;
		}
		for (int i = 0; i < Count; i++)
		{
			if (!BaseGet(i).Equals(configurationElementCollection.BaseGet(i)))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Gets the element key for a specified configuration element when overridden in a derived class.</summary>
	/// <returns>An <see cref="T:System.Object" /> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for. </param>
	protected abstract object GetElementKey(ConfigurationElement element);

	/// <summary>Gets a unique value representing the <see cref="T:System.Configuration.ConfigurationElementCollection" /> instance.</summary>
	/// <returns>A unique value representing the <see cref="T:System.Configuration.ConfigurationElementCollection" /> current instance.</returns>
	public override int GetHashCode()
	{
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			num += BaseGet(i).GetHashCode();
		}
		return num;
	}

	/// <summary>Copies the <see cref="T:System.Configuration.ConfigurationElementCollection" /> to an array.</summary>
	/// <param name="arr">Array to which to copy this <see cref="T:System.Configuration.ConfigurationElementCollection" />.</param>
	/// <param name="index">Index location at which to begin copying.</param>
	void ICollection.CopyTo(Array arr, int index)
	{
		list.CopyTo(arr, index);
	}

	/// <summary>Gets an <see cref="T:System.Collections.IEnumerator" /> which is used to iterate through the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> which is used to iterate through the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</returns>
	public IEnumerator GetEnumerator()
	{
		return list.GetEnumerator();
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Configuration.ConfigurationElement" /> exists in the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</summary>
	/// <returns>true if the element exists in the collection; otherwise, false. The default is false.</returns>
	/// <param name="elementName">The name of the element to verify. </param>
	protected virtual bool IsElementName(string elementName)
	{
		return false;
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Configuration.ConfigurationElement" /> can be removed from the <see cref="T:System.Configuration.ConfigurationElementCollection" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Configuration.ConfigurationElement" /> can be removed from this <see cref="T:System.Configuration.ConfigurationElementCollection" />; otherwise, false. The default is true.</returns>
	/// <param name="element">The element to check.</param>
	protected virtual bool IsElementRemovable(ConfigurationElement element)
	{
		return !IsReadOnly();
	}

	/// <summary>Indicates whether this <see cref="T:System.Configuration.ConfigurationElementCollection" /> has been modified since it was last saved or loaded when overridden in a derived class.</summary>
	/// <returns>true if any contained element has been modified; otherwise, false</returns>
	protected internal override bool IsModified()
	{
		if (modified)
		{
			return true;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (((ConfigurationElement)list[i]).IsModified())
			{
				modified = true;
				break;
			}
		}
		return modified;
	}

	/// <summary>Indicates whether the <see cref="T:System.Configuration.ConfigurationElementCollection" /> object is read only.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.ConfigurationElementCollection" /> object is read only; otherwise, false.</returns>
	[System.MonoTODO]
	public override bool IsReadOnly()
	{
		return base.IsReadOnly();
	}

	internal override void PrepareSave(ConfigurationElement parentElement, ConfigurationSaveMode mode)
	{
		ConfigurationElementCollection configurationElementCollection = (ConfigurationElementCollection)parentElement;
		base.PrepareSave(parentElement, mode);
		for (int i = 0; i < list.Count; i++)
		{
			ConfigurationElement configurationElement = (ConfigurationElement)list[i];
			object elementKey = GetElementKey(configurationElement);
			ConfigurationElement parent = configurationElementCollection?.BaseGet(elementKey);
			configurationElement.PrepareSave(parent, mode);
		}
	}

	internal override bool HasValues(ConfigurationElement parentElement, ConfigurationSaveMode mode)
	{
		ConfigurationElementCollection configurationElementCollection = (ConfigurationElementCollection)parentElement;
		if (mode == ConfigurationSaveMode.Full)
		{
			return list.Count > 0;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ConfigurationElement configurationElement = (ConfigurationElement)list[i];
			object elementKey = GetElementKey(configurationElement);
			ConfigurationElement parent = configurationElementCollection?.BaseGet(elementKey);
			if (configurationElement.HasValues(parent, mode))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Resets the <see cref="T:System.Configuration.ConfigurationElementCollection" /> to its unmodified state when overridden in a derived class.</summary>
	/// <param name="parentElement">The <see cref="T:System.Configuration.ConfigurationElement" /> representing the collection parent element, if any; otherwise, null. </param>
	protected internal override void Reset(ConfigurationElement parentElement)
	{
		bool isBasic = IsBasic;
		ConfigurationElementCollection configurationElementCollection = (ConfigurationElementCollection)parentElement;
		for (int i = 0; i < configurationElementCollection.Count; i++)
		{
			ConfigurationElement parentElement2 = configurationElementCollection.BaseGet(i);
			ConfigurationElement configurationElement = CreateNewElementInternal(null);
			configurationElement.Reset(parentElement2);
			BaseAdd(configurationElement);
			if (isBasic)
			{
				if (inherited == null)
				{
					inherited = new ArrayList();
				}
				inherited.Add(configurationElement);
			}
		}
		if (IsAlternate)
		{
			inheritedLimitIndex = 0;
		}
		else
		{
			inheritedLimitIndex = Count - 1;
		}
		modified = false;
	}

	/// <summary>Resets the value of the <see cref="M:System.Configuration.ConfigurationElementCollection.IsModified" /> property to false when overridden in a derived class.</summary>
	protected internal override void ResetModified()
	{
		modified = false;
		for (int i = 0; i < list.Count; i++)
		{
			((ConfigurationElement)list[i]).ResetModified();
		}
	}

	/// <summary>Sets the <see cref="M:System.Configuration.ConfigurationElementCollection.IsReadOnly" /> property for the <see cref="T:System.Configuration.ConfigurationElementCollection" /> object and for all sub-elements.</summary>
	[System.MonoTODO]
	protected internal override void SetReadOnly()
	{
		base.SetReadOnly();
	}

	/// <summary>Writes the configuration data to an XML element in the configuration file when overridden in a derived class.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.ConfigurationElementCollection" /> was written to the configuration file successfully.</returns>
	/// <param name="writer">Output stream that writes XML to the configuration file.</param>
	/// <param name="serializeCollectionKey">true to serialize the collection key; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentException">One of the elements in the collection was added or replaced and starts with the reserved prefix "config" or "lock".</exception>
	protected internal override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
	{
		if (serializeCollectionKey)
		{
			return base.SerializeElement(writer, serializeCollectionKey);
		}
		bool flag = false;
		if (IsBasic)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ConfigurationElement configurationElement = (ConfigurationElement)list[i];
				flag = ((!(ElementName != string.Empty)) ? (configurationElement.SerializeElement(writer, serializeCollectionKey: false) || flag) : (configurationElement.SerializeToXmlElement(writer, ElementName) || flag));
			}
		}
		else
		{
			if (emitClear)
			{
				writer.WriteElementString(clearElementName, "");
				flag = true;
			}
			if (removed != null)
			{
				for (int j = 0; j < removed.Count; j++)
				{
					writer.WriteStartElement(removeElementName);
					((ConfigurationElement)removed[j]).SerializeElement(writer, serializeCollectionKey: true);
					writer.WriteEndElement();
				}
				flag = flag || removed.Count > 0;
			}
			for (int k = 0; k < list.Count; k++)
			{
				((ConfigurationElement)list[k]).SerializeToXmlElement(writer, addElementName);
			}
			flag = flag || list.Count > 0;
		}
		return flag;
	}

	/// <summary>Causes the configuration system to throw an exception.</summary>
	/// <returns>true if the unrecognized element was deserialized successfully; otherwise, false. The default is false.</returns>
	/// <param name="elementName">The name of the unrecognized element.</param>
	/// <param name="reader">An input stream that reads XML from the configuration file. </param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element specified in <paramref name="elementName" /> is the &lt;clear&gt; element.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="elementName" /> starts with the reserved prefix "config" or "lock".</exception>
	protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
	{
		if (IsBasic)
		{
			ConfigurationElement configurationElement = null;
			if (elementName == ElementName)
			{
				configurationElement = CreateNewElementInternal(null);
			}
			if (IsElementName(elementName))
			{
				configurationElement = CreateNewElementInternal(elementName);
			}
			if (configurationElement != null)
			{
				configurationElement.DeserializeElement(reader, serializeCollectionKey: false);
				BaseAdd(configurationElement);
				modified = false;
				return true;
			}
		}
		else
		{
			if (elementName == clearElementName)
			{
				reader.MoveToContent();
				if (reader.MoveToNextAttribute())
				{
					throw new ConfigurationErrorsException("Unrecognized attribute '" + reader.LocalName + "'.");
				}
				reader.MoveToElement();
				reader.Skip();
				BaseClear();
				emitClear = true;
				modified = false;
				return true;
			}
			if (elementName == removeElementName)
			{
				ConfigurationRemoveElement configurationRemoveElement = new ConfigurationRemoveElement(CreateNewElementInternal(null), this);
				configurationRemoveElement.DeserializeElement(reader, serializeCollectionKey: true);
				BaseRemove(configurationRemoveElement.KeyValue);
				modified = false;
				return true;
			}
			if (elementName == addElementName)
			{
				ConfigurationElement configurationElement2 = CreateNewElementInternal(null);
				configurationElement2.DeserializeElement(reader, serializeCollectionKey: false);
				BaseAdd(configurationElement2);
				modified = false;
				return true;
			}
		}
		return false;
	}

	/// <summary>Reverses the effect of merging configuration information from different levels of the configuration hierarchy </summary>
	/// <param name="sourceElement">A <see cref="T:System.Configuration.ConfigurationElement" /> object at the current level containing a merged view of the properties.</param>
	/// <param name="parentElement">The parent <see cref="T:System.Configuration.ConfigurationElement" /> object of the current element, or null if this is the top level.</param>
	/// <param name="saveMode">A <see cref="T:System.Configuration.ConfigurationSaveMode" /> enumerated value that determines which property values to include.</param>
	protected internal override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
	{
		ConfigurationElementCollection configurationElementCollection = (ConfigurationElementCollection)sourceElement;
		ConfigurationElementCollection configurationElementCollection2 = (ConfigurationElementCollection)parentElement;
		for (int i = 0; i < configurationElementCollection.Count; i++)
		{
			ConfigurationElement configurationElement = configurationElementCollection.BaseGet(i);
			object elementKey = configurationElementCollection.GetElementKey(configurationElement);
			ConfigurationElement configurationElement2 = configurationElementCollection2?.BaseGet(elementKey);
			ConfigurationElement configurationElement3 = CreateNewElementInternal(null);
			if (configurationElement2 != null && saveMode != ConfigurationSaveMode.Full)
			{
				configurationElement3.Unmerge(configurationElement, configurationElement2, saveMode);
				if (configurationElement3.HasValues(configurationElement2, saveMode))
				{
					BaseAdd(configurationElement3);
				}
			}
			else
			{
				configurationElement3.Unmerge(configurationElement, null, ConfigurationSaveMode.Full);
				BaseAdd(configurationElement3);
			}
		}
		if (saveMode == ConfigurationSaveMode.Full)
		{
			EmitClear = true;
		}
		else
		{
			if (configurationElementCollection2 == null)
			{
				return;
			}
			for (int j = 0; j < configurationElementCollection2.Count; j++)
			{
				ConfigurationElement configurationElement4 = configurationElementCollection2.BaseGet(j);
				object elementKey2 = configurationElementCollection2.GetElementKey(configurationElement4);
				if (configurationElementCollection.IndexOfKey(elementKey2) == -1)
				{
					if (removed == null)
					{
						removed = new ArrayList();
					}
					removed.Add(configurationElement4);
				}
			}
		}
	}
}
