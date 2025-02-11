using System.Collections.Generic;
using System.Windows.Input;
using MS.Internal.Automation;
using MS.Internal.PresentationCore;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.ContentElement" /> types to UI Automation.</summary>
public class ContentElementAutomationPeer : AutomationPeer
{
	private ContentElement _owner;

	private SynchronizedInputAdaptor _synchronizedInputPattern;

	/// <summary>Gets the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</summary>
	/// <returns>The <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</returns>
	public ContentElement Owner => _owner;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</param>
	public ContentElementAutomationPeer(ContentElement owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		_owner = owner;
	}

	/// <summary>Creates a <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> for the specified <see cref="T:System.Windows.ContentElement" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> for the specified <see cref="T:System.Windows.ContentElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</param>
	public static AutomationPeer CreatePeerForElement(ContentElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.CreateAutomationPeer();
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> for the specified <see cref="T:System.Windows.ContentElement" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> for the specified <see cref="T:System.Windows.ContentElement" />, or null if the <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> has not been created by the <see cref="M:System.Windows.Automation.Peers.ContentElementAutomationPeer.CreatePeerForElement(System.Windows.ContentElement)" /> method.</returns>
	/// <param name="element">The <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</param>
	public static AutomationPeer FromElement(ContentElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.GetAutomationPeer();
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		return null;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</summary>
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

	/// <summary>Gets the control type for the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}

	/// <summary>Gets the string that uniquely identifies the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationId" />.</summary>
	/// <returns>A string containing the automation identifier.</returns>
	protected override string GetAutomationIdCore()
	{
		return AutomationProperties.GetAutomationId(_owner);
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the label.</returns>
	protected override string GetNameCore()
	{
		return AutomationProperties.GetName(_owner);
	}

	/// <summary>Gets the string that describes the functionality of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetHelpText" />.</summary>
	/// <returns>The string that describes the functionality of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</returns>
	protected override string GetHelpTextCore()
	{
		return AutomationProperties.GetHelpText(_owner);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Rect" /> representing the bounding rectangle of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetBoundingRectangle" />.</summary>
	/// <returns>The bounding rectangle.</returns>
	protected override Rect GetBoundingRectangleCore()
	{
		return Rect.Empty;
	}

	/// <summary>Gets a value that indicates whether <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> is off of the screen. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" />.</summary>
	/// <returns>true if the element is not on the screen; otherwise, false.</returns>
	protected override bool IsOffscreenCore()
	{
		if (AutomationProperties.GetIsOffscreenBehavior(_owner) == IsOffscreenBehavior.Onscreen)
		{
			return false;
		}
		return true;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> is laid out in a specific direction. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetOrientation" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationOrientation.None" />.</returns>
	protected override AutomationOrientation GetOrientationCore()
	{
		return AutomationOrientation.None;
	}

	/// <summary>Gets a human-readable string that contains the type of the item that the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> represents. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetItemType" />.</summary>
	/// <returns>The string that contains the item type.</returns>
	protected override string GetItemTypeCore()
	{
		return string.Empty;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>
	///   <see cref="F:System.String.Empty" />.</returns>
	protected override string GetClassNameCore()
	{
		return string.Empty;
	}

	/// <summary>Gets a string that conveys the visual status of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetItemStatus" />.</summary>
	/// <returns>A string containing the status.</returns>
	protected override string GetItemStatusCore()
	{
		return string.Empty;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> is required to be filled out on a form. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsRequiredForForm" />.</summary>
	/// <returns>false.</returns>
	protected override bool IsRequiredForFormCore()
	{
		return false;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> can accept keyboard focus. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsKeyboardFocusable" />.</summary>
	/// <returns>false.</returns>
	protected override bool IsKeyboardFocusableCore()
	{
		return Keyboard.IsFocusable(_owner);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> currently has keyboard input focus. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.HasKeyboardFocus" />.</summary>
	/// <returns>true if the element has keyboard input focus; otherwise, false.</returns>
	protected override bool HasKeyboardFocusCore()
	{
		return _owner.IsKeyboardFocused;
	}

	/// <summary>Gets a value that indicates whether this automation peer can receive and send events to the associated element. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsEnabled" />.</summary>
	/// <returns>true if the automation peer can receive and send events; otherwise, false.</returns>
	protected override bool IsEnabledCore()
	{
		return _owner.IsEnabled;
	}

	protected override bool IsDialogCore()
	{
		return AutomationProperties.GetIsDialog(_owner);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> contains protected content. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsPassword" />.</summary>
	/// <returns>false.</returns>
	protected override bool IsPasswordCore()
	{
		return false;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> is an element that contains data that is presented to the user. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>false.</returns>
	protected override bool IsContentElementCore()
	{
		return true;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" /> is something that the end user would understand as being interactive or as contributing to the logical structure of the control in the GUI. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>false.</returns>
	protected override bool IsControlElementCore()
	{
		return false;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.Label" /> that is targeted to the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetLabeledBy" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" /> for the element that is targeted by the <see cref="T:System.Windows.Controls.Label" />.</returns>
	protected override AutomationPeer GetLabeledByCore()
	{
		return null;
	}

	/// <summary>Gets the accelerator key for the element associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAcceleratorKey" />.</summary>
	/// <returns>
	///   <see cref="F:System.String.Empty" />.</returns>
	protected override string GetAcceleratorKeyCore()
	{
		return string.Empty;
	}

	/// <summary>Gets the access key for the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAccessKey" />.</summary>
	/// <returns>The access key for this <see cref="T:System.Windows.ContentElement" />. </returns>
	protected override string GetAccessKeyCore()
	{
		return AccessKeyManager.InternalGetAccessKeyCharacter(_owner);
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

	/// <summary>Gets a <see cref="T:System.Windows.Point" /> that represents the clickable space that is on the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>The point that represents the clickable space that is on the element.</returns>
	protected override Point GetClickablePointCore()
	{
		return new Point(double.NaN, double.NaN);
	}

	/// <summary>Sets the keyboard input focus on the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.SetFocus" />.</summary>
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
