using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Xaml.Schema;

namespace System.Xaml;

/// <summary>Provides the XAML type system identifier for a member if the member is also a XAML directive. XAML readers and XAML writers use the <see cref="T:System.Xaml.XamlDirective" /> identifier during processing of member nodes. The identifier is used when the XAML reader is positioned on a <see cref="F:System.Xaml.XamlNodeType.StartMember" /> and <see cref="P:System.Xaml.XamlMember.IsDirective" /> is true.</summary>
public class XamlDirective : XamlMember
{
	private AllowedMemberLocations _allowedLocation;

	private readonly ReadOnlyCollection<string> _xamlNamespaces;

	/// <summary>Gets a value that specifies the XAML node types where the directive can be specified.</summary>
	/// <returns>A value of the enumeration. The default is the enumeration default, which is <see cref="F:System.Xaml.Schema.AllowedMemberLocations.None" />.</returns>
	public AllowedMemberLocations AllowedLocation => _allowedLocation;

	internal XamlDirective(ReadOnlyCollection<string> immutableXamlNamespaces, string name, AllowedMemberLocations allowedLocation, MemberReflector reflector)
		: base(name, reflector)
	{
		_xamlNamespaces = immutableXamlNamespaces;
		_allowedLocation = allowedLocation;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDirective" /> class, specifying values for each per-case value of a <see cref="T:System.Xaml.XamlDirective" />. </summary>
	/// <param name="xamlNamespaces">A set of XAML namespaces where this <see cref="T:System.Xaml.XamlDirective" /> can exist, passed as an enumerable set of the identifier strings.</param>
	/// <param name="name">The identifying name of the <see cref="T:System.Xaml.XamlDirective" />.</param>
	/// <param name="xamlType">The XAML type that backs the <see cref="T:System.Xaml.XamlDirective" />.</param>
	/// <param name="typeConverter">The type converter that this <see cref="T:System.Xaml.XamlDirective" /> uses for text syntax conversion.</param>
	/// <param name="allowedLocation">A value of the <see cref="T:System.Xaml.Schema.AllowedMemberLocations" /> enumeration.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="xamlType" /> parameter is null.</exception>
	public XamlDirective(IEnumerable<string> xamlNamespaces, string name, XamlType xamlType, XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
		: base(name, new MemberReflector(xamlType, typeConverter))
	{
		ArgumentNullException.ThrowIfNull(xamlType, "xamlType");
		ArgumentNullException.ThrowIfNull(xamlNamespaces, "xamlNamespaces");
		List<string> list = new List<string>(xamlNamespaces);
		foreach (string item in list)
		{
			if (item == null)
			{
				throw new ArgumentException(System.SR.CollectionCannotContainNulls, "xamlNamespaces");
			}
		}
		_xamlNamespaces = list.AsReadOnly();
		_allowedLocation = allowedLocation;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDirective" /> class, specifying values for a name and a single XAML namespace. Use this signature only when you want or expect <see cref="P:System.Xaml.XamlMember.IsUnknown" /> to report true for the directive.</summary>
	/// <param name="xamlNamespace">The primary XAML namespace where this <see cref="T:System.Xaml.XamlDirective" /> can exist.</param>
	/// <param name="name">The identifying name of the <see cref="T:System.Xaml.XamlDirective" />.</param>
	public XamlDirective(string xamlNamespace, string name)
		: base(name, null)
	{
		ArgumentNullException.ThrowIfNull(xamlNamespace, "xamlNamespace");
		_xamlNamespaces = new ReadOnlyCollection<string>(new string[1] { xamlNamespace });
		_allowedLocation = AllowedMemberLocations.Any;
	}

	/// <summary>Returns the hash code for this object.</summary>
	/// <returns>An integer hash code.</returns>
	public override int GetHashCode()
	{
		int num = ((base.Name != null) ? base.Name.GetHashCode() : 0);
		ReadOnlyCollection<string> xamlNamespaces = _xamlNamespaces;
		for (int i = 0; i < xamlNamespaces.Count; i++)
		{
			num ^= xamlNamespaces[i].GetHashCode();
		}
		return num;
	}

	/// <summary>Returns a string representation of this <see cref="T:System.Xaml.XamlDirective" />.</summary>
	/// <returns>A string representation of this <see cref="T:System.Xaml.XamlDirective" />.</returns>
	public override string ToString()
	{
		if (_xamlNamespaces.Count > 0)
		{
			return "{" + _xamlNamespaces[0] + "}" + base.Name;
		}
		return base.Name;
	}

	/// <summary>Returns a list of XAML namespaces where this XAML member can exist. </summary>
	/// <returns>A list of XAML namespace identifiers, as strings.</returns>
	public override IList<string> GetXamlNamespaces()
	{
		return _xamlNamespaces;
	}

	internal static bool NamespacesAreEqual(XamlDirective directive1, XamlDirective directive2)
	{
		ReadOnlyCollection<string> xamlNamespaces = directive1._xamlNamespaces;
		ReadOnlyCollection<string> xamlNamespaces2 = directive2._xamlNamespaces;
		if (xamlNamespaces.Count != xamlNamespaces2.Count)
		{
			return false;
		}
		for (int i = 0; i < xamlNamespaces.Count; i++)
		{
			if (xamlNamespaces[i] != xamlNamespaces2[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns the <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> that is associated with a <see cref="T:System.Xaml.XamlDirective" />.</summary>
	/// <returns>The <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> information for this <see cref="T:System.Xaml.XamlMember" />.</returns>
	protected sealed override XamlMemberInvoker LookupInvoker()
	{
		return XamlMemberInvoker.DirectiveInvoker;
	}

	/// <summary>Returns an <see cref="T:System.Reflection.ICustomAttributeProvider" /> implementation. This implementation always returns null.</summary>
	/// <returns>Always returns null.</returns>
	protected sealed override ICustomAttributeProvider LookupCustomAttributeProvider()
	{
		return null;
	}

	/// <summary>Returns a list of <see cref="T:System.Xaml.XamlMember" /> objects. The list reports the members where dependency relationships for initialization order exist relative to this <see cref="T:System.Xaml.XamlMember" />. This implementation always returns null.</summary>
	/// <returns>Always returns null.</returns>
	protected sealed override IList<XamlMember> LookupDependsOn()
	{
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> object, which is used during deferred loading of XAML-declared objects. This implementation always returns null.</summary>
	/// <returns>Always returns null.</returns>
	protected sealed override XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		return null;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlMember" /> is reported as an ambient property.</summary>
	/// <returns>Always returns false.</returns>
	protected sealed override bool LookupIsAmbient()
	{
		return false;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlDirective" /> represents an event.</summary>
	/// <returns>Always returns false.</returns>
	protected sealed override bool LookupIsEvent()
	{
		return false;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlDirective" /> represents an intended read-only property.</summary>
	/// <returns>Always returns false.</returns>
	protected sealed override bool LookupIsReadOnly()
	{
		return false;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlDirective" /> represents a property that has a public get accessor.</summary>
	/// <returns>Always returns true.</returns>
	protected sealed override bool LookupIsReadPublic()
	{
		return true;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlDirective" /> represents a member that is not resolvable by the backing system that is used for type and member resolution.</summary>
	/// <returns>true if this <see cref="T:System.Xaml.XamlDirective" /> represents a non-resolvable member; otherwise, false.</returns>
	protected sealed override bool LookupIsUnknown()
	{
		return base.IsUnknown;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlDirective" /> represents an intended write-only property.</summary>
	/// <returns>Always returns false.</returns>
	protected sealed override bool LookupIsWriteOnly()
	{
		return false;
	}

	/// <summary>Returns whether this <see cref="T:System.Xaml.XamlDirective" /> represents a property that has a public set accessor.</summary>
	/// <returns>Always returns true.</returns>
	protected sealed override bool LookupIsWritePublic()
	{
		return true;
	}

	/// <summary>Returns the <see cref="T:System.Xaml.XamlType" /> of the type where the <see cref="T:System.Xaml.XamlMember" /> can exist. This implementation always returns null.</summary>
	/// <returns>Always returns null.</returns>
	protected sealed override XamlType LookupTargetType()
	{
		return null;
	}

	/// <summary>Returns a type converter implementation that is associated with this <see cref="T:System.Xaml.XamlDirective" />.</summary>
	/// <returns>A <see cref="T:System.Xaml.Schema.XamlValueConverter`1" /> instance that has <see cref="T:System.ComponentModel.TypeConverter" /> constraint; or null.</returns>
	protected sealed override XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		return base.TypeConverter;
	}

	/// <summary>Returns the <see cref="T:System.Xaml.XamlType" /> of the type that is used by the member. </summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> of the type that is used by the member. </returns>
	protected sealed override XamlType LookupType()
	{
		return base.Type;
	}

	/// <summary>Returns a get accessor that is associated with this <see cref="T:System.Xaml.XamlDirective" />. This implementation always returns null.</summary>
	/// <returns>Always returns null.</returns>
	protected sealed override MethodInfo LookupUnderlyingGetter()
	{
		return null;
	}

	/// <summary>Returns a CLR reflection <see cref="T:System.Reflection.MemberInfo" /> that is associated with this <see cref="T:System.Xaml.XamlDirective" />. This implementation always returns null.</summary>
	/// <returns>Always returns null.</returns>
	protected sealed override MemberInfo LookupUnderlyingMember()
	{
		return null;
	}

	/// <summary>Returns a set accessor that is associated with this <see cref="T:System.Xaml.XamlDirective" />. This implementation always returns null.</summary>
	/// <returns>Always returns null.</returns>
	protected sealed override MethodInfo LookupUnderlyingSetter()
	{
		return null;
	}
}
