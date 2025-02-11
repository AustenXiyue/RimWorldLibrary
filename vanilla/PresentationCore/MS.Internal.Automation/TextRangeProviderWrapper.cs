using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using MS.Internal.PresentationCore;

namespace MS.Internal.Automation;

internal class TextRangeProviderWrapper : MarshalByRefObject, ITextRangeProvider
{
	private AutomationPeer _peer;

	private ITextRangeProvider _iface;

	internal TextRangeProviderWrapper(AutomationPeer peer, ITextRangeProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public ITextRangeProvider Clone()
	{
		return (ITextRangeProvider)ElementUtil.Invoke(_peer, Clone, null);
	}

	public bool Compare(ITextRangeProvider range)
	{
		if (!(range is TextRangeProviderWrapper))
		{
			throw new ArgumentException(SR.Format(SR.TextRangeProvider_InvalidRangeProvider, "range"));
		}
		return (bool)ElementUtil.Invoke(_peer, Compare, range);
	}

	public int CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
	{
		if (!(targetRange is TextRangeProviderWrapper))
		{
			throw new ArgumentException(SR.Format(SR.TextRangeProvider_InvalidRangeProvider, "targetRange"));
		}
		object[] arg = new object[3] { endpoint, targetRange, targetEndpoint };
		return (int)ElementUtil.Invoke(_peer, CompareEndpoints, arg);
	}

	public void ExpandToEnclosingUnit(TextUnit unit)
	{
		object[] arg = new object[1] { unit };
		ElementUtil.Invoke(_peer, ExpandToEnclosingUnit, arg);
	}

	public ITextRangeProvider FindAttribute(int attribute, object val, bool backward)
	{
		object[] arg = new object[3] { attribute, val, backward };
		return (ITextRangeProvider)ElementUtil.Invoke(_peer, FindAttribute, arg);
	}

	public ITextRangeProvider FindText(string text, bool backward, bool ignoreCase)
	{
		object[] arg = new object[3] { text, backward, ignoreCase };
		return (ITextRangeProvider)ElementUtil.Invoke(_peer, FindText, arg);
	}

	public object GetAttributeValue(int attribute)
	{
		object[] arg = new object[1] { attribute };
		return ElementUtil.Invoke(_peer, GetAttributeValue, arg);
	}

	public double[] GetBoundingRectangles()
	{
		return (double[])ElementUtil.Invoke(_peer, GetBoundingRectangles, null);
	}

	public IRawElementProviderSimple GetEnclosingElement()
	{
		return (IRawElementProviderSimple)ElementUtil.Invoke(_peer, GetEnclosingElement, null);
	}

	public string GetText(int maxLength)
	{
		object[] arg = new object[1] { maxLength };
		return (string)ElementUtil.Invoke(_peer, GetText, arg);
	}

	public int Move(TextUnit unit, int count)
	{
		object[] arg = new object[2] { unit, count };
		return (int)ElementUtil.Invoke(_peer, Move, arg);
	}

	public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
	{
		object[] arg = new object[3] { endpoint, unit, count };
		return (int)ElementUtil.Invoke(_peer, MoveEndpointByUnit, arg);
	}

	public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
	{
		if (!(targetRange is TextRangeProviderWrapper))
		{
			throw new ArgumentException(SR.Format(SR.TextRangeProvider_InvalidRangeProvider, "targetRange"));
		}
		object[] arg = new object[3] { endpoint, targetRange, targetEndpoint };
		ElementUtil.Invoke(_peer, MoveEndpointByRange, arg);
	}

	public void Select()
	{
		ElementUtil.Invoke(_peer, Select, null);
	}

	public void AddToSelection()
	{
		ElementUtil.Invoke(_peer, AddToSelection, null);
	}

	public void RemoveFromSelection()
	{
		ElementUtil.Invoke(_peer, RemoveFromSelection, null);
	}

	public void ScrollIntoView(bool alignToTop)
	{
		ElementUtil.Invoke(_peer, ScrollIntoView, alignToTop);
	}

	public IRawElementProviderSimple[] GetChildren()
	{
		return (IRawElementProviderSimple[])ElementUtil.Invoke(_peer, GetChildren, null);
	}

	internal static ITextRangeProvider WrapArgument(ITextRangeProvider argument, AutomationPeer peer)
	{
		if (argument == null)
		{
			return null;
		}
		if (argument is TextRangeProviderWrapper)
		{
			return argument;
		}
		return new TextRangeProviderWrapper(peer, argument);
	}

	internal static ITextRangeProvider[] WrapArgument(ITextRangeProvider[] argument, AutomationPeer peer)
	{
		if (argument == null)
		{
			return null;
		}
		if (argument is TextRangeProviderWrapper[])
		{
			return argument;
		}
		ITextRangeProvider[] array = new ITextRangeProvider[argument.Length];
		for (int i = 0; i < argument.Length; i++)
		{
			array[i] = WrapArgument(argument[i], peer);
		}
		return array;
	}

	internal static ITextRangeProvider UnwrapArgument(ITextRangeProvider argument)
	{
		if (argument is TextRangeProviderWrapper)
		{
			return ((TextRangeProviderWrapper)argument)._iface;
		}
		return argument;
	}

	private object Clone(object unused)
	{
		return WrapArgument(_iface.Clone(), _peer);
	}

	private object Compare(object arg)
	{
		ITextRangeProvider argument = (ITextRangeProvider)arg;
		return _iface.Compare(UnwrapArgument(argument));
	}

	private object CompareEndpoints(object arg)
	{
		object[] obj = (object[])arg;
		TextPatternRangeEndpoint endpoint = (TextPatternRangeEndpoint)obj[0];
		ITextRangeProvider argument = (ITextRangeProvider)obj[1];
		TextPatternRangeEndpoint targetEndpoint = (TextPatternRangeEndpoint)obj[2];
		return _iface.CompareEndpoints(endpoint, UnwrapArgument(argument), targetEndpoint);
	}

	private object ExpandToEnclosingUnit(object arg)
	{
		TextUnit unit = (TextUnit)((object[])arg)[0];
		_iface.ExpandToEnclosingUnit(unit);
		return null;
	}

	private object FindAttribute(object arg)
	{
		object[] obj = (object[])arg;
		int attribute = (int)obj[0];
		object value = obj[1];
		bool backward = (bool)obj[2];
		return WrapArgument(_iface.FindAttribute(attribute, value, backward), _peer);
	}

	private object FindText(object arg)
	{
		object[] obj = (object[])arg;
		string text = (string)obj[0];
		bool backward = (bool)obj[1];
		bool ignoreCase = (bool)obj[2];
		return WrapArgument(_iface.FindText(text, backward, ignoreCase), _peer);
	}

	private object GetAttributeValue(object arg)
	{
		int attribute = (int)((object[])arg)[0];
		return _iface.GetAttributeValue(attribute);
	}

	private object GetBoundingRectangles(object unused)
	{
		return _iface.GetBoundingRectangles();
	}

	private object GetEnclosingElement(object unused)
	{
		return _iface.GetEnclosingElement();
	}

	private object GetText(object arg)
	{
		int maxLength = (int)((object[])arg)[0];
		return _iface.GetText(maxLength);
	}

	private object Move(object arg)
	{
		object[] obj = (object[])arg;
		TextUnit unit = (TextUnit)obj[0];
		int count = (int)obj[1];
		return _iface.Move(unit, count);
	}

	private object MoveEndpointByUnit(object arg)
	{
		object[] obj = (object[])arg;
		TextPatternRangeEndpoint endpoint = (TextPatternRangeEndpoint)obj[0];
		TextUnit unit = (TextUnit)obj[1];
		int count = (int)obj[2];
		return _iface.MoveEndpointByUnit(endpoint, unit, count);
	}

	private object MoveEndpointByRange(object arg)
	{
		object[] obj = (object[])arg;
		TextPatternRangeEndpoint endpoint = (TextPatternRangeEndpoint)obj[0];
		ITextRangeProvider argument = (ITextRangeProvider)obj[1];
		TextPatternRangeEndpoint targetEndpoint = (TextPatternRangeEndpoint)obj[2];
		_iface.MoveEndpointByRange(endpoint, UnwrapArgument(argument), targetEndpoint);
		return null;
	}

	private object Select(object unused)
	{
		_iface.Select();
		return null;
	}

	private object AddToSelection(object unused)
	{
		_iface.AddToSelection();
		return null;
	}

	private object RemoveFromSelection(object unused)
	{
		_iface.RemoveFromSelection();
		return null;
	}

	private object ScrollIntoView(object arg)
	{
		bool alignToTop = (bool)arg;
		_iface.ScrollIntoView(alignToTop);
		return null;
	}

	private object GetChildren(object unused)
	{
		return _iface.GetChildren();
	}
}
