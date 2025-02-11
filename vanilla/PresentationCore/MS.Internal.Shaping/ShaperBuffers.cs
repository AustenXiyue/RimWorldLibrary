namespace MS.Internal.Shaping;

internal class ShaperBuffers
{
	private UshortList _charMap;

	private GlyphInfoList _glyphInfoList;

	private OpenTypeLayoutWorkspace _layoutWorkspace;

	private ShaperFeaturesList _textFeatures;

	public UshortList CharMap => _charMap;

	public GlyphInfoList GlyphInfoList
	{
		get
		{
			return _glyphInfoList;
		}
		set
		{
			_glyphInfoList = value;
		}
	}

	public OpenTypeLayoutWorkspace LayoutWorkspace => _layoutWorkspace;

	public ShaperFeaturesList TextFeatures => _textFeatures;

	public ShaperBuffers(ushort charCount, ushort glyphCount)
	{
		_glyphInfoList = new GlyphInfoList((glyphCount > charCount) ? glyphCount : charCount, 16, justify: false);
		_charMap = new UshortList(charCount, 16);
		_layoutWorkspace = new OpenTypeLayoutWorkspace();
		_charMap.SetRange(0, charCount);
		if (glyphCount > 0)
		{
			_glyphInfoList.SetRange(0, glyphCount);
		}
	}

	~ShaperBuffers()
	{
		_glyphInfoList = null;
		_charMap = null;
		_layoutWorkspace = null;
		_textFeatures = null;
	}

	public bool Initialize(ushort charCount, ushort glyphCount)
	{
		if (charCount <= 0)
		{
			return false;
		}
		if (_charMap.Length > 0)
		{
			_charMap.Remove(0, _charMap.Length);
		}
		_charMap.Insert(0, charCount);
		if (_glyphInfoList.Length > 0)
		{
			_glyphInfoList.Remove(0, _glyphInfoList.Length);
		}
		if (glyphCount > 0)
		{
			_glyphInfoList.Insert(0, glyphCount);
		}
		return true;
	}

	public bool InitializeFeatureList(ushort size, ushort keep)
	{
		if (_textFeatures == null)
		{
			_textFeatures = new ShaperFeaturesList();
			if (_textFeatures == null)
			{
				return false;
			}
			_textFeatures.Initialize(size);
		}
		else
		{
			_textFeatures.Resize(size, keep);
		}
		return true;
	}
}
