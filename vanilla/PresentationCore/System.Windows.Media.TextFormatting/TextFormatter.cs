using System.ComponentModel;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

/// <summary>Provides services for formatting text and breaking text lines using a custom text layout client.</summary>
public abstract class TextFormatter : IDisposable
{
	private static readonly object _staticLock = new object();

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" /> class with the specified formatting mode. This is a static method.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" />.</returns>
	/// <param name="textFormattingMode">The <see cref="T:System.Windows.Media.TextFormattingMode" /> that specifies the text layout for the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" />.  </param>
	public static TextFormatter Create(TextFormattingMode textFormattingMode)
	{
		if (textFormattingMode < TextFormattingMode.Ideal || textFormattingMode > TextFormattingMode.Display)
		{
			throw new InvalidEnumArgumentException("textFormattingMode", (int)textFormattingMode, typeof(TextFormattingMode));
		}
		return new TextFormatterImp(textFormattingMode);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" /> class. This is a static method.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" />.</returns>
	public static TextFormatter Create()
	{
		return new TextFormatterImp();
	}

	[FriendAccessAllowed]
	internal static TextFormatter CreateFromContext(TextFormatterContext soleContext)
	{
		return new TextFormatterImp(soleContext, TextFormattingMode.Ideal);
	}

	[FriendAccessAllowed]
	internal static TextFormatter CreateFromContext(TextFormatterContext soleContext, TextFormattingMode textFormattingMode)
	{
		return new TextFormatterImp(soleContext, textFormattingMode);
	}

	[FriendAccessAllowed]
	internal static TextFormatter FromCurrentDispatcher()
	{
		return FromCurrentDispatcher(TextFormattingMode.Ideal);
	}

	[FriendAccessAllowed]
	internal static TextFormatter FromCurrentDispatcher(TextFormattingMode textFormattingMode)
	{
		Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
		if (currentDispatcher == null)
		{
			throw new ArgumentException(SR.CurrentDispatcherNotFound);
		}
		TextFormatter textFormatter = ((textFormattingMode != TextFormattingMode.Display) ? ((TextFormatterImp)currentDispatcher.Reserved1) : ((TextFormatterImp)currentDispatcher.Reserved4));
		if (textFormatter == null)
		{
			lock (_staticLock)
			{
				if (textFormatter == null)
				{
					textFormatter = Create(textFormattingMode);
					if (textFormattingMode == TextFormattingMode.Display)
					{
						currentDispatcher.Reserved4 = textFormatter;
					}
					else
					{
						currentDispatcher.Reserved1 = textFormatter;
					}
				}
			}
		}
		Invariant.Assert(textFormatter != null);
		return textFormatter;
	}

	/// <summary>Releases all managed and unmanaged resources used by the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" /> object.</summary>
	public virtual void Dispose()
	{
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> that is used for formatting and displaying document content.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> value that represents a line of text that can be displayed.</returns>
	/// <param name="textSource">A <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> value that represents the text source for the line.</param>
	/// <param name="firstCharIndex">An <see cref="T:System.Int32" /> value that specifies the character index of the starting character in the line.</param>
	/// <param name="paragraphWidth">A <see cref="T:System.Double" /> value that specifies the width of the paragraph that the line fills.</param>
	/// <param name="paragraphProperties">A <see cref="T:System.Windows.Media.TextFormatting.TextParagraphProperties" /> value that represents paragraph properties, such as flow direction, alignment, or indentation.</param>
	/// <param name="previousLineBreak">A <see cref="T:System.Windows.Media.TextFormatting.TextLineBreak" /> value that specifies the text formatter state, in terms of where the previous line in the paragraph was broken by the text formatting process.</param>
	public abstract TextLine FormatLine(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak);

	/// <summary>Creates a <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> that is used for formatting and displaying document content.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> value that represents a line of text that can be displayed.</returns>
	/// <param name="textSource">A <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> object that represents the text source for the line.</param>
	/// <param name="firstCharIndex">An <see cref="T:System.Int32" /> value that specifies the character index of the starting character in the line.</param>
	/// <param name="paragraphWidth">A <see cref="T:System.Double" /> value that specifies the width of the paragraph that the line fills.</param>
	/// <param name="paragraphProperties">A <see cref="T:System.Windows.Media.TextFormatting.TextParagraphProperties" /> object that represents paragraph properties, such as flow direction, alignment, or indentation.</param>
	/// <param name="previousLineBreak">A <see cref="T:System.Windows.Media.TextFormatting.TextLineBreak" /> object that specifies the text formatter state, in terms of where the previous line in the paragraph was broken by the text formatting process.</param>
	/// <param name="textRunCache">A <see cref="T:System.Windows.Media.TextFormatting.TextRunCache" /> object that represents the caching mechanism for the layout of text.</param>
	public abstract TextLine FormatLine(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak, TextRunCache textRunCache);

	[FriendAccessAllowed]
	internal abstract TextLine RecreateLine(TextSource textSource, int firstCharIndex, int lineLength, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak, TextRunCache textRunCache);

	[FriendAccessAllowed]
	internal abstract TextParagraphCache CreateParagraphCache(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties, TextLineBreak previousLineBreak, TextRunCache textRunCache);

	/// <summary>Returns a value that represents the smallest and largest possible paragraph width that can fully contain the specified text content.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.MinMaxParagraphWidth" /> value that represents the smallest and largest possible paragraph width that can fully contain the specified text content.</returns>
	/// <param name="textSource">A <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> object that represents the text source for the line.</param>
	/// <param name="firstCharIndex">An <see cref="T:System.Int32" /> value that specifies the character index of the starting character in the line.</param>
	/// <param name="paragraphProperties">A <see cref="T:System.Windows.Media.TextFormatting.TextParagraphProperties" /> object that represents paragraph properties, such as flow direction, alignment, or indentation.</param>
	public abstract MinMaxParagraphWidth FormatMinMaxParagraphWidth(TextSource textSource, int firstCharIndex, TextParagraphProperties paragraphProperties);

	/// <summary>Returns a value that represents the smallest and largest possible paragraph width that can fully contain the specified text content.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.MinMaxParagraphWidth" /> value that represents the smallest and largest possible paragraph width that can fully contain the specified text content.</returns>
	/// <param name="textSource">A <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> object that represents the text source for the line.</param>
	/// <param name="firstCharIndex">An <see cref="T:System.Int32" /> value that specifies the character index of the starting character in the line.</param>
	/// <param name="paragraphProperties">A <see cref="T:System.Windows.Media.TextFormatting.TextParagraphProperties" /> object that represents paragraph properties, such as flow direction, alignment, or indentation.</param>
	/// <param name="textRunCache">A <see cref="T:System.Windows.Media.TextFormatting.TextRunCache" /> object that represents the caching mechanism for the layout of text.</param>
	public abstract MinMaxParagraphWidth FormatMinMaxParagraphWidth(TextSource textSource, int firstCharIndex, TextParagraphProperties paragraphProperties, TextRunCache textRunCache);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" /> class.</summary>
	protected TextFormatter()
	{
	}
}
