using System.Xaml;

namespace System.Windows.Markup;

/// <summary>Implements the {x:Reference} markup extension.</summary>
[ContentProperty("Name")]
public class Reference : MarkupExtension
{
	/// <summary>Gets or sets the XAML name to obtain the reference for.</summary>
	/// <returns>The XAML name of the element to obtain the reference for.</returns>
	[ConstructorArgument("name")]
	public string Name { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Reference" /> class.</summary>
	public Reference()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Reference" /> class with the <paramref name="name" /> argument.</summary>
	/// <param name="name">The XAML name of the element to reference.</param>
	public Reference(string name)
	{
		Name = name;
	}

	/// <summary>Returns an object that is the value of the target property. For the <see cref="T:System.Windows.Markup.Reference" /> type, this is the object that the provided <see cref="P:System.Windows.Markup.Reference.Name" /> references.</summary>
	/// <returns>The value of the target property. This is potentially any object that is type-mapped in the relevant backing assemblies.</returns>
	/// <param name="serviceProvider">A class that implements the <see cref="T:System.Xaml.IXamlNameResolver" /> service. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="serviceProvider" /> value does not implement the <see cref="T:System.Xaml.IXamlNameResolver" /> service.-or-<see cref="P:System.Windows.Markup.Reference.Name" /> value has not been set through construction or positional usage.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceProvider" /> is null.</exception>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		ArgumentNullException.ThrowIfNull(serviceProvider, "serviceProvider");
		if (!(serviceProvider.GetService(typeof(IXamlNameResolver)) is IXamlNameResolver xamlNameResolver))
		{
			throw new InvalidOperationException(System.SR.MissingNameResolver);
		}
		if (string.IsNullOrEmpty(Name))
		{
			throw new InvalidOperationException(System.SR.MustHaveName);
		}
		object obj = xamlNameResolver.Resolve(Name);
		if (obj == null)
		{
			string[] names = new string[1] { Name };
			obj = xamlNameResolver.GetFixupToken(names, canAssignDirectly: true);
		}
		return obj;
	}
}
