using System;
using System.Runtime.CompilerServices;

namespace MS.Internal;

internal static class FrameworkAppContextSwitches
{
	internal const string DoNotApplyLayoutRoundingToMarginsAndBorderThicknessSwitchName = "Switch.MS.Internal.DoNotApplyLayoutRoundingToMarginsAndBorderThickness";

	private static int _doNotApplyLayoutRoundingToMarginsAndBorderThickness;

	internal const string GridStarDefinitionsCanExceedAvailableSpaceSwitchName = "Switch.System.Windows.Controls.Grid.StarDefinitionsCanExceedAvailableSpace";

	private static int _gridStarDefinitionsCanExceedAvailableSpace;

	internal const string SelectionPropertiesCanLagBehindSelectionChangedEventSwitchName = "Switch.System.Windows.Controls.TabControl.SelectionPropertiesCanLagBehindSelectionChangedEvent";

	private static int _selectionPropertiesCanLagBehindSelectionChangedEvent;

	internal const string DoNotUseFollowParentWhenBindingToADODataRelationSwitchName = "Switch.System.Windows.Data.DoNotUseFollowParentWhenBindingToADODataRelation";

	private static int _doNotUseFollowParentWhenBindingToADODataRelation;

	internal const string UseAdornerForTextboxSelectionRenderingSwitchName = "Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering";

	private static int _useAdornerForTextboxSelectionRendering;

	internal const string AppendLocalAssemblyVersionForSourceUriSwitchName = "Switch.System.Windows.Baml2006.AppendLocalAssemblyVersionForSourceUri";

	private static int _AppendLocalAssemblyVersionForSourceUriSwitchName;

	internal const string IListIndexerHidesCustomIndexerSwitchName = "Switch.System.Windows.Data.Binding.IListIndexerHidesCustomIndexer";

	private static int _IListIndexerHidesCustomIndexer;

	internal const string KeyboardNavigationFromHyperlinkInItemsControlIsNotRelativeToFocusedElementSwitchName = "Switch.System.Windows.Controls.KeyboardNavigationFromHyperlinkInItemsControlIsNotRelativeToFocusedElement";

	private static int _KeyboardNavigationFromHyperlinkInItemsControlIsNotRelativeToFocusedElement;

	internal const string ItemAutomationPeerKeepsItsItemAliveSwitchName = "Switch.System.Windows.Automation.Peers.ItemAutomationPeerKeepsItsItemAlive";

	private static int _ItemAutomationPeerKeepsItsItemAlive;

	public static bool DoNotApplyLayoutRoundingToMarginsAndBorderThickness
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.MS.Internal.DoNotApplyLayoutRoundingToMarginsAndBorderThickness", ref _doNotApplyLayoutRoundingToMarginsAndBorderThickness);
		}
	}

	public static bool GridStarDefinitionsCanExceedAvailableSpace
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Controls.Grid.StarDefinitionsCanExceedAvailableSpace", ref _gridStarDefinitionsCanExceedAvailableSpace);
		}
	}

	public static bool SelectionPropertiesCanLagBehindSelectionChangedEvent
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Controls.TabControl.SelectionPropertiesCanLagBehindSelectionChangedEvent", ref _selectionPropertiesCanLagBehindSelectionChangedEvent);
		}
	}

	public static bool DoNotUseFollowParentWhenBindingToADODataRelation
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Data.DoNotUseFollowParentWhenBindingToADODataRelation", ref _doNotUseFollowParentWhenBindingToADODataRelation);
		}
	}

	public static bool UseAdornerForTextboxSelectionRendering
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", ref _useAdornerForTextboxSelectionRendering);
		}
	}

	public static bool AppendLocalAssemblyVersionForSourceUri
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Baml2006.AppendLocalAssemblyVersionForSourceUri", ref _AppendLocalAssemblyVersionForSourceUriSwitchName);
		}
	}

	public static bool IListIndexerHidesCustomIndexer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Data.Binding.IListIndexerHidesCustomIndexer", ref _IListIndexerHidesCustomIndexer);
		}
	}

	public static bool KeyboardNavigationFromHyperlinkInItemsControlIsNotRelativeToFocusedElement
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Controls.KeyboardNavigationFromHyperlinkInItemsControlIsNotRelativeToFocusedElement", ref _KeyboardNavigationFromHyperlinkInItemsControlIsNotRelativeToFocusedElement);
		}
	}

	public static bool ItemAutomationPeerKeepsItsItemAlive
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Automation.Peers.ItemAutomationPeerKeepsItsItemAlive", ref _ItemAutomationPeerKeepsItsItemAlive);
		}
	}
}
