using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Implements a markup extension that returns a <see cref="T:System.Type" /> based on a string input. </summary>
[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[TypeConverter(typeof(TypeExtensionConverter))]
[MarkupExtensionReturnType(typeof(Type))]
public class TypeExtension : MarkupExtension
{
	private string _typeName;

	private Type _type;

	/// <summary>Gets or sets the type name represented by this markup extension.</summary>
	/// <returns>A string that identifies the type. This string uses the format prefix:className. (prefix is the mapping prefix for an XML namespace, and is only required to reference types that are not mapped to the default XML namespace for WPF (http://schemas.microsoft.com/winfx/2006/xaml/presentation).</returns>
	/// <exception cref="T:System.ArgumentNullException">Attempted to set to null.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string TypeName
	{
		get
		{
			return _typeName;
		}
		set
		{
			_typeName = value ?? throw new ArgumentNullException("value");
			_type = null;
		}
	}

	/// <summary>Gets or sets the type information for this extension.</summary>
	/// <returns>The established type. For runtime purposes, this may be null for get access, but cannot be set to null.</returns>
	/// <exception cref="T:System.ArgumentNullException">Attempted to set to null.</exception>
	[DefaultValue(null)]
	[ConstructorArgument("type")]
	public Type Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value ?? throw new ArgumentNullException("value");
			_typeName = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.TypeExtension" /> class.</summary>
	public TypeExtension()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.TypeExtension" /> class, initializing the <see cref="P:System.Windows.Markup.TypeExtension.TypeName" /> value based on the provided <paramref name="typeName" /> string.</summary>
	/// <param name="typeName">A string that identifies the type to make a reference to. This string uses the format prefix:className. prefix is the mapping prefix for a XAML namespace, and is only required to reference types that are not mapped to the default XAML namespace.</param>
	/// <exception cref="T:System.ArgumentNullException">Attempted to specify <paramref name="typeName" /> as null.</exception>
	public TypeExtension(string typeName)
	{
		_typeName = typeName ?? throw new ArgumentNullException("typeName");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.TypeExtension" /> class, declaring the type directly.</summary>
	/// <param name="type">The type to be represented by this <see cref="T:System.Windows.Markup.TypeExtension" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null</exception>
	public TypeExtension(Type type)
	{
		_type = type ?? throw new ArgumentNullException("type");
	}

	/// <summary>Returns an object that should be set on the property where this extension is applied. For <see cref="T:System.Windows.Markup.TypeExtension" /> , this is the <see cref="T:System.Type" /> value as evaluated for the requested type name.</summary>
	/// <returns>The <see cref="T:System.Type" /> to set on the property where the extension is applied. </returns>
	/// <param name="serviceProvider">Object that can provide services for the markup extension. The provider is expected to provide a service for <see cref="T:System.Windows.Markup.IXamlTypeResolver" />.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="member" /> value for the extension is null.</exception>
	/// <exception cref="T:System.ArgumentException">Some part of the <paramref name="typeName" /> string did not parse properly.-or-<paramref name="serviceProvider" /> did not provide a service for <see cref="T:System.Windows.Markup.IXamlTypeResolver" />-or-<paramref name="typeName" /> value did not resolve to a type.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceProvider" /> is null</exception>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_type != null)
		{
			return _type;
		}
		if (_typeName == null)
		{
			throw new InvalidOperationException(System.SR.MarkupExtensionTypeName);
		}
		ArgumentNullException.ThrowIfNull(serviceProvider, "serviceProvider");
		if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver xamlTypeResolver))
		{
			throw new InvalidOperationException(System.SR.Format(System.SR.MarkupExtensionNoContext, GetType().Name, "IXamlTypeResolver"));
		}
		_type = xamlTypeResolver.Resolve(_typeName);
		if (_type == null)
		{
			throw new InvalidOperationException(System.SR.Format(System.SR.MarkupExtensionTypeNameBad, _typeName));
		}
		return _type;
	}
}
