using System.Windows.Input;

namespace System.Windows.Documents;

/// <summary>Provides a standard set of editing related commands.</summary>
public static class EditingCommands
{
	private static object _synchronize = new object();

	private static RoutedUICommand _ToggleInsert;

	private static RoutedUICommand _Delete;

	private static RoutedUICommand _Backspace;

	private static RoutedUICommand _DeleteNextWord;

	private static RoutedUICommand _DeletePreviousWord;

	private static RoutedUICommand _EnterParagraphBreak;

	private static RoutedUICommand _EnterLineBreak;

	private static RoutedUICommand _TabForward;

	private static RoutedUICommand _TabBackward;

	private static RoutedUICommand _Space;

	private static RoutedUICommand _ShiftSpace;

	private static RoutedUICommand _MoveRightByCharacter;

	private static RoutedUICommand _MoveLeftByCharacter;

	private static RoutedUICommand _MoveRightByWord;

	private static RoutedUICommand _MoveLeftByWord;

	private static RoutedUICommand _MoveDownByLine;

	private static RoutedUICommand _MoveUpByLine;

	private static RoutedUICommand _MoveDownByParagraph;

	private static RoutedUICommand _MoveUpByParagraph;

	private static RoutedUICommand _MoveDownByPage;

	private static RoutedUICommand _MoveUpByPage;

	private static RoutedUICommand _MoveToLineStart;

	private static RoutedUICommand _MoveToLineEnd;

	private static RoutedUICommand _MoveToColumnStart;

	private static RoutedUICommand _MoveToColumnEnd;

	private static RoutedUICommand _MoveToWindowTop;

	private static RoutedUICommand _MoveToWindowBottom;

	private static RoutedUICommand _MoveToDocumentStart;

	private static RoutedUICommand _MoveToDocumentEnd;

	private static RoutedUICommand _SelectRightByCharacter;

	private static RoutedUICommand _SelectLeftByCharacter;

	private static RoutedUICommand _SelectRightByWord;

	private static RoutedUICommand _SelectLeftByWord;

	private static RoutedUICommand _SelectDownByLine;

	private static RoutedUICommand _SelectUpByLine;

	private static RoutedUICommand _SelectDownByParagraph;

	private static RoutedUICommand _SelectUpByParagraph;

	private static RoutedUICommand _SelectDownByPage;

	private static RoutedUICommand _SelectUpByPage;

	private static RoutedUICommand _SelectToLineStart;

	private static RoutedUICommand _SelectToLineEnd;

	private static RoutedUICommand _SelectToColumnStart;

	private static RoutedUICommand _SelectToColumnEnd;

	private static RoutedUICommand _SelectToWindowTop;

	private static RoutedUICommand _SelectToWindowBottom;

	private static RoutedUICommand _SelectToDocumentStart;

	private static RoutedUICommand _SelectToDocumentEnd;

	private static RoutedUICommand _CopyFormat;

	private static RoutedUICommand _PasteFormat;

	private static RoutedUICommand _ResetFormat;

	private static RoutedUICommand _ToggleBold;

	private static RoutedUICommand _ToggleItalic;

	private static RoutedUICommand _ToggleUnderline;

	private static RoutedUICommand _ToggleSubscript;

	private static RoutedUICommand _ToggleSuperscript;

	private static RoutedUICommand _IncreaseFontSize;

	private static RoutedUICommand _DecreaseFontSize;

	private static RoutedUICommand _ApplyFontSize;

	private static RoutedUICommand _ApplyFontFamily;

	private static RoutedUICommand _ApplyForeground;

	private static RoutedUICommand _ApplyBackground;

	private static RoutedUICommand _ToggleSpellCheck;

	private static RoutedUICommand _ApplyInlineFlowDirectionRTL;

	private static RoutedUICommand _ApplyInlineFlowDirectionLTR;

	private static RoutedUICommand _AlignLeft;

	private static RoutedUICommand _AlignCenter;

	private static RoutedUICommand _AlignRight;

	private static RoutedUICommand _AlignJustify;

	private static RoutedUICommand _ApplySingleSpace;

	private static RoutedUICommand _ApplyOneAndAHalfSpace;

	private static RoutedUICommand _ApplyDoubleSpace;

	private static RoutedUICommand _IncreaseIndentation;

	private static RoutedUICommand _DecreaseIndentation;

	private static RoutedUICommand _ApplyParagraphFlowDirectionRTL;

	private static RoutedUICommand _ApplyParagraphFlowDirectionLTR;

	private static RoutedUICommand _RemoveListMarkers;

	private static RoutedUICommand _ToggleBullets;

	private static RoutedUICommand _ToggleNumbering;

	private static RoutedUICommand _InsertTable;

	private static RoutedUICommand _InsertRows;

	private static RoutedUICommand _InsertColumns;

	private static RoutedUICommand _DeleteRows;

	private static RoutedUICommand _DeleteColumns;

	private static RoutedUICommand _MergeCells;

	private static RoutedUICommand _SplitCell;

	private static RoutedUICommand _CorrectSpellingError;

	private static RoutedUICommand _IgnoreSpellingError;

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.ToggleInsert" /> command, which toggles the typing mode between Insert and Overtype.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Insert.</returns>
	public static RoutedUICommand ToggleInsert => EnsureCommand(ref _ToggleInsert, "ToggleInsert");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.Delete" /> command, which requests that the current selection be deleted.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Delete.</returns>
	public static RoutedUICommand Delete => EnsureCommand(ref _Delete, "Delete");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.Backspace" /> command, which requests that a backspace be entered at the current position or over the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Backspace.</returns>
	public static RoutedUICommand Backspace => EnsureCommand(ref _Backspace, "Backspace");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.DeleteNextWord" /> command, which requests that the next word (relative to a current position) be deleted.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Delete.</returns>
	public static RoutedUICommand DeleteNextWord => EnsureCommand(ref _DeleteNextWord, "DeleteNextWord");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.DeletePreviousWord" /> command, which requests that the previous word (relative to a current position) be deleted.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Backspace.</returns>
	public static RoutedUICommand DeletePreviousWord => EnsureCommand(ref _DeletePreviousWord, "DeletePreviousWord");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.EnterParagraphBreak" /> command, which requests that a paragraph break be inserted at the current position or over the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Enter.</returns>
	public static RoutedUICommand EnterParagraphBreak => EnsureCommand(ref _EnterParagraphBreak, "EnterParagraphBreak");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.EnterLineBreak" /> command, which requests that a line break be inserted at the current position or over the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+Enter.</returns>
	public static RoutedUICommand EnterLineBreak => EnsureCommand(ref _EnterLineBreak, "EnterLineBreak");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.TabForward" /> command.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Tab.</returns>
	public static RoutedUICommand TabForward => EnsureCommand(ref _TabForward, "TabForward");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.TabBackward" /> command.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+Tab.</returns>
	public static RoutedUICommand TabBackward => EnsureCommand(ref _TabBackward, "TabBackward");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveRightByCharacter" /> command, which requests that the caret move one character right.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Right.</returns>
	public static RoutedUICommand MoveRightByCharacter => EnsureCommand(ref _MoveRightByCharacter, "MoveRightByCharacter");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveLeftByCharacter" /> command, which requests that the caret move one character left.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Left.</returns>
	public static RoutedUICommand MoveLeftByCharacter => EnsureCommand(ref _MoveLeftByCharacter, "MoveLeftByCharacter");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveRightByWord" /> command, which requests that the caret move right by one word.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Right.</returns>
	public static RoutedUICommand MoveRightByWord => EnsureCommand(ref _MoveRightByWord, "MoveRightByWord");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveLeftByWord" /> command, which requests that the caret move one word left.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Left.</returns>
	public static RoutedUICommand MoveLeftByWord => EnsureCommand(ref _MoveLeftByWord, "MoveLeftByWord");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveDownByLine" /> command, which requests that the caret move down by one line.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Down.</returns>
	public static RoutedUICommand MoveDownByLine => EnsureCommand(ref _MoveDownByLine, "MoveDownByLine");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveUpByLine" /> command, which requests that the caret move up by one line.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Up.</returns>
	public static RoutedUICommand MoveUpByLine => EnsureCommand(ref _MoveUpByLine, "MoveUpByLine");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveDownByParagraph" /> command, which requests that the caret move down by one paragraph.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Down.</returns>
	public static RoutedUICommand MoveDownByParagraph => EnsureCommand(ref _MoveDownByParagraph, "MoveDownByParagraph");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveUpByParagraph" /> command, which requests that the caret move up by one paragraph.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Up.</returns>
	public static RoutedUICommand MoveUpByParagraph => EnsureCommand(ref _MoveUpByParagraph, "MoveUpByParagraph");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveDownByPage" /> command, which requests that the caret move down by one page.</summary>
	/// <returns>The requested command.  The default key gesture for this command is PageDown.</returns>
	public static RoutedUICommand MoveDownByPage => EnsureCommand(ref _MoveDownByPage, "MoveDownByPage");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveUpByPage" /> command, which requests that the caret move up by one page.</summary>
	/// <returns>The requested command.  The default key gesture for this command is PageUp.</returns>
	public static RoutedUICommand MoveUpByPage => EnsureCommand(ref _MoveUpByPage, "MoveUpByPage");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveToLineStart" /> command, which requests that the caret move to the beginning of the current line.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Home.</returns>
	public static RoutedUICommand MoveToLineStart => EnsureCommand(ref _MoveToLineStart, "MoveToLineStart");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveToLineEnd" /> command, which requests that the caret move to the end of the current line.</summary>
	/// <returns>The requested command.  The default key gesture for this command is End.</returns>
	public static RoutedUICommand MoveToLineEnd => EnsureCommand(ref _MoveToLineEnd, "MoveToLineEnd");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveToDocumentStart" /> command, which requests that the caret move to the very beginning of content.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Home.</returns>
	public static RoutedUICommand MoveToDocumentStart => EnsureCommand(ref _MoveToDocumentStart, "MoveToDocumentStart");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.MoveToDocumentEnd" /> command, which requests that the caret move to the very end of content.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+End.</returns>
	public static RoutedUICommand MoveToDocumentEnd => EnsureCommand(ref _MoveToDocumentEnd, "MoveToDocumentEnd");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectRightByCharacter" /> command, which requests that the current selection be expanded right by one character.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+Right.</returns>
	public static RoutedUICommand SelectRightByCharacter => EnsureCommand(ref _SelectRightByCharacter, "SelectRightByCharacter");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectLeftByCharacter" /> command, which requests that the current selection be expanded left by one character.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+Left.</returns>
	public static RoutedUICommand SelectLeftByCharacter => EnsureCommand(ref _SelectLeftByCharacter, "SelectLeftByCharacter");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectRightByWord" /> command, which requests that the current selection be expanded right by one word.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+Right.</returns>
	public static RoutedUICommand SelectRightByWord => EnsureCommand(ref _SelectRightByWord, "SelectRightByWord");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectLeftByWord" /> command, which requests that the current selection be expanded left by one word.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+Left.</returns>
	public static RoutedUICommand SelectLeftByWord => EnsureCommand(ref _SelectLeftByWord, "SelectLeftByWord");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectDownByLine" /> command, which requests that the current selection be expanded down by one line.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+Down.</returns>
	public static RoutedUICommand SelectDownByLine => EnsureCommand(ref _SelectDownByLine, "SelectDownByLine");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectUpByLine" /> command, which requests that the current selection be expanded up by one line.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+Up.</returns>
	public static RoutedUICommand SelectUpByLine => EnsureCommand(ref _SelectUpByLine, "SelectUpByLine");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectDownByParagraph" /> command, which requests that the current selection be expanded down by one paragraph.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+Down.</returns>
	public static RoutedUICommand SelectDownByParagraph => EnsureCommand(ref _SelectDownByParagraph, "SelectDownByParagraph");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectUpByParagraph" /> command, which requests that the current selection be expanded up by one paragraph.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+Up.</returns>
	public static RoutedUICommand SelectUpByParagraph => EnsureCommand(ref _SelectUpByParagraph, "SelectUpByParagraph");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectDownByPage" /> command, which requests that the current selection be expanded down by one page.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+PageDown.</returns>
	public static RoutedUICommand SelectDownByPage => EnsureCommand(ref _SelectDownByPage, "SelectDownByPage");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectUpByPage" /> command, which requests that the current selection be expanded  up by one page.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+PageUp.</returns>
	public static RoutedUICommand SelectUpByPage => EnsureCommand(ref _SelectUpByPage, "SelectUpByPage");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectToLineStart" /> command, which requests that the current selection be expanded to the beginning of the current line.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+Home.</returns>
	public static RoutedUICommand SelectToLineStart => EnsureCommand(ref _SelectToLineStart, "SelectToLineStart");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectToLineEnd" /> command, which requests that the current selection be expanded to the end of the current line.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Shift+End.</returns>
	public static RoutedUICommand SelectToLineEnd => EnsureCommand(ref _SelectToLineEnd, "SelectToLineEnd");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectToDocumentStart" /> command, which requests that the current selection be expanded to the very beginning of content.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+Home.</returns>
	public static RoutedUICommand SelectToDocumentStart => EnsureCommand(ref _SelectToDocumentStart, "SelectToDocumentStart");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.SelectToDocumentEnd" /> command, which requests that the current selection be expanded to the very end of content.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+End.</returns>
	public static RoutedUICommand SelectToDocumentEnd => EnsureCommand(ref _SelectToDocumentEnd, "SelectToDocumentEnd");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.ToggleBold" /> command, which requests that <see cref="T:System.Windows.Documents.Bold" /> formatting be toggled on the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+B.</returns>
	public static RoutedUICommand ToggleBold => EnsureCommand(ref _ToggleBold, "ToggleBold");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.ToggleItalic" /> command, which requests that <see cref="T:System.Windows.Documents.Italic" /> formatting be toggled on the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+I.</returns>
	public static RoutedUICommand ToggleItalic => EnsureCommand(ref _ToggleItalic, "ToggleItalic");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.ToggleUnderline" /> command, which requests that <see cref="T:System.Windows.Documents.Underline" /> formatting be toggled on the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+U.</returns>
	public static RoutedUICommand ToggleUnderline => EnsureCommand(ref _ToggleUnderline, "ToggleUnderline");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.ToggleSubscript" /> command, which requests that subscript formatting be toggled on the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+OemPlus.</returns>
	public static RoutedUICommand ToggleSubscript => EnsureCommand(ref _ToggleSubscript, "ToggleSubscript");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.ToggleSuperscript" /> command, which requests that superscript formatting be toggled on the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+OemPlus.</returns>
	public static RoutedUICommand ToggleSuperscript => EnsureCommand(ref _ToggleSuperscript, "ToggleSuperscript");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.IncreaseFontSize" /> command, which requests that the font size for the current selection be increased by 1 point.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+OemCloseBrackets.</returns>
	public static RoutedUICommand IncreaseFontSize => EnsureCommand(ref _IncreaseFontSize, "IncreaseFontSize");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.DecreaseFontSize" /> command, which requests that the font size for the current selection be decreased by 1 point.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+OemOpenBrackets.</returns>
	public static RoutedUICommand DecreaseFontSize => EnsureCommand(ref _DecreaseFontSize, "DecreaseFontSize");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.AlignLeft" /> command, which requests that a selection of content be aligned left.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+L.</returns>
	public static RoutedUICommand AlignLeft => EnsureCommand(ref _AlignLeft, "AlignLeft");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.AlignCenter" /> command, which requests that the current paragraph or a selection of paragraphs be centered.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+E.</returns>
	public static RoutedUICommand AlignCenter => EnsureCommand(ref _AlignCenter, "AlignCenter");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.AlignRight" /> command, which requests that a selection of content be aligned right.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+R.</returns>
	public static RoutedUICommand AlignRight => EnsureCommand(ref _AlignRight, "AlignRight");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.AlignJustify" /> command, which requests that the current paragraph or a selection of paragraphs be justified.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+J.</returns>
	public static RoutedUICommand AlignJustify => EnsureCommand(ref _AlignJustify, "AlignJustify");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.ToggleBullets" /> command, which requests that unordered list (also referred to as bulleted list) formatting be toggled on the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+L.</returns>
	public static RoutedUICommand ToggleBullets => EnsureCommand(ref _ToggleBullets, "ToggleBullets");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.ToggleNumbering" /> command, which requests that ordered list (also referred to as numbered list) formatting be toggled on the current selection.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+N.</returns>
	public static RoutedUICommand ToggleNumbering => EnsureCommand(ref _ToggleNumbering, "ToggleNumbering");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.IncreaseIndentation" /> command, which requests that indentation for the current paragraph be increased by one tab stop.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+T.</returns>
	public static RoutedUICommand IncreaseIndentation => EnsureCommand(ref _IncreaseIndentation, "IncreaseIndentation");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.DecreaseIndentation" /> command, which requests that indentation for the current paragraph be decreased by one tab stop.</summary>
	/// <returns>The requested command.  The default key gesture for this command is Ctrl+Shift+T.</returns>
	public static RoutedUICommand DecreaseIndentation => EnsureCommand(ref _DecreaseIndentation, "DecreaseIndentation");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.CorrectSpellingError" /> command, which requests that any misspelled word at the current position be corrected.</summary>
	/// <returns>The requested command.  This command has no default key gesture.</returns>
	public static RoutedUICommand CorrectSpellingError => EnsureCommand(ref _CorrectSpellingError, "CorrectSpellingError");

	/// <summary>Represents the <see cref="P:System.Windows.Documents.EditingCommands.IgnoreSpellingError" /> command, which requests that any instances of misspelled words at the current position or in the current selection be ignored.</summary>
	/// <returns>The requested command.  This command has no default key gesture.</returns>
	public static RoutedUICommand IgnoreSpellingError => EnsureCommand(ref _IgnoreSpellingError, "IgnoreSpellingError");

	internal static RoutedUICommand Space => EnsureCommand(ref _Space, "Space");

	internal static RoutedUICommand ShiftSpace => EnsureCommand(ref _ShiftSpace, "ShiftSpace");

	internal static RoutedUICommand MoveToColumnStart => EnsureCommand(ref _MoveToColumnStart, "MoveToColumnStart");

	internal static RoutedUICommand MoveToColumnEnd => EnsureCommand(ref _MoveToColumnEnd, "MoveToColumnEnd");

	internal static RoutedUICommand MoveToWindowTop => EnsureCommand(ref _MoveToWindowTop, "MoveToWindowTop");

	internal static RoutedUICommand MoveToWindowBottom => EnsureCommand(ref _MoveToWindowBottom, "MoveToWindowBottom");

	internal static RoutedUICommand SelectToColumnStart => EnsureCommand(ref _SelectToColumnStart, "SelectToColumnStart");

	internal static RoutedUICommand SelectToColumnEnd => EnsureCommand(ref _SelectToColumnEnd, "SelectToColumnEnd");

	internal static RoutedUICommand SelectToWindowTop => EnsureCommand(ref _SelectToWindowTop, "SelectToWindowTop");

	internal static RoutedUICommand SelectToWindowBottom => EnsureCommand(ref _SelectToWindowBottom, "SelectToWindowBottom");

	internal static RoutedUICommand ResetFormat => EnsureCommand(ref _ResetFormat, "ResetFormat");

	internal static RoutedUICommand ToggleSpellCheck => EnsureCommand(ref _ToggleSpellCheck, "ToggleSpellCheck");

	internal static RoutedUICommand ApplyFontSize => EnsureCommand(ref _ApplyFontSize, "ApplyFontSize");

	internal static RoutedUICommand ApplyFontFamily => EnsureCommand(ref _ApplyFontFamily, "ApplyFontFamily");

	internal static RoutedUICommand ApplyForeground => EnsureCommand(ref _ApplyForeground, "ApplyForeground");

	internal static RoutedUICommand ApplyBackground => EnsureCommand(ref _ApplyBackground, "ApplyBackground");

	internal static RoutedUICommand ApplyInlineFlowDirectionRTL => EnsureCommand(ref _ApplyInlineFlowDirectionRTL, "ApplyInlineFlowDirectionRTL");

	internal static RoutedUICommand ApplyInlineFlowDirectionLTR => EnsureCommand(ref _ApplyInlineFlowDirectionLTR, "ApplyInlineFlowDirectionLTR");

	internal static RoutedUICommand ApplySingleSpace => EnsureCommand(ref _ApplySingleSpace, "ApplySingleSpace");

	internal static RoutedUICommand ApplyOneAndAHalfSpace => EnsureCommand(ref _ApplyOneAndAHalfSpace, "ApplyOneAndAHalfSpace");

	internal static RoutedUICommand ApplyDoubleSpace => EnsureCommand(ref _ApplyDoubleSpace, "ApplyDoubleSpace");

	internal static RoutedUICommand ApplyParagraphFlowDirectionRTL => EnsureCommand(ref _ApplyParagraphFlowDirectionRTL, "ApplyParagraphFlowDirectionRTL");

	internal static RoutedUICommand ApplyParagraphFlowDirectionLTR => EnsureCommand(ref _ApplyParagraphFlowDirectionLTR, "ApplyParagraphFlowDirectionLTR");

	internal static RoutedUICommand CopyFormat => EnsureCommand(ref _CopyFormat, "CopyFormat");

	internal static RoutedUICommand PasteFormat => EnsureCommand(ref _PasteFormat, "PasteFormat");

	internal static RoutedUICommand RemoveListMarkers => EnsureCommand(ref _RemoveListMarkers, "RemoveListMarkers");

	internal static RoutedUICommand InsertTable => EnsureCommand(ref _InsertTable, "InsertTable");

	internal static RoutedUICommand InsertRows => EnsureCommand(ref _InsertRows, "InsertRows");

	internal static RoutedUICommand InsertColumns => EnsureCommand(ref _InsertColumns, "InsertColumns");

	internal static RoutedUICommand DeleteRows => EnsureCommand(ref _DeleteRows, "DeleteRows");

	internal static RoutedUICommand DeleteColumns => EnsureCommand(ref _DeleteColumns, "DeleteColumns");

	internal static RoutedUICommand MergeCells => EnsureCommand(ref _MergeCells, "MergeCells");

	internal static RoutedUICommand SplitCell => EnsureCommand(ref _SplitCell, "SplitCell");

	private static RoutedUICommand EnsureCommand(ref RoutedUICommand command, string commandPropertyName)
	{
		lock (_synchronize)
		{
			if (command == null)
			{
				command = new RoutedUICommand(commandPropertyName, commandPropertyName, typeof(EditingCommands));
			}
		}
		return command;
	}
}
