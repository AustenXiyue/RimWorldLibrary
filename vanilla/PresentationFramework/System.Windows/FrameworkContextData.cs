using System.Collections.Generic;
using System.Windows.Threading;

namespace System.Windows;

internal class FrameworkContextData
{
	private struct WalkerEntry
	{
		public object Data;

		public DescendentsWalkerBase Walker;
	}

	private List<WalkerEntry> _currentWalkers = new List<WalkerEntry>(4);

	public static FrameworkContextData From(Dispatcher context)
	{
		FrameworkContextData frameworkContextData = (FrameworkContextData)context.Reserved2;
		if (frameworkContextData == null)
		{
			frameworkContextData = (FrameworkContextData)(context.Reserved2 = new FrameworkContextData());
		}
		return frameworkContextData;
	}

	private FrameworkContextData()
	{
	}

	public void AddWalker(object data, DescendentsWalkerBase walker)
	{
		WalkerEntry item = default(WalkerEntry);
		item.Data = data;
		item.Walker = walker;
		_currentWalkers.Add(item);
	}

	public void RemoveWalker(object data, DescendentsWalkerBase walker)
	{
		int index = _currentWalkers.Count - 1;
		_currentWalkers.RemoveAt(index);
	}

	public bool WasNodeVisited(DependencyObject d, object data)
	{
		if (_currentWalkers.Count > 0)
		{
			int index = _currentWalkers.Count - 1;
			WalkerEntry walkerEntry = _currentWalkers[index];
			if (walkerEntry.Data == data)
			{
				return walkerEntry.Walker.WasVisited(d);
			}
		}
		return false;
	}
}
