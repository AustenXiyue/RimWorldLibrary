using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using MS.Internal.Utility;

namespace System.Windows.Documents;

/// <summary>Represents the set of glyphs that are used for rendering fixed text.</summary>
public sealed class Glyphs : FrameworkElement, IUriContext
{
	private class ParsedGlyphData
	{
		public ushort glyphIndex;

		public double advanceWidth;

		public double offsetX;

		public double offsetY;
	}

	private class LayoutDependentGlyphRunProperties
	{
		public double fontRenderingSize;

		public ushort[] glyphIndices;

		public double[] advanceWidths;

		public Point[] glyphOffsets;

		public ushort[] clusterMap;

		public bool sideways;

		public int bidiLevel;

		public GlyphTypeface glyphTypeface;

		public string unicodeString;

		public IList<bool> caretStops;

		public string deviceFontName;

		private float _pixelsPerDip;

		public LayoutDependentGlyphRunProperties(double pixelsPerDip)
		{
			_pixelsPerDip = (float)pixelsPerDip;
		}

		public GlyphRun CreateGlyphRun(Point origin, XmlLanguage language)
		{
			return new GlyphRun(glyphTypeface, bidiLevel, sideways, fontRenderingSize, _pixelsPerDip, glyphIndices, origin, advanceWidths, glyphOffsets, unicodeString.ToCharArray(), deviceFontName, clusterMap, caretStops, language);
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.Fill" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.Fill" /> dependency property. </returns>
	public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(Glyphs), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, FillChanged, null));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.Indices" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.Indices" /> dependency property. </returns>
	public static readonly DependencyProperty IndicesProperty = DependencyProperty.Register("Indices", typeof(string), typeof(Glyphs), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.UnicodeString" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.UnicodeString" /> dependency property. </returns>
	public static readonly DependencyProperty UnicodeStringProperty = DependencyProperty.Register("UnicodeString", typeof(string), typeof(Glyphs), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.CaretStops" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.CaretStops" /> dependency property. </returns>
	public static readonly DependencyProperty CaretStopsProperty = DependencyProperty.Register("CaretStops", typeof(string), typeof(Glyphs), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.FontRenderingEmSize" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.FontRenderingEmSize" /> dependency property. </returns>
	public static readonly DependencyProperty FontRenderingEmSizeProperty = DependencyProperty.Register("FontRenderingEmSize", typeof(double), typeof(Glyphs), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.OriginX" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.OriginX" /> dependency property. </returns>
	public static readonly DependencyProperty OriginXProperty = DependencyProperty.Register("OriginX", typeof(double), typeof(Glyphs), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OriginPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.OriginY" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.OriginY" /> dependency property. </returns>
	public static readonly DependencyProperty OriginYProperty = DependencyProperty.Register("OriginY", typeof(double), typeof(Glyphs), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OriginPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.FontUri" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.FontUri" /> dependency property. </returns>
	public static readonly DependencyProperty FontUriProperty = DependencyProperty.Register("FontUri", typeof(Uri), typeof(Glyphs), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.StyleSimulations" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.StyleSimulations" /> dependency property. </returns>
	public static readonly DependencyProperty StyleSimulationsProperty = DependencyProperty.Register("StyleSimulations", typeof(StyleSimulations), typeof(Glyphs), new FrameworkPropertyMetadata(StyleSimulations.None, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.IsSideways" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.IsSideways" /> dependency property. </returns>
	public static readonly DependencyProperty IsSidewaysProperty = DependencyProperty.Register("IsSideways", typeof(bool), typeof(Glyphs), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.BidiLevel" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.BidiLevel" /> dependency property. </returns>
	public static readonly DependencyProperty BidiLevelProperty = DependencyProperty.Register("BidiLevel", typeof(int), typeof(Glyphs), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Glyphs.DeviceFontName" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Glyphs.DeviceFontName" /> dependency property. </returns>
	public static readonly DependencyProperty DeviceFontNameProperty = DependencyProperty.Register("DeviceFontName", typeof(string), typeof(Glyphs), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, GlyphRunPropertyChanged));

	private LayoutDependentGlyphRunProperties _glyphRunProperties;

	private GlyphRun _measurementGlyphRun;

	private Point _glyphRunOrigin;

	private const double EmMultiplier = 100.0;

	/// <summary>For a description of this member, see <see cref="P:System.Windows.Markup.IUriContext.BaseUri" />.</summary>
	/// <returns>The base URI of the current context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return (Uri)GetValue(BaseUriHelper.BaseUriProperty);
		}
		set
		{
			SetValue(BaseUriHelper.BaseUriProperty, value);
		}
	}

	/// <summary>Gets the sets the <see cref="T:System.Windows.Media.Brush" /> that is used for the fill of the <see cref="T:System.Windows.Documents.Glyphs" /> class.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> that is used for the fill of the <see cref="T:System.Windows.Documents.Glyphs" /> class.</returns>
	public Brush Fill
	{
		get
		{
			return (Brush)GetValue(FillProperty);
		}
		set
		{
			SetValue(FillProperty, value);
		}
	}

	/// <summary>Gets or sets a collection of glyph specifications that represents the <see cref="T:System.Windows.Documents.Glyphs" /> object.</summary>
	/// <returns>A collection of glyph specifications that represents the <see cref="T:System.Windows.Documents.Glyphs" /> object.</returns>
	public string Indices
	{
		get
		{
			return (string)GetValue(IndicesProperty);
		}
		set
		{
			SetValue(IndicesProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.String" /> that represents the Unicode string for the <see cref="T:System.Windows.Documents.Glyphs" /> object.  </summary>
	/// <returns>A Unicode string for the <see cref="T:System.Windows.Documents.Glyphs" /> object.</returns>
	public string UnicodeString
	{
		get
		{
			return (string)GetValue(UnicodeStringProperty);
		}
		set
		{
			SetValue(UnicodeStringProperty, value);
		}
	}

	/// <summary>Gets or sets the caret stops that correspond to the code points in the Unicode string representing the <see cref="T:System.Windows.Documents.Glyphs" />.  </summary>
	/// <returns>A value of type <see cref="T:System.String" /> that represents whether the code points have caret stops.</returns>
	public string CaretStops
	{
		get
		{
			return (string)GetValue(CaretStopsProperty);
		}
		set
		{
			SetValue(CaretStopsProperty, value);
		}
	}

	/// <summary>Gets or sets the em size used for rendering the <see cref="T:System.Windows.Documents.Glyphs" /> class.  </summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the em size used for rendering.</returns>
	[TypeConverter("System.Windows.FontSizeConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	public double FontRenderingEmSize
	{
		get
		{
			return (double)GetValue(FontRenderingEmSizeProperty);
		}
		set
		{
			SetValue(FontRenderingEmSizeProperty, value);
		}
	}

	/// <summary>Gets or sets the value of the x origin for the <see cref="T:System.Windows.Documents.Glyphs" /> object.  </summary>
	/// <returns>The x origin for the <see cref="T:System.Windows.Documents.Glyphs" /> object.</returns>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	public double OriginX
	{
		get
		{
			return (double)GetValue(OriginXProperty);
		}
		set
		{
			SetValue(OriginXProperty, value);
		}
	}

	/// <summary>Gets or sets the value of the y origin for the <see cref="T:System.Windows.Documents.Glyphs" /> object.  </summary>
	/// <returns>The y origin for the <see cref="T:System.Windows.Documents.Glyphs" /> object.</returns>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	public double OriginY
	{
		get
		{
			return (double)GetValue(OriginYProperty);
		}
		set
		{
			SetValue(OriginYProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Uri" /> that represents the location of the font used for rendering the <see cref="T:System.Windows.Documents.Glyphs" /> class.  </summary>
	/// <returns>A <see cref="T:System.Uri" /> that represents the location of the font used for rendering the <see cref="T:System.Windows.Documents.Glyphs" /> class.</returns>
	public Uri FontUri
	{
		get
		{
			return (Uri)GetValue(FontUriProperty);
		}
		set
		{
			SetValue(FontUriProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.StyleSimulations" /> for the <see cref="T:System.Windows.Documents.Glyphs" /> class.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.StyleSimulations" /> for the <see cref="T:System.Windows.Documents.Glyphs" /> class.</returns>
	public StyleSimulations StyleSimulations
	{
		get
		{
			return (StyleSimulations)GetValue(StyleSimulationsProperty);
		}
		set
		{
			SetValue(StyleSimulationsProperty, value);
		}
	}

	/// <summary>Determines whether to rotate the <see cref="T:System.Windows.Documents.Glyphs" /> object.  </summary>
	/// <returns>true if the glyphs that make up the <see cref="T:System.Windows.Documents.Glyphs" /> object are rotated 90° counter-clockwise; otherwise, false.</returns>
	public bool IsSideways
	{
		get
		{
			return (bool)GetValue(IsSidewaysProperty);
		}
		set
		{
			SetValue(IsSidewaysProperty, value);
		}
	}

	/// <summary>Gets or sets the bidirectional nesting level of <see cref="T:System.Windows.Documents.Glyphs" />.  </summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the bidirectional nesting level.</returns>
	public int BidiLevel
	{
		get
		{
			return (int)GetValue(BidiLevelProperty);
		}
		set
		{
			SetValue(BidiLevelProperty, value);
		}
	}

	/// <summary>Gets or sets the specific device font for which the <see cref="T:System.Windows.Documents.Glyphs" /> object has been optimized.  </summary>
	/// <returns>A <see cref="T:System.String" /> value that represents the name of the device font.</returns>
	public string DeviceFontName
	{
		get
		{
			return (string)GetValue(DeviceFontNameProperty);
		}
		set
		{
			SetValue(DeviceFontNameProperty, value);
		}
	}

	internal GlyphRun MeasurementGlyphRun
	{
		get
		{
			if (_glyphRunProperties == null || _measurementGlyphRun == null)
			{
				ComputeMeasurementGlyphRunAndOrigin();
			}
			return _measurementGlyphRun;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Glyphs" /> class.</summary>
	public Glyphs()
	{
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.GlyphRun" /> from the properties of the <see cref="T:System.Windows.Documents.Glyphs" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.GlyphRun" /> that was created using the properties of the <see cref="T:System.Windows.Documents.Glyphs" /> object.</returns>
	public GlyphRun ToGlyphRun()
	{
		ComputeMeasurementGlyphRunAndOrigin();
		if (_measurementGlyphRun == null)
		{
			return null;
		}
		return _measurementGlyphRun;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		base.ArrangeOverride(finalSize);
		if (_measurementGlyphRun != null)
		{
			_measurementGlyphRun.ComputeInkBoundingBox();
		}
		return finalSize;
	}

	protected override void OnRender(DrawingContext context)
	{
		if (_glyphRunProperties == null || _measurementGlyphRun == null)
		{
			return;
		}
		context.PushGuidelineY1(_glyphRunOrigin.Y);
		try
		{
			context.DrawGlyphRun(Fill, _measurementGlyphRun);
		}
		finally
		{
			context.Pop();
		}
	}

	protected override Size MeasureOverride(Size constraint)
	{
		ComputeMeasurementGlyphRunAndOrigin();
		if (_measurementGlyphRun == null)
		{
			return default(Size);
		}
		Rect rect = _measurementGlyphRun.ComputeAlignmentBox();
		rect.Offset(_glyphRunOrigin.X, _glyphRunOrigin.Y);
		return new Size(Math.Max(0.0, rect.Right), Math.Max(0.0, rect.Bottom));
	}

	private void ComputeMeasurementGlyphRunAndOrigin()
	{
		if (_glyphRunProperties == null)
		{
			_measurementGlyphRun = null;
			ParseGlyphRunProperties();
			if (_glyphRunProperties == null)
			{
				return;
			}
		}
		else if (_measurementGlyphRun != null)
		{
			return;
		}
		bool flag = (BidiLevel & 1) == 0;
		bool num = !double.IsNaN(OriginX);
		bool flag2 = !double.IsNaN(OriginY);
		bool flag3 = false;
		Rect rect = default(Rect);
		if (num && flag2 && flag)
		{
			_measurementGlyphRun = _glyphRunProperties.CreateGlyphRun(new Point(OriginX, OriginY), base.Language);
			flag3 = true;
		}
		else
		{
			_measurementGlyphRun = _glyphRunProperties.CreateGlyphRun(default(Point), base.Language);
			rect = _measurementGlyphRun.ComputeAlignmentBox();
		}
		if (num)
		{
			_glyphRunOrigin.X = OriginX;
		}
		else
		{
			_glyphRunOrigin.X = (flag ? 0.0 : rect.Width);
		}
		if (flag2)
		{
			_glyphRunOrigin.Y = OriginY;
		}
		else
		{
			_glyphRunOrigin.Y = 0.0 - rect.Y;
		}
		if (!flag3)
		{
			_measurementGlyphRun = _glyphRunProperties.CreateGlyphRun(_glyphRunOrigin, base.Language);
		}
	}

	private void ParseCaretStops(LayoutDependentGlyphRunProperties glyphRunProperties)
	{
		string caretStops = CaretStops;
		if (string.IsNullOrEmpty(caretStops))
		{
			glyphRunProperties.caretStops = null;
			return;
		}
		int num = ((!string.IsNullOrEmpty(glyphRunProperties.unicodeString)) ? (glyphRunProperties.unicodeString.Length + 1) : ((glyphRunProperties.clusterMap == null || glyphRunProperties.clusterMap.Length == 0) ? (glyphRunProperties.glyphIndices.Length + 1) : (glyphRunProperties.clusterMap.Length + 1)));
		bool[] array = new bool[num];
		int num2 = 0;
		string text = caretStops;
		foreach (char c in text)
		{
			if (char.IsWhiteSpace(c))
			{
				continue;
			}
			int num3;
			if ('0' <= c && c <= '9')
			{
				num3 = c - 48;
			}
			else if ('a' <= c && c <= 'f')
			{
				num3 = c - 97 + 10;
			}
			else
			{
				if ('A' > c || c > 'F')
				{
					throw new ArgumentException(SR.GlyphsCaretStopsContainsHexDigits, "CaretStops");
				}
				num3 = c - 65 + 10;
			}
			if ((num3 & 8) != 0)
			{
				if (num2 >= array.Length)
				{
					throw new ArgumentException(SR.GlyphsCaretStopsLengthCorrespondsToUnicodeString, "CaretStops");
				}
				array[num2] = true;
			}
			num2++;
			if ((num3 & 4) != 0)
			{
				if (num2 >= array.Length)
				{
					throw new ArgumentException(SR.GlyphsCaretStopsLengthCorrespondsToUnicodeString, "CaretStops");
				}
				array[num2] = true;
			}
			num2++;
			if ((num3 & 2) != 0)
			{
				if (num2 >= array.Length)
				{
					throw new ArgumentException(SR.GlyphsCaretStopsLengthCorrespondsToUnicodeString, "CaretStops");
				}
				array[num2] = true;
			}
			num2++;
			if ((num3 & 1) != 0)
			{
				if (num2 >= array.Length)
				{
					throw new ArgumentException(SR.GlyphsCaretStopsLengthCorrespondsToUnicodeString, "CaretStops");
				}
				array[num2] = true;
			}
			num2++;
		}
		while (num2 < array.Length)
		{
			array[num2++] = true;
		}
		glyphRunProperties.caretStops = array;
	}

	private void ParseGlyphRunProperties()
	{
		LayoutDependentGlyphRunProperties layoutDependentGlyphRunProperties = null;
		Uri uri = FontUri;
		if (uri != null)
		{
			if (string.IsNullOrEmpty(UnicodeString) && string.IsNullOrEmpty(Indices))
			{
				throw new ArgumentException(SR.GlyphsUnicodeStringAndIndicesCannotBothBeEmpty);
			}
			layoutDependentGlyphRunProperties = new LayoutDependentGlyphRunProperties(GetDpi().PixelsPerDip);
			if (!uri.IsAbsoluteUri)
			{
				uri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(BaseUriHelper.GetBaseUri(this), uri);
			}
			layoutDependentGlyphRunProperties.glyphTypeface = new GlyphTypeface(uri, StyleSimulations);
			layoutDependentGlyphRunProperties.unicodeString = UnicodeString;
			layoutDependentGlyphRunProperties.sideways = IsSideways;
			layoutDependentGlyphRunProperties.deviceFontName = DeviceFontName;
			List<ParsedGlyphData> parsedGlyphs;
			int num = ParseGlyphsProperty(layoutDependentGlyphRunProperties.glyphTypeface, layoutDependentGlyphRunProperties.unicodeString, layoutDependentGlyphRunProperties.sideways, out parsedGlyphs, out layoutDependentGlyphRunProperties.clusterMap);
			layoutDependentGlyphRunProperties.glyphIndices = new ushort[num];
			layoutDependentGlyphRunProperties.advanceWidths = new double[num];
			ParseCaretStops(layoutDependentGlyphRunProperties);
			layoutDependentGlyphRunProperties.glyphOffsets = null;
			int num2 = 0;
			layoutDependentGlyphRunProperties.fontRenderingSize = FontRenderingEmSize;
			layoutDependentGlyphRunProperties.bidiLevel = BidiLevel;
			double num3 = layoutDependentGlyphRunProperties.fontRenderingSize / 100.0;
			foreach (ParsedGlyphData item in parsedGlyphs)
			{
				layoutDependentGlyphRunProperties.glyphIndices[num2] = item.glyphIndex;
				layoutDependentGlyphRunProperties.advanceWidths[num2] = item.advanceWidth * num3;
				if (item.offsetX != 0.0 || item.offsetY != 0.0)
				{
					if (layoutDependentGlyphRunProperties.glyphOffsets == null)
					{
						layoutDependentGlyphRunProperties.glyphOffsets = new Point[num];
					}
					layoutDependentGlyphRunProperties.glyphOffsets[num2].X = item.offsetX * num3;
					layoutDependentGlyphRunProperties.glyphOffsets[num2].Y = item.offsetY * num3;
				}
				num2++;
			}
		}
		_glyphRunProperties = layoutDependentGlyphRunProperties;
	}

	private static bool IsEmpty(ReadOnlySpan<char> s)
	{
		ReadOnlySpan<char> readOnlySpan = s;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (!char.IsWhiteSpace(readOnlySpan[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool ReadGlyphIndex(ReadOnlySpan<char> valueSpec, ref bool inCluster, ref int glyphClusterSize, ref int characterClusterSize, ref ushort glyphIndex)
	{
		ReadOnlySpan<char> s = valueSpec;
		int num = valueSpec.IndexOf('(');
		if (num != -1)
		{
			for (int i = 0; i < num; i++)
			{
				if (!char.IsWhiteSpace(valueSpec[i]))
				{
					throw new ArgumentException(SR.GlyphsClusterBadCharactersBeforeBracket);
				}
			}
			if (inCluster)
			{
				throw new ArgumentException(SR.GlyphsClusterNoNestedClusters);
			}
			int num2 = valueSpec.IndexOf(')');
			if (num2 == -1 || num2 <= num + 1)
			{
				throw new ArgumentException(SR.GlyphsClusterNoMatchingBracket);
			}
			int num3 = valueSpec.IndexOf(':');
			if (num3 == -1)
			{
				ReadOnlySpan<char> s2 = valueSpec.Slice(num + 1, num2 - (num + 1));
				characterClusterSize = int.Parse(s2, CultureInfo.InvariantCulture);
				glyphClusterSize = 1;
			}
			else
			{
				if (num3 <= num + 1 || num3 >= num2 - 1)
				{
					throw new ArgumentException(SR.GlyphsClusterMisplacedSeparator);
				}
				ReadOnlySpan<char> s3 = valueSpec.Slice(num + 1, num3 - (num + 1));
				characterClusterSize = int.Parse(s3, CultureInfo.InvariantCulture);
				ReadOnlySpan<char> s4 = valueSpec.Slice(num3 + 1, num2 - (num3 + 1));
				glyphClusterSize = int.Parse(s4, CultureInfo.InvariantCulture);
			}
			inCluster = true;
			s = valueSpec.Slice(num2 + 1);
		}
		if (IsEmpty(s))
		{
			return false;
		}
		glyphIndex = ushort.Parse(s, CultureInfo.InvariantCulture);
		return true;
	}

	private static double GetAdvanceWidth(GlyphTypeface glyphTypeface, ushort glyphIndex, bool sideways)
	{
		return (sideways ? glyphTypeface.AdvanceHeights[glyphIndex] : glyphTypeface.AdvanceWidths[glyphIndex]) * 100.0;
	}

	private ushort GetGlyphFromCharacter(GlyphTypeface glyphTypeface, char character)
	{
		glyphTypeface.CharacterToGlyphMap.TryGetValue(character, out var value);
		return value;
	}

	private static void SetClusterMapEntry(ushort[] clusterMap, int index, ushort value)
	{
		if (index < 0 || index >= clusterMap.Length)
		{
			throw new ArgumentException(SR.GlyphsUnicodeStringIsTooShort);
		}
		clusterMap[index] = value;
	}

	private int ParseGlyphsProperty(GlyphTypeface fontFace, string unicodeString, bool sideways, out List<ParsedGlyphData> parsedGlyphs, out ushort[] clusterMap)
	{
		string indices = Indices;
		int num = 0;
		int num2 = 0;
		int characterClusterSize = 1;
		int glyphClusterSize = 1;
		bool inCluster = false;
		int num3;
		if (!string.IsNullOrEmpty(unicodeString))
		{
			clusterMap = new ushort[unicodeString.Length];
			num3 = unicodeString.Length;
		}
		else
		{
			clusterMap = null;
			num3 = 8;
		}
		if (!string.IsNullOrEmpty(indices))
		{
			num3 = Math.Max(num3, indices.Length / 5);
		}
		parsedGlyphs = new List<ParsedGlyphData>(num3);
		ParsedGlyphData parsedGlyphData = new ParsedGlyphData();
		if (!string.IsNullOrEmpty(indices))
		{
			int num4 = 0;
			int num5 = 0;
			for (int i = 0; i <= indices.Length; i++)
			{
				char c = ((i < indices.Length) ? indices[i] : '\0');
				if (c == ',' || c == ';' || i == indices.Length)
				{
					int length = i - num5;
					ReadOnlySpan<char> readOnlySpan = indices.AsSpan(num5, length);
					switch (num4)
					{
					case 0:
					{
						bool num6 = inCluster;
						if (!ReadGlyphIndex(readOnlySpan, ref inCluster, ref glyphClusterSize, ref characterClusterSize, ref parsedGlyphData.glyphIndex))
						{
							if (string.IsNullOrEmpty(unicodeString))
							{
								throw new ArgumentException(SR.GlyphsIndexRequiredIfNoUnicode);
							}
							if (unicodeString.Length <= num2)
							{
								throw new ArgumentException(SR.GlyphsUnicodeStringIsTooShort);
							}
							parsedGlyphData.glyphIndex = GetGlyphFromCharacter(fontFace, unicodeString[num2]);
						}
						if (!num6 && clusterMap != null)
						{
							if (inCluster)
							{
								for (int j = num2; j < num2 + characterClusterSize; j++)
								{
									SetClusterMapEntry(clusterMap, j, (ushort)num);
								}
							}
							else
							{
								SetClusterMapEntry(clusterMap, num2, (ushort)num);
							}
						}
						parsedGlyphData.advanceWidth = GetAdvanceWidth(fontFace, parsedGlyphData.glyphIndex, sideways);
						break;
					}
					case 1:
						if (!IsEmpty(readOnlySpan))
						{
							parsedGlyphData.advanceWidth = double.Parse(readOnlySpan, CultureInfo.InvariantCulture);
							if (parsedGlyphData.advanceWidth < 0.0)
							{
								throw new ArgumentException(SR.GlyphsAdvanceWidthCannotBeNegative);
							}
						}
						break;
					case 2:
						if (!IsEmpty(readOnlySpan))
						{
							parsedGlyphData.offsetX = double.Parse(readOnlySpan, CultureInfo.InvariantCulture);
						}
						break;
					case 3:
						if (!IsEmpty(readOnlySpan))
						{
							parsedGlyphData.offsetY = double.Parse(readOnlySpan, CultureInfo.InvariantCulture);
						}
						break;
					default:
						throw new ArgumentException(SR.GlyphsTooManyCommas);
					}
					num4++;
					num5 = i + 1;
				}
				if (c != ';' && i != indices.Length)
				{
					continue;
				}
				parsedGlyphs.Add(parsedGlyphData);
				parsedGlyphData = new ParsedGlyphData();
				if (inCluster)
				{
					glyphClusterSize--;
					if (glyphClusterSize == 0)
					{
						num2 += characterClusterSize;
						inCluster = false;
					}
				}
				else
				{
					num2++;
				}
				num++;
				num4 = 0;
				num5 = i + 1;
			}
		}
		if (unicodeString != null)
		{
			while (num2 < unicodeString.Length)
			{
				if (inCluster)
				{
					throw new ArgumentException(SR.GlyphsIndexRequiredWithinCluster);
				}
				if (unicodeString.Length <= num2)
				{
					throw new ArgumentException(SR.GlyphsUnicodeStringIsTooShort);
				}
				parsedGlyphData.glyphIndex = GetGlyphFromCharacter(fontFace, unicodeString[num2]);
				parsedGlyphData.advanceWidth = GetAdvanceWidth(fontFace, parsedGlyphData.glyphIndex, sideways);
				parsedGlyphs.Add(parsedGlyphData);
				parsedGlyphData = new ParsedGlyphData();
				SetClusterMapEntry(clusterMap, num2, (ushort)num);
				num2++;
				num++;
			}
		}
		return num;
	}

	private static void FillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((UIElement)d).InvalidateVisual();
	}

	private static void GlyphRunPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Glyphs)d)._glyphRunProperties = null;
	}

	private static void OriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Glyphs)d)._measurementGlyphRun = null;
	}
}
