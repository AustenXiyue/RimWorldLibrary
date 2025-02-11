namespace System.Windows.Input;

internal interface IMouseInputProvider : IInputProvider
{
	bool SetCursor(Cursor cursor);

	bool CaptureMouse();

	void ReleaseMouseCapture();

	int GetIntermediatePoints(IInputElement relativeTo, Point[] points);
}
