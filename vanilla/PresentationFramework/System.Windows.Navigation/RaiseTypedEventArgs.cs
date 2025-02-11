namespace System.Windows.Navigation;

internal class RaiseTypedEventArgs : EventArgs
{
	internal Delegate D;

	internal object O;

	internal RaiseTypedEventArgs(Delegate d, object o)
	{
		D = d;
		O = o;
	}
}
