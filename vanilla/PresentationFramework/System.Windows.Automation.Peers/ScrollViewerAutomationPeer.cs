using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using MS.Internal;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ScrollViewer" /> types to UI Automation.</summary>
public class ScrollViewerAutomationPeer : FrameworkElementAutomationPeer, IScrollProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The horizontal scroll position as a percentage of the total content area within the control.</returns>
	double IScrollProvider.HorizontalScrollPercent
	{
		get
		{
			if (!HorizontallyScrollable)
			{
				return -1.0;
			}
			ScrollViewer scrollViewer = (ScrollViewer)base.Owner;
			return scrollViewer.HorizontalOffset * 100.0 / (scrollViewer.ExtentWidth - scrollViewer.ViewportWidth);
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The vertical scroll position as a percentage of the total content area within the control.</returns>
	double IScrollProvider.VerticalScrollPercent
	{
		get
		{
			if (!VerticallyScrollable)
			{
				return -1.0;
			}
			ScrollViewer scrollViewer = (ScrollViewer)base.Owner;
			return scrollViewer.VerticalOffset * 100.0 / (scrollViewer.ExtentHeight - scrollViewer.ViewportHeight);
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
	double IScrollProvider.HorizontalViewSize
	{
		get
		{
			ScrollViewer scrollViewer = (ScrollViewer)base.Owner;
			if (scrollViewer.ScrollInfo == null || DoubleUtil.IsZero(scrollViewer.ExtentWidth))
			{
				return 100.0;
			}
			return Math.Min(100.0, scrollViewer.ViewportWidth * 100.0 / scrollViewer.ExtentWidth);
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
	double IScrollProvider.VerticalViewSize
	{
		get
		{
			ScrollViewer scrollViewer = (ScrollViewer)base.Owner;
			if (scrollViewer.ScrollInfo == null || DoubleUtil.IsZero(scrollViewer.ExtentHeight))
			{
				return 100.0;
			}
			return Math.Min(100.0, scrollViewer.ViewportHeight * 100.0 / scrollViewer.ExtentHeight);
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Value indicating whether control is can be scrolled in the horizontal direction.</returns>
	bool IScrollProvider.HorizontallyScrollable => HorizontallyScrollable;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the control can scroll vertically; otherwise false.</returns>
	bool IScrollProvider.VerticallyScrollable => VerticallyScrollable;

	private bool HorizontallyScrollable
	{
		get
		{
			ScrollViewer scrollViewer = (ScrollViewer)base.Owner;
			if (scrollViewer.ScrollInfo != null)
			{
				return DoubleUtil.GreaterThan(scrollViewer.ExtentWidth, scrollViewer.ViewportWidth);
			}
			return false;
		}
	}

	private bool VerticallyScrollable
	{
		get
		{
			ScrollViewer scrollViewer = (ScrollViewer)base.Owner;
			if (scrollViewer.ScrollInfo != null)
			{
				return DoubleUtil.GreaterThan(scrollViewer.ExtentHeight, scrollViewer.ViewportHeight);
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" />.</param>
	public ScrollViewerAutomationPeer(ScrollViewer owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ScrollViewer".</returns>
	protected override string GetClassNameCore()
	{
		return "ScrollViewer";
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Pane" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Pane;
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.ScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" /> is understood by the end user as interactive or as contributing to the logical structure of the control in the GUI. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>If the <see cref="P:System.Windows.FrameworkElement.TemplatedParent" /> is null, this method returns true; otherwise, false.</returns>
	protected override bool IsControlElementCore()
	{
		DependencyObject templatedParent = ((ScrollViewer)base.Owner).TemplatedParent;
		if (templatedParent == null || templatedParent is ContentPresenter)
		{
			return base.IsControlElementCore();
		}
		return false;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.ScrollViewer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Scroll" />, this method returns a this pointer; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Scroll)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="horizontalAmount"> The horizontal increment specific to the control.</param>
	/// <param name="verticalAmount"> The vertical increment specific to the control.</param>
	void IScrollProvider.Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		bool num = horizontalAmount != ScrollAmount.NoAmount;
		bool flag = verticalAmount != ScrollAmount.NoAmount;
		ScrollViewer scrollViewer = (ScrollViewer)base.Owner;
		if ((num && !HorizontallyScrollable) || (flag && !VerticallyScrollable))
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		switch (horizontalAmount)
		{
		case ScrollAmount.LargeDecrement:
			scrollViewer.PageLeft();
			break;
		case ScrollAmount.SmallDecrement:
			scrollViewer.LineLeft();
			break;
		case ScrollAmount.SmallIncrement:
			scrollViewer.LineRight();
			break;
		case ScrollAmount.LargeIncrement:
			scrollViewer.PageRight();
			break;
		default:
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		case ScrollAmount.NoAmount:
			break;
		}
		switch (verticalAmount)
		{
		case ScrollAmount.LargeDecrement:
			scrollViewer.PageUp();
			break;
		case ScrollAmount.SmallDecrement:
			scrollViewer.LineUp();
			break;
		case ScrollAmount.SmallIncrement:
			scrollViewer.LineDown();
			break;
		case ScrollAmount.LargeIncrement:
			scrollViewer.PageDown();
			break;
		default:
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		case ScrollAmount.NoAmount:
			break;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="horizontalPercent"> Percent scrolled horizontally.</param>
	/// <param name="verticalPercent"> Percent scrolled vertically.</param>
	void IScrollProvider.SetScrollPercent(double horizontalPercent, double verticalPercent)
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		bool flag = horizontalPercent != -1.0;
		bool flag2 = verticalPercent != -1.0;
		ScrollViewer scrollViewer = (ScrollViewer)base.Owner;
		if ((flag && !HorizontallyScrollable) || (flag2 && !VerticallyScrollable))
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		if ((flag && horizontalPercent < 0.0) || horizontalPercent > 100.0)
		{
			throw new ArgumentOutOfRangeException("horizontalPercent", SR.Format(SR.ScrollViewer_OutOfRange, "horizontalPercent", horizontalPercent.ToString(CultureInfo.InvariantCulture), "0", "100"));
		}
		if ((flag2 && verticalPercent < 0.0) || verticalPercent > 100.0)
		{
			throw new ArgumentOutOfRangeException("verticalPercent", SR.Format(SR.ScrollViewer_OutOfRange, "verticalPercent", verticalPercent.ToString(CultureInfo.InvariantCulture), "0", "100"));
		}
		if (flag)
		{
			scrollViewer.ScrollToHorizontalOffset((scrollViewer.ExtentWidth - scrollViewer.ViewportWidth) * horizontalPercent * 0.01);
		}
		if (flag2)
		{
			scrollViewer.ScrollToVerticalOffset((scrollViewer.ExtentHeight - scrollViewer.ViewportHeight) * verticalPercent * 0.01);
		}
	}

	private static bool AutomationIsScrollable(double extent, double viewport)
	{
		return DoubleUtil.GreaterThan(extent, viewport);
	}

	private static double AutomationGetScrollPercent(double extent, double viewport, double actualOffset)
	{
		if (!AutomationIsScrollable(extent, viewport))
		{
			return -1.0;
		}
		return actualOffset * 100.0 / (extent - viewport);
	}

	private static double AutomationGetViewSize(double extent, double viewport)
	{
		if (DoubleUtil.IsZero(extent))
		{
			return 100.0;
		}
		return Math.Min(100.0, viewport * 100.0 / extent);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseAutomationEvents(double extentX, double extentY, double viewportX, double viewportY, double offsetX, double offsetY)
	{
		if (AutomationIsScrollable(extentX, viewportX) != ((IScrollProvider)this).HorizontallyScrollable)
		{
			RaisePropertyChangedEvent(ScrollPatternIdentifiers.HorizontallyScrollableProperty, AutomationIsScrollable(extentX, viewportX), ((IScrollProvider)this).HorizontallyScrollable);
		}
		if (AutomationIsScrollable(extentY, viewportY) != ((IScrollProvider)this).VerticallyScrollable)
		{
			RaisePropertyChangedEvent(ScrollPatternIdentifiers.VerticallyScrollableProperty, AutomationIsScrollable(extentY, viewportY), ((IScrollProvider)this).VerticallyScrollable);
		}
		if (AutomationGetViewSize(extentX, viewportX) != ((IScrollProvider)this).HorizontalViewSize)
		{
			RaisePropertyChangedEvent(ScrollPatternIdentifiers.HorizontalViewSizeProperty, AutomationGetViewSize(extentX, viewportX), ((IScrollProvider)this).HorizontalViewSize);
		}
		if (AutomationGetViewSize(extentY, viewportY) != ((IScrollProvider)this).VerticalViewSize)
		{
			RaisePropertyChangedEvent(ScrollPatternIdentifiers.VerticalViewSizeProperty, AutomationGetViewSize(extentY, viewportY), ((IScrollProvider)this).VerticalViewSize);
		}
		if (AutomationGetScrollPercent(extentX, viewportX, offsetX) != ((IScrollProvider)this).HorizontalScrollPercent)
		{
			RaisePropertyChangedEvent(ScrollPatternIdentifiers.HorizontalScrollPercentProperty, AutomationGetScrollPercent(extentX, viewportX, offsetX), ((IScrollProvider)this).HorizontalScrollPercent);
		}
		if (AutomationGetScrollPercent(extentY, viewportY, offsetY) != ((IScrollProvider)this).VerticalScrollPercent)
		{
			RaisePropertyChangedEvent(ScrollPatternIdentifiers.VerticalScrollPercentProperty, AutomationGetScrollPercent(extentY, viewportY, offsetY), ((IScrollProvider)this).VerticalScrollPercent);
		}
	}
}
