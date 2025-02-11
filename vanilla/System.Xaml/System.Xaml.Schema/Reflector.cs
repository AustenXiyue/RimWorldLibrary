using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Markup;

namespace System.Xaml.Schema;

internal abstract class Reflector
{
	protected NullableReference<ICustomAttributeProvider> _attributeProvider;

	protected IList<CustomAttributeData> _attributeData;

	internal ICustomAttributeProvider CustomAttributeProvider
	{
		get
		{
			return _attributeProvider.Value;
		}
		set
		{
			_attributeProvider.Value = value;
		}
	}

	internal bool CustomAttributeProviderIsSet => _attributeProvider.IsSet;

	internal bool CustomAttributeProviderIsSetVolatile => _attributeProvider.IsSetVolatile;

	protected abstract MemberInfo Member { get; }

	internal void SetCustomAttributeProviderVolatile(ICustomAttributeProvider value)
	{
		_attributeProvider.SetVolatile(value);
	}

	public bool IsAttributePresent(Type attributeType)
	{
		if (CustomAttributeProvider != null)
		{
			return CustomAttributeProvider.IsDefined(attributeType, inherit: false);
		}
		try
		{
			return GetAttribute(attributeType) != null;
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return IsAttributePresent(attributeType);
		}
	}

	public string GetAttributeString(Type attributeType, out bool checkedInherited)
	{
		if (CustomAttributeProvider != null)
		{
			checkedInherited = true;
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: true);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (attributeType == typeof(ContentPropertyAttribute))
			{
				return ((ContentPropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(RuntimeNamePropertyAttribute))
			{
				return ((RuntimeNamePropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(DictionaryKeyPropertyAttribute))
			{
				return ((DictionaryKeyPropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(XamlSetMarkupExtensionAttribute))
			{
				return ((XamlSetMarkupExtensionAttribute)customAttributes[0]).XamlSetMarkupExtensionHandler;
			}
			if (attributeType == typeof(XamlSetTypeConverterAttribute))
			{
				return ((XamlSetTypeConverterAttribute)customAttributes[0]).XamlSetTypeConverterHandler;
			}
			if (attributeType == typeof(UidPropertyAttribute))
			{
				return ((UidPropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(XmlLangPropertyAttribute))
			{
				return ((XmlLangPropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(ConstructorArgumentAttribute))
			{
				return ((ConstructorArgumentAttribute)customAttributes[0]).ArgumentName;
			}
			return null;
		}
		try
		{
			checkedInherited = false;
			CustomAttributeData attribute = GetAttribute(attributeType);
			if (attribute == null)
			{
				return null;
			}
			return Extract<string>(attribute) ?? string.Empty;
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAttributeString(attributeType, out checkedInherited);
		}
	}

	public IReadOnlyDictionary<char, char> GetBracketCharacterAttributes(Type attributeType)
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (attributeType == typeof(MarkupExtensionBracketCharactersAttribute))
			{
				Dictionary<char, char> dictionary = new Dictionary<char, char>();
				object[] array = customAttributes;
				for (int i = 0; i < array.Length; i++)
				{
					MarkupExtensionBracketCharactersAttribute markupExtensionBracketCharactersAttribute = (MarkupExtensionBracketCharactersAttribute)array[i];
					dictionary.Add(markupExtensionBracketCharactersAttribute.OpeningBracket, markupExtensionBracketCharactersAttribute.ClosingBracket);
				}
				return new ReadOnlyDictionary<char, char>(dictionary);
			}
			return null;
		}
		if (attributeType == typeof(MarkupExtensionBracketCharactersAttribute))
		{
			return TokenizeBracketCharacters(attributeType);
		}
		return null;
	}

	public T? GetAttributeValue<T>(Type attributeType) where T : struct
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (attributeType == typeof(DesignerSerializationVisibilityAttribute))
			{
				return (T)(object)((DesignerSerializationVisibilityAttribute)customAttributes[0]).Visibility;
			}
			if (attributeType == typeof(UsableDuringInitializationAttribute))
			{
				return (T)(object)((UsableDuringInitializationAttribute)customAttributes[0]).Usable;
			}
			return null;
		}
		try
		{
			CustomAttributeData attribute = GetAttribute(attributeType);
			if (attribute == null)
			{
				return null;
			}
			return Extract<T>(attribute);
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAttributeValue<T>(attributeType);
		}
	}

	public Type GetAttributeType(Type attributeType)
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (attributeType == typeof(TypeConverterAttribute))
			{
				return Type.GetType(((TypeConverterAttribute)customAttributes[0]).ConverterTypeName);
			}
			if (attributeType == typeof(MarkupExtensionReturnTypeAttribute))
			{
				return ((MarkupExtensionReturnTypeAttribute)customAttributes[0]).ReturnType;
			}
			if (attributeType == typeof(ValueSerializerAttribute))
			{
				return ((ValueSerializerAttribute)customAttributes[0]).ValueSerializerType;
			}
			return null;
		}
		try
		{
			CustomAttributeData attribute = GetAttribute(attributeType);
			if (attribute == null)
			{
				return null;
			}
			return ExtractType(attribute);
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAttributeType(attributeType);
		}
	}

	public Type[] GetAttributeTypes(Type attributeType, int count)
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			XamlDeferLoadAttribute obj = (XamlDeferLoadAttribute)customAttributes[0];
			Type type = Type.GetType(obj.LoaderTypeName);
			Type type2 = Type.GetType(obj.ContentTypeName);
			return new Type[2] { type, type2 };
		}
		try
		{
			CustomAttributeData attribute = GetAttribute(attributeType);
			if (attribute == null)
			{
				return null;
			}
			return ExtractTypes(attribute, count);
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAttributeTypes(attributeType, count);
		}
	}

	public List<T> GetAllAttributeContents<T>(Type attributeType)
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			List<T> list = new List<T>();
			if (attributeType == typeof(ContentWrapperAttribute))
			{
				object[] array = customAttributes;
				for (int i = 0; i < array.Length; i++)
				{
					ContentWrapperAttribute contentWrapperAttribute = (ContentWrapperAttribute)array[i];
					list.Add((T)(object)contentWrapperAttribute.ContentWrapper);
				}
				return list;
			}
			if (attributeType == typeof(DependsOnAttribute))
			{
				object[] array = customAttributes;
				for (int i = 0; i < array.Length; i++)
				{
					DependsOnAttribute dependsOnAttribute = (DependsOnAttribute)array[i];
					list.Add((T)(object)dependsOnAttribute.Name);
				}
				return list;
			}
			return null;
		}
		try
		{
			List<CustomAttributeData> list2 = new List<CustomAttributeData>();
			GetAttributes(attributeType, list2);
			if (list2.Count == 0)
			{
				return null;
			}
			List<T> list3 = new List<T>();
			foreach (CustomAttributeData item in list2)
			{
				T val = Extract<T>(item);
				list3.Add((T)(object)val);
			}
			return list3;
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAllAttributeContents<T>(attributeType);
		}
	}

	protected static bool? GetFlag(int bitMask, int bitToCheck)
	{
		int validMask = GetValidMask(bitToCheck);
		if ((bitMask & validMask) != 0)
		{
			return (bitMask & bitToCheck) != 0;
		}
		return null;
	}

	protected static int GetValidMask(int flagMask)
	{
		return flagMask << 16;
	}

	protected static void SetFlag(ref int bitMask, int bitToSet, bool value)
	{
		int mask = GetValidMask(bitToSet) + (value ? bitToSet : 0);
		SetBit(ref bitMask, mask);
	}

	protected static void SetBit(ref int flags, int mask)
	{
		int num;
		int value;
		do
		{
			num = flags;
			value = num | mask;
		}
		while (num != Interlocked.CompareExchange(ref flags, value, num));
	}

	private static bool TypesAreEqual(Type userType, Type builtInType)
	{
		if (userType.Assembly.ReflectionOnly)
		{
			return LooseTypeExtensions.AssemblyQualifiedNameEquals(userType, builtInType);
		}
		return userType == builtInType;
	}

	private ReadOnlyDictionary<char, char> TokenizeBracketCharacters(Type attributeType)
	{
		if (attributeType == typeof(MarkupExtensionBracketCharactersAttribute))
		{
			IList<CustomAttributeData> list = new List<CustomAttributeData>();
			GetAttributes(attributeType, list);
			Dictionary<char, char> dictionary = new Dictionary<char, char>();
			foreach (CustomAttributeData item in list)
			{
				char key = (char)item.ConstructorArguments[0].Value;
				char value = (char)item.ConstructorArguments[1].Value;
				dictionary.Add(key, value);
			}
			return new ReadOnlyDictionary<char, char>(dictionary);
		}
		return null;
	}

	private Type ExtractType(CustomAttributeData cad)
	{
		Type type = null;
		if (cad.ConstructorArguments.Count == 1)
		{
			type = ExtractType(cad.ConstructorArguments[0]);
		}
		if (type == null)
		{
			ThrowInvalidMetadata(cad, 1, typeof(Type));
		}
		return type;
	}

	private Type[] ExtractTypes(CustomAttributeData cad, int count)
	{
		if (cad.ConstructorArguments.Count != count)
		{
			ThrowInvalidMetadata(cad, count, typeof(Type));
		}
		Type[] array = new Type[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = ExtractType(cad.ConstructorArguments[i]);
			if (array[i] == null)
			{
				ThrowInvalidMetadata(cad, count, typeof(Type));
			}
		}
		return array;
	}

	private Type ExtractType(CustomAttributeTypedArgument arg)
	{
		if (arg.ArgumentType == typeof(Type))
		{
			return (Type)arg.Value;
		}
		if (arg.ArgumentType == typeof(string))
		{
			return Type.GetType((string)arg.Value);
		}
		return null;
	}

	private T Extract<T>(CustomAttributeData cad)
	{
		if (cad.ConstructorArguments.Count == 0)
		{
			return default(T);
		}
		if (cad.ConstructorArguments.Count > 1 || !TypesAreEqual(cad.ConstructorArguments[0].ArgumentType, typeof(T)))
		{
			ThrowInvalidMetadata(cad, 1, typeof(T));
		}
		return (T)cad.ConstructorArguments[0].Value;
	}

	protected void EnsureAttributeData()
	{
		if (_attributeData == null)
		{
			_attributeData = CustomAttributeData.GetCustomAttributes(Member);
		}
	}

	private CustomAttributeData GetAttribute(Type attributeType)
	{
		EnsureAttributeData();
		for (int i = 0; i < _attributeData.Count; i++)
		{
			if (TypesAreEqual(_attributeData[i].Constructor.DeclaringType, attributeType))
			{
				return _attributeData[i];
			}
		}
		return null;
	}

	private void GetAttributes(Type attributeType, IList<CustomAttributeData> cads)
	{
		EnsureAttributeData();
		for (int i = 0; i < _attributeData.Count; i++)
		{
			if (TypesAreEqual(_attributeData[i].Constructor.DeclaringType, attributeType))
			{
				cads.Add(_attributeData[i]);
			}
		}
	}

	protected void ThrowInvalidMetadata(CustomAttributeData cad, int expectedCount, Type expectedType)
	{
		throw new XamlSchemaException(System.SR.Format(System.SR.UnexpectedConstructorArg, cad.Constructor.DeclaringType, Member, expectedCount, expectedType));
	}
}
