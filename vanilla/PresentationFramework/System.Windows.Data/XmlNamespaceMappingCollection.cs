using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Xml;

namespace System.Windows.Data;

/// <summary>Represents a collection of <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> objects.</summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public class XmlNamespaceMappingCollection : XmlNamespaceManager, ICollection<XmlNamespaceMapping>, IEnumerable<XmlNamespaceMapping>, IEnumerable, IAddChildInternal, IAddChild
{
	/// <summary>Gets the number of <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> objects in the collection.</summary>
	/// <returns>The number of <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> objects in the collection.</returns>
	public int Count
	{
		get
		{
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					_ = (XmlNamespaceMapping)enumerator.Current;
					num++;
				}
				return num;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}

	/// <summary>Gets a value that indicates whether this collection is read-only.</summary>
	/// <returns>This always returns false.</returns>
	public bool IsReadOnly => false;

	private IEnumerator BaseEnumerator => base.GetEnumerator();

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.XmlNamespaceMappingCollection" /> class.</summary>
	public XmlNamespaceMappingCollection()
		: base(new NameTable())
	{
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Markup.IAddChild.AddChild(System.Object)" />.</summary>
	/// <param name="value">The child <see cref="T:System.Object" /> to add.</param>
	void IAddChild.AddChild(object value)
	{
		AddChild(value);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to this collection.</summary>
	/// <param name="value">The <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to add. This cannot be null.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="mapping" /> is null.</exception>
	protected virtual void AddChild(object value)
	{
		XmlNamespaceMapping xmlNamespaceMapping = value as XmlNamespaceMapping;
		if (xmlNamespaceMapping == null)
		{
			throw new ArgumentException(SR.Format(SR.RequiresXmlNamespaceMapping, value.GetType().FullName), "value");
		}
		Add(xmlNamespaceMapping);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Markup.IAddChild.AddText(System.String)" />.</summary>
	/// <param name="text">The text to add to the <see cref="T:System.Object" />.</param>
	void IAddChild.AddText(string text)
	{
		AddText(text);
	}

	/// <summary>Adds a text string as a child of this <see cref="T:System.Windows.Data.XmlNamespaceMappingCollection" /> object.</summary>
	/// <param name="text">The text string to add as a child.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="text" /> is null.</exception>
	protected virtual void AddText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to this collection.</summary>
	/// <param name="mapping">The <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to add. This cannot be null.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="mapping" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">If the <see cref="P:System.Windows.Data.XmlNamespaceMapping.Uri" /> of the <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object is null. </exception>
	public void Add(XmlNamespaceMapping mapping)
	{
		if (mapping == null)
		{
			throw new ArgumentNullException("mapping");
		}
		if (mapping.Uri == null)
		{
			throw new ArgumentException(SR.RequiresXmlNamespaceMappingUri, "mapping");
		}
		AddNamespace(mapping.Prefix, mapping.Uri.OriginalString);
	}

	/// <summary>Removes all <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> objects in this collection.</summary>
	public void Clear()
	{
		int count = Count;
		XmlNamespaceMapping[] array = new XmlNamespaceMapping[count];
		CopyTo(array, 0);
		for (int i = 0; i < count; i++)
		{
			Remove(array[i]);
		}
	}

	/// <summary>Returns a value that indicates whether this collection contains the specified <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object.</summary>
	/// <returns>true if this collection contains the specified <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object; otherwise, false.</returns>
	/// <param name="mapping">The <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object of interest. This cannot be null.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="mapping" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">If the <see cref="P:System.Windows.Data.XmlNamespaceMapping.Uri" /> of the <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object is null. </exception>
	public bool Contains(XmlNamespaceMapping mapping)
	{
		if (mapping == null)
		{
			throw new ArgumentNullException("mapping");
		}
		if (mapping.Uri == null)
		{
			throw new ArgumentException(SR.RequiresXmlNamespaceMappingUri, "mapping");
		}
		return LookupNamespace(mapping.Prefix) == mapping.Uri.OriginalString;
	}

	/// <summary>Copies the items of the collection to the specified array, starting at the specified index.</summary>
	/// <param name="array">The array that is the destination of the items copied from the collection.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying starts.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">If the number of items exceed the length of the array. </exception>
	public void CopyTo(XmlNamespaceMapping[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = arrayIndex;
		int num2 = array.Length;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				XmlNamespaceMapping xmlNamespaceMapping = (XmlNamespaceMapping)enumerator.Current;
				if (num >= num2)
				{
					throw new ArgumentException(SR.Format(SR.Collection_CopyTo_NumberOfElementsExceedsArrayLength, "arrayIndex", "array"));
				}
				array[num] = xmlNamespaceMapping;
				num++;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object from this collection.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object has been successfully removed; otherwise, false.</returns>
	/// <param name="mapping">The <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to remove. This cannot be null.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="mapping" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">If the <see cref="P:System.Windows.Data.XmlNamespaceMapping.Uri" /> of the <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object is null. </exception>
	public bool Remove(XmlNamespaceMapping mapping)
	{
		if (mapping == null)
		{
			throw new ArgumentNullException("mapping");
		}
		if (mapping.Uri == null)
		{
			throw new ArgumentException(SR.RequiresXmlNamespaceMappingUri, "mapping");
		}
		if (Contains(mapping))
		{
			RemoveNamespace(mapping.Prefix, mapping.Uri.OriginalString);
			return true;
		}
		return false;
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> object that you can use to enumerate the items in this collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that you can use to enumerate the items in this collection.</returns>
	public override IEnumerator GetEnumerator()
	{
		return ProtectedGetEnumerator();
	}

	IEnumerator<XmlNamespaceMapping> IEnumerable<XmlNamespaceMapping>.GetEnumerator()
	{
		return ProtectedGetEnumerator();
	}

	/// <summary>Returns a generic <see cref="T:System.Collections.Generic.IEnumerator`1" /> object.</summary>
	/// <returns>A generic <see cref="T:System.Collections.Generic.IEnumerator`1" /> object.</returns>
	protected IEnumerator<XmlNamespaceMapping> ProtectedGetEnumerator()
	{
		IEnumerator enumerator = BaseEnumerator;
		while (enumerator.MoveNext())
		{
			string text = (string)enumerator.Current;
			if (!(text == "xmlns") && !(text == "xml"))
			{
				string text2 = LookupNamespace(text);
				if (!(text == string.Empty) || !(text2 == string.Empty))
				{
					Uri uri = new Uri(text2, UriKind.RelativeOrAbsolute);
					yield return new XmlNamespaceMapping(text, uri);
				}
			}
		}
	}
}
