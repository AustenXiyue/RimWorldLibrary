using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Used within the template of an item control to specify the place in the control’s visual tree where the <see cref="P:System.Windows.Controls.ItemsControl.ItemsPanel" /> defined by the <see cref="T:System.Windows.Controls.ItemsControl" /> is to be added.</summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public class ItemsPresenter : FrameworkElement
{
	internal static readonly DependencyProperty TemplateProperty = DependencyProperty.Register("Template", typeof(ItemsPanelTemplate), typeof(ItemsPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTemplateChanged));

	private ItemsControl _owner;

	private ItemContainerGenerator _generator;

	private ItemsPanelTemplate _templateCache;

	internal ItemsControl Owner => _owner;

	internal ItemContainerGenerator Generator => _generator;

	internal override FrameworkTemplate TemplateInternal => Template;

	internal override FrameworkTemplate TemplateCache
	{
		get
		{
			return _templateCache;
		}
		set
		{
			_templateCache = (ItemsPanelTemplate)value;
		}
	}

	private ItemsPanelTemplate Template
	{
		get
		{
			return _templateCache;
		}
		set
		{
			SetValue(TemplateProperty, value);
		}
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		AttachToOwner();
	}

	/// <summary>Called when an internal process or application calls <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />, which is used to build the current template's visual tree.</summary>
	public override void OnApplyTemplate()
	{
		if (!(GetVisualChild(0) is Panel reference) || VisualTreeHelper.GetChildrenCount(reference) > 0)
		{
			throw new InvalidOperationException(SR.ItemsPanelNotSingleNode);
		}
		OnPanelChanged(this, EventArgs.Empty);
		base.OnApplyTemplate();
	}

	/// <summary>Overrides the base class implementation of <see cref="M:System.Windows.FrameworkElement.MeasureOverride(System.Windows.Size)" /> to measure the size of the <see cref="T:System.Windows.Controls.ItemsPresenter" /> object and return proper sizes to the layout engine.</summary>
	/// <returns>The desired size.</returns>
	/// <param name="constraint">Constraint size is an "upper limit." The return value should not exceed this size.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		return Helper.MeasureElementWithSingleChild(this, constraint);
	}

	/// <summary> Called to arrange and size the content of an <see cref="T:System.Windows.Controls.ItemsPresenter" /> object. </summary>
	/// <returns>Size of content.</returns>
	/// <param name="arrangeSize">Computed size used to arrange the content.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		return Helper.ArrangeElementWithSingleChild(this, arrangeSize);
	}

	internal override void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		OnTemplateChanged((ItemsPanelTemplate)oldTemplate, (ItemsPanelTemplate)newTemplate);
	}

	private static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		StyleHelper.UpdateTemplateCache((ItemsPresenter)d, (FrameworkTemplate)e.OldValue, (FrameworkTemplate)e.NewValue, TemplateProperty);
	}

	/// <summary>Called when the control template changes.</summary>
	/// <param name="oldTemplate">Value of the old template.</param>
	/// <param name="newTemplate">Value of the new template.</param>
	protected virtual void OnTemplateChanged(ItemsPanelTemplate oldTemplate, ItemsPanelTemplate newTemplate)
	{
	}

	internal static ItemsPresenter FromPanel(Panel panel)
	{
		if (panel == null)
		{
			return null;
		}
		return panel.TemplatedParent as ItemsPresenter;
	}

	internal static ItemsPresenter FromGroupItem(GroupItem groupItem)
	{
		if (groupItem == null)
		{
			return null;
		}
		if (!(VisualTreeHelper.GetParent(groupItem) is Visual reference))
		{
			return null;
		}
		return VisualTreeHelper.GetParent(reference) as ItemsPresenter;
	}

	internal override void OnAncestorChanged()
	{
		if (base.TemplatedParent == null)
		{
			UseGenerator(null);
			ClearPanel();
		}
		base.OnAncestorChanged();
	}

	private void AttachToOwner()
	{
		DependencyObject templatedParent = base.TemplatedParent;
		ItemsControl itemsControl = templatedParent as ItemsControl;
		ItemContainerGenerator generator;
		if (itemsControl != null)
		{
			generator = itemsControl.ItemContainerGenerator;
		}
		else
		{
			GroupItem groupItem = templatedParent as GroupItem;
			ItemsPresenter itemsPresenter = FromGroupItem(groupItem);
			if (itemsPresenter != null)
			{
				itemsControl = itemsPresenter.Owner;
			}
			generator = groupItem?.Generator;
		}
		_owner = itemsControl;
		UseGenerator(generator);
		ItemsPanelTemplate itemsPanelTemplate = null;
		GroupStyle groupStyle = ((_generator != null) ? _generator.GroupStyle : null);
		if (groupStyle != null)
		{
			itemsPanelTemplate = groupStyle.Panel;
			if (itemsPanelTemplate == null)
			{
				itemsPanelTemplate = ((!VirtualizingPanel.GetIsVirtualizingWhenGrouping(itemsControl)) ? GroupStyle.DefaultStackPanel : GroupStyle.DefaultVirtualizingStackPanel);
			}
		}
		else
		{
			itemsPanelTemplate = ((_owner != null) ? _owner.ItemsPanel : null);
		}
		Template = itemsPanelTemplate;
	}

	private void UseGenerator(ItemContainerGenerator generator)
	{
		if (generator != _generator)
		{
			if (_generator != null)
			{
				_generator.PanelChanged -= OnPanelChanged;
			}
			_generator = generator;
			if (_generator != null)
			{
				_generator.PanelChanged += OnPanelChanged;
			}
		}
	}

	private void OnPanelChanged(object sender, EventArgs e)
	{
		InvalidateMeasure();
		if (base.Parent is ScrollViewer && VisualTreeHelper.GetParent(this) is ScrollContentPresenter scrollContentPresenter)
		{
			scrollContentPresenter.HookupScrollingComponents();
		}
	}

	private void ClearPanel()
	{
		Panel panel = ((VisualChildrenCount > 0) ? (GetVisualChild(0) as Panel) : null);
		Type type = null;
		if (Template != null)
		{
			if (Template.VisualTree != null)
			{
				type = Template.VisualTree.Type;
			}
			else if (Template.HasXamlNodeContent)
			{
				type = Template.Template.RootType.UnderlyingType;
			}
		}
		if (panel != null && panel.GetType() == type)
		{
			panel.IsItemsHost = false;
		}
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Controls.ItemsPresenter" /> class. </summary>
	public ItemsPresenter()
	{
	}
}
