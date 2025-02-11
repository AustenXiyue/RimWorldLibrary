using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Automation;
using MS.Internal.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="F:System.Windows.Automation.ControlType.Document" /> control types to UI Automation.</summary>
public class DocumentAutomationPeer : ContentTextAutomationPeer
{
	private ITextPointer _childrenStart;

	private ITextPointer _childrenEnd;

	private TextAdaptor _textPattern;

	private ITextContainer _textContainer;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.FrameworkContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" />.</param>
	public DocumentAutomationPeer(FrameworkContentElement owner)
		: base(owner)
	{
		if (owner is IServiceProvider)
		{
			_textContainer = ((IServiceProvider)owner).GetService(typeof(ITextContainer)) as ITextContainer;
			if (_textContainer != null)
			{
				_textPattern = new TextAdaptor(this, _textContainer);
			}
		}
	}

	internal void OnDisconnected()
	{
		if (_textPattern != null)
		{
			_textPattern.Dispose();
			_textPattern = null;
		}
	}

	/// <summary>Gets the collection of child elements for the <see cref="T:System.Windows.FrameworkContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>Because <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" /> gives access to its content through <see cref="T:System.Windows.Automation.TextPattern" />, this method always returns null.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		if (_childrenStart != null && _childrenEnd != null)
		{
			ITextContainer textContainer = ((IServiceProvider)base.Owner).GetService(typeof(ITextContainer)) as ITextContainer;
			return TextContainerHelper.GetAutomationPeersFromRange(_childrenStart, _childrenEnd, textContainer.Start);
		}
		return null;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.FrameworkContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" />. </summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Text" />, this method returns an <see cref="T:System.Windows.Automation.Provider.ITextProvider" />.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object obj = null;
		if (patternInterface == PatternInterface.Text)
		{
			if (_textPattern == null && base.Owner is IServiceProvider && ((IServiceProvider)base.Owner).GetService(typeof(ITextContainer)) is ITextContainer textContainer)
			{
				_textPattern = new TextAdaptor(this, textContainer);
			}
			return _textPattern;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the control type for the control that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Text" /> in all cases.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Document;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.FrameworkContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Document".</returns>
	protected override string GetClassNameCore()
	{
		return "Document";
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.FrameworkContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" /> is understood by the end user as interactive or the user might understand the <see cref="T:System.Windows.FrameworkContentElement" /> as contributing to the logical structure of the control in the GUI. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>true in all cases.</returns>
	protected override bool IsControlElementCore()
	{
		if (base.IncludeInvisibleElementsInControlView)
		{
			return true;
		}
		return ((_textContainer?.TextView)?.RenderScope)?.IsVisible ?? false;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Rect" /> that represents the screen coordinates of the element that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetBoundingRectangle" />.</summary>
	/// <returns>The bounding rectangle.</returns>
	protected override Rect GetBoundingRectangleCore()
	{
		Rect rect = CalculateBoundingRect(clipToVisible: false, out var uiScope);
		if (rect != Rect.Empty && uiScope != null && PresentationSource.CriticalFromVisual(uiScope) is HwndSource hwndSource)
		{
			rect = PointUtil.ElementToRoot(rect, uiScope, hwndSource);
			rect = PointUtil.RootToClient(rect, hwndSource);
			rect = PointUtil.ClientToScreen(rect, hwndSource);
		}
		return rect;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Point" /> that represents the clickable space that is on the <see cref="T:System.Windows.FrameworkContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>The point that represents the clickable space that is on the element.</returns>
	protected override Point GetClickablePointCore()
	{
		Point result = default(Point);
		Rect rect = CalculateBoundingRect(clipToVisible: true, out var uiScope);
		if (rect != Rect.Empty && uiScope != null && PresentationSource.CriticalFromVisual(uiScope) is HwndSource hwndSource)
		{
			rect = PointUtil.ElementToRoot(rect, uiScope, hwndSource);
			rect = PointUtil.RootToClient(rect, hwndSource);
			rect = PointUtil.ClientToScreen(rect, hwndSource);
			result = new Point(rect.Left + rect.Width * 0.1, rect.Top + rect.Height * 0.1);
		}
		return result;
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.FrameworkContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" /> is off the screen. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" />.</summary>
	/// <returns>true if the element is not on the screen; otherwise, false.</returns>
	protected override bool IsOffscreenCore()
	{
		switch (AutomationProperties.GetIsOffscreenBehavior(base.Owner))
		{
		case IsOffscreenBehavior.Onscreen:
			return false;
		case IsOffscreenBehavior.Offscreen:
			return true;
		default:
		{
			if (!DoubleUtil.AreClose(CalculateBoundingRect(clipToVisible: true, out var uiScope), Rect.Empty))
			{
				return uiScope == null;
			}
			return true;
		}
		}
	}

	internal override List<AutomationPeer> GetAutomationPeersFromRange(ITextPointer start, ITextPointer end)
	{
		_childrenStart = start.CreatePointer();
		_childrenEnd = end.CreatePointer();
		ResetChildrenCache();
		return GetChildren();
	}

	private Rect CalculateBoundingRect(bool clipToVisible, out UIElement uiScope)
	{
		uiScope = null;
		Rect rect = Rect.Empty;
		if (base.Owner is IServiceProvider)
		{
			ITextView textView = ((((IServiceProvider)base.Owner).GetService(typeof(ITextContainer)) is ITextContainer textContainer) ? textContainer.TextView : null);
			if (textView != null)
			{
				if (!textView.IsValid)
				{
					if (!textView.Validate())
					{
						textView = null;
					}
					if (textView != null && !textView.IsValid)
					{
						textView = null;
					}
				}
				if (textView != null)
				{
					rect = new Rect(textView.RenderScope.RenderSize);
					uiScope = textView.RenderScope;
					if (clipToVisible)
					{
						Visual visual = textView.RenderScope;
						while (visual != null && rect != Rect.Empty)
						{
							if (VisualTreeHelper.GetClip(visual) != null)
							{
								GeneralTransform inverse = textView.RenderScope.TransformToAncestor(visual).Inverse;
								if (inverse != null)
								{
									Rect bounds = VisualTreeHelper.GetClip(visual).Bounds;
									bounds = inverse.TransformBounds(bounds);
									rect.Intersect(bounds);
								}
								else
								{
									rect = Rect.Empty;
								}
							}
							visual = VisualTreeHelper.GetParent(visual) as Visual;
						}
					}
				}
			}
		}
		return rect;
	}
}
