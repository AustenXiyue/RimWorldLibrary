using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Automation;
using MS.Internal.PresentationCore;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.UIElement3D" /> types to UI Automation.</summary>
public class UIElement3DAutomationPeer : AutomationPeer
{
	private delegate bool IteratorCallback(AutomationPeer peer);

	private UIElement3D _owner;

	private SynchronizedInputAdaptor _synchronizedInputPattern;

	/// <summary>Gets the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</summary>
	/// <returns>The <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</returns>
	public UIElement3D Owner => _owner;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> class. </summary>
	/// <param name="owner">The <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</param>
	public UIElement3DAutomationPeer(UIElement3D owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		_owner = owner;
	}

	/// <summary>Creates a <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> for the specified <see cref="T:System.Windows.UIElement3D" />.</summary>
	/// <returns>A  <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> for the specified <see cref="T:System.Windows.UIElement3D" />.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</param>
	public static AutomationPeer CreatePeerForElement(UIElement3D element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.CreateAutomationPeer();
	}

	/// <summary>Returns the <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> for the specified <see cref="T:System.Windows.UIElement3D" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />, or null if the <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> was not created by the <see cref="M:System.Windows.Automation.Peers.UIElement3DAutomationPeer.CreatePeerForElement(System.Windows.UIElement3D)" /> method.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</param>
	public static AutomationPeer FromElement(UIElement3D element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.GetAutomationPeer();
	}

	/// <summary>Returns the collection of child elements of the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> elements than correspond to the child elements of the <see cref="T:System.Windows.UIElement3D" />.</returns>
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
				flag = ((child != null && child is UIElement && (automationPeer = UIElementAutomationPeer.CreatePeerForElement((UIElement)child)) != null) ? callback(automationPeer) : ((child == null || !(child is UIElement3D) || (automationPeer = CreatePeerForElement((UIElement3D)child)) == null) ? iterate(child, callback) : callback(automationPeer)));
			}
		}
		return flag;
	}

	/// <summary>Returns the control pattern for the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</summary>
	/// <returns>An object that implements the <see cref="T:System.Windows.Automation.Provider.ISynchronizedInputProvider" /> interface if <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.SynchronizedInput" />; otherwise, null.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
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

	/// <summary>Returns the control type for the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" /> in all cases.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}

	/// <summary>Returns the string that uniquely identifies the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationId" />.</summary>
	/// <returns>The string that uniquely identifies the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</returns>
	protected override string GetAutomationIdCore()
	{
		return AutomationProperties.GetAutomationId(_owner);
	}

	/// <summary>Returns the string that represents the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that represents the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</returns>
	protected override string GetNameCore()
	{
		return AutomationProperties.GetName(_owner);
	}

	/// <summary>Returns the string that describes the functionality of the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetHelpText" />.</summary>
	/// <returns>A string that describes the functionality of the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</returns>
	protected override string GetHelpTextCore()
	{
		return AutomationProperties.GetHelpText(_owner);
	}

	/// <summary>Returns the <see cref="T:System.Windows.Rect" /> that represents the bounding rectangle of the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetBoundingRectangle" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Rect" /> that contains the coordinates of the element. </returns>
	protected override Rect GetBoundingRectangleCore()
	{
		if (!ComputeBoundingRectangle(out var rect))
		{
			return Rect.Empty;
		}
		return rect;
	}

	private bool ComputeBoundingRectangle(out Rect rect)
	{
		rect = Rect.Empty;
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(_owner);
		if (presentationSource == null)
		{
			return false;
		}
		if (!(presentationSource is HwndSource hwndSource))
		{
			return false;
		}
		Rect rectClient = PointUtil.RootToClient(PointUtil.ElementToRoot(_owner.Visual2DContentBounds, VisualTreeHelper.GetContainingVisual2D(_owner), presentationSource), presentationSource);
		rect = PointUtil.ClientToScreen(rectClient, hwndSource);
		return true;
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> is off the screen. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" />.</summary>
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
				UIElement containingUIElement = GetContainingUIElement(_owner);
				if (containingUIElement != null)
				{
					Rect rect = UIElementAutomationPeer.CalculateVisibleBoundingRect(containingUIElement);
					flag = DoubleUtil.AreClose(rect, Rect.Empty) || DoubleUtil.AreClose(rect.Height, 0.0) || DoubleUtil.AreClose(rect.Width, 0.0);
				}
			}
			return flag;
		}
		default:
			return !_owner.IsVisible;
		}
	}

	private static UIElement GetContainingUIElement(DependencyObject reference)
	{
		UIElement uIElement = null;
		while (reference != null)
		{
			uIElement = reference as UIElement;
			if (uIElement != null)
			{
				break;
			}
			reference = VisualTreeHelper.GetParent(reference);
		}
		return uIElement;
	}

	/// <summary>Returns the orientation of the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetOrientation" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationOrientation.None" /> in all cases.</returns>
	protected override AutomationOrientation GetOrientationCore()
	{
		return AutomationOrientation.None;
	}

	/// <summary>Returns a human-readable string that represents the item type that the <see cref="T:System.Windows.UIElement3D" /> for this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetItemType" />.</summary>
	/// <returns>A string that represents the item type that the <see cref="T:System.Windows.UIElement3D" /> for this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</returns>
	protected override string GetItemTypeCore()
	{
		return AutomationProperties.GetItemType(_owner);
	}

	/// <summary>Returns the name of the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>
	///   <see cref="F:System.String.Empty" /> in all cases.</returns>
	protected override string GetClassNameCore()
	{
		return string.Empty;
	}

	/// <summary>Returns a string that communicates the status of the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetItemStatus" />.</summary>
	/// <returns>The status of the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</returns>
	protected override string GetItemStatusCore()
	{
		return AutomationProperties.GetItemStatus(_owner);
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> is required to be completed on a form. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsRequiredForForm" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.UIElement3D" /> is required to be completed on a form; otherwise, false.</returns>
	protected override bool IsRequiredForFormCore()
	{
		return AutomationProperties.GetIsRequiredForForm(_owner);
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> can accept keyboard focus. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsKeyboardFocusable" />.</summary>
	/// <returns>true if the element can receive keyboard focus; otherwise, false.</returns>
	protected override bool IsKeyboardFocusableCore()
	{
		return Keyboard.IsFocusable(_owner);
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> currently has keyboard input focus. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.HasKeyboardFocus" />.</summary>
	/// <returns>true if the element has keyboard input focus; otherwise, false.</returns>
	protected override bool HasKeyboardFocusCore()
	{
		return _owner.IsKeyboardFocused;
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> can participate in hit testing or accept focus. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsKeyboardFocusable" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> can participate in hit testing or accept focus; otherwise, false.</returns>
	protected override bool IsEnabledCore()
	{
		return _owner.IsEnabled;
	}

	protected override bool IsDialogCore()
	{
		return AutomationProperties.GetIsDialog(_owner);
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> contains protected content. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsPassword" />.</summary>
	/// <returns>false in all cases.</returns>
	protected override bool IsPasswordCore()
	{
		return false;
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> is an element that contains data that is presented to the user. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>true in all cases.</returns>
	protected override bool IsContentElementCore()
	{
		return true;
	}

	/// <summary>Returns a value that indicates whether the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" /> is understood by the end user as interactive. Optionally, the user might understand the <see cref="T:System.Windows.UIElement3D" /> as contributing to the logical structure of the control in the GUI. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>true in all cases.</returns>
	protected override bool IsControlElementCore()
	{
		if (!base.IncludeInvisibleElementsInControlView)
		{
			return _owner.IsVisible;
		}
		return true;
	}

	/// <summary>Returns the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the element that targets the <see cref="T:System.Windows.UIElement3D" /> for this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetLabeledBy" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the element that targets the <see cref="T:System.Windows.UIElement3D" /> for this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. </returns>
	protected override AutomationPeer GetLabeledByCore()
	{
		return AutomationProperties.GetLabeledBy(_owner)?.GetAutomationPeer();
	}

	/// <summary>Returns the accelerator key for the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAcceleratorKey" />.</summary>
	/// <returns>The accelerator key for the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</returns>
	protected override string GetAcceleratorKeyCore()
	{
		return AutomationProperties.GetAcceleratorKey(_owner);
	}

	/// <summary>Returns the access key for the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAccessKey" />.</summary>
	/// <returns>The access key for the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />.</returns>
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
		return AutomationProperties.GetPositionInSet(_owner);
	}

	protected override int GetSizeOfSetCore()
	{
		return AutomationProperties.GetSizeOfSet(_owner);
	}

	protected override AutomationHeadingLevel GetHeadingLevelCore()
	{
		return AutomationProperties.GetHeadingLevel(_owner);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Point" /> that represents the clickable space that is on the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Point" /> on the element that allows a click. The point values are (<see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" />) if the element is not both a <see cref="T:System.Windows.Interop.HwndSource" /> and a <see cref="T:System.Windows.PresentationSource" />.</returns>
	protected override Point GetClickablePointCore()
	{
		Point result = new Point(double.NaN, double.NaN);
		if (ComputeBoundingRectangle(out var rect))
		{
			result = new Point(rect.Left + rect.Width * 0.5, rect.Top + rect.Height * 0.5);
		}
		return result;
	}

	/// <summary>Sets the keyboard input focus on the <see cref="T:System.Windows.UIElement3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UIElement3DAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.SetFocus" />.</summary>
	protected override void SetFocusCore()
	{
		if (!_owner.Focus())
		{
			throw new InvalidOperationException(SR.SetFocusFailed);
		}
	}

	internal override Rect GetVisibleBoundingRectCore()
	{
		return GetBoundingRectangle();
	}
}
