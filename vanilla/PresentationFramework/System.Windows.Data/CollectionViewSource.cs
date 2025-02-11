using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>The Extensible Application Markup Language (XAML) proxy of a <see cref="T:System.Windows.Data.CollectionView" /> class.</summary>
public class CollectionViewSource : DependencyObject, ISupportInitialize, IWeakEventListener
{
	private class DeferHelper : IDisposable
	{
		private CollectionViewSource _target;

		public DeferHelper(CollectionViewSource target)
		{
			_target = target;
			_target.BeginDefer();
		}

		public void Dispose()
		{
			if (_target != null)
			{
				CollectionViewSource target = _target;
				_target = null;
				target.EndDefer();
			}
			GC.SuppressFinalize(this);
		}
	}

	private class FilterStub
	{
		private WeakReference _parent;

		private Predicate<object> _filterWrapper;

		public Predicate<object> FilterWrapper => _filterWrapper;

		public FilterStub(CollectionViewSource parent)
		{
			_parent = new WeakReference(parent);
			_filterWrapper = WrapFilter;
		}

		private bool WrapFilter(object item)
		{
			return ((CollectionViewSource)_parent.Target)?.WrapFilter(item) ?? true;
		}
	}

	private static readonly DependencyPropertyKey ViewPropertyKey = DependencyProperty.RegisterReadOnly("View", typeof(ICollectionView), typeof(CollectionViewSource), new FrameworkPropertyMetadata((object)null));

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.CollectionViewType" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.View" /> dependency property. </returns>
	public static readonly DependencyProperty ViewProperty = ViewPropertyKey.DependencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.Source" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.Source" /> dependency property. </returns>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(CollectionViewSource), new FrameworkPropertyMetadata((object)null, (PropertyChangedCallback)OnSourceChanged), IsSourceValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.CollectionViewType" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.CollectionViewType" /> dependency property.</returns>
	public static readonly DependencyProperty CollectionViewTypeProperty = DependencyProperty.Register("CollectionViewType", typeof(Type), typeof(CollectionViewSource), new FrameworkPropertyMetadata(null, OnCollectionViewTypeChanged), IsCollectionViewTypeValid);

	private static readonly DependencyPropertyKey CanChangeLiveSortingPropertyKey = DependencyProperty.RegisterReadOnly("CanChangeLiveSorting", typeof(bool), typeof(CollectionViewSource), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.CanChangeLiveSorting" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.CanChangeLiveSorting" /> dependency property.</returns>
	public static readonly DependencyProperty CanChangeLiveSortingProperty = CanChangeLiveSortingPropertyKey.DependencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveSortingRequested" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveSortingRequested" /> dependency property.</returns>
	public static readonly DependencyProperty IsLiveSortingRequestedProperty = DependencyProperty.Register("IsLiveSortingRequested", typeof(bool), typeof(CollectionViewSource), new FrameworkPropertyMetadata(false, OnIsLiveSortingRequestedChanged));

	private static readonly DependencyPropertyKey IsLiveSortingPropertyKey = DependencyProperty.RegisterReadOnly("IsLiveSorting", typeof(bool?), typeof(CollectionViewSource), new FrameworkPropertyMetadata((object)null));

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveSorting" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveSorting" /> dependency property.</returns>
	public static readonly DependencyProperty IsLiveSortingProperty = IsLiveSortingPropertyKey.DependencyProperty;

	private static readonly DependencyPropertyKey CanChangeLiveFilteringPropertyKey = DependencyProperty.RegisterReadOnly("CanChangeLiveFiltering", typeof(bool), typeof(CollectionViewSource), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.CanChangeLiveFiltering" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.CanChangeLiveFiltering" /> dependency property.</returns>
	public static readonly DependencyProperty CanChangeLiveFilteringProperty = CanChangeLiveFilteringPropertyKey.DependencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveFilteringRequested" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveFilteringRequested" /> dependency property.</returns>
	public static readonly DependencyProperty IsLiveFilteringRequestedProperty = DependencyProperty.Register("IsLiveFilteringRequested", typeof(bool), typeof(CollectionViewSource), new FrameworkPropertyMetadata(false, OnIsLiveFilteringRequestedChanged));

	private static readonly DependencyPropertyKey IsLiveFilteringPropertyKey = DependencyProperty.RegisterReadOnly("IsLiveFiltering", typeof(bool?), typeof(CollectionViewSource), new FrameworkPropertyMetadata((object)null));

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveFiltering" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveFiltering" /> dependency property.</returns>
	public static readonly DependencyProperty IsLiveFilteringProperty = IsLiveFilteringPropertyKey.DependencyProperty;

	private static readonly DependencyPropertyKey CanChangeLiveGroupingPropertyKey = DependencyProperty.RegisterReadOnly("CanChangeLiveGrouping", typeof(bool), typeof(CollectionViewSource), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.CanChangeLiveGrouping" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.CanChangeLiveGrouping" /> dependency property.</returns>
	public static readonly DependencyProperty CanChangeLiveGroupingProperty = CanChangeLiveGroupingPropertyKey.DependencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveGroupingRequested" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveGroupingRequested" /> dependency property.</returns>
	public static readonly DependencyProperty IsLiveGroupingRequestedProperty = DependencyProperty.Register("IsLiveGroupingRequested", typeof(bool), typeof(CollectionViewSource), new FrameworkPropertyMetadata(false, OnIsLiveGroupingRequestedChanged));

	private static readonly DependencyPropertyKey IsLiveGroupingPropertyKey = DependencyProperty.RegisterReadOnly("IsLiveGrouping", typeof(bool?), typeof(CollectionViewSource), new FrameworkPropertyMetadata((object)null));

	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveGrouping" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionViewSource.IsLiveGrouping" /> dependency property.</returns>
	public static readonly DependencyProperty IsLiveGroupingProperty = IsLiveGroupingPropertyKey.DependencyProperty;

	private CultureInfo _culture;

	private SortDescriptionCollection _sort;

	private ObservableCollection<GroupDescription> _groupBy;

	private ObservableCollection<string> _liveSortingProperties;

	private ObservableCollection<string> _liveFilteringProperties;

	private ObservableCollection<string> _liveGroupingProperties;

	private bool _isInitializing;

	private bool _isViewInitialized;

	private int _version;

	private int _deferLevel;

	private DataSourceProvider _dataProvider;

	private FilterStub _filterStub;

	private DependencyObject _inheritanceContext;

	private bool _hasMultipleInheritanceContexts;

	private DependencyProperty _propertyForInheritanceContext;

	internal static readonly CollectionViewSource DefaultSource = new CollectionViewSource();

	private static readonly UncommonField<FilterEventHandler> FilterHandlersField = new UncommonField<FilterEventHandler>();

	/// <summary>Gets the view object that is currently associated with this instance of <see cref="T:System.Windows.Data.CollectionViewSource" />.  </summary>
	/// <returns>The view object that is currently associated with this instance of <see cref="T:System.Windows.Data.CollectionViewSource" />.</returns>
	[ReadOnly(true)]
	public ICollectionView View => GetOriginalView(CollectionView);

	/// <summary>Gets or sets the collection object from which to create this view.   </summary>
	/// <returns>The default value is null.</returns>
	public object Source
	{
		get
		{
			return GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	/// <summary>Gets or sets the desired view type.  </summary>
	/// <returns>The desired view type.</returns>
	public Type CollectionViewType
	{
		get
		{
			return (Type)GetValue(CollectionViewTypeProperty);
		}
		set
		{
			SetValue(CollectionViewTypeProperty, value);
		}
	}

	/// <summary>Gets or sets the culture that is used for operations such as sorting and comparisons. </summary>
	/// <returns>The culture that is used for operations such as sorting and comparisons.</returns>
	[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
	public CultureInfo Culture
	{
		get
		{
			return _culture;
		}
		set
		{
			_culture = value;
			OnForwardedPropertyChanged();
		}
	}

	/// <summary>Gets or sets a collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describes how the items in the collection are sorted in the view.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describes how the items in the collection are sorted in the view.</returns>
	public SortDescriptionCollection SortDescriptions => _sort;

	/// <summary>Gets or sets a collection of <see cref="T:System.ComponentModel.GroupDescription" /> objects that describes how the items in the collection are grouped in the view.</summary>
	/// <returns>An <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> of <see cref="T:System.ComponentModel.GroupDescription" /> objects that describes how the items in the collection are grouped in the view.</returns>
	public ObservableCollection<GroupDescription> GroupDescriptions => _groupBy;

	/// <summary>Gets a value that indicates whether the collection view supports turning sorting data in real time on or off.</summary>
	/// <returns>true if the collection view supports turning live sorting on or off; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	[ReadOnly(true)]
	public bool CanChangeLiveSorting
	{
		get
		{
			return (bool)GetValue(CanChangeLiveSortingProperty);
		}
		private set
		{
			SetValue(CanChangeLiveSortingPropertyKey, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether <see cref="T:System.Windows.Data.CollectionViewSource" /> should sort the data in real time if it can.</summary>
	/// <returns>true if live sorting has been requested; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsLiveSortingRequested
	{
		get
		{
			return (bool)GetValue(IsLiveSortingRequestedProperty);
		}
		set
		{
			SetValue(IsLiveSortingRequestedProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Data.CollectionViewSource" /> sorts data in real time.</summary>
	/// <returns>true if sorting data in real time is enable; false if live sorting is not enabled; null if it cannot be determined whether the collection view implements live sorting. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	[ReadOnly(true)]
	public bool? IsLiveSorting
	{
		get
		{
			return (bool?)GetValue(IsLiveSortingProperty);
		}
		private set
		{
			SetValue(IsLiveSortingPropertyKey, value);
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in sorting data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in sorting data in real time.</returns>
	public ObservableCollection<string> LiveSortingProperties
	{
		get
		{
			if (_liveSortingProperties == null)
			{
				_liveSortingProperties = new ObservableCollection<string>();
				((INotifyCollectionChanged)_liveSortingProperties).CollectionChanged += OnForwardedCollectionChanged;
			}
			return _liveSortingProperties;
		}
	}

	/// <summary>Gets a value that indicates whether the collection view supports turning filtering data in real time on or off.</summary>
	/// <returns>true if the collection view supports turning live filtering on or off; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	[ReadOnly(true)]
	public bool CanChangeLiveFiltering
	{
		get
		{
			return (bool)GetValue(CanChangeLiveFilteringProperty);
		}
		private set
		{
			SetValue(CanChangeLiveFilteringPropertyKey, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether <see cref="T:System.Windows.Data.CollectionViewSource" /> should filter the data in real time if it can.</summary>
	/// <returns>true if live filtering has been requested; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsLiveFilteringRequested
	{
		get
		{
			return (bool)GetValue(IsLiveFilteringRequestedProperty);
		}
		set
		{
			SetValue(IsLiveFilteringRequestedProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Data.CollectionViewSource" /> is filtering data in real time.</summary>
	/// <returns>true if filtering data in real time is enabled; false if live filtering is not enabled; null if it cannot be determined whether the collection view implements live filtering. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	[ReadOnly(true)]
	public bool? IsLiveFiltering
	{
		get
		{
			return (bool?)GetValue(IsLiveFilteringProperty);
		}
		private set
		{
			SetValue(IsLiveFilteringPropertyKey, value);
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in filtering data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in filtering data in real time.</returns>
	public ObservableCollection<string> LiveFilteringProperties
	{
		get
		{
			if (_liveFilteringProperties == null)
			{
				_liveFilteringProperties = new ObservableCollection<string>();
				((INotifyCollectionChanged)_liveFilteringProperties).CollectionChanged += OnForwardedCollectionChanged;
			}
			return _liveFilteringProperties;
		}
	}

	/// <summary>Gets a value that indicates whether the collection view supports turning grouping data in real time on or off.</summary>
	/// <returns>true if the collection view supports turning live grouping on or off; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	[ReadOnly(true)]
	public bool CanChangeLiveGrouping
	{
		get
		{
			return (bool)GetValue(CanChangeLiveGroupingProperty);
		}
		private set
		{
			SetValue(CanChangeLiveGroupingPropertyKey, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether <see cref="T:System.Windows.Data.CollectionViewSource" /> should group the data in real time if it can.</summary>
	/// <returns>true if live grouping has been requested; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsLiveGroupingRequested
	{
		get
		{
			return (bool)GetValue(IsLiveGroupingRequestedProperty);
		}
		set
		{
			SetValue(IsLiveGroupingRequestedProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Data.CollectionViewSource" /> groups data in real time.</summary>
	/// <returns>true if grouping data in real time is enable; false if live grouping is not enabled; null if it cannot be determined whether the collection view implements live grouping. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	[ReadOnly(true)]
	public bool? IsLiveGrouping
	{
		get
		{
			return (bool?)GetValue(IsLiveGroupingProperty);
		}
		private set
		{
			SetValue(IsLiveGroupingPropertyKey, value);
		}
	}

	/// <summary>Gets a collection of strings that specify the properties that participate in grouping data in real time.</summary>
	/// <returns>A collection of strings that specify the properties that participate in grouping data in real time.</returns>
	public ObservableCollection<string> LiveGroupingProperties
	{
		get
		{
			if (_liveGroupingProperties == null)
			{
				_liveGroupingProperties = new ObservableCollection<string>();
				((INotifyCollectionChanged)_liveGroupingProperties).CollectionChanged += OnForwardedCollectionChanged;
			}
			return _liveGroupingProperties;
		}
	}

	internal CollectionView CollectionView
	{
		get
		{
			ICollectionView obj = (ICollectionView)GetValue(ViewProperty);
			if (obj != null && !_isViewInitialized)
			{
				object obj2 = Source;
				if (obj2 is DataSourceProvider dataSourceProvider)
				{
					obj2 = dataSourceProvider.Data;
				}
				if (obj2 != null)
				{
					ViewRecord viewRecord = DataBindEngine.CurrentDataBindEngine.GetViewRecord(obj2, this, CollectionViewType, createView: true, null);
					if (viewRecord != null)
					{
						viewRecord.InitializeView();
						_isViewInitialized = true;
					}
				}
			}
			return (CollectionView)obj;
		}
	}

	internal DependencyProperty PropertyForInheritanceContext => _propertyForInheritanceContext;

	internal override DependencyObject InheritanceContext => _inheritanceContext;

	internal override bool HasMultipleInheritanceContexts => _hasMultipleInheritanceContexts;

	private Predicate<object> FilterWrapper
	{
		get
		{
			if (_filterStub == null)
			{
				_filterStub = new FilterStub(this);
			}
			return _filterStub.FilterWrapper;
		}
	}

	internal override int EffectiveValuesInitialSize => 3;

	/// <summary>Provides filtering logic.</summary>
	public event FilterEventHandler Filter
	{
		add
		{
			FilterEventHandler value2 = FilterHandlersField.GetValue(this);
			value2 = ((value2 == null) ? value : ((FilterEventHandler)Delegate.Combine(value2, value)));
			FilterHandlersField.SetValue(this, value2);
			OnForwardedPropertyChanged();
		}
		remove
		{
			FilterEventHandler value2 = FilterHandlersField.GetValue(this);
			if (value2 != null)
			{
				value2 = (FilterEventHandler)Delegate.Remove(value2, value);
				if (value2 == null)
				{
					FilterHandlersField.ClearValue(this);
				}
				else
				{
					FilterHandlersField.SetValue(this, value2);
				}
			}
			OnForwardedPropertyChanged();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.CollectionViewSource" /> class.</summary>
	public CollectionViewSource()
	{
		_sort = new SortDescriptionCollection();
		((INotifyCollectionChanged)_sort).CollectionChanged += OnForwardedCollectionChanged;
		_groupBy = new ObservableCollection<GroupDescription>();
		((INotifyCollectionChanged)_groupBy).CollectionChanged += OnForwardedCollectionChanged;
	}

	private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		CollectionViewSource obj = (CollectionViewSource)d;
		obj.OnSourceChanged(e.OldValue, e.NewValue);
		obj.EnsureView();
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Data.CollectionViewSource.Source" /> property changes.</summary>
	/// <param name="oldSource">The old value of the <see cref="P:System.Windows.Data.CollectionViewSource.Source" /> property.</param>
	/// <param name="newSource">The new value of the <see cref="P:System.Windows.Data.CollectionViewSource.Source" /> property.</param>
	protected virtual void OnSourceChanged(object oldSource, object newSource)
	{
	}

	private static bool IsSourceValid(object o)
	{
		if (o == null || o is IEnumerable || o is IListSource || o is DataSourceProvider)
		{
			return !(o is ICollectionView);
		}
		return false;
	}

	private static bool IsValidSourceForView(object o)
	{
		if (!(o is IEnumerable))
		{
			return o is IListSource;
		}
		return true;
	}

	private static void OnCollectionViewTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		CollectionViewSource obj = (CollectionViewSource)d;
		Type oldCollectionViewType = (Type)e.OldValue;
		Type newCollectionViewType = (Type)e.NewValue;
		if (!obj._isInitializing)
		{
			throw new InvalidOperationException(SR.CollectionViewTypeIsInitOnly);
		}
		obj.OnCollectionViewTypeChanged(oldCollectionViewType, newCollectionViewType);
		obj.EnsureView();
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Data.CollectionViewSource.CollectionViewType" /> property changes.</summary>
	/// <param name="oldCollectionViewType">The old value of the <see cref="P:System.Windows.Data.CollectionViewSource.CollectionViewType" /> property.</param>
	/// <param name="newCollectionViewType">The new value of the <see cref="P:System.Windows.Data.CollectionViewSource.CollectionViewType" /> property.</param>
	protected virtual void OnCollectionViewTypeChanged(Type oldCollectionViewType, Type newCollectionViewType)
	{
	}

	private static bool IsCollectionViewTypeValid(object o)
	{
		Type type = (Type)o;
		if (!(type == null))
		{
			return typeof(ICollectionView).IsAssignableFrom(type);
		}
		return true;
	}

	private static void OnIsLiveSortingRequestedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((CollectionViewSource)d).OnForwardedPropertyChanged();
	}

	private static void OnIsLiveFilteringRequestedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((CollectionViewSource)d).OnForwardedPropertyChanged();
	}

	private static void OnIsLiveGroupingRequestedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((CollectionViewSource)d).OnForwardedPropertyChanged();
	}

	/// <summary>Returns the default view for the given source.</summary>
	/// <returns>Returns an <see cref="T:System.ComponentModel.ICollectionView" /> object that is the default view for the given source collection.</returns>
	/// <param name="source">An object reference to the binding source.</param>
	public static ICollectionView GetDefaultView(object source)
	{
		return GetOriginalView(GetDefaultCollectionView(source, createView: true));
	}

	private static ICollectionView LazyGetDefaultView(object source)
	{
		return GetOriginalView(GetDefaultCollectionView(source, createView: false));
	}

	/// <summary>Returns a value that indicates whether the given view is the default view for the <see cref="P:System.Windows.Data.CollectionViewSource.Source" /> collection.</summary>
	/// <returns>true if the given view is the default view for the <see cref="P:System.Windows.Data.CollectionViewSource.Source" /> collection or if the given view is nulll; otherwise, false.</returns>
	/// <param name="view">The view object to check.</param>
	public static bool IsDefaultView(ICollectionView view)
	{
		if (view != null)
		{
			object sourceCollection = view.SourceCollection;
			return GetOriginalView(view) == LazyGetDefaultView(sourceCollection);
		}
		return true;
	}

	/// <summary>Enters a defer cycle that you can use to merge changes to the view and delay automatic refresh.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that you can use to dispose of the calling object.</returns>
	public IDisposable DeferRefresh()
	{
		return new DeferHelper(this);
	}

	/// <summary>Signals the object that initialization is starting.</summary>
	void ISupportInitialize.BeginInit()
	{
		_isInitializing = true;
	}

	/// <summary>Signals the object that initialization is complete.</summary>
	void ISupportInitialize.EndInit()
	{
		_isInitializing = false;
		EnsureView();
	}

	/// <summary>Receives events from the centralized event manager.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method. This only recognizes manager objects of type <see cref="T:System.Windows.Data.DataChangedEventManager" />.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return ReceiveWeakEvent(managerType, sender, e);
	}

	/// <summary>Handles events from the centralized event table.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method. This only recognizes manager objects of type <see cref="T:System.Windows.Data.DataChangedEventManager" />.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	protected virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	internal static CollectionView GetDefaultCollectionView(object source, bool createView, Func<object, object> GetSourceItem = null)
	{
		if (!IsValidSourceForView(source))
		{
			return null;
		}
		ViewRecord viewRecord = DataBindEngine.CurrentDataBindEngine.GetViewRecord(source, DefaultSource, null, createView, GetSourceItem);
		if (viewRecord == null)
		{
			return null;
		}
		return (CollectionView)viewRecord.View;
	}

	internal static CollectionView GetDefaultCollectionView(object source, DependencyObject d, Func<object, object> GetSourceItem = null)
	{
		CollectionView defaultCollectionView = GetDefaultCollectionView(source, createView: true, GetSourceItem);
		if (defaultCollectionView != null && defaultCollectionView.Culture == null)
		{
			XmlLanguage xmlLanguage = ((d != null) ? ((XmlLanguage)d.GetValue(FrameworkElement.LanguageProperty)) : null);
			if (xmlLanguage != null)
			{
				try
				{
					defaultCollectionView.Culture = xmlLanguage.GetSpecificCulture();
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		return defaultCollectionView;
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (!_hasMultipleInheritanceContexts && _inheritanceContext == null)
		{
			_propertyForInheritanceContext = property;
		}
		else
		{
			_propertyForInheritanceContext = null;
		}
		InheritanceContextHelper.AddInheritanceContext(context, this, ref _hasMultipleInheritanceContexts, ref _inheritanceContext);
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		InheritanceContextHelper.RemoveInheritanceContext(context, this, ref _hasMultipleInheritanceContexts, ref _inheritanceContext);
		_propertyForInheritanceContext = null;
	}

	internal bool IsShareableInTemplate()
	{
		return false;
	}

	private void EnsureView()
	{
		EnsureView(Source, CollectionViewType);
	}

	private void EnsureView(object source, Type collectionViewType)
	{
		if (_isInitializing || _deferLevel > 0)
		{
			return;
		}
		DataSourceProvider dataSourceProvider = source as DataSourceProvider;
		if (dataSourceProvider != _dataProvider)
		{
			if (_dataProvider != null)
			{
				DataChangedEventManager.RemoveHandler(_dataProvider, OnDataChanged);
			}
			_dataProvider = dataSourceProvider;
			if (_dataProvider != null)
			{
				DataChangedEventManager.AddHandler(_dataProvider, OnDataChanged);
				_dataProvider.InitialLoad();
			}
		}
		if (dataSourceProvider != null)
		{
			source = dataSourceProvider.Data;
		}
		ICollectionView collectionView = null;
		if (source != null)
		{
			ViewRecord viewRecord = DataBindEngine.CurrentDataBindEngine.GetViewRecord(source, this, collectionViewType, createView: true, (object x) => BindingOperations.GetBindingExpressionBase(this, SourceProperty)?.GetSourceItem(x));
			if (viewRecord != null)
			{
				collectionView = viewRecord.View;
				_isViewInitialized = viewRecord.IsInitialized;
				if (_version != viewRecord.Version)
				{
					ApplyPropertiesToView(collectionView);
					viewRecord.Version = _version;
				}
			}
		}
		SetValue(ViewPropertyKey, collectionView);
	}

	private void ApplyPropertiesToView(ICollectionView view)
	{
		if (view == null || _deferLevel > 0)
		{
			return;
		}
		ICollectionViewLiveShaping collectionViewLiveShaping = view as ICollectionViewLiveShaping;
		using (view.DeferRefresh())
		{
			if (Culture != null)
			{
				view.Culture = Culture;
			}
			if (view.CanSort)
			{
				view.SortDescriptions.Clear();
				int i = 0;
				for (int count = SortDescriptions.Count; i < count; i++)
				{
					view.SortDescriptions.Add(SortDescriptions[i]);
				}
			}
			else if (SortDescriptions.Count > 0)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotSortView, view));
			}
			Predicate<object> predicate = ((FilterHandlersField.GetValue(this) == null) ? null : FilterWrapper);
			if (view.CanFilter)
			{
				view.Filter = predicate;
			}
			else if (predicate != null)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotFilterView, view));
			}
			if (view.CanGroup)
			{
				view.GroupDescriptions.Clear();
				int i = 0;
				for (int count = GroupDescriptions.Count; i < count; i++)
				{
					view.GroupDescriptions.Add(GroupDescriptions[i]);
				}
			}
			else if (GroupDescriptions.Count > 0)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotGroupView, view));
			}
			if (collectionViewLiveShaping != null)
			{
				if (collectionViewLiveShaping.CanChangeLiveSorting)
				{
					collectionViewLiveShaping.IsLiveSorting = IsLiveSortingRequested;
					ObservableCollection<string> liveSortingProperties = collectionViewLiveShaping.LiveSortingProperties;
					liveSortingProperties.Clear();
					if (IsLiveSortingRequested)
					{
						foreach (string liveSortingProperty in LiveSortingProperties)
						{
							liveSortingProperties.Add(liveSortingProperty);
						}
					}
				}
				CanChangeLiveSorting = collectionViewLiveShaping.CanChangeLiveSorting;
				IsLiveSorting = collectionViewLiveShaping.IsLiveSorting;
				if (collectionViewLiveShaping.CanChangeLiveFiltering)
				{
					collectionViewLiveShaping.IsLiveFiltering = IsLiveFilteringRequested;
					ObservableCollection<string> liveSortingProperties = collectionViewLiveShaping.LiveFilteringProperties;
					liveSortingProperties.Clear();
					if (IsLiveFilteringRequested)
					{
						foreach (string liveFilteringProperty in LiveFilteringProperties)
						{
							liveSortingProperties.Add(liveFilteringProperty);
						}
					}
				}
				CanChangeLiveFiltering = collectionViewLiveShaping.CanChangeLiveFiltering;
				IsLiveFiltering = collectionViewLiveShaping.IsLiveFiltering;
				if (collectionViewLiveShaping.CanChangeLiveGrouping)
				{
					collectionViewLiveShaping.IsLiveGrouping = IsLiveGroupingRequested;
					ObservableCollection<string> liveSortingProperties = collectionViewLiveShaping.LiveGroupingProperties;
					liveSortingProperties.Clear();
					if (IsLiveGroupingRequested)
					{
						foreach (string liveGroupingProperty in LiveGroupingProperties)
						{
							liveSortingProperties.Add(liveGroupingProperty);
						}
					}
				}
				CanChangeLiveGrouping = collectionViewLiveShaping.CanChangeLiveGrouping;
				IsLiveGrouping = collectionViewLiveShaping.IsLiveGrouping;
			}
			else
			{
				CanChangeLiveSorting = false;
				IsLiveSorting = null;
				CanChangeLiveFiltering = false;
				IsLiveFiltering = null;
				CanChangeLiveGrouping = false;
				IsLiveGrouping = null;
			}
		}
	}

	private static ICollectionView GetOriginalView(ICollectionView view)
	{
		for (CollectionViewProxy collectionViewProxy = view as CollectionViewProxy; collectionViewProxy != null; collectionViewProxy = view as CollectionViewProxy)
		{
			view = collectionViewProxy.ProxiedView;
		}
		return view;
	}

	private bool WrapFilter(object item)
	{
		FilterEventArgs filterEventArgs = new FilterEventArgs(item);
		FilterHandlersField.GetValue(this)?.Invoke(this, filterEventArgs);
		return filterEventArgs.Accepted;
	}

	private void OnDataChanged(object sender, EventArgs e)
	{
		EnsureView();
	}

	private void OnForwardedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		OnForwardedPropertyChanged();
	}

	private void OnForwardedPropertyChanged()
	{
		_version++;
		ApplyPropertiesToView(View);
	}

	private void BeginDefer()
	{
		_deferLevel++;
	}

	private void EndDefer()
	{
		if (--_deferLevel == 0)
		{
			EnsureView();
		}
	}
}
