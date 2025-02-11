using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

internal sealed class MouseButtonUtilities
{
	private MouseButtonUtilities()
	{
	}

	[FriendAccessAllowed]
	internal static void Validate(MouseButton button)
	{
		if ((uint)button > 4u)
		{
			throw new InvalidEnumArgumentException("button", (int)button, typeof(MouseButton));
		}
	}
}
