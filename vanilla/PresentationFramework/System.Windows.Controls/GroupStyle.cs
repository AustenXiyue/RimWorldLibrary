using System.ComponentModel;

namespace System.Windows.Controls;

/// <summary>Defines how you want the group to look at each level.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class GroupStyle : INotifyPropertyChanged
{
	/// <summary>Identifies the default <see cref="T:System.Windows.Controls.ItemsPanelTemplate" /> that creates the panel used to layout the items.</summary>
	public static readonly ItemsPanelTemplate DefaultGroupPanel;

	private ItemsPanelTemplate _panel;

	private Style _containerStyle;

	private StyleSelector _containerStyleSelector;

	private DataTemplate _headerTemplate;

	private DataTemplateSelector _headerTemplateSelector;

	private string _headerStringFormat;

	private bool _hidesIfEmpty;

	private bool _isAlternationCountSet;

	private int _alternationCount;

	private static GroupStyle s_DefaultGroupStyle;

	internal static ItemsPanelTemplate DefaultStackPanel;

	internal static ItemsPanelTemplate DefaultVirtualizingStackPanel;

	/// <summary>Gets or sets a template that creates the panel used to layout the items.</summary>
	/// <returns>An <see cref="T:System.Windows.Controls.ItemsPanelTemplate" /> object that creates the panel used to layout the items.</returns>
	public ItemsPanelTemplate Panel
	{
		get
		{
			return _panel;
		}
		set
		{
			_panel = value;
			OnPropertyChanged("Panel");
		}
	}

	/// <summary>Gets or sets the style that is applied to the <see cref="T:System.Windows.Controls.GroupItem" /> generated for each item.</summary>
	/// <returns>The style that is applied to the <see cref="T:System.Windows.Controls.GroupItem" /> generated for each item. The default is null.</returns>
	[DefaultValue(null)]
	public Style ContainerStyle
	{
		get
		{
			return _containerStyle;
		}
		set
		{
			_containerStyle = value;
			OnPropertyChanged("ContainerStyle");
		}
	}

	/// <summary>Enables the application writer to provide custom selection logic for a style to apply to each generated <see cref="T:System.Windows.Controls.GroupItem" />.</summary>
	/// <returns>An object that derives from <see cref="T:System.Windows.Controls.StyleSelector" />. The default is null.</returns>
	[DefaultValue(null)]
	public StyleSelector ContainerStyleSelector
	{
		get
		{
			return _containerStyleSelector;
		}
		set
		{
			_containerStyleSelector = value;
			OnPropertyChanged("ContainerStyleSelector");
		}
	}

	/// <summary>Gets or sets the template that is used to display the group header.</summary>
	/// <returns>A <see cref="T:System.Windows.DataTemplate" /> object that is used to display the group header. The default is null.</returns>
	[DefaultValue(null)]
	public DataTemplate HeaderTemplate
	{
		get
		{
			return _headerTemplate;
		}
		set
		{
			_headerTemplate = value;
			OnPropertyChanged("HeaderTemplate");
		}
	}

	/// <summary>Enables the application writer to provide custom selection logic for a template that is used to display the group header.</summary>
	/// <returns>An object that derives from <see cref="T:System.Windows.Controls.DataTemplateSelector" />. The default is null.</returns>
	[DefaultValue(null)]
	public DataTemplateSelector HeaderTemplateSelector
	{
		get
		{
			return _headerTemplateSelector;
		}
		set
		{
			_headerTemplateSelector = value;
			OnPropertyChanged("HeaderTemplateSelector");
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the header if it is displayed as a string.</summary>
	/// <returns>A composite string that specifies how to format the header if it is displayed as a string.</returns>
	[DefaultValue(null)]
	public string HeaderStringFormat
	{
		get
		{
			return _headerStringFormat;
		}
		set
		{
			_headerStringFormat = value;
			OnPropertyChanged("HeaderStringFormat");
		}
	}

	/// <summary>Gets or sets a value that indicates whether items corresponding to empty groups should be displayed.</summary>
	/// <returns>true to not display empty groups; otherwise, false. The default is false.</returns>
	[DefaultValue(false)]
	public bool HidesIfEmpty
	{
		get
		{
			return _hidesIfEmpty;
		}
		set
		{
			_hidesIfEmpty = value;
			OnPropertyChanged("HidesIfEmpty");
		}
	}

	/// <summary>Gets or sets the number of alternating <see cref="T:System.Windows.Controls.GroupItem" /> objects.</summary>
	/// <returns>The number of alternating <see cref="T:System.Windows.Controls.GroupItem" /> objects.</returns>
	[DefaultValue(0)]
	public int AlternationCount
	{
		get
		{
			return _alternationCount;
		}
		set
		{
			_alternationCount = value;
			_isAlternationCountSet = true;
			OnPropertyChanged("AlternationCount");
		}
	}

	/// <summary>Gets the default style of the group.</summary>
	/// <returns>The default style of the group.</returns>
	public static GroupStyle Default => s_DefaultGroupStyle;

	internal bool IsAlternationCountSet => _isAlternationCountSet;

	/// <summary>Occurs when a property value changes.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PropertyChanged += value;
		}
		remove
		{
			PropertyChanged -= value;
		}
	}

	/// <summary>Occurs when a property value changes.</summary>
	protected virtual event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GroupStyle" /> class.</summary>
	public GroupStyle()
	{
	}

	static GroupStyle()
	{
		ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));
		itemsPanelTemplate.Seal();
		DefaultGroupPanel = itemsPanelTemplate;
		DefaultStackPanel = itemsPanelTemplate;
		ItemsPanelTemplate itemsPanelTemplate2 = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));
		itemsPanelTemplate2.Seal();
		DefaultVirtualizingStackPanel = itemsPanelTemplate2;
		s_DefaultGroupStyle = new GroupStyle();
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.GroupStyle.PropertyChanged" /> event using the provided arguments.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}
}
