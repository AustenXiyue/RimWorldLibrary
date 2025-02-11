using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Composition;
using System.Windows.Media.Converters;
using System.Windows.Media.TextFormatting;
using MS.Internal;
using MS.Internal.FontCache;
using MS.Internal.PresentationCore;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;

namespace System.Windows.Media;

/// <summary>Represents a sequence of glyphs from a single face of a single font at a single size, and with a single rendering style.</summary>
public class GlyphRun : DUCE.IResource, ISupportInitialize
{
	private struct EmGlyphMetrics
	{
		internal double LeftSideBearing;

		internal double AdvanceWidth;

		internal double RightSideBearing;

		internal double TopSideBearing;

		internal double AdvanceHeight;

		internal double BottomSideBearing;

		internal double Baseline;

		internal EmGlyphMetrics(GlyphMetrics glyphMetrics, double designToEm, double pixelsPerDip, TextFormattingMode textFormattingMode)
		{
			if (TextFormattingMode.Display == textFormattingMode)
			{
				AdvanceWidth = TextFormatterImp.RoundDipForDisplayMode(designToEm * (double)glyphMetrics.AdvanceWidth, pixelsPerDip);
				AdvanceHeight = TextFormatterImp.RoundDipForDisplayMode(designToEm * (double)glyphMetrics.AdvanceHeight, pixelsPerDip);
				LeftSideBearing = TextFormatterImp.RoundDipForDisplayMode(designToEm * (double)glyphMetrics.LeftSideBearing, pixelsPerDip);
				RightSideBearing = TextFormatterImp.RoundDipForDisplayMode(designToEm * (double)glyphMetrics.RightSideBearing, pixelsPerDip);
				TopSideBearing = TextFormatterImp.RoundDipForDisplayMode(designToEm * (double)glyphMetrics.TopSideBearing, pixelsPerDip);
				BottomSideBearing = TextFormatterImp.RoundDipForDisplayMode(designToEm * (double)glyphMetrics.BottomSideBearing, pixelsPerDip);
				Baseline = TextFormatterImp.RoundDipForDisplayMode(designToEm * GlyphTypeface.BaselineHelper(glyphMetrics), pixelsPerDip);
			}
			else
			{
				AdvanceWidth = designToEm * (double)glyphMetrics.AdvanceWidth;
				AdvanceHeight = designToEm * (double)glyphMetrics.AdvanceHeight;
				LeftSideBearing = designToEm * (double)glyphMetrics.LeftSideBearing;
				RightSideBearing = designToEm * (double)glyphMetrics.RightSideBearing;
				TopSideBearing = designToEm * (double)glyphMetrics.TopSideBearing;
				BottomSideBearing = designToEm * (double)glyphMetrics.BottomSideBearing;
				Baseline = designToEm * GlyphTypeface.BaselineHelper(glyphMetrics);
			}
		}
	}

	internal struct Scale
	{
		internal double _baseVectorX;

		internal double _baseVectorY;

		internal bool IsValid
		{
			get
			{
				if (_baseVectorX != 0.0)
				{
					return _baseVectorY != 0.0;
				}
				return false;
			}
		}

		internal Scale(ref Matrix matrix)
		{
			double m = matrix.M11;
			double m2 = matrix.M12;
			double m3 = matrix.M21;
			double m4 = matrix.M22;
			_baseVectorX = Math.Sqrt(m * m + m2 * m2);
			if (double.IsNaN(_baseVectorX))
			{
				_baseVectorX = 0.0;
			}
			_baseVectorY = ((_baseVectorX == 0.0) ? 0.0 : (Math.Abs(m * m4 - m2 * m3) / _baseVectorX));
			if (double.IsNaN(_baseVectorY))
			{
				_baseVectorY = 0.0;
			}
		}

		internal bool IsSame(ref Scale scale)
		{
			if (_baseVectorX * 0.999999999 <= scale._baseVectorX && _baseVectorX * 1.000000001 >= scale._baseVectorX && _baseVectorY * 0.999999999 <= scale._baseVectorY)
			{
				return _baseVectorY * 1.000000001 >= scale._baseVectorY;
			}
			return false;
		}
	}

	private class DefaultCaretStopList : IList<bool>, ICollection<bool>, IEnumerable<bool>, IEnumerable
	{
		private int _count;

		public bool this[int index]
		{
			get
			{
				return true;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public int Count => _count;

		public bool IsReadOnly => true;

		public DefaultCaretStopList(int codePointCount)
		{
			_count = codePointCount + 1;
		}

		public int IndexOf(bool item)
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, bool item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public void Add(bool item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(bool item)
		{
			throw new NotSupportedException();
		}

		public void CopyTo(bool[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		public bool Remove(bool item)
		{
			throw new NotSupportedException();
		}

		IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException();
		}
	}

	private class DefaultClusterMap : IList<ushort>, ICollection<ushort>, IEnumerable<ushort>, IEnumerable
	{
		private int _count;

		public ushort this[int index]
		{
			get
			{
				return (ushort)index;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public int Count => _count;

		public bool IsReadOnly => true;

		public DefaultClusterMap(int count)
		{
			_count = count;
		}

		public int IndexOf(ushort item)
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, ushort item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public void Add(ushort item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(ushort item)
		{
			throw new NotSupportedException();
		}

		public void CopyTo(ushort[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		public bool Remove(ushort item)
		{
			throw new NotSupportedException();
		}

		IEnumerator<ushort> IEnumerable<ushort>.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException();
		}
	}

	[Flags]
	private enum GlyphRunFlags : byte
	{
		None = 0,
		IsSideways = 1,
		IsInitialized = 8,
		IsInitializing = 0x10,
		CacheInkBounds = 0x20
	}

	private DUCE.MultiChannelResource _mcr;

	private Point _baselineOrigin;

	private GlyphRunFlags _flags;

	private double _renderingEmSize;

	private IList<ushort> _glyphIndices;

	private IList<double> _advanceWidths;

	private IList<Point> _glyphOffsets;

	private int _bidiLevel;

	private GlyphTypeface _glyphTypeface;

	private IList<char> _characters;

	private IList<ushort> _clusterMap;

	private IList<bool> _caretStops;

	private XmlLanguage _language;

	private string _deviceFontName;

	private object _inkBoundingBox;

	private TextFormattingMode _textFormattingMode;

	private float _pixelsPerDip = Util.PixelsPerDip;

	private const double Sin20 = 0.3420201433256687;

	private const double InkMetricsEpsilon = 1E-07;

	private const double DefaultFontHintingSize = 12.0;

	internal static double RelativeFlatteningTolerance = 0.01;

	internal const int MaxGlyphCount = 65535;

	internal const int MaxStackAlloc = 1024;

	public float PixelsPerDip
	{
		get
		{
			CheckInitialized();
			return _pixelsPerDip;
		}
		set
		{
			CheckInitializing();
			_pixelsPerDip = value;
		}
	}

	private double AdvanceWidth
	{
		get
		{
			double num = 0.0;
			if (_advanceWidths != null)
			{
				foreach (double advanceWidth in _advanceWidths)
				{
					num += advanceWidth;
				}
			}
			return num;
		}
	}

	private double Ascent
	{
		get
		{
			if (IsSideways)
			{
				return _renderingEmSize * _glyphTypeface.Height / 2.0;
			}
			return _glyphTypeface.Baseline * _renderingEmSize;
		}
	}

	private double Height => _glyphTypeface.Height * _renderingEmSize;

	/// <summary>Gets or sets the baseline origin of the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> value representing the baseline origin.</returns>
	public Point BaselineOrigin
	{
		get
		{
			CheckInitialized();
			return _baselineOrigin;
		}
		set
		{
			CheckInitializing();
			_baselineOrigin = value;
		}
	}

	/// <summary>Gets or sets the em size used for rendering the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the em size used for rendering.</returns>
	public double FontRenderingEmSize
	{
		get
		{
			CheckInitialized();
			return _renderingEmSize;
		}
		set
		{
			CheckInitializing();
			_renderingEmSize = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.GlyphTypeface" /> for the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.GlyphTypeface" /> for the <see cref="T:System.Windows.Media.GlyphRun" />.</returns>
	public GlyphTypeface GlyphTypeface
	{
		get
		{
			CheckInitialized();
			return _glyphTypeface;
		}
		set
		{
			CheckInitializing();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_glyphTypeface = value;
		}
	}

	/// <summary>Gets or sets the bidirectional nesting level of the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the bidirectional nesting level.</returns>
	public int BidiLevel
	{
		get
		{
			CheckInitialized();
			return _bidiLevel;
		}
		set
		{
			CheckInitializing();
			_bidiLevel = value;
		}
	}

	private bool IsLeftToRight => (_bidiLevel & 1) == 0;

	/// <summary>Gets or sets a value indicating whether to rotate glyphs.</summary>
	/// <returns>true if the glyphs are rotated; otherwise, false.</returns>
	public bool IsSideways
	{
		get
		{
			CheckInitialized();
			return (_flags & GlyphRunFlags.IsSideways) != 0;
		}
		set
		{
			CheckInitializing();
			if (value)
			{
				_flags |= GlyphRunFlags.IsSideways;
			}
			else
			{
				_flags &= ~GlyphRunFlags.IsSideways;
			}
		}
	}

	/// <summary>Gets or sets the list of <see cref="T:System.Boolean" /> values that determine whether there are caret stops for every UTF16 code point in the Unicode representing the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A list of <see cref="T:System.Boolean" /> values that represent whether there are caret stops.</returns>
	[CLSCompliant(false)]
	[TypeConverter(typeof(BoolIListConverter))]
	public IList<bool> CaretStops
	{
		get
		{
			CheckInitialized();
			return _caretStops;
		}
		set
		{
			CheckInitializing();
			_caretStops = value;
		}
	}

	/// <summary>Gets a value indicating whether there are any valid caret character hits within the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>true if there are valid caret character hits; otherwise, false.</returns>
	public bool IsHitTestable
	{
		get
		{
			CheckInitialized();
			if (CaretStops == null || CaretStops.Count == 0)
			{
				return true;
			}
			foreach (bool caretStop in CaretStops)
			{
				if (caretStop)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>Gets or sets the list of <see cref="T:System.UInt16" /> values that maps characters in the <see cref="T:System.Windows.Media.GlyphRun" /> to glyph indices.</summary>
	/// <returns>A list of <see cref="T:System.UInt16" /> values that represent mapped glyph indices.</returns>
	[CLSCompliant(false)]
	[TypeConverter(typeof(UShortIListConverter))]
	public IList<ushort> ClusterMap
	{
		get
		{
			CheckInitialized();
			return _clusterMap;
		}
		set
		{
			CheckInitializing();
			_clusterMap = value;
		}
	}

	/// <summary>Gets or sets the list of UTF16 code points that represent the Unicode content of the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A list of <see cref="T:System.Char" /> values that represent Unicode content.</returns>
	[CLSCompliant(false)]
	[TypeConverter(typeof(CharIListConverter))]
	public IList<char> Characters
	{
		get
		{
			CheckInitialized();
			return _characters;
		}
		set
		{
			CheckInitializing();
			_characters = value;
		}
	}

	/// <summary>Gets or sets an array of <see cref="T:System.UInt16" /> values that represent the glyph indices in the rendering physical font.</summary>
	/// <returns>A list of <see cref="T:System.UInt16" /> values that represent the glyph indices.</returns>
	[CLSCompliant(false)]
	[TypeConverter(typeof(UShortIListConverter))]
	public IList<ushort> GlyphIndices
	{
		get
		{
			CheckInitialized();
			return _glyphIndices;
		}
		set
		{
			CheckInitializing();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Count <= 0)
			{
				throw new ArgumentException(SR.CollectionNumberOfElementsMustBeGreaterThanZero, "value");
			}
			_glyphIndices = value;
		}
	}

	/// <summary>Gets or sets the list of <see cref="T:System.Double" /> values that represent the advance widths corresponding to the glyph indices.</summary>
	/// <returns>A list of <see cref="T:System.Double" /> values that represent the advance widths.</returns>
	[CLSCompliant(false)]
	[TypeConverter(typeof(DoubleIListConverter))]
	public IList<double> AdvanceWidths
	{
		get
		{
			CheckInitialized();
			return _advanceWidths;
		}
		set
		{
			CheckInitializing();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Count <= 0)
			{
				throw new ArgumentException(SR.CollectionNumberOfElementsMustBeGreaterThanZero, "value");
			}
			_advanceWidths = value;
		}
	}

	/// <summary>Gets or sets an array of <see cref="T:System.Windows.Point" /> values representing the offsets of the glyphs in the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A list of <see cref="T:System.Windows.Point" /> values representing the offsets of glyph.</returns>
	[CLSCompliant(false)]
	[TypeConverter(typeof(PointIListConverter))]
	public IList<Point> GlyphOffsets
	{
		get
		{
			CheckInitialized();
			return _glyphOffsets;
		}
		set
		{
			CheckInitializing();
			_glyphOffsets = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Markup.XmlLanguage" /> for the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Markup.XmlLanguage" /> for the <see cref="T:System.Windows.Media.GlyphRun" />.</returns>
	public XmlLanguage Language
	{
		get
		{
			CheckInitialized();
			return _language;
		}
		set
		{
			CheckInitializing();
			_language = value;
		}
	}

	/// <summary>Gets or sets the specific device font for which the <see cref="T:System.Windows.Media.GlyphRun" /> has been optimized.</summary>
	/// <returns>A <see cref="T:System.String" /> value that represents the device font.</returns>
	public string DeviceFontName
	{
		get
		{
			CheckInitialized();
			return _deviceFontName;
		}
		set
		{
			CheckInitializing();
			_deviceFontName = value;
		}
	}

	internal int GlyphCount => _glyphIndices.Count;

	internal int CodepointCount
	{
		get
		{
			if (_characters != null && _characters.Count != 0)
			{
				return _characters.Count;
			}
			if (_clusterMap != null && _clusterMap.Count != 0)
			{
				return _clusterMap.Count;
			}
			return _glyphIndices.Count;
		}
	}

	private bool IsInitializing
	{
		get
		{
			return (_flags & GlyphRunFlags.IsInitializing) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= GlyphRunFlags.IsInitializing;
			}
			else
			{
				_flags &= ~GlyphRunFlags.IsInitializing;
			}
		}
	}

	private bool IsInitialized
	{
		get
		{
			return (_flags & GlyphRunFlags.IsInitialized) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= GlyphRunFlags.IsInitialized;
			}
			else
			{
				_flags &= ~GlyphRunFlags.IsInitialized;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GlyphRun" /> class.</summary>
	[Obsolete("Use the PixelsPerDip override", false)]
	public GlyphRun()
	{
	}

	public GlyphRun(float pixelsPerDip)
	{
		_pixelsPerDip = pixelsPerDip;
	}

	[CLSCompliant(false)]
	public GlyphRun(GlyphTypeface glyphTypeface, int bidiLevel, bool isSideways, double renderingEmSize, float pixelsPerDip, IList<ushort> glyphIndices, Point baselineOrigin, IList<double> advanceWidths, IList<Point> glyphOffsets, IList<char> characters, string deviceFontName, IList<ushort> clusterMap, IList<bool> caretStops, XmlLanguage language)
	{
		Initialize(glyphTypeface, bidiLevel, isSideways, renderingEmSize, pixelsPerDip, glyphIndices, baselineOrigin, advanceWidths, glyphOffsets, characters, deviceFontName, clusterMap, caretStops, language, TextFormattingMode.Ideal);
		_flags |= GlyphRunFlags.CacheInkBounds;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GlyphRun" /> class by specifying properties of the class.</summary>
	/// <param name="glyphTypeface">A value of type <see cref="T:System.Windows.Media.GlyphTypeface" />.</param>
	/// <param name="bidiLevel">A value of type <see cref="T:System.Int32" />.</param>
	/// <param name="isSideways">A value of type <see cref="T:System.Boolean" />.</param>
	/// <param name="renderingEmSize">A value of type <see cref="T:System.Double" />.</param>
	/// <param name="glyphIndices">An array of type <see cref="T:System.Collections.Generic.IList`1" />.</param>
	/// <param name="baselineOrigin">A value of type <see cref="T:System.Windows.Point" />.</param>
	/// <param name="advanceWidths">An array of type <see cref="T:System.Collections.Generic.IList`1" />.</param>
	/// <param name="glyphOffsets">An array of type <see cref="T:System.Collections.Generic.IList`1" />.</param>
	/// <param name="characters">An array of type <see cref="T:System.Collections.Generic.IList`1" />.</param>
	/// <param name="deviceFontName">A value of type <see cref="T:System.String" />.</param>
	/// <param name="clusterMap">An array of type <see cref="T:System.Collections.Generic.IList`1" />.</param>
	/// <param name="caretStops">An array of type <see cref="T:System.Collections.Generic.IList`1" />.</param>
	/// <param name="language">A value of type <see cref="T:System.Windows.Markup.XmlLanguage" />.</param>
	[CLSCompliant(false)]
	[Obsolete("Use the PixelsPerDip override", false)]
	public GlyphRun(GlyphTypeface glyphTypeface, int bidiLevel, bool isSideways, double renderingEmSize, IList<ushort> glyphIndices, Point baselineOrigin, IList<double> advanceWidths, IList<Point> glyphOffsets, IList<char> characters, string deviceFontName, IList<ushort> clusterMap, IList<bool> caretStops, XmlLanguage language)
	{
		Initialize(glyphTypeface, bidiLevel, isSideways, renderingEmSize, Util.PixelsPerDip, glyphIndices, baselineOrigin, advanceWidths, glyphOffsets, characters, deviceFontName, clusterMap, caretStops, language, TextFormattingMode.Ideal);
		_flags |= GlyphRunFlags.CacheInkBounds;
	}

	internal static GlyphRun TryCreate(GlyphTypeface glyphTypeface, int bidiLevel, bool isSideways, double renderingEmSize, float pixelsPerDip, IList<ushort> glyphIndices, Point baselineOrigin, IList<double> advanceWidths, IList<Point> glyphOffsets, IList<char> characters, string deviceFontName, IList<ushort> clusterMap, IList<bool> caretStops, XmlLanguage language, TextFormattingMode textLayout)
	{
		GlyphRun glyphRun = new GlyphRun(pixelsPerDip);
		glyphRun.Initialize(glyphTypeface, bidiLevel, isSideways, renderingEmSize, pixelsPerDip, glyphIndices, baselineOrigin, advanceWidths, glyphOffsets, characters, deviceFontName, clusterMap, caretStops, language, textLayout);
		glyphRun._flags |= GlyphRunFlags.CacheInkBounds;
		if (glyphRun.IsInitialized)
		{
			return glyphRun;
		}
		return null;
	}

	private void Initialize(GlyphTypeface glyphTypeface, int bidiLevel, bool isSideways, double renderingEmSize, float pixelsPerDip, IList<ushort> glyphIndices, Point baselineOrigin, IList<double> advanceWidths, IList<Point> glyphOffsets, IList<char> characters, string deviceFontName, IList<ushort> clusterMap, IList<bool> caretStops, XmlLanguage language, TextFormattingMode textFormattingMode)
	{
		if (glyphTypeface != null && glyphIndices != null && advanceWidths != null && renderingEmSize >= 0.0 && glyphIndices.Count > 0 && glyphIndices.Count <= 65535 && advanceWidths.Count == glyphIndices.Count && (glyphOffsets == null || (glyphOffsets != null && glyphOffsets.Count != 0 && glyphOffsets.Count == glyphIndices.Count)))
		{
			_textFormattingMode = textFormattingMode;
			_glyphIndices = glyphIndices;
			_characters = characters;
			_clusterMap = clusterMap;
			_baselineOrigin = baselineOrigin;
			_renderingEmSize = renderingEmSize;
			_advanceWidths = advanceWidths;
			_glyphOffsets = glyphOffsets;
			_glyphTypeface = glyphTypeface;
			_flags = (isSideways ? GlyphRunFlags.IsSideways : GlyphRunFlags.None);
			_bidiLevel = bidiLevel;
			_caretStops = caretStops;
			_language = language;
			_deviceFontName = deviceFontName;
			_pixelsPerDip = pixelsPerDip;
			if (characters != null && characters.Count != 0)
			{
				if (clusterMap != null && clusterMap.Count != 0)
				{
					if (clusterMap.Count != characters.Count)
					{
						throw new ArgumentException(SR.Format(SR.CollectionNumberOfElementsShouldBeEqualTo, characters.Count), "clusterMap");
					}
					if (clusterMap[0] != 0)
					{
						throw new ArgumentException(SR.ClusterMapFirstEntryMustBeZero, "clusterMap");
					}
					int glyphCount = GlyphCount;
					int count = clusterMap.Count;
					ushort num = clusterMap[0];
					for (int i = 1; i < count; i++)
					{
						ushort num2 = clusterMap[i];
						if (num2 >= num && num2 < glyphCount)
						{
							num = num2;
							continue;
						}
						if (clusterMap[i] < clusterMap[i - 1])
						{
							throw new ArgumentException(SR.ClusterMapEntriesShouldNotDecrease, "clusterMap");
						}
						if (clusterMap[i] >= GlyphCount)
						{
							throw new ArgumentException(SR.ClusterMapEntryShouldPointWithinGlyphIndices, "clusterMap");
						}
					}
				}
				else if (GlyphCount != characters.Count)
				{
					throw new ArgumentException(SR.Format(SR.CollectionNumberOfElementsShouldBeEqualTo, GlyphCount), "clusterMap");
				}
			}
			if (caretStops != null && caretStops.Count != 0 && caretStops.Count != CodepointCount + 1)
			{
				throw new ArgumentException(SR.Format(SR.CollectionNumberOfElementsShouldBeEqualTo, CodepointCount + 1), "caretStops");
			}
			if (isSideways && (bidiLevel & 1) != 0)
			{
				throw new ArgumentException(SR.SidewaysRTLTextIsNotSupported);
			}
		}
		else
		{
			if (double.IsNaN(renderingEmSize))
			{
				throw new ArgumentOutOfRangeException("renderingEmSize", SR.ParameterValueCannotBeNaN);
			}
			if (renderingEmSize < 0.0)
			{
				throw new ArgumentOutOfRangeException("renderingEmSize", SR.ParameterValueCannotBeNegative);
			}
			if (glyphTypeface == null)
			{
				throw new ArgumentNullException("glyphTypeface");
			}
			if (glyphIndices == null)
			{
				throw new ArgumentNullException("glyphIndices");
			}
			if (glyphIndices.Count <= 0)
			{
				throw new ArgumentException(SR.CollectionNumberOfElementsMustBeGreaterThanZero, "glyphIndices");
			}
			if (glyphIndices.Count > 65535)
			{
				throw new ArgumentException(SR.Format(SR.CollectionNumberOfElementsMustBeLessOrEqualTo, 65535), "glyphIndices");
			}
			if (advanceWidths == null)
			{
				throw new ArgumentNullException("advanceWidths");
			}
			if (advanceWidths.Count != glyphIndices.Count)
			{
				throw new ArgumentException(SR.Format(SR.CollectionNumberOfElementsShouldBeEqualTo, glyphIndices.Count), "advanceWidths");
			}
			if (glyphOffsets != null && glyphOffsets.Count != 0 && glyphOffsets.Count != glyphIndices.Count)
			{
				throw new ArgumentException(SR.Format(SR.CollectionNumberOfElementsShouldBeEqualTo, glyphIndices.Count), "glyphOffsets");
			}
			Invariant.Assert(condition: false);
		}
		IsInitialized = true;
	}

	/// <summary>Retrieves the offset from the leading edge of the <see cref="T:System.Windows.Media.GlyphRun" /> to the leading or trailing edge of a caret stop containing the specified character hit.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the offset from the leading edge of the <see cref="T:System.Windows.Media.GlyphRun" /> to the leading or trailing edge of a caret stop containing the character hit.</returns>
	/// <param name="characterHit">The <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> to use for computing the offset.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The character hit is outside the range specified by the <see cref="T:System.Windows.Media.GlyphRun" /> Unicode string.</exception>
	public double GetDistanceFromCaretCharacterHit(CharacterHit characterHit)
	{
		CheckInitialized();
		IList<bool> list2;
		if (CaretStops == null || CaretStops.Count == 0)
		{
			IList<bool> list = new DefaultCaretStopList(CodepointCount);
			list2 = list;
		}
		else
		{
			list2 = CaretStops;
		}
		IList<bool> caretStops = list2;
		if (characterHit.FirstCharacterIndex < 0 || characterHit.FirstCharacterIndex > CodepointCount)
		{
			throw new ArgumentOutOfRangeException("characterHit");
		}
		FindNearestCaretStop(characterHit.FirstCharacterIndex, caretStops, out var caretStopIndex, out var codePointsUntilNextStop);
		if (caretStopIndex == -1)
		{
			return 0.0;
		}
		if (codePointsUntilNextStop == -1 && characterHit.TrailingLength != 0)
		{
			return 0.0;
		}
		int num = ((characterHit.TrailingLength == 0) ? caretStopIndex : (caretStopIndex + codePointsUntilNextStop));
		double num2 = 0.0;
		IList<ushort> list3 = ClusterMap;
		if (list3 == null)
		{
			list3 = new DefaultClusterMap(CodepointCount);
		}
		int num3 = 0;
		int num4 = num3;
		IList<double> advanceWidths = AdvanceWidths;
		double num5;
		while (true)
		{
			num4++;
			if (num4 >= list3.Count || list3[num4] != list3[num3])
			{
				num5 = 0.0;
				int num6 = ((num4 < list3.Count) ? list3[num4] : advanceWidths.Count);
				for (int i = list3[num3]; i < num6; i++)
				{
					num5 += advanceWidths[i];
				}
				if (num < num4 || num4 >= list3.Count)
				{
					break;
				}
				num2 += num5;
				num3 = num4;
			}
		}
		num5 *= (double)(num - num3) / (double)(num4 - num3);
		return num2 + num5;
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> value that represents the character hit of the caret of the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> value that represents the character hit that is closest to the <paramref name="distance" /> value. The out parameter <paramref name="isInside" /> returns true if the character hit is inside the <see cref="T:System.Windows.Media.GlyphRun" />; otherwise, false.</returns>
	/// <param name="distance">Offset to use for computing the caret character hit.</param>
	/// <param name="isInside">Determines whether the character hit is inside the <see cref="T:System.Windows.Media.GlyphRun" />.</param>
	public CharacterHit GetCaretCharacterHitFromDistance(double distance, out bool isInside)
	{
		CheckInitialized();
		IList<double> advanceWidths = AdvanceWidths;
		IList<bool> list2;
		if (CaretStops == null || CaretStops.Count == 0)
		{
			IList<bool> list = new DefaultCaretStopList(CodepointCount);
			list2 = list;
		}
		else
		{
			list2 = CaretStops;
		}
		IList<bool> list3 = list2;
		IList<ushort> list4 = ClusterMap;
		if (list4 == null)
		{
			list4 = new DefaultClusterMap(CodepointCount);
		}
		int num = -1;
		double num2 = 0.0;
		int num3 = -1;
		double num4 = 0.0;
		int num5 = 0;
		int num6 = 1;
		while (true)
		{
			int j;
			if (num6 < list3.Count)
			{
				if (num6 >= list4.Count || list4[num6] != list4[num5])
				{
					ushort num7 = ((num6 < list4.Count) ? list4[num6] : ((ushort)advanceWidths.Count));
					double num8 = 0.0;
					for (int i = list4[num5]; i < num7; i++)
					{
						num8 += advanceWidths[i];
					}
					num8 /= (double)(num6 - num5);
					for (j = num5; j < num6; num4 += num8, j++)
					{
						if (!list3[j])
						{
							continue;
						}
						if (num4 <= distance)
						{
							num = j;
							num2 = num4;
							continue;
						}
						goto IL_010b;
					}
					num5 = num6;
				}
				num6++;
				continue;
			}
			if (list3[list3.Count - 1] && num4 > distance)
			{
				num3 = list3.Count - 1;
			}
			break;
			IL_010b:
			num3 = j;
			break;
		}
		if (num == -1 && num3 == -1)
		{
			isInside = false;
			if (list3[list3.Count - 1])
			{
				return new CharacterHit(list3.Count - 1, 0);
			}
			return new CharacterHit(0, 0);
		}
		if (num == -1)
		{
			isInside = false;
			return new CharacterHit(num3, 0);
		}
		if (num3 == -1)
		{
			isInside = false;
			return new CharacterHit(num, list3.Count - 1 - num);
		}
		isInside = true;
		if (distance <= (num2 + num4) / 2.0)
		{
			return new CharacterHit(num, 0);
		}
		return new CharacterHit(num, num3 - num);
	}

	/// <summary>Retrieves the next valid caret character hit in the logical direction in the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> that represents the next valid caret character hit in the logical direction. If the return value is equal to <paramref name="characterHit" />, no further navigation is possible in the <see cref="T:System.Windows.Media.GlyphRun" />.</returns>
	/// <param name="characterHit">The <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> to use for computing the next hit value.</param>
	public CharacterHit GetNextCaretCharacterHit(CharacterHit characterHit)
	{
		CheckInitialized();
		IList<bool> list2;
		if (CaretStops == null || CaretStops.Count == 0)
		{
			IList<bool> list = new DefaultCaretStopList(CodepointCount);
			list2 = list;
		}
		else
		{
			list2 = CaretStops;
		}
		IList<bool> caretStops = list2;
		if (characterHit.FirstCharacterIndex < 0 || characterHit.FirstCharacterIndex > CodepointCount)
		{
			throw new ArgumentOutOfRangeException("characterHit");
		}
		FindNearestCaretStop(characterHit.FirstCharacterIndex, caretStops, out var caretStopIndex, out var codePointsUntilNextStop);
		if (caretStopIndex == -1 || codePointsUntilNextStop == -1)
		{
			return characterHit;
		}
		if (characterHit.TrailingLength == 0)
		{
			return new CharacterHit(caretStopIndex, codePointsUntilNextStop);
		}
		FindNearestCaretStop(caretStopIndex + codePointsUntilNextStop, caretStops, out var caretStopIndex2, out var codePointsUntilNextStop2);
		if (codePointsUntilNextStop2 == -1)
		{
			return characterHit;
		}
		return new CharacterHit(caretStopIndex2, codePointsUntilNextStop2);
	}

	/// <summary>Retrieves the previous valid caret character hit in the logical direction in the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> that represents the previous valid caret character hit in the logical direction. If the return value is equal to <paramref name="characterHit" />, no further navigation is possible in the <see cref="T:System.Windows.Media.GlyphRun" />.</returns>
	/// <param name="characterHit">The <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> to use for computing the previous hit value.</param>
	public CharacterHit GetPreviousCaretCharacterHit(CharacterHit characterHit)
	{
		CheckInitialized();
		IList<bool> list2;
		if (CaretStops == null || CaretStops.Count == 0)
		{
			IList<bool> list = new DefaultCaretStopList(CodepointCount);
			list2 = list;
		}
		else
		{
			list2 = CaretStops;
		}
		IList<bool> caretStops = list2;
		if (characterHit.FirstCharacterIndex < 0 || characterHit.FirstCharacterIndex > CodepointCount)
		{
			throw new ArgumentOutOfRangeException("characterHit");
		}
		FindNearestCaretStop(characterHit.FirstCharacterIndex, caretStops, out var caretStopIndex, out var codePointsUntilNextStop);
		if (caretStopIndex == -1)
		{
			return characterHit;
		}
		if (characterHit.TrailingLength != 0)
		{
			return new CharacterHit(caretStopIndex, 0);
		}
		FindNearestCaretStop(caretStopIndex - 1, caretStops, out var caretStopIndex2, out codePointsUntilNextStop);
		if (caretStopIndex2 == -1 || caretStopIndex2 == caretStopIndex)
		{
			return characterHit;
		}
		return new CharacterHit(caretStopIndex2, 0);
	}

	internal Point GetGlyphOffset(int i)
	{
		if (_glyphOffsets == null || _glyphOffsets.Count == 0)
		{
			return new Point(0.0, 0.0);
		}
		return _glyphOffsets[i];
	}

	/// <summary>Retrieves the ink bounding box for the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that represents the ink bounding box the <see cref="T:System.Windows.Media.GlyphRun" />.</returns>
	public Rect ComputeInkBoundingBox()
	{
		CheckInitialized();
		if ((_flags & GlyphRunFlags.CacheInkBounds) != 0 && _inkBoundingBox != null)
		{
			return (Rect)_inkBoundingBox;
		}
		int count = _glyphIndices.Count;
		ushort[] uShorts = BufferCache.GetUShorts(count);
		_glyphIndices.CopyTo(uShorts, 0);
		GlyphMetrics[] glyphMetrics = BufferCache.GetGlyphMetrics(count);
		_glyphTypeface.GetGlyphMetrics(uShorts, count, _renderingEmSize, _pixelsPerDip, _textFormattingMode, IsSideways, glyphMetrics);
		BufferCache.ReleaseUShorts(uShorts);
		uShorts = null;
		Rect rect;
		if (IsLeftToRight && !IsSideways)
		{
			rect = ComputeInkBoundingBoxLtoR(glyphMetrics);
		}
		else
		{
			double num = 0.0;
			double num2 = double.PositiveInfinity;
			double num3 = double.PositiveInfinity;
			double num4 = double.NegativeInfinity;
			double num5 = double.NegativeInfinity;
			double designToEm = _renderingEmSize / (double)(int)_glyphTypeface.DesignEmHeight;
			for (int i = 0; i < GlyphCount; i++)
			{
				EmGlyphMetrics emGlyphMetrics = new EmGlyphMetrics(glyphMetrics[i], designToEm, _pixelsPerDip, _textFormattingMode);
				if (TextFormattingMode.Display == _textFormattingMode)
				{
					emGlyphMetrics.AdvanceHeight = AdjustAdvanceForDisplayLayout(emGlyphMetrics.AdvanceHeight, emGlyphMetrics.TopSideBearing, emGlyphMetrics.BottomSideBearing);
					emGlyphMetrics.AdvanceWidth = AdjustAdvanceForDisplayLayout(emGlyphMetrics.AdvanceWidth, emGlyphMetrics.LeftSideBearing, emGlyphMetrics.RightSideBearing);
				}
				Point glyphOffset = GetGlyphOffset(i);
				double num6 = ((!IsLeftToRight) ? (0.0 - num - (emGlyphMetrics.AdvanceWidth + glyphOffset.X)) : (num + glyphOffset.X));
				num += _advanceWidths[i];
				double num7 = 0.0 - glyphOffset.Y;
				double num8;
				double num9;
				double num10;
				double num11;
				if (IsSideways)
				{
					num7 += emGlyphMetrics.AdvanceWidth / 2.0;
					num8 = num7 - emGlyphMetrics.LeftSideBearing;
					num9 = num7 - emGlyphMetrics.AdvanceWidth + emGlyphMetrics.RightSideBearing;
					num10 = num6 + emGlyphMetrics.TopSideBearing;
					num11 = num10 + emGlyphMetrics.AdvanceHeight - emGlyphMetrics.TopSideBearing - emGlyphMetrics.BottomSideBearing;
				}
				else
				{
					num10 = num6 + emGlyphMetrics.LeftSideBearing;
					num11 = num6 + emGlyphMetrics.AdvanceWidth - emGlyphMetrics.RightSideBearing;
					num8 = num7 + emGlyphMetrics.Baseline;
					num9 = num8 - emGlyphMetrics.AdvanceHeight + emGlyphMetrics.TopSideBearing + emGlyphMetrics.BottomSideBearing;
				}
				if (!(num10 + 1E-07 >= num11) && !(num9 + 1E-07 >= num8))
				{
					if (num2 > num10)
					{
						num2 = num10;
					}
					if (num3 > num9)
					{
						num3 = num9;
					}
					if (num4 < num11)
					{
						num4 = num11;
					}
					if (num5 < num8)
					{
						num5 = num8;
					}
				}
			}
			rect = ((!(num2 > num4)) ? new Rect(num2, num3, num4 - num2, num5 - num3) : Rect.Empty);
		}
		BufferCache.ReleaseGlyphMetrics(glyphMetrics);
		if (CoreCompatibilityPreferences.GetIncludeAllInkInBoundingBox())
		{
			if (!rect.IsEmpty)
			{
				double num12 = Math.Min(_renderingEmSize / 7.0, 1.0);
				rect.Inflate(num12, num12);
			}
		}
		else if (TextFormattingMode.Display == _textFormattingMode && !rect.IsEmpty)
		{
			rect.Inflate(1.0, 1.0);
		}
		if ((_flags & GlyphRunFlags.CacheInkBounds) != 0)
		{
			_inkBoundingBox = rect;
		}
		return rect;
	}

	private double AdjustAdvanceForDisplayLayout(double advance, double oneSideBearing, double otherSideBearing)
	{
		return Math.Max(advance, oneSideBearing + otherSideBearing + 1.0);
	}

	private Rect ComputeInkBoundingBoxLtoR(GlyphMetrics[] glyphMetrics)
	{
		double num = double.PositiveInfinity;
		double num2 = double.PositiveInfinity;
		double num3 = double.NegativeInfinity;
		double num4 = double.NegativeInfinity;
		double num5 = 0.0;
		double designToEm = _renderingEmSize / (double)(int)_glyphTypeface.DesignEmHeight;
		int glyphCount = GlyphCount;
		for (int i = 0; i < glyphCount; i++)
		{
			EmGlyphMetrics emGlyphMetrics = new EmGlyphMetrics(glyphMetrics[i], designToEm, _pixelsPerDip, _textFormattingMode);
			if (TextFormattingMode.Display == _textFormattingMode)
			{
				emGlyphMetrics.AdvanceHeight = AdjustAdvanceForDisplayLayout(emGlyphMetrics.AdvanceHeight, emGlyphMetrics.TopSideBearing, emGlyphMetrics.BottomSideBearing);
				emGlyphMetrics.AdvanceWidth = AdjustAdvanceForDisplayLayout(emGlyphMetrics.AdvanceWidth, emGlyphMetrics.LeftSideBearing, emGlyphMetrics.RightSideBearing);
			}
			if (GlyphOffsets != null)
			{
				Point glyphOffset = GetGlyphOffset(i);
				double num6 = num5 + glyphOffset.X;
				num5 += _advanceWidths[i];
				double num7 = 0.0 - glyphOffset.Y;
				double num8 = num6 + emGlyphMetrics.LeftSideBearing;
				double num9 = num6 + emGlyphMetrics.AdvanceWidth - emGlyphMetrics.RightSideBearing;
				double num10 = num7 + emGlyphMetrics.Baseline;
				double num11 = num10 - emGlyphMetrics.AdvanceHeight + emGlyphMetrics.TopSideBearing + emGlyphMetrics.BottomSideBearing;
				if (!(num8 + 1E-07 >= num9) && !(num11 + 1E-07 >= num10))
				{
					if (num > num8)
					{
						num = num8;
					}
					if (num2 > num11)
					{
						num2 = num11;
					}
					if (num3 < num9)
					{
						num3 = num9;
					}
					if (num4 < num10)
					{
						num4 = num10;
					}
				}
				continue;
			}
			double num12 = num5 + emGlyphMetrics.LeftSideBearing;
			double num13 = num5 + emGlyphMetrics.AdvanceWidth - emGlyphMetrics.RightSideBearing;
			double num14 = emGlyphMetrics.Baseline - emGlyphMetrics.AdvanceHeight + emGlyphMetrics.TopSideBearing + emGlyphMetrics.BottomSideBearing;
			num5 += _advanceWidths[i];
			if (!(num12 + 1E-07 >= num13) && !(num14 + 1E-07 >= emGlyphMetrics.Baseline))
			{
				if (num > num12)
				{
					num = num12;
				}
				if (num2 > num14)
				{
					num2 = num14;
				}
				if (num3 < num13)
				{
					num3 = num13;
				}
				if (num4 < emGlyphMetrics.Baseline)
				{
					num4 = emGlyphMetrics.Baseline;
				}
			}
		}
		if (num > num3)
		{
			return Rect.Empty;
		}
		return new Rect(num, num2, num3 - num, num4 - num2);
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Media.Geometry" /> for the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> corresponding to the <see cref="T:System.Windows.Media.GlyphRun" />.</returns>
	public Geometry BuildGeometry()
	{
		CheckInitialized();
		GeometryGroup geometryGroup = null;
		double num = 0.0;
		for (int i = 0; i < GlyphCount; i++)
		{
			ushort num2 = _glyphIndices[i];
			double num3;
			if (IsLeftToRight)
			{
				num3 = num;
				num3 += GetGlyphOffset(i).X;
			}
			else
			{
				double num4 = TextFormatterImp.RoundDip(_glyphTypeface.GetAdvanceWidth(num2, _pixelsPerDip, _textFormattingMode, IsSideways) * _renderingEmSize, _pixelsPerDip, _textFormattingMode);
				num3 = 0.0 - num;
				num3 -= num4 + GetGlyphOffset(i).X;
			}
			num += _advanceWidths[i];
			double num5 = 0.0 - GetGlyphOffset(i).Y;
			Geometry geometry = _glyphTypeface.ComputeGlyphOutline(num2, IsSideways, _renderingEmSize);
			if (!geometry.IsEmpty())
			{
				geometry.Transform = new TranslateTransform(num3 + _baselineOrigin.X, num5 + _baselineOrigin.Y);
				if (geometryGroup == null)
				{
					geometryGroup = new GeometryGroup();
					geometryGroup.FillRule = FillRule.Nonzero;
				}
				geometryGroup.Children.Add(geometry.GetOutlinedPathGeometry(RelativeFlatteningTolerance, ToleranceType.Relative));
			}
		}
		if (geometryGroup == null || geometryGroup.IsEmpty())
		{
			return Geometry.Empty;
		}
		return geometryGroup;
	}

	/// <summary>Retrieves the alignment box for the <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that represents the alignment box for the <see cref="T:System.Windows.Media.GlyphRun" />.</returns>
	public Rect ComputeAlignmentBox()
	{
		CheckInitialized();
		double num = AdvanceWidth;
		bool flag = IsLeftToRight;
		if (num < 0.0)
		{
			flag = !flag;
			num = 0.0 - num;
		}
		if (flag)
		{
			return new Rect(0.0, 0.0 - Ascent, num, Height);
		}
		return new Rect(0.0 - num, 0.0 - Ascent, num, Height);
	}

	internal void EmitBackground(DrawingContext dc, Brush backgroundBrush)
	{
		double advanceWidth;
		if (backgroundBrush != null && (advanceWidth = AdvanceWidth) > 0.0)
		{
			Rect rectangle = ((!IsLeftToRight) ? new Rect(_baselineOrigin.X - advanceWidth, _baselineOrigin.Y - Ascent, advanceWidth, Height) : new Rect(_baselineOrigin.X, _baselineOrigin.Y - Ascent, advanceWidth, Height));
			dc.DrawRectangle(backgroundBrush, null, rectangle);
		}
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		CheckInitialized();
		using (CompositionEngineLock.Acquire())
		{
			if (_mcr.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_GLYPHRUN))
			{
				CreateOnChannel(channel);
			}
			return _mcr.GetHandle(channel);
		}
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		CheckInitialized();
		using (CompositionEngineLock.Acquire())
		{
			_mcr.ReleaseOnChannel(channel);
		}
	}

	void DUCE.IResource.RemoveChildFromParent(DUCE.IResource parent, DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}

	DUCE.ResourceHandle DUCE.IResource.Get3DHandle(DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		CheckInitialized();
		return _mcr.GetHandle(channel);
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _mcr.GetChannelCount();
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _mcr.GetChannel(index);
	}

	private unsafe void CreateOnChannel(DUCE.Channel channel)
	{
		int glyphCount = GlyphCount;
		Rect managedBounds = ComputeInkBoundingBox();
		if (!managedBounds.IsEmpty)
		{
			managedBounds.Offset((Vector)BaselineOrigin);
		}
		DUCE.MILCMD_GLYPHRUN_CREATE mILCMD_GLYPHRUN_CREATE = default(DUCE.MILCMD_GLYPHRUN_CREATE);
		mILCMD_GLYPHRUN_CREATE.Type = MILCMD.MilCmdGlyphRunCreate;
		mILCMD_GLYPHRUN_CREATE.Handle = _mcr.GetHandle(channel);
		mILCMD_GLYPHRUN_CREATE.GlyphRunFlags = ComposeFlags();
		mILCMD_GLYPHRUN_CREATE.Origin.X = (float)_baselineOrigin.X;
		mILCMD_GLYPHRUN_CREATE.Origin.Y = (float)_baselineOrigin.Y;
		mILCMD_GLYPHRUN_CREATE.MuSize = (float)_renderingEmSize;
		mILCMD_GLYPHRUN_CREATE.ManagedBounds = managedBounds;
		checked
		{
			mILCMD_GLYPHRUN_CREATE.GlyphCount = (ushort)glyphCount;
			mILCMD_GLYPHRUN_CREATE.BidiLevel = (ushort)_bidiLevel;
		}
		mILCMD_GLYPHRUN_CREATE.pIDWriteFont = (ulong)_glyphTypeface.GetDWriteFontAddRef;
		mILCMD_GLYPHRUN_CREATE.DWriteTextMeasuringMethod = (ushort)DWriteTypeConverter.Convert(_textFormattingMode);
		int num = glyphCount * 2;
		num += glyphCount * 4;
		if (_glyphOffsets != null && _glyphOffsets.Count != 0)
		{
			num += glyphCount * 8;
		}
		channel.BeginCommand((byte*)(&mILCMD_GLYPHRUN_CREATE), sizeof(DUCE.MILCMD_GLYPHRUN_CREATE), num);
		if (glyphCount <= 512)
		{
			ushort* ptr = stackalloc ushort[glyphCount];
			for (int i = 0; i < glyphCount; i++)
			{
				ptr[i] = _glyphIndices[i];
			}
			channel.AppendCommandData((byte*)ptr, glyphCount * 2);
		}
		else
		{
			for (int j = 0; j < glyphCount; j++)
			{
				ushort num2 = _glyphIndices[j];
				channel.AppendCommandData((byte*)(&num2), 2);
			}
		}
		if (glyphCount <= 256)
		{
			float* ptr2 = stackalloc float[glyphCount];
			for (int k = 0; k < glyphCount; k++)
			{
				ptr2[k] = (float)_advanceWidths[k];
			}
			channel.AppendCommandData((byte*)ptr2, glyphCount * 4);
		}
		else
		{
			for (int l = 0; l < glyphCount; l++)
			{
				float num3 = (float)_advanceWidths[l];
				channel.AppendCommandData((byte*)(&num3), 4);
			}
		}
		if (_glyphOffsets != null && _glyphOffsets.Count != 0)
		{
			if (glyphCount <= 128)
			{
				float* ptr3 = stackalloc float[2 * glyphCount];
				for (int m = 0; m < glyphCount; m++)
				{
					ptr3[2 * m] = (float)_glyphOffsets[m].X;
					ptr3[2 * m + 1] = (float)_glyphOffsets[m].Y;
				}
				channel.AppendCommandData((byte*)ptr3, 2 * glyphCount * 4);
			}
			else
			{
				for (int n = 0; n < glyphCount; n++)
				{
					float num4 = (float)_glyphOffsets[n].X;
					float num5 = (float)_glyphOffsets[n].Y;
					channel.AppendCommandData((byte*)(&num4), 4);
					channel.AppendCommandData((byte*)(&num5), 4);
				}
			}
		}
		channel.EndCommand();
	}

	private ushort ComposeFlags()
	{
		ushort num = 0;
		if (IsSideways)
		{
			num |= 1;
		}
		if (_glyphOffsets != null && _glyphOffsets.Count != 0)
		{
			num |= 0x10;
		}
		return num;
	}

	private void FindNearestCaretStop(int characterIndex, IList<bool> caretStops, out int caretStopIndex, out int codePointsUntilNextStop)
	{
		caretStopIndex = -1;
		codePointsUntilNextStop = -1;
		if (characterIndex < 0 || characterIndex >= caretStops.Count)
		{
			return;
		}
		for (int num = characterIndex; num >= 0; num--)
		{
			if (caretStops[num])
			{
				caretStopIndex = num;
				break;
			}
		}
		if (caretStopIndex == -1)
		{
			for (int i = characterIndex + 1; i < caretStops.Count; i++)
			{
				if (caretStops[i])
				{
					caretStopIndex = i;
					break;
				}
			}
		}
		if (caretStopIndex == -1)
		{
			return;
		}
		for (int j = caretStopIndex + 1; j < caretStops.Count; j++)
		{
			if (caretStops[j])
			{
				codePointsUntilNextStop = j - caretStopIndex;
				break;
			}
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.ISupportInitialize.BeginInit" />.</summary>
	void ISupportInitialize.BeginInit()
	{
		if (IsInitialized)
		{
			throw new InvalidOperationException(SR.OnlyOneInitialization);
		}
		if (IsInitializing)
		{
			throw new InvalidOperationException(SR.InInitialization);
		}
		IsInitializing = true;
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.ISupportInitialize.EndInit" />.</summary>
	void ISupportInitialize.EndInit()
	{
		if (!IsInitializing)
		{
			throw new InvalidOperationException(SR.NotInInitialization);
		}
		GlyphTypeface glyphTypeface = _glyphTypeface;
		int bidiLevel = _bidiLevel;
		bool isSideways = (_flags & GlyphRunFlags.IsSideways) != 0;
		double renderingEmSize = _renderingEmSize;
		float pixelsPerDip = _pixelsPerDip;
		IList<ushort> glyphIndices = _glyphIndices;
		Point baselineOrigin = _baselineOrigin;
		object advanceWidths;
		if (_advanceWidths != null)
		{
			if (_textFormattingMode == TextFormattingMode.Ideal)
			{
				IList<double> list = new ThousandthOfEmRealDoubles(_renderingEmSize, _advanceWidths);
				advanceWidths = list;
			}
			else
			{
				IList<double> list = new List<double>();
				advanceWidths = list;
			}
		}
		else
		{
			advanceWidths = null;
		}
		object glyphOffsets;
		if (_glyphOffsets != null)
		{
			if (_textFormattingMode == TextFormattingMode.Ideal)
			{
				IList<Point> list2 = new ThousandthOfEmRealPoints(_renderingEmSize, _glyphOffsets);
				glyphOffsets = list2;
			}
			else
			{
				IList<Point> list2 = new List<Point>();
				glyphOffsets = list2;
			}
		}
		else
		{
			glyphOffsets = null;
		}
		Initialize(glyphTypeface, bidiLevel, isSideways, renderingEmSize, pixelsPerDip, glyphIndices, baselineOrigin, (IList<double>)advanceWidths, (IList<Point>)glyphOffsets, _characters, _deviceFontName, _clusterMap, _caretStops, _language, TextFormattingMode.Ideal);
		IsInitializing = false;
	}

	private void CheckInitialized()
	{
		if (!IsInitialized)
		{
			throw new InvalidOperationException(SR.InitializationIncomplete);
		}
	}

	private void CheckInitializing()
	{
		if (!IsInitializing)
		{
			throw new InvalidOperationException(SR.NotInInitialization);
		}
	}
}
