using System.ComponentModel;

namespace System.Windows.Documents;

internal class BringLineIntoViewCompletedEventArgs : AsyncCompletedEventArgs
{
	private readonly ITextPointer _position;

	private readonly int _count;

	private readonly ITextPointer _newPosition;

	private readonly double _newSuggestedX;

	public ITextPointer Position
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _position;
		}
	}

	public int Count
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _count;
		}
	}

	public ITextPointer NewPosition
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _newPosition;
		}
	}

	public double NewSuggestedX
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _newSuggestedX;
		}
	}

	public BringLineIntoViewCompletedEventArgs(ITextPointer position, double suggestedX, int count, ITextPointer newPosition, double newSuggestedX, int linesMoved, bool succeeded, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		_position = position;
		_count = count;
		_newPosition = newPosition;
		_newSuggestedX = newSuggestedX;
	}
}
