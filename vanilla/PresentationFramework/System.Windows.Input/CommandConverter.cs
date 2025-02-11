using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;

namespace System.Windows.Input;

/// <summary>Converts an <see cref="T:System.Windows.Input.ICommand" /> object to and from other types.</summary>
public sealed class CommandConverter : TypeConverter
{
	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.Input.ICommand" />, using the specified context.</summary>
	/// <returns>true if <paramref name="sourceType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="sourceType">The type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Input.ICommand" /> can be converted to the specified type, using the specified context.</summary>
	/// <returns>true if <paramref name="destinationType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			RoutedCommand routedCommand = ((context != null) ? (context.Instance as RoutedCommand) : null);
			if (routedCommand != null && routedCommand.OwnerType != null && IsKnownType(routedCommand.OwnerType))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Attempts to convert the specified object to an <see cref="T:System.Windows.Input.ICommand" />, using the specified context.</summary>
	/// <returns>The converted object, or null if <paramref name="source" /> is an empty string.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> cannot be converted.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		if (source != null && source is string)
		{
			if (!((string)source != string.Empty))
			{
				return null;
			}
			ParseUri((string)source, out var typeName, out var localName);
			ICommand command = ConvertFromHelper(GetTypeFromContext(context, typeName), localName);
			if (command != null)
			{
				return command;
			}
		}
		throw GetConvertFromException(source);
	}

	internal static ICommand ConvertFromHelper(Type ownerType, string localName)
	{
		ICommand command = null;
		if (IsKnownType(ownerType) || ownerType == null)
		{
			command = GetKnownCommand(localName, ownerType);
		}
		if (command == null && ownerType != null)
		{
			PropertyInfo property = ownerType.GetProperty(localName, BindingFlags.Static | BindingFlags.Public);
			if (property != null)
			{
				command = property.GetValue(null, null) as ICommand;
			}
			if (command == null)
			{
				FieldInfo field = ownerType.GetField(localName, BindingFlags.Static | BindingFlags.Public);
				if (field != null)
				{
					command = field.GetValue(null) as ICommand;
				}
			}
		}
		return command;
	}

	/// <summary>Attempts to convert an <see cref="T:System.Windows.Input.ICommand" /> to the specified type, using the specified context.</summary>
	/// <returns>The converted object, or an empty string.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (null == destinationType)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string))
		{
			if (value is RoutedCommand routedCommand && routedCommand.OwnerType != null && IsKnownType(routedCommand.OwnerType))
			{
				return routedCommand.Name;
			}
			return string.Empty;
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static bool IsKnownType(Type commandType)
	{
		if (commandType == typeof(ApplicationCommands) || commandType == typeof(EditingCommands) || commandType == typeof(NavigationCommands) || commandType == typeof(ComponentCommands) || commandType == typeof(MediaCommands))
		{
			return true;
		}
		return false;
	}

	private Type GetTypeFromContext(ITypeDescriptorContext context, string typeName)
	{
		if (context != null && typeName != null)
		{
			IXamlTypeResolver xamlTypeResolver = (IXamlTypeResolver)context.GetService(typeof(IXamlTypeResolver));
			if (xamlTypeResolver != null)
			{
				return xamlTypeResolver.Resolve(typeName);
			}
		}
		return null;
	}

	private void ParseUri(string source, out string typeName, out string localName)
	{
		typeName = null;
		localName = source.Trim();
		int num = localName.LastIndexOf('.');
		if (num >= 0)
		{
			typeName = localName.Substring(0, num);
			localName = localName.Substring(num + 1);
		}
	}

	private static RoutedUICommand GetKnownCommand(string localName, Type ownerType)
	{
		RoutedUICommand routedUICommand = null;
		bool flag = false;
		if (ownerType == null)
		{
			flag = true;
		}
		if (ownerType == typeof(NavigationCommands) || (routedUICommand == null && flag))
		{
			switch (localName)
			{
			case "BrowseBack":
				routedUICommand = NavigationCommands.BrowseBack;
				break;
			case "BrowseForward":
				routedUICommand = NavigationCommands.BrowseForward;
				break;
			case "BrowseHome":
				routedUICommand = NavigationCommands.BrowseHome;
				break;
			case "BrowseStop":
				routedUICommand = NavigationCommands.BrowseStop;
				break;
			case "Refresh":
				routedUICommand = NavigationCommands.Refresh;
				break;
			case "Favorites":
				routedUICommand = NavigationCommands.Favorites;
				break;
			case "Search":
				routedUICommand = NavigationCommands.Search;
				break;
			case "IncreaseZoom":
				routedUICommand = NavigationCommands.IncreaseZoom;
				break;
			case "DecreaseZoom":
				routedUICommand = NavigationCommands.DecreaseZoom;
				break;
			case "Zoom":
				routedUICommand = NavigationCommands.Zoom;
				break;
			case "NextPage":
				routedUICommand = NavigationCommands.NextPage;
				break;
			case "PreviousPage":
				routedUICommand = NavigationCommands.PreviousPage;
				break;
			case "FirstPage":
				routedUICommand = NavigationCommands.FirstPage;
				break;
			case "LastPage":
				routedUICommand = NavigationCommands.LastPage;
				break;
			case "GoToPage":
				routedUICommand = NavigationCommands.GoToPage;
				break;
			case "NavigateJournal":
				routedUICommand = NavigationCommands.NavigateJournal;
				break;
			}
		}
		if (ownerType == typeof(ApplicationCommands) || (routedUICommand == null && flag))
		{
			switch (localName)
			{
			case "Cut":
				routedUICommand = ApplicationCommands.Cut;
				break;
			case "Copy":
				routedUICommand = ApplicationCommands.Copy;
				break;
			case "Paste":
				routedUICommand = ApplicationCommands.Paste;
				break;
			case "Undo":
				routedUICommand = ApplicationCommands.Undo;
				break;
			case "Redo":
				routedUICommand = ApplicationCommands.Redo;
				break;
			case "Delete":
				routedUICommand = ApplicationCommands.Delete;
				break;
			case "Find":
				routedUICommand = ApplicationCommands.Find;
				break;
			case "Replace":
				routedUICommand = ApplicationCommands.Replace;
				break;
			case "Help":
				routedUICommand = ApplicationCommands.Help;
				break;
			case "New":
				routedUICommand = ApplicationCommands.New;
				break;
			case "Open":
				routedUICommand = ApplicationCommands.Open;
				break;
			case "Save":
				routedUICommand = ApplicationCommands.Save;
				break;
			case "SaveAs":
				routedUICommand = ApplicationCommands.SaveAs;
				break;
			case "Close":
				routedUICommand = ApplicationCommands.Close;
				break;
			case "Print":
				routedUICommand = ApplicationCommands.Print;
				break;
			case "CancelPrint":
				routedUICommand = ApplicationCommands.CancelPrint;
				break;
			case "PrintPreview":
				routedUICommand = ApplicationCommands.PrintPreview;
				break;
			case "Properties":
				routedUICommand = ApplicationCommands.Properties;
				break;
			case "ContextMenu":
				routedUICommand = ApplicationCommands.ContextMenu;
				break;
			case "CorrectionList":
				routedUICommand = ApplicationCommands.CorrectionList;
				break;
			case "SelectAll":
				routedUICommand = ApplicationCommands.SelectAll;
				break;
			case "Stop":
				routedUICommand = ApplicationCommands.Stop;
				break;
			case "NotACommand":
				routedUICommand = ApplicationCommands.NotACommand;
				break;
			}
		}
		if (ownerType == typeof(ComponentCommands) || (routedUICommand == null && flag))
		{
			switch (localName)
			{
			case "ScrollPageLeft":
				routedUICommand = ComponentCommands.ScrollPageLeft;
				break;
			case "ScrollPageRight":
				routedUICommand = ComponentCommands.ScrollPageRight;
				break;
			case "ScrollPageUp":
				routedUICommand = ComponentCommands.ScrollPageUp;
				break;
			case "ScrollPageDown":
				routedUICommand = ComponentCommands.ScrollPageDown;
				break;
			case "ScrollByLine":
				routedUICommand = ComponentCommands.ScrollByLine;
				break;
			case "MoveLeft":
				routedUICommand = ComponentCommands.MoveLeft;
				break;
			case "MoveRight":
				routedUICommand = ComponentCommands.MoveRight;
				break;
			case "MoveUp":
				routedUICommand = ComponentCommands.MoveUp;
				break;
			case "MoveDown":
				routedUICommand = ComponentCommands.MoveDown;
				break;
			case "ExtendSelectionUp":
				routedUICommand = ComponentCommands.ExtendSelectionUp;
				break;
			case "ExtendSelectionDown":
				routedUICommand = ComponentCommands.ExtendSelectionDown;
				break;
			case "ExtendSelectionLeft":
				routedUICommand = ComponentCommands.ExtendSelectionLeft;
				break;
			case "ExtendSelectionRight":
				routedUICommand = ComponentCommands.ExtendSelectionRight;
				break;
			case "MoveToHome":
				routedUICommand = ComponentCommands.MoveToHome;
				break;
			case "MoveToEnd":
				routedUICommand = ComponentCommands.MoveToEnd;
				break;
			case "MoveToPageUp":
				routedUICommand = ComponentCommands.MoveToPageUp;
				break;
			case "MoveToPageDown":
				routedUICommand = ComponentCommands.MoveToPageDown;
				break;
			case "SelectToHome":
				routedUICommand = ComponentCommands.SelectToHome;
				break;
			case "SelectToEnd":
				routedUICommand = ComponentCommands.SelectToEnd;
				break;
			case "SelectToPageDown":
				routedUICommand = ComponentCommands.SelectToPageDown;
				break;
			case "SelectToPageUp":
				routedUICommand = ComponentCommands.SelectToPageUp;
				break;
			case "MoveFocusUp":
				routedUICommand = ComponentCommands.MoveFocusUp;
				break;
			case "MoveFocusDown":
				routedUICommand = ComponentCommands.MoveFocusDown;
				break;
			case "MoveFocusBack":
				routedUICommand = ComponentCommands.MoveFocusBack;
				break;
			case "MoveFocusForward":
				routedUICommand = ComponentCommands.MoveFocusForward;
				break;
			case "MoveFocusPageUp":
				routedUICommand = ComponentCommands.MoveFocusPageUp;
				break;
			case "MoveFocusPageDown":
				routedUICommand = ComponentCommands.MoveFocusPageDown;
				break;
			}
		}
		if (ownerType == typeof(EditingCommands) || (routedUICommand == null && flag))
		{
			switch (localName)
			{
			case "ToggleInsert":
				routedUICommand = EditingCommands.ToggleInsert;
				break;
			case "Delete":
				routedUICommand = EditingCommands.Delete;
				break;
			case "Backspace":
				routedUICommand = EditingCommands.Backspace;
				break;
			case "DeleteNextWord":
				routedUICommand = EditingCommands.DeleteNextWord;
				break;
			case "DeletePreviousWord":
				routedUICommand = EditingCommands.DeletePreviousWord;
				break;
			case "EnterParagraphBreak":
				routedUICommand = EditingCommands.EnterParagraphBreak;
				break;
			case "EnterLineBreak":
				routedUICommand = EditingCommands.EnterLineBreak;
				break;
			case "TabForward":
				routedUICommand = EditingCommands.TabForward;
				break;
			case "TabBackward":
				routedUICommand = EditingCommands.TabBackward;
				break;
			case "MoveRightByCharacter":
				routedUICommand = EditingCommands.MoveRightByCharacter;
				break;
			case "MoveLeftByCharacter":
				routedUICommand = EditingCommands.MoveLeftByCharacter;
				break;
			case "MoveRightByWord":
				routedUICommand = EditingCommands.MoveRightByWord;
				break;
			case "MoveLeftByWord":
				routedUICommand = EditingCommands.MoveLeftByWord;
				break;
			case "MoveDownByLine":
				routedUICommand = EditingCommands.MoveDownByLine;
				break;
			case "MoveUpByLine":
				routedUICommand = EditingCommands.MoveUpByLine;
				break;
			case "MoveDownByParagraph":
				routedUICommand = EditingCommands.MoveDownByParagraph;
				break;
			case "MoveUpByParagraph":
				routedUICommand = EditingCommands.MoveUpByParagraph;
				break;
			case "MoveDownByPage":
				routedUICommand = EditingCommands.MoveDownByPage;
				break;
			case "MoveUpByPage":
				routedUICommand = EditingCommands.MoveUpByPage;
				break;
			case "MoveToLineStart":
				routedUICommand = EditingCommands.MoveToLineStart;
				break;
			case "MoveToLineEnd":
				routedUICommand = EditingCommands.MoveToLineEnd;
				break;
			case "MoveToDocumentStart":
				routedUICommand = EditingCommands.MoveToDocumentStart;
				break;
			case "MoveToDocumentEnd":
				routedUICommand = EditingCommands.MoveToDocumentEnd;
				break;
			case "SelectRightByCharacter":
				routedUICommand = EditingCommands.SelectRightByCharacter;
				break;
			case "SelectLeftByCharacter":
				routedUICommand = EditingCommands.SelectLeftByCharacter;
				break;
			case "SelectRightByWord":
				routedUICommand = EditingCommands.SelectRightByWord;
				break;
			case "SelectLeftByWord":
				routedUICommand = EditingCommands.SelectLeftByWord;
				break;
			case "SelectDownByLine":
				routedUICommand = EditingCommands.SelectDownByLine;
				break;
			case "SelectUpByLine":
				routedUICommand = EditingCommands.SelectUpByLine;
				break;
			case "SelectDownByParagraph":
				routedUICommand = EditingCommands.SelectDownByParagraph;
				break;
			case "SelectUpByParagraph":
				routedUICommand = EditingCommands.SelectUpByParagraph;
				break;
			case "SelectDownByPage":
				routedUICommand = EditingCommands.SelectDownByPage;
				break;
			case "SelectUpByPage":
				routedUICommand = EditingCommands.SelectUpByPage;
				break;
			case "SelectToLineStart":
				routedUICommand = EditingCommands.SelectToLineStart;
				break;
			case "SelectToLineEnd":
				routedUICommand = EditingCommands.SelectToLineEnd;
				break;
			case "SelectToDocumentStart":
				routedUICommand = EditingCommands.SelectToDocumentStart;
				break;
			case "SelectToDocumentEnd":
				routedUICommand = EditingCommands.SelectToDocumentEnd;
				break;
			case "ToggleBold":
				routedUICommand = EditingCommands.ToggleBold;
				break;
			case "ToggleItalic":
				routedUICommand = EditingCommands.ToggleItalic;
				break;
			case "ToggleUnderline":
				routedUICommand = EditingCommands.ToggleUnderline;
				break;
			case "ToggleSubscript":
				routedUICommand = EditingCommands.ToggleSubscript;
				break;
			case "ToggleSuperscript":
				routedUICommand = EditingCommands.ToggleSuperscript;
				break;
			case "IncreaseFontSize":
				routedUICommand = EditingCommands.IncreaseFontSize;
				break;
			case "DecreaseFontSize":
				routedUICommand = EditingCommands.DecreaseFontSize;
				break;
			case "ApplyFontSize":
				routedUICommand = EditingCommands.ApplyFontSize;
				break;
			case "ApplyFontFamily":
				routedUICommand = EditingCommands.ApplyFontFamily;
				break;
			case "ApplyForeground":
				routedUICommand = EditingCommands.ApplyForeground;
				break;
			case "ApplyBackground":
				routedUICommand = EditingCommands.ApplyBackground;
				break;
			case "AlignLeft":
				routedUICommand = EditingCommands.AlignLeft;
				break;
			case "AlignCenter":
				routedUICommand = EditingCommands.AlignCenter;
				break;
			case "AlignRight":
				routedUICommand = EditingCommands.AlignRight;
				break;
			case "AlignJustify":
				routedUICommand = EditingCommands.AlignJustify;
				break;
			case "ToggleBullets":
				routedUICommand = EditingCommands.ToggleBullets;
				break;
			case "ToggleNumbering":
				routedUICommand = EditingCommands.ToggleNumbering;
				break;
			case "IncreaseIndentation":
				routedUICommand = EditingCommands.IncreaseIndentation;
				break;
			case "DecreaseIndentation":
				routedUICommand = EditingCommands.DecreaseIndentation;
				break;
			case "CorrectSpellingError":
				routedUICommand = EditingCommands.CorrectSpellingError;
				break;
			case "IgnoreSpellingError":
				routedUICommand = EditingCommands.IgnoreSpellingError;
				break;
			}
		}
		if (ownerType == typeof(MediaCommands) || (routedUICommand == null && flag))
		{
			switch (localName)
			{
			case "Play":
				routedUICommand = MediaCommands.Play;
				break;
			case "Pause":
				routedUICommand = MediaCommands.Pause;
				break;
			case "Stop":
				routedUICommand = MediaCommands.Stop;
				break;
			case "Record":
				routedUICommand = MediaCommands.Record;
				break;
			case "NextTrack":
				routedUICommand = MediaCommands.NextTrack;
				break;
			case "PreviousTrack":
				routedUICommand = MediaCommands.PreviousTrack;
				break;
			case "FastForward":
				routedUICommand = MediaCommands.FastForward;
				break;
			case "Rewind":
				routedUICommand = MediaCommands.Rewind;
				break;
			case "ChannelUp":
				routedUICommand = MediaCommands.ChannelUp;
				break;
			case "ChannelDown":
				routedUICommand = MediaCommands.ChannelDown;
				break;
			case "TogglePlayPause":
				routedUICommand = MediaCommands.TogglePlayPause;
				break;
			case "IncreaseVolume":
				routedUICommand = MediaCommands.IncreaseVolume;
				break;
			case "DecreaseVolume":
				routedUICommand = MediaCommands.DecreaseVolume;
				break;
			case "MuteVolume":
				routedUICommand = MediaCommands.MuteVolume;
				break;
			case "IncreaseTreble":
				routedUICommand = MediaCommands.IncreaseTreble;
				break;
			case "DecreaseTreble":
				routedUICommand = MediaCommands.DecreaseTreble;
				break;
			case "IncreaseBass":
				routedUICommand = MediaCommands.IncreaseBass;
				break;
			case "DecreaseBass":
				routedUICommand = MediaCommands.DecreaseBass;
				break;
			case "BoostBass":
				routedUICommand = MediaCommands.BoostBass;
				break;
			case "IncreaseMicrophoneVolume":
				routedUICommand = MediaCommands.IncreaseMicrophoneVolume;
				break;
			case "DecreaseMicrophoneVolume":
				routedUICommand = MediaCommands.DecreaseMicrophoneVolume;
				break;
			case "MuteMicrophoneVolume":
				routedUICommand = MediaCommands.MuteMicrophoneVolume;
				break;
			case "ToggleMicrophoneOnOff":
				routedUICommand = MediaCommands.ToggleMicrophoneOnOff;
				break;
			case "Select":
				routedUICommand = MediaCommands.Select;
				break;
			}
		}
		return routedUICommand;
	}

	internal static object GetKnownControlCommand(Type ownerType, string commandName)
	{
		if (ownerType == typeof(ScrollBar))
		{
			switch (commandName)
			{
			case "LineUpCommand":
				return ScrollBar.LineUpCommand;
			case "LineDownCommand":
				return ScrollBar.LineDownCommand;
			case "LineLeftCommand":
				return ScrollBar.LineLeftCommand;
			case "LineRightCommand":
				return ScrollBar.LineRightCommand;
			case "PageUpCommand":
				return ScrollBar.PageUpCommand;
			case "PageDownCommand":
				return ScrollBar.PageDownCommand;
			case "PageLeftCommand":
				return ScrollBar.PageLeftCommand;
			case "PageRightCommand":
				return ScrollBar.PageRightCommand;
			}
		}
		else if (ownerType == typeof(Slider))
		{
			if (commandName == "IncreaseLarge")
			{
				return Slider.IncreaseLarge;
			}
			if (commandName == "DecreaseLarge")
			{
				return Slider.DecreaseLarge;
			}
		}
		return null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandConverter" /> class.</summary>
	public CommandConverter()
	{
	}
}
