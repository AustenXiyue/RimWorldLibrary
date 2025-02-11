using System.Collections.Generic;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Input.StylusWisp;
using System.Windows.Input.Tracing;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

internal sealed class PenContexts
{
	internal SecurityCriticalData<HwndSource> _inputSource;

	private WispLogic _stylusLogic;

	private object __rtiLock = new object();

	private List<StylusPlugInCollection> _plugInCollectionList = new List<StylusPlugInCollection>();

	private PenContext[] _contexts;

	private bool _isWindowDisabled;

	private Point _destroyedLocation = new Point(0.0, 0.0);

	internal bool IsWindowDisabled
	{
		get
		{
			return _isWindowDisabled;
		}
		set
		{
			_isWindowDisabled = value;
		}
	}

	internal Point DestroyedLocation
	{
		get
		{
			return _destroyedLocation;
		}
		set
		{
			_destroyedLocation = value;
		}
	}

	internal object SyncRoot => __rtiLock;

	internal HwndSource InputSource => _inputSource.Value;

	internal PenContexts(WispLogic stylusLogic, PresentationSource inputSource)
	{
		if (!(inputSource is HwndSource hwndSource) || IntPtr.Zero == hwndSource.CriticalHandle)
		{
			throw new InvalidOperationException(SR.Stylus_PenContextFailure);
		}
		_stylusLogic = stylusLogic;
		_inputSource = new SecurityCriticalData<HwndSource>(hwndSource);
	}

	internal void Enable()
	{
		if (_contexts == null)
		{
			_contexts = _stylusLogic.WispTabletDevices.CreateContexts(_inputSource.Value.CriticalHandle, this);
			PenContext[] contexts = _contexts;
			for (int i = 0; i < contexts.Length; i++)
			{
				contexts[i].Enable();
			}
		}
	}

	internal void Disable(bool shutdownWorkerThread)
	{
		if (_contexts != null)
		{
			PenContext[] contexts = _contexts;
			for (int i = 0; i < contexts.Length; i++)
			{
				contexts[i].Disable(shutdownWorkerThread);
			}
			_contexts = null;
		}
	}

	internal void OnPenDown(PenContext penContext, int tabletDeviceId, int stylusPointerId, int[] data, int timestamp)
	{
		ProcessInput(RawStylusActions.Down, penContext, tabletDeviceId, stylusPointerId, data, timestamp);
	}

	internal void OnPenUp(PenContext penContext, int tabletDeviceId, int stylusPointerId, int[] data, int timestamp)
	{
		ProcessInput(RawStylusActions.Up, penContext, tabletDeviceId, stylusPointerId, data, timestamp);
	}

	internal void OnPackets(PenContext penContext, int tabletDeviceId, int stylusPointerId, int[] data, int timestamp)
	{
		ProcessInput(RawStylusActions.Move, penContext, tabletDeviceId, stylusPointerId, data, timestamp);
	}

	internal void OnInAirPackets(PenContext penContext, int tabletDeviceId, int stylusPointerId, int[] data, int timestamp)
	{
		ProcessInput(RawStylusActions.InAirMove, penContext, tabletDeviceId, stylusPointerId, data, timestamp);
	}

	internal void OnPenInRange(PenContext penContext, int tabletDeviceId, int stylusPointerId, int[] data, int timestamp)
	{
		ProcessInput(RawStylusActions.InRange, penContext, tabletDeviceId, stylusPointerId, data, timestamp);
	}

	internal void OnPenOutOfRange(PenContext penContext, int tabletDeviceId, int stylusPointerId, int timestamp)
	{
		ProcessInput(RawStylusActions.OutOfRange, penContext, tabletDeviceId, stylusPointerId, Array.Empty<int>(), timestamp);
	}

	internal void OnSystemEvent(PenContext penContext, int tabletDeviceId, int stylusPointerId, int timestamp, SystemGesture id, int gestureX, int gestureY, int buttonState)
	{
		_stylusLogic.ProcessSystemEvent(penContext, tabletDeviceId, stylusPointerId, timestamp, id, gestureX, gestureY, buttonState, _inputSource.Value);
	}

	private void ProcessInput(RawStylusActions actions, PenContext penContext, int tabletDeviceId, int stylusPointerId, int[] data, int timestamp)
	{
		_stylusLogic.ProcessInput(actions, penContext, tabletDeviceId, stylusPointerId, data, timestamp, _inputSource.Value);
	}

	internal PenContext GetTabletDeviceIDPenContext(int tabletDeviceId)
	{
		if (_contexts != null)
		{
			for (int i = 0; i < _contexts.Length; i++)
			{
				PenContext penContext = _contexts[i];
				if (penContext.TabletDeviceId == tabletDeviceId)
				{
					return penContext;
				}
			}
		}
		return null;
	}

	internal bool ConsiderInRange(int timestamp)
	{
		if (_contexts != null)
		{
			for (int i = 0; i < _contexts.Length; i++)
			{
				PenContext penContext = _contexts[i];
				if (penContext.QueuedInRangeCount > 0 || Math.Abs(timestamp - penContext.LastInRangeTime) <= 500)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal void AddContext(uint index)
	{
		if (_contexts != null && index <= _contexts.Length && _inputSource.Value.CriticalHandle != IntPtr.Zero)
		{
			PenContext[] array = new PenContext[_contexts.Length + 1];
			uint num = (uint)_contexts.Length - index;
			Array.Copy(_contexts, 0L, array, 0L, index);
			PenContext penContext = (array[index] = _stylusLogic.TabletDevices[(int)index].As<WispTabletDevice>().CreateContext(_inputSource.Value.CriticalHandle, this));
			Array.Copy(_contexts, index, array, index + 1, num);
			_contexts = array;
			penContext.Enable();
		}
	}

	internal void RemoveContext(uint index)
	{
		if (_contexts != null && index < _contexts.Length)
		{
			PenContext obj = _contexts[index];
			PenContext[] array = new PenContext[_contexts.Length - 1];
			uint num = (uint)(_contexts.Length - (int)index - 1);
			Array.Copy(_contexts, 0L, array, 0L, index);
			Array.Copy(_contexts, index + 1, array, index, num);
			obj.Disable(shutdownWorkerThread: false);
			_contexts = array;
		}
	}

	internal void AddStylusPlugInCollection(StylusPlugInCollection pic)
	{
		_plugInCollectionList.Insert(FindZOrderIndex(pic), pic);
	}

	internal void RemoveStylusPlugInCollection(StylusPlugInCollection pic)
	{
		_plugInCollectionList.Remove(pic);
	}

	internal int FindZOrderIndex(StylusPlugInCollection spicAdding)
	{
		DependencyObject dependencyObject = spicAdding.Element;
		int i;
		for (i = 0; i < _plugInCollectionList.Count; i++)
		{
			DependencyObject dependencyObject2 = _plugInCollectionList[i].Element;
			if (VisualTreeHelper.IsAncestorOf(dependencyObject, dependencyObject2))
			{
				for (i++; i < _plugInCollectionList.Count; i++)
				{
					dependencyObject2 = _plugInCollectionList[i].Element;
					if (!VisualTreeHelper.IsAncestorOf(dependencyObject, dependencyObject2))
					{
						break;
					}
				}
				return i;
			}
			DependencyObject dependencyObject3 = VisualTreeHelper.FindCommonAncestor(dependencyObject, dependencyObject2);
			if (dependencyObject3 == null)
			{
				continue;
			}
			if (dependencyObject2 == dependencyObject3)
			{
				return i;
			}
			while (VisualTreeHelper.GetParentInternal(dependencyObject) != dependencyObject3)
			{
				dependencyObject = VisualTreeHelper.GetParentInternal(dependencyObject);
			}
			while (VisualTreeHelper.GetParentInternal(dependencyObject2) != dependencyObject3)
			{
				dependencyObject2 = VisualTreeHelper.GetParentInternal(dependencyObject2);
			}
			int childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject3);
			for (int j = 0; j < childrenCount; j++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(dependencyObject3, j);
				if (child == dependencyObject)
				{
					return i;
				}
				if (child == dependencyObject2)
				{
					break;
				}
			}
		}
		return i;
	}

	internal StylusPlugInCollection InvokeStylusPluginCollectionForMouse(RawStylusInputReport inputReport, IInputElement directlyOver, StylusPlugInCollection currentPlugInCollection)
	{
		StylusPlugInCollection stylusPlugInCollection = null;
		lock (__rtiLock)
		{
			if (directlyOver != null && InputElement.GetContainingUIElement(directlyOver as DependencyObject) is UIElement element)
			{
				stylusPlugInCollection = FindPlugInCollection(element);
			}
			if (currentPlugInCollection != null && currentPlugInCollection != stylusPlugInCollection)
			{
				RawStylusInput rawStylusInput = new RawStylusInput(inputReport, currentPlugInCollection.ViewToElement, currentPlugInCollection);
				currentPlugInCollection.FireEnterLeave(isEnter: false, rawStylusInput, confirmed: true);
				_stylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
			}
			if (stylusPlugInCollection != null)
			{
				RawStylusInput rawStylusInput3 = (inputReport.RawStylusInput = new RawStylusInput(inputReport, stylusPlugInCollection.ViewToElement, stylusPlugInCollection));
				if (stylusPlugInCollection != currentPlugInCollection)
				{
					stylusPlugInCollection.FireEnterLeave(isEnter: true, rawStylusInput3, confirmed: true);
				}
				stylusPlugInCollection.FireRawStylusInput(rawStylusInput3);
				_stylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
				foreach (RawStylusInputCustomData customData in rawStylusInput3.CustomDataList)
				{
					customData.Owner.FireCustomData(customData.Data, inputReport.Actions, targetVerified: true);
				}
			}
		}
		return stylusPlugInCollection;
	}

	internal void InvokeStylusPluginCollection(RawStylusInputReport inputReport)
	{
		StylusPlugInCollection stylusPlugInCollection = null;
		lock (__rtiLock)
		{
			RawStylusActions actions = inputReport.Actions;
			if (actions != RawStylusActions.Down && actions != RawStylusActions.Up && actions != RawStylusActions.Move)
			{
				return;
			}
			stylusPlugInCollection = TargetPlugInCollection(inputReport);
			WispStylusDevice wispStylusDevice = inputReport.StylusDevice.As<WispStylusDevice>();
			StylusPlugInCollection currentNonVerifiedTarget = wispStylusDevice.CurrentNonVerifiedTarget;
			if (currentNonVerifiedTarget != null && currentNonVerifiedTarget != stylusPlugInCollection)
			{
				GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
				generalTransformGroup.Children.Add(new MatrixTransform(_stylusLogic.GetTabletToViewTransform(wispStylusDevice.CriticalActiveSource, wispStylusDevice.TabletDevice)));
				generalTransformGroup.Children.Add(currentNonVerifiedTarget.ViewToElement);
				generalTransformGroup.Freeze();
				RawStylusInput rawStylusInput = new RawStylusInput(inputReport, generalTransformGroup, currentNonVerifiedTarget);
				currentNonVerifiedTarget.FireEnterLeave(isEnter: false, rawStylusInput, confirmed: false);
				_stylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
				wispStylusDevice.CurrentNonVerifiedTarget = null;
			}
			if (stylusPlugInCollection != null)
			{
				GeneralTransformGroup generalTransformGroup2 = new GeneralTransformGroup();
				generalTransformGroup2.Children.Add(new MatrixTransform(_stylusLogic.GetTabletToViewTransform(wispStylusDevice.CriticalActiveSource, wispStylusDevice.TabletDevice)));
				generalTransformGroup2.Children.Add(stylusPlugInCollection.ViewToElement);
				generalTransformGroup2.Freeze();
				RawStylusInput rawStylusInput3 = (inputReport.RawStylusInput = new RawStylusInput(inputReport, generalTransformGroup2, stylusPlugInCollection));
				if (stylusPlugInCollection != currentNonVerifiedTarget)
				{
					wispStylusDevice.CurrentNonVerifiedTarget = stylusPlugInCollection;
					stylusPlugInCollection.FireEnterLeave(isEnter: true, rawStylusInput3, confirmed: false);
				}
				stylusPlugInCollection.FireRawStylusInput(rawStylusInput3);
				_stylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
			}
		}
	}

	internal StylusPlugInCollection TargetPlugInCollection(RawStylusInputReport inputReport)
	{
		StylusPlugInCollection stylusPlugInCollection = null;
		WispStylusDevice wispStylusDevice = inputReport.StylusDevice.As<WispStylusDevice>();
		bool elementHasCapture = false;
		stylusPlugInCollection = wispStylusDevice.GetCapturedPlugInCollection(ref elementHasCapture);
		int inputArrayLengthPerPoint = inputReport.PenContext.StylusPointDescription.GetInputArrayLengthPerPoint();
		if (elementHasCapture && !_plugInCollectionList.Contains(stylusPlugInCollection))
		{
			elementHasCapture = false;
		}
		if (!elementHasCapture && inputReport.Data != null && inputReport.Data.Length >= inputArrayLengthPerPoint)
		{
			int[] data = inputReport.Data;
			Point measurePoint = new Point(data[^inputArrayLengthPerPoint], data[data.Length - inputArrayLengthPerPoint + 1]);
			measurePoint *= wispStylusDevice.TabletDevice.TabletDeviceImpl.TabletToScreen;
			measurePoint.X = (int)Math.Round(measurePoint.X);
			measurePoint.Y = (int)Math.Round(measurePoint.Y);
			measurePoint = _stylusLogic.MeasureUnitsFromDeviceUnits(wispStylusDevice.CriticalActiveSource, measurePoint);
			stylusPlugInCollection = HittestPlugInCollection(measurePoint);
		}
		return stylusPlugInCollection;
	}

	internal StylusPlugInCollection FindPlugInCollection(UIElement element)
	{
		foreach (StylusPlugInCollection plugInCollection in _plugInCollectionList)
		{
			if (plugInCollection.Element == element || plugInCollection.Element.IsAncestorOf(element))
			{
				return plugInCollection;
			}
		}
		return null;
	}

	private StylusPlugInCollection HittestPlugInCollection(Point pt)
	{
		foreach (StylusPlugInCollection plugInCollection in _plugInCollectionList)
		{
			if (plugInCollection.IsHit(pt))
			{
				return plugInCollection;
			}
		}
		return null;
	}
}
