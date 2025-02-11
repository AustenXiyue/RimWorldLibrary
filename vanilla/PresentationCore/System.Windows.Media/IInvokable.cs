namespace System.Windows.Media;

internal interface IInvokable
{
	void RaiseEvent(byte[] buffer, int cb);
}
