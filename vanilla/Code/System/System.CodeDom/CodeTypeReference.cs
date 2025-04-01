using System.Collections.Generic;
using System.Globalization;

namespace System.CodeDom;

/// <summary>Represents a reference to a type.</summary>
[Serializable]
public class CodeTypeReference : CodeObject
{
	private string _baseType;

	private readonly bool _isInterface;

	private CodeTypeReferenceCollection _typeArguments;

	private bool _needsFixup;

	/// <summary>Gets or sets the type of the elements in the array.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type of the array elements.</returns>
	public CodeTypeReference ArrayElementType { get; set; }

	/// <summary>Gets or sets the array rank of the array.</summary>
	/// <returns>The number of dimensions of the array.</returns>
	public int ArrayRank { get; set; }

	internal int NestedArrayDepth
	{
		get
		{
			if (ArrayElementType != null)
			{
				return 1 + ArrayElementType.NestedArrayDepth;
			}
			return 0;
		}
	}

	/// <summary>Gets or sets the name of the type being referenced.</summary>
	/// <returns>The name of the type being referenced.</returns>
	public string BaseType
	{
		get
		{
			if (ArrayRank > 0 && ArrayElementType != null)
			{
				return ArrayElementType.BaseType;
			}
			if (string.IsNullOrEmpty(_baseType))
			{
				return string.Empty;
			}
			string baseType = _baseType;
			if (!_needsFixup || TypeArguments.Count <= 0)
			{
				return baseType;
			}
			return baseType + "`" + TypeArguments.Count.ToString(CultureInfo.InvariantCulture);
		}
		set
		{
			_baseType = value;
			Initialize(_baseType);
		}
	}

	/// <summary>Gets or sets the code type reference option.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.CodeDom.CodeTypeReferenceOptions" /> values. </returns>
	public CodeTypeReferenceOptions Options { get; set; }

	/// <summary>Gets the type arguments for the current generic type reference.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReferenceCollection" /> containing the type arguments for the current <see cref="T:System.CodeDom.CodeTypeReference" /> object.</returns>
	public CodeTypeReferenceCollection TypeArguments
	{
		get
		{
			if (ArrayRank > 0 && ArrayElementType != null)
			{
				return ArrayElementType.TypeArguments;
			}
			if (_typeArguments == null)
			{
				_typeArguments = new CodeTypeReferenceCollection();
			}
			return _typeArguments;
		}
	}

	internal bool IsInterface => _isInterface;

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class. </summary>
	public CodeTypeReference()
	{
		_baseType = string.Empty;
		ArrayRank = 0;
		ArrayElementType = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class using the specified type.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> to reference. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type " />is null.</exception>
	public CodeTypeReference(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (type.IsArray)
		{
			ArrayRank = type.GetArrayRank();
			ArrayElementType = new CodeTypeReference(type.GetElementType());
			_baseType = null;
		}
		else
		{
			InitializeFromType(type);
			ArrayRank = 0;
			ArrayElementType = null;
		}
		_isInterface = type.IsInterface;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class using the specified type and code type reference.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> to reference.</param>
	/// <param name="codeTypeReferenceOption">The code type reference option, one of the <see cref="T:System.CodeDom.CodeTypeReferenceOptions" /> values. </param>
	public CodeTypeReference(Type type, CodeTypeReferenceOptions codeTypeReferenceOption)
		: this(type)
	{
		Options = codeTypeReferenceOption;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class using the specified type name and code type reference option.</summary>
	/// <param name="typeName">The name of the type to reference.</param>
	/// <param name="codeTypeReferenceOption">The code type reference option, one of the <see cref="T:System.CodeDom.CodeTypeReferenceOptions" /> values.</param>
	public CodeTypeReference(string typeName, CodeTypeReferenceOptions codeTypeReferenceOption)
	{
		Initialize(typeName, codeTypeReferenceOption);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class using the specified type name.</summary>
	/// <param name="typeName">The name of the type to reference. </param>
	public CodeTypeReference(string typeName)
	{
		Initialize(typeName);
	}

	private void InitializeFromType(Type type)
	{
		_baseType = type.Name;
		if (!type.IsGenericParameter)
		{
			Type type2 = type;
			while (type2.IsNested)
			{
				type2 = type2.DeclaringType;
				_baseType = type2.Name + "+" + _baseType;
			}
			if (!string.IsNullOrEmpty(type.Namespace))
			{
				_baseType = type.Namespace + "." + _baseType;
			}
		}
		if (type.IsGenericType && !type.ContainsGenericParameters)
		{
			Type[] genericArguments = type.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				TypeArguments.Add(new CodeTypeReference(genericArguments[i]));
			}
		}
		else if (!type.IsGenericTypeDefinition)
		{
			_needsFixup = true;
		}
	}

	private void Initialize(string typeName)
	{
		Initialize(typeName, Options);
	}

	private void Initialize(string typeName, CodeTypeReferenceOptions options)
	{
		Options = options;
		if (string.IsNullOrEmpty(typeName))
		{
			typeName = typeof(void).FullName;
			_baseType = typeName;
			ArrayRank = 0;
			ArrayElementType = null;
			return;
		}
		typeName = RipOffAssemblyInformationFromTypeName(typeName);
		int num = typeName.Length - 1;
		int num2 = num;
		_needsFixup = true;
		Queue<int> queue = new Queue<int>();
		while (num2 >= 0)
		{
			int num3 = 1;
			if (typeName[num2--] != ']')
			{
				break;
			}
			while (num2 >= 0 && typeName[num2] == ',')
			{
				num3++;
				num2--;
			}
			if (num2 < 0 || typeName[num2] != '[')
			{
				break;
			}
			queue.Enqueue(num3);
			num2--;
			num = num2;
		}
		num2 = num;
		List<CodeTypeReference> list = new List<CodeTypeReference>();
		Stack<string> stack = new Stack<string>();
		if (num2 > 0 && typeName[num2--] == ']')
		{
			_needsFixup = false;
			int num4 = 1;
			int num5 = num;
			while (num2 >= 0)
			{
				if (typeName[num2] == '[')
				{
					if (--num4 == 0)
					{
						break;
					}
				}
				else if (typeName[num2] == ']')
				{
					num4++;
				}
				else if (typeName[num2] == ',' && num4 == 1)
				{
					if (num2 + 1 < num5)
					{
						stack.Push(typeName.Substring(num2 + 1, num5 - num2 - 1));
					}
					num5 = num2;
				}
				num2--;
			}
			if (num2 > 0 && num - num2 - 1 > 0)
			{
				if (num2 + 1 < num5)
				{
					stack.Push(typeName.Substring(num2 + 1, num5 - num2 - 1));
				}
				while (stack.Count > 0)
				{
					string typeName2 = RipOffAssemblyInformationFromTypeName(stack.Pop());
					list.Add(new CodeTypeReference(typeName2));
				}
				num = num2 - 1;
			}
		}
		if (num < 0)
		{
			_baseType = typeName;
			return;
		}
		if (queue.Count > 0)
		{
			CodeTypeReference codeTypeReference = new CodeTypeReference(typeName.Substring(0, num + 1), Options);
			for (int i = 0; i < list.Count; i++)
			{
				codeTypeReference.TypeArguments.Add(list[i]);
			}
			while (queue.Count > 1)
			{
				codeTypeReference = new CodeTypeReference(codeTypeReference, queue.Dequeue());
			}
			_baseType = null;
			ArrayRank = queue.Dequeue();
			ArrayElementType = codeTypeReference;
		}
		else if (list.Count > 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				TypeArguments.Add(list[j]);
			}
			_baseType = typeName.Substring(0, num + 1);
		}
		else
		{
			_baseType = typeName;
		}
		if (_baseType != null && _baseType.IndexOf('`') != -1)
		{
			_needsFixup = false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class using the specified type name and type arguments.</summary>
	/// <param name="typeName">The name of the type to reference.</param>
	/// <param name="typeArguments">An array of <see cref="T:System.CodeDom.CodeTypeReference" /> values.</param>
	public CodeTypeReference(string typeName, params CodeTypeReference[] typeArguments)
		: this(typeName)
	{
		if (typeArguments != null && typeArguments.Length != 0)
		{
			TypeArguments.AddRange(typeArguments);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class using the specified code type parameter. </summary>
	/// <param name="typeParameter">A <see cref="T:System.CodeDom.CodeTypeParameter" /> that represents the type of the type parameter.</param>
	public CodeTypeReference(CodeTypeParameter typeParameter)
		: this(typeParameter?.Name)
	{
		Options = CodeTypeReferenceOptions.GenericTypeParameter;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class using the specified array type name and rank.</summary>
	/// <param name="baseType">The name of the type of the elements of the array. </param>
	/// <param name="rank">The number of dimensions of the array. </param>
	public CodeTypeReference(string baseType, int rank)
	{
		_baseType = null;
		ArrayRank = rank;
		ArrayElementType = new CodeTypeReference(baseType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReference" /> class using the specified array type and rank.</summary>
	/// <param name="arrayType">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type of the array. </param>
	/// <param name="rank">The number of dimensions in the array. </param>
	public CodeTypeReference(CodeTypeReference arrayType, int rank)
	{
		_baseType = null;
		ArrayRank = rank;
		ArrayElementType = arrayType;
	}

	private string RipOffAssemblyInformationFromTypeName(string typeName)
	{
		int i = 0;
		int num = typeName.Length - 1;
		string result = typeName;
		for (; i < typeName.Length && char.IsWhiteSpace(typeName[i]); i++)
		{
		}
		while (num >= 0 && char.IsWhiteSpace(typeName[num]))
		{
			num--;
		}
		if (i < num)
		{
			if (typeName[i] == '[' && typeName[num] == ']')
			{
				i++;
				num--;
			}
			if (typeName[num] != ']')
			{
				int num2 = 0;
				for (int num3 = num; num3 >= i; num3--)
				{
					if (typeName[num3] == ',')
					{
						num2++;
						if (num2 == 4)
						{
							result = typeName.Substring(i, num3 - i);
							break;
						}
					}
				}
			}
		}
		return result;
	}
}
