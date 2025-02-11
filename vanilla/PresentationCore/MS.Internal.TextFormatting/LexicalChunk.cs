using System;
using System.Windows.Media.TextFormatting;
using MS.Internal.Generic;

namespace MS.Internal.TextFormatting;

internal struct LexicalChunk
{
	private TextLexicalBreaks _breaks;

	private SpanVector<int> _ichVector;

	internal TextLexicalBreaks Breaks => _breaks;

	internal bool IsNoBreak => _breaks == null;

	internal LexicalChunk(TextLexicalBreaks breaks, SpanVector<int> ichVector)
	{
		Invariant.Assert(breaks != null);
		_breaks = breaks;
		_ichVector = ichVector;
	}

	internal int LSCPToCharacterIndex(int lsdcp)
	{
		if (_ichVector.Count > 0)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < _ichVector.Count; i++)
			{
				MS.Internal.Generic.Span<int> span = _ichVector[i];
				int value = span.Value;
				if (value > lsdcp)
				{
					return num - num2 + Math.Min(num2, lsdcp - num3);
				}
				num += span.Length;
				num2 = span.Length;
				num3 = value;
			}
			return num - num2 + Math.Min(num2, lsdcp - num3);
		}
		return lsdcp;
	}

	internal int CharacterIndexToLSCP(int ich)
	{
		if (_ichVector.Count > 0)
		{
			SpanRider<int> spanRider = new SpanRider<int>(_ichVector);
			spanRider.At(ich);
			return spanRider.CurrentValue + ich - spanRider.CurrentSpanStart;
		}
		return ich;
	}
}
