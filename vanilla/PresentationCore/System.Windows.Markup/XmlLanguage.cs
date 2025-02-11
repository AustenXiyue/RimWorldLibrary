using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using MS.Internal.PresentationCore;

namespace System.Windows.Markup;

/// <summary>Represents a language tag for use in XAML markup.</summary>
[TypeConverter(typeof(XmlLanguageConverter))]
public class XmlLanguage
{
	internal struct MatchingLanguageCollection : IEnumerable<XmlLanguage>, IEnumerable
	{
		private XmlLanguage _start;

		public MatchingLanguageCollection(XmlLanguage start)
		{
			_start = start;
		}

		public MatchingLanguageEnumerator GetEnumerator()
		{
			return new MatchingLanguageEnumerator(_start);
		}

		IEnumerator<XmlLanguage> IEnumerable<XmlLanguage>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	internal struct MatchingLanguageEnumerator : IEnumerator<XmlLanguage>, IEnumerator, IDisposable
	{
		private readonly XmlLanguage _start;

		private XmlLanguage _current;

		private bool _atStart;

		private bool _pastEnd;

		private int _maxCultureDepth;

		public XmlLanguage Current
		{
			get
			{
				if (_atStart)
				{
					throw new InvalidOperationException(SR.Enumerator_NotStarted);
				}
				if (_pastEnd)
				{
					throw new InvalidOperationException(SR.Enumerator_ReachedEnd);
				}
				return _current;
			}
		}

		object IEnumerator.Current => Current;

		public MatchingLanguageEnumerator(XmlLanguage start)
		{
			_start = start;
			_current = start;
			_pastEnd = false;
			_atStart = true;
			_maxCultureDepth = 32;
		}

		public void Reset()
		{
			_current = _start;
			_pastEnd = false;
			_atStart = true;
			_maxCultureDepth = 32;
		}

		public bool MoveNext()
		{
			if (_atStart)
			{
				_atStart = false;
				return true;
			}
			if (_current.IetfLanguageTag.Length == 0)
			{
				_atStart = false;
				_pastEnd = true;
				return false;
			}
			XmlLanguage prefixLanguage = _current.PrefixLanguage;
			CultureInfo culture = null;
			if (_maxCultureDepth > 0)
			{
				culture = ((!_current.TryGetEquivalentCulture(out culture)) ? null : culture.Parent);
			}
			if (culture == null)
			{
				_current = prefixLanguage;
				_atStart = false;
				return true;
			}
			XmlLanguage language = GetLanguage(culture.IetfLanguageTag);
			if (language.IsPrefixOf(prefixLanguage.IetfLanguageTag))
			{
				_current = prefixLanguage;
				_atStart = false;
				return true;
			}
			_maxCultureDepth--;
			_current = language;
			_atStart = false;
			return true;
		}

		void IDisposable.Dispose()
		{
		}
	}

	private static Hashtable _cache = new Hashtable(10);

	private const int InitialDictionarySize = 10;

	private const int MaxCultureDepth = 32;

	private static XmlLanguage _empty = null;

	private readonly string _lowerCaseTag;

	private CultureInfo _equivalentCulture;

	private CultureInfo _specificCulture;

	private CultureInfo _compatibleCulture;

	private int _specificity;

	private bool _equivalentCultureFailed;

	/// <summary>Gets a static <see cref="T:System.Windows.Markup.XmlLanguage" /> instance as would be created by <see cref="M:System.Windows.Markup.XmlLanguage.GetLanguage(System.String)" /> with the language tag as an empty attribute string.</summary>
	/// <returns>The empty language tag version of <see cref="T:System.Windows.Markup.XmlLanguage" />, for use in comparison operations.</returns>
	public static XmlLanguage Empty
	{
		get
		{
			if (_empty == null)
			{
				_empty = GetLanguage(string.Empty);
			}
			return _empty;
		}
	}

	/// <summary>Gets the string representation of the language tag.</summary>
	/// <returns>The string representation of the language tag.</returns>
	public string IetfLanguageTag => _lowerCaseTag;

	internal MatchingLanguageCollection MatchingLanguages => new MatchingLanguageCollection(this);

	private XmlLanguage PrefixLanguage => GetLanguage(Shorten(IetfLanguageTag));

	private XmlLanguage(string lowercase)
	{
		_lowerCaseTag = lowercase;
		_equivalentCulture = null;
		_specificCulture = null;
		_compatibleCulture = null;
		_specificity = -1;
		_equivalentCultureFailed = false;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Markup.XmlLanguage" /> instance, based on a string representing the language per RFC 3066.</summary>
	/// <returns>A new <see cref="T:System.Windows.Markup.XmlLanguage" /> with the provided string as its <see cref="P:System.Windows.Markup.XmlLanguage.IetfLanguageTag" /> value.</returns>
	/// <param name="ietfLanguageTag">An RFC 3066 language string, or empty string.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ietfLanguageTag" /> parameter cannot be null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ietfLanguageTag" /> parameter was non-empty, but did not conform to the syntax specified in RFC 3066. See Remarks.</exception>
	public static XmlLanguage GetLanguage(string ietfLanguageTag)
	{
		if (ietfLanguageTag == null)
		{
			throw new ArgumentNullException("ietfLanguageTag");
		}
		string text = AsciiToLower(ietfLanguageTag);
		XmlLanguage xmlLanguage = (XmlLanguage)_cache[text];
		if (xmlLanguage == null)
		{
			ValidateLowerCaseTag(text);
			lock (_cache.SyncRoot)
			{
				xmlLanguage = (XmlLanguage)_cache[text];
				if (xmlLanguage == null)
				{
					xmlLanguage = (XmlLanguage)(_cache[text] = new XmlLanguage(text));
				}
			}
		}
		return xmlLanguage;
	}

	/// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Windows.Markup.XmlLanguage" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Windows.Markup.XmlLanguage" />.</returns>
	public override string ToString()
	{
		return IetfLanguageTag;
	}

	/// <summary>Returns the appropriate equivalent <see cref="T:System.Globalization.CultureInfo" /> for this <see cref="T:System.Windows.Markup.XmlLanguage" />, if and only if such a <see cref="T:System.Globalization.CultureInfo" /> is registered for the <see cref="P:System.Windows.Markup.XmlLanguage.IetfLanguageTag" /> value of this <see cref="T:System.Windows.Markup.XmlLanguage" /></summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> that can be used for localization-globalization API calls that take that type as an argument.</returns>
	/// <exception cref="T:System.InvalidOperationException">No registered <see cref="T:System.Globalization.CultureInfo" /> for the provided <see cref="T:System.Windows.Markup.XmlLanguage" /> exists, as determined by a call to <see cref="M:System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(System.String)" />.</exception>
	public CultureInfo GetEquivalentCulture()
	{
		if (_equivalentCulture == null)
		{
			string text = _lowerCaseTag;
			if (string.CompareOrdinal(text, "und") == 0)
			{
				text = string.Empty;
			}
			try
			{
				_equivalentCulture = SafeSecurityHelper.GetCultureInfoByIetfLanguageTag(text);
			}
			catch (ArgumentException innerException)
			{
				_equivalentCultureFailed = true;
				throw new InvalidOperationException(SR.Format(SR.XmlLangGetCultureFailure, text), innerException);
			}
		}
		return _equivalentCulture;
	}

	/// <summary>Returns the most-closely-related non-neutral <see cref="T:System.Globalization.CultureInfo" /> for this <see cref="T:System.Windows.Markup.XmlLanguage" />.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> that can be used for localization-globalization API calls that take that type as an argument.</returns>
	/// <exception cref="T:System.InvalidOperationException">No related non-neutral <see cref="T:System.Globalization.CultureInfo" /> is registered for the current <see cref="T:System.Windows.Markup.XmlLanguage" />  <see cref="P:System.Windows.Markup.XmlLanguage.IetfLanguageTag" />.</exception>
	public CultureInfo GetSpecificCulture()
	{
		if (_specificCulture == null)
		{
			if (_lowerCaseTag.Length == 0 || string.CompareOrdinal(_lowerCaseTag, "und") == 0)
			{
				_specificCulture = GetEquivalentCulture();
			}
			else
			{
				CultureInfo compatibleCulture = GetCompatibleCulture();
				if (compatibleCulture.IetfLanguageTag.Length == 0)
				{
					throw new InvalidOperationException(SR.Format(SR.XmlLangGetSpecificCulture, _lowerCaseTag));
				}
				if (!compatibleCulture.IsNeutralCulture)
				{
					_specificCulture = compatibleCulture;
				}
				else
				{
					try
					{
						compatibleCulture = CultureInfo.CreateSpecificCulture(compatibleCulture.Name);
						_specificCulture = SafeSecurityHelper.GetCultureInfoByIetfLanguageTag(compatibleCulture.IetfLanguageTag);
					}
					catch (ArgumentException innerException)
					{
						throw new InvalidOperationException(SR.Format(SR.XmlLangGetSpecificCulture, _lowerCaseTag), innerException);
					}
				}
			}
		}
		return _specificCulture;
	}

	[FriendAccessAllowed]
	internal CultureInfo GetCompatibleCulture()
	{
		if (_compatibleCulture == null)
		{
			CultureInfo culture = null;
			if (!TryGetEquivalentCulture(out culture))
			{
				string text = IetfLanguageTag;
				do
				{
					text = Shorten(text);
					if (text == null)
					{
						culture = CultureInfo.InvariantCulture;
						continue;
					}
					try
					{
						culture = SafeSecurityHelper.GetCultureInfoByIetfLanguageTag(text);
					}
					catch (ArgumentException)
					{
					}
				}
				while (culture == null);
			}
			_compatibleCulture = culture;
		}
		return _compatibleCulture;
	}

	[FriendAccessAllowed]
	internal bool RangeIncludes(XmlLanguage language)
	{
		if (IsPrefixOf(language.IetfLanguageTag))
		{
			return true;
		}
		return RangeIncludes(language.GetCompatibleCulture());
	}

	internal bool RangeIncludes(CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		for (int i = 0; i < 32; i++)
		{
			if (IsPrefixOf(culture.IetfLanguageTag))
			{
				return true;
			}
			CultureInfo parent = culture.Parent;
			if (parent == null || parent.Equals(CultureInfo.InvariantCulture) || parent == culture)
			{
				break;
			}
			culture = parent;
		}
		return false;
	}

	internal int GetSpecificity()
	{
		if (_specificity < 0)
		{
			CultureInfo compatibleCulture = GetCompatibleCulture();
			int num = GetSpecificity(compatibleCulture, 32);
			if (compatibleCulture != _equivalentCulture)
			{
				num += GetSubtagCount(_lowerCaseTag) - GetSubtagCount(compatibleCulture.IetfLanguageTag);
			}
			_specificity = num;
		}
		return _specificity;
	}

	private static int GetSpecificity(CultureInfo culture, int maxDepth)
	{
		int result = 0;
		if (maxDepth != 0 && culture != null)
		{
			string ietfLanguageTag = culture.IetfLanguageTag;
			if (ietfLanguageTag.Length > 0)
			{
				result = Math.Max(GetSubtagCount(ietfLanguageTag), 1 + GetSpecificity(culture.Parent, maxDepth - 1));
			}
		}
		return result;
	}

	private static int GetSubtagCount(string languageTag)
	{
		int length = languageTag.Length;
		int num = 0;
		if (length > 0)
		{
			num = 1;
			for (int i = 0; i < length; i++)
			{
				if (languageTag[i] == '-')
				{
					num++;
				}
			}
		}
		return num;
	}

	private bool IsPrefixOf(string longTag)
	{
		string ietfLanguageTag = IetfLanguageTag;
		if (!longTag.StartsWith(ietfLanguageTag, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (ietfLanguageTag.Length != 0 && ietfLanguageTag.Length != longTag.Length)
		{
			return longTag[ietfLanguageTag.Length] == '-';
		}
		return true;
	}

	private bool TryGetEquivalentCulture(out CultureInfo culture)
	{
		culture = null;
		if (_equivalentCulture == null && !_equivalentCultureFailed)
		{
			try
			{
				GetEquivalentCulture();
			}
			catch (InvalidOperationException)
			{
			}
		}
		culture = _equivalentCulture;
		return culture != null;
	}

	private static string Shorten(string languageTag)
	{
		if (languageTag.Length == 0)
		{
			return null;
		}
		int num = languageTag.Length - 1;
		while (languageTag[num] != '-' && num > 0)
		{
			num--;
		}
		return languageTag.Substring(0, num);
	}

	private static void ValidateLowerCaseTag(string ietfLanguageTag)
	{
		if (ietfLanguageTag == null)
		{
			throw new ArgumentNullException("ietfLanguageTag");
		}
		if (ietfLanguageTag.Length <= 0)
		{
			return;
		}
		using StringReader reader = new StringReader(ietfLanguageTag);
		for (int num = ParseSubtag(ietfLanguageTag, reader, isPrimary: true); num != -1; num = ParseSubtag(ietfLanguageTag, reader, isPrimary: false))
		{
		}
	}

	private static int ParseSubtag(string ietfLanguageTag, StringReader reader, bool isPrimary)
	{
		int c = reader.Read();
		bool flag = IsLowerAlpha(c);
		if (!flag && !isPrimary)
		{
			flag = IsDigit(c);
		}
		if (!flag)
		{
			ThrowParseException(ietfLanguageTag);
		}
		int num = 1;
		while (true)
		{
			c = reader.Read();
			num++;
			flag = IsLowerAlpha(c);
			if (!flag && !isPrimary)
			{
				flag = IsDigit(c);
			}
			if (!flag)
			{
				if (c == -1 || c == 45)
				{
					break;
				}
				ThrowParseException(ietfLanguageTag);
			}
			else if (num > 8)
			{
				ThrowParseException(ietfLanguageTag);
			}
		}
		return c;
	}

	private static bool IsLowerAlpha(int c)
	{
		if (c >= 97)
		{
			return c <= 122;
		}
		return false;
	}

	private static bool IsDigit(int c)
	{
		if (c >= 48)
		{
			return c <= 57;
		}
		return false;
	}

	private static void ThrowParseException(string ietfLanguageTag)
	{
		throw new ArgumentException(SR.Format(SR.XmlLangMalformed, ietfLanguageTag), "ietfLanguageTag");
	}

	private static string AsciiToLower(string tag)
	{
		int length = tag.Length;
		for (int i = 0; i < length; i++)
		{
			if (tag[i] > '\u007f')
			{
				ThrowParseException(tag);
			}
		}
		return tag.ToLowerInvariant();
	}
}
