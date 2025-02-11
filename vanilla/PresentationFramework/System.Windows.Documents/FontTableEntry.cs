using System.Text;
using System.Windows.Media;
using MS.Internal.Text;

namespace System.Windows.Documents;

internal class FontTableEntry
{
	private string _name;

	private int _index;

	private int _codePage;

	private int _charSet;

	private bool _bNameSealed;

	private bool _bPending;

	internal int Index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
		}
	}

	internal string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal bool IsNameSealed
	{
		get
		{
			return _bNameSealed;
		}
		set
		{
			_bNameSealed = value;
		}
	}

	internal bool IsPending
	{
		get
		{
			return _bPending;
		}
		set
		{
			_bPending = value;
		}
	}

	internal int CodePage
	{
		get
		{
			return _codePage;
		}
		set
		{
			_codePage = value;
		}
	}

	internal int CodePageFromCharSet
	{
		set
		{
			int num = CharSetToCodePage(value);
			if (num != 0)
			{
				CodePage = num;
			}
		}
	}

	internal int CharSet
	{
		get
		{
			return _charSet;
		}
		set
		{
			_charSet = value;
		}
	}

	internal FontTableEntry()
	{
		_index = -1;
		_codePage = -1;
		_charSet = 0;
		_bNameSealed = false;
		_bPending = true;
	}

	internal static int CharSetToCodePage(int cs)
	{
		switch (cs)
		{
		case 0:
			return 1252;
		case 1:
			return -1;
		case 2:
			return 1252;
		case 3:
			return -1;
		case 77:
			return 10000;
		case 78:
		case 128:
			return 932;
		case 129:
			return 949;
		case 130:
			return 1361;
		case 134:
			return 936;
		case 136:
			return 950;
		case 161:
			return 1253;
		case 162:
			return 1254;
		case 163:
			return 1258;
		case 177:
			return 1255;
		case 178:
			return 1256;
		case 179:
			return 1256;
		case 180:
			return 1256;
		case 181:
			return 1255;
		case 186:
			return 1257;
		case 204:
			return 1251;
		case 222:
			return 874;
		case 238:
			return 1250;
		case 254:
			return 437;
		case 255:
			return 850;
		default:
			return 0;
		}
	}

	internal void ComputePreferredCodePage()
	{
		int[] array = new int[17]
		{
			1252, 932, 949, 1361, 936, 950, 1253, 1254, 1258, 1255,
			1256, 1257, 1251, 874, 1250, 437, 850
		};
		CodePage = 1252;
		CharSet = 0;
		if (Name == null || Name.Length <= 0)
		{
			return;
		}
		byte[] bytes = new byte[Name.Length * 6];
		char[] array2 = new char[Name.Length * 6];
		for (int i = 0; i < array.Length; i++)
		{
			Encoding encoding = InternalEncoding.GetEncoding(array[i]);
			int bytes2 = encoding.GetBytes(Name, 0, Name.Length, bytes, 0);
			int chars = encoding.GetChars(bytes, 0, bytes2, array2, 0);
			if (chars == Name.Length)
			{
				int num = 0;
				for (num = 0; num < chars && array2[num] == Name[num]; num++)
				{
				}
				if (num == chars)
				{
					CodePage = array[i];
					CharSet = CodePageToCharSet(CodePage);
					break;
				}
			}
		}
		if (IsSymbolFont(Name))
		{
			CharSet = 2;
		}
	}

	private static int CodePageToCharSet(int cp)
	{
		return cp switch
		{
			1252 => 0, 
			10000 => 77, 
			932 => 128, 
			949 => 129, 
			1361 => 130, 
			936 => 134, 
			950 => 136, 
			1253 => 161, 
			1254 => 162, 
			1258 => 163, 
			1255 => 177, 
			1256 => 178, 
			1257 => 186, 
			1251 => 204, 
			874 => 222, 
			1250 => 238, 
			437 => 254, 
			850 => 255, 
			_ => 0, 
		};
	}

	private static bool IsSymbolFont(string typefaceName)
	{
		bool result = false;
		Typeface typeface = new Typeface(typefaceName);
		if (typeface != null)
		{
			GlyphTypeface glyphTypeface = typeface.TryGetGlyphTypeface();
			if (glyphTypeface != null && glyphTypeface.Symbol)
			{
				result = true;
			}
		}
		return result;
	}
}
