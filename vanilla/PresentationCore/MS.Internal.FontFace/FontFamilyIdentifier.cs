using System;
using MS.Internal.FontCache;

namespace MS.Internal.FontFace;

internal struct FontFamilyIdentifier
{
	private sealed class BasedFriendlyName : BasedName
	{
		public BasedFriendlyName(Uri baseUri, string name)
			: base(baseUri, name)
		{
		}

		public override int GetHashCode()
		{
			return InternalGetHashCode(1);
		}

		public override bool Equals(object obj)
		{
			return InternalEquals(obj as BasedFriendlyName);
		}
	}

	private sealed class BasedNormalizedName : BasedName
	{
		public BasedNormalizedName(Uri baseUri, string name)
			: base(baseUri, name)
		{
		}

		public override int GetHashCode()
		{
			return InternalGetHashCode(int.MaxValue);
		}

		public override bool Equals(object obj)
		{
			return InternalEquals(obj as BasedNormalizedName);
		}
	}

	private abstract class BasedName
	{
		private Uri _baseUri;

		private string _name;

		protected BasedName(Uri baseUri, string name)
		{
			_baseUri = baseUri;
			_name = name;
		}

		public abstract override int GetHashCode();

		public abstract override bool Equals(object obj);

		protected int InternalGetHashCode(int seed)
		{
			int num = seed;
			if (_baseUri != null)
			{
				num += HashFn.HashMultiply(_baseUri.GetHashCode());
			}
			if (_name != null)
			{
				num = HashFn.HashMultiply(num) + _name.GetHashCode();
			}
			return HashFn.HashScramble(num);
		}

		protected bool InternalEquals(BasedName other)
		{
			if (other != null && other._baseUri == _baseUri)
			{
				return other._name == _name;
			}
			return false;
		}
	}

	private string _friendlyName;

	private Uri _baseUri;

	private int _tokenCount;

	private CanonicalFontFamilyReference[] _canonicalReferences;

	internal const char FamilyNameDelimiter = ',';

	internal const int MaxFamilyNamePerFamilyMapTarget = 32;

	internal string Source => _friendlyName;

	internal Uri BaseUri => _baseUri;

	internal int Count
	{
		get
		{
			if (_tokenCount < 0)
			{
				_tokenCount = CountTokens(_friendlyName);
			}
			return _tokenCount;
		}
	}

	internal CanonicalFontFamilyReference this[int tokenIndex]
	{
		get
		{
			if (tokenIndex < 0 || tokenIndex >= Count)
			{
				throw new ArgumentOutOfRangeException("tokenIndex");
			}
			if (_canonicalReferences != null)
			{
				return _canonicalReferences[tokenIndex];
			}
			int tokenIndex2;
			int tokenLength;
			int i = FindToken(_friendlyName, 0, out tokenIndex2, out tokenLength);
			for (int j = 0; j < tokenIndex; j++)
			{
				i = FindToken(_friendlyName, i, out tokenIndex2, out tokenLength);
			}
			return GetCanonicalReference(tokenIndex2, tokenLength);
		}
	}

	internal FontFamilyIdentifier(string friendlyName, Uri baseUri)
	{
		_friendlyName = friendlyName;
		_baseUri = baseUri;
		_tokenCount = ((friendlyName != null) ? (-1) : 0);
		_canonicalReferences = null;
	}

	internal FontFamilyIdentifier(FontFamilyIdentifier first, FontFamilyIdentifier second)
	{
		first.Canonicalize();
		second.Canonicalize();
		_friendlyName = null;
		_tokenCount = first._tokenCount + second._tokenCount;
		_baseUri = null;
		if (first._tokenCount == 0)
		{
			_canonicalReferences = second._canonicalReferences;
			return;
		}
		if (second._tokenCount == 0)
		{
			_canonicalReferences = first._canonicalReferences;
			return;
		}
		_canonicalReferences = new CanonicalFontFamilyReference[_tokenCount];
		int num = 0;
		CanonicalFontFamilyReference[] canonicalReferences = first._canonicalReferences;
		foreach (CanonicalFontFamilyReference canonicalFontFamilyReference in canonicalReferences)
		{
			_canonicalReferences[num++] = canonicalFontFamilyReference;
		}
		canonicalReferences = second._canonicalReferences;
		foreach (CanonicalFontFamilyReference canonicalFontFamilyReference2 in canonicalReferences)
		{
			_canonicalReferences[num++] = canonicalFontFamilyReference2;
		}
	}

	public bool Equals(FontFamilyIdentifier other)
	{
		if (_friendlyName == other._friendlyName && _baseUri == other._baseUri)
		{
			return true;
		}
		int count = Count;
		if (other.Count != count)
		{
			return false;
		}
		if (count != 0)
		{
			Canonicalize();
			other.Canonicalize();
			for (int i = 0; i < count; i++)
			{
				if (!_canonicalReferences[i].Equals(other._canonicalReferences[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is FontFamilyIdentifier)
		{
			return Equals((FontFamilyIdentifier)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hash = 1;
		if (Count != 0)
		{
			Canonicalize();
			CanonicalFontFamilyReference[] canonicalReferences = _canonicalReferences;
			foreach (CanonicalFontFamilyReference canonicalFontFamilyReference in canonicalReferences)
			{
				hash = HashFn.HashMultiply(hash) + canonicalFontFamilyReference.GetHashCode();
			}
		}
		return HashFn.HashScramble(hash);
	}

	internal void Canonicalize()
	{
		if (_canonicalReferences != null)
		{
			return;
		}
		int count = Count;
		if (count == 0)
		{
			return;
		}
		BasedFriendlyName key = new BasedFriendlyName(_baseUri, _friendlyName);
		CanonicalFontFamilyReference[] array = TypefaceMetricsCache.ReadonlyLookup(key) as CanonicalFontFamilyReference[];
		if (array == null)
		{
			array = new CanonicalFontFamilyReference[count];
			int tokenIndex;
			int tokenLength;
			int i = FindToken(_friendlyName, 0, out tokenIndex, out tokenLength);
			array[0] = GetCanonicalReference(tokenIndex, tokenLength);
			for (int j = 1; j < count; j++)
			{
				i = FindToken(_friendlyName, i, out tokenIndex, out tokenLength);
				array[j] = GetCanonicalReference(tokenIndex, tokenLength);
			}
			TypefaceMetricsCache.Add(key, array);
		}
		_canonicalReferences = array;
	}

	private static int CountTokens(string friendlyName)
	{
		int num = 0;
		int tokenIndex;
		int tokenLength;
		int num2 = FindToken(friendlyName, 0, out tokenIndex, out tokenLength);
		while (num2 >= 0 && ++num != 32)
		{
			num2 = FindToken(friendlyName, num2, out tokenIndex, out tokenLength);
		}
		return num;
	}

	private static int FindToken(string friendlyName, int i, out int tokenIndex, out int tokenLength)
	{
		int length = friendlyName.Length;
		while (i < length)
		{
			while (i < length && char.IsWhiteSpace(friendlyName[i]))
			{
				i++;
			}
			int num = i;
			while (i < length)
			{
				if (friendlyName[i] == ',')
				{
					if (i + 1 >= length || friendlyName[i + 1] != ',')
					{
						break;
					}
					i += 2;
				}
				else
				{
					if (friendlyName[i] == '\0')
					{
						break;
					}
					i++;
				}
			}
			int num2 = i;
			while (num2 > num && char.IsWhiteSpace(friendlyName[num2 - 1]))
			{
				num2--;
			}
			if (num < num2)
			{
				tokenIndex = num;
				tokenLength = num2 - num;
				return i + 1;
			}
			i++;
		}
		tokenIndex = length;
		tokenLength = 0;
		return -1;
	}

	private CanonicalFontFamilyReference GetCanonicalReference(int startIndex, int length)
	{
		string normalizedFontFamilyReference = Util.GetNormalizedFontFamilyReference(_friendlyName, startIndex, length);
		BasedNormalizedName key = new BasedNormalizedName(_baseUri, normalizedFontFamilyReference);
		CanonicalFontFamilyReference canonicalFontFamilyReference = TypefaceMetricsCache.ReadonlyLookup(key) as CanonicalFontFamilyReference;
		if (canonicalFontFamilyReference == null)
		{
			canonicalFontFamilyReference = CanonicalFontFamilyReference.Create(_baseUri, normalizedFontFamilyReference);
			TypefaceMetricsCache.Add(key, canonicalFontFamilyReference);
		}
		return canonicalFontFamilyReference;
	}
}
