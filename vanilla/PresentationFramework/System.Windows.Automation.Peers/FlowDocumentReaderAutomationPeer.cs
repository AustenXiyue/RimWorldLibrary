using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using MS.Internal;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.FlowDocumentReader" /> types to UI Automation.</summary>
public class FlowDocumentReaderAutomationPeer : FrameworkElementAutomationPeer, IMultipleViewProvider
{
	private DocumentAutomationPeer _documentPeer;

	private FlowDocumentReader FlowDocumentReader => (FlowDocumentReader)base.Owner;

	/// <summary>Gets the current control-specific view.</summary>
	/// <returns>The value for the current view of the UI Automation element.</returns>
	int IMultipleViewProvider.CurrentView => ConvertModeToViewId(FlowDocumentReader.ViewingMode);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.FlowDocumentReaderAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.FlowDocumentReader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentReaderAutomationPeer" />.</param>
	public FlowDocumentReaderAutomationPeer(FlowDocumentReader owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentReaderAutomationPeer" />. </summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.MultipleView" />, this method returns a this pointer. Otherwise, this method returns null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object obj = null;
		if (patternInterface == PatternInterface.MultipleView)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentReaderAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = base.GetChildrenCore();
		FlowDocument document = ((FlowDocumentReader)base.Owner).Document;
		if (document != null)
		{
			AutomationPeer automationPeer = ContentElementAutomationPeer.CreatePeerForElement(document);
			if (_documentPeer != automationPeer)
			{
				if (_documentPeer != null)
				{
					_documentPeer.OnDisconnected();
				}
				_documentPeer = automationPeer as DocumentAutomationPeer;
			}
			if (automationPeer != null)
			{
				if (list == null)
				{
					list = new List<AutomationPeer>();
				}
				list.Add(automationPeer);
			}
		}
		return list;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentReaderAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FlowDocumentReaderAutomationPeer" />.</returns>
	protected override string GetClassNameCore()
	{
		return "FlowDocumentReader";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseCurrentViewChangedEvent(FlowDocumentReaderViewingMode newMode, FlowDocumentReaderViewingMode oldMode)
	{
		if (newMode != oldMode)
		{
			RaisePropertyChangedEvent(MultipleViewPatternIdentifiers.CurrentViewProperty, ConvertModeToViewId(newMode), ConvertModeToViewId(oldMode));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseSupportedViewsChangedEvent(DependencyPropertyChangedEventArgs e)
	{
		bool flag;
		bool flag2;
		bool flag4;
		bool flag3;
		bool flag6;
		bool flag5;
		if (e.Property == FlowDocumentReader.IsPageViewEnabledProperty)
		{
			flag = (bool)e.NewValue;
			flag2 = (bool)e.OldValue;
			flag4 = (flag3 = FlowDocumentReader.IsTwoPageViewEnabled);
			flag6 = (flag5 = FlowDocumentReader.IsScrollViewEnabled);
		}
		else if (e.Property == FlowDocumentReader.IsTwoPageViewEnabledProperty)
		{
			flag = (flag2 = FlowDocumentReader.IsPageViewEnabled);
			flag4 = (bool)e.NewValue;
			flag3 = (bool)e.OldValue;
			flag6 = (flag5 = FlowDocumentReader.IsScrollViewEnabled);
		}
		else
		{
			flag = (flag2 = FlowDocumentReader.IsPageViewEnabled);
			flag4 = (flag3 = FlowDocumentReader.IsTwoPageViewEnabled);
			flag6 = (bool)e.NewValue;
			flag5 = (bool)e.OldValue;
		}
		if (flag != flag2 || flag4 != flag3 || flag6 != flag5)
		{
			int[] supportedViews = GetSupportedViews(flag, flag4, flag6);
			int[] supportedViews2 = GetSupportedViews(flag2, flag3, flag5);
			RaisePropertyChangedEvent(MultipleViewPatternIdentifiers.SupportedViewsProperty, supportedViews, supportedViews2);
		}
	}

	private int[] GetSupportedViews(bool single, bool facing, bool scroll)
	{
		int num = 0;
		if (single)
		{
			num++;
		}
		if (facing)
		{
			num++;
		}
		if (scroll)
		{
			num++;
		}
		int[] array = ((num > 0) ? new int[num] : null);
		num = 0;
		if (single)
		{
			array[num++] = ConvertModeToViewId(FlowDocumentReaderViewingMode.Page);
		}
		if (facing)
		{
			array[num++] = ConvertModeToViewId(FlowDocumentReaderViewingMode.TwoPage);
		}
		if (scroll)
		{
			array[num++] = ConvertModeToViewId(FlowDocumentReaderViewingMode.Scroll);
		}
		return array;
	}

	private int ConvertModeToViewId(FlowDocumentReaderViewingMode mode)
	{
		return (int)mode;
	}

	private FlowDocumentReaderViewingMode ConvertViewIdToMode(int viewId)
	{
		Invariant.Assert(viewId >= 0 && viewId <= 2);
		return (FlowDocumentReaderViewingMode)viewId;
	}

	/// <summary>Returns the name of a control-specific view.</summary>
	/// <returns>The name of a control-specific view.</returns>
	/// <param name="viewId">The ID of a view. </param>
	string IMultipleViewProvider.GetViewName(int viewId)
	{
		string result = string.Empty;
		if (viewId >= 0 && viewId <= 2)
		{
			switch (ConvertViewIdToMode(viewId))
			{
			case FlowDocumentReaderViewingMode.Page:
				result = SR.FlowDocumentReader_MultipleViewProvider_PageViewName;
				break;
			case FlowDocumentReaderViewingMode.TwoPage:
				result = SR.FlowDocumentReader_MultipleViewProvider_TwoPageViewName;
				break;
			case FlowDocumentReaderViewingMode.Scroll:
				result = SR.FlowDocumentReader_MultipleViewProvider_ScrollViewName;
				break;
			}
		}
		return result;
	}

	/// <summary>Sets the current control-specific view. </summary>
	/// <param name="viewId">The ID of a view. </param>
	void IMultipleViewProvider.SetCurrentView(int viewId)
	{
		if (viewId >= 0 && viewId <= 2)
		{
			FlowDocumentReader.ViewingMode = ConvertViewIdToMode(viewId);
		}
	}

	/// <summary>Returns a collection of control-specific view identifiers.</summary>
	/// <returns>A collection of values that identifies the views available for a UI Automation element.</returns>
	int[] IMultipleViewProvider.GetSupportedViews()
	{
		return GetSupportedViews(FlowDocumentReader.IsPageViewEnabled, FlowDocumentReader.IsTwoPageViewEnabled, FlowDocumentReader.IsScrollViewEnabled);
	}
}
