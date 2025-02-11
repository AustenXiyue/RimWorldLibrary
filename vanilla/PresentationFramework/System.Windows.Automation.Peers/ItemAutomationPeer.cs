using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Data;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes a data item in an <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection to UI Automation. </summary>
public abstract class ItemAutomationPeer : AutomationPeer, IVirtualizedItemProvider
{
	private class ItemWeakReference : WeakReference
	{
		public ItemWeakReference(object o)
			: base(o)
		{
		}
	}

	private object _item;

	private ItemsControlAutomationPeer _itemsControlAutomationPeer;

	internal override bool AncestorsInvalid
	{
		get
		{
			return base.AncestorsInvalid;
		}
		set
		{
			base.AncestorsInvalid = value;
			if (!value)
			{
				AutomationPeer wrapperPeer = GetWrapperPeer();
				if (wrapperPeer != null)
				{
					wrapperPeer.AncestorsInvalid = false;
				}
			}
		}
	}

	/// <summary>Gets the data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />.</summary>
	/// <returns>The data item.</returns>
	public object Item
	{
		get
		{
			if (!(_item is ItemWeakReference itemWeakReference))
			{
				return _item;
			}
			return itemWeakReference.Target;
		}
		private set
		{
			if (value != null && !value.GetType().IsValueType && !FrameworkAppContextSwitches.ItemAutomationPeerKeepsItsItemAlive)
			{
				_item = new ItemWeakReference(value);
			}
			else
			{
				_item = value;
			}
		}
	}

	private object RawItem
	{
		get
		{
			if (_item is ItemWeakReference itemWeakReference)
			{
				object target = itemWeakReference.Target;
				if (target != null)
				{
					return target;
				}
				return DependencyProperty.UnsetValue;
			}
			return _item;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" /> that is associated with the <see cref="T:System.Windows.Controls.ItemsControl" /> that holds the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" /> that is associated with the <see cref="T:System.Windows.Controls.ItemsControl" /> object.</returns>
	public ItemsControlAutomationPeer ItemsControlAutomationPeer
	{
		get
		{
			return GetItemsControlAutomationPeer();
		}
		internal set
		{
			_itemsControlAutomationPeer = value;
		}
	}

	/// <summary>Provides initialization for base class values when called by the constructor of a derived class.</summary>
	/// <param name="item">The data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />.</param>
	/// <param name="itemsControlAutomationPeer">The <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" /> that is associated with the <see cref="T:System.Windows.Controls.ItemsControl" /> that holds the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection.</param>
	protected ItemAutomationPeer(object item, ItemsControlAutomationPeer itemsControlAutomationPeer)
	{
		Item = item;
		_itemsControlAutomationPeer = itemsControlAutomationPeer;
	}

	/// <summary>Returns the object that supports the specified control pattern of the element that is associated with this automation peer.</summary>
	/// <returns>An object that supports the control pattern if <paramref name="patternInterface" /> is a supported value; otherwise, null. </returns>
	/// <param name="patternInterface">An enumeration value that specifies the control pattern.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.VirtualizedItem:
			if (VirtualizedItemPatternIdentifiers.Pattern != null)
			{
				if (GetWrapperPeer() == null)
				{
					return this;
				}
				if (ItemsControlAutomationPeer != null && !IsItemInAutomationTree())
				{
					return this;
				}
				if (ItemsControlAutomationPeer == null)
				{
					return this;
				}
			}
			return null;
		case PatternInterface.SynchronizedInput:
			if (GetWrapperPeer() is UIElementAutomationPeer uIElementAutomationPeer)
			{
				return uIElementAutomationPeer.GetPattern(patternInterface);
			}
			break;
		}
		return null;
	}

	internal UIElement GetWrapper()
	{
		UIElement result = null;
		ItemsControlAutomationPeer itemsControlAutomationPeer = ItemsControlAutomationPeer;
		if (itemsControlAutomationPeer != null)
		{
			ItemsControl itemsControl = (ItemsControl)itemsControlAutomationPeer.Owner;
			if (itemsControl != null)
			{
				object rawItem = RawItem;
				if (rawItem != DependencyProperty.UnsetValue)
				{
					result = ((!((IGeneratorHost)itemsControl).IsItemItsOwnContainer(rawItem)) ? (itemsControl.ItemContainerGenerator.ContainerFromItem(rawItem) as UIElement) : (rawItem as UIElement));
				}
			}
		}
		return result;
	}

	internal virtual AutomationPeer GetWrapperPeer()
	{
		AutomationPeer automationPeer = null;
		UIElement wrapper = GetWrapper();
		if (wrapper != null)
		{
			automationPeer = UIElementAutomationPeer.CreatePeerForElement(wrapper);
			if (automationPeer == null)
			{
				automationPeer = ((!(wrapper is FrameworkElement)) ? new UIElementAutomationPeer(wrapper) : new FrameworkElementAutomationPeer((FrameworkElement)wrapper));
			}
		}
		return automationPeer;
	}

	internal void ThrowElementNotAvailableException()
	{
		if (VirtualizedItemPatternIdentifiers.Pattern != null && !(this is GridViewItemAutomationPeer) && !IsItemInAutomationTree())
		{
			throw new ElementNotAvailableException(SR.VirtualizedElement);
		}
	}

	private bool IsItemInAutomationTree()
	{
		AutomationPeer parent = GetParent();
		if (base.Index != -1 && parent != null && parent.Children != null && base.Index < parent.Children.Count && parent.Children[base.Index] == this)
		{
			return true;
		}
		return false;
	}

	internal override bool IgnoreUpdatePeer()
	{
		if (!IsItemInAutomationTree())
		{
			return true;
		}
		return base.IgnoreUpdatePeer();
	}

	internal override bool IsDataItemAutomationPeer()
	{
		return true;
	}

	internal override void AddToParentProxyWeakRefCache()
	{
		ItemsControlAutomationPeer?.AddProxyToWeakRefStorage(base.ElementProxyWeakReference, this);
	}

	internal override Rect GetVisibleBoundingRectCore()
	{
		return GetWrapperPeer()?.GetVisibleBoundingRectCore() ?? GetBoundingRectangle();
	}

	/// <summary>Gets a human-readable string that contains the type of item that the specified <see cref="T:System.Windows.UIElement" /> represents. </summary>
	/// <returns>The item type. An example includes "Mail Message" or "Contact".</returns>
	protected override string GetItemTypeCore()
	{
		return string.Empty;
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.UIElement" /> that corresponds to the data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />. </summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			wrapperPeer.ForceEnsureChildren();
			return wrapperPeer.GetChildren();
		}
		return null;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Rect" /> that represents the bounding rectangle of the specified <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>The bounding rectangle.</returns>
	protected override Rect GetBoundingRectangleCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetBoundingRectangle();
		}
		ThrowElementNotAvailableException();
		return default(Rect);
	}

	/// <summary>Gets a value that indicates whether the specified <see cref="T:System.Windows.UIElement" /> is off the screen. </summary>
	/// <returns>true if the specified <see cref="T:System.Windows.UIElement" /> is not on the screen; otherwise, false.</returns>
	protected override bool IsOffscreenCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsOffscreen();
		}
		ThrowElementNotAvailableException();
		return true;
	}

	/// <summary>Gets a value that indicates whether the specified <see cref="T:System.Windows.UIElement" /> is laid out in a particular direction. </summary>
	/// <returns>The direction of the specified <see cref="T:System.Windows.UIElement" />. Optionally, the method returns <see cref="F:System.Windows.Automation.Peers.AutomationOrientation.None" /> if the <see cref="T:System.Windows.UIElement" /> is not laid out in a particular direction.</returns>
	protected override AutomationOrientation GetOrientationCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetOrientation();
		}
		ThrowElementNotAvailableException();
		return AutomationOrientation.None;
	}

	protected override AutomationHeadingLevel GetHeadingLevelCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		AutomationHeadingLevel result = AutomationHeadingLevel.None;
		if (wrapperPeer != null)
		{
			result = wrapperPeer.GetHeadingLevel();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
		return result;
	}

	protected override int GetPositionInSetCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			int num = wrapperPeer.GetPositionInSet();
			if (num == -1)
			{
				num = GetPositionInSetFromItemsControl((ItemsControl)ItemsControlAutomationPeer.Owner, Item);
			}
			return num;
		}
		ThrowElementNotAvailableException();
		return -1;
	}

	protected override int GetSizeOfSetCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			int num = wrapperPeer.GetSizeOfSet();
			if (num == -1)
			{
				num = GetSizeOfSetFromItemsControl((ItemsControl)ItemsControlAutomationPeer.Owner, Item);
			}
			return num;
		}
		ThrowElementNotAvailableException();
		return -1;
	}

	internal static int GetPositionInSetFromItemsControl(ItemsControl itemsControl, object item)
	{
		int num = -1;
		ItemCollection items = itemsControl.Items;
		num = items.IndexOf(item);
		if (itemsControl.IsGrouping)
		{
			num = FindPositionInGroup(items.Groups, num, out var _);
		}
		return num + 1;
	}

	internal static int GetSizeOfSetFromItemsControl(ItemsControl itemsControl, object item)
	{
		int sizeOfGroup = -1;
		ItemCollection items = itemsControl.Items;
		if (itemsControl.IsGrouping)
		{
			int position = items.IndexOf(item);
			FindPositionInGroup(items.Groups, position, out sizeOfGroup);
		}
		else
		{
			sizeOfGroup = items.Count;
		}
		return sizeOfGroup;
	}

	private static int FindPositionInGroup(ReadOnlyObservableCollection<object> collection, int position, out int sizeOfGroup)
	{
		CollectionViewGroupInternal collectionViewGroupInternal = null;
		ReadOnlyObservableCollection<object> readOnlyObservableCollection = null;
		sizeOfGroup = -1;
		do
		{
			readOnlyObservableCollection = null;
			foreach (CollectionViewGroupInternal item in collection)
			{
				if (position < item.ItemCount)
				{
					collectionViewGroupInternal = item;
					if (collectionViewGroupInternal.IsBottomLevel)
					{
						readOnlyObservableCollection = null;
						sizeOfGroup = item.ItemCount;
					}
					else
					{
						readOnlyObservableCollection = collectionViewGroupInternal.Items;
					}
					break;
				}
				position -= item.ItemCount;
			}
		}
		while ((collection = readOnlyObservableCollection) != null);
		return position;
	}

	/// <summary>Gets a string that conveys the visual status of the specified <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>The status. Examples include "Busy" or "Online".</returns>
	protected override string GetItemStatusCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetItemStatus();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	/// <summary>Gets a value that indicates whether the specified <see cref="T:System.Windows.UIElement" /> is required to be completed on a form. </summary>
	/// <returns>true if the specified <see cref="T:System.Windows.UIElement" /> is required to be completed; otherwise, false.</returns>
	protected override bool IsRequiredForFormCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsRequiredForForm();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	/// <summary>Gets a value that indicates whether the specified <see cref="T:System.Windows.UIElement" /> can accept keyboard focus. </summary>
	/// <returns>true if the element can accept keyboard focus; otherwise, false.</returns>
	protected override bool IsKeyboardFocusableCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsKeyboardFocusable();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	/// <summary>Gets a value that indicates whether the specified <see cref="T:System.Windows.UIElement" /> currently has keyboard input focus. </summary>
	/// <returns>true if the specified <see cref="T:System.Windows.UIElement" /> has keyboard input focus; otherwise, false.</returns>
	protected override bool HasKeyboardFocusCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.HasKeyboardFocus();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	/// <summary>Gets a value that indicates whether the specified <see cref="T:System.Windows.UIElement" /> can receive and send events. </summary>
	/// <returns>true if the UI Automation peer can receive and send events; otherwise, false.</returns>
	protected override bool IsEnabledCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsEnabled();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsDialogCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsDialog();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	/// <summary>Gets a value that indicates whether the specified <see cref="T:System.Windows.UIElement" /> contains protected content. </summary>
	/// <returns>true if the specified <see cref="T:System.Windows.UIElement" /> contains protected content; otherwise, false.</returns>
	protected override bool IsPasswordCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsPassword();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	/// <summary>Gets the string that uniquely identifies the <see cref="T:System.Windows.UIElement" /> that corresponds to the data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />. </summary>
	/// <returns>A string that contains the UI Automation identifier.</returns>
	protected override string GetAutomationIdCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		string result = null;
		object item;
		if (wrapperPeer != null)
		{
			result = wrapperPeer.GetAutomationId();
		}
		else if ((item = Item) != null)
		{
			using RecyclableWrapper recyclableWrapper = ItemsControlAutomationPeer.GetRecyclableWrapperPeer(item);
			result = recyclableWrapper.Peer.GetAutomationId();
		}
		return result;
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.UIElement" /> that corresponds to the data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />. </summary>
	/// <returns>The text label.</returns>
	protected override string GetNameCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		string text = null;
		object item = Item;
		if (wrapperPeer != null)
		{
			text = wrapperPeer.GetName();
		}
		else if (item != null)
		{
			ItemsControlAutomationPeer itemsControlAutomationPeer = ItemsControlAutomationPeer;
			if (itemsControlAutomationPeer != null)
			{
				using RecyclableWrapper recyclableWrapper = itemsControlAutomationPeer.GetRecyclableWrapperPeer(item);
				text = recyclableWrapper.Peer.GetName();
			}
		}
		if (string.IsNullOrEmpty(text) && item != null)
		{
			if (item is FrameworkElement frameworkElement)
			{
				text = frameworkElement.GetPlainText();
			}
			if (string.IsNullOrEmpty(text))
			{
				text = item.ToString();
			}
		}
		return text;
	}

	/// <summary>Gets a value that indicates whether the specified <see cref="T:System.Windows.UIElement" /> contains data that is presented to the user. </summary>
	/// <returns>true if the element is a content element; otherwise, false.</returns>
	protected override bool IsContentElementCore()
	{
		return GetWrapperPeer()?.IsContentElement() ?? true;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.UIElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> is understood by the end user as interactive. </summary>
	/// <returns>true if the element is a control; otherwise, false.</returns>
	protected override bool IsControlElementCore()
	{
		return GetWrapperPeer()?.IsControlElement() ?? true;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.Label" /> that is targeted to the specified <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" /> for the element that is targeted by the <see cref="T:System.Windows.Controls.Label" />.</returns>
	protected override AutomationPeer GetLabeledByCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetLabeledBy();
		}
		ThrowElementNotAvailableException();
		return null;
	}

	protected override AutomationLiveSetting GetLiveSettingCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetLiveSetting();
		}
		ThrowElementNotAvailableException();
		return AutomationLiveSetting.Off;
	}

	/// <summary>Gets the string that describes the functionality of the <see cref="T:System.Windows.UIElement" /> that corresponds to the data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />. </summary>
	/// <returns>The help text.</returns>
	protected override string GetHelpTextCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetHelpText();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	/// <summary>Gets the accelerator key for the <see cref="T:System.Windows.UIElement" /> that corresponds to the data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />. </summary>
	/// <returns>The accelerator key.</returns>
	protected override string GetAcceleratorKeyCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetAcceleratorKey();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	/// <summary>Gets the access key for the <see cref="T:System.Windows.UIElement" /> that corresponds to the data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />. </summary>
	/// <returns>The access key.</returns>
	protected override string GetAccessKeyCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetAccessKey();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Point" /> that represents the clickable space that is on the specified <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>The point that represents the clickable space that is on the specified <see cref="T:System.Windows.UIElement" />.</returns>
	protected override Point GetClickablePointCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetClickablePoint();
		}
		ThrowElementNotAvailableException();
		return new Point(double.NaN, double.NaN);
	}

	/// <summary>Sets the keyboard input focus on the specified <see cref="T:System.Windows.UIElement" />. The <see cref="T:System.Windows.UIElement" /> corresponds to the data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />. </summary>
	protected override void SetFocusCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			wrapperPeer.SetFocus();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	internal virtual ItemsControlAutomationPeer GetItemsControlAutomationPeer()
	{
		return _itemsControlAutomationPeer;
	}

	internal void ReuseForItem(object item)
	{
		if (_item is ItemWeakReference itemWeakReference)
		{
			if (item != itemWeakReference.Target)
			{
				itemWeakReference.Target = item;
			}
		}
		else
		{
			_item = item;
		}
	}

	/// <summary>Makes the virtual item fully accessible as a UI Automation element.</summary>
	void IVirtualizedItemProvider.Realize()
	{
		RealizeCore();
	}

	internal virtual void RealizeCore()
	{
		ItemsControlAutomationPeer itemsControlAutomationPeer = ItemsControlAutomationPeer;
		if (itemsControlAutomationPeer == null)
		{
			return;
		}
		ItemsControl parent = itemsControlAutomationPeer.Owner as ItemsControl;
		if (parent == null)
		{
			return;
		}
		if (parent.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
		{
			if (AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures && VirtualizingPanel.GetIsVirtualizingWhenGrouping(parent))
			{
				itemsControlAutomationPeer.RecentlyRealizedPeers.Add(this);
			}
			parent.OnBringItemIntoView(Item);
			return;
		}
		base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (DispatcherOperationCallback)delegate(object arg)
		{
			if (AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures && VirtualizingPanel.GetIsVirtualizingWhenGrouping(parent))
			{
				itemsControlAutomationPeer.RecentlyRealizedPeers.Add(this);
			}
			parent.OnBringItemIntoView(arg);
			return (object)null;
		}, Item);
	}
}
