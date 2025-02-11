using System.ComponentModel;

namespace System.Windows.Documents;

internal class BringPageIntoViewCompletedEventArgs : AsyncCompletedEventArgs
{
	private readonly ITextPointer _position;

	private readonly int _count;

	private readonly ITextPointer _newPosition;

	private readonly Point _newSuggestedOffset;

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

	public Point NewSuggestedOffset
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _newSuggestedOffset;
		}
	}

	public BringPageIntoViewCompletedEventArgs(ITextPointer position, Point suggestedOffset, int count, ITextPointer newPosition, Point newSuggestedOffset, int pagesMoved, bool succeeded, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		_position = position;
		_count = count;
		_newPosition = newPosition;
		_newSuggestedOffset = newSuggestedOffset;
	}
}
