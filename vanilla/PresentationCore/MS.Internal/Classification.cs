using System.Runtime.InteropServices;
using System.Windows.Media.TextFormatting;

namespace MS.Internal;

internal static class Classification
{
	internal struct CombiningMarksClassificationData
	{
		internal nint CombiningCharsIndexes;

		internal int CombiningCharsIndexesTableLength;

		internal int CombiningCharsIndexesTableSegmentLength;

		internal nint CombiningMarkIndexes;

		internal int CombiningMarkIndexesTableLength;

		internal nint CombinationChars;

		internal int CombinationCharsBaseCount;

		internal int CombinationCharsMarkCount;
	}

	internal struct RawClassificationTables
	{
		internal nint UnicodeClasses;

		internal nint CharacterAttributes;

		internal nint Mirroring;

		internal CombiningMarksClassificationData CombiningMarksClassification;
	}

	private static readonly SecurityCriticalData<nint> _unicodeClassTable;

	private static readonly SecurityCriticalData<nint> _charAttributeTable;

	private static readonly SecurityCriticalData<nint> _mirroredCharTable;

	private static readonly SecurityCriticalData<CombiningMarksClassificationData> _combiningMarksClassification;

	private unsafe static short*** UnicodeClassTable => (short***)_unicodeClassTable.Value;

	private unsafe static CharacterAttribute* CharAttributeTable => (CharacterAttribute*)_charAttributeTable.Value;

	[DllImport("PresentationNative_cor3.dll")]
	internal static extern void MILGetClassificationTables(out RawClassificationTables ct);

	static Classification()
	{
		RawClassificationTables ct = default(RawClassificationTables);
		MILGetClassificationTables(out ct);
		_unicodeClassTable = new SecurityCriticalData<nint>(ct.UnicodeClasses);
		_charAttributeTable = new SecurityCriticalData<nint>(ct.CharacterAttributes);
		_mirroredCharTable = new SecurityCriticalData<nint>(ct.Mirroring);
		_combiningMarksClassification = new SecurityCriticalData<CombiningMarksClassificationData>(ct.CombiningMarksClassification);
	}

	public unsafe static short GetUnicodeClassUTF16(char codepoint)
	{
		short** unicodeClassTable = *UnicodeClassTable;
		Invariant.Assert((long)unicodeClassTable >= 472L);
		short* ptr = unicodeClassTable[(int)codepoint >> 8];
		if ((long)ptr >= 472L)
		{
			return ptr[codepoint & 0xFF];
		}
		return (short)ptr;
	}

	public unsafe static short GetUnicodeClass(int unicodeScalar)
	{
		Invariant.Assert(unicodeScalar >= 0 && unicodeScalar <= 1114111);
		short** ptr = UnicodeClassTable[((unicodeScalar >> 16) & 0xFF) % 17];
		if ((long)ptr < 472L)
		{
			return (short)ptr;
		}
		short* ptr2 = ptr[(unicodeScalar & 0xFFFF) >> 8];
		if ((long)ptr2 < 472L)
		{
			return (short)ptr2;
		}
		return ptr2[unicodeScalar & 0xFF];
	}

	public unsafe static ScriptID GetScript(int unicodeScalar)
	{
		return (ScriptID)CharAttributeTable[GetUnicodeClass(unicodeScalar)].Script;
	}

	internal static int UnicodeScalar(CharacterBufferRange unicodeString, out int sizeofChar)
	{
		Invariant.Assert(unicodeString.CharacterBuffer != null && unicodeString.Length > 0);
		int num = unicodeString[0];
		sizeofChar = 1;
		if (unicodeString.Length >= 2 && (num & 0xFC00) == 55296 && (unicodeString[1] & 0xFC00) == 56320)
		{
			num = (((num & 0x3FF) << 10) | (unicodeString[1] & 0x3FF)) + 65536;
			sizeofChar++;
		}
		return num;
	}

	public unsafe static bool IsCombining(int unicodeScalar)
	{
		byte itemClass = CharAttributeTable[GetUnicodeClass(unicodeScalar)].ItemClass;
		if (itemClass != 7 && itemClass != 8)
		{
			return IsIVS(unicodeScalar);
		}
		return true;
	}

	public unsafe static bool IsJoiner(int unicodeScalar)
	{
		return CharAttributeTable[GetUnicodeClass(unicodeScalar)].ItemClass == 10;
	}

	public static bool IsIVS(int unicodeScalar)
	{
		if (unicodeScalar >= 917760)
		{
			return unicodeScalar <= 917999;
		}
		return false;
	}

	public unsafe static int AdvanceUntilUTF16(CharacterBuffer charBuffer, int offsetToFirstChar, int stringLength, ushort mask, out ushort charFlags)
	{
		int i = offsetToFirstChar;
		int num = offsetToFirstChar + stringLength;
		charFlags = 0;
		for (; i < num; i++)
		{
			ushort flags = CharAttributeTable[GetUnicodeClassUTF16(charBuffer[i])].Flags;
			if ((flags & mask) != 0)
			{
				break;
			}
			charFlags |= flags;
		}
		return i - offsetToFirstChar;
	}

	public unsafe static int AdvanceWhile(CharacterBufferRange unicodeString, ItemClass itemClass)
	{
		int i = 0;
		int length = unicodeString.Length;
		for (int sizeofChar = 0; i < length; i += sizeofChar)
		{
			int unicodeScalar = UnicodeScalar(new CharacterBufferRange(unicodeString, i, length - i), out sizeofChar);
			if ((uint)CharAttributeTable[GetUnicodeClass(unicodeScalar)].ItemClass != (uint)itemClass)
			{
				break;
			}
		}
		return i;
	}

	internal unsafe static CharacterAttribute CharAttributeOf(int charClass)
	{
		Invariant.Assert(charClass >= 0 && charClass < 472);
		return CharAttributeTable[charClass];
	}
}
