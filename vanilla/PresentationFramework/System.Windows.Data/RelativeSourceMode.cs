namespace System.Windows.Data;

/// <summary>Describes the location of the binding source relative to the position of the binding target.</summary>
public enum RelativeSourceMode
{
	/// <summary>Allows you to bind the previous data item (not that control that contains the data item) in the list of data items being displayed.</summary>
	PreviousData,
	/// <summary>Refers to the element to which the template (in which the data-bound element exists) is applied. This is similar to setting a <see cref="T:System.Windows.TemplateBindingExtension" /> and is only applicable if the <see cref="T:System.Windows.Data.Binding" /> is within a template.</summary>
	TemplatedParent,
	/// <summary>Refers to the element on which you are setting the binding and allows you to bind one property of that element to another property on the same element.</summary>
	Self,
	/// <summary>Refers to the ancestor in the parent chain of the data-bound element. You can use this to bind to an ancestor of a specific type or its subclasses. This is the mode you use if you want to specify <see cref="P:System.Windows.Data.RelativeSource.AncestorType" /> and/or <see cref="P:System.Windows.Data.RelativeSource.AncestorLevel" />.</summary>
	FindAncestor
}
