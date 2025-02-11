using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Implements a markup extension that supports the binding between the value of a property in a template and the value of some other exposed property on the templated control.</summary>
[TypeConverter(typeof(TemplateBindingExtensionConverter))]
[MarkupExtensionReturnType(typeof(object))]
public class TemplateBindingExtension : MarkupExtension
{
	private DependencyProperty _property;

	private IValueConverter _converter;

	private object _parameter;

	/// <summary>Gets or sets the property being bound to. </summary>
	/// <returns>Identifier of the dependency property being bound.</returns>
	[ConstructorArgument("property")]
	public DependencyProperty Property
	{
		get
		{
			return _property;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_property = value;
		}
	}

	/// <summary>Gets or sets the converter that interprets between source and target of a binding.</summary>
	/// <returns>The converter implementation. This value defaults to null and is typically provided as an optional parameter of the binding.</returns>
	[DefaultValue(null)]
	public IValueConverter Converter
	{
		get
		{
			return _converter;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_converter = value;
		}
	}

	/// <summary>Gets or sets the parameter to pass to the converter.</summary>
	/// <returns>The parameter being bound as referenced by the converter implementation. The default value is null.</returns>
	[DefaultValue(null)]
	public object ConverterParameter
	{
		get
		{
			return _parameter;
		}
		set
		{
			_parameter = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplateBindingExtension" /> class.</summary>
	public TemplateBindingExtension()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplateBindingExtension" /> class with the specified dependency property that is the source of the binding.</summary>
	/// <param name="property">The identifier of the property being bound.</param>
	public TemplateBindingExtension(DependencyProperty property)
	{
		if (property != null)
		{
			_property = property;
			return;
		}
		throw new ArgumentNullException("property");
	}

	/// <summary>Returns an object that should be set as the value on the target object's property for this markup extension. For <see cref="T:System.Windows.TemplateBindingExtension" />, this is an expression (<see cref="T:System.Windows.TemplateBindingExpression" />) that supports the binding. </summary>
	/// <returns>The expression that supports the binding.</returns>
	/// <param name="serviceProvider">An object that can provide services for the markup extension. May be null in this implementation.</param>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (Property == null)
		{
			throw new InvalidOperationException(SR.MarkupExtensionProperty);
		}
		return new TemplateBindingExpression(this);
	}
}
