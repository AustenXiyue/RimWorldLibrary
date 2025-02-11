using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MS.Internal.Xaml.Parser;

namespace System.Xaml.Schema;

/// <summary>Provides a means to specify a XAML type in terms of name and namespace.</summary>
[DebuggerDisplay("{ToString()}")]
public class XamlTypeName
{
	private List<XamlTypeName> _typeArguments;

	/// <summary>Gets the name used to construct this <see cref="T:System.Xaml.Schema.XamlTypeName" />.</summary>
	/// <returns>The name of the type.</returns>
	public string Name { get; set; }

	/// <summary>Gets the XAML namespace identifier used to construct this <see cref="T:System.Xaml.Schema.XamlTypeName" />.</summary>
	/// <returns>The XAML namespace identifier.</returns>
	public string Namespace { get; set; }

	/// <summary>Gets the type arguments used to construct this <see cref="T:System.Xaml.Schema.XamlTypeName" />.</summary>
	/// <returns>The type arguments, if any. May be null.</returns>
	public IList<XamlTypeName> TypeArguments
	{
		get
		{
			if (_typeArguments == null)
			{
				_typeArguments = new List<XamlTypeName>();
			}
			return _typeArguments;
		}
	}

	internal List<XamlTypeName> TypeArgumentsList => _typeArguments;

	internal bool HasTypeArgs
	{
		get
		{
			if (_typeArguments != null)
			{
				return _typeArguments.Count > 0;
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlTypeName" /> class.</summary>
	public XamlTypeName()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlTypeName" /> class, based on name and namespace strings.</summary>
	/// <param name="xamlNamespace">The XAML namespace that contains name.</param>
	/// <param name="name">The name of the type to create a <see cref="T:System.Xaml.Schema.XamlTypeName" /> for.</param>
	public XamlTypeName(string xamlNamespace, string name)
		: this(xamlNamespace, name, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlTypeName" /> class, based on name and namespace strings as well as an array of type arguments.</summary>
	/// <param name="xamlNamespace">The XAML namespace that contains <paramref name="name" />.</param>
	/// <param name="name">The name of the type to create a <see cref="T:System.Xaml.Schema.XamlTypeName" /> for.</param>
	/// <param name="typeArguments">An array of type arguments, each of which must be a <see cref="T:System.Xaml.Schema.XamlTypeName" />.</param>
	public XamlTypeName(string xamlNamespace, string name, IEnumerable<XamlTypeName> typeArguments)
	{
		Name = name;
		Namespace = xamlNamespace;
		if (typeArguments != null)
		{
			List<XamlTypeName> typeArguments2 = new List<XamlTypeName>(typeArguments);
			_typeArguments = typeArguments2;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlTypeName" /> class, based on an existing <see cref="T:System.Xaml.XamlType" />.</summary>
	/// <param name="xamlType">An existing <see cref="T:System.Xaml.XamlType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xamlType" /> is null.</exception>
	public XamlTypeName(XamlType xamlType)
	{
		ArgumentNullException.ThrowIfNull(xamlType, "xamlType");
		Name = xamlType.Name;
		Namespace = xamlType.GetXamlNamespaces()[0];
		if (xamlType.TypeArguments == null)
		{
			return;
		}
		foreach (XamlType typeArgument in xamlType.TypeArguments)
		{
			TypeArguments.Add(new XamlTypeName(typeArgument));
		}
	}

	/// <summary>Converts the value of this <see cref="T:System.Xaml.Schema.XamlTypeName" /> to its equivalent string representation.</summary>
	/// <returns>The equivalent string representation of this <see cref="T:System.Xaml.Schema.XamlTypeName" /> .</returns>
	public override string ToString()
	{
		return ToString(null);
	}

	/// <summary>Converts the value of this <see cref="T:System.Xaml.Schema.XamlTypeName" /> to its equivalent string representation, which can be used in markup syntax for an object element usage of a type.</summary>
	/// <returns>A prefixed usage string.</returns>
	/// <param name="prefixLookup">A service reference for prefix lookup.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Xaml.Schema.XamlTypeName.Namespace" /> for this <see cref="T:System.Xaml.Schema.XamlTypeName" /> is null.-or-<see cref="P:System.Xaml.Schema.XamlTypeName.Name" /> is not valid.-or-Could not look up the prefix.</exception>
	public string ToString(INamespacePrefixLookup prefixLookup)
	{
		if (prefixLookup == null)
		{
			return ConvertToStringInternal(null);
		}
		return ConvertToStringInternal(prefixLookup.LookupPrefix);
	}

	/// <summary>Converts the value of this <see cref="T:System.Xaml.Schema.XamlTypeName" /> to its equivalent string representation, which can be used in markup syntax for an object element usage of multiple types.</summary>
	/// <returns>A concatenated string of all type results.</returns>
	/// <param name="typeNameList">A list of types.</param>
	/// <param name="prefixLookup">A service reference for prefix lookup.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="prefixLookup" /> or <paramref name="typeNameList" /> is null.</exception>
	public static string ToString(IList<XamlTypeName> typeNameList, INamespacePrefixLookup prefixLookup)
	{
		ArgumentNullException.ThrowIfNull(typeNameList, "typeNameList");
		ArgumentNullException.ThrowIfNull(prefixLookup, "prefixLookup");
		return ConvertListToStringInternal(typeNameList, prefixLookup.LookupPrefix);
	}

	/// <summary>Provides a <see cref="T:System.Xaml.Schema.XamlTypeName" /> value based on a type name and an object that can resolve a markup prefix into a namespace.</summary>
	/// <returns>The created <see cref="T:System.Xaml.Schema.XamlTypeName" />.</returns>
	/// <param name="typeName">The type name to create a <see cref="T:System.Xaml.Schema.XamlTypeName" /> value for.</param>
	/// <param name="namespaceResolver">An object or service provider that implements <see cref="T:System.Xaml.IXamlNamespaceResolver" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> or <paramref name="namespaceResolver" /> is null.</exception>
	/// <exception cref="T:System.FormatException">String cannot be parsed.</exception>
	public static XamlTypeName Parse(string typeName, IXamlNamespaceResolver namespaceResolver)
	{
		ArgumentNullException.ThrowIfNull(typeName, "typeName");
		ArgumentNullException.ThrowIfNull(namespaceResolver, "namespaceResolver");
		string error;
		return ParseInternal(typeName, namespaceResolver.GetNamespace, out error) ?? throw new FormatException(error);
	}

	/// <summary>Provides a <see cref="T:System.Xaml.Schema.XamlTypeName" /> value based on a string that can specify multiple type names, and an object that can resolve a markup prefix into a namespace.</summary>
	/// <returns>The created <see cref="T:System.Xaml.Schema.XamlTypeName" />.</returns>
	/// <param name="typeNameList">A string that contains multiple types. See Remarks. </param>
	/// <param name="namespaceResolver">An object or service provider that implements <see cref="T:System.Xaml.IXamlNamespaceResolver" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeNameList" /> or <paramref name="namespaceResolver" /> is null.</exception>
	/// <exception cref="T:System.FormatException">String cannot be parsed.</exception>
	public static IList<XamlTypeName> ParseList(string typeNameList, IXamlNamespaceResolver namespaceResolver)
	{
		ArgumentNullException.ThrowIfNull(typeNameList, "typeNameList");
		ArgumentNullException.ThrowIfNull(namespaceResolver, "namespaceResolver");
		string error;
		return ParseListInternal(typeNameList, namespaceResolver.GetNamespace, out error) ?? throw new FormatException(error);
	}

	/// <summary>Provides a <see cref="T:System.Xaml.Schema.XamlTypeName" /> value based on a type name and an object that can resolve a markup prefix into a namespace.</summary>
	/// <returns>true if the parse was successful and <paramref name="result" /> contains a useful value; otherwise, false.</returns>
	/// <param name="typeName">The type name to create a <see cref="T:System.Xaml.Schema.XamlTypeName" /> value for.</param>
	/// <param name="namespaceResolver">An object or service provider that implements <see cref="T:System.Xaml.IXamlNamespaceResolver" />.</param>
	/// <param name="result">Out parameter that contains the created <see cref="T:System.Xaml.Schema.XamlTypeName" /> if the return value is true.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> or <paramref name="namespaceResolver" /> is null.</exception>
	public static bool TryParse(string typeName, IXamlNamespaceResolver namespaceResolver, out XamlTypeName result)
	{
		ArgumentNullException.ThrowIfNull(typeName, "typeName");
		ArgumentNullException.ThrowIfNull(namespaceResolver, "namespaceResolver");
		result = ParseInternal(typeName, namespaceResolver.GetNamespace, out var _);
		return result != null;
	}

	/// <summary>Provides a <see cref="T:System.Xaml.Schema.XamlTypeName" /> value based on a string that can specify multiple type names, and an object that can resolve a markup prefix into a namespace.</summary>
	/// <returns>true if the parse was successful and <paramref name="result" /> contains a useful value; otherwise, false.</returns>
	/// <param name="typeNameList">A string that contains multiple types. See Remarks. </param>
	/// <param name="namespaceResolver">An object or service provider that implements <see cref="T:System.Xaml.IXamlNamespaceResolver" />.</param>
	/// <param name="result">Out parameter that contains the created <see cref="T:System.Xaml.Schema.XamlTypeName" /> if the return value is true.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeNameList" /> or <paramref name="namespaceResolver" /> is null.</exception>
	public static bool TryParseList(string typeNameList, IXamlNamespaceResolver namespaceResolver, out IList<XamlTypeName> result)
	{
		ArgumentNullException.ThrowIfNull(typeNameList, "typeNameList");
		ArgumentNullException.ThrowIfNull(namespaceResolver, "namespaceResolver");
		result = ParseListInternal(typeNameList, namespaceResolver.GetNamespace, out var _);
		return result != null;
	}

	internal static string ConvertListToStringInternal(IList<XamlTypeName> typeNameList, Func<string, string> prefixGenerator)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ConvertListToStringInternal(stringBuilder, typeNameList, prefixGenerator);
		return stringBuilder.ToString();
	}

	internal static void ConvertListToStringInternal(StringBuilder result, IList<XamlTypeName> typeNameList, Func<string, string> prefixGenerator)
	{
		bool flag = true;
		foreach (XamlTypeName typeName in typeNameList)
		{
			if (!flag)
			{
				result.Append(", ");
			}
			else
			{
				flag = false;
			}
			typeName.ConvertToStringInternal(result, prefixGenerator);
		}
	}

	internal static XamlTypeName ParseInternal(string typeName, Func<string, string> prefixResolver, out string error)
	{
		XamlTypeName xamlTypeName = GenericTypeNameParser.ParseIfTrivalName(typeName, prefixResolver, out error);
		if (xamlTypeName != null)
		{
			return xamlTypeName;
		}
		return new GenericTypeNameParser(prefixResolver).ParseName(typeName, out error);
	}

	internal static IList<XamlTypeName> ParseListInternal(string typeNameList, Func<string, string> prefixResolver, out string error)
	{
		return new GenericTypeNameParser(prefixResolver).ParseList(typeNameList, out error);
	}

	internal string ConvertToStringInternal(Func<string, string> prefixGenerator)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ConvertToStringInternal(stringBuilder, prefixGenerator);
		return stringBuilder.ToString();
	}

	internal void ConvertToStringInternal(StringBuilder result, Func<string, string> prefixGenerator)
	{
		if (Namespace == null)
		{
			throw new InvalidOperationException(System.SR.XamlTypeNameNamespaceIsNull);
		}
		if (string.IsNullOrEmpty(Name))
		{
			throw new InvalidOperationException(System.SR.XamlTypeNameNameIsNullOrEmpty);
		}
		if (prefixGenerator == null)
		{
			result.Append('{');
			result.Append(Namespace);
			result.Append('}');
		}
		else
		{
			string text = prefixGenerator(Namespace);
			if (text == null)
			{
				throw new InvalidOperationException(System.SR.Format(System.SR.XamlTypeNameCannotGetPrefix, Namespace));
			}
			if (text.Length != 0)
			{
				result.Append(text);
				result.Append(':');
			}
		}
		if (HasTypeArgs)
		{
			string subscript;
			string value = GenericTypeNameScanner.StripSubscript(Name, out subscript);
			result.Append(value);
			result.Append('(');
			ConvertListToStringInternal(result, TypeArguments, prefixGenerator);
			result.Append(')');
			result.Append(subscript);
		}
		else
		{
			result.Append(Name);
		}
	}
}
