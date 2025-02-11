using MS.Internal.WindowsBase;

namespace MS.Internal.KnownBoxes;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class BooleanBoxes
{
	internal static readonly object TrueBox = true;

	internal static readonly object FalseBox = false;

	internal static object Box(bool value)
	{
		if (!value)
		{
			return FalseBox;
		}
		return TrueBox;
	}

	internal static object Box(bool? value)
	{
		if (value.HasValue)
		{
			if (value == true)
			{
				return TrueBox;
			}
			return FalseBox;
		}
		return null;
	}
}
