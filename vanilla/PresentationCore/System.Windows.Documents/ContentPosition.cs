namespace System.Windows.Documents;

/// <summary>Abstract class that represents the position of content. This position is content specific.  </summary>
public abstract class ContentPosition
{
	private class MissingContentPosition : ContentPosition
	{
	}

	/// <summary>Static representation of a non-existent ContentPosition. </summary>
	public static readonly ContentPosition Missing = new MissingContentPosition();

	/// <summary>Call this constructor as the initializer of a derived class' constructor, in order to properly initialize a class derived from <see cref="T:System.Windows.Documents.ContentPosition" />. </summary>
	protected ContentPosition()
	{
	}
}
