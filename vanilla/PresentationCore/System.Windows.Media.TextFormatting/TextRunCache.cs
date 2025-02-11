using System.Collections.Generic;
using MS.Internal.PresentationCore;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

/// <summary>Provides caching services to the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" /> object in order to improve performance.</summary>
public sealed class TextRunCache
{
	private TextRunCacheImp _imp;

	internal TextRunCacheImp Imp
	{
		get
		{
			return _imp;
		}
		set
		{
			_imp = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextRunCache" /> class.</summary>
	public TextRunCache()
	{
	}

	/// <summary>Notifies the text engine client of a change to the cache when text content or text run properties of <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> are added, removed, or replaced.</summary>
	/// <param name="textSourceCharacterIndex">Specifies the <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> character index position of the start of the change.</param>
	/// <param name="addition">Indicates the number of <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> characters to be added.</param>
	/// <param name="removal">Indicates the number of <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> characters to be removed.</param>
	public void Change(int textSourceCharacterIndex, int addition, int removal)
	{
		if (_imp != null)
		{
			_imp.Change(textSourceCharacterIndex, addition, removal);
		}
	}

	/// <summary>Signals the text engine client to invalidate the entire contents of the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" /> cache.</summary>
	public void Invalidate()
	{
		if (_imp != null)
		{
			_imp = null;
		}
	}

	[FriendAccessAllowed]
	internal IList<TextSpan<TextRun>> GetTextRunSpans()
	{
		if (_imp != null)
		{
			return _imp.GetTextRunSpans();
		}
		return Array.Empty<TextSpan<TextRun>>();
	}
}
