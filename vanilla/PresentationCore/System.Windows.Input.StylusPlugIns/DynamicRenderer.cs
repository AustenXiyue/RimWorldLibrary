using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.Ink;
using MS.Internal.PresentationCore;

namespace System.Windows.Input.StylusPlugIns;

/// <summary>Draws ink on a surface as the user moves the tablet pen.</summary>
public class DynamicRenderer : StylusPlugIn
{
	private class StrokeInfo
	{
		private int _stylusId;

		private int _startTime;

		private int _lastTime;

		private ContainerVisual _strokeCV;

		private ContainerVisual _strokeRTICV;

		private bool _seenUp;

		private bool _isReset;

		private SolidColorBrush _fillBrush;

		private DrawingAttributes _drawingAttributes;

		private StrokeNodeIterator _strokeNodeIterator;

		private double _opacity;

		private DynamicRendererHostVisual _strokeHV;

		public int StylusId => _stylusId;

		public int StartTime => _startTime;

		public int LastTime
		{
			get
			{
				return _lastTime;
			}
			set
			{
				_lastTime = value;
			}
		}

		public ContainerVisual StrokeCV
		{
			get
			{
				return _strokeCV;
			}
			set
			{
				_strokeCV = value;
			}
		}

		public ContainerVisual StrokeRTICV
		{
			get
			{
				return _strokeRTICV;
			}
			set
			{
				_strokeRTICV = value;
			}
		}

		public bool SeenUp
		{
			get
			{
				return _seenUp;
			}
			set
			{
				_seenUp = value;
			}
		}

		public bool IsReset
		{
			get
			{
				return _isReset;
			}
			set
			{
				_isReset = value;
			}
		}

		public StrokeNodeIterator StrokeNodeIterator
		{
			get
			{
				return _strokeNodeIterator;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("StrokeNodeIterator");
				}
				_strokeNodeIterator = value;
			}
		}

		public SolidColorBrush FillBrush
		{
			get
			{
				return _fillBrush;
			}
			set
			{
				_fillBrush = value;
			}
		}

		public DrawingAttributes DrawingAttributes => _drawingAttributes;

		public double Opacity => _opacity;

		public DynamicRendererHostVisual StrokeHV => _strokeHV;

		public StrokeInfo(DrawingAttributes drawingAttributes, int stylusDeviceId, int startTimestamp, DynamicRendererHostVisual hostVisual)
		{
			_stylusId = stylusDeviceId;
			_startTime = startTimestamp;
			_lastTime = _startTime;
			_drawingAttributes = drawingAttributes.Clone();
			_strokeNodeIterator = new StrokeNodeIterator(_drawingAttributes);
			Color color = _drawingAttributes.Color;
			_opacity = (_drawingAttributes.IsHighlighter ? 0.0 : ((double)(int)color.A / 255.0));
			color.A = byte.MaxValue;
			SolidColorBrush solidColorBrush = new SolidColorBrush(color);
			solidColorBrush.Freeze();
			_fillBrush = solidColorBrush;
			_strokeHV = hostVisual;
			hostVisual.AddStrokeInfoRef(this);
		}

		public bool IsTimestampWithin(int timestamp)
		{
			if (SeenUp)
			{
				if (StartTime < LastTime)
				{
					if (timestamp >= StartTime)
					{
						return timestamp <= LastTime;
					}
					return false;
				}
				if (timestamp < StartTime)
				{
					return timestamp <= LastTime;
				}
				return true;
			}
			return true;
		}

		public bool IsTimestampAfter(int timestamp)
		{
			if (!SeenUp)
			{
				if (LastTime >= StartTime)
				{
					if (timestamp >= LastTime)
					{
						return true;
					}
					if (LastTime > 0)
					{
						return timestamp < 0;
					}
					return false;
				}
				if (timestamp >= LastTime)
				{
					return timestamp <= StartTime;
				}
				return false;
			}
			return false;
		}
	}

	private class DynamicRendererHostVisual : HostVisual
	{
		private VisualTarget _visualTarget;

		private List<StrokeInfo> _strokeInfoList = new List<StrokeInfo>();

		internal bool InUse => _strokeInfoList.Count > 0;

		internal bool HasSingleReference => _strokeInfoList.Count == 1;

		internal VisualTarget VisualTarget
		{
			get
			{
				if (_visualTarget == null)
				{
					_visualTarget = new VisualTarget(this);
					_visualTarget.RootVisual = new ContainerVisual();
				}
				return _visualTarget;
			}
		}

		internal void AddStrokeInfoRef(StrokeInfo si)
		{
			_strokeInfoList.Add(si);
		}

		internal void RemoveStrokeInfoRef(StrokeInfo si)
		{
			_strokeInfoList.Remove(si);
		}
	}

	private Dispatcher _applicationDispatcher;

	private Geometry _zeroSizedFrozenRect;

	private DrawingAttributes _drawAttrsSource = new DrawingAttributes();

	private List<StrokeInfo> _strokeInfoList = new List<StrokeInfo>();

	private ContainerVisual _mainContainerVisual;

	private ContainerVisual _mainRawInkContainerVisual;

	private DynamicRendererHostVisual _rawInkHostVisual1;

	private DynamicRendererHostVisual _rawInkHostVisual2;

	private DynamicRendererHostVisual _currentHostVisual;

	private EventHandler _onRenderComplete;

	private bool _waitingForRenderComplete;

	private readonly object __siLock = new object();

	private StrokeInfo _renderCompleteStrokeInfo;

	private DynamicRendererThreadManager _renderingThread;

	private EventHandler _onDRThreadRenderComplete;

	private bool _waitingForDRThreadRenderComplete;

	private Queue<StrokeInfo> _renderCompleteDRThreadStrokeInfoList = new Queue<StrokeInfo>();

	/// <summary>Gets the root visual for the <see cref="T:System.Windows.Input.StylusPlugIns.DynamicRenderer" />.</summary>
	/// <returns>The root <see cref="T:System.Windows.Media.Visual" /> for the <see cref="T:System.Windows.Input.StylusPlugIns.DynamicRenderer" />.</returns>
	public Visual RootVisual
	{
		get
		{
			if (_mainContainerVisual == null)
			{
				CreateInkingVisuals();
			}
			return _mainContainerVisual;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Ink.DrawingAttributes" /> that specifies the appearance of the rendered ink.</summary>
	/// <returns>The <see cref="T:System.Windows.Ink.DrawingAttributes" /> that specifies the appearance of the rendered ink.</returns>
	public DrawingAttributes DrawingAttributes
	{
		get
		{
			return _drawAttrsSource;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_drawAttrsSource = value;
			OnDrawingAttributesReplaced();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPlugIns.DynamicRenderer" /> class. </summary>
	public DynamicRenderer()
	{
		_zeroSizedFrozenRect = new RectangleGeometry(new Rect(0.0, 0.0, 0.0, 0.0));
		_zeroSizedFrozenRect.Freeze();
	}

	/// <summary>Clears rendering on the current stroke and redraws it.</summary>
	/// <param name="stylusDevice">The current stylus device.</param>
	/// <param name="stylusPoints">The stylus points to be redrawn.</param>
	/// <exception cref="T:System.ArgumentException">Neither the stylus nor the mouse is in the down state.</exception>
	public virtual void Reset(StylusDevice stylusDevice, StylusPointCollection stylusPoints)
	{
		if (_mainContainerVisual == null || _applicationDispatcher == null || !base.IsActiveForInput)
		{
			return;
		}
		_applicationDispatcher.VerifyAccess();
		if (stylusDevice?.InAir ?? (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Released))
		{
			throw new ArgumentException(SR.Stylus_MustBeDownToCallReset, "stylusDevice");
		}
		using (_applicationDispatcher.DisableProcessing())
		{
			lock (__siLock)
			{
				AbortAllStrokes();
				StrokeInfo strokeInfo = new StrokeInfo(DrawingAttributes, stylusDevice?.Id ?? 0, Environment.TickCount, GetCurrentHostVisual());
				_strokeInfoList.Add(strokeInfo);
				strokeInfo.IsReset = true;
				if (stylusPoints != null)
				{
					RenderPackets(stylusPoints, strokeInfo);
				}
			}
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.StylusPlugIns.DynamicRenderer" /> is added to an element.</summary>
	protected override void OnAdded()
	{
		_applicationDispatcher = base.Element.Dispatcher;
		if (base.IsActiveForInput)
		{
			CreateRealTimeVisuals();
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.DynamicRenderer.StylusPlugIn" /> is removed from an element.</summary>
	protected override void OnRemoved()
	{
		DestroyRealTimeVisuals();
		_applicationDispatcher = null;
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Input.StylusPlugIns.DynamicRenderer.IsActiveForInput" /> property changes.</summary>
	protected override void OnIsActiveForInputChanged()
	{
		if (base.IsActiveForInput)
		{
			CreateRealTimeVisuals();
		}
		else
		{
			DestroyRealTimeVisuals();
		}
	}

	/// <summary>Occurs on a pen thread when the cursor enters the bounds of an element.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	/// <param name="confirmed">true if the pen actually entered the bounds of the element; otherwise, false.</param>
	protected override void OnStylusEnter(RawStylusInput rawStylusInput, bool confirmed)
	{
		HandleStylusEnterLeave(rawStylusInput, isEnter: true, confirmed);
	}

	/// <summary>Occurs on a pen thread when the cursor leaves the bounds of an element.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	/// <param name="confirmed">true if the pen actually left the bounds of the element; otherwise, false.</param>
	protected override void OnStylusLeave(RawStylusInput rawStylusInput, bool confirmed)
	{
		HandleStylusEnterLeave(rawStylusInput, isEnter: false, confirmed);
	}

	private void HandleStylusEnterLeave(RawStylusInput rawStylusInput, bool isEnter, bool isConfirmed)
	{
		if (isConfirmed)
		{
			StrokeInfo strokeInfo = FindStrokeInfo(rawStylusInput.Timestamp);
			if (strokeInfo != null && rawStylusInput.StylusDeviceId == strokeInfo.StylusId && ((isEnter && rawStylusInput.Timestamp > strokeInfo.StartTime) || (!isEnter && !strokeInfo.SeenUp)))
			{
				TransitionStrokeVisuals(strokeInfo, abortStroke: true);
			}
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Input.StylusPlugIns.StylusPlugIn.Enabled" /> property changes.</summary>
	protected override void OnEnabledChanged()
	{
		if (!base.Enabled)
		{
			AbortAllStrokes();
		}
	}

	/// <summary>Occurs on a thread in the pen thread pool when the tablet pen touches the digitizer.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	protected override void OnStylusDown(RawStylusInput rawStylusInput)
	{
		if (_mainContainerVisual == null)
		{
			return;
		}
		StrokeInfo strokeInfo;
		lock (__siLock)
		{
			strokeInfo = FindStrokeInfo(rawStylusInput.Timestamp);
			if (strokeInfo != null)
			{
				return;
			}
			strokeInfo = new StrokeInfo(DrawingAttributes, rawStylusInput.StylusDeviceId, rawStylusInput.Timestamp, GetCurrentHostVisual());
			_strokeInfoList.Add(strokeInfo);
		}
		rawStylusInput.NotifyWhenProcessed(strokeInfo);
		RenderPackets(rawStylusInput.GetStylusPoints(), strokeInfo);
	}

	/// <summary>Occurs on a pen thread when the tablet pen moves on the digitizer.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	protected override void OnStylusMove(RawStylusInput rawStylusInput)
	{
		if (_mainContainerVisual != null)
		{
			StrokeInfo strokeInfo = FindStrokeInfo(rawStylusInput.Timestamp);
			if (strokeInfo != null && strokeInfo.StylusId == rawStylusInput.StylusDeviceId && strokeInfo.IsTimestampAfter(rawStylusInput.Timestamp))
			{
				strokeInfo.LastTime = rawStylusInput.Timestamp;
				RenderPackets(rawStylusInput.GetStylusPoints(), strokeInfo);
			}
		}
	}

	/// <summary>Occurs on a pen thread when the user lifts the tablet pen from the digitizer.</summary>
	/// <param name="rawStylusInput">A <see cref="T:System.Windows.Input.StylusPlugIns.RawStylusInput" /> that contains information about input from the pen.</param>
	protected override void OnStylusUp(RawStylusInput rawStylusInput)
	{
		if (_mainContainerVisual != null)
		{
			StrokeInfo strokeInfo = FindStrokeInfo(rawStylusInput.Timestamp);
			if (strokeInfo != null && (strokeInfo.StylusId == rawStylusInput.StylusDeviceId || (rawStylusInput.StylusDeviceId == 0 && (strokeInfo.IsReset || (strokeInfo.IsTimestampAfter(rawStylusInput.Timestamp) && IsStylusUp(strokeInfo.StylusId))))))
			{
				strokeInfo.SeenUp = true;
				strokeInfo.LastTime = rawStylusInput.Timestamp;
				rawStylusInput.NotifyWhenProcessed(strokeInfo);
			}
		}
	}

	private bool IsStylusUp(int stylusId)
	{
		TabletDeviceCollection tabletDevices = Tablet.TabletDevices;
		for (int i = 0; i < tabletDevices.Count; i++)
		{
			TabletDevice tabletDevice = tabletDevices[i];
			for (int j = 0; j < tabletDevice.StylusDevices.Count; j++)
			{
				StylusDevice stylusDevice = tabletDevice.StylusDevices[j];
				if (stylusId == stylusDevice.Id)
				{
					return stylusDevice.InAir;
				}
			}
		}
		return true;
	}

	private void OnRenderComplete()
	{
		StrokeInfo renderCompleteStrokeInfo = _renderCompleteStrokeInfo;
		if (renderCompleteStrokeInfo != null)
		{
			if (renderCompleteStrokeInfo.StrokeHV.Clip == null)
			{
				TransitionComplete(renderCompleteStrokeInfo);
				_renderCompleteStrokeInfo = null;
			}
			else
			{
				RemoveDynamicRendererVisualAndNotifyWhenDone(renderCompleteStrokeInfo);
			}
		}
	}

	private void RemoveDynamicRendererVisualAndNotifyWhenDone(StrokeInfo si)
	{
		if (si == null)
		{
			return;
		}
		DynamicRendererThreadManager renderingThread = _renderingThread;
		if (renderingThread == null)
		{
			return;
		}
		renderingThread.ThreadDispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
		{
			if (si.StrokeRTICV != null)
			{
				if (_onDRThreadRenderComplete == null)
				{
					_onDRThreadRenderComplete = OnDRThreadRenderComplete;
				}
				_renderCompleteDRThreadStrokeInfoList.Enqueue(si);
				if (!_waitingForDRThreadRenderComplete)
				{
					((ContainerVisual)si.StrokeHV.VisualTarget.RootVisual).Children.Remove(si.StrokeRTICV);
					si.StrokeRTICV = null;
					MediaContext.From(renderingThread.ThreadDispatcher).RenderComplete += _onDRThreadRenderComplete;
					_waitingForDRThreadRenderComplete = true;
				}
			}
			else
			{
				NotifyAppOfDRThreadRenderComplete(si);
			}
			return (object)null;
		}, null);
	}

	private void NotifyAppOfDRThreadRenderComplete(StrokeInfo si)
	{
		_applicationDispatcher?.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
		{
			if (si == _renderCompleteStrokeInfo)
			{
				if (si.StrokeHV.Clip != null)
				{
					si.StrokeHV.Clip = null;
					NotifyOnNextRenderComplete();
				}
				else
				{
					TransitionComplete(si);
				}
			}
			else
			{
				TransitionComplete(si);
			}
			return (object)null;
		}, null);
	}

	private void OnDRThreadRenderComplete(object sender, EventArgs e)
	{
		DynamicRendererThreadManager renderingThread = _renderingThread;
		Dispatcher dispatcher = null;
		if (renderingThread == null)
		{
			return;
		}
		dispatcher = renderingThread.ThreadDispatcher;
		if (dispatcher == null)
		{
			return;
		}
		if (_renderCompleteDRThreadStrokeInfoList.Count > 0)
		{
			StrokeInfo si = _renderCompleteDRThreadStrokeInfoList.Dequeue();
			NotifyAppOfDRThreadRenderComplete(si);
		}
		if (_renderCompleteDRThreadStrokeInfoList.Count == 0)
		{
			MediaContext.From(dispatcher).RenderComplete -= _onDRThreadRenderComplete;
			_waitingForDRThreadRenderComplete = false;
			return;
		}
		StrokeInfo siNext = _renderCompleteDRThreadStrokeInfoList.Peek();
		if (siNext.StrokeRTICV != null)
		{
			dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
			{
				((ContainerVisual)siNext.StrokeHV.VisualTarget.RootVisual).Children.Remove(siNext.StrokeRTICV);
				siNext.StrokeRTICV = null;
				return (object)null;
			}, null);
		}
	}

	/// <summary>Occurs on the application UI (user interface) thread when the tablet pen touches the digitizer.</summary>
	/// <param name="callbackData">The object that the application passed to the <see cref="M:System.Windows.Input.StylusPlugIns.RawStylusInput.NotifyWhenProcessed(System.Object)" /> method.</param>
	/// <param name="targetVerified">true if the pen's input was correctly routed to the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" />; otherwise, false.</param>
	protected override void OnStylusDownProcessed(object callbackData, bool targetVerified)
	{
		if (callbackData is StrokeInfo si && !targetVerified)
		{
			TransitionStrokeVisuals(si, abortStroke: true);
		}
	}

	/// <summary>Occurs on the application UI (user interface) thread when the user lifts the tablet pen from the digitizer.</summary>
	/// <param name="callbackData">The object that the application passed to the <see cref="M:System.Windows.Input.StylusPlugIns.RawStylusInput.NotifyWhenProcessed(System.Object)" /> method.</param>
	/// <param name="targetVerified">true if the pen's input was correctly routed to the <see cref="T:System.Windows.Input.StylusPlugIns.StylusPlugIn" />; otherwise, false.</param>
	protected override void OnStylusUpProcessed(object callbackData, bool targetVerified)
	{
		if (callbackData is StrokeInfo si)
		{
			TransitionStrokeVisuals(si, !targetVerified);
		}
	}

	private void OnInternalRenderComplete(object sender, EventArgs e)
	{
		MediaContext.From(_applicationDispatcher).RenderComplete -= _onRenderComplete;
		_waitingForRenderComplete = false;
		using (_applicationDispatcher.DisableProcessing())
		{
			OnRenderComplete();
		}
	}

	private void NotifyOnNextRenderComplete()
	{
		if (_applicationDispatcher != null)
		{
			_applicationDispatcher.VerifyAccess();
			if (_onRenderComplete == null)
			{
				_onRenderComplete = OnInternalRenderComplete;
			}
			if (!_waitingForRenderComplete)
			{
				MediaContext.From(_applicationDispatcher).RenderComplete += _onRenderComplete;
				_waitingForRenderComplete = true;
			}
		}
	}

	/// <summary>Draws the ink in real-time so it appears to "flow" from the tablet pen or other pointing device.</summary>
	/// <param name="drawingContext">The <see cref="T:System.Windows.Media.DrawingContext" /> object onto which the stroke is rendered.</param>
	/// <param name="stylusPoints">The <see cref="T:System.Windows.Input.StylusPointCollection" /> that represents the segment of the stroke to draw.</param>
	/// <param name="geometry">A <see cref="T:System.Windows.Media.Geometry" /> that represents the path of the mouse pointer.</param>
	/// <param name="fillBrush">A Brush that specifies the appearance of the current stroke.</param>
	protected virtual void OnDraw(DrawingContext drawingContext, StylusPointCollection stylusPoints, Geometry geometry, Brush fillBrush)
	{
		if (drawingContext == null)
		{
			throw new ArgumentNullException("drawingContext");
		}
		drawingContext.DrawGeometry(fillBrush, null, geometry);
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Input.StylusPlugIns.DynamicRenderer.DrawingAttributes" /> property changes.</summary>
	protected virtual void OnDrawingAttributesReplaced()
	{
	}

	/// <summary>Returns a <see cref="T:System.Windows.Threading.Dispatcher" /> for the rendering thread.</summary>
	/// <returns>A <see cref="T:System.Windows.Threading.Dispatcher" /> for the rendering thread.</returns>
	protected Dispatcher GetDispatcher()
	{
		if (_renderingThread == null)
		{
			return null;
		}
		return _renderingThread.ThreadDispatcher;
	}

	private void RenderPackets(StylusPointCollection stylusPoints, StrokeInfo si)
	{
		if (stylusPoints.Count == 0 || _applicationDispatcher == null)
		{
			return;
		}
		si.StrokeNodeIterator = si.StrokeNodeIterator.GetIteratorForNextSegment(stylusPoints);
		if (si.StrokeNodeIterator == null)
		{
			return;
		}
		StrokeRenderer.CalcGeometryAndBounds(si.StrokeNodeIterator, si.DrawingAttributes, calculateBounds: false, out var strokeGeometry, out var _);
		if (_applicationDispatcher.CheckAccess())
		{
			if (si.StrokeCV == null)
			{
				si.StrokeCV = new ContainerVisual();
				if (!si.DrawingAttributes.IsHighlighter)
				{
					si.StrokeCV.Opacity = si.Opacity;
				}
				_mainRawInkContainerVisual.Children.Add(si.StrokeCV);
			}
			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();
			try
			{
				OnDraw(drawingContext, stylusPoints, strokeGeometry, si.FillBrush);
			}
			finally
			{
				drawingContext.Close();
			}
			if (si.StrokeCV != null)
			{
				si.StrokeCV.Children.Add(drawingVisual);
			}
			return;
		}
		(_renderingThread?.ThreadDispatcher)?.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
		{
			SolidColorBrush fillBrush = si.FillBrush;
			if (fillBrush != null)
			{
				if (si.StrokeRTICV == null)
				{
					si.StrokeRTICV = new ContainerVisual();
					if (!si.DrawingAttributes.IsHighlighter)
					{
						si.StrokeRTICV.Opacity = si.Opacity;
					}
					((ContainerVisual)si.StrokeHV.VisualTarget.RootVisual).Children.Add(si.StrokeRTICV);
				}
				DrawingVisual drawingVisual2 = new DrawingVisual();
				DrawingContext drawingContext2 = drawingVisual2.RenderOpen();
				try
				{
					OnDraw(drawingContext2, stylusPoints, strokeGeometry, fillBrush);
				}
				finally
				{
					drawingContext2.Close();
				}
				si.StrokeRTICV.Children.Add(drawingVisual2);
			}
			return (object)null;
		}, null);
	}

	private void AbortAllStrokes()
	{
		lock (__siLock)
		{
			while (_strokeInfoList.Count > 0)
			{
				TransitionStrokeVisuals(_strokeInfoList[0], abortStroke: true);
			}
		}
	}

	private void TransitionStrokeVisuals(StrokeInfo si, bool abortStroke)
	{
		RemoveStrokeInfo(si);
		if (si.StrokeCV != null)
		{
			if (_mainRawInkContainerVisual != null)
			{
				_mainRawInkContainerVisual.Children.Remove(si.StrokeCV);
			}
			si.StrokeCV = null;
		}
		si.FillBrush = null;
		if (_rawInkHostVisual1 == null)
		{
			return;
		}
		bool flag = false;
		if (!abortStroke && _renderCompleteStrokeInfo == null)
		{
			using (_applicationDispatcher.DisableProcessing())
			{
				lock (__siLock)
				{
					if (si.StrokeHV.HasSingleReference)
					{
						si.StrokeHV.Clip = _zeroSizedFrozenRect;
						_renderCompleteStrokeInfo = si;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			NotifyOnNextRenderComplete();
		}
		else
		{
			RemoveDynamicRendererVisualAndNotifyWhenDone(si);
		}
	}

	private DynamicRendererHostVisual GetCurrentHostVisual()
	{
		if (_currentHostVisual == null)
		{
			_currentHostVisual = _rawInkHostVisual1;
		}
		else
		{
			HostVisual hostVisual = ((_renderCompleteStrokeInfo != null) ? _renderCompleteStrokeInfo.StrokeHV : null);
			if (_currentHostVisual.InUse)
			{
				if (_currentHostVisual == _rawInkHostVisual1)
				{
					if (!_rawInkHostVisual2.InUse || _rawInkHostVisual1 == hostVisual)
					{
						_currentHostVisual = _rawInkHostVisual2;
					}
				}
				else if (!_rawInkHostVisual1.InUse || _rawInkHostVisual2 == hostVisual)
				{
					_currentHostVisual = _rawInkHostVisual1;
				}
			}
		}
		return _currentHostVisual;
	}

	private void TransitionComplete(StrokeInfo si)
	{
		using (_applicationDispatcher.DisableProcessing())
		{
			lock (__siLock)
			{
				si.StrokeHV.RemoveStrokeInfoRef(si);
			}
		}
	}

	private void RemoveStrokeInfo(StrokeInfo si)
	{
		lock (__siLock)
		{
			_strokeInfoList.Remove(si);
		}
	}

	private StrokeInfo FindStrokeInfo(int timestamp)
	{
		lock (__siLock)
		{
			for (int i = 0; i < _strokeInfoList.Count; i++)
			{
				StrokeInfo strokeInfo = _strokeInfoList[i];
				if (strokeInfo.IsTimestampWithin(timestamp))
				{
					return strokeInfo;
				}
			}
		}
		return null;
	}

	private void CreateInkingVisuals()
	{
		if (_mainContainerVisual == null)
		{
			_mainContainerVisual = new ContainerVisual();
			_mainRawInkContainerVisual = new ContainerVisual();
			_mainContainerVisual.Children.Add(_mainRawInkContainerVisual);
		}
		if (base.IsActiveForInput)
		{
			using (base.Element.Dispatcher.DisableProcessing())
			{
				CreateRealTimeVisuals();
			}
		}
	}

	private void CreateRealTimeVisuals()
	{
		if (_mainContainerVisual != null && _rawInkHostVisual1 == null)
		{
			_rawInkHostVisual1 = new DynamicRendererHostVisual();
			_rawInkHostVisual2 = new DynamicRendererHostVisual();
			_currentHostVisual = null;
			_mainContainerVisual.Children.Add(_rawInkHostVisual1);
			_mainContainerVisual.Children.Add(_rawInkHostVisual2);
			_renderingThread = DynamicRendererThreadManager.GetCurrentThreadInstance();
		}
	}

	private void DestroyRealTimeVisuals()
	{
		if (_mainContainerVisual == null || _rawInkHostVisual1 == null)
		{
			return;
		}
		if (_waitingForRenderComplete)
		{
			MediaContext.From(_applicationDispatcher).RenderComplete -= _onRenderComplete;
			_waitingForRenderComplete = false;
		}
		_mainContainerVisual.Children.Remove(_rawInkHostVisual1);
		_mainContainerVisual.Children.Remove(_rawInkHostVisual2);
		_renderCompleteStrokeInfo = null;
		DynamicRendererThreadManager renderingThread = _renderingThread;
		Dispatcher drDispatcher = ((renderingThread != null) ? renderingThread.ThreadDispatcher : null);
		if (drDispatcher != null)
		{
			drDispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
			{
				_renderCompleteDRThreadStrokeInfoList.Clear();
				drDispatcher = renderingThread.ThreadDispatcher;
				if (drDispatcher != null && _waitingForDRThreadRenderComplete)
				{
					MediaContext.From(drDispatcher).RenderComplete -= _onDRThreadRenderComplete;
				}
				_waitingForDRThreadRenderComplete = false;
				return (object)null;
			}, null);
		}
		_renderingThread = null;
		_rawInkHostVisual1 = null;
		_rawInkHostVisual2 = null;
		_currentHostVisual = null;
		AbortAllStrokes();
	}
}
