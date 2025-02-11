using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace System.Windows.Markup;

internal class BamlMapTable
{
	private const string _coreAssembly = "PresentationCore";

	private const string _frameworkAssembly = "PresentationFramework";

	private static string[] _knownStrings = new string[3] { null, "Name", "Uid" };

	internal static short NameStringId = -1;

	internal static short UidStringId = -2;

	internal static string NameString = "Name";

	private Hashtable _objectHashTable = new Hashtable();

	private ArrayList _assemblyIdToInfo = new ArrayList(1);

	private ArrayList _typeIdToInfo = new ArrayList(0);

	private ArrayList _attributeIdToInfo = new ArrayList(10);

	private ArrayList _stringIdToInfo = new ArrayList(1);

	private XamlTypeMapper _xamlTypeMapper;

	private BamlAssemblyInfoRecord _knownAssemblyInfoRecord;

	private Hashtable _converterCache;

	private bool _reusingMapTable;

	private Hashtable ObjectHashTable => _objectHashTable;

	private ArrayList AssemblyIdMap => _assemblyIdToInfo;

	private ArrayList TypeIdMap => _typeIdToInfo;

	private ArrayList AttributeIdMap => _attributeIdToInfo;

	private ArrayList StringIdMap => _stringIdToInfo;

	internal XamlTypeMapper XamlTypeMapper
	{
		get
		{
			return _xamlTypeMapper;
		}
		set
		{
			_xamlTypeMapper = value;
		}
	}

	private Hashtable ConverterCache
	{
		get
		{
			if (_converterCache == null)
			{
				_converterCache = new Hashtable();
			}
			return _converterCache;
		}
	}

	internal BamlMapTable(XamlTypeMapper xamlTypeMapper)
	{
		_xamlTypeMapper = xamlTypeMapper;
		_knownAssemblyInfoRecord = new BamlAssemblyInfoRecord();
		_knownAssemblyInfoRecord.AssemblyId = -1;
		_knownAssemblyInfoRecord.Assembly = ReflectionHelper.LoadAssembly("PresentationFramework", string.Empty);
		_knownAssemblyInfoRecord.AssemblyFullName = _knownAssemblyInfoRecord.Assembly.FullName;
	}

	internal object CreateKnownTypeFromId(short id)
	{
		if (id < 0)
		{
			return KnownTypes.CreateKnownElement((KnownElements)(-id));
		}
		return null;
	}

	internal static Type GetKnownTypeFromId(short id)
	{
		if (id < 0)
		{
			return KnownTypes.Types[-id];
		}
		return null;
	}

	internal static short GetKnownTypeIdFromName(string assemblyFullName, string clrNamespace, string typeShortName)
	{
		if (typeShortName == string.Empty)
		{
			return 0;
		}
		int num = 759;
		int num2 = 1;
		while (num2 <= num)
		{
			int num3 = (num + num2) / 2;
			Type type = KnownTypes.Types[num3];
			int num4 = string.CompareOrdinal(typeShortName, type.Name);
			if (num4 == 0)
			{
				if (type.Namespace == clrNamespace && type.Assembly.FullName == assemblyFullName)
				{
					return (short)(-num3);
				}
				return 0;
			}
			if (num4 < 0)
			{
				num = num3 - 1;
			}
			else
			{
				num2 = num3 + 1;
			}
		}
		return 0;
	}

	internal static short GetKnownTypeIdFromType(Type type)
	{
		if (type == null)
		{
			return 0;
		}
		return GetKnownTypeIdFromName(type.Assembly.FullName, type.Namespace, type.Name);
	}

	private static short GetKnownStringIdFromName(string stringValue)
	{
		int num = _knownStrings.Length;
		for (int i = 1; i < num; i++)
		{
			if (_knownStrings[i] == stringValue)
			{
				return (short)(-i);
			}
		}
		return 0;
	}

	internal static KnownElements GetKnownTypeConverterIdFromType(Type type)
	{
		if (ReflectionHelper.IsNullableType(type))
		{
			return KnownElements.NullableConverter;
		}
		if (type == typeof(Type))
		{
			return KnownElements.TypeTypeConverter;
		}
		short knownTypeIdFromType = GetKnownTypeIdFromType(type);
		if (knownTypeIdFromType < 0)
		{
			return KnownTypes.GetKnownTypeConverterId((KnownElements)(-knownTypeIdFromType));
		}
		return KnownElements.UnknownElement;
	}

	internal TypeConverter GetKnownConverterFromType(Type type)
	{
		KnownElements knownTypeConverterIdFromType = GetKnownTypeConverterIdFromType(type);
		if (knownTypeConverterIdFromType != 0)
		{
			return GetConverterFromId(0 - knownTypeConverterIdFromType, type, null);
		}
		return null;
	}

	internal static TypeConverter GetKnownConverterFromType_NoCache(Type type)
	{
		KnownElements knownTypeConverterIdFromType = GetKnownTypeConverterIdFromType(type);
		return knownTypeConverterIdFromType switch
		{
			KnownElements.EnumConverter => new EnumConverter(type), 
			KnownElements.NullableConverter => new NullableConverter(type), 
			KnownElements.UnknownElement => null, 
			_ => KnownTypes.CreateKnownElement(knownTypeConverterIdFromType) as TypeConverter, 
		};
	}

	internal Type GetKnownConverterTypeFromType(Type type)
	{
		if (type == typeof(Type))
		{
			return typeof(TypeTypeConverter);
		}
		short knownTypeIdFromType = GetKnownTypeIdFromType(type);
		if (knownTypeIdFromType == 0)
		{
			return null;
		}
		KnownElements knownTypeConverterId = KnownTypes.GetKnownTypeConverterId((KnownElements)(-knownTypeIdFromType));
		if (knownTypeConverterId == KnownElements.UnknownElement)
		{
			return null;
		}
		return KnownTypes.Types[(int)knownTypeConverterId];
	}

	private static Type GetKnownConverterTypeFromPropName(Type propOwnerType, string propName)
	{
		short knownTypeIdFromType = GetKnownTypeIdFromType(propOwnerType);
		if (knownTypeIdFromType == 0)
		{
			return null;
		}
		KnownElements knownTypeConverterIdForProperty = KnownTypes.GetKnownTypeConverterIdForProperty((KnownElements)(-knownTypeIdFromType), propName);
		if (knownTypeConverterIdForProperty == KnownElements.UnknownElement)
		{
			return null;
		}
		return KnownTypes.Types[(int)knownTypeConverterIdForProperty];
	}

	internal void Initialize()
	{
		if (AttributeIdMap.Count > 0 || TypeIdMap.Count > 0)
		{
			_reusingMapTable = true;
			if (ObjectHashTable.Count == 0)
			{
				for (int i = 0; i < AttributeIdMap.Count; i++)
				{
					BamlAttributeInfoRecord bamlAttributeInfoRecord = AttributeIdMap[i] as BamlAttributeInfoRecord;
					if (bamlAttributeInfoRecord.PropInfo != null)
					{
						object attributeInfoKey = GetAttributeInfoKey(bamlAttributeInfoRecord.OwnerType.FullName, bamlAttributeInfoRecord.Name);
						ObjectHashTable.Add(attributeInfoKey, bamlAttributeInfoRecord);
					}
				}
				for (int j = 0; j < TypeIdMap.Count; j++)
				{
					BamlTypeInfoRecord bamlTypeInfoRecord = TypeIdMap[j] as BamlTypeInfoRecord;
					if (bamlTypeInfoRecord.Type != null)
					{
						BamlAssemblyInfoRecord assemblyInfoFromId = GetAssemblyInfoFromId(bamlTypeInfoRecord.AssemblyId);
						TypeInfoKey typeInfoKey = GetTypeInfoKey(assemblyInfoFromId.AssemblyFullName, bamlTypeInfoRecord.TypeFullName);
						ObjectHashTable.Add(typeInfoKey, bamlTypeInfoRecord);
					}
				}
			}
		}
		AssemblyIdMap.Clear();
		TypeIdMap.Clear();
		AttributeIdMap.Clear();
		StringIdMap.Clear();
	}

	internal Type GetTypeFromId(short id)
	{
		Type type = null;
		if (id < 0)
		{
			return KnownTypes.Types[-id];
		}
		BamlTypeInfoRecord bamlTypeInfoRecord = (BamlTypeInfoRecord)TypeIdMap[id];
		if (bamlTypeInfoRecord != null)
		{
			type = GetTypeFromTypeInfo(bamlTypeInfoRecord);
			if (null == type)
			{
				ThrowException("ParserFailFindType", bamlTypeInfoRecord.TypeFullName);
			}
		}
		return type;
	}

	internal bool HasSerializerForTypeId(short id)
	{
		if (id >= 0)
		{
			return false;
		}
		if (-id == 620 || -id == 231 || -id == 108 || -id == 120 || -id == 271 || -id == 330)
		{
			return true;
		}
		return false;
	}

	internal BamlTypeInfoRecord GetTypeInfoFromId(short id)
	{
		if (id < 0)
		{
			BamlTypeInfoRecord bamlTypeInfoRecord;
			if (-id == 620)
			{
				bamlTypeInfoRecord = new BamlTypeInfoWithSerializerRecord();
				((BamlTypeInfoWithSerializerRecord)bamlTypeInfoRecord).SerializerTypeId = -750;
				((BamlTypeInfoWithSerializerRecord)bamlTypeInfoRecord).SerializerType = KnownTypes.Types[750];
				bamlTypeInfoRecord.AssemblyId = -1;
			}
			else if (-id == 108 || -id == 120 || -id == 271 || -id == 330)
			{
				bamlTypeInfoRecord = new BamlTypeInfoWithSerializerRecord();
				((BamlTypeInfoWithSerializerRecord)bamlTypeInfoRecord).SerializerTypeId = -751;
				((BamlTypeInfoWithSerializerRecord)bamlTypeInfoRecord).SerializerType = KnownTypes.Types[751];
				bamlTypeInfoRecord.AssemblyId = -1;
			}
			else
			{
				bamlTypeInfoRecord = new BamlTypeInfoRecord();
				bamlTypeInfoRecord.AssemblyId = GetAssemblyIdForType(KnownTypes.Types[-id]);
			}
			bamlTypeInfoRecord.TypeId = id;
			bamlTypeInfoRecord.Type = KnownTypes.Types[-id];
			bamlTypeInfoRecord.TypeFullName = bamlTypeInfoRecord.Type.FullName;
			return bamlTypeInfoRecord;
		}
		return (BamlTypeInfoRecord)TypeIdMap[id];
	}

	private short GetAssemblyIdForType(Type t)
	{
		string fullName = t.Assembly.FullName;
		for (int i = 0; i < AssemblyIdMap.Count; i++)
		{
			if (((BamlAssemblyInfoRecord)AssemblyIdMap[i]).AssemblyFullName == fullName)
			{
				return (short)i;
			}
		}
		return -1;
	}

	internal TypeConverter GetConverterFromId(short typeId, Type propType, ParserContext pc)
	{
		TypeConverter typeConverter = null;
		if (typeId < 0)
		{
			switch ((KnownElements)(short)(-typeId))
			{
			case KnownElements.EnumConverter:
				typeConverter = GetConverterFromCache(propType);
				if (typeConverter == null)
				{
					typeConverter = new EnumConverter(propType);
					ConverterCache.Add(propType, typeConverter);
				}
				break;
			case KnownElements.NullableConverter:
				typeConverter = GetConverterFromCache(propType);
				if (typeConverter == null)
				{
					typeConverter = new NullableConverter(propType);
					ConverterCache.Add(propType, typeConverter);
				}
				break;
			default:
				typeConverter = GetConverterFromCache(typeId);
				if (typeConverter == null)
				{
					typeConverter = CreateKnownTypeFromId(typeId) as TypeConverter;
					ConverterCache.Add(typeId, typeConverter);
				}
				break;
			}
		}
		else
		{
			Type typeFromId = GetTypeFromId(typeId);
			typeConverter = GetConverterFromCache(typeFromId);
			if (typeConverter == null)
			{
				typeConverter = ((!ReflectionHelper.IsPublicType(typeFromId)) ? (XamlTypeMapper.CreateInternalInstance(pc, typeFromId) as TypeConverter) : (Activator.CreateInstance(typeFromId) as TypeConverter));
				if (typeConverter == null)
				{
					ThrowException("ParserNoTypeConv", propType.Name);
				}
				else
				{
					ConverterCache.Add(typeFromId, typeConverter);
				}
			}
		}
		return typeConverter;
	}

	internal string GetStringFromStringId(int id)
	{
		if (id < 0)
		{
			return _knownStrings[-id];
		}
		return ((BamlStringInfoRecord)StringIdMap[id]).Value;
	}

	internal BamlAttributeInfoRecord GetAttributeInfoFromId(short id)
	{
		if (id < 0)
		{
			KnownProperties knownProperties = (KnownProperties)(-id);
			BamlAttributeInfoRecord bamlAttributeInfoRecord = new BamlAttributeInfoRecord();
			bamlAttributeInfoRecord.AttributeId = id;
			bamlAttributeInfoRecord.OwnerTypeId = 0 - KnownTypes.GetKnownElementFromKnownCommonProperty(knownProperties);
			GetAttributeOwnerType(bamlAttributeInfoRecord);
			bamlAttributeInfoRecord.Name = GetAttributeNameFromKnownId(knownProperties);
			if (knownProperties < KnownProperties.MaxDependencyProperty)
			{
				DependencyProperty knownDependencyPropertyFromId = KnownTypes.GetKnownDependencyPropertyFromId(knownProperties);
				bamlAttributeInfoRecord.DP = knownDependencyPropertyFromId;
			}
			else
			{
				Type ownerType = bamlAttributeInfoRecord.OwnerType;
				bamlAttributeInfoRecord.PropInfo = ownerType.GetProperty(bamlAttributeInfoRecord.Name, BindingFlags.Instance | BindingFlags.Public);
			}
			return bamlAttributeInfoRecord;
		}
		return (BamlAttributeInfoRecord)AttributeIdMap[id];
	}

	internal BamlAttributeInfoRecord GetAttributeInfoFromIdWithOwnerType(short attributeId)
	{
		BamlAttributeInfoRecord attributeInfoFromId = GetAttributeInfoFromId(attributeId);
		GetAttributeOwnerType(attributeInfoFromId);
		return attributeInfoFromId;
	}

	private string GetAttributeNameFromKnownId(KnownProperties knownId)
	{
		if (knownId < KnownProperties.MaxDependencyProperty)
		{
			return KnownTypes.GetKnownDependencyPropertyFromId(knownId).Name;
		}
		return KnownTypes.GetKnownClrPropertyNameFromId(knownId);
	}

	internal string GetAttributeNameFromId(short id)
	{
		if (id < 0)
		{
			return GetAttributeNameFromKnownId((KnownProperties)(-id));
		}
		return ((BamlAttributeInfoRecord)AttributeIdMap[id])?.Name;
	}

	internal bool DoesAttributeMatch(short id, short ownerTypeId, string name)
	{
		if (id < 0)
		{
			KnownProperties knownProperties = (KnownProperties)(-id);
			string attributeNameFromKnownId = GetAttributeNameFromKnownId(knownProperties);
			KnownElements knownElementFromKnownCommonProperty = KnownTypes.GetKnownElementFromKnownCommonProperty(knownProperties);
			if (ownerTypeId == 0 - knownElementFromKnownCommonProperty)
			{
				return string.CompareOrdinal(attributeNameFromKnownId, name) == 0;
			}
			return false;
		}
		BamlAttributeInfoRecord bamlAttributeInfoRecord = (BamlAttributeInfoRecord)AttributeIdMap[id];
		if (bamlAttributeInfoRecord.OwnerTypeId == ownerTypeId)
		{
			return string.CompareOrdinal(bamlAttributeInfoRecord.Name, name) == 0;
		}
		return false;
	}

	internal bool DoesAttributeMatch(short id, string name)
	{
		string attributeNameFromId = GetAttributeNameFromId(id);
		if (attributeNameFromId == null)
		{
			return false;
		}
		return string.CompareOrdinal(attributeNameFromId, name) == 0;
	}

	internal bool DoesAttributeMatch(short id, BamlAttributeUsage attributeUsage)
	{
		if (id < 0)
		{
			return attributeUsage == GetAttributeUsageFromKnownAttribute((KnownProperties)(-id));
		}
		BamlAttributeInfoRecord bamlAttributeInfoRecord = (BamlAttributeInfoRecord)AttributeIdMap[id];
		return attributeUsage == bamlAttributeInfoRecord.AttributeUsage;
	}

	internal void GetAttributeInfoFromId(short id, out short ownerTypeId, out string name, out BamlAttributeUsage attributeUsage)
	{
		if (id < 0)
		{
			KnownProperties knownProperties = (KnownProperties)(-id);
			name = GetAttributeNameFromKnownId(knownProperties);
			ownerTypeId = 0 - KnownTypes.GetKnownElementFromKnownCommonProperty(knownProperties);
			attributeUsage = GetAttributeUsageFromKnownAttribute(knownProperties);
		}
		else
		{
			BamlAttributeInfoRecord bamlAttributeInfoRecord = (BamlAttributeInfoRecord)AttributeIdMap[id];
			name = bamlAttributeInfoRecord.Name;
			ownerTypeId = bamlAttributeInfoRecord.OwnerTypeId;
			attributeUsage = bamlAttributeInfoRecord.AttributeUsage;
		}
	}

	private static BamlAttributeUsage GetAttributeUsageFromKnownAttribute(KnownProperties knownId)
	{
		if (knownId == KnownProperties.FrameworkElement_Name)
		{
			return BamlAttributeUsage.RuntimeName;
		}
		return BamlAttributeUsage.Default;
	}

	internal Type GetTypeFromTypeInfo(BamlTypeInfoRecord typeInfo)
	{
		if (null == typeInfo.Type)
		{
			BamlAssemblyInfoRecord assemblyInfoFromId = GetAssemblyInfoFromId(typeInfo.AssemblyId);
			if (assemblyInfoFromId != null)
			{
				TypeInfoKey typeInfoKey = GetTypeInfoKey(assemblyInfoFromId.AssemblyFullName, typeInfo.TypeFullName);
				if (GetHashTableData(typeInfoKey) is BamlTypeInfoRecord bamlTypeInfoRecord && bamlTypeInfoRecord.Type != null)
				{
					typeInfo.Type = bamlTypeInfoRecord.Type;
				}
				else
				{
					Assembly assemblyFromAssemblyInfo = GetAssemblyFromAssemblyInfo(assemblyInfoFromId);
					if (null != assemblyFromAssemblyInfo)
					{
						Type type = assemblyFromAssemblyInfo.GetType(typeInfo.TypeFullName);
						typeInfo.Type = type;
						AddHashTableData(typeInfoKey, typeInfo);
					}
				}
			}
		}
		return typeInfo.Type;
	}

	private Type GetAttributeOwnerType(BamlAttributeInfoRecord bamlAttributeInfoRecord)
	{
		if (bamlAttributeInfoRecord.OwnerType == null)
		{
			if (bamlAttributeInfoRecord.OwnerTypeId < 0)
			{
				bamlAttributeInfoRecord.OwnerType = GetKnownTypeFromId(bamlAttributeInfoRecord.OwnerTypeId);
			}
			else
			{
				BamlTypeInfoRecord bamlTypeInfoRecord = (BamlTypeInfoRecord)TypeIdMap[bamlAttributeInfoRecord.OwnerTypeId];
				if (bamlTypeInfoRecord != null)
				{
					bamlAttributeInfoRecord.OwnerType = GetTypeFromTypeInfo(bamlTypeInfoRecord);
				}
			}
		}
		return bamlAttributeInfoRecord.OwnerType;
	}

	internal Type GetCLRPropertyTypeAndNameFromId(short attributeId, out string propName)
	{
		propName = null;
		Type type = null;
		BamlAttributeInfoRecord attributeInfoFromIdWithOwnerType = GetAttributeInfoFromIdWithOwnerType(attributeId);
		if (attributeInfoFromIdWithOwnerType != null && attributeInfoFromIdWithOwnerType.OwnerType != null)
		{
			XamlTypeMapper.UpdateClrPropertyInfo(attributeInfoFromIdWithOwnerType.OwnerType, attributeInfoFromIdWithOwnerType);
			type = attributeInfoFromIdWithOwnerType.GetPropertyType();
		}
		else
		{
			propName = string.Empty;
		}
		if (type == null)
		{
			if (propName == null)
			{
				propName = attributeInfoFromIdWithOwnerType.OwnerType.FullName + "." + attributeInfoFromIdWithOwnerType.Name;
			}
			ThrowException("ParserNoPropType", propName);
		}
		else
		{
			propName = attributeInfoFromIdWithOwnerType.Name;
		}
		return type;
	}

	internal DependencyProperty GetDependencyPropertyValueFromId(short memberId, string memberName, out Type declaringType)
	{
		declaringType = null;
		DependencyProperty result = null;
		if (memberName == null)
		{
			KnownProperties knownProperties = (KnownProperties)(-memberId);
			if (knownProperties < KnownProperties.MaxDependencyProperty || knownProperties == KnownProperties.Run_Text)
			{
				result = KnownTypes.GetKnownDependencyPropertyFromId(knownProperties);
			}
		}
		else
		{
			declaringType = GetTypeFromId(memberId);
			result = DependencyProperty.FromName(memberName, declaringType);
		}
		return result;
	}

	internal DependencyProperty GetDependencyPropertyValueFromId(short memberId)
	{
		DependencyProperty dependencyProperty = null;
		if (memberId < 0)
		{
			KnownProperties knownProperties = (KnownProperties)(-memberId);
			if (knownProperties < KnownProperties.MaxDependencyProperty)
			{
				dependencyProperty = KnownTypes.GetKnownDependencyPropertyFromId(knownProperties);
			}
		}
		if (dependencyProperty == null)
		{
			GetAttributeInfoFromId(memberId, out var ownerTypeId, out var name, out var _);
			Type typeFromId = GetTypeFromId(ownerTypeId);
			dependencyProperty = DependencyProperty.FromName(name, typeFromId);
		}
		return dependencyProperty;
	}

	internal DependencyProperty GetDependencyProperty(int id)
	{
		if (id < 0)
		{
			return KnownTypes.GetKnownDependencyPropertyFromId((KnownProperties)(-id));
		}
		BamlAttributeInfoRecord bamlAttributeInfoRecord = (BamlAttributeInfoRecord)AttributeIdMap[id];
		return GetDependencyProperty(bamlAttributeInfoRecord);
	}

	internal DependencyProperty GetDependencyProperty(BamlAttributeInfoRecord bamlAttributeInfoRecord)
	{
		if (bamlAttributeInfoRecord.DP == null && null == bamlAttributeInfoRecord.PropInfo)
		{
			GetAttributeOwnerType(bamlAttributeInfoRecord);
			if (null != bamlAttributeInfoRecord.OwnerType)
			{
				bamlAttributeInfoRecord.DP = DependencyProperty.FromName(bamlAttributeInfoRecord.Name, bamlAttributeInfoRecord.OwnerType);
			}
		}
		return bamlAttributeInfoRecord.DP;
	}

	internal RoutedEvent GetRoutedEvent(BamlAttributeInfoRecord bamlAttributeInfoRecord)
	{
		if (bamlAttributeInfoRecord.Event == null)
		{
			Type attributeOwnerType = GetAttributeOwnerType(bamlAttributeInfoRecord);
			if (null != attributeOwnerType)
			{
				bamlAttributeInfoRecord.Event = XamlTypeMapper.RoutedEventFromName(bamlAttributeInfoRecord.Name, attributeOwnerType);
			}
		}
		return bamlAttributeInfoRecord.Event;
	}

	internal short GetAttributeOrTypeId(BinaryWriter binaryWriter, Type declaringType, string memberName, out short typeId)
	{
		short result = 0;
		if (!GetTypeInfoId(binaryWriter, declaringType.Assembly.FullName, declaringType.FullName, out typeId))
		{
			typeId = AddTypeInfoMap(binaryWriter, declaringType.Assembly.FullName, declaringType.FullName, declaringType, string.Empty, string.Empty);
		}
		else if (typeId < 0)
		{
			result = (short)(-KnownTypes.GetKnownPropertyAttributeId((KnownElements)(-typeId), memberName));
		}
		return result;
	}

	internal BamlAssemblyInfoRecord GetAssemblyInfoFromId(short id)
	{
		if (id == -1)
		{
			return _knownAssemblyInfoRecord;
		}
		return (BamlAssemblyInfoRecord)AssemblyIdMap[id];
	}

	private Assembly GetAssemblyFromAssemblyInfo(BamlAssemblyInfoRecord assemblyInfoRecord)
	{
		if (null == assemblyInfoRecord.Assembly)
		{
			string assemblyPath = XamlTypeMapper.AssemblyPathFor(assemblyInfoRecord.AssemblyFullName);
			assemblyInfoRecord.Assembly = ReflectionHelper.LoadAssembly(assemblyInfoRecord.AssemblyFullName, assemblyPath);
		}
		return assemblyInfoRecord.Assembly;
	}

	internal BamlAssemblyInfoRecord AddAssemblyMap(BinaryWriter binaryWriter, string assemblyFullName)
	{
		AssemblyInfoKey assemblyInfoKey = default(AssemblyInfoKey);
		assemblyInfoKey.AssemblyFullName = assemblyFullName;
		BamlAssemblyInfoRecord bamlAssemblyInfoRecord = (BamlAssemblyInfoRecord)GetHashTableData(assemblyInfoKey);
		if (bamlAssemblyInfoRecord == null)
		{
			bamlAssemblyInfoRecord = new BamlAssemblyInfoRecord();
			bamlAssemblyInfoRecord.AssemblyFullName = assemblyFullName;
			bamlAssemblyInfoRecord.AssemblyId = (short)AssemblyIdMap.Add(bamlAssemblyInfoRecord);
			ObjectHashTable.Add(assemblyInfoKey, bamlAssemblyInfoRecord);
			bamlAssemblyInfoRecord.Write(binaryWriter);
		}
		else if (bamlAssemblyInfoRecord.AssemblyId == -1)
		{
			bamlAssemblyInfoRecord.AssemblyId = (short)AssemblyIdMap.Add(bamlAssemblyInfoRecord);
			bamlAssemblyInfoRecord.Write(binaryWriter);
		}
		return bamlAssemblyInfoRecord;
	}

	internal void LoadAssemblyInfoRecord(BamlAssemblyInfoRecord record)
	{
		if (AssemblyIdMap.Count == record.AssemblyId)
		{
			AssemblyIdMap.Add(record);
		}
	}

	internal void EnsureAssemblyRecord(Assembly asm)
	{
		string fullName = asm.FullName;
		BamlAssemblyInfoRecord bamlAssemblyInfoRecord = ObjectHashTable[fullName] as BamlAssemblyInfoRecord;
		if (bamlAssemblyInfoRecord == null)
		{
			bamlAssemblyInfoRecord = new BamlAssemblyInfoRecord();
			bamlAssemblyInfoRecord.AssemblyFullName = fullName;
			bamlAssemblyInfoRecord.Assembly = asm;
			ObjectHashTable[fullName] = bamlAssemblyInfoRecord;
		}
	}

	private TypeInfoKey GetTypeInfoKey(string assemblyFullName, string typeFullName)
	{
		TypeInfoKey result = default(TypeInfoKey);
		result.DeclaringAssembly = assemblyFullName;
		result.TypeFullName = typeFullName;
		return result;
	}

	internal bool GetTypeInfoId(BinaryWriter binaryWriter, string assemblyFullName, string typeFullName, out short typeId)
	{
		int num = typeFullName.LastIndexOf('.');
		string typeShortName;
		string clrNamespace;
		if (num >= 0)
		{
			typeShortName = typeFullName.Substring(num + 1);
			clrNamespace = typeFullName.Substring(0, num);
		}
		else
		{
			typeShortName = typeFullName;
			clrNamespace = string.Empty;
		}
		typeId = GetKnownTypeIdFromName(assemblyFullName, clrNamespace, typeShortName);
		if (typeId < 0)
		{
			return true;
		}
		TypeInfoKey typeInfoKey = GetTypeInfoKey(assemblyFullName, typeFullName);
		BamlTypeInfoRecord bamlTypeInfoRecord = (BamlTypeInfoRecord)GetHashTableData(typeInfoKey);
		if (bamlTypeInfoRecord == null)
		{
			return false;
		}
		typeId = bamlTypeInfoRecord.TypeId;
		return true;
	}

	internal short AddTypeInfoMap(BinaryWriter binaryWriter, string assemblyFullName, string typeFullName, Type elementType, string serializerAssemblyFullName, string serializerTypeFullName)
	{
		TypeInfoKey typeInfoKey = GetTypeInfoKey(assemblyFullName, typeFullName);
		BamlTypeInfoRecord bamlTypeInfoRecord;
		if (serializerTypeFullName == string.Empty)
		{
			bamlTypeInfoRecord = new BamlTypeInfoRecord();
		}
		else
		{
			bamlTypeInfoRecord = new BamlTypeInfoWithSerializerRecord();
			if (!GetTypeInfoId(binaryWriter, serializerAssemblyFullName, serializerTypeFullName, out var typeId))
			{
				typeId = AddTypeInfoMap(binaryWriter, serializerAssemblyFullName, serializerTypeFullName, null, string.Empty, string.Empty);
			}
			((BamlTypeInfoWithSerializerRecord)bamlTypeInfoRecord).SerializerTypeId = typeId;
		}
		bamlTypeInfoRecord.TypeFullName = typeFullName;
		BamlAssemblyInfoRecord bamlAssemblyInfoRecord = AddAssemblyMap(binaryWriter, assemblyFullName);
		bamlTypeInfoRecord.AssemblyId = bamlAssemblyInfoRecord.AssemblyId;
		bamlTypeInfoRecord.IsInternalType = elementType != null && ReflectionHelper.IsInternalType(elementType);
		bamlTypeInfoRecord.TypeId = (short)TypeIdMap.Add(bamlTypeInfoRecord);
		ObjectHashTable.Add(typeInfoKey, bamlTypeInfoRecord);
		bamlTypeInfoRecord.Write(binaryWriter);
		return bamlTypeInfoRecord.TypeId;
	}

	internal void LoadTypeInfoRecord(BamlTypeInfoRecord record)
	{
		if (TypeIdMap.Count == record.TypeId)
		{
			TypeIdMap.Add(record);
		}
	}

	internal object GetAttributeInfoKey(string ownerTypeName, string attributeName)
	{
		return ownerTypeName + "." + attributeName;
	}

	internal short AddAttributeInfoMap(BinaryWriter binaryWriter, string assemblyFullName, string typeFullName, Type owningType, string fieldName, Type attributeType, BamlAttributeUsage attributeUsage)
	{
		BamlAttributeInfoRecord bamlAttributeInfoRecord;
		return AddAttributeInfoMap(binaryWriter, assemblyFullName, typeFullName, owningType, fieldName, attributeType, attributeUsage, out bamlAttributeInfoRecord);
	}

	internal short AddAttributeInfoMap(BinaryWriter binaryWriter, string assemblyFullName, string typeFullName, Type owningType, string fieldName, Type attributeType, BamlAttributeUsage attributeUsage, out BamlAttributeInfoRecord bamlAttributeInfoRecord)
	{
		if (!GetTypeInfoId(binaryWriter, assemblyFullName, typeFullName, out var typeId))
		{
			Type xamlSerializerForType = XamlTypeMapper.GetXamlSerializerForType(owningType);
			string serializerAssemblyFullName = ((xamlSerializerForType == null) ? string.Empty : xamlSerializerForType.Assembly.FullName);
			string serializerTypeFullName = ((xamlSerializerForType == null) ? string.Empty : xamlSerializerForType.FullName);
			typeId = AddTypeInfoMap(binaryWriter, assemblyFullName, typeFullName, owningType, serializerAssemblyFullName, serializerTypeFullName);
		}
		else if (typeId < 0)
		{
			short num = (short)(-KnownTypes.GetKnownPropertyAttributeId((KnownElements)(-typeId), fieldName));
			if (num < 0)
			{
				bamlAttributeInfoRecord = null;
				return num;
			}
		}
		object attributeInfoKey = GetAttributeInfoKey(typeFullName, fieldName);
		bamlAttributeInfoRecord = (BamlAttributeInfoRecord)GetHashTableData(attributeInfoKey);
		if (bamlAttributeInfoRecord == null)
		{
			bamlAttributeInfoRecord = new BamlAttributeInfoRecord();
			bamlAttributeInfoRecord.Name = fieldName;
			bamlAttributeInfoRecord.OwnerTypeId = typeId;
			bamlAttributeInfoRecord.AttributeId = (short)AttributeIdMap.Add(bamlAttributeInfoRecord);
			bamlAttributeInfoRecord.AttributeUsage = attributeUsage;
			ObjectHashTable.Add(attributeInfoKey, bamlAttributeInfoRecord);
			bamlAttributeInfoRecord.Write(binaryWriter);
		}
		return bamlAttributeInfoRecord.AttributeId;
	}

	internal bool GetCustomSerializerOrConverter(BinaryWriter binaryWriter, Type ownerType, Type attributeType, object piOrMi, string fieldName, out short converterOrSerializerTypeId, out Type converterOrSerializerType)
	{
		converterOrSerializerType = null;
		converterOrSerializerTypeId = 0;
		if (!ShouldBypassCustomCheck(ownerType, attributeType))
		{
			converterOrSerializerType = GetCustomSerializer(attributeType, out converterOrSerializerTypeId);
			if (converterOrSerializerType != null)
			{
				return true;
			}
			converterOrSerializerType = GetCustomConverter(piOrMi, ownerType, fieldName, attributeType);
			if (converterOrSerializerType == null && attributeType.IsEnum)
			{
				converterOrSerializerTypeId = 195;
				converterOrSerializerType = KnownTypes.Types[converterOrSerializerTypeId];
				return true;
			}
			if (converterOrSerializerType != null)
			{
				string fullName = converterOrSerializerType.FullName;
				EnsureAssemblyRecord(converterOrSerializerType.Assembly);
				if (!GetTypeInfoId(binaryWriter, converterOrSerializerType.Assembly.FullName, fullName, out converterOrSerializerTypeId))
				{
					converterOrSerializerTypeId = AddTypeInfoMap(binaryWriter, converterOrSerializerType.Assembly.FullName, fullName, null, string.Empty, string.Empty);
				}
			}
		}
		return false;
	}

	internal bool GetStringInfoId(string stringValue, out short stringId)
	{
		stringId = GetKnownStringIdFromName(stringValue);
		if (stringId < 0)
		{
			return true;
		}
		BamlStringInfoRecord bamlStringInfoRecord = (BamlStringInfoRecord)GetHashTableData(stringValue);
		if (bamlStringInfoRecord == null)
		{
			return false;
		}
		stringId = bamlStringInfoRecord.StringId;
		return true;
	}

	internal short AddStringInfoMap(BinaryWriter binaryWriter, string stringValue)
	{
		BamlStringInfoRecord bamlStringInfoRecord = new BamlStringInfoRecord();
		bamlStringInfoRecord.StringId = (short)StringIdMap.Add(bamlStringInfoRecord);
		bamlStringInfoRecord.Value = stringValue;
		ObjectHashTable.Add(stringValue, bamlStringInfoRecord);
		bamlStringInfoRecord.Write(binaryWriter);
		return bamlStringInfoRecord.StringId;
	}

	internal short GetStaticMemberId(BinaryWriter binaryWriter, ParserContext pc, short extensionTypeId, string memberValue, Type defaultTargetType)
	{
		short num = 0;
		Type type = null;
		string memberName = null;
		switch (extensionTypeId)
		{
		case 602:
			type = XamlTypeMapper.GetTargetTypeAndMember(memberValue, pc, isTypeExpected: true, out memberName);
			if (XamlTypeMapper.GetStaticMemberInfo(type, memberName, fieldInfoOnly: false) is PropertyInfo)
			{
				num = SystemResourceKey.GetBamlIdBasedOnSystemResourceKeyId(type, memberName);
			}
			break;
		case 634:
			type = XamlTypeMapper.GetDependencyPropertyOwnerAndName(memberValue, pc, defaultTargetType, out memberName);
			break;
		}
		if (num == 0)
		{
			num = AddAttributeInfoMap(binaryWriter, type.Assembly.FullName, type.FullName, type, memberName, null, BamlAttributeUsage.Default);
		}
		return num;
	}

	private bool ShouldBypassCustomCheck(Type declaringType, Type attributeType)
	{
		if (declaringType == null)
		{
			return true;
		}
		if (attributeType == null)
		{
			return true;
		}
		return false;
	}

	private Type GetCustomConverter(object piOrMi, Type ownerType, string fieldName, Type attributeType)
	{
		Type knownConverterTypeFromPropName = GetKnownConverterTypeFromPropName(ownerType, fieldName);
		if (knownConverterTypeFromPropName != null)
		{
			return knownConverterTypeFromPropName;
		}
		Assembly assembly = ownerType.Assembly;
		if (!assembly.FullName.StartsWith("PresentationFramework", StringComparison.OrdinalIgnoreCase) && !assembly.FullName.StartsWith("PresentationCore", StringComparison.OrdinalIgnoreCase) && !assembly.FullName.StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase))
		{
			knownConverterTypeFromPropName = XamlTypeMapper.GetPropertyConverterType(attributeType, piOrMi);
			if (knownConverterTypeFromPropName != null)
			{
				return knownConverterTypeFromPropName;
			}
		}
		return XamlTypeMapper.GetTypeConverterType(attributeType);
	}

	private Type GetCustomSerializer(Type type, out short converterOrSerializerTypeId)
	{
		int num;
		if (type == typeof(bool))
		{
			num = 46;
		}
		else if (type == KnownTypes.Types[136])
		{
			num = 137;
		}
		else
		{
			num = XamlTypeMapper.GetCustomBamlSerializerIdForType(type);
			if (num == 0)
			{
				converterOrSerializerTypeId = 0;
				return null;
			}
		}
		converterOrSerializerTypeId = (short)num;
		return KnownTypes.Types[num];
	}

	private void ThrowException(string id, string parameter)
	{
		throw new ApplicationException(SR.Format(SR.GetResourceString(id), parameter));
	}

	internal void LoadAttributeInfoRecord(BamlAttributeInfoRecord record)
	{
		if (AttributeIdMap.Count == record.AttributeId)
		{
			AttributeIdMap.Add(record);
		}
	}

	internal void LoadStringInfoRecord(BamlStringInfoRecord record)
	{
		if (StringIdMap.Count == record.StringId)
		{
			StringIdMap.Add(record);
		}
	}

	internal object GetHashTableData(object key)
	{
		return ObjectHashTable[key];
	}

	internal void AddHashTableData(object key, object data)
	{
		if (_reusingMapTable)
		{
			ObjectHashTable[key] = data;
		}
	}

	internal BamlMapTable Clone()
	{
		return new BamlMapTable(_xamlTypeMapper)
		{
			_objectHashTable = (Hashtable)_objectHashTable.Clone(),
			_assemblyIdToInfo = (ArrayList)_assemblyIdToInfo.Clone(),
			_typeIdToInfo = (ArrayList)_typeIdToInfo.Clone(),
			_attributeIdToInfo = (ArrayList)_attributeIdToInfo.Clone(),
			_stringIdToInfo = (ArrayList)_stringIdToInfo.Clone()
		};
	}

	private TypeConverter GetConverterFromCache(short typeId)
	{
		TypeConverter result = null;
		if (_converterCache != null)
		{
			result = _converterCache[typeId] as TypeConverter;
		}
		return result;
	}

	private TypeConverter GetConverterFromCache(Type type)
	{
		TypeConverter result = null;
		if (_converterCache != null)
		{
			result = _converterCache[type] as TypeConverter;
		}
		return result;
	}

	internal void ClearConverterCache()
	{
		if (_converterCache != null)
		{
			_converterCache.Clear();
			_converterCache = null;
		}
	}
}
