using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using MS.Internal;
using MS.Internal.Commands;

namespace System.Windows.Documents;

internal static class TextEditorCopyPaste
{
	private const string KeyCopy = "Ctrl+C";

	private const string KeyCopyFormat = "Ctrl+Shift+C";

	private const string KeyCtrlInsert = "Ctrl+Insert";

	private const string KeyCut = "Ctrl+X";

	private const string KeyPasteFormat = "Ctrl+Shift+V";

	private const string KeyShiftDelete = "Shift+Delete";

	private const string KeyShiftInsert = "Shift+Insert";

	internal static void _RegisterClassHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
	{
		CommandHelpers.RegisterCommandHandler(controlType, ApplicationCommands.Copy, OnCopy, OnQueryStatusCopy, KeyGesture.CreateFromResourceStrings("Ctrl+C", SR.KeyCopyDisplayString), KeyGesture.CreateFromResourceStrings("Ctrl+Insert", SR.KeyCtrlInsertDisplayString));
		if (acceptsRichContent)
		{
			CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.CopyFormat, OnCopyFormat, OnQueryStatusCopyFormat, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+C", "KeyCopyFormatDisplayString"));
		}
		if (!readOnly)
		{
			CommandHelpers.RegisterCommandHandler(controlType, ApplicationCommands.Cut, OnCut, OnQueryStatusCut, KeyGesture.CreateFromResourceStrings("Ctrl+X", SR.KeyCutDisplayString), KeyGesture.CreateFromResourceStrings("Shift+Delete", SR.KeyShiftDeleteDisplayString));
			ExecutedRoutedEventHandler executedRoutedEventHandler = OnPaste;
			CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = OnQueryStatusPaste;
			InputGesture inputGesture = KeyGesture.CreateFromResourceStrings("Shift+Insert", SR.KeyShiftInsertDisplayString);
			CommandHelpers.RegisterCommandHandler(controlType, ApplicationCommands.Paste, executedRoutedEventHandler, canExecuteRoutedEventHandler, inputGesture);
			if (acceptsRichContent)
			{
				CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.PasteFormat, OnPasteFormat, OnQueryStatusPasteFormat, "Ctrl+Shift+V", "KeyPasteFormatDisplayString");
			}
		}
	}

	internal static DataObject _CreateDataObject(TextEditor This, bool isDragDrop)
	{
		DataObject dataObject = new DataObject();
		string text = This.Selection.Text;
		if (text != string.Empty)
		{
			if (ConfirmDataFormatSetting(This.UiScope, dataObject, DataFormats.Text))
			{
				dataObject.SetData(DataFormats.Text, text, autoConvert: true);
			}
			if (ConfirmDataFormatSetting(This.UiScope, dataObject, DataFormats.UnicodeText))
			{
				dataObject.SetData(DataFormats.UnicodeText, text, autoConvert: true);
			}
		}
		if (This.AcceptsRichContent)
		{
			Stream stream = null;
			string text2 = WpfPayload.SaveRange(This.Selection, ref stream, useFlowDocumentAsRoot: false);
			if (text2.Length > 0)
			{
				if (stream != null && ConfirmDataFormatSetting(This.UiScope, dataObject, DataFormats.XamlPackage))
				{
					dataObject.SetData(DataFormats.XamlPackage, stream);
				}
				if (ConfirmDataFormatSetting(This.UiScope, dataObject, DataFormats.Rtf))
				{
					string text3 = ConvertXamlToRtf(text2, stream);
					if (text3 != string.Empty)
					{
						dataObject.SetData(DataFormats.Rtf, text3, autoConvert: true);
					}
				}
				if (This.Selection.GetUIElementSelected() is Image image && image.Source is BitmapSource)
				{
					dataObject.SetImage((BitmapSource)image.Source);
				}
			}
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			TextRangeSerialization.WriteXaml(new XmlTextWriter(stringWriter), This.Selection, useFlowDocumentAsRoot: false, null);
			string text4 = stringWriter.ToString();
			if (text4.Length > 0 && ConfirmDataFormatSetting(This.UiScope, dataObject, DataFormats.Xaml))
			{
				dataObject.SetData(DataFormats.Xaml, text4, autoConvert: false);
			}
		}
		DataObjectCopyingEventArgs dataObjectCopyingEventArgs = new DataObjectCopyingEventArgs(dataObject, isDragDrop);
		This.UiScope.RaiseEvent(dataObjectCopyingEventArgs);
		if (dataObjectCopyingEventArgs.CommandCancelled)
		{
			dataObject = null;
		}
		return dataObject;
	}

	internal static bool _DoPaste(TextEditor This, IDataObject dataObject, bool isDragDrop)
	{
		Invariant.Assert(dataObject != null);
		bool result = false;
		string pasteApplyFormat = GetPasteApplyFormat(This, dataObject);
		DataObjectPastingEventArgs dataObjectPastingEventArgs;
		try
		{
			dataObjectPastingEventArgs = new DataObjectPastingEventArgs(dataObject, isDragDrop, pasteApplyFormat);
		}
		catch (ArgumentException)
		{
			return result;
		}
		This.UiScope.RaiseEvent(dataObjectPastingEventArgs);
		if (!dataObjectPastingEventArgs.CommandCancelled)
		{
			IDataObject dataObject2 = dataObjectPastingEventArgs.DataObject;
			pasteApplyFormat = dataObjectPastingEventArgs.FormatToApply;
			result = PasteContentData(This, dataObject, dataObject2, pasteApplyFormat);
		}
		return result;
	}

	internal static string GetPasteApplyFormat(TextEditor This, IDataObject dataObject)
	{
		if (This.AcceptsRichContent && dataObject.GetDataPresent(DataFormats.XamlPackage))
		{
			return DataFormats.XamlPackage;
		}
		if (This.AcceptsRichContent && dataObject.GetDataPresent(DataFormats.Xaml))
		{
			return DataFormats.Xaml;
		}
		if (This.AcceptsRichContent && dataObject.GetDataPresent(DataFormats.Rtf))
		{
			return DataFormats.Rtf;
		}
		if (dataObject.GetDataPresent(DataFormats.UnicodeText))
		{
			return DataFormats.UnicodeText;
		}
		if (dataObject.GetDataPresent(DataFormats.Text))
		{
			return DataFormats.Text;
		}
		if (This.AcceptsRichContent && dataObject is DataObject && ((DataObject)dataObject).ContainsImage())
		{
			return DataFormats.Bitmap;
		}
		return string.Empty;
	}

	internal static void Cut(TextEditor This, bool userInitiated)
	{
		TextEditorTyping._FlushPendingInputItems(This);
		TextEditorTyping._BreakTypingSequence(This);
		if (This.Selection == null || This.Selection.IsEmpty)
		{
			return;
		}
		DataObject dataObject = _CreateDataObject(This, isDragDrop: false);
		if (dataObject == null)
		{
			return;
		}
		try
		{
			Clipboard.CriticalSetDataObject(dataObject, copy: true);
		}
		catch (ExternalException) when (!FrameworkCompatibilityPreferences.ShouldThrowOnCopyOrCutFailure)
		{
			return;
		}
		using (This.Selection.DeclareChangeBlock())
		{
			TextEditorSelection._ClearSuggestedX(This);
			This.Selection.Text = string.Empty;
			if (This.Selection is TextSelection)
			{
				((TextSelection)This.Selection).ClearSpringloadFormatting();
			}
		}
	}

	internal static void Copy(TextEditor This, bool userInitiated)
	{
		TextEditorTyping._FlushPendingInputItems(This);
		TextEditorTyping._BreakTypingSequence(This);
		if (This.Selection == null || This.Selection.IsEmpty)
		{
			return;
		}
		DataObject dataObject = _CreateDataObject(This, isDragDrop: false);
		if (dataObject != null)
		{
			try
			{
				Clipboard.CriticalSetDataObject(dataObject, copy: true);
			}
			catch (ExternalException) when (!FrameworkCompatibilityPreferences.ShouldThrowOnCopyOrCutFailure)
			{
			}
		}
	}

	internal static void Paste(TextEditor This)
	{
		if (This.Selection.IsTableCellRange)
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(This);
		TextEditorTyping._BreakTypingSequence(This);
		IDataObject dataObject;
		try
		{
			dataObject = Clipboard.GetDataObject();
		}
		catch (ExternalException)
		{
			dataObject = null;
		}
		bool coversEntireContent = This.Selection.CoversEntireContent;
		if (dataObject != null)
		{
			using (This.Selection.DeclareChangeBlock())
			{
				TextEditorSelection._ClearSuggestedX(This);
				if (_DoPaste(This, dataObject, isDragDrop: false))
				{
					This.Selection.SetCaretToPosition(This.Selection.End, LogicalDirection.Backward, allowStopAtLineEnd: false, allowStopNearSpace: true);
					if (This.Selection is TextSelection)
					{
						((TextSelection)This.Selection).ClearSpringloadFormatting();
					}
				}
			}
		}
		if (coversEntireContent)
		{
			This.Selection.ValidateLayout();
		}
	}

	internal static string ConvertXamlToRtf(string xamlContent, Stream wpfContainerMemory)
	{
		XamlRtfConverter xamlRtfConverter = new XamlRtfConverter();
		if (wpfContainerMemory != null)
		{
			xamlRtfConverter.WpfPayload = WpfPayload.OpenWpfPayload(wpfContainerMemory);
		}
		return xamlRtfConverter.ConvertXamlToRtf(xamlContent);
	}

	internal static MemoryStream ConvertRtfToXaml(string rtfContent)
	{
		MemoryStream memoryStream = new MemoryStream();
		WpfPayload wpfPayload = WpfPayload.CreateWpfPayload(memoryStream);
		using (wpfPayload.Package)
		{
			using Stream stream = wpfPayload.CreateXamlStream();
			string text = new XamlRtfConverter
			{
				WpfPayload = wpfPayload
			}.ConvertRtfToXaml(rtfContent);
			if (text != string.Empty)
			{
				StreamWriter streamWriter = new StreamWriter(stream);
				using (streamWriter)
				{
					streamWriter.Write(text);
				}
			}
			else
			{
				memoryStream = null;
			}
		}
		return memoryStream;
	}

	private static void OnQueryStatusCut(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly)
		{
			if (textEditor.UiScope is PasswordBox)
			{
				args.CanExecute = false;
				args.Handled = true;
			}
			else
			{
				args.CanExecute = !textEditor.Selection.IsEmpty;
				args.Handled = true;
			}
		}
	}

	private static void OnCut(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && !(textEditor.UiScope is PasswordBox))
		{
			Cut(textEditor, args.UserInitiated);
		}
	}

	private static void OnQueryStatusCopy(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled)
		{
			if (textEditor.UiScope is PasswordBox)
			{
				args.CanExecute = false;
				args.Handled = true;
			}
			else
			{
				args.CanExecute = !textEditor.Selection.IsEmpty;
				args.Handled = true;
			}
		}
	}

	private static void OnCopy(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !(textEditor.UiScope is PasswordBox))
		{
			Copy(textEditor, args.UserInitiated);
		}
	}

	private static void OnQueryStatusPaste(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly)
		{
			return;
		}
		args.Handled = true;
		try
		{
			string pasteApplyFormat = GetPasteApplyFormat(textEditor, Clipboard.GetDataObject());
			args.CanExecute = pasteApplyFormat.Length > 0;
		}
		catch (ExternalException)
		{
			args.CanExecute = false;
		}
	}

	private static void OnPaste(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly)
		{
			Paste(textEditor);
		}
	}

	private static void OnQueryStatusCopyFormat(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled)
		{
			args.CanExecute = false;
			args.Handled = true;
		}
	}

	private static void OnCopyFormat(object sender, ExecutedRoutedEventArgs args)
	{
	}

	private static void OnQueryStatusPasteFormat(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly)
		{
			args.CanExecute = false;
			args.Handled = true;
		}
	}

	private static void OnPasteFormat(object sender, ExecutedRoutedEventArgs args)
	{
	}

	private static bool PasteContentData(TextEditor This, IDataObject dataObject, IDataObject dataObjectToApply, string formatToApply)
	{
		if (formatToApply == DataFormats.Bitmap && dataObjectToApply is DataObject && This.AcceptsRichContent && This.Selection is TextSelection && GetPasteData(dataObjectToApply, DataFormats.Bitmap) is BitmapSource bitmapSource)
		{
			MemoryStream data = WpfPayload.SaveImage(bitmapSource, "image/bmp");
			dataObjectToApply = new DataObject();
			formatToApply = DataFormats.XamlPackage;
			dataObjectToApply.SetData(DataFormats.XamlPackage, data);
		}
		if (formatToApply == DataFormats.XamlPackage)
		{
			if (This.AcceptsRichContent && This.Selection is TextSelection && GetPasteData(dataObjectToApply, DataFormats.XamlPackage) is MemoryStream stream)
			{
				object obj = WpfPayload.LoadElement(stream);
				if ((obj is Section || obj is Span) && PasteTextElement(This, (TextElement)obj))
				{
					return true;
				}
				if (obj is FrameworkElement)
				{
					((TextSelection)This.Selection).InsertEmbeddedUIElement((FrameworkElement)obj);
					return true;
				}
			}
			dataObjectToApply = dataObject;
			if (dataObjectToApply.GetDataPresent(DataFormats.Xaml))
			{
				formatToApply = DataFormats.Xaml;
			}
			else if (dataObjectToApply.GetDataPresent(DataFormats.Rtf))
			{
				formatToApply = DataFormats.Rtf;
			}
			else if (dataObjectToApply.GetDataPresent(DataFormats.UnicodeText))
			{
				formatToApply = DataFormats.UnicodeText;
			}
			else if (dataObjectToApply.GetDataPresent(DataFormats.Text))
			{
				formatToApply = DataFormats.Text;
			}
		}
		if (formatToApply == DataFormats.Xaml)
		{
			if (This.AcceptsRichContent && This.Selection is TextSelection)
			{
				object pasteData = GetPasteData(dataObjectToApply, DataFormats.Xaml);
				if (pasteData != null && PasteXaml(This, pasteData.ToString()))
				{
					return true;
				}
			}
			dataObjectToApply = dataObject;
			if (dataObjectToApply.GetDataPresent(DataFormats.Rtf))
			{
				formatToApply = DataFormats.Rtf;
			}
			else if (dataObjectToApply.GetDataPresent(DataFormats.UnicodeText))
			{
				formatToApply = DataFormats.UnicodeText;
			}
			else if (dataObjectToApply.GetDataPresent(DataFormats.Text))
			{
				formatToApply = DataFormats.Text;
			}
		}
		if (formatToApply == DataFormats.Rtf)
		{
			if (This.AcceptsRichContent)
			{
				object pasteData2 = GetPasteData(dataObjectToApply, DataFormats.Rtf);
				if (pasteData2 != null)
				{
					MemoryStream memoryStream = ConvertRtfToXaml(pasteData2.ToString());
					if (memoryStream != null)
					{
						TextElement textElement = WpfPayload.LoadElement(memoryStream) as TextElement;
						if ((textElement is Section || textElement is Span) && PasteTextElement(This, textElement))
						{
							return true;
						}
					}
				}
			}
			dataObjectToApply = dataObject;
			if (dataObjectToApply.GetDataPresent(DataFormats.UnicodeText))
			{
				formatToApply = DataFormats.UnicodeText;
			}
			else if (dataObjectToApply.GetDataPresent(DataFormats.Text))
			{
				formatToApply = DataFormats.Text;
			}
		}
		if (formatToApply == DataFormats.UnicodeText)
		{
			object pasteData3 = GetPasteData(dataObjectToApply, DataFormats.UnicodeText);
			if (pasteData3 != null)
			{
				return PastePlainText(This, pasteData3.ToString());
			}
			if (dataObjectToApply.GetDataPresent(DataFormats.Text))
			{
				formatToApply = DataFormats.Text;
				dataObjectToApply = dataObject;
			}
		}
		if (formatToApply == DataFormats.Text)
		{
			object pasteData4 = GetPasteData(dataObjectToApply, DataFormats.Text);
			if (pasteData4 != null && PastePlainText(This, pasteData4.ToString()))
			{
				return true;
			}
		}
		return false;
	}

	private static object GetPasteData(IDataObject dataObject, string dataFormat)
	{
		try
		{
			return dataObject.GetData(dataFormat, autoConvert: true);
		}
		catch (OutOfMemoryException)
		{
			return null;
		}
		catch (ExternalException)
		{
			return null;
		}
	}

	private static bool PasteTextElement(TextEditor This, TextElement sectionOrSpan)
	{
		bool flag = false;
		This.Selection.BeginChange();
		try
		{
			((TextRange)This.Selection).SetXmlVirtual(sectionOrSpan);
			TextRangeEditLists.MergeListsAroundNormalizedPosition((TextPointer)This.Selection.Start);
			TextRangeEditLists.MergeListsAroundNormalizedPosition((TextPointer)This.Selection.End);
			TextRangeEdit.MergeFlowDirection((TextPointer)This.Selection.Start);
			TextRangeEdit.MergeFlowDirection((TextPointer)This.Selection.End);
			return true;
		}
		finally
		{
			This.Selection.EndChange();
		}
	}

	private static bool PasteXaml(TextEditor This, string pasteXaml)
	{
		if (pasteXaml.Length == 0)
		{
			return false;
		}
		try
		{
			return XamlReader.Load(new XmlTextReader(new StringReader(pasteXaml)), useRestrictiveXamlReader: true) is TextElement sectionOrSpan && PasteTextElement(This, sectionOrSpan);
		}
		catch (XamlParseException ex)
		{
			Invariant.Assert(ex != null);
			return false;
		}
	}

	private static bool PastePlainText(TextEditor This, string pastedText)
	{
		pastedText = This._FilterText(pastedText, This.Selection);
		if (pastedText.Length > 0)
		{
			if (This.AcceptsRichContent && This.Selection.Start is TextPointer)
			{
				This.Selection.Text = string.Empty;
				TextPointer textPointer = TextRangeEditTables.EnsureInsertionPosition((TextPointer)This.Selection.Start);
				textPointer = textPointer.GetPositionAtOffset(0, LogicalDirection.Backward);
				TextPointer textPointer2 = textPointer.GetPositionAtOffset(0, LogicalDirection.Forward);
				int num = 0;
				for (int i = 0; i < pastedText.Length; i++)
				{
					if (pastedText[i] == '\r' || pastedText[i] == '\n')
					{
						textPointer2.InsertTextInRun(pastedText.Substring(num, i - num));
						if (!This.AcceptsReturn)
						{
							return true;
						}
						if (textPointer2.HasNonMergeableInlineAncestor)
						{
							textPointer2.InsertTextInRun(" ");
						}
						else
						{
							textPointer2 = textPointer2.InsertParagraphBreak();
						}
						if (pastedText[i] == '\r' && i + 1 < pastedText.Length && pastedText[i + 1] == '\n')
						{
							i++;
						}
						num = i + 1;
					}
				}
				textPointer2.InsertTextInRun(pastedText.Substring(num, pastedText.Length - num));
				This.Selection.Select(textPointer, textPointer2);
			}
			else
			{
				This.Selection.Text = pastedText;
			}
			return true;
		}
		return false;
	}

	private static bool ConfirmDataFormatSetting(FrameworkElement uiScope, IDataObject dataObject, string format)
	{
		DataObjectSettingDataEventArgs dataObjectSettingDataEventArgs = new DataObjectSettingDataEventArgs(dataObject, format);
		uiScope.RaiseEvent(dataObjectSettingDataEventArgs);
		return !dataObjectSettingDataEventArgs.CommandCancelled;
	}
}
