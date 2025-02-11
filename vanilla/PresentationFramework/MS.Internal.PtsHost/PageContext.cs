using System.Collections.Generic;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal class PageContext
{
	private List<BaseParaClient> _floatingElementList;

	private PTS.FSRECT _pageRect;

	internal PTS.FSRECT PageRect
	{
		get
		{
			return _pageRect;
		}
		set
		{
			_pageRect = value;
		}
	}

	internal List<BaseParaClient> FloatingElementList => _floatingElementList;

	internal void AddFloatingParaClient(BaseParaClient floatingElement)
	{
		if (_floatingElementList == null)
		{
			_floatingElementList = new List<BaseParaClient>();
		}
		if (!_floatingElementList.Contains(floatingElement))
		{
			_floatingElementList.Add(floatingElement);
		}
	}

	internal void RemoveFloatingParaClient(BaseParaClient floatingElement)
	{
		if (_floatingElementList.Contains(floatingElement))
		{
			_floatingElementList.Remove(floatingElement);
		}
		if (_floatingElementList.Count == 0)
		{
			_floatingElementList = null;
		}
	}
}
