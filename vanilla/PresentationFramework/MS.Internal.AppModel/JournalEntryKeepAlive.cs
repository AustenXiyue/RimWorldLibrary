using System;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

internal class JournalEntryKeepAlive : JournalEntry
{
	private object _keepAliveRoot;

	internal object KeepAliveRoot => _keepAliveRoot;

	internal JournalEntryKeepAlive(JournalEntryGroupState jeGroupState, Uri uri, object keepAliveRoot)
		: base(jeGroupState, uri)
	{
		Invariant.Assert(keepAliveRoot != null);
		_keepAliveRoot = keepAliveRoot;
	}

	internal override bool IsAlive()
	{
		return KeepAliveRoot != null;
	}

	internal override void SaveState(object contentObject)
	{
		_keepAliveRoot = contentObject;
	}

	internal override bool Navigate(INavigator navigator, NavigationMode navMode)
	{
		return navigator.Navigate(KeepAliveRoot, new NavigateInfo(base.Source, navMode, this));
	}
}
