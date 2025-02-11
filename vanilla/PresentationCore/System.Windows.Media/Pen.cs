using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media;

/// <summary> Describes how a shape is outlined. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class Pen : Animatable, DUCE.IResource
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.Pen.Brush" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Pen.Brush" /> dependency property.</returns>
	public static readonly DependencyProperty BrushProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Pen.Thickness" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Pen.Thickness" /> dependency property.</returns>
	public static readonly DependencyProperty ThicknessProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Pen.StartLineCap" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Pen.StartLineCap" /> dependency property.</returns>
	public static readonly DependencyProperty StartLineCapProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Pen.EndLineCap" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Pen.EndLineCap" /> dependency property.</returns>
	public static readonly DependencyProperty EndLineCapProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Pen.DashCap" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Pen.DashCap" /> dependency property.</returns>
	public static readonly DependencyProperty DashCapProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Pen.LineJoin" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Pen.LineJoin" /> dependency property.</returns>
	public static readonly DependencyProperty LineJoinProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Pen.MiterLimit" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Pen.MiterLimit" /> dependency property.</returns>
	public static readonly DependencyProperty MiterLimitProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Pen.DashStyle" /> dependency property.</summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Pen.DashStyle" /> dependency property.</returns>
	public static readonly DependencyProperty DashStyleProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_Thickness = 1.0;

	internal const PenLineCap c_StartLineCap = PenLineCap.Flat;

	internal const PenLineCap c_EndLineCap = PenLineCap.Flat;

	internal const PenLineCap c_DashCap = PenLineCap.Square;

	internal const PenLineJoin c_LineJoin = PenLineJoin.Miter;

	internal const double c_MiterLimit = 10.0;

	internal static DashStyle s_DashStyle;

	internal bool DoesNotContainGaps
	{
		get
		{
			DashStyle dashStyle = DashStyle;
			if (dashStyle != null)
			{
				DoubleCollection dashes = dashStyle.Dashes;
				if (dashes != null && dashes.Count > 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary> Gets or sets the fill the outline produced by this <see cref="T:System.Windows.Media.Pen" />.  </summary>
	/// <returns>The fill of the outline produced by this <see cref="T:System.Windows.Media.Pen" />. The default value is null.</returns>
	public Brush Brush
	{
		get
		{
			return (Brush)GetValue(BrushProperty);
		}
		set
		{
			SetValueInternal(BrushProperty, value);
		}
	}

	/// <summary> Gets or sets the thickness of the stroke produced by this <see cref="T:System.Windows.Media.Pen" />.  </summary>
	/// <returns>The thickness of the stroke produced by this <see cref="T:System.Windows.Media.Pen" />. Default is 1.</returns>
	public double Thickness
	{
		get
		{
			return (double)GetValue(ThicknessProperty);
		}
		set
		{
			SetValueInternal(ThicknessProperty, value);
		}
	}

	/// <summary> Gets or sets the type of shape to use at the beginning of a stroke.  </summary>
	/// <returns>The type of shape that starts the stroke. The default value is <see cref="F:System.Windows.Media.PenLineCap.Flat" />.</returns>
	public PenLineCap StartLineCap
	{
		get
		{
			return (PenLineCap)GetValue(StartLineCapProperty);
		}
		set
		{
			SetValueInternal(StartLineCapProperty, value);
		}
	}

	/// <summary> Gets or sets the type of shape to use at the end of a stroke.  </summary>
	/// <returns>The type of shape that ends the stroke. The default value is <see cref="F:System.Windows.Media.PenLineCap.Flat" />.</returns>
	public PenLineCap EndLineCap
	{
		get
		{
			return (PenLineCap)GetValue(EndLineCapProperty);
		}
		set
		{
			SetValueInternal(EndLineCapProperty, value);
		}
	}

	/// <summary> Gets or sets a value that specifies how the ends of each dash are drawn.  </summary>
	/// <returns>Specifies how the ends of each dash are drawn.  This setting applies to both ends of each dash. The default value is <see cref="F:System.Windows.Media.PenLineCap.Square" />.</returns>
	public PenLineCap DashCap
	{
		get
		{
			return (PenLineCap)GetValue(DashCapProperty);
		}
		set
		{
			SetValueInternal(DashCapProperty, value);
		}
	}

	/// <summary> Gets or sets the type of joint used at the vertices of a shape's outline.   </summary>
	/// <returns>The type of joint used at the vertices of a shape's outline. The default value is <see cref="F:System.Windows.Media.PenLineJoin.Miter" />.</returns>
	public PenLineJoin LineJoin
	{
		get
		{
			return (PenLineJoin)GetValue(LineJoinProperty);
		}
		set
		{
			SetValueInternal(LineJoinProperty, value);
		}
	}

	/// <summary> Gets or sets the limit on the ratio of the miter length to half this pen's <see cref="P:System.Windows.Media.Pen.Thickness" />.  </summary>
	/// <returns>The limit on the ratio of the miter length to half the pen's <see cref="P:System.Windows.Media.Pen.Thickness" />. This value is always a positive number greater than or equal to 1.  The default value is 10.0.</returns>
	public double MiterLimit
	{
		get
		{
			return (double)GetValue(MiterLimitProperty);
		}
		set
		{
			SetValueInternal(MiterLimitProperty, value);
		}
	}

	/// <summary>Gets or sets a value that describes the pattern of dashes generated by this <see cref="T:System.Windows.Media.Pen" />.  </summary>
	/// <returns>A value that describes the pattern of dashes generated by this <see cref="T:System.Windows.Media.Pen" />. The default is <see cref="P:System.Windows.Media.DashStyles.Solid" />, which indicates that there should be no dashes.</returns>
	public DashStyle DashStyle
	{
		get
		{
			return (DashStyle)GetValue(DashStyleProperty);
		}
		set
		{
			SetValueInternal(DashStyleProperty, value);
		}
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Pen" /> class. </summary>
	public Pen()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Pen" /> class with the specified <see cref="T:System.Windows.Media.Brush" /> and thickness. </summary>
	/// <param name="brush">The Brush for this Pen.</param>
	/// <param name="thickness">The thickness of the Pen. </param>
	public Pen(Brush brush, double thickness)
	{
		Brush = brush;
		Thickness = thickness;
	}

	internal Pen(Brush brush, double thickness, PenLineCap startLineCap, PenLineCap endLineCap, PenLineCap dashCap, PenLineJoin lineJoin, double miterLimit, DashStyle dashStyle)
	{
		Thickness = thickness;
		StartLineCap = startLineCap;
		EndLineCap = endLineCap;
		DashCap = dashCap;
		LineJoin = lineJoin;
		MiterLimit = miterLimit;
		Brush = brush;
		DashStyle = dashStyle;
	}

	private MIL_PEN_CAP GetInternalCapType(PenLineCap cap)
	{
		return (MIL_PEN_CAP)cap;
	}

	private MIL_PEN_JOIN GetInternalJoinType(PenLineJoin join)
	{
		return (MIL_PEN_JOIN)join;
	}

	internal unsafe void GetBasicPenData(MIL_PEN_DATA* pData, out double[] dashArray)
	{
		dashArray = null;
		Invariant.Assert(pData != null);
		pData->Thickness = Thickness;
		pData->StartLineCap = GetInternalCapType(StartLineCap);
		pData->EndLineCap = GetInternalCapType(EndLineCap);
		pData->DashCap = GetInternalCapType(DashCap);
		pData->LineJoin = GetInternalJoinType(LineJoin);
		pData->MiterLimit = MiterLimit;
		if (DashStyle != null)
		{
			DashStyle.GetDashData(pData, out dashArray);
		}
	}

	internal static bool ContributesToBounds(Pen pen)
	{
		if (pen != null)
		{
			return pen.Brush != null;
		}
		return false;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Pen" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Pen Clone()
	{
		return (Pen)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Pen" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Pen CloneCurrentValue()
	{
		return (Pen)base.CloneCurrentValue();
	}

	private static void BrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Pen pen = (Pen)d;
		Brush resource = (Brush)e.OldValue;
		Brush resource2 = (Brush)e.NewValue;
		if (pen.Dispatcher != null)
		{
			DUCE.IResource resource3 = pen;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					pen.ReleaseResource(resource, channel);
					pen.AddRefResource(resource2, channel);
				}
			}
		}
		pen.PropertyChanged(BrushProperty);
	}

	private static void ThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Pen)d).PropertyChanged(ThicknessProperty);
	}

	private static void StartLineCapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Pen)d).PropertyChanged(StartLineCapProperty);
	}

	private static void EndLineCapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Pen)d).PropertyChanged(EndLineCapProperty);
	}

	private static void DashCapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Pen)d).PropertyChanged(DashCapProperty);
	}

	private static void LineJoinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Pen)d).PropertyChanged(LineJoinProperty);
	}

	private static void MiterLimitPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Pen)d).PropertyChanged(MiterLimitProperty);
	}

	private static void DashStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Pen pen = (Pen)d;
		DashStyle resource = (DashStyle)e.OldValue;
		DashStyle resource2 = (DashStyle)e.NewValue;
		if (pen.Dispatcher != null)
		{
			DUCE.IResource resource3 = pen;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					pen.ReleaseResource(resource, channel);
					pen.AddRefResource(resource2, channel);
				}
			}
		}
		pen.PropertyChanged(DashStyleProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new Pen();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Brush brush = Brush;
			DashStyle dashStyle = DashStyle;
			DUCE.ResourceHandle hBrush = ((DUCE.IResource)brush)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hDashStyle = ((DUCE.IResource)dashStyle)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(ThicknessProperty, channel);
			DUCE.MILCMD_PEN mILCMD_PEN = default(DUCE.MILCMD_PEN);
			mILCMD_PEN.Type = MILCMD.MilCmdPen;
			mILCMD_PEN.Handle = _duceResource.GetHandle(channel);
			mILCMD_PEN.hBrush = hBrush;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_PEN.Thickness = Thickness;
			}
			mILCMD_PEN.hThicknessAnimations = animationResourceHandle;
			mILCMD_PEN.StartLineCap = StartLineCap;
			mILCMD_PEN.EndLineCap = EndLineCap;
			mILCMD_PEN.DashCap = DashCap;
			mILCMD_PEN.LineJoin = LineJoin;
			mILCMD_PEN.MiterLimit = MiterLimit;
			mILCMD_PEN.hDashStyle = hDashStyle;
			channel.SendCommand((byte*)(&mILCMD_PEN), sizeof(DUCE.MILCMD_PEN));
		}
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_PEN))
			{
				((DUCE.IResource)Brush)?.AddRefOnChannel(channel);
				((DUCE.IResource)DashStyle)?.AddRefOnChannel(channel);
				AddRefOnChannelAnimations(channel);
				UpdateResource(channel, skipOnChannelCheck: true);
			}
			return _duceResource.GetHandle(channel);
		}
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.ReleaseOnChannel(channel))
			{
				((DUCE.IResource)Brush)?.ReleaseOnChannel(channel);
				((DUCE.IResource)DashStyle)?.ReleaseOnChannel(channel);
				ReleaseOnChannelAnimations(channel);
			}
		}
	}

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return _duceResource.GetHandle(channel);
		}
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _duceResource.GetChannelCount();
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static Pen()
	{
		s_DashStyle = DashStyles.Solid;
		Type typeFromHandle = typeof(Pen);
		BrushProperty = Animatable.RegisterProperty("Brush", typeof(Brush), typeFromHandle, null, BrushPropertyChanged, null, isIndependentlyAnimated: false, null);
		ThicknessProperty = Animatable.RegisterProperty("Thickness", typeof(double), typeFromHandle, 1.0, ThicknessPropertyChanged, null, isIndependentlyAnimated: true, null);
		StartLineCapProperty = Animatable.RegisterProperty("StartLineCap", typeof(PenLineCap), typeFromHandle, PenLineCap.Flat, StartLineCapPropertyChanged, ValidateEnums.IsPenLineCapValid, isIndependentlyAnimated: false, null);
		EndLineCapProperty = Animatable.RegisterProperty("EndLineCap", typeof(PenLineCap), typeFromHandle, PenLineCap.Flat, EndLineCapPropertyChanged, ValidateEnums.IsPenLineCapValid, isIndependentlyAnimated: false, null);
		DashCapProperty = Animatable.RegisterProperty("DashCap", typeof(PenLineCap), typeFromHandle, PenLineCap.Square, DashCapPropertyChanged, ValidateEnums.IsPenLineCapValid, isIndependentlyAnimated: false, null);
		LineJoinProperty = Animatable.RegisterProperty("LineJoin", typeof(PenLineJoin), typeFromHandle, PenLineJoin.Miter, LineJoinPropertyChanged, ValidateEnums.IsPenLineJoinValid, isIndependentlyAnimated: false, null);
		MiterLimitProperty = Animatable.RegisterProperty("MiterLimit", typeof(double), typeFromHandle, 10.0, MiterLimitPropertyChanged, null, isIndependentlyAnimated: false, null);
		DashStyleProperty = Animatable.RegisterProperty("DashStyle", typeof(DashStyle), typeFromHandle, DashStyles.Solid, DashStylePropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
