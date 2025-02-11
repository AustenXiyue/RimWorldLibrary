using System.Diagnostics;

namespace MS.Internal.Shaping;

internal class GlyphInfoList
{
	private UshortList _glyphs;

	private UshortList _glyphFlags;

	private UshortList _firstChars;

	private UshortList _ligatureCounts;

	public int Length => _glyphs.Length;

	internal int Offset => _glyphs.Offset;

	public UshortList Glyphs => _glyphs;

	public UshortList GlyphFlags => _glyphFlags;

	public UshortList FirstChars => _firstChars;

	public UshortList LigatureCounts => _ligatureCounts;

	internal GlyphInfoList(int capacity, int leap, bool justify)
	{
		_glyphs = new UshortList(capacity, leap);
		_glyphFlags = new UshortList(capacity, leap);
		_firstChars = new UshortList(capacity, leap);
		_ligatureCounts = new UshortList(capacity, leap);
	}

	[Conditional("DEBUG")]
	internal void ValidateLength(int cch)
	{
	}

	public void SetRange(int index, int length)
	{
		_glyphs.SetRange(index, length);
		_glyphFlags.SetRange(index, length);
		_firstChars.SetRange(index, length);
		_ligatureCounts.SetRange(index, length);
	}

	public void SetLength(int length)
	{
		_glyphs.Length = length;
		_glyphFlags.Length = length;
		_firstChars.Length = length;
		_ligatureCounts.Length = length;
	}

	public void Insert(int index, int Count)
	{
		_glyphs.Insert(index, Count);
		_glyphFlags.Insert(index, Count);
		_firstChars.Insert(index, Count);
		_ligatureCounts.Insert(index, Count);
	}

	public void Remove(int index, int Count)
	{
		_glyphs.Remove(index, Count);
		_glyphFlags.Remove(index, Count);
		_firstChars.Remove(index, Count);
		_ligatureCounts.Remove(index, Count);
	}
}
