namespace System.Windows.Input;

internal static class RawStylusActionsHelper
{
	private const RawStylusActions MaxActions = RawStylusActions.Activate | RawStylusActions.Deactivate | RawStylusActions.Down | RawStylusActions.Up | RawStylusActions.Move | RawStylusActions.InAirMove | RawStylusActions.InRange | RawStylusActions.OutOfRange | RawStylusActions.SystemGesture;

	internal static bool IsValid(RawStylusActions action)
	{
		if ((action < RawStylusActions.None) || action > (RawStylusActions.Activate | RawStylusActions.Deactivate | RawStylusActions.Down | RawStylusActions.Up | RawStylusActions.Move | RawStylusActions.InAirMove | RawStylusActions.InRange | RawStylusActions.OutOfRange | RawStylusActions.SystemGesture))
		{
			return false;
		}
		return true;
	}
}
