using System.Collections.Generic;

namespace System.Windows.Media.TextFormatting;

/// <summary>Represents the bounding rectangle of a range of characters. </summary>
public sealed class TextBounds
{
	private FlowDirection _flowDirection;

	private Rect _bounds;

	private IList<TextRunBounds> _runBounds;

	/// <summary>Gets the bounding rectangle for the <see cref="T:System.Windows.Media.TextFormatting.TextBounds" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> value that represents the bounding rectangle of a range of characters.</returns>
	public Rect Rectangle => _bounds;

	/// <summary>Gets a list of <see cref="T:System.Windows.Media.TextFormatting.TextRunBounds" /> objects.</summary>
	/// <returns>A list of <see cref="T:System.Windows.Media.TextFormatting.TextRunBounds" /> objects.</returns>
	public IList<TextRunBounds> TextRunBounds => _runBounds;

	/// <summary>Gets the text flow direction for the <see cref="T:System.Windows.Media.TextFormatting.TextBounds" /> object.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.FlowDirection" />.</returns>
	public FlowDirection FlowDirection => _flowDirection;

	internal TextBounds(Rect bounds, FlowDirection flowDirection, IList<TextRunBounds> runBounds)
	{
		_bounds = bounds;
		_flowDirection = flowDirection;
		_runBounds = runBounds;
	}
}
