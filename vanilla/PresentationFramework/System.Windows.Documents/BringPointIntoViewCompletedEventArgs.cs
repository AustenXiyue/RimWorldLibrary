using System.ComponentModel;

namespace System.Windows.Documents;

internal class BringPointIntoViewCompletedEventArgs : AsyncCompletedEventArgs
{
	private readonly Point _point;

	private readonly ITextPointer _position;

	public Point Point
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _point;
		}
	}

	public ITextPointer Position
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _position;
		}
	}

	public BringPointIntoViewCompletedEventArgs(Point point, ITextPointer position, bool succeeded, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		_point = point;
		_position = position;
	}
}
