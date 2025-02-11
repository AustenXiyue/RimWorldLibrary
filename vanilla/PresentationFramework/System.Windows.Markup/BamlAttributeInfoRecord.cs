using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace System.Windows.Markup;

internal class BamlAttributeInfoRecord : BamlVariableSizedRecord
{
	private static BitVector32.Section _isInternalSection = BitVector32.CreateSection(1, BamlVariableSizedRecord.LastFlagsSection);

	private static BitVector32.Section _attributeUsageSection = BitVector32.CreateSection(3, _isInternalSection);

	private short _ownerId;

	private short _attributeId;

	private string _name;

	private Type _ownerType;

	private RoutedEvent _Event;

	private DependencyProperty _dp;

	private EventInfo _ei;

	private PropertyInfo _pi;

	private MethodInfo _smi;

	private MethodInfo _gmi;

	private object _dpOrMiOrPi;

	internal short OwnerTypeId
	{
		get
		{
			return _ownerId;
		}
		set
		{
			_ownerId = value;
		}
	}

	internal short AttributeId
	{
		get
		{
			return _attributeId;
		}
		set
		{
			_attributeId = value;
		}
	}

	internal string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal override BamlRecordType RecordType => BamlRecordType.AttributeInfo;

	internal object PropertyMember
	{
		get
		{
			return _dpOrMiOrPi;
		}
		set
		{
			_dpOrMiOrPi = value;
		}
	}

	internal Type OwnerType
	{
		get
		{
			return _ownerType;
		}
		set
		{
			_ownerType = value;
		}
	}

	internal RoutedEvent Event
	{
		get
		{
			return _Event;
		}
		set
		{
			_Event = value;
		}
	}

	internal DependencyProperty DP
	{
		get
		{
			if (_dp != null)
			{
				return _dp;
			}
			return _dpOrMiOrPi as DependencyProperty;
		}
		set
		{
			_dp = value;
			if (_dp != null)
			{
				_name = _dp.Name;
			}
		}
	}

	internal MethodInfo AttachedPropertySetter
	{
		get
		{
			return _smi;
		}
		set
		{
			_smi = value;
		}
	}

	internal MethodInfo AttachedPropertyGetter
	{
		get
		{
			return _gmi;
		}
		set
		{
			_gmi = value;
		}
	}

	internal EventInfo EventInfo
	{
		get
		{
			return _ei;
		}
		set
		{
			_ei = value;
		}
	}

	internal PropertyInfo PropInfo
	{
		get
		{
			return _pi;
		}
		set
		{
			_pi = value;
		}
	}

	internal bool IsInternal
	{
		get
		{
			return _flags[_isInternalSection] == 1;
		}
		set
		{
			_flags[_isInternalSection] = (value ? 1 : 0);
		}
	}

	internal BamlAttributeUsage AttributeUsage
	{
		get
		{
			return (BamlAttributeUsage)_flags[_attributeUsageSection];
		}
		set
		{
			_flags[_attributeUsageSection] = (int)value;
		}
	}

	internal new static BitVector32.Section LastFlagsSection => _attributeUsageSection;

	internal BamlAttributeInfoRecord()
	{
		Pin();
		AttributeUsage = BamlAttributeUsage.Default;
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		AttributeId = bamlBinaryReader.ReadInt16();
		OwnerTypeId = bamlBinaryReader.ReadInt16();
		AttributeUsage = (BamlAttributeUsage)bamlBinaryReader.ReadByte();
		Name = bamlBinaryReader.ReadString();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(AttributeId);
		bamlBinaryWriter.Write(OwnerTypeId);
		bamlBinaryWriter.Write((byte)AttributeUsage);
		bamlBinaryWriter.Write(Name);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlAttributeInfoRecord obj = (BamlAttributeInfoRecord)record;
		obj._ownerId = _ownerId;
		obj._attributeId = _attributeId;
		obj._name = _name;
		obj._ownerType = _ownerType;
		obj._Event = _Event;
		obj._dp = _dp;
		obj._ei = _ei;
		obj._pi = _pi;
		obj._smi = _smi;
		obj._gmi = _gmi;
		obj._dpOrMiOrPi = _dpOrMiOrPi;
	}

	internal Type GetPropertyType()
	{
		Type type = null;
		DependencyProperty dP = DP;
		if (dP == null)
		{
			MethodInfo attachedPropertySetter = AttachedPropertySetter;
			if (attachedPropertySetter == null)
			{
				return PropInfo.PropertyType;
			}
			return attachedPropertySetter.GetParameters()[1].ParameterType;
		}
		return dP.PropertyType;
	}

	internal void SetPropertyMember(object propertyMember)
	{
		if (PropertyMember == null)
		{
			PropertyMember = propertyMember;
		}
		else if (!(PropertyMember is object[] array))
		{
			object[] array2 = new object[3] { PropertyMember, propertyMember, null };
		}
		else
		{
			array[2] = propertyMember;
		}
	}

	internal object GetPropertyMember(bool onlyPropInfo)
	{
		if (PropertyMember == null || PropertyMember is MemberInfo || KnownTypes.Types[136].IsAssignableFrom(PropertyMember.GetType()))
		{
			if (onlyPropInfo)
			{
				return PropInfo;
			}
			return PropertyMember;
		}
		object[] array = (object[])PropertyMember;
		if (onlyPropInfo)
		{
			if (array[0] is PropertyInfo)
			{
				return (PropertyInfo)array[0];
			}
			if (array[1] is PropertyInfo)
			{
				return (PropertyInfo)array[1];
			}
			return array[2] as PropertyInfo;
		}
		return array[0];
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} owner={1} attr({2}) is '{3}'", RecordType, BamlRecord.GetTypeName(OwnerTypeId), AttributeId, _name);
	}
}
