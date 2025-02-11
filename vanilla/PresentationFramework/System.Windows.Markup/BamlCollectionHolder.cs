using System.Collections;
using System.Reflection;

namespace System.Windows.Markup;

internal class BamlCollectionHolder
{
	private object _collection;

	private object _defaultCollection;

	private short _attributeId;

	private WpfPropertyDefinition _propDef;

	private object _parent;

	private BamlRecordReader _reader;

	private IHaveResources _resourcesParent;

	private bool _readonly;

	private bool _isClosed;

	private bool _isPropertyValueSet;

	internal object Collection
	{
		get
		{
			return _collection;
		}
		set
		{
			_collection = value;
		}
	}

	internal IList List => _collection as IList;

	internal IDictionary Dictionary => _collection as IDictionary;

	internal ArrayExtension ArrayExt => _collection as ArrayExtension;

	internal object DefaultCollection => _defaultCollection;

	internal WpfPropertyDefinition PropertyDefinition => _propDef;

	internal Type PropertyType
	{
		get
		{
			if (_resourcesParent == null)
			{
				return PropertyDefinition.PropertyType;
			}
			return typeof(ResourceDictionary);
		}
	}

	internal object Parent => _parent;

	internal bool ReadOnly
	{
		get
		{
			return _readonly;
		}
		set
		{
			_readonly = value;
		}
	}

	internal bool IsClosed
	{
		get
		{
			return _isClosed;
		}
		set
		{
			_isClosed = value;
		}
	}

	internal string AttributeName => _reader.GetPropertyNameFromAttributeId(_attributeId);

	internal BamlCollectionHolder()
	{
	}

	internal BamlCollectionHolder(BamlRecordReader reader, object parent, short attributeId)
		: this(reader, parent, attributeId, needDefault: true)
	{
	}

	internal BamlCollectionHolder(BamlRecordReader reader, object parent, short attributeId, bool needDefault)
	{
		_reader = reader;
		_parent = parent;
		_propDef = new WpfPropertyDefinition(reader, attributeId, parent is DependencyObject);
		_attributeId = attributeId;
		if (needDefault)
		{
			InitDefaultValue();
		}
		CheckReadOnly();
	}

	internal void SetPropertyValue()
	{
		if (_isPropertyValueSet)
		{
			return;
		}
		_isPropertyValueSet = true;
		if (_resourcesParent != null)
		{
			_resourcesParent.Resources = (ResourceDictionary)Collection;
		}
		else if (PropertyDefinition.DependencyProperty != null)
		{
			DependencyObject dependencyObject = Parent as DependencyObject;
			if (dependencyObject == null)
			{
				_reader.ThrowException("ParserParentDO", Parent.ToString());
			}
			_reader.SetDependencyValue(dependencyObject, PropertyDefinition.DependencyProperty, Collection);
		}
		else if (PropertyDefinition.AttachedPropertySetter != null)
		{
			PropertyDefinition.AttachedPropertySetter.Invoke(null, new object[2] { Parent, Collection });
		}
		else if (PropertyDefinition.PropertyInfo != null)
		{
			PropertyDefinition.PropertyInfo.SetValue(Parent, Collection, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, null, TypeConverterHelper.InvariantEnglishUS);
		}
		else
		{
			_reader.ThrowException("ParserCantGetDPOrPi", AttributeName);
		}
	}

	internal void InitDefaultValue()
	{
		if (AttributeName == "Resources" && Parent is IHaveResources)
		{
			_resourcesParent = (IHaveResources)Parent;
			_defaultCollection = _resourcesParent.Resources;
		}
		else if (PropertyDefinition.DependencyProperty != null)
		{
			_defaultCollection = ((DependencyObject)Parent).GetValue(PropertyDefinition.DependencyProperty);
		}
		else if (PropertyDefinition.AttachedPropertyGetter != null)
		{
			_defaultCollection = PropertyDefinition.AttachedPropertyGetter.Invoke(null, new object[1] { Parent });
		}
		else if (PropertyDefinition.PropertyInfo != null)
		{
			if (PropertyDefinition.IsInternal)
			{
				_defaultCollection = XamlTypeMapper.GetInternalPropertyValue(_reader.ParserContext, _reader.ParserContext.RootElement, PropertyDefinition.PropertyInfo, Parent);
				if (_defaultCollection == null)
				{
					_reader.ThrowException("ParserCantGetProperty", PropertyDefinition.Name);
				}
			}
			else
			{
				_defaultCollection = PropertyDefinition.PropertyInfo.GetValue(Parent, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, null, TypeConverterHelper.InvariantEnglishUS);
			}
		}
		else
		{
			_reader.ThrowException("ParserCantGetDPOrPi", AttributeName);
		}
	}

	private void CheckReadOnly()
	{
		if (_resourcesParent == null && (PropertyDefinition.DependencyProperty == null || PropertyDefinition.DependencyProperty.ReadOnly) && (PropertyDefinition.PropertyInfo == null || !PropertyDefinition.PropertyInfo.CanWrite) && PropertyDefinition.AttachedPropertySetter == null)
		{
			if (DefaultCollection == null)
			{
				_reader.ThrowException("ParserReadOnlyNullProperty", PropertyDefinition.Name);
			}
			ReadOnly = true;
			Collection = DefaultCollection;
		}
	}
}
