using System.Windows.Input;
using MS.Internal.Commands;

namespace System.Windows.Documents;

internal static class TextEditorParagraphs
{
	private const string KeyAlignCenter = "Ctrl+E";

	private const string KeyAlignJustify = "Ctrl+J";

	private const string KeyAlignLeft = "Ctrl+L";

	private const string KeyAlignRight = "Ctrl+R";

	private const string KeyApplyDoubleSpace = "Ctrl+2";

	private const string KeyApplyOneAndAHalfSpace = "Ctrl+5";

	private const string KeyApplySingleSpace = "Ctrl+1";

	internal static void _RegisterClassHandlers(Type controlType, bool acceptsRichContent, bool registerEventListeners)
	{
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = OnQueryStatusNYI;
		if (acceptsRichContent)
		{
			CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.AlignLeft, OnAlignLeft, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+L", "KeyAlignLeftDisplayString"));
			CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.AlignCenter, OnAlignCenter, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+E", "KeyAlignCenterDisplayString"));
			CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.AlignRight, OnAlignRight, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+R", "KeyAlignRightDisplayString"));
			CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.AlignJustify, OnAlignJustify, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+J", "KeyAlignJustifyDisplayString"));
			CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplySingleSpace, OnApplySingleSpace, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+1", "KeyApplySingleSpaceDisplayString"));
			CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyOneAndAHalfSpace, OnApplyOneAndAHalfSpace, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+5", "KeyApplyOneAndAHalfSpaceDisplayString"));
			CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyDoubleSpace, OnApplyDoubleSpace, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+2", "KeyApplyDoubleSpaceDisplayString"));
		}
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyParagraphFlowDirectionLTR, OnApplyParagraphFlowDirectionLTR, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyParagraphFlowDirectionRTL, OnApplyParagraphFlowDirectionRTL, canExecuteRoutedEventHandler);
	}

	private static void OnAlignLeft(object sender, ExecutedRoutedEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null)
		{
			TextEditorCharacters._OnApplyProperty(textEditor, Block.TextAlignmentProperty, TextAlignment.Left, applyToParagraphs: true);
		}
	}

	private static void OnAlignCenter(object sender, ExecutedRoutedEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null)
		{
			TextEditorCharacters._OnApplyProperty(textEditor, Block.TextAlignmentProperty, TextAlignment.Center, applyToParagraphs: true);
		}
	}

	private static void OnAlignRight(object sender, ExecutedRoutedEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null)
		{
			TextEditorCharacters._OnApplyProperty(textEditor, Block.TextAlignmentProperty, TextAlignment.Right, applyToParagraphs: true);
		}
	}

	private static void OnAlignJustify(object sender, ExecutedRoutedEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null)
		{
			TextEditorCharacters._OnApplyProperty(textEditor, Block.TextAlignmentProperty, TextAlignment.Justify, applyToParagraphs: true);
		}
	}

	private static void OnApplySingleSpace(object sender, ExecutedRoutedEventArgs e)
	{
	}

	private static void OnApplyOneAndAHalfSpace(object sender, ExecutedRoutedEventArgs e)
	{
	}

	private static void OnApplyDoubleSpace(object sender, ExecutedRoutedEventArgs e)
	{
	}

	private static void OnApplyParagraphFlowDirectionLTR(object sender, ExecutedRoutedEventArgs e)
	{
		TextEditorCharacters._OnApplyProperty(TextEditor._GetTextEditor(sender), FrameworkElement.FlowDirectionProperty, FlowDirection.LeftToRight, applyToParagraphs: true);
	}

	private static void OnApplyParagraphFlowDirectionRTL(object sender, ExecutedRoutedEventArgs e)
	{
		TextEditorCharacters._OnApplyProperty(TextEditor._GetTextEditor(sender), FrameworkElement.FlowDirectionProperty, FlowDirection.RightToLeft, applyToParagraphs: true);
	}

	private static void OnQueryStatusNYI(object sender, CanExecuteRoutedEventArgs e)
	{
		if (TextEditor._GetTextEditor(sender) != null)
		{
			e.CanExecute = true;
		}
	}
}
