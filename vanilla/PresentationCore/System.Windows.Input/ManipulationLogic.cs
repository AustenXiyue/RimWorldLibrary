using System.Collections.Generic;
using System.Windows.Input.Manipulations;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Input;

internal sealed class ManipulationLogic
{
	private ManipulationDevice _manipulationDevice;

	private IInputElement _currentContainer;

	private ManipulationPivot _pivot;

	private ManipulationModes _mode;

	private ManipulationProcessor2D _manipulationProcessor;

	private InertiaProcessor2D _inertiaProcessor;

	private Dictionary<int, Manipulator2D> _currentManipulators = new Dictionary<int, Manipulator2D>(2);

	private Dictionary<int, Manipulator2D> _removedManipulators = new Dictionary<int, Manipulator2D>(2);

	private ManipulationDelta _lastManipulationBeforeInertia;

	private InputEventArgs _generatedEvent;

	private DispatcherTimer _inertiaTimer;

	private bool _manualComplete;

	private bool _manualCompleteWithInertia;

	private EventHandler<EventArgs> _containerLayoutUpdated;

	private static readonly Point LayoutUpdateDetectionPivotPoint = new Point(-10234.1234, -10234.1234);

	private Point _containerPivotPoint;

	private Size _containerSize;

	private UIElement _root;

	private bool HasPendingBoundaryFeedback { get; set; }

	private int LastTimestamp { get; set; }

	internal IInputElement ManipulationContainer
	{
		get
		{
			return _currentContainer;
		}
		set
		{
			SetContainer(value);
		}
	}

	internal ManipulationModes ManipulationMode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
			if (_manipulationProcessor != null)
			{
				_manipulationProcessor.SupportedManipulations = ConvertMode(_mode);
			}
		}
	}

	internal ManipulationPivot ManipulationPivot
	{
		get
		{
			return _pivot;
		}
		set
		{
			_pivot = value;
			if (_manipulationProcessor != null)
			{
				_manipulationProcessor.Pivot = ConvertPivot(value);
			}
		}
	}

	private IEnumerable<Manipulator2D> CurrentManipulators
	{
		get
		{
			if (_currentManipulators.Count <= 0)
			{
				return null;
			}
			return _currentManipulators.Values;
		}
	}

	internal bool IsManipulationActive => _manipulationProcessor != null;

	private bool IsInertiaActive => _inertiaProcessor != null;

	internal event EventHandler<EventArgs> ContainerLayoutUpdated
	{
		add
		{
			bool num = _containerLayoutUpdated == null;
			_containerLayoutUpdated = (EventHandler<EventArgs>)Delegate.Combine(_containerLayoutUpdated, value);
			if (num && _containerLayoutUpdated != null)
			{
				SubscribeToLayoutUpdated();
			}
		}
		remove
		{
			bool num = _containerLayoutUpdated == null;
			_containerLayoutUpdated = (EventHandler<EventArgs>)Delegate.Remove(_containerLayoutUpdated, value);
			if (!num && _containerLayoutUpdated == null)
			{
				UnsubscribeFromLayoutUpdated();
			}
		}
	}

	internal ManipulationLogic(ManipulationDevice manipulationDevice)
	{
		_manipulationDevice = manipulationDevice;
	}

	private void OnManipulationStarted(object sender, Manipulation2DStartedEventArgs e)
	{
		PushEvent(new ManipulationStartedEventArgs(_manipulationDevice, LastTimestamp, _currentContainer, new Point(e.OriginX, e.OriginY)));
	}

	private void OnManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
	{
		ManipulationDeltaEventArgs e2 = new ManipulationDeltaEventArgs(_manipulationDevice, LastTimestamp, _currentContainer, new Point(e.OriginX, e.OriginY), ConvertDelta(e.Delta, null), ConvertDelta(e.Cumulative, _lastManipulationBeforeInertia), ConvertVelocities(e.Velocities), IsInertiaActive);
		PushEvent(e2);
	}

	private void OnManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e)
	{
		if (_manualComplete && !_manualCompleteWithInertia)
		{
			ManipulationCompletedEventArgs e2 = ConvertCompletedArguments(e);
			RaiseManipulationCompleted(e2);
		}
		else
		{
			_lastManipulationBeforeInertia = ConvertDelta(e.Total, null);
			ManipulationInertiaStartingEventArgs e3 = new ManipulationInertiaStartingEventArgs(_manipulationDevice, LastTimestamp, _currentContainer, new Point(e.OriginX, e.OriginY), ConvertVelocities(e.Velocities), isInInertia: false);
			PushEvent(e3);
		}
		_manipulationProcessor = null;
	}

	private void OnInertiaCompleted(object sender, Manipulation2DCompletedEventArgs e)
	{
		ClearTimer();
		if (_manualComplete && _manualCompleteWithInertia)
		{
			_lastManipulationBeforeInertia = ConvertDelta(e.Total, _lastManipulationBeforeInertia);
			ManipulationInertiaStartingEventArgs e2 = new ManipulationInertiaStartingEventArgs(_manipulationDevice, LastTimestamp, _currentContainer, new Point(e.OriginX, e.OriginY), ConvertVelocities(e.Velocities), isInInertia: true);
			PushEvent(e2);
		}
		else
		{
			ManipulationCompletedEventArgs e3 = ConvertCompletedArguments(e);
			RaiseManipulationCompleted(e3);
		}
		_inertiaProcessor = null;
	}

	private void RaiseManipulationCompleted(ManipulationCompletedEventArgs e)
	{
		PushEvent(e);
	}

	internal void OnCompleted()
	{
		_lastManipulationBeforeInertia = null;
		SetContainer(null);
	}

	private ManipulationCompletedEventArgs ConvertCompletedArguments(Manipulation2DCompletedEventArgs e)
	{
		return new ManipulationCompletedEventArgs(_manipulationDevice, LastTimestamp, _currentContainer, new Point(e.OriginX, e.OriginY), ConvertDelta(e.Total, _lastManipulationBeforeInertia), ConvertVelocities(e.Velocities), IsInertiaActive);
	}

	private static ManipulationDelta ConvertDelta(ManipulationDelta2D delta, ManipulationDelta add)
	{
		if (add != null)
		{
			return new ManipulationDelta(new Vector((double)delta.TranslationX + add.Translation.X, (double)delta.TranslationY + add.Translation.Y), AngleUtil.RadiansToDegrees(delta.Rotation) + add.Rotation, new Vector((double)delta.ScaleX * add.Scale.X, (double)delta.ScaleY * add.Scale.Y), new Vector((double)delta.ExpansionX + add.Expansion.X, (double)delta.ExpansionY + add.Expansion.Y));
		}
		return new ManipulationDelta(new Vector(delta.TranslationX, delta.TranslationY), AngleUtil.RadiansToDegrees(delta.Rotation), new Vector(delta.ScaleX, delta.ScaleY), new Vector(delta.ExpansionX, delta.ExpansionY));
	}

	private static ManipulationVelocities ConvertVelocities(ManipulationVelocities2D velocities)
	{
		return new ManipulationVelocities(new Vector(velocities.LinearVelocityX, velocities.LinearVelocityY), AngleUtil.RadiansToDegrees(velocities.AngularVelocity), new Vector(velocities.ExpansionVelocityX, velocities.ExpansionVelocityY));
	}

	internal void Complete(bool withInertia)
	{
		try
		{
			_manualComplete = true;
			_manualCompleteWithInertia = withInertia;
			if (IsManipulationActive)
			{
				_manipulationProcessor.CompleteManipulation(GetCurrentTimestamp());
			}
			else if (IsInertiaActive)
			{
				_inertiaProcessor.Complete(GetCurrentTimestamp());
			}
		}
		finally
		{
			_manualComplete = false;
			_manualCompleteWithInertia = false;
		}
	}

	private ManipulationCompletedEventArgs GetManipulationCompletedArguments(ManipulationInertiaStartingEventArgs e)
	{
		return new ManipulationCompletedEventArgs(_manipulationDevice, LastTimestamp, _currentContainer, new Point(e.ManipulationOrigin.X, e.ManipulationOrigin.Y), _lastManipulationBeforeInertia, e.InitialVelocities, IsInertiaActive);
	}

	internal void BeginInertia(ManipulationInertiaStartingEventArgs e)
	{
		if (e.CanBeginInertia())
		{
			_inertiaProcessor = new InertiaProcessor2D();
			_inertiaProcessor.Delta += OnManipulationDelta;
			_inertiaProcessor.Completed += OnInertiaCompleted;
			e.ApplyParameters(_inertiaProcessor);
			_inertiaTimer = new DispatcherTimer();
			_inertiaTimer.Interval = TimeSpan.FromMilliseconds(15.0);
			_inertiaTimer.Tick += OnInertiaTick;
			_inertiaTimer.Start();
		}
		else
		{
			ManipulationCompletedEventArgs manipulationCompletedArguments = GetManipulationCompletedArguments(e);
			RaiseManipulationCompleted(manipulationCompletedArguments);
			PushEventsToDevice();
		}
	}

	internal static long GetCurrentTimestamp()
	{
		return MediaContext.CurrentTicks;
	}

	private void OnInertiaTick(object sender, EventArgs e)
	{
		if (IsInertiaActive)
		{
			if (!_inertiaProcessor.Process(GetCurrentTimestamp()))
			{
				ClearTimer();
			}
			PushEventsToDevice();
		}
		else
		{
			ClearTimer();
		}
	}

	private void ClearTimer()
	{
		if (_inertiaTimer != null)
		{
			_inertiaTimer.Stop();
			_inertiaTimer = null;
		}
	}

	private void PushEvent(InputEventArgs e)
	{
		_generatedEvent = e;
	}

	internal void PushEventsToDevice()
	{
		if (_generatedEvent != null)
		{
			InputEventArgs generatedEvent = _generatedEvent;
			_generatedEvent = null;
			_manipulationDevice.ProcessManipulationInput(generatedEvent);
		}
	}

	internal void RaiseBoundaryFeedback(ManipulationDelta unusedManipulation, bool requestedComplete)
	{
		bool flag = unusedManipulation != null;
		if ((!flag || requestedComplete) && HasPendingBoundaryFeedback)
		{
			unusedManipulation = new ManipulationDelta(default(Vector), 0.0, new Vector(1.0, 1.0), default(Vector));
			HasPendingBoundaryFeedback = false;
		}
		else if (flag)
		{
			HasPendingBoundaryFeedback = true;
		}
		if (unusedManipulation != null)
		{
			PushEvent(new ManipulationBoundaryFeedbackEventArgs(_manipulationDevice, LastTimestamp, _currentContainer, unusedManipulation));
		}
	}

	internal void ReportFrame(ICollection<IManipulator> manipulators)
	{
		long currentTimestamp = GetCurrentTimestamp();
		LastTimestamp = SafeNativeMethods.GetMessageTime();
		int count = manipulators.Count;
		if (IsInertiaActive && count > 0)
		{
			_inertiaProcessor.Complete(currentTimestamp);
			PushEventsToDevice();
		}
		if (!IsManipulationActive && count > 0)
		{
			ManipulationStartingEventArgs manipulationStartingEventArgs = RaiseStarting();
			if (!manipulationStartingEventArgs.RequestedCancel && manipulationStartingEventArgs.Mode != 0 && (manipulationStartingEventArgs.IsSingleTouchEnabled || count >= 2))
			{
				SetContainer(manipulationStartingEventArgs.ManipulationContainer);
				_mode = manipulationStartingEventArgs.Mode;
				_pivot = manipulationStartingEventArgs.Pivot;
				IList<ManipulationParameters2D> parameters = manipulationStartingEventArgs.Parameters;
				_manipulationProcessor = new ManipulationProcessor2D(ConvertMode(_mode), ConvertPivot(_pivot));
				if (parameters != null)
				{
					_ = parameters.Count;
					for (int i = 0; i < parameters.Count; i++)
					{
						_manipulationProcessor.SetParameters(parameters[i]);
					}
				}
				_manipulationProcessor.Started += OnManipulationStarted;
				_manipulationProcessor.Delta += OnManipulationDelta;
				_manipulationProcessor.Completed += OnManipulationCompleted;
				_currentManipulators.Clear();
			}
		}
		if (IsManipulationActive)
		{
			UpdateManipulators(manipulators);
			_manipulationProcessor.ProcessManipulators(currentTimestamp, CurrentManipulators);
			PushEventsToDevice();
		}
	}

	private ManipulationStartingEventArgs RaiseStarting()
	{
		ManipulationStartingEventArgs manipulationStartingEventArgs = new ManipulationStartingEventArgs(_manipulationDevice, Environment.TickCount);
		manipulationStartingEventArgs.ManipulationContainer = _manipulationDevice.Target;
		_manipulationDevice.ProcessManipulationInput(manipulationStartingEventArgs);
		return manipulationStartingEventArgs;
	}

	private static Manipulations2D ConvertMode(ManipulationModes mode)
	{
		Manipulations2D manipulations2D = Manipulations2D.None;
		if ((mode & ManipulationModes.TranslateX) != 0)
		{
			manipulations2D |= Manipulations2D.TranslateX;
		}
		if ((mode & ManipulationModes.TranslateY) != 0)
		{
			manipulations2D |= Manipulations2D.TranslateY;
		}
		if ((mode & ManipulationModes.Scale) != 0)
		{
			manipulations2D |= Manipulations2D.Scale;
		}
		if ((mode & ManipulationModes.Rotate) != 0)
		{
			manipulations2D |= Manipulations2D.Rotate;
		}
		return manipulations2D;
	}

	private static ManipulationPivot2D ConvertPivot(ManipulationPivot pivot)
	{
		if (pivot != null)
		{
			Point center = pivot.Center;
			return new ManipulationPivot2D
			{
				X = (float)center.X,
				Y = (float)center.Y,
				Radius = (float)Math.Max(1.0, pivot.Radius)
			};
		}
		return null;
	}

	internal void SetManipulationParameters(ManipulationParameters2D parameter)
	{
		if (_manipulationProcessor != null)
		{
			_manipulationProcessor.SetParameters(parameter);
		}
	}

	private void UpdateManipulators(ICollection<IManipulator> updatedManipulators)
	{
		_removedManipulators.Clear();
		Dictionary<int, Manipulator2D> removedManipulators = _removedManipulators;
		_removedManipulators = _currentManipulators;
		_currentManipulators = removedManipulators;
		if (_currentContainer is UIElement uIElement)
		{
			if (!uIElement.IsVisible)
			{
				return;
			}
		}
		else if (_currentContainer is UIElement3D { IsVisible: false })
		{
			return;
		}
		foreach (IManipulator updatedManipulator in updatedManipulators)
		{
			int id = updatedManipulator.Id;
			_removedManipulators.Remove(id);
			Point position = updatedManipulator.GetPosition(_currentContainer);
			position = _manipulationDevice.GetTransformedManipulatorPosition(position);
			_currentManipulators[id] = new Manipulator2D(id, (float)position.X, (float)position.Y);
		}
	}

	private void SetContainer(IInputElement newContainer)
	{
		UnsubscribeFromLayoutUpdated();
		_containerPivotPoint = default(Point);
		_containerSize = default(Size);
		_root = null;
		_currentContainer = newContainer;
		if (newContainer != null)
		{
			PresentationSource presentationSource = PresentationSource.CriticalFromVisual((Visual)newContainer);
			if (presentationSource != null)
			{
				_root = presentationSource.RootVisual as UIElement;
			}
			if (_containerLayoutUpdated != null)
			{
				SubscribeToLayoutUpdated();
			}
		}
	}

	private void SubscribeToLayoutUpdated()
	{
		if (_currentContainer is UIElement uIElement)
		{
			uIElement.LayoutUpdated += OnLayoutUpdated;
		}
	}

	private void UnsubscribeFromLayoutUpdated()
	{
		if (_currentContainer is UIElement uIElement)
		{
			uIElement.LayoutUpdated -= OnLayoutUpdated;
		}
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		if (UpdateCachedPositionAndSize())
		{
			_containerLayoutUpdated(this, EventArgs.Empty);
		}
	}

	private bool UpdateCachedPositionAndSize()
	{
		if (_root == null)
		{
			return false;
		}
		if (!(_currentContainer is UIElement { RenderSize: var renderSize } uIElement))
		{
			return false;
		}
		Point point = _root.TranslatePoint(LayoutUpdateDetectionPivotPoint, uIElement);
		int num;
		if (DoubleUtil.AreClose(renderSize, _containerSize))
		{
			num = ((!DoubleUtil.AreClose(point, _containerPivotPoint)) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0065;
			}
		}
		else
		{
			num = 1;
		}
		_containerSize = renderSize;
		_containerPivotPoint = point;
		goto IL_0065;
		IL_0065:
		return (byte)num != 0;
	}
}
