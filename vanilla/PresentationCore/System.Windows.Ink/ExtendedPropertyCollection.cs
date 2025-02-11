using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

internal sealed class ExtendedPropertyCollection
{
	private List<ExtendedProperty> _extendedProperties = new List<ExtendedProperty>();

	private int _optimisticIndex = -1;

	internal object this[Guid attributeId]
	{
		get
		{
			ExtendedProperty extendedPropertyById = GetExtendedPropertyById(attributeId);
			if (extendedPropertyById == null)
			{
				throw new ArgumentException(SR.EPNotFound, "attributeId");
			}
			return extendedPropertyById.Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			for (int i = 0; i < _extendedProperties.Count; i++)
			{
				ExtendedProperty extendedProperty = _extendedProperties[i];
				if (extendedProperty.Id == attributeId)
				{
					object value2 = extendedProperty.Value;
					extendedProperty.Value = value;
					if (this.Changed != null)
					{
						ExtendedPropertiesChangedEventArgs e = new ExtendedPropertiesChangedEventArgs(new ExtendedProperty(extendedProperty.Id, value2), extendedProperty);
						this.Changed(this, e);
					}
					return;
				}
			}
			ExtendedProperty extendedProperty2 = new ExtendedProperty(attributeId, value);
			Add(extendedProperty2);
		}
	}

	internal ExtendedProperty this[int index] => _extendedProperties[index];

	internal int Count => _extendedProperties.Count;

	internal event ExtendedPropertiesChangedEventHandler Changed;

	internal ExtendedPropertyCollection()
	{
	}

	public override bool Equals(object o)
	{
		if (o == null || o.GetType() != GetType())
		{
			return false;
		}
		ExtendedPropertyCollection extendedPropertyCollection = (ExtendedPropertyCollection)o;
		if (extendedPropertyCollection.Count != Count)
		{
			return false;
		}
		for (int i = 0; i < extendedPropertyCollection.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < _extendedProperties.Count; j++)
			{
				if (_extendedProperties[j].Equals(extendedPropertyCollection[i]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator ==(ExtendedPropertyCollection first, ExtendedPropertyCollection second)
	{
		if (((object)first == null && (object)second == null) || (object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Equals(second);
	}

	public static bool operator !=(ExtendedPropertyCollection first, ExtendedPropertyCollection second)
	{
		return !(first == second);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	internal bool Contains(Guid attributeId)
	{
		for (int i = 0; i < _extendedProperties.Count; i++)
		{
			if (_extendedProperties[i].Id == attributeId)
			{
				_optimisticIndex = i;
				return true;
			}
		}
		return false;
	}

	internal ExtendedPropertyCollection Clone()
	{
		ExtendedPropertyCollection extendedPropertyCollection = new ExtendedPropertyCollection();
		for (int i = 0; i < _extendedProperties.Count; i++)
		{
			extendedPropertyCollection.Add(_extendedProperties[i].Clone());
		}
		return extendedPropertyCollection;
	}

	internal void Add(Guid id, object value)
	{
		if (Contains(id))
		{
			throw new ArgumentException(SR.EPExists, "id");
		}
		ExtendedProperty extendedProperty = new ExtendedProperty(id, value);
		Add(extendedProperty);
	}

	internal void Remove(Guid id)
	{
		if (!Contains(id))
		{
			throw new ArgumentException(SR.EPGuidNotFound, "id");
		}
		ExtendedProperty extendedPropertyById = GetExtendedPropertyById(id);
		_extendedProperties.Remove(extendedPropertyById);
		_optimisticIndex = -1;
		if (this.Changed != null)
		{
			ExtendedPropertiesChangedEventArgs e = new ExtendedPropertiesChangedEventArgs(extendedPropertyById, null);
			this.Changed(this, e);
		}
	}

	internal Guid[] GetGuidArray()
	{
		if (_extendedProperties.Count > 0)
		{
			Guid[] array = new Guid[_extendedProperties.Count];
			for (int i = 0; i < _extendedProperties.Count; i++)
			{
				array[i] = this[i].Id;
			}
			return array;
		}
		return Array.Empty<Guid>();
	}

	private void Add(ExtendedProperty extendedProperty)
	{
		_extendedProperties.Add(extendedProperty);
		if (this.Changed != null)
		{
			ExtendedPropertiesChangedEventArgs e = new ExtendedPropertiesChangedEventArgs(null, extendedProperty);
			this.Changed(this, e);
		}
	}

	private ExtendedProperty GetExtendedPropertyById(Guid id)
	{
		if (_optimisticIndex != -1 && _optimisticIndex < _extendedProperties.Count && _extendedProperties[_optimisticIndex].Id == id)
		{
			return _extendedProperties[_optimisticIndex];
		}
		for (int i = 0; i < _extendedProperties.Count; i++)
		{
			if (_extendedProperties[i].Id == id)
			{
				return _extendedProperties[i];
			}
		}
		return null;
	}
}
