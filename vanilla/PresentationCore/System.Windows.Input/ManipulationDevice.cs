using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input.Manipulations;
using System.Windows.Media;
using MS.Utility;

namespace System.Windows.Input;

internal sealed class ManipulationDevice : InputDevice
{
	private InputManager _inputManager;

	private ManipulationLogic _manipulationLogic;

	private PresentationSource _activeSource;

	private UIElement _target;

	private List<IManipulator> _manipulators;

	private bool _ticking;

	private bool _wasTicking;

	private Func<Point, Point> _compensateForBoundaryFeedback;

	private bool _manipulationEnded;

	private IManipulator _removedManipulator;

	[ThreadStatic]
	private static long LastUpdatedTimestamp;

	private const long ThrottleTimeout = 50000000L;

	[ThreadStatic]
	private static Dictionary<UIElement, ManipulationDevice> _manipulationDevices;

	public override IInputElement Target => _target;

	public override PresentationSource ActiveSource => _activeSource;

	internal ManipulationModes ManipulationMode
	{
		get
		{
			return _manipulationLogic.ManipulationMode;
		}
		set
		{
			_manipulationLogic.ManipulationMode = value;
		}
	}

	internal ManipulationPivot ManipulationPivot
	{
		get
		{
			return _manipulationLogic.ManipulationPivot;
		}
		set
		{
			_manipulationLogic.ManipulationPivot = value;
		}
	}

	internal IInputElement ManipulationContainer
	{
		get
		{
			return _manipulationLogic.ManipulationContainer;
		}
		set
		{
			_manipulationLogic.ManipulationContainer = value;
		}
	}

	internal bool IsManipulationActive => _manipulationLogic.IsManipulationActive;

	private ManipulationDevice(UIElement element)
	{
		_target = element;
		_activeSource = PresentationSource.CriticalFromVisual(element);
		_inputManager = InputManager.UnsecureCurrent;
		_inputManager.PostProcessInput += PostProcessInput;
		_manipulationLogic = new ManipulationLogic(this);
	}

	private void DetachManipulationDevice()
	{
		_inputManager.PostProcessInput -= PostProcessInput;
	}

	internal static ManipulationDevice AddManipulationDevice(UIElement element)
	{
		element.VerifyAccess();
		ManipulationDevice manipulationDevice = GetManipulationDevice(element);
		if (manipulationDevice == null)
		{
			if (_manipulationDevices == null)
			{
				_manipulationDevices = new Dictionary<UIElement, ManipulationDevice>(2);
			}
			manipulationDevice = new ManipulationDevice(element);
			_manipulationDevices[element] = manipulationDevice;
		}
		return manipulationDevice;
	}

	internal static ManipulationDevice GetManipulationDevice(UIElement element)
	{
		if (_manipulationDevices != null)
		{
			_manipulationDevices.TryGetValue(element, out var value);
			return value;
		}
		return null;
	}

	private void RemoveManipulationDevice()
	{
		_wasTicking = false;
		StopTicking();
		DetachManipulationDevice();
		_compensateForBoundaryFeedback = null;
		RemoveAllManipulators();
		if (_manipulationDevices != null)
		{
			_manipulationDevices.Remove(_target);
		}
	}

	private void RemoveAllManipulators()
	{
		if (_manipulators != null)
		{
			for (int num = _manipulators.Count - 1; num >= 0; num--)
			{
				_manipulators[num].Updated -= OnManipulatorUpdated;
			}
			_manipulators.Clear();
		}
	}

	internal void AddManipulator(IManipulator manipulator)
	{
		VerifyAccess();
		_manipulationEnded = false;
		if (_manipulators == null)
		{
			_manipulators = new List<IManipulator>(2);
		}
		_manipulators.Add(manipulator);
		manipulator.Updated += OnManipulatorUpdated;
		OnManipulatorUpdated(manipulator, EventArgs.Empty);
	}

	internal void RemoveManipulator(IManipulator manipulator)
	{
		VerifyAccess();
		manipulator.Updated -= OnManipulatorUpdated;
		if (_manipulators != null)
		{
			_manipulators.Remove(manipulator);
		}
		OnManipulatorUpdated(manipulator, EventArgs.Empty);
		if (!_manipulationEnded)
		{
			if (_manipulators == null || _manipulators.Count == 0)
			{
				_removedManipulator = manipulator;
			}
			ReportFrame();
			_removedManipulator = null;
		}
	}

	internal IEnumerable<IManipulator> GetManipulatorsReadOnly()
	{
		if (_manipulators != null)
		{
			return new ReadOnlyCollection<IManipulator>(_manipulators);
		}
		return new ReadOnlyCollection<IManipulator>(new List<IManipulator>(2));
	}

	internal void OnManipulatorUpdated(object sender, EventArgs e)
	{
		LastUpdatedTimestamp = ManipulationLogic.GetCurrentTimestamp();
		ResumeAllTicking();
		StartTicking();
	}

	internal Point GetTransformedManipulatorPosition(Point point)
	{
		if (_compensateForBoundaryFeedback != null)
		{
			return _compensateForBoundaryFeedback(point);
		}
		return point;
	}

	private static void ResumeAllTicking()
	{
		if (_manipulationDevices == null)
		{
			return;
		}
		foreach (UIElement key in _manipulationDevices.Keys)
		{
			ManipulationDevice manipulationDevice = _manipulationDevices[key];
			if (manipulationDevice != null && manipulationDevice._wasTicking)
			{
				manipulationDevice.StartTicking();
				manipulationDevice._wasTicking = false;
			}
		}
	}

	private void StartTicking()
	{
		if (!_ticking)
		{
			_ticking = true;
			CompositionTarget.Rendering += OnRendering;
			SubscribeToLayoutUpdate();
		}
	}

	private void StopTicking()
	{
		if (_ticking)
		{
			CompositionTarget.Rendering -= OnRendering;
			_ticking = false;
			UnsubscribeFromLayoutUpdate();
		}
	}

	private void SubscribeToLayoutUpdate()
	{
		_manipulationLogic.ContainerLayoutUpdated += OnContainerLayoutUpdated;
	}

	private void UnsubscribeFromLayoutUpdate()
	{
		_manipulationLogic.ContainerLayoutUpdated -= OnContainerLayoutUpdated;
	}

	private void OnContainerLayoutUpdated(object sender, EventArgs e)
	{
		ReportFrame();
	}

	private void OnRendering(object sender, EventArgs e)
	{
		ReportFrame();
		if (!IsManipulationActive || ManipulationLogic.GetCurrentTimestamp() - LastUpdatedTimestamp > 50000000)
		{
			_wasTicking = _ticking;
			StopTicking();
		}
	}

	private void ReportFrame()
	{
		if (!_manipulationEnded)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, EventTrace.Event.ManipulationReportFrame, 0);
			_manipulationLogic.ReportFrame(_manipulators);
		}
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		InputEventArgs input = e.StagingItem.Input;
		if (input.Device != this)
		{
			return;
		}
		RoutedEvent routedEvent = input.RoutedEvent;
		if (routedEvent == Manipulation.ManipulationDeltaEvent)
		{
			if (input is ManipulationDeltaEventArgs { UnusedManipulation: var unusedManipulation } manipulationDeltaEventArgs)
			{
				_manipulationLogic.RaiseBoundaryFeedback(unusedManipulation, manipulationDeltaEventArgs.RequestedComplete);
				_manipulationLogic.PushEventsToDevice();
				if (manipulationDeltaEventArgs.RequestedComplete)
				{
					_manipulationLogic.Complete(manipulationDeltaEventArgs.RequestedInertia);
					_manipulationLogic.PushEventsToDevice();
				}
				else if (manipulationDeltaEventArgs.RequestedCancel)
				{
					OnManipulationCancel();
				}
			}
		}
		else if (routedEvent == Manipulation.ManipulationStartingEvent)
		{
			if (input is ManipulationStartingEventArgs { RequestedCancel: not false })
			{
				OnManipulationCancel();
			}
		}
		else if (routedEvent == Manipulation.ManipulationStartedEvent)
		{
			if (input is ManipulationStartedEventArgs manipulationStartedEventArgs)
			{
				if (manipulationStartedEventArgs.RequestedComplete)
				{
					_manipulationLogic.Complete(withInertia: false);
					_manipulationLogic.PushEventsToDevice();
				}
				else if (manipulationStartedEventArgs.RequestedCancel)
				{
					OnManipulationCancel();
				}
				else
				{
					ResumeAllTicking();
					StartTicking();
				}
			}
		}
		else if (routedEvent == Manipulation.ManipulationInertiaStartingEvent)
		{
			StopTicking();
			RemoveAllManipulators();
			if (input is ManipulationInertiaStartingEventArgs manipulationInertiaStartingEventArgs)
			{
				if (manipulationInertiaStartingEventArgs.RequestedCancel)
				{
					OnManipulationCancel();
				}
				else
				{
					_manipulationLogic.BeginInertia(manipulationInertiaStartingEventArgs);
				}
			}
		}
		else if (routedEvent == Manipulation.ManipulationCompletedEvent)
		{
			_manipulationLogic.OnCompleted();
			if (input is ManipulationCompletedEventArgs manipulationCompletedEventArgs)
			{
				if (manipulationCompletedEventArgs.RequestedCancel)
				{
					OnManipulationCancel();
				}
				else if (!manipulationCompletedEventArgs.IsInertial || !_ticking)
				{
					OnManipulationComplete();
				}
			}
		}
		else if (routedEvent == Manipulation.ManipulationBoundaryFeedbackEvent && input is ManipulationBoundaryFeedbackEventArgs manipulationBoundaryFeedbackEventArgs)
		{
			_compensateForBoundaryFeedback = manipulationBoundaryFeedbackEventArgs.CompensateForBoundaryFeedback;
		}
	}

	private void OnManipulationCancel()
	{
		_manipulationEnded = true;
		if (_manipulators != null)
		{
			if (_removedManipulator != null)
			{
				_removedManipulator.ManipulationEnded(cancel: true);
			}
			else
			{
				foreach (IManipulator item in new List<IManipulator>(_manipulators))
				{
					item.ManipulationEnded(cancel: true);
				}
			}
		}
		RemoveManipulationDevice();
	}

	private void OnManipulationComplete()
	{
		_manipulationEnded = true;
		if (_manipulators != null)
		{
			foreach (IManipulator item in new List<IManipulator>(_manipulators))
			{
				item.ManipulationEnded(cancel: false);
			}
		}
		RemoveManipulationDevice();
	}

	internal void SetManipulationParameters(ManipulationParameters2D parameter)
	{
		_manipulationLogic.SetManipulationParameters(parameter);
	}

	internal void CompleteManipulation(bool withInertia)
	{
		if (_manipulationLogic != null)
		{
			_manipulationLogic.Complete(withInertia);
			_manipulationLogic.PushEventsToDevice();
		}
	}

	internal void ProcessManipulationInput(InputEventArgs e)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, EventTrace.Event.ManipulationEventRaised, 0);
		_inputManager.ProcessInput(e);
	}
}
