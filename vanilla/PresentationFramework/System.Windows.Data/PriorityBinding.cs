using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Describes a collection of <see cref="T:System.Windows.Data.Binding" /> objects that is attached to a single binding target property, which receives its value from the first binding in the collection that produces a value successfully.</summary>
[ContentProperty("Bindings")]
public class PriorityBinding : BindingBase, IAddChild
{
	private BindingCollection _bindingCollection;

	/// <summary>Gets the collection of <see cref="T:System.Windows.Data.Binding" /> objects that is established for this instance of <see cref="T:System.Windows.Data.PriorityBinding" />.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Data.Binding" /> objects. <see cref="T:System.Windows.Data.PriorityBinding" /> currently supports only objects of type <see cref="T:System.Windows.Data.Binding" /> and not <see cref="T:System.Windows.Data.MultiBinding" /> or <see cref="T:System.Windows.Data.PriorityBinding" />. Adding a <see cref="T:System.Windows.Data.Binding" /> child to a <see cref="T:System.Windows.Data.PriorityBinding" /> object implicitly adds the child to the <see cref="T:System.Windows.Data.BindingBase" /> collection for the <see cref="T:System.Windows.Data.MultiBinding" /> object. The default is an empty collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public Collection<BindingBase> Bindings => _bindingCollection;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.PriorityBinding" /> class.</summary>
	public PriorityBinding()
	{
		_bindingCollection = new BindingCollection(this, OnBindingCollectionChanged);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		if (value is BindingBase item)
		{
			Bindings.Add(item);
			return;
		}
		throw new ArgumentException(SR.Format(SR.ChildHasWrongType, GetType().Name, "BindingBase", value.GetType().FullName), "value");
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Returns a value that indicates whether serialization processes should serialize the effective value of the <see cref="P:System.Windows.Data.PriorityBinding.Bindings" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Data.PriorityBinding.Bindings" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeBindings()
	{
		if (Bindings != null)
		{
			return Bindings.Count > 0;
		}
		return false;
	}

	internal override BindingExpressionBase CreateBindingExpressionOverride(DependencyObject target, DependencyProperty dp, BindingExpressionBase owner)
	{
		return PriorityBindingExpression.CreateBindingExpression(target, dp, this, owner);
	}

	internal override BindingBase CreateClone()
	{
		return new PriorityBinding();
	}

	internal override void InitializeClone(BindingBase baseClone, BindingMode mode)
	{
		PriorityBinding priorityBinding = (PriorityBinding)baseClone;
		for (int i = 0; i <= _bindingCollection.Count; i++)
		{
			priorityBinding._bindingCollection.Add(_bindingCollection[i].Clone(mode));
		}
		base.InitializeClone(baseClone, mode);
	}

	private void OnBindingCollectionChanged()
	{
		CheckSealed();
	}
}
