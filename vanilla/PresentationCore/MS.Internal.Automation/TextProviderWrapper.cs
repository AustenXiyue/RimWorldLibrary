using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using MS.Internal.PresentationCore;

namespace MS.Internal.Automation;

internal class TextProviderWrapper : MarshalByRefObject, ITextProvider
{
	private AutomationPeer _peer;

	private ITextProvider _iface;

	public ITextRangeProvider DocumentRange => (ITextRangeProvider)ElementUtil.Invoke(_peer, GetDocumentRange, null);

	public SupportedTextSelection SupportedTextSelection => (SupportedTextSelection)ElementUtil.Invoke(_peer, GetSupportedTextSelection, null);

	private TextProviderWrapper(AutomationPeer peer, ITextProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public ITextRangeProvider[] GetSelection()
	{
		return (ITextRangeProvider[])ElementUtil.Invoke(_peer, GetSelection, null);
	}

	public ITextRangeProvider[] GetVisibleRanges()
	{
		return (ITextRangeProvider[])ElementUtil.Invoke(_peer, GetVisibleRanges, null);
	}

	public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement)
	{
		if (!(childElement is ElementProxy))
		{
			throw new ArgumentException(SR.Format(SR.TextProvider_InvalidChild, "childElement"));
		}
		return (ITextRangeProvider)ElementUtil.Invoke(_peer, RangeFromChild, childElement);
	}

	public ITextRangeProvider RangeFromPoint(Point screenLocation)
	{
		return (ITextRangeProvider)ElementUtil.Invoke(_peer, RangeFromPoint, screenLocation);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new TextProviderWrapper(peer, (ITextProvider)iface);
	}

	private object GetSelection(object unused)
	{
		return TextRangeProviderWrapper.WrapArgument(_iface.GetSelection(), _peer);
	}

	private object GetVisibleRanges(object unused)
	{
		return TextRangeProviderWrapper.WrapArgument(_iface.GetVisibleRanges(), _peer);
	}

	private object RangeFromChild(object arg)
	{
		IRawElementProviderSimple childElement = (IRawElementProviderSimple)arg;
		return TextRangeProviderWrapper.WrapArgument(_iface.RangeFromChild(childElement), _peer);
	}

	private object RangeFromPoint(object arg)
	{
		Point screenLocation = (Point)arg;
		return TextRangeProviderWrapper.WrapArgument(_iface.RangeFromPoint(screenLocation), _peer);
	}

	private object GetDocumentRange(object unused)
	{
		return TextRangeProviderWrapper.WrapArgument(_iface.DocumentRange, _peer);
	}

	private object GetSupportedTextSelection(object unused)
	{
		return _iface.SupportedTextSelection;
	}
}
