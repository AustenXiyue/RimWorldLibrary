using System.Collections;
using System.Globalization;
using System.Security.Permissions;
using Unity;

namespace System.ComponentModel.Design;

/// <summary>Provides a base class for getting and setting option values for a designer.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class DesignerOptionService : IDesignerOptionService
{
	/// <summary>Contains a collection of designer options. This class cannot be inherited.</summary>
	[TypeConverter(typeof(DesignerOptionConverter))]
	[Editor("", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public sealed class DesignerOptionCollection : IList, ICollection, IEnumerable
	{
		private sealed class WrappedPropertyDescriptor : PropertyDescriptor
		{
			private object target;

			private PropertyDescriptor property;

			public override AttributeCollection Attributes => property.Attributes;

			public override Type ComponentType => property.ComponentType;

			public override bool IsReadOnly => property.IsReadOnly;

			public override Type PropertyType => property.PropertyType;

			internal WrappedPropertyDescriptor(PropertyDescriptor property, object target)
				: base(property.Name, null)
			{
				this.property = property;
				this.target = target;
			}

			public override bool CanResetValue(object component)
			{
				return property.CanResetValue(target);
			}

			public override object GetValue(object component)
			{
				return property.GetValue(target);
			}

			public override void ResetValue(object component)
			{
				property.ResetValue(target);
			}

			public override void SetValue(object component, object value)
			{
				property.SetValue(target, value);
			}

			public override bool ShouldSerializeValue(object component)
			{
				return property.ShouldSerializeValue(target);
			}
		}

		private DesignerOptionService _service;

		private DesignerOptionCollection _parent;

		private string _name;

		private object _value;

		private ArrayList _children;

		private PropertyDescriptorCollection _properties;

		/// <summary>Gets the number of child option collections this <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" /> contains.</summary>
		/// <returns>The number of child option collections this <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" /> contains.</returns>
		public int Count
		{
			get
			{
				EnsurePopulated();
				return _children.Count;
			}
		}

		/// <summary>Gets the name of this <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" />.</summary>
		/// <returns>The name of this <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" />.</returns>
		public string Name => _name;

		/// <summary>Gets the parent collection object.</summary>
		/// <returns>The parent collection object, or null if there is no parent.</returns>
		public DesignerOptionCollection Parent => _parent;

		/// <summary>Gets the collection of properties offered by this <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" />, along with all of its children.</summary>
		/// <returns>The collection of properties offered by this <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" />, along with all of its children.</returns>
		public PropertyDescriptorCollection Properties
		{
			get
			{
				if (_properties == null)
				{
					ArrayList arrayList;
					if (_value != null)
					{
						PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(_value);
						arrayList = new ArrayList(properties.Count);
						foreach (PropertyDescriptor item in properties)
						{
							arrayList.Add(new WrappedPropertyDescriptor(item, _value));
						}
					}
					else
					{
						arrayList = new ArrayList(1);
					}
					EnsurePopulated();
					foreach (DesignerOptionCollection child in _children)
					{
						arrayList.AddRange(child.Properties);
					}
					PropertyDescriptor[] properties2 = (PropertyDescriptor[])arrayList.ToArray(typeof(PropertyDescriptor));
					_properties = new PropertyDescriptorCollection(properties2, readOnly: true);
				}
				return _properties;
			}
		}

		/// <summary>Gets the child collection at the given index.</summary>
		/// <returns>The child collection at the specified index.</returns>
		/// <param name="index">The zero-based index of the child collection to get.</param>
		public DesignerOptionCollection this[int index]
		{
			get
			{
				EnsurePopulated();
				if (index < 0 || index >= _children.Count)
				{
					throw new IndexOutOfRangeException("index");
				}
				return (DesignerOptionCollection)_children[index];
			}
		}

		/// <summary>Gets the child collection at the given name.</summary>
		/// <returns>The child collection with the name specified by the <paramref name="name" /> parameter, or null if the name is not found.</returns>
		/// <param name="name">The name of the child collection.</param>
		public DesignerOptionCollection this[string name]
		{
			get
			{
				EnsurePopulated();
				foreach (DesignerOptionCollection child in _children)
				{
					if (string.Compare(child.Name, name, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
					{
						return child;
					}
				}
				return null;
			}
		}

		/// <summary>Gets a value indicating whether access to the collection is synchronized and, therefore, thread safe.</summary>
		/// <returns>true if the access to the collection is synchronized; otherwise, false.</returns>
		bool ICollection.IsSynchronized => false;

		/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
		/// <returns>An object that can be used to synchronize access to the collection.</returns>
		object ICollection.SyncRoot => this;

		/// <summary>Gets a value indicating whether the collection has a fixed size.</summary>
		/// <returns>true if the collection has a fixed size; otherwise, false.</returns>
		bool IList.IsFixedSize => true;

		/// <summary>Gets a value indicating whether the collection is read-only.</summary>
		/// <returns>true if the collection is read-only; otherwise, false.</returns>
		bool IList.IsReadOnly => true;

		/// <summary>Gets or sets the element at the specified index.</summary>
		/// <returns>The element at the specified index.</returns>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		internal DesignerOptionCollection(DesignerOptionService service, DesignerOptionCollection parent, string name, object value)
		{
			_service = service;
			_parent = parent;
			_name = name;
			_value = value;
			if (_parent != null)
			{
				if (_parent._children == null)
				{
					_parent._children = new ArrayList(1);
				}
				_parent._children.Add(this);
			}
		}

		/// <summary>Copies the entire collection to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the collection. The <paramref name="array" /> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		public void CopyTo(Array array, int index)
		{
			EnsurePopulated();
			_children.CopyTo(array, index);
		}

		private void EnsurePopulated()
		{
			if (_children == null)
			{
				_service.PopulateOptionCollection(this);
				if (_children == null)
				{
					_children = new ArrayList(1);
				}
			}
		}

		/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate this collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate this collection.</returns>
		public IEnumerator GetEnumerator()
		{
			EnsurePopulated();
			return _children.GetEnumerator();
		}

		/// <summary>Returns the index of the first occurrence of a given value in a range of this collection.</summary>
		/// <returns>The index of the first occurrence of value within the entire collection, if found; otherwise, the lower bound of the collection minus 1.</returns>
		/// <param name="value">The object to locate in the collection.</param>
		public int IndexOf(DesignerOptionCollection value)
		{
			EnsurePopulated();
			return _children.IndexOf(value);
		}

		private static object RecurseFindValue(DesignerOptionCollection options)
		{
			if (options._value != null)
			{
				return options._value;
			}
			foreach (DesignerOptionCollection option in options)
			{
				object obj = RecurseFindValue(option);
				if (obj != null)
				{
					return obj;
				}
			}
			return null;
		}

		/// <summary>Displays a dialog box user interface (UI) with which the user can configure the options in this <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" />.</summary>
		/// <returns>true if the dialog box can be displayed; otherwise, false.</returns>
		public bool ShowDialog()
		{
			object obj = RecurseFindValue(this);
			if (obj == null)
			{
				return false;
			}
			return _service.ShowDialog(this, obj);
		}

		/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
		/// <returns>The position into which the new element was inserted.</returns>
		/// <param name="value">The <see cref="T:System.Object" /> to add to the <see cref="T:System.Collections.IList" />.</param>
		int IList.Add(object value)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes all items from the collection.</summary>
		void IList.Clear()
		{
			throw new NotSupportedException();
		}

		/// <summary>Determines whether the collection contains a specific value.</summary>
		/// <returns>true if the <see cref="T:System.Object" /> is found in the collection; otherwise, false. </returns>
		/// <param name="value">The <see cref="T:System.Object" /> to locate in the collection</param>
		bool IList.Contains(object value)
		{
			EnsurePopulated();
			return _children.Contains(value);
		}

		/// <summary>Determines the index of a specific item in the collection.</summary>
		/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
		/// <param name="value">The <see cref="T:System.Object" /> to locate in the collection.</param>
		int IList.IndexOf(object value)
		{
			EnsurePopulated();
			return _children.IndexOf(value);
		}

		/// <summary>Inserts an item into the collection at the specified index.</summary>
		/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
		/// <param name="value">The <see cref="T:System.Object" /> to insert into the collection.</param>
		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes the first occurrence of a specific object from the collection.</summary>
		/// <param name="value">The <see cref="T:System.Object" /> to remove from the collection.</param>
		void IList.Remove(object value)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes the collection item at the specified index.</summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		internal DesignerOptionCollection()
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	internal sealed class DesignerOptionConverter : TypeConverter
	{
		private class OptionPropertyDescriptor : PropertyDescriptor
		{
			private DesignerOptionCollection _option;

			public override Type ComponentType => _option.GetType();

			public override bool IsReadOnly => true;

			public override Type PropertyType => _option.GetType();

			internal OptionPropertyDescriptor(DesignerOptionCollection option)
				: base(option.Name, null)
			{
				_option = option;
			}

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override object GetValue(object component)
			{
				return _option;
			}

			public override void ResetValue(object component)
			{
			}

			public override void SetValue(object component, object value)
			{
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext cxt)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext cxt, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = new PropertyDescriptorCollection(null);
			if (!(value is DesignerOptionCollection designerOptionCollection))
			{
				return propertyDescriptorCollection;
			}
			foreach (DesignerOptionCollection item in designerOptionCollection)
			{
				propertyDescriptorCollection.Add(new OptionPropertyDescriptor(item));
			}
			foreach (PropertyDescriptor property in designerOptionCollection.Properties)
			{
				propertyDescriptorCollection.Add(property);
			}
			return propertyDescriptorCollection;
		}

		public override object ConvertTo(ITypeDescriptorContext cxt, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return global::SR.GetString("(Collection)");
			}
			return base.ConvertTo(cxt, culture, value, destinationType);
		}
	}

	private DesignerOptionCollection _options;

	/// <summary>Gets the options collection for this service.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" /> populated with available designer options.</returns>
	public DesignerOptionCollection Options
	{
		get
		{
			if (_options == null)
			{
				_options = new DesignerOptionCollection(this, null, string.Empty, null);
			}
			return _options;
		}
	}

	/// <summary>Creates a new <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" /> with the given name and adds it to the given parent. </summary>
	/// <returns>A new <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" /> with the given name. </returns>
	/// <param name="parent">The parent designer option collection. All collections have a parent except the root object collection.</param>
	/// <param name="name">The name of this collection.</param>
	/// <param name="value">The object providing properties for this collection. Can be null if the collection should not provide any properties.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="parent" /> or <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string.</exception>
	protected DesignerOptionCollection CreateOptionCollection(DesignerOptionCollection parent, string name, object value)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("'{1}' is not a valid value for '{0}'.", name.Length.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture)), "name.Length");
		}
		return new DesignerOptionCollection(this, parent, name, value);
	}

	private PropertyDescriptor GetOptionProperty(string pageName, string valueName)
	{
		if (pageName == null)
		{
			throw new ArgumentNullException("pageName");
		}
		if (valueName == null)
		{
			throw new ArgumentNullException("valueName");
		}
		string[] array = pageName.Split('\\');
		DesignerOptionCollection designerOptionCollection = Options;
		string[] array2 = array;
		foreach (string name in array2)
		{
			designerOptionCollection = designerOptionCollection[name];
			if (designerOptionCollection == null)
			{
				return null;
			}
		}
		return designerOptionCollection.Properties[valueName];
	}

	/// <summary>Populates a <see cref="T:System.ComponentModel.Design.DesignerOptionService.DesignerOptionCollection" />.</summary>
	/// <param name="options">The collection to populate.</param>
	protected virtual void PopulateOptionCollection(DesignerOptionCollection options)
	{
	}

	/// <summary>Shows the options dialog box for the given object.</summary>
	/// <returns>true if the dialog box is shown; otherwise, false.</returns>
	/// <param name="options">The options collection containing the object to be invoked.</param>
	/// <param name="optionObject">The actual options object.</param>
	protected virtual bool ShowDialog(DesignerOptionCollection options, object optionObject)
	{
		return false;
	}

	/// <summary>Gets the value of an option defined in this package.</summary>
	/// <returns>The value of the option named <paramref name="valueName" />.</returns>
	/// <param name="pageName">The page to which the option is bound.</param>
	/// <param name="valueName">The name of the option value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pageName" /> or <paramref name="valueName" /> is null.</exception>
	object IDesignerOptionService.GetOptionValue(string pageName, string valueName)
	{
		return GetOptionProperty(pageName, valueName)?.GetValue(null);
	}

	/// <summary>Sets the value of an option defined in this package.</summary>
	/// <param name="pageName">The page to which the option is bound</param>
	/// <param name="valueName">The name of the option value.</param>
	/// <param name="value">The value of the option.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pageName" /> or <paramref name="valueName" /> is null.</exception>
	void IDesignerOptionService.SetOptionValue(string pageName, string valueName, object value)
	{
		GetOptionProperty(pageName, valueName)?.SetValue(null, value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerOptionService" /> class. </summary>
	protected DesignerOptionService()
	{
	}
}
