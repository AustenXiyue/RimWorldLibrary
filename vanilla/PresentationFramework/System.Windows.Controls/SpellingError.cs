using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a misspelled word in an editing control (i.e. <see cref="T:System.Windows.Controls.TextBox" /> or <see cref="T:System.Windows.Controls.RichTextBox" />).</summary>
public class SpellingError
{
	private readonly Speller _speller;

	private readonly ITextPointer _start;

	private readonly ITextPointer _end;

	/// <summary>Gets a list of suggested spelling replacements for the misspelled word.</summary>
	/// <returns>The collection of spelling suggestions for the misspelled word.</returns>
	public IEnumerable<string> Suggestions
	{
		get
		{
			IList suggestions = _speller.GetSuggestionsForError(this);
			for (int i = 0; i < suggestions.Count; i++)
			{
				yield return (string)suggestions[i];
			}
		}
	}

	internal ITextPointer Start => _start;

	internal ITextPointer End => _end;

	internal SpellingError(Speller speller, ITextPointer start, ITextPointer end)
	{
		Invariant.Assert(start.CompareTo(end) < 0);
		_speller = speller;
		_start = start.GetFrozenPointer(LogicalDirection.Forward);
		_end = end.GetFrozenPointer(LogicalDirection.Backward);
	}

	/// <summary>Replaces the spelling error text with the specified correction.</summary>
	/// <param name="correctedText">The text used to replace the misspelled text.</param>
	public void Correct(string correctedText)
	{
		if (correctedText == null)
		{
			correctedText = string.Empty;
		}
		((ITextRange)new TextRange(_start, _end)).Text = correctedText;
	}

	/// <summary>Instructs the control to ignore this error and any duplicates for the remainder of the lifetime of the control.</summary>
	public void IgnoreAll()
	{
		_speller.IgnoreAll(TextRangeBase.GetTextInternal(_start, _end));
	}
}
