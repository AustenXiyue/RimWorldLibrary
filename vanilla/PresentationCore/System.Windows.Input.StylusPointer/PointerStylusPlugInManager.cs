using System.Collections.Generic;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Input.Tracing;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Input.StylusPointer;

internal class PointerStylusPlugInManager
{
	private static StylusPointDescription _mousePointDescription;

	internal SecurityCriticalData<PresentationSource> _inputSource;

	private List<StylusPlugInCollection> _plugInCollectionList = new List<StylusPlugInCollection>();

	[ThreadStatic]
	private static StylusPlugInCollection _activeMousePlugInCollection;

	private static StylusPointDescription MousePointDescription
	{
		get
		{
			if (_mousePointDescription == null)
			{
				_mousePointDescription = new StylusPointDescription(new StylusPointPropertyInfo[6]
				{
					StylusPointPropertyInfoDefaults.X,
					StylusPointPropertyInfoDefaults.Y,
					StylusPointPropertyInfoDefaults.NormalPressure,
					StylusPointPropertyInfoDefaults.PacketStatus,
					StylusPointPropertyInfoDefaults.TipButton,
					StylusPointPropertyInfoDefaults.BarrelButton
				}, -1);
			}
			return _mousePointDescription;
		}
	}

	internal PointerStylusPlugInManager(PresentationSource source)
	{
		_inputSource = new SecurityCriticalData<PresentationSource>(source);
	}

	internal void AddStylusPlugInCollection(StylusPlugInCollection pic)
	{
		_plugInCollectionList.Insert(FindZOrderIndex(pic), pic);
	}

	internal void RemoveStylusPlugInCollection(StylusPlugInCollection pic)
	{
		_plugInCollectionList.Remove(pic);
	}

	private Matrix GetTabletToViewTransform(TabletDevice tablet)
	{
		Matrix matrix = (_inputSource.Value as HwndSource)?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
		matrix.Invert();
		return matrix * tablet.TabletDeviceImpl.TabletToScreen;
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

	internal StylusPlugInCollection TargetPlugInCollection(RawStylusInputReport inputReport)
	{
		StylusPlugInCollection stylusPlugInCollection = null;
		PointerStylusDevice pointerStylusDevice = inputReport.StylusDevice.As<PointerStylusDevice>();
		bool elementHasCapture = false;
		stylusPlugInCollection = pointerStylusDevice.GetCapturedPlugInCollection(ref elementHasCapture);
		int inputArrayLengthPerPoint = inputReport.StylusPointDescription.GetInputArrayLengthPerPoint();
		if (elementHasCapture && !_plugInCollectionList.Contains(stylusPlugInCollection))
		{
			elementHasCapture = false;
		}
		if (!elementHasCapture && inputReport.Data != null && inputReport.Data.Length >= inputArrayLengthPerPoint)
		{
			int[] data = inputReport.Data;
			Point pt = new Point(data[^inputArrayLengthPerPoint], data[data.Length - inputArrayLengthPerPoint + 1]);
			pt *= pointerStylusDevice.TabletDevice.TabletDeviceImpl.TabletToScreen;
			pt.X = (int)Math.Round(pt.X);
			pt.Y = (int)Math.Round(pt.Y);
			pt *= inputReport.InputSource.CompositionTarget.TransformFromDevice;
			stylusPlugInCollection = HittestPlugInCollection(pt);
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

	internal void VerifyStylusPlugInCollectionTarget(RawStylusInputReport rawStylusInputReport)
	{
		RawStylusActions actions = rawStylusInputReport.Actions;
		if (actions != RawStylusActions.Down && actions != RawStylusActions.Up && actions != RawStylusActions.Move)
		{
			return;
		}
		PointerLogic currentStylusLogicAs = StylusLogic.GetCurrentStylusLogicAs<PointerLogic>();
		RawStylusInput rawStylusInput = rawStylusInputReport.RawStylusInput;
		StylusPlugInCollection stylusPlugInCollection = null;
		StylusPlugInCollection stylusPlugInCollection2 = rawStylusInput?.Target;
		bool flag = false;
		if (InputElement.GetContainingUIElement(rawStylusInputReport.StylusDevice.DirectlyOver as DependencyObject) is UIElement element)
		{
			stylusPlugInCollection = FindPlugInCollection(element);
		}
		using (Dispatcher.CurrentDispatcher.DisableProcessing())
		{
			if (stylusPlugInCollection2 != null && stylusPlugInCollection2 != stylusPlugInCollection && rawStylusInput != null)
			{
				foreach (RawStylusInputCustomData customData in rawStylusInput.CustomDataList)
				{
					customData.Owner.FireCustomData(customData.Data, rawStylusInputReport.Actions, targetVerified: false);
				}
				flag = rawStylusInput.StylusPointsModified;
				rawStylusInputReport.RawStylusInput = null;
			}
			bool flag2 = false;
			if (stylusPlugInCollection != null && rawStylusInputReport.RawStylusInput == null)
			{
				GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
				generalTransformGroup.Children.Add(rawStylusInputReport.StylusDevice.As<PointerStylusDevice>().GetTabletToElementTransform(null));
				generalTransformGroup.Children.Add(stylusPlugInCollection.ViewToElement);
				generalTransformGroup.Freeze();
				RawStylusInput rawStylusInput2 = new RawStylusInput(rawStylusInputReport, generalTransformGroup, stylusPlugInCollection);
				rawStylusInputReport.RawStylusInput = rawStylusInput2;
				flag2 = true;
			}
			PointerStylusDevice pointerStylusDevice = rawStylusInputReport.StylusDevice.As<PointerStylusDevice>();
			StylusPlugInCollection currentVerifiedTarget = pointerStylusDevice.CurrentVerifiedTarget;
			if (stylusPlugInCollection != currentVerifiedTarget)
			{
				if (currentVerifiedTarget != null)
				{
					if (rawStylusInput == null)
					{
						GeneralTransformGroup generalTransformGroup2 = new GeneralTransformGroup();
						generalTransformGroup2.Children.Add(pointerStylusDevice.GetTabletToElementTransform(null));
						generalTransformGroup2.Children.Add(currentVerifiedTarget.ViewToElement);
						generalTransformGroup2.Freeze();
						rawStylusInput = new RawStylusInput(rawStylusInputReport, generalTransformGroup2, currentVerifiedTarget);
					}
					currentVerifiedTarget.FireEnterLeave(isEnter: false, rawStylusInput, confirmed: true);
				}
				if (stylusPlugInCollection != null)
				{
					stylusPlugInCollection.FireEnterLeave(isEnter: true, rawStylusInputReport.RawStylusInput, confirmed: true);
					currentStylusLogicAs.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
				}
				pointerStylusDevice.CurrentVerifiedTarget = stylusPlugInCollection;
			}
			if (flag2)
			{
				stylusPlugInCollection.FireRawStylusInput(rawStylusInputReport.RawStylusInput);
				flag = flag || rawStylusInputReport.RawStylusInput.StylusPointsModified;
				currentStylusLogicAs.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
			}
			if (stylusPlugInCollection != null)
			{
				foreach (RawStylusInputCustomData customData2 in rawStylusInputReport.RawStylusInput.CustomDataList)
				{
					customData2.Owner.FireCustomData(customData2.Data, rawStylusInputReport.Actions, targetVerified: true);
				}
			}
			if (flag)
			{
				rawStylusInputReport.StylusDevice.As<PointerStylusDevice>().UpdateEventStylusPoints(rawStylusInputReport, resetIfNoOverride: true);
			}
		}
	}

	internal StylusPlugInCollection InvokeStylusPluginCollectionForMouse(RawStylusInputReport inputReport, IInputElement directlyOver, StylusPlugInCollection currentPlugInCollection)
	{
		StylusPlugInCollection stylusPlugInCollection = null;
		if (directlyOver != null && InputElement.GetContainingUIElement(directlyOver as DependencyObject) is UIElement element)
		{
			stylusPlugInCollection = FindPlugInCollection(element);
		}
		if (currentPlugInCollection != null && currentPlugInCollection != stylusPlugInCollection)
		{
			RawStylusInput rawStylusInput = new RawStylusInput(inputReport, currentPlugInCollection.ViewToElement, currentPlugInCollection);
			currentPlugInCollection.FireEnterLeave(isEnter: false, rawStylusInput, confirmed: true);
			StylusLogic.CurrentStylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
		}
		if (stylusPlugInCollection != null)
		{
			RawStylusInput rawStylusInput3 = (inputReport.RawStylusInput = new RawStylusInput(inputReport, stylusPlugInCollection.ViewToElement, stylusPlugInCollection));
			if (stylusPlugInCollection != currentPlugInCollection)
			{
				stylusPlugInCollection.FireEnterLeave(isEnter: true, rawStylusInput3, confirmed: true);
			}
			stylusPlugInCollection.FireRawStylusInput(rawStylusInput3);
			StylusLogic.CurrentStylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
			foreach (RawStylusInputCustomData customData in rawStylusInput3.CustomDataList)
			{
				customData.Owner.FireCustomData(customData.Data, inputReport.Actions, targetVerified: true);
			}
		}
		return stylusPlugInCollection;
	}

	internal void InvokeStylusPluginCollection(RawStylusInputReport inputReport)
	{
		StylusPlugInCollection stylusPlugInCollection = null;
		RawStylusActions actions = inputReport.Actions;
		if (actions != RawStylusActions.Down && actions != RawStylusActions.Up && actions != RawStylusActions.Move)
		{
			return;
		}
		stylusPlugInCollection = TargetPlugInCollection(inputReport);
		PointerStylusDevice pointerStylusDevice = inputReport.StylusDevice.As<PointerStylusDevice>();
		StylusPlugInCollection currentVerifiedTarget = pointerStylusDevice.CurrentVerifiedTarget;
		if (currentVerifiedTarget != null && currentVerifiedTarget != stylusPlugInCollection)
		{
			GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
			generalTransformGroup.Children.Add(new MatrixTransform(GetTabletToViewTransform(pointerStylusDevice.TabletDevice)));
			generalTransformGroup.Children.Add(currentVerifiedTarget.ViewToElement);
			generalTransformGroup.Freeze();
			RawStylusInput rawStylusInput = new RawStylusInput(inputReport, generalTransformGroup, currentVerifiedTarget);
			currentVerifiedTarget.FireEnterLeave(isEnter: false, rawStylusInput, confirmed: false);
			StylusLogic.CurrentStylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
			pointerStylusDevice.CurrentVerifiedTarget = null;
		}
		if (stylusPlugInCollection != null)
		{
			GeneralTransformGroup generalTransformGroup2 = new GeneralTransformGroup();
			generalTransformGroup2.Children.Add(new MatrixTransform(GetTabletToViewTransform(pointerStylusDevice.TabletDevice)));
			generalTransformGroup2.Children.Add(stylusPlugInCollection.ViewToElement);
			generalTransformGroup2.Freeze();
			RawStylusInput rawStylusInput3 = (inputReport.RawStylusInput = new RawStylusInput(inputReport, generalTransformGroup2, stylusPlugInCollection));
			if (stylusPlugInCollection != currentVerifiedTarget)
			{
				pointerStylusDevice.CurrentVerifiedTarget = stylusPlugInCollection;
				stylusPlugInCollection.FireEnterLeave(isEnter: true, rawStylusInput3, confirmed: false);
			}
			stylusPlugInCollection.FireRawStylusInput(rawStylusInput3);
			StylusLogic.CurrentStylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
		}
	}

	internal static void InvokePlugInsForMouse(ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.Handled || (e.StagingItem.Input.RoutedEvent != Mouse.PreviewMouseDownEvent && e.StagingItem.Input.RoutedEvent != Mouse.PreviewMouseUpEvent && e.StagingItem.Input.RoutedEvent != Mouse.PreviewMouseMoveEvent && e.StagingItem.Input.RoutedEvent != InputManager.InputReportEvent))
		{
			return;
		}
		RawStylusActions actions = RawStylusActions.None;
		MouseDevice mouseDevice;
		bool flag;
		bool flag2;
		int timestamp;
		PresentationSource presentationSource;
		if (e.StagingItem.Input.RoutedEvent == InputManager.InputReportEvent)
		{
			if (_activeMousePlugInCollection?.Element == null)
			{
				return;
			}
			InputReportEventArgs inputReportEventArgs = e.StagingItem.Input as InputReportEventArgs;
			if (inputReportEventArgs.Report.Type != InputType.Mouse)
			{
				return;
			}
			RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)inputReportEventArgs.Report;
			if ((rawMouseInputReport.Actions & RawMouseActions.Deactivate) != RawMouseActions.Deactivate)
			{
				return;
			}
			mouseDevice = InputManager.UnsecureCurrent.PrimaryMouseDevice;
			if (mouseDevice == null || mouseDevice.DirectlyOver != null)
			{
				return;
			}
			flag = mouseDevice.LeftButton == MouseButtonState.Pressed;
			flag2 = mouseDevice.RightButton == MouseButtonState.Pressed;
			timestamp = rawMouseInputReport.Timestamp;
			presentationSource = PresentationSource.CriticalFromVisual(_activeMousePlugInCollection.Element);
		}
		else
		{
			MouseEventArgs mouseEventArgs = e.StagingItem.Input as MouseEventArgs;
			mouseDevice = mouseEventArgs.MouseDevice;
			flag = mouseDevice.LeftButton == MouseButtonState.Pressed;
			flag2 = mouseDevice.RightButton == MouseButtonState.Pressed;
			if (mouseEventArgs.StylusDevice != null && e.StagingItem.Input.RoutedEvent != Mouse.PreviewMouseUpEvent)
			{
				return;
			}
			if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseMoveEvent)
			{
				if (!flag)
				{
					return;
				}
				actions = RawStylusActions.Move;
			}
			if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseDownEvent)
			{
				if ((mouseEventArgs as MouseButtonEventArgs).ChangedButton != 0)
				{
					return;
				}
				actions = RawStylusActions.Down;
			}
			if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseUpEvent)
			{
				if ((mouseEventArgs as MouseButtonEventArgs).ChangedButton != 0)
				{
					return;
				}
				actions = RawStylusActions.Up;
			}
			timestamp = mouseEventArgs.Timestamp;
			if (!(mouseDevice.DirectlyOver is Visual v))
			{
				return;
			}
			presentationSource = PresentationSource.CriticalFromVisual(v);
		}
		if (presentationSource != null && presentationSource.CompositionTarget != null && !presentationSource.CompositionTarget.IsDisposed)
		{
			IInputElement directlyOver = mouseDevice.DirectlyOver;
			int num = (flag ? 1 : 0) | (flag2 ? 9 : 0);
			Point position = mouseDevice.GetPosition(presentationSource.RootVisual as IInputElement);
			position = presentationSource.CompositionTarget.TransformToDevice.Transform(position);
			int num2 = (flag ? 1 : 0) | (flag2 ? 3 : 0);
			int[] data = new int[4]
			{
				(int)position.X,
				(int)position.Y,
				num,
				num2
			};
			RawStylusInputReport inputReport = new RawStylusInputReport(InputMode.Foreground, timestamp, presentationSource, actions, () => MousePointDescription, 0, 0, data);
			PointerStylusPlugInManager pointerStylusPlugInManager = StylusLogic.GetCurrentStylusLogicAs<PointerLogic>()?.GetManagerForSource(presentationSource);
			using (Dispatcher.CurrentDispatcher.DisableProcessing())
			{
				_activeMousePlugInCollection = pointerStylusPlugInManager?.InvokeStylusPluginCollectionForMouse(inputReport, directlyOver, _activeMousePlugInCollection);
			}
		}
	}
}
