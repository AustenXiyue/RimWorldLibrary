using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Automation;
using MS.Internal.PresentationCore;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.UIElement" /> types to UI Automation.</summary>
public class UIElementAutomationPeer : AutomationPeer
{
	private delegate bool IteratorCallback(AutomationPeer peer);

	private UIElement _owner;

	private SynchronizedInputAdaptor _synchronizedInputPattern;

	/// <summary>Gets the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />.</summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />.</returns>
	public UIElement Owner => _owner;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />.</param>
	public UIElementAutomationPeer(UIElement owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		_owner = owner;
	}

	/// <summary>Creates a <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> for the specified <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>The created <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> for the specified <see cref="T:System.Windows.UIElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />.</param>
	public static AutomationPeer CreatePeerForElement(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.CreateAutomationPeer();
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> for the specified <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />; or null, if the <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> was not created by the <see cref="M:System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(System.Windows.UIElement)" /> method.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />.</param>
	public static AutomationPeer FromElement(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.GetAutomationPeer();
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>A list of child <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> children = null;
		iterate(_owner, delegate(AutomationPeer peer)
		{
			if (children == null)
			{
				children = new List<AutomationPeer>();
			}
			children.Add(peer);
			return false;
		});
		return children;
	}

	internal static AutomationPeer GetRootAutomationPeer(Visual rootVisual, nint hwnd)
	{
		AutomationPeer root = null;
		iterate(rootVisual, delegate(AutomationPeer peer)
		{
			root = peer;
			return true;
		});
		if (root != null)
		{
			root.Hwnd = hwnd;
		}
		return root;
	}

	private static bool iterate(DependencyObject parent, IteratorCallback callback)
	{
		bool flag = false;
		if (parent != null)
		{
			AutomationPeer automationPeer = null;
			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				if (flag)
				{
					break;
				}
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				flag = ((child != null && child is UIElement && (automationPeer = CreatePeerForElement((UIElement)child)) != null) ? callback(automationPeer) : ((child == null || !(child is UIElement3D) || (automationPeer = UIElement3DAutomationPeer.CreatePeerForElement((UIElement3D)child)) == null) ? iterate(child, callback) : callback(automationPeer)));
			}
		}
		return flag;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />.</summary>
	/// <returns>An object that implements the <see cref="T:System.Windows.Automation.Provider.ISynchronizedInputProvider" /> interface if <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.SynchronizedInput" />; otherwise, null.</returns>
	/// <param name="patternInterface">A value from the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.SynchronizedInput)
		{
			if (_synchronizedInputPattern == null)
			{
				_synchronizedInputPattern = new SynchronizedInputAdaptor(_owner);
			}
			return _synchronizedInputPattern;
		}
		return null;
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}

	/// <summary>Gets the string that uniquely identifies the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationId" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Automation.AutomationProperties.AutomationId" /> that is returned by <see cref="M:System.Windows.Automation.AutomationProperties.GetAutomationId(System.Windows.DependencyObject)" />. </returns>
	protected override string GetAutomationIdCore()
	{
		return AutomationProperties.GetAutomationId(_owner);
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the <see cref="P:System.Windows.Automation.AutomationProperties.Name" /> that is returned by <see cref="M:System.Windows.Automation.AutomationProperties.GetName(System.Windows.DependencyObject)" />.</returns>
	protected override string GetNameCore()
	{
		return AutomationProperties.GetName(_owner);
	}

	/// <summary>Gets the string that describes the functionality of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetHelpText" />.</summary>
	/// <returns>The string that contains the <see cref="P:System.Windows.Automation.AutomationProperties.HelpText" /> that is returned by <see cref="M:System.Windows.Automation.AutomationProperties.GetHelpText(System.Windows.DependencyObject)" />.</returns>
	protected override string GetHelpTextCore()
	{
		return AutomationProperties.GetHelpText(_owner);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Rect" /> that represents the bounding rectangle of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetBoundingRectangle" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Rect" /> that contains the coordinates of the element. Optionally, if the element is not both a <see cref="T:System.Windows.Interop.HwndSource" /> and a <see cref="T:System.Windows.PresentationSource" />, this method returns <see cref="P:System.Windows.Rect.Empty" />.</returns>
	protected override Rect GetBoundingRectangleCore()
	{
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(_owner);
		if (presentationSource == null)
		{
			return Rect.Empty;
		}
		if (!(presentationSource is HwndSource hwndSource))
		{
			return Rect.Empty;
		}
		return PointUtil.ClientToScreen(PointUtil.RootToClient(PointUtil.ElementToRoot(new Rect(new Point(0.0, 0.0), _owner.RenderSize), _owner, presentationSource), presentationSource), hwndSource);
	}

	internal override Rect GetVisibleBoundingRectCore()
	{
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(_owner);
		if (presentationSource == null)
		{
			return Rect.Empty;
		}
		if (!(presentationSource is HwndSource hwndSource))
		{
			return Rect.Empty;
		}
		return PointUtil.ClientToScreen(PointUtil.RootToClient(PointUtil.ElementToRoot(CalculateVisibleBoundingRect(_owner), _owner, presentationSource), presentationSource), hwndSource);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> is off the screen. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" />.</summary>
	/// <returns>true if the element is not on the screen; otherwise, false.</returns>
	protected override bool IsOffscreenCore()
	{
		switch (AutomationProperties.GetIsOffscreenBehavior(_owner))
		{
		case IsOffscreenBehavior.Onscreen:
			return false;
		case IsOffscreenBehavior.Offscreen:
			return true;
		case IsOffscreenBehavior.FromClip:
		{
			bool flag = !_owner.IsVisible;
			if (!flag)
			{
				Rect rect = CalculateVisibleBoundingRect(_owner);
				flag = DoubleUtil.AreClose(rect, Rect.Empty) || DoubleUtil.AreClose(rect.Height, 0.0) || DoubleUtil.AreClose(rect.Width, 0.0);
			}
			return flag;
		}
		default:
			return !_owner.IsVisible;
		}
	}

	internal static Rect CalculateVisibleBoundingRect(UIElement owner)
	{
		Rect rect = new Rect(owner.RenderSize);
		DependencyObject parent = VisualTreeHelper.GetParent(owner);
		while (parent != null && !DoubleUtil.AreClose(rect, Rect.Empty) && !DoubleUtil.AreClose(rect.Height, 0.0) && !DoubleUtil.AreClose(rect.Width, 0.0))
		{
			if (parent is Visual visual)
			{
				Geometry clip = VisualTreeHelper.GetClip(visual);
				if (clip != null)
				{
					GeneralTransform inverse = owner.TransformToAncestor(visual).Inverse;
					if (inverse != null)
					{
						Rect bounds = clip.Bounds;
						bounds = inverse.TransformBounds(bounds);
						rect.Intersect(bounds);
					}
					else
					{
						rect = Rect.Empty;
					}
				}
			}
			parent = VisualTreeHelper.GetParent(parent);
		}
		return rect;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> is laid out in a specific direction. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetOrientation" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationOrientation.None" /> enumeration value.</returns>
	protected override AutomationOrientation GetOrientationCore()
	{
		return AutomationOrientation.None;
	}

	/// <summary>Gets a human-readable string that contains the item type that the <see cref="T:System.Windows.UIElement" /> for this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> represents. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetItemType" />.</summary>
	/// <returns>The string that contains the <see cref="P:System.Windows.Automation.AutomationProperties.ItemType" /> that is returned by <see cref="M:System.Windows.Automation.AutomationProperties.GetItemType(System.Windows.DependencyObject)" />. </returns>
	protected override string GetItemTypeCore()
	{
		return AutomationProperties.GetItemType(_owner);
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>An <see cref="F:System.String.Empty" /> string.</returns>
	protected override string GetClassNameCore()
	{
		return string.Empty;
	}

	/// <summary>Gets a string that communicates the visual status of the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetItemStatus" />.</summary>
	/// <returns>The string that contains the <see cref="P:System.Windows.Automation.AutomationProperties.ItemStatus" /> that is returned by <see cref="M:System.Windows.Automation.AutomationProperties.GetItemStatus(System.Windows.DependencyObject)" />.</returns>
	protected override string GetItemStatusCore()
	{
		return AutomationProperties.GetItemStatus(_owner);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> is required to be completed on a form. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsRequiredForForm" />.</summary>
	/// <returns>A boolean that contains the value that is returned by <see cref="M:System.Windows.Automation.AutomationProperties.GetIsRequiredForForm(System.Windows.DependencyObject)" />, if it's set; otherwise false.</returns>
	protected override bool IsRequiredForFormCore()
	{
		return AutomationProperties.GetIsRequiredForForm(_owner);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> can accept keyboard focus. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsKeyboardFocusable" />.</summary>
	/// <returns>true if the element is focusable by the keyboard; otherwise false.</returns>
	protected override bool IsKeyboardFocusableCore()
	{
		return Keyboard.IsFocusable(_owner);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> currently has keyboard input focus. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.HasKeyboardFocus" />.</summary>
	/// <returns>true if the element has keyboard input focus; otherwise, false.</returns>
	protected override bool HasKeyboardFocusCore()
	{
		return _owner.IsKeyboardFocused;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> can accept keyboard focus. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsKeyboardFocusable" />.</summary>
	/// <returns>A boolean that contains the value of <see cref="P:System.Windows.UIElement.IsEnabled" />.</returns>
	protected override bool IsEnabledCore()
	{
		return _owner.IsEnabled;
	}

	protected override bool IsDialogCore()
	{
		return AutomationProperties.GetIsDialog(_owner);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> contains protected content. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsPassword" />.</summary>
	/// <returns>false.</returns>
	protected override bool IsPasswordCore()
	{
		return false;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> is an element that contains data that is presented to the user. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>true.</returns>
	protected override bool IsContentElementCore()
	{
		return true;
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> is understood by the end user as interactive. Optionally, the user might understand the <see cref="T:System.Windows.UIElement" /> as contributing to the logical structure of the control in the GUI. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>true.</returns>
	protected override bool IsControlElementCore()
	{
		if (!base.IncludeInvisibleElementsInControlView)
		{
			return _owner.IsVisible;
		}
		return true;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the element that is targeted to the <see cref="T:System.Windows.UIElement" /> for this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetLabeledBy" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the element that is targeted to the <see cref="T:System.Windows.UIElement" /> for this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. </returns>
	protected override AutomationPeer GetLabeledByCore()
	{
		return AutomationProperties.GetLabeledBy(_owner)?.GetAutomationPeer();
	}

	/// <summary>Gets the accelerator key for the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAcceleratorKey" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Automation.AutomationProperties.AcceleratorKey" /> that is returned by <see cref="M:System.Windows.Automation.AutomationProperties.GetAcceleratorKey(System.Windows.DependencyObject)" />.</returns>
	protected override string GetAcceleratorKeyCore()
	{
		return AutomationProperties.GetAcceleratorKey(_owner);
	}

	/// <summary>Gets the access key for the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />.This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAccessKey" />.</summary>
	/// <returns>The access key for the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />.</returns>
	protected override string GetAccessKeyCore()
	{
		if (string.IsNullOrEmpty(AutomationProperties.GetAccessKey(_owner)))
		{
			return AccessKeyManager.InternalGetAccessKeyCharacter(_owner);
		}
		return string.Empty;
	}

	protected override AutomationLiveSetting GetLiveSettingCore()
	{
		return AutomationProperties.GetLiveSetting(_owner);
	}

	protected override int GetPositionInSetCore()
	{
		int num = AutomationProperties.GetPositionInSet(_owner);
		if (num == -1)
		{
			UIElement positionAndSizeOfSetController = _owner.PositionAndSizeOfSetController;
			if (positionAndSizeOfSetController != null)
			{
				AutomationPeer automationPeer = FromElement(positionAndSizeOfSetController);
				automationPeer = automationPeer.EventsSource ?? automationPeer;
				if (automationPeer != null)
				{
					try
					{
						num = automationPeer.GetPositionInSet();
					}
					catch (ElementNotAvailableException)
					{
						num = -1;
					}
				}
			}
		}
		return num;
	}

	protected override int GetSizeOfSetCore()
	{
		int num = AutomationProperties.GetSizeOfSet(_owner);
		if (num == -1)
		{
			UIElement positionAndSizeOfSetController = _owner.PositionAndSizeOfSetController;
			if (positionAndSizeOfSetController != null)
			{
				AutomationPeer automationPeer = FromElement(positionAndSizeOfSetController);
				automationPeer = automationPeer.EventsSource ?? automationPeer;
				if (automationPeer != null)
				{
					try
					{
						num = automationPeer.GetSizeOfSet();
					}
					catch (ElementNotAvailableException)
					{
						num = -1;
					}
				}
			}
		}
		return num;
	}

	protected override AutomationHeadingLevel GetHeadingLevelCore()
	{
		return AutomationProperties.GetHeadingLevel(_owner);
	}

	/// <summary>Gets a <see cref="T:System.Windows.Point" /> that represents the clickable space that is on the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Point" /> on the element that allows a click. The point values are (<see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" />) if the element is not both a <see cref="T:System.Windows.Interop.HwndSource" /> and a <see cref="T:System.Windows.PresentationSource" />.</returns>
	protected override Point GetClickablePointCore()
	{
		Point result = new Point(double.NaN, double.NaN);
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(_owner);
		if (presentationSource == null)
		{
			return result;
		}
		if (!(presentationSource is HwndSource hwndSource))
		{
			return result;
		}
		Rect rect = PointUtil.ClientToScreen(PointUtil.RootToClient(PointUtil.ElementToRoot(new Rect(new Point(0.0, 0.0), _owner.RenderSize), _owner, presentationSource), presentationSource), hwndSource);
		return new Point(rect.Left + rect.Width * 0.5, rect.Top + rect.Height * 0.5);
	}

	/// <summary>Sets the keyboard input focus on the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.SetFocus" />.</summary>
	protected override void SetFocusCore()
	{
		if (!_owner.Focus())
		{
			throw new InvalidOperationException(SR.SetFocusFailed);
		}
	}
}
