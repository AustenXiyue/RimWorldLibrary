using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

[FriendAccessAllowed]
internal class GlyphsSerializer
{
	private GlyphTypeface _glyphTypeface;

	private IList<char> _characters;

	private double _milToEm;

	private bool _sideways;

	private int _glyphClusterInitialOffset;

	private double _currentAdvanceTotal;

	private double _idealAdvanceTotal;

	private IList<ushort> _clusters;

	private IList<ushort> _indices;

	private IList<double> _advances;

	private IList<Point> _offsets;

	private IList<bool> _caretStops;

	private StringBuilder _indicesStringBuider;

	private StringBuilder _glyphStringBuider;

	private const char GlyphSubEntrySeparator = ',';

	private const char GlyphSeparator = ';';

	private const double EmScaleFactor = 100.0;

	public GlyphsSerializer(GlyphRun glyphRun)
	{
		if (glyphRun == null)
		{
			throw new ArgumentNullException("glyphRun");
		}
		_glyphTypeface = glyphRun.GlyphTypeface;
		_milToEm = 100.0 / glyphRun.FontRenderingEmSize;
		_sideways = glyphRun.IsSideways;
		_characters = glyphRun.Characters;
		_caretStops = glyphRun.CaretStops;
		_clusters = glyphRun.ClusterMap;
		if (_clusters != null)
		{
			_glyphClusterInitialOffset = _clusters[0];
		}
		_indices = glyphRun.GlyphIndices;
		_advances = glyphRun.AdvanceWidths;
		_offsets = glyphRun.GlyphOffsets;
		_currentAdvanceTotal = 0.0;
		_idealAdvanceTotal = 0.0;
		_glyphStringBuider = new StringBuilder(10);
		_indicesStringBuider = new StringBuilder(Math.Max((_characters != null) ? _characters.Count : 0, _indices.Count) * _glyphStringBuider.Capacity);
	}

	public void ComputeContentStrings(out string characters, out string indices, out string caretStops)
	{
		_currentAdvanceTotal = 0.0;
		_idealAdvanceTotal = 0.0;
		if (_clusters != null)
		{
			int num = 0;
			int charClusterStart = 0;
			bool flag = true;
			int i;
			for (i = 0; i < _clusters.Count; i++)
			{
				if (flag)
				{
					num = _clusters[i];
					charClusterStart = i;
					flag = false;
				}
				else if (_clusters[i] != num)
				{
					AddCluster(num - _glyphClusterInitialOffset, _clusters[i] - _glyphClusterInitialOffset, charClusterStart, i);
					num = _clusters[i];
					charClusterStart = i;
				}
			}
			AddCluster(num - _glyphClusterInitialOffset, _indices.Count, charClusterStart, i);
		}
		else
		{
			for (int j = 0; j < _indices.Count; j++)
			{
				AddCluster(j, j + 1, j, j + 1);
			}
		}
		RemoveTrailingCharacters(_indicesStringBuider, ';');
		indices = _indicesStringBuider.ToString();
		if (_characters == null || _characters.Count == 0)
		{
			characters = string.Empty;
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder(_characters.Count);
			foreach (char character in _characters)
			{
				stringBuilder.Append(character);
			}
			characters = stringBuilder.ToString();
		}
		caretStops = CreateCaretStopsString();
	}

	private void RemoveTrailingCharacters(StringBuilder sb, char trailingCharacter)
	{
		int num = sb.Length - 1;
		while (num >= 0 && sb[num] == trailingCharacter)
		{
			num--;
		}
		sb.Length = num + 1;
	}

	private void AddGlyph(int glyph, int sourceCharacter)
	{
		ushort num = _indices[glyph];
		if (sourceCharacter == -1 || !_glyphTypeface.CharacterToGlyphMap.TryGetValue(sourceCharacter, out var value) || num != value)
		{
			_glyphStringBuider.Append(num.ToString(CultureInfo.InvariantCulture));
		}
		_glyphStringBuider.Append(',');
		double num2 = _advances[glyph] * _milToEm;
		double num3 = (_sideways ? _glyphTypeface.AdvanceHeights[num] : _glyphTypeface.AdvanceWidths[num]);
		int num4 = (int)Math.Round(_idealAdvanceTotal + num2 - _currentAdvanceTotal);
		int num5 = (int)Math.Round(num3);
		if (num4 != num5)
		{
			_glyphStringBuider.Append(num4.ToString(CultureInfo.InvariantCulture));
			_currentAdvanceTotal += num4;
			_idealAdvanceTotal += num2;
		}
		else
		{
			_currentAdvanceTotal += num3;
			_idealAdvanceTotal += num3;
		}
		_glyphStringBuider.Append(',');
		if (_offsets != null)
		{
			int num6 = (int)Math.Round(_offsets[glyph].X * _milToEm);
			if (num6 != 0)
			{
				_glyphStringBuider.Append(num6.ToString(CultureInfo.InvariantCulture));
			}
			_glyphStringBuider.Append(',');
			num6 = (int)Math.Round(_offsets[glyph].Y * _milToEm);
			if (num6 != 0)
			{
				_glyphStringBuider.Append(num6.ToString(CultureInfo.InvariantCulture));
			}
			_glyphStringBuider.Append(',');
		}
		RemoveTrailingCharacters(_glyphStringBuider, ',');
		_glyphStringBuider.Append(';');
		_indicesStringBuider.Append(_glyphStringBuider);
		_glyphStringBuider.Length = 0;
	}

	private void AddCluster(int glyphClusterStart, int glyphClusterEnd, int charClusterStart, int charClusterEnd)
	{
		int num = charClusterEnd - charClusterStart;
		int num2 = glyphClusterEnd - glyphClusterStart;
		int sourceCharacter = -1;
		if (num2 != 1)
		{
			_indicesStringBuider.AppendFormat(CultureInfo.InvariantCulture, "({0}:{1})", num, num2);
		}
		else if (num != 1)
		{
			_indicesStringBuider.AppendFormat(CultureInfo.InvariantCulture, "({0})", num);
		}
		else if (_characters != null && _characters.Count != 0)
		{
			sourceCharacter = _characters[charClusterStart];
		}
		for (int i = glyphClusterStart; i < glyphClusterEnd; i++)
		{
			AddGlyph(i, sourceCharacter);
		}
	}

	private string CreateCaretStopsString()
	{
		if (_caretStops == null)
		{
			return string.Empty;
		}
		int num = 0;
		int num2 = 0;
		for (int num3 = _caretStops.Count - 1; num3 >= 0; num3--)
		{
			if (!_caretStops[num3])
			{
				num = (num3 + 4) / 4;
				num2 = Math.Min(num3 | 3, _caretStops.Count - 1);
				break;
			}
		}
		if (num == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		byte b = 8;
		byte b2 = 0;
		for (int i = 0; i <= num2; i++)
		{
			if (_caretStops[i])
			{
				b2 |= b;
			}
			if (b != 1)
			{
				b >>= 1;
				continue;
			}
			stringBuilder.AppendFormat("{0:x1}", b2);
			b2 = 0;
			b = 8;
		}
		if (b != 8)
		{
			stringBuilder.AppendFormat("{0:x1}", b2);
		}
		return stringBuilder.ToString();
	}
}
