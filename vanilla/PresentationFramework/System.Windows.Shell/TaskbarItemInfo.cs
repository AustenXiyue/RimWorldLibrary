using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Shell;

/// <summary>Represents information about how the taskbar thumbnail is displayed.</summary>
public sealed class TaskbarItemInfo : Freezable
{
	/// <summary>Identifies the <see cref="P:System.Windows.Shell.TaskbarItemInfo.ProgressState" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.TaskbarItemInfo.ProgressState" /> dependency property.</returns>
	public static readonly DependencyProperty ProgressStateProperty = DependencyProperty.Register("ProgressState", typeof(TaskbarItemProgressState), typeof(TaskbarItemInfo), new PropertyMetadata(TaskbarItemProgressState.None, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TaskbarItemInfo)d).NotifyDependencyPropertyChanged(e);
	}, (DependencyObject d, object baseValue) => ((TaskbarItemInfo)d).CoerceProgressState((TaskbarItemProgressState)baseValue)));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.TaskbarItemInfo.ProgressValue" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.TaskbarItemInfo.ProgressValue" /> dependency property.</returns>
	public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register("ProgressValue", typeof(double), typeof(TaskbarItemInfo), new PropertyMetadata(0.0, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TaskbarItemInfo)d).NotifyDependencyPropertyChanged(e);
	}, (DependencyObject d, object baseValue) => CoerceProgressValue((double)baseValue)));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.TaskbarItemInfo.Overlay" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.TaskbarItemInfo.Overlay" /> dependency property.</returns>
	public static readonly DependencyProperty OverlayProperty = DependencyProperty.Register("Overlay", typeof(ImageSource), typeof(TaskbarItemInfo), new PropertyMetadata(null, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TaskbarItemInfo)d).NotifyDependencyPropertyChanged(e);
	}));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.TaskbarItemInfo.Description" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.TaskbarItemInfo.Description" /> dependency property.</returns>
	public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(TaskbarItemInfo), new PropertyMetadata(string.Empty, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TaskbarItemInfo)d).NotifyDependencyPropertyChanged(e);
	}));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.TaskbarItemInfo.ThumbnailClipMargin" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.TaskbarItemInfo.ThumbnailClipMargin" /> dependency property.</returns>
	public static readonly DependencyProperty ThumbnailClipMarginProperty = DependencyProperty.Register("ThumbnailClipMargin", typeof(Thickness), typeof(TaskbarItemInfo), new PropertyMetadata(default(Thickness), delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TaskbarItemInfo)d).NotifyDependencyPropertyChanged(e);
	}, (DependencyObject d, object baseValue) => ((TaskbarItemInfo)d).CoerceThumbnailClipMargin((Thickness)baseValue)));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.TaskbarItemInfo.ThumbButtonInfos" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.TaskbarItemInfo.ThumbButtonInfos" /> dependency property.</returns>
	public static readonly DependencyProperty ThumbButtonInfosProperty = DependencyProperty.Register("ThumbButtonInfos", typeof(ThumbButtonInfoCollection), typeof(TaskbarItemInfo), new PropertyMetadata(new FreezableDefaultValueFactory(ThumbButtonInfoCollection.Empty), delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TaskbarItemInfo)d).NotifyDependencyPropertyChanged(e);
	}));

	/// <summary>Gets or sets a value that indicates how the progress indicator is displayed in the taskbar button.</summary>
	/// <returns>An enumeration value that indicates how the progress indicator is displayed in the taskbar button. The default is <see cref="F:System.Windows.Shell.TaskbarItemProgressState.None" />.</returns>
	public TaskbarItemProgressState ProgressState
	{
		get
		{
			return (TaskbarItemProgressState)GetValue(ProgressStateProperty);
		}
		set
		{
			SetValue(ProgressStateProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the fullness of the progress indicator in the taskbar button.</summary>
	/// <returns>A value that indicates the fullness of the progress indicator in the taskbar button. The default is 0.</returns>
	public double ProgressValue
	{
		get
		{
			return (double)GetValue(ProgressValueProperty);
		}
		set
		{
			SetValue(ProgressValueProperty, value);
		}
	}

	/// <summary>Gets or sets the image that is displayed over the program icon in the taskbar button.</summary>
	/// <returns>The image that is displayed over the program icon in the taskbar button. The default is null.</returns>
	public ImageSource Overlay
	{
		get
		{
			return (ImageSource)GetValue(OverlayProperty);
		}
		set
		{
			SetValue(OverlayProperty, value);
		}
	}

	/// <summary>Gets or sets the text for the taskbar item tooltip.</summary>
	/// <returns>The text for the taskbar item tooltip. The default is an empty string.</returns>
	public string Description
	{
		get
		{
			return (string)GetValue(DescriptionProperty);
		}
		set
		{
			SetValue(DescriptionProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the part of the application window's client area that is displayed in the taskbar thumbnail.</summary>
	/// <returns>A value that specifies the part of the application window's client area that is displayed in the taskbar thumbnail. The default is an empty <see cref="T:System.Windows.Thickness" />.</returns>
	public Thickness ThumbnailClipMargin
	{
		get
		{
			return (Thickness)GetValue(ThumbnailClipMarginProperty);
		}
		set
		{
			SetValue(ThumbnailClipMarginProperty, value);
		}
	}

	/// <summary>Gets or sets the collection of <see cref="T:System.Windows.Shell.ThumbButtonInfo" /> objects that are associated with the <see cref="T:System.Windows.Window" />.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.Shell.ThumbButtonInfo" /> objects that are associated with the <see cref="T:System.Windows.Window" />. The default is an empty collection.</returns>
	public ThumbButtonInfoCollection ThumbButtonInfos
	{
		get
		{
			return (ThumbButtonInfoCollection)GetValue(ThumbButtonInfosProperty);
		}
		set
		{
			SetValue(ThumbButtonInfosProperty, value);
		}
	}

	internal event DependencyPropertyChangedEventHandler PropertyChanged;

	protected override Freezable CreateInstanceCore()
	{
		return new TaskbarItemInfo();
	}

	private TaskbarItemProgressState CoerceProgressState(TaskbarItemProgressState value)
	{
		if ((uint)value > 4u)
		{
			value = TaskbarItemProgressState.None;
		}
		return value;
	}

	private static double CoerceProgressValue(double progressValue)
	{
		if (double.IsNaN(progressValue))
		{
			progressValue = 0.0;
		}
		else
		{
			progressValue = Math.Max(progressValue, 0.0);
			progressValue = Math.Min(1.0, progressValue);
		}
		return progressValue;
	}

	private Thickness CoerceThumbnailClipMargin(Thickness margin)
	{
		if (!margin.IsValid(allowNegative: false, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false))
		{
			return default(Thickness);
		}
		return margin;
	}

	private void NotifyDependencyPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.TaskbarItemInfo" /> class.</summary>
	public TaskbarItemInfo()
	{
	}
}
