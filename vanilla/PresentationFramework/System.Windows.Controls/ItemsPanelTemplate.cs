using System.Xaml;

namespace System.Windows.Controls;

/// <summary>Specifies the panel that the <see cref="T:System.Windows.Controls.ItemsPresenter" /> creates for the layout of the items of an <see cref="T:System.Windows.Controls.ItemsControl" />.</summary>
public class ItemsPanelTemplate : FrameworkTemplate
{
	internal override Type TargetTypeInternal => DefaultTargetType;

	internal static Type DefaultTargetType => typeof(ItemsPresenter);

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.Controls.ItemsPanelTemplate" /> class.</summary>
	public ItemsPanelTemplate()
	{
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.Controls.ItemsPanelTemplate" /> class with the specified template.</summary>
	/// <param name="root">The <see cref="T:System.Windows.FrameworkElementFactory" /> object that represents the template.</param>
	public ItemsPanelTemplate(FrameworkElementFactory root)
	{
		base.VisualTree = root;
	}

	internal override void SetTargetTypeInternal(Type targetType)
	{
		throw new InvalidOperationException(SR.TemplateNotTargetType);
	}

	internal override void ProcessTemplateBeforeSeal()
	{
		FrameworkElementFactory visualTree;
		if (base.HasContent)
		{
			TemplateContent template = base.Template;
			XamlType xamlType = template.SchemaContext.GetXamlType(typeof(Panel));
			if (template.RootType == null || !template.RootType.CanAssignTo(xamlType))
			{
				throw new InvalidOperationException(SR.Format(SR.ItemsPanelNotAPanel, template.RootType));
			}
		}
		else if ((visualTree = base.VisualTree) != null)
		{
			if (!typeof(Panel).IsAssignableFrom(visualTree.Type))
			{
				throw new InvalidOperationException(SR.Format(SR.ItemsPanelNotAPanel, visualTree.Type));
			}
			visualTree.SetValue(Panel.IsItemsHostProperty, true);
		}
	}

	/// <summary>Checks that the templated parent is a non-null <see cref="T:System.Windows.Controls.ItemsPresenter" /> object.</summary>
	/// <param name="templatedParent">The element this template is applied to. This must be an <see cref="T:System.Windows.Controls.ItemsPresenter" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="templatedParent" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="templatedParent" /> is not an <see cref="T:System.Windows.Controls.ItemsPresenter" />.</exception>
	protected override void ValidateTemplatedParent(FrameworkElement templatedParent)
	{
		if (templatedParent == null)
		{
			throw new ArgumentNullException("templatedParent");
		}
		if (!(templatedParent is ItemsPresenter))
		{
			throw new ArgumentException(SR.Format(SR.TemplateTargetTypeMismatch, "ItemsPresenter", templatedParent.GetType().Name));
		}
	}
}
