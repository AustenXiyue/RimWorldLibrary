using System;
using System.Windows.Input;
using MS.Internal.PresentationCore;

namespace MS.Internal;

[FriendAccessAllowed]
internal static class CommandLibraryHelper
{
	internal static RoutedUICommand CreateUICommand(string name, Type ownerType, byte commandId)
	{
		return new RoutedUICommand(name, ownerType, commandId)
		{
			AreInputGesturesDelayLoaded = true
		};
	}
}
