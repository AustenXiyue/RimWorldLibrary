using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Implements a markup extension that returns static field and property references. </summary>
[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[TypeConverter(typeof(StaticExtensionConverter))]
[MarkupExtensionReturnType(typeof(object))]
public class StaticExtension : MarkupExtension
{
	private string _member;

	private Type _memberType;

	/// <summary>Gets or sets a member name string that is used to resolve a static field or property based on the service-provided type resolver.</summary>
	/// <returns>A string that identifies the member to make a reference to. See Remarks.</returns>
	/// <exception cref="T:System.ArgumentNullException">Attempted to set <see cref="P:System.Windows.Markup.StaticExtension.Member" />  to null.</exception>
	[ConstructorArgument("member")]
	public string Member
	{
		get
		{
			return _member;
		}
		set
		{
			_member = value ?? throw new ArgumentNullException("value");
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Type" /> that defines the static member to return.</summary>
	/// <returns>The <see cref="T:System.Type" /> that defines the static member to return.</returns>
	/// <exception cref="T:System.ArgumentNullException">Attempted to set <see cref="P:System.Windows.Markup.StaticExtension.MemberType" />  to null.</exception>
	[DefaultValue(null)]
	public Type MemberType
	{
		get
		{
			return _memberType;
		}
		set
		{
			_memberType = value ?? throw new ArgumentNullException("value");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.StaticExtension" /> class.</summary>
	public StaticExtension()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.StaticExtension" /> class using the provided <paramref name="member" /> string.</summary>
	/// <param name="member">A string that identifies the member to make a reference to. This string uses the format prefix:typeName.fieldOrPropertyName. prefix is the mapping prefix for a XAML namespace, and is only required to reference static values that are not mapped to the default XAML namespace.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="member" /> is null.</exception>
	public StaticExtension(string member)
	{
		_member = member ?? throw new ArgumentNullException("member");
	}

	/// <summary>Returns an object value to set on the property where you apply this extension. For <see cref="T:System.Windows.Markup.StaticExtension" />, the return value is the static value that is evaluated for the requested static member.</summary>
	/// <returns>The static value to set on the property where the extension is applied. </returns>
	/// <param name="serviceProvider">An object that can provide services for the markup extension. The service provider is expected to provide a service that implements a type resolver (<see cref="T:System.Windows.Markup.IXamlTypeResolver" />).</param>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="member" /> value for the extension is null at the time of evaluation.</exception>
	/// <exception cref="T:System.ArgumentException">Some part of the <paramref name="member" /> string did not parse properly-or-<paramref name="serviceProvider" /> did not provide a service for <see cref="T:System.Windows.Markup.IXamlTypeResolver" />-or-<paramref name="member" /> value did not resolve to a static member.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceProvider" /> is null.</exception>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_member == null)
		{
			throw new InvalidOperationException(System.SR.MarkupExtensionStaticMember);
		}
		Type type = MemberType;
		string text = null;
		string text2;
		if (type != null)
		{
			text2 = _member;
			text = type.FullName;
		}
		else
		{
			int num = _member.IndexOf('.');
			if (num < 0)
			{
				throw new ArgumentException(System.SR.Format(System.SR.MarkupExtensionBadStatic, _member));
			}
			string text3 = _member.Substring(0, num);
			if (string.IsNullOrEmpty(text3))
			{
				throw new ArgumentException(System.SR.Format(System.SR.MarkupExtensionBadStatic, _member));
			}
			ArgumentNullException.ThrowIfNull(serviceProvider, "serviceProvider");
			type = ((serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver) ?? throw new ArgumentException(System.SR.Format(System.SR.MarkupExtensionNoContext, GetType().Name, "IXamlTypeResolver"))).Resolve(text3);
			text2 = _member.Substring(num + 1, _member.Length - num - 1);
			if (string.IsNullOrEmpty(text3))
			{
				throw new ArgumentException(System.SR.Format(System.SR.MarkupExtensionBadStatic, _member));
			}
		}
		if (type.IsEnum)
		{
			return Enum.Parse(type, text2);
		}
		if (GetFieldOrPropertyValue(type, text2, out var value))
		{
			return value;
		}
		throw new ArgumentException(System.SR.Format(System.SR.MarkupExtensionBadStatic, (text != null) ? (text + "." + _member) : _member));
	}

	private bool GetFieldOrPropertyValue(Type type, string name, out object value)
	{
		FieldInfo fieldInfo = null;
		Type type2 = type;
		do
		{
			fieldInfo = type2.GetField(name, BindingFlags.Static | BindingFlags.Public);
			if (fieldInfo != null)
			{
				value = fieldInfo.GetValue(null);
				return true;
			}
			type2 = type2.BaseType;
		}
		while (type2 != null);
		PropertyInfo propertyInfo = null;
		type2 = type;
		do
		{
			propertyInfo = type2.GetProperty(name, BindingFlags.Static | BindingFlags.Public);
			if (propertyInfo != null)
			{
				value = propertyInfo.GetValue(null, null);
				return true;
			}
			type2 = type2.BaseType;
		}
		while (type2 != null);
		value = null;
		return false;
	}
}
