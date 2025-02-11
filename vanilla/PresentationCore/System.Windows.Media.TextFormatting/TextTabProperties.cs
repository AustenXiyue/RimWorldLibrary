namespace System.Windows.Media.TextFormatting;

/// <summary>Specifies properties of user-defined tabs.</summary>
public class TextTabProperties
{
	private TextTabAlignment _alignment;

	private double _location;

	private int _tabLeader;

	private int _aligningChar;

	/// <summary>Gets the alignment style of the text at the tab location.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.Media.TextFormatting.TextTabAlignment" /> that represents the alignment of the text at the tab location.</returns>
	public TextTabAlignment Alignment => _alignment;

	/// <summary>Gets the index value of the tab location.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the tab location.</returns>
	public double Location => _location;

	/// <summary>Gets the index of the character that is used to display the tab leader. </summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the tab leader location.</returns>
	public int TabLeader => _tabLeader;

	/// <summary>Gets the index of the specific character in the text that is aligned at the specified tab location.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value representing the index.</returns>
	public int AligningCharacter => _aligningChar;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextTabProperties" /> class by specifying tab properties.</summary>
	/// <param name="alignment">An enumerated value of <see cref="T:System.Windows.Media.TextFormatting.TextTabAlignment" /> that represents the alignment of text at the tab location.</param>
	/// <param name="location">A <see cref="T:System.Double" /> value that represents the tab location.</param>
	/// <param name="tabLeader">An <see cref="T:System.Int32" /> value that represents the tab leader.</param>
	/// <param name="aligningChar">An <see cref="T:System.Int32" /> value that represents the specific character in text that is aligned at tab location.</param>
	public TextTabProperties(TextTabAlignment alignment, double location, int tabLeader, int aligningChar)
	{
		_alignment = alignment;
		_location = location;
		_tabLeader = tabLeader;
		_aligningChar = aligningChar;
	}
}
