namespace System.ComponentModel;

/// <summary>Specifies whether the template can be bound one way or two ways.</summary>
public enum BindingDirection
{
	/// <summary>The template can only accept property values. Used with a generic <see cref="T:System.Web.UI.ITemplate" />.</summary>
	OneWay,
	/// <summary>The template can accept and expose property values. Used with an <see cref="T:System.Web.UI.IBindableTemplate" />.</summary>
	TwoWay
}
