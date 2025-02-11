using System.Xaml;

namespace System.Windows.Markup;

/// <summary>Provides data for callbacks that are invoked when a XAML object writer sets a value using a markup extension.</summary>
public class XamlSetMarkupExtensionEventArgs : XamlSetValueEventArgs
{
	/// <summary>Gets the <see cref="T:System.Windows.Markup.MarkupExtension" /> reference that is relevant to this <see cref="T:System.Windows.Markup.XamlSetMarkupExtensionEventArgs" />.</summary>
	/// <returns>The markup extension reference that is relevant to this <see cref="T:System.Windows.Markup.XamlSetMarkupExtensionEventArgs" />.</returns>
	public MarkupExtension MarkupExtension => base.Value as MarkupExtension;

	/// <summary>Gets service provider information that was passed to the markup extension.</summary>
	/// <returns>Service provider information that was passed to the markup extension.</returns>
	public IServiceProvider ServiceProvider { get; private set; }

	internal XamlType CurrentType { get; set; }

	internal object TargetObject { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlSetMarkupExtensionEventArgs" /> class. </summary>
	/// <param name="member">XAML type system / schema information for the member being set.</param>
	/// <param name="value">The markup extension reference to provide for the member being set.</param>
	/// <param name="serviceProvider">Service provider information passed to the markup extension.</param>
	public XamlSetMarkupExtensionEventArgs(XamlMember member, MarkupExtension value, IServiceProvider serviceProvider)
		: base(member, value)
	{
		ServiceProvider = serviceProvider;
	}

	internal XamlSetMarkupExtensionEventArgs(XamlMember member, MarkupExtension value, IServiceProvider serviceProvider, object targetObject)
		: this(member, value, serviceProvider)
	{
		TargetObject = targetObject;
	}

	/// <summary>Provides a way to invoke a callback as defined on a base class of the current acting type.</summary>
	public override void CallBase()
	{
		if (!(CurrentType != null))
		{
			return;
		}
		XamlType baseType = CurrentType.BaseType;
		if (baseType != null)
		{
			CurrentType = baseType;
			if (baseType.SetMarkupExtensionHandler != null)
			{
				baseType.SetMarkupExtensionHandler(TargetObject, this);
			}
		}
	}
}
