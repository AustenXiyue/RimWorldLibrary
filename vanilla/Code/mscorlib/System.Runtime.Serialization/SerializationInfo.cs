using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security;

namespace System.Runtime.Serialization;

/// <summary>Stores all the data needed to serialize or deserialize an object. This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class SerializationInfo
{
	private const int defaultSize = 4;

	private const string s_mscorlibAssemblySimpleName = "mscorlib";

	private const string s_mscorlibFileName = "mscorlib.dll";

	internal string[] m_members;

	internal object[] m_data;

	internal Type[] m_types;

	private Dictionary<string, int> m_nameToIndex;

	internal int m_currMember;

	internal IFormatterConverter m_converter;

	private string m_fullTypeName;

	private string m_assemName;

	private Type objectType;

	private bool isFullTypeNameSetExplicit;

	private bool isAssemblyNameSetExplicit;

	private bool requireSameTokenInPartialTrust;

	/// <summary>Gets or sets the full name of the <see cref="T:System.Type" /> to serialize.</summary>
	/// <returns>The full name of the type to serialize.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value this property is set to is null. </exception>
	public string FullTypeName
	{
		get
		{
			return m_fullTypeName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_fullTypeName = value;
			isFullTypeNameSetExplicit = true;
		}
	}

	/// <summary>Gets or sets the assembly name of the type to serialize during serialization only.</summary>
	/// <returns>The full name of the assembly of the type to serialize.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value the property is set to is null. </exception>
	public string AssemblyName
	{
		get
		{
			return m_assemName;
		}
		[SecuritySafeCritical]
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (requireSameTokenInPartialTrust)
			{
				DemandForUnsafeAssemblyNameAssignments(m_assemName, value);
			}
			m_assemName = value;
			isAssemblyNameSetExplicit = true;
		}
	}

	/// <summary>Gets the number of members that have been added to the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The number of members that have been added to the current <see cref="T:System.Runtime.Serialization.SerializationInfo" />.</returns>
	public int MemberCount => m_currMember;

	/// <summary>Returns the type of the object to be serialized.</summary>
	/// <returns>The type of the object being serialized.</returns>
	public Type ObjectType => objectType;

	/// <summary>Gets whether the full type name has been explicitly set.</summary>
	/// <returns>True if the full type name has been explicitly set; otherwise false.</returns>
	public bool IsFullTypeNameSetExplicit => isFullTypeNameSetExplicit;

	/// <summary>Gets whether the assembly name has been explicitly set.</summary>
	/// <returns>True if the assembly name has been explicitly set; otherwise false.</returns>
	public bool IsAssemblyNameSetExplicit => isAssemblyNameSetExplicit;

	internal string[] MemberNames => m_members;

	internal object[] MemberValues => m_data;

	/// <summary>Creates a new instance of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> class.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the object to serialize. </param>
	/// <param name="converter">The <see cref="T:System.Runtime.Serialization.IFormatterConverter" /> used during deserialization. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="converter" /> is null. </exception>
	[CLSCompliant(false)]
	public SerializationInfo(Type type, IFormatterConverter converter)
		: this(type, converter, requireSameTokenInPartialTrust: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> class.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the object to serialize.</param>
	/// <param name="converter">The <see cref="T:System.Runtime.Serialization.IFormatterConverter" /> used during deserialization.</param>
	/// <param name="requireSameTokenInPartialTrust">Indicates whether the object requires same token in partial trust.</param>
	[CLSCompliant(false)]
	public SerializationInfo(Type type, IFormatterConverter converter, bool requireSameTokenInPartialTrust)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (converter == null)
		{
			throw new ArgumentNullException("converter");
		}
		objectType = type;
		m_fullTypeName = type.FullName;
		m_assemName = type.Module.Assembly.FullName;
		m_members = new string[4];
		m_data = new object[4];
		m_types = new Type[4];
		m_nameToIndex = new Dictionary<string, int>();
		m_converter = converter;
		this.requireSameTokenInPartialTrust = requireSameTokenInPartialTrust;
	}

	/// <summary>Sets the <see cref="T:System.Type" /> of the object to serialize.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the object to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> parameter is null. </exception>
	[SecuritySafeCritical]
	public void SetType(Type type)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (requireSameTokenInPartialTrust)
		{
			DemandForUnsafeAssemblyNameAssignments(ObjectType.Assembly.FullName, type.Assembly.FullName);
		}
		if ((object)objectType != type)
		{
			objectType = type;
			m_fullTypeName = type.FullName;
			m_assemName = type.Module.Assembly.FullName;
			isFullTypeNameSetExplicit = false;
			isAssemblyNameSetExplicit = false;
		}
	}

	private static bool Compare(byte[] a, byte[] b)
	{
		if (a == null || b == null || a.Length == 0 || b.Length == 0 || a.Length != b.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	[SecuritySafeCritical]
	internal static void DemandForUnsafeAssemblyNameAssignments(string originalAssemblyName, string newAssemblyName)
	{
		IsAssemblyNameAssignmentSafe(originalAssemblyName, newAssemblyName);
	}

	internal static bool IsAssemblyNameAssignmentSafe(string originalAssemblyName, string newAssemblyName)
	{
		if (originalAssemblyName == newAssemblyName)
		{
			return true;
		}
		AssemblyName assemblyName = new AssemblyName(originalAssemblyName);
		AssemblyName assemblyName2 = new AssemblyName(newAssemblyName);
		if (string.Equals(assemblyName2.Name, "mscorlib", StringComparison.OrdinalIgnoreCase) || string.Equals(assemblyName2.Name, "mscorlib.dll", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return Compare(assemblyName.GetPublicKeyToken(), assemblyName2.GetPublicKeyToken());
	}

	/// <summary>Returns a <see cref="T:System.Runtime.Serialization.SerializationInfoEnumerator" /> used to iterate through the name-value pairs in the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.SerializationInfoEnumerator" /> for parsing the name-value pairs contained in the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</returns>
	public SerializationInfoEnumerator GetEnumerator()
	{
		return new SerializationInfoEnumerator(m_members, m_data, m_types, m_currMember);
	}

	private void ExpandArrays()
	{
		int num = m_currMember * 2;
		if (num < m_currMember && int.MaxValue > m_currMember)
		{
			num = int.MaxValue;
		}
		string[] array = new string[num];
		object[] array2 = new object[num];
		Type[] array3 = new Type[num];
		Array.Copy(m_members, array, m_currMember);
		Array.Copy(m_data, array2, m_currMember);
		Array.Copy(m_types, array3, m_currMember);
		m_members = array;
		m_data = array2;
		m_types = array3;
	}

	/// <summary>Adds a value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store, where <paramref name="value" /> is associated with <paramref name="name" /> and is serialized as being of <see cref="T:System.Type" /><paramref name="type" />.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The value to be serialized. Any children of this object will automatically be serialized. </param>
	/// <param name="type">The <see cref="T:System.Type" /> to associate with the current object. This parameter must always be the type of the object itself or of one of its base classes. </param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="name" /> or <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, object value, Type type)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		AddValueInternal(name, value, type);
	}

	/// <summary>Adds the specified object into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store, where it is associated with a specified name.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The value to be serialized. Any children of this object will automatically be serialized. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, object value)
	{
		if (value == null)
		{
			AddValue(name, value, typeof(object));
		}
		else
		{
			AddValue(name, value, value.GetType());
		}
	}

	/// <summary>Adds a Boolean value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The Boolean value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, bool value)
	{
		AddValue(name, value, typeof(bool));
	}

	/// <summary>Adds a Unicode character value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The character value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, char value)
	{
		AddValue(name, value, typeof(char));
	}

	/// <summary>Adds an 8-bit signed integer value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The Sbyte value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	[CLSCompliant(false)]
	public void AddValue(string name, sbyte value)
	{
		AddValue(name, value, typeof(sbyte));
	}

	/// <summary>Adds an 8-bit unsigned integer value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The byte value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, byte value)
	{
		AddValue(name, value, typeof(byte));
	}

	/// <summary>Adds a 16-bit signed integer value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The Int16 value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, short value)
	{
		AddValue(name, value, typeof(short));
	}

	/// <summary>Adds a 16-bit unsigned integer value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The UInt16 value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	[CLSCompliant(false)]
	public void AddValue(string name, ushort value)
	{
		AddValue(name, value, typeof(ushort));
	}

	/// <summary>Adds a 32-bit signed integer value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The Int32 value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, int value)
	{
		AddValue(name, value, typeof(int));
	}

	/// <summary>Adds a 32-bit unsigned integer value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The UInt32 value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	[CLSCompliant(false)]
	public void AddValue(string name, uint value)
	{
		AddValue(name, value, typeof(uint));
	}

	/// <summary>Adds a 64-bit signed integer value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The Int64 value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, long value)
	{
		AddValue(name, value, typeof(long));
	}

	/// <summary>Adds a 64-bit unsigned integer value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The UInt64 value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	[CLSCompliant(false)]
	public void AddValue(string name, ulong value)
	{
		AddValue(name, value, typeof(ulong));
	}

	/// <summary>Adds a single-precision floating-point value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The single value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, float value)
	{
		AddValue(name, value, typeof(float));
	}

	/// <summary>Adds a double-precision floating-point value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The double value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, double value)
	{
		AddValue(name, value, typeof(double));
	}

	/// <summary>Adds a decimal value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The decimal value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">If The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">If a value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, decimal value)
	{
		AddValue(name, value, typeof(decimal));
	}

	/// <summary>Adds a <see cref="T:System.DateTime" /> value into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <param name="name">The name to associate with the value, so it can be deserialized later. </param>
	/// <param name="value">The <see cref="T:System.DateTime" /> value to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A value has already been associated with <paramref name="name" />. </exception>
	public void AddValue(string name, DateTime value)
	{
		AddValue(name, value, typeof(DateTime));
	}

	internal void AddValueInternal(string name, object value, Type type)
	{
		if (m_nameToIndex.ContainsKey(name))
		{
			throw new SerializationException(Environment.GetResourceString("Cannot add the same member twice to a SerializationInfo object."));
		}
		m_nameToIndex.Add(name, m_currMember);
		if (m_currMember >= m_members.Length)
		{
			ExpandArrays();
		}
		m_members[m_currMember] = name;
		m_data[m_currMember] = value;
		m_types[m_currMember] = type;
		m_currMember++;
	}

	internal void UpdateValue(string name, object value, Type type)
	{
		int num = FindElement(name);
		if (num < 0)
		{
			AddValueInternal(name, value, type);
			return;
		}
		m_data[num] = value;
		m_types[num] = type;
	}

	private int FindElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (m_nameToIndex.TryGetValue(name, out var value))
		{
			return value;
		}
		return -1;
	}

	private object GetElement(string name, out Type foundType)
	{
		int num = FindElement(name);
		if (num == -1)
		{
			throw new SerializationException(Environment.GetResourceString("Member '{0}' was not found.", name));
		}
		foundType = m_types[num];
		return m_data[num];
	}

	[ComVisible(true)]
	private object GetElementNoThrow(string name, out Type foundType)
	{
		int num = FindElement(name);
		if (num == -1)
		{
			foundType = null;
			return null;
		}
		foundType = m_types[num];
		return m_data[num];
	}

	/// <summary>Retrieves a value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The object of the specified <see cref="T:System.Type" /> associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <param name="type">The <see cref="T:System.Type" /> of the value to retrieve. If the stored value cannot be converted to this type, the system will throw a <see cref="T:System.InvalidCastException" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to <paramref name="type" />. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	[SecuritySafeCritical]
	public object GetValue(string name, Type type)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		RuntimeType runtimeType = type as RuntimeType;
		if (runtimeType == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a runtime Type object."));
		}
		Type foundType;
		object element = GetElement(name, out foundType);
		if (RemotingServices.IsTransparentProxy(element))
		{
			if (RemotingServices.ProxyCheckCast(RemotingServices.GetRealProxy(element), runtimeType))
			{
				return element;
			}
		}
		else if ((object)foundType == type || type.IsAssignableFrom(foundType) || element == null)
		{
			return element;
		}
		return m_converter.Convert(element, type);
	}

	[ComVisible(true)]
	[SecuritySafeCritical]
	internal object GetValueNoThrow(string name, Type type)
	{
		Type foundType;
		object elementNoThrow = GetElementNoThrow(name, out foundType);
		if (elementNoThrow == null)
		{
			return null;
		}
		if (RemotingServices.IsTransparentProxy(elementNoThrow))
		{
			if (RemotingServices.ProxyCheckCast(RemotingServices.GetRealProxy(elementNoThrow), (RuntimeType)type))
			{
				return elementNoThrow;
			}
		}
		else if ((object)foundType == type || type.IsAssignableFrom(foundType) || elementNoThrow == null)
		{
			return elementNoThrow;
		}
		return m_converter.Convert(elementNoThrow, type);
	}

	/// <summary>Retrieves a Boolean value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The Boolean value associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a Boolean value. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public bool GetBoolean(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(bool))
		{
			return (bool)element;
		}
		return m_converter.ToBoolean(element);
	}

	/// <summary>Retrieves a Unicode character value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The Unicode character associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a Unicode character. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public char GetChar(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(char))
		{
			return (char)element;
		}
		return m_converter.ToChar(element);
	}

	/// <summary>Retrieves an 8-bit signed integer value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The 8-bit signed integer associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to an 8-bit signed integer. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	[CLSCompliant(false)]
	public sbyte GetSByte(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(sbyte))
		{
			return (sbyte)element;
		}
		return m_converter.ToSByte(element);
	}

	/// <summary>Retrieves an 8-bit unsigned integer value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The 8-bit unsigned integer associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to an 8-bit unsigned integer. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public byte GetByte(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(byte))
		{
			return (byte)element;
		}
		return m_converter.ToByte(element);
	}

	/// <summary>Retrieves a 16-bit signed integer value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The 16-bit signed integer associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a 16-bit signed integer. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public short GetInt16(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(short))
		{
			return (short)element;
		}
		return m_converter.ToInt16(element);
	}

	/// <summary>Retrieves a 16-bit unsigned integer value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The 16-bit unsigned integer associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a 16-bit unsigned integer. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	[CLSCompliant(false)]
	public ushort GetUInt16(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(ushort))
		{
			return (ushort)element;
		}
		return m_converter.ToUInt16(element);
	}

	/// <summary>Retrieves a 32-bit signed integer value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The 32-bit signed integer associated with <paramref name="name" />.</returns>
	/// <param name="name">The name of the value to retrieve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a 32-bit signed integer. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public int GetInt32(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(int))
		{
			return (int)element;
		}
		return m_converter.ToInt32(element);
	}

	/// <summary>Retrieves a 32-bit unsigned integer value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The 32-bit unsigned integer associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a 32-bit unsigned integer. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	[CLSCompliant(false)]
	public uint GetUInt32(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(uint))
		{
			return (uint)element;
		}
		return m_converter.ToUInt32(element);
	}

	/// <summary>Retrieves a 64-bit signed integer value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The 64-bit signed integer associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a 64-bit signed integer. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public long GetInt64(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(long))
		{
			return (long)element;
		}
		return m_converter.ToInt64(element);
	}

	/// <summary>Retrieves a 64-bit unsigned integer value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The 64-bit unsigned integer associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a 64-bit unsigned integer. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	[CLSCompliant(false)]
	public ulong GetUInt64(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(ulong))
		{
			return (ulong)element;
		}
		return m_converter.ToUInt64(element);
	}

	/// <summary>Retrieves a single-precision floating-point value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The single-precision floating-point value associated with <paramref name="name" />.</returns>
	/// <param name="name">The name of the value to retrieve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a single-precision floating-point value. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public float GetSingle(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(float))
		{
			return (float)element;
		}
		return m_converter.ToSingle(element);
	}

	/// <summary>Retrieves a double-precision floating-point value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The double-precision floating-point value associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a double-precision floating-point value. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public double GetDouble(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(double))
		{
			return (double)element;
		}
		return m_converter.ToDouble(element);
	}

	/// <summary>Retrieves a decimal value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>A decimal value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.  </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a decimal. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public decimal GetDecimal(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(decimal))
		{
			return (decimal)element;
		}
		return m_converter.ToDecimal(element);
	}

	/// <summary>Retrieves a <see cref="T:System.DateTime" /> value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> value associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.  </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a <see cref="T:System.DateTime" /> value. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public DateTime GetDateTime(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(DateTime))
		{
			return (DateTime)element;
		}
		return m_converter.ToDateTime(element);
	}

	/// <summary>Retrieves a <see cref="T:System.String" /> value from the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> store.</summary>
	/// <returns>The <see cref="T:System.String" /> associated with <paramref name="name" />.</returns>
	/// <param name="name">The name associated with the value to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The value associated with <paramref name="name" /> cannot be converted to a <see cref="T:System.String" />. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An element with the specified name is not found in the current instance. </exception>
	public string GetString(string name)
	{
		Type foundType;
		object element = GetElement(name, out foundType);
		if ((object)foundType == typeof(string) || element == null)
		{
			return (string)element;
		}
		return m_converter.ToString(element);
	}
}
