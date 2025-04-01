using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using Mono.Globalization.Unicode;
using Unity;

namespace System.Globalization;

/// <summary>Implements a set of methods for culture-sensitive string comparisons.</summary>
[Serializable]
[ComVisible(true)]
public class CompareInfo : IDeserializationCallback
{
	private const CompareOptions ValidIndexMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);

	private const CompareOptions ValidCompareMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.StringSort);

	private const CompareOptions ValidHashCodeOfStringMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);

	[OptionalField(VersionAdded = 2)]
	private string m_name;

	[NonSerialized]
	private string m_sortName;

	[OptionalField(VersionAdded = 1)]
	private int win32LCID;

	private int culture;

	private const int LINGUISTIC_IGNORECASE = 16;

	private const int NORM_IGNORECASE = 1;

	private const int NORM_IGNOREKANATYPE = 65536;

	private const int LINGUISTIC_IGNOREDIACRITIC = 32;

	private const int NORM_IGNORENONSPACE = 2;

	private const int NORM_IGNORESYMBOLS = 4;

	private const int NORM_IGNOREWIDTH = 131072;

	private const int SORT_STRINGSORT = 4096;

	private const int COMPARE_OPTIONS_ORDINAL = 1073741824;

	internal const int NORM_LINGUISTIC_CASING = 134217728;

	private const int RESERVED_FIND_ASCII_STRING = 536870912;

	private const int SORT_VERSION_WHIDBEY = 4096;

	private const int SORT_VERSION_V4 = 393473;

	[OptionalField(VersionAdded = 3)]
	private SortVersion m_SortVersion;

	[NonSerialized]
	private SimpleCollator collator;

	private static Dictionary<string, SimpleCollator> collators;

	private static bool managedCollation;

	private static bool managedCollationChecked;

	/// <summary>Gets the name of the culture used for sorting operations by this <see cref="T:System.Globalization.CompareInfo" /> object.</summary>
	/// <returns>The name of a culture.</returns>
	[ComVisible(false)]
	public virtual string Name
	{
		get
		{
			if (m_name == "zh-CHT" || m_name == "zh-CHS")
			{
				return m_name;
			}
			return m_sortName;
		}
	}

	/// <summary>Gets the properly formed culture identifier for the current <see cref="T:System.Globalization.CompareInfo" />.</summary>
	/// <returns>The properly formed culture identifier for the current <see cref="T:System.Globalization.CompareInfo" />.</returns>
	public int LCID => CultureInfo.GetCultureInfo(Name).LCID;

	internal static bool IsLegacy20SortingBehaviorRequested => InternalSortVersion == 4096;

	private static uint InternalSortVersion
	{
		[SecuritySafeCritical]
		get
		{
			return 393473u;
		}
	}

	/// <summary>Gets information about the version of Unicode used for comparing and sorting strings.</summary>
	/// <returns>An object that contains information about the Unicode version used for comparing and sorting strings.</returns>
	public SortVersion Version
	{
		[SecuritySafeCritical]
		get
		{
			if (m_SortVersion == null)
			{
				m_SortVersion = new SortVersion(393473, new Guid("00000001-57ee-1e5c-00b4-d0000bb1e11e"));
			}
			return m_SortVersion;
		}
	}

	private static bool UseManagedCollation
	{
		get
		{
			if (!managedCollationChecked)
			{
				managedCollation = Environment.internalGetEnvironmentVariable("MONO_DISABLE_MANAGED_COLLATION") != "yes" && MSCompatUnicodeTable.IsReady;
				managedCollationChecked = true;
			}
			return managedCollation;
		}
	}

	internal CompareInfo(CultureInfo culture)
	{
		m_name = culture.m_name;
		m_sortName = culture.SortName;
	}

	/// <summary>Initializes a new <see cref="T:System.Globalization.CompareInfo" /> object that is associated with the specified culture and that uses string comparison methods in the specified <see cref="T:System.Reflection.Assembly" />.</summary>
	/// <returns>A new <see cref="T:System.Globalization.CompareInfo" /> object associated with the culture with the specified identifier and using string comparison methods in the current <see cref="T:System.Reflection.Assembly" />.</returns>
	/// <param name="culture">An integer representing the culture identifier. </param>
	/// <param name="assembly">An <see cref="T:System.Reflection.Assembly" /> that contains the string comparison methods to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assembly" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assembly" /> is of an invalid type. </exception>
	public static CompareInfo GetCompareInfo(int culture, Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		if (assembly != typeof(object).Module.Assembly)
		{
			throw new ArgumentException(Environment.GetResourceString("Only mscorlib's assembly is valid."));
		}
		return GetCompareInfo(culture);
	}

	/// <summary>Initializes a new <see cref="T:System.Globalization.CompareInfo" /> object that is associated with the specified culture and that uses string comparison methods in the specified <see cref="T:System.Reflection.Assembly" />.</summary>
	/// <returns>A new <see cref="T:System.Globalization.CompareInfo" /> object associated with the culture with the specified identifier and using string comparison methods in the current <see cref="T:System.Reflection.Assembly" />.</returns>
	/// <param name="name">A string representing the culture name. </param>
	/// <param name="assembly">An <see cref="T:System.Reflection.Assembly" /> that contains the string comparison methods to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.-or- <paramref name="assembly" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an invalid culture name.-or- <paramref name="assembly" /> is of an invalid type. </exception>
	public static CompareInfo GetCompareInfo(string name, Assembly assembly)
	{
		if (name == null || assembly == null)
		{
			throw new ArgumentNullException((name == null) ? "name" : "assembly");
		}
		if (assembly != typeof(object).Module.Assembly)
		{
			throw new ArgumentException(Environment.GetResourceString("Only mscorlib's assembly is valid."));
		}
		return GetCompareInfo(name);
	}

	/// <summary>Initializes a new <see cref="T:System.Globalization.CompareInfo" /> object that is associated with the culture with the specified identifier.</summary>
	/// <returns>A new <see cref="T:System.Globalization.CompareInfo" /> object associated with the culture with the specified identifier and using string comparison methods in the current <see cref="T:System.Reflection.Assembly" />.</returns>
	/// <param name="culture">An integer representing the culture identifier. </param>
	public static CompareInfo GetCompareInfo(int culture)
	{
		if (CultureData.IsCustomCultureId(culture))
		{
			throw new ArgumentException(Environment.GetResourceString("Customized cultures cannot be passed by LCID, only by name.", "culture"));
		}
		return CultureInfo.GetCultureInfo(culture).CompareInfo;
	}

	/// <summary>Initializes a new <see cref="T:System.Globalization.CompareInfo" /> object that is associated with the culture with the specified name.</summary>
	/// <returns>A new <see cref="T:System.Globalization.CompareInfo" /> object associated with the culture with the specified identifier and using string comparison methods in the current <see cref="T:System.Reflection.Assembly" />.</returns>
	/// <param name="name">A string representing the culture name. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an invalid culture name. </exception>
	public static CompareInfo GetCompareInfo(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return CultureInfo.GetCultureInfo(name).CompareInfo;
	}

	/// <summary>Indicates whether a specified Unicode character is sortable.</summary>
	/// <returns>true if the <paramref name="ch" /> parameter is sortable; otherwise, false.</returns>
	/// <param name="ch">A Unicode character.</param>
	[ComVisible(false)]
	public static bool IsSortable(char ch)
	{
		return IsSortable(ch.ToString());
	}

	/// <summary>Indicates whether a specified Unicode string is sortable.</summary>
	/// <returns>true if the <paramref name="str" /> parameter is not an empty string ("") and all the Unicode characters in <paramref name="str" /> are sortable; otherwise, false.</returns>
	/// <param name="text">A string of zero or more Unicode characters.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null.</exception>
	[SecuritySafeCritical]
	[ComVisible(false)]
	public static bool IsSortable(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length == 0)
		{
			return false;
		}
		return MSCompatUnicodeTable.IsSortable(text);
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext ctx)
	{
		m_name = null;
	}

	private void OnDeserialized()
	{
		CultureInfo cultureInfo;
		if (m_name == null)
		{
			cultureInfo = CultureInfo.GetCultureInfo(culture);
			m_name = cultureInfo.m_name;
		}
		else
		{
			cultureInfo = CultureInfo.GetCultureInfo(m_name);
		}
		m_sortName = cultureInfo.SortName;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext ctx)
	{
		OnDeserialized();
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext ctx)
	{
		culture = CultureInfo.GetCultureInfo(Name).LCID;
	}

	/// <summary>Runs when the entire object graph has been deserialized.</summary>
	/// <param name="sender">The object that initiated the callback. </param>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
		OnDeserialized();
	}

	internal static int GetNativeCompareFlags(CompareOptions options)
	{
		int num = 134217728;
		if ((options & CompareOptions.IgnoreCase) != 0)
		{
			num |= 1;
		}
		if ((options & CompareOptions.IgnoreKanaType) != 0)
		{
			num |= 0x10000;
		}
		if ((options & CompareOptions.IgnoreNonSpace) != 0)
		{
			num |= 2;
		}
		if ((options & CompareOptions.IgnoreSymbols) != 0)
		{
			num |= 4;
		}
		if ((options & CompareOptions.IgnoreWidth) != 0)
		{
			num |= 0x20000;
		}
		if ((options & CompareOptions.StringSort) != 0)
		{
			num |= 0x1000;
		}
		if (options == CompareOptions.Ordinal)
		{
			num = 1073741824;
		}
		return num;
	}

	/// <summary>Compares two strings. </summary>
	/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.Value Condition zero The two strings are equal. less than zero <paramref name="string1" /> is less than <paramref name="string2" />. greater than zero <paramref name="string1" /> is greater than <paramref name="string2" />. </returns>
	/// <param name="string1">The first string to compare. </param>
	/// <param name="string2">The second string to compare. </param>
	public virtual int Compare(string string1, string string2)
	{
		return Compare(string1, string2, CompareOptions.None);
	}

	/// <summary>Compares two strings using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.Value Condition zero The two strings are equal. less than zero <paramref name="string1" /> is less than <paramref name="string2" />. greater than zero <paramref name="string1" /> is greater than <paramref name="string2" />. </returns>
	/// <param name="string1">The first string to compare. </param>
	/// <param name="string2">The second string to compare. </param>
	/// <param name="options">A value that defines how <paramref name="string1" /> and <paramref name="string2" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />, and <see cref="F:System.Globalization.CompareOptions.StringSort" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	[SecuritySafeCritical]
	public virtual int Compare(string string1, string string2, CompareOptions options)
	{
		if (options == CompareOptions.OrdinalIgnoreCase)
		{
			return string.Compare(string1, string2, StringComparison.OrdinalIgnoreCase);
		}
		if ((options & CompareOptions.Ordinal) != 0)
		{
			if (options != CompareOptions.Ordinal)
			{
				throw new ArgumentException(Environment.GetResourceString("CompareOption.Ordinal cannot be used with other options."), "options");
			}
			return string.CompareOrdinal(string1, string2);
		}
		if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.StringSort)) != 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
		}
		if (string1 == null)
		{
			if (string2 == null)
			{
				return 0;
			}
			return -1;
		}
		if (string2 == null)
		{
			return 1;
		}
		return internal_compare_switch(string1, 0, string1.Length, string2, 0, string2.Length, options);
	}

	/// <summary>Compares a section of one string with a section of another string.</summary>
	/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.Value Condition zero The two strings are equal. less than zero The specified section of <paramref name="string1" /> is less than the specified section of <paramref name="string2" />. greater than zero The specified section of <paramref name="string1" /> is greater than the specified section of <paramref name="string2" />. </returns>
	/// <param name="string1">The first string to compare. </param>
	/// <param name="offset1">The zero-based index of the character in <paramref name="string1" /> at which to start comparing. </param>
	/// <param name="length1">The number of consecutive characters in <paramref name="string1" /> to compare. </param>
	/// <param name="string2">The second string to compare. </param>
	/// <param name="offset2">The zero-based index of the character in <paramref name="string2" /> at which to start comparing. </param>
	/// <param name="length2">The number of consecutive characters in <paramref name="string2" /> to compare. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset1" /> or <paramref name="length1" /> or <paramref name="offset2" /> or <paramref name="length2" /> is less than zero.-or- <paramref name="offset1" /> is greater than or equal to the number of characters in <paramref name="string1" />.-or- <paramref name="offset2" /> is greater than or equal to the number of characters in <paramref name="string2" />.-or- <paramref name="length1" /> is greater than the number of characters from <paramref name="offset1" /> to the end of <paramref name="string1" />.-or- <paramref name="length2" /> is greater than the number of characters from <paramref name="offset2" /> to the end of <paramref name="string2" />. </exception>
	public virtual int Compare(string string1, int offset1, int length1, string string2, int offset2, int length2)
	{
		return Compare(string1, offset1, length1, string2, offset2, length2, CompareOptions.None);
	}

	/// <summary>Compares the end section of a string with the end section of another string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.Value Condition zero The two strings are equal. less than zero The specified section of <paramref name="string1" /> is less than the specified section of <paramref name="string2" />. greater than zero The specified section of <paramref name="string1" /> is greater than the specified section of <paramref name="string2" />. </returns>
	/// <param name="string1">The first string to compare. </param>
	/// <param name="offset1">The zero-based index of the character in <paramref name="string1" /> at which to start comparing. </param>
	/// <param name="string2">The second string to compare. </param>
	/// <param name="offset2">The zero-based index of the character in <paramref name="string2" /> at which to start comparing. </param>
	/// <param name="options">A value that defines how <paramref name="string1" /> and <paramref name="string2" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />, and <see cref="F:System.Globalization.CompareOptions.StringSort" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset1" /> or <paramref name="offset2" /> is less than zero.-or- <paramref name="offset1" /> is greater than or equal to the number of characters in <paramref name="string1" />.-or- <paramref name="offset2" /> is greater than or equal to the number of characters in <paramref name="string2" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int Compare(string string1, int offset1, string string2, int offset2, CompareOptions options)
	{
		return Compare(string1, offset1, (string1 != null) ? (string1.Length - offset1) : 0, string2, offset2, (string2 != null) ? (string2.Length - offset2) : 0, options);
	}

	/// <summary>Compares the end section of a string with the end section of another string.</summary>
	/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.Value Condition zero The two strings are equal. less than zero The specified section of <paramref name="string1" /> is less than the specified section of <paramref name="string2" />. greater than zero The specified section of <paramref name="string1" /> is greater than the specified section of <paramref name="string2" />. </returns>
	/// <param name="string1">The first string to compare. </param>
	/// <param name="offset1">The zero-based index of the character in <paramref name="string1" /> at which to start comparing. </param>
	/// <param name="string2">The second string to compare. </param>
	/// <param name="offset2">The zero-based index of the character in <paramref name="string2" /> at which to start comparing. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset1" /> or <paramref name="offset2" /> is less than zero.-or- <paramref name="offset1" /> is greater than or equal to the number of characters in <paramref name="string1" />.-or- <paramref name="offset2" /> is greater than or equal to the number of characters in <paramref name="string2" />. </exception>
	public virtual int Compare(string string1, int offset1, string string2, int offset2)
	{
		return Compare(string1, offset1, string2, offset2, CompareOptions.None);
	}

	/// <summary>Compares a section of one string with a section of another string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.Value Condition zero The two strings are equal. less than zero The specified section of <paramref name="string1" /> is less than the specified section of <paramref name="string2" />. greater than zero The specified section of <paramref name="string1" /> is greater than the specified section of <paramref name="string2" />. </returns>
	/// <param name="string1">The first string to compare. </param>
	/// <param name="offset1">The zero-based index of the character in <paramref name="string1" /> at which to start comparing. </param>
	/// <param name="length1">The number of consecutive characters in <paramref name="string1" /> to compare. </param>
	/// <param name="string2">The second string to compare. </param>
	/// <param name="offset2">The zero-based index of the character in <paramref name="string2" /> at which to start comparing. </param>
	/// <param name="length2">The number of consecutive characters in <paramref name="string2" /> to compare. </param>
	/// <param name="options">A value that defines how <paramref name="string1" /> and <paramref name="string2" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />, and <see cref="F:System.Globalization.CompareOptions.StringSort" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset1" /> or <paramref name="length1" /> or <paramref name="offset2" /> or <paramref name="length2" /> is less than zero.-or- <paramref name="offset1" /> is greater than or equal to the number of characters in <paramref name="string1" />.-or- <paramref name="offset2" /> is greater than or equal to the number of characters in <paramref name="string2" />.-or- <paramref name="length1" /> is greater than the number of characters from <paramref name="offset1" /> to the end of <paramref name="string1" />.-or- <paramref name="length2" /> is greater than the number of characters from <paramref name="offset2" /> to the end of <paramref name="string2" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	[SecuritySafeCritical]
	public virtual int Compare(string string1, int offset1, int length1, string string2, int offset2, int length2, CompareOptions options)
	{
		if (options == CompareOptions.OrdinalIgnoreCase)
		{
			int num = string.Compare(string1, offset1, string2, offset2, (length1 < length2) ? length1 : length2, StringComparison.OrdinalIgnoreCase);
			if (length1 != length2 && num == 0)
			{
				if (length1 <= length2)
				{
					return -1;
				}
				return 1;
			}
			return num;
		}
		if (length1 < 0 || length2 < 0)
		{
			throw new ArgumentOutOfRangeException((length1 < 0) ? "length1" : "length2", Environment.GetResourceString("Positive number required."));
		}
		if (offset1 < 0 || offset2 < 0)
		{
			throw new ArgumentOutOfRangeException((offset1 < 0) ? "offset1" : "offset2", Environment.GetResourceString("Positive number required."));
		}
		if (offset1 > (string1?.Length ?? 0) - length1)
		{
			throw new ArgumentOutOfRangeException("string1", Environment.GetResourceString("Offset and length must refer to a position in the string."));
		}
		if (offset2 > (string2?.Length ?? 0) - length2)
		{
			throw new ArgumentOutOfRangeException("string2", Environment.GetResourceString("Offset and length must refer to a position in the string."));
		}
		if ((options & CompareOptions.Ordinal) != 0)
		{
			if (options != CompareOptions.Ordinal)
			{
				throw new ArgumentException(Environment.GetResourceString("CompareOption.Ordinal cannot be used with other options."), "options");
			}
		}
		else if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.StringSort)) != 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
		}
		if (string1 == null)
		{
			if (string2 == null)
			{
				return 0;
			}
			return -1;
		}
		if (string2 == null)
		{
			return 1;
		}
		if (options == CompareOptions.Ordinal)
		{
			return CompareOrdinal(string1, offset1, length1, string2, offset2, length2);
		}
		return internal_compare_switch(string1, offset1, length1, string2, offset2, length2, options);
	}

	[SecurityCritical]
	private static int CompareOrdinal(string string1, int offset1, int length1, string string2, int offset2, int length2)
	{
		int num = string.nativeCompareOrdinalEx(string1, offset1, string2, offset2, (length1 < length2) ? length1 : length2);
		if (length1 != length2 && num == 0)
		{
			if (length1 <= length2)
			{
				return -1;
			}
			return 1;
		}
		return num;
	}

	/// <summary>Determines whether the specified source string starts with the specified prefix using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>true if the length of <paramref name="prefix" /> is less than or equal to the length of <paramref name="source" /> and <paramref name="source" /> starts with <paramref name="prefix" />; otherwise, false.</returns>
	/// <param name="source">The string to search in. </param>
	/// <param name="prefix">The string to compare with the beginning of <paramref name="source" />. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="prefix" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="prefix" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	[SecuritySafeCritical]
	public virtual bool IsPrefix(string source, string prefix, CompareOptions options)
	{
		if (source == null || prefix == null)
		{
			throw new ArgumentNullException((source == null) ? "source" : "prefix", Environment.GetResourceString("String reference not set to an instance of a String."));
		}
		if (prefix.Length == 0)
		{
			return true;
		}
		switch (options)
		{
		case CompareOptions.OrdinalIgnoreCase:
			return source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		case CompareOptions.Ordinal:
			return source.StartsWith(prefix, StringComparison.Ordinal);
		default:
			if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
			}
			if (UseManagedCollation)
			{
				return GetCollator().IsPrefix(source, prefix, options);
			}
			if (source.Length < prefix.Length)
			{
				return false;
			}
			return Compare(source, 0, prefix.Length, prefix, 0, prefix.Length, options) == 0;
		}
	}

	/// <summary>Determines whether the specified source string starts with the specified prefix.</summary>
	/// <returns>true if the length of <paramref name="prefix" /> is less than or equal to the length of <paramref name="source" /> and <paramref name="source" /> starts with <paramref name="prefix" />; otherwise, false.</returns>
	/// <param name="source">The string to search in. </param>
	/// <param name="prefix">The string to compare with the beginning of <paramref name="source" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="prefix" /> is null. </exception>
	public virtual bool IsPrefix(string source, string prefix)
	{
		return IsPrefix(source, prefix, CompareOptions.None);
	}

	/// <summary>Determines whether the specified source string ends with the specified suffix using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>true if the length of <paramref name="suffix" /> is less than or equal to the length of <paramref name="source" /> and <paramref name="source" /> ends with <paramref name="suffix" />; otherwise, false.</returns>
	/// <param name="source">The string to search in. </param>
	/// <param name="suffix">The string to compare with the end of <paramref name="source" />. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="suffix" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" /> used by itself, or the bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="suffix" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	[SecuritySafeCritical]
	public virtual bool IsSuffix(string source, string suffix, CompareOptions options)
	{
		if (source == null || suffix == null)
		{
			throw new ArgumentNullException((source == null) ? "source" : "suffix", Environment.GetResourceString("String reference not set to an instance of a String."));
		}
		if (suffix.Length == 0)
		{
			return true;
		}
		switch (options)
		{
		case CompareOptions.OrdinalIgnoreCase:
			return source.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
		case CompareOptions.Ordinal:
			return source.EndsWith(suffix, StringComparison.Ordinal);
		default:
			if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
			}
			if (UseManagedCollation)
			{
				return GetCollator().IsSuffix(source, suffix, options);
			}
			if (source.Length < suffix.Length)
			{
				return false;
			}
			return Compare(source, source.Length - suffix.Length, suffix.Length, suffix, 0, suffix.Length, options) == 0;
		}
	}

	/// <summary>Determines whether the specified source string ends with the specified suffix.</summary>
	/// <returns>true if the length of <paramref name="suffix" /> is less than or equal to the length of <paramref name="source" /> and <paramref name="source" /> ends with <paramref name="suffix" />; otherwise, false.</returns>
	/// <param name="source">The string to search in. </param>
	/// <param name="suffix">The string to compare with the end of <paramref name="source" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="suffix" /> is null. </exception>
	public virtual bool IsSuffix(string source, string suffix)
	{
		return IsSuffix(source, suffix, CompareOptions.None);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the first occurrence within the entire source string.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within <paramref name="source" />; otherwise, -1. Returns 0 (zero) if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	public virtual int IndexOf(string source, char value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return IndexOf(source, value, 0, source.Length, CompareOptions.None);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the first occurrence within the entire source string.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within <paramref name="source" />; otherwise, -1. Returns 0 (zero) if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	public virtual int IndexOf(string source, string value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return IndexOf(source, value, 0, source.Length, CompareOptions.None);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the first occurrence within the entire source string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within <paramref name="source" />, using the specified comparison options; otherwise, -1. Returns 0 (zero) if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="options">A value that defines how the strings should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int IndexOf(string source, char value, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return IndexOf(source, value, 0, source.Length, options);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the first occurrence within the entire source string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within <paramref name="source" />, using the specified comparison options; otherwise, -1. Returns 0 (zero) if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int IndexOf(string source, string value, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return IndexOf(source, value, 0, source.Length, options);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the first occurrence within the section of the source string that extends from the specified index to the end of the string.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that extends from <paramref name="startIndex" /> to the end of <paramref name="source" />; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />. </exception>
	public virtual int IndexOf(string source, char value, int startIndex)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return IndexOf(source, value, startIndex, source.Length - startIndex, CompareOptions.None);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the first occurrence within the section of the source string that extends from the specified index to the end of the string.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that extends from <paramref name="startIndex" /> to the end of <paramref name="source" />; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />. </exception>
	public virtual int IndexOf(string source, string value, int startIndex)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return IndexOf(source, value, startIndex, source.Length - startIndex, CompareOptions.None);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the first occurrence within the section of the source string that extends from the specified index to the end of the string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that extends from <paramref name="startIndex" /> to the end of <paramref name="source" />, using the specified comparison options; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the search. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int IndexOf(string source, char value, int startIndex, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return IndexOf(source, value, startIndex, source.Length - startIndex, options);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the first occurrence within the section of the source string that extends from the specified index to the end of the string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that extends from <paramref name="startIndex" /> to the end of <paramref name="source" />, using the specified comparison options; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the search. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int IndexOf(string source, string value, int startIndex, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return IndexOf(source, value, startIndex, source.Length - startIndex, options);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the first occurrence within the section of the source string that starts at the specified index and contains the specified number of elements.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that starts at <paramref name="startIndex" /> and contains the number of elements specified by <paramref name="count" />; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="source" />. </exception>
	public virtual int IndexOf(string source, char value, int startIndex, int count)
	{
		return IndexOf(source, value, startIndex, count, CompareOptions.None);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the first occurrence within the section of the source string that starts at the specified index and contains the specified number of elements.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that starts at <paramref name="startIndex" /> and contains the number of elements specified by <paramref name="count" />; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="source" />. </exception>
	public virtual int IndexOf(string source, string value, int startIndex, int count)
	{
		return IndexOf(source, value, startIndex, count, CompareOptions.None);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the first occurrence within the section of the source string that starts at the specified index and contains the specified number of elements using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that starts at <paramref name="startIndex" /> and contains the number of elements specified by <paramref name="count" />, using the specified comparison options; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="source" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	[SecuritySafeCritical]
	public virtual int IndexOf(string source, char value, int startIndex, int count, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (startIndex < 0 || startIndex > source.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count < 0 || startIndex > source.Length - count)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		if (options == CompareOptions.OrdinalIgnoreCase)
		{
			return source.IndexOf(value.ToString(), startIndex, count, StringComparison.OrdinalIgnoreCase);
		}
		if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)) != 0 && options != CompareOptions.Ordinal)
		{
			throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
		}
		return internal_index_switch(source, startIndex, count, value, options, first: true);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the first occurrence within the section of the source string that starts at the specified index and contains the specified number of elements using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that starts at <paramref name="startIndex" /> and contains the number of elements specified by <paramref name="count" />, using the specified comparison options; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="source" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	[SecuritySafeCritical]
	public virtual int IndexOf(string source, string value, int startIndex, int count, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex > source.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (source.Length == 0)
		{
			if (value.Length == 0)
			{
				return 0;
			}
			return -1;
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count < 0 || startIndex > source.Length - count)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		if (options == CompareOptions.OrdinalIgnoreCase)
		{
			return source.IndexOf(value, startIndex, count, StringComparison.OrdinalIgnoreCase);
		}
		if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)) != 0 && options != CompareOptions.Ordinal)
		{
			throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
		}
		return internal_index_switch(source, startIndex, count, value, options, first: true);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the last occurrence within the entire source string.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within <paramref name="source" />; otherwise, -1.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	public virtual int LastIndexOf(string source, char value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return LastIndexOf(source, value, source.Length - 1, source.Length, CompareOptions.None);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the last occurrence within the entire source string.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within <paramref name="source" />; otherwise, -1.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	public virtual int LastIndexOf(string source, string value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return LastIndexOf(source, value, source.Length - 1, source.Length, CompareOptions.None);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the last occurrence within the entire source string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within <paramref name="source" />, using the specified comparison options; otherwise, -1.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int LastIndexOf(string source, char value, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return LastIndexOf(source, value, source.Length - 1, source.Length, options);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the last occurrence within the entire source string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within <paramref name="source" />, using the specified comparison options; otherwise, -1.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int LastIndexOf(string source, string value, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return LastIndexOf(source, value, source.Length - 1, source.Length, options);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the last occurrence within the section of the source string that extends from the beginning of the string to the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that extends from the beginning of <paramref name="source" /> to <paramref name="startIndex" />; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />. </exception>
	public virtual int LastIndexOf(string source, char value, int startIndex)
	{
		return LastIndexOf(source, value, startIndex, startIndex + 1, CompareOptions.None);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the last occurrence within the section of the source string that extends from the beginning of the string to the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that extends from the beginning of <paramref name="source" /> to <paramref name="startIndex" />; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />. </exception>
	public virtual int LastIndexOf(string source, string value, int startIndex)
	{
		return LastIndexOf(source, value, startIndex, startIndex + 1, CompareOptions.None);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the last occurrence within the section of the source string that extends from the beginning of the string to the specified index using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that extends from the beginning of <paramref name="source" /> to <paramref name="startIndex" />, using the specified comparison options; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int LastIndexOf(string source, char value, int startIndex, CompareOptions options)
	{
		return LastIndexOf(source, value, startIndex, startIndex + 1, options);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the last occurrence within the section of the source string that extends from the beginning of the string to the specified index using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that extends from the beginning of <paramref name="source" /> to <paramref name="startIndex" />, using the specified comparison options; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	public virtual int LastIndexOf(string source, string value, int startIndex, CompareOptions options)
	{
		return LastIndexOf(source, value, startIndex, startIndex + 1, options);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the last occurrence within the section of the source string that contains the specified number of elements and ends at the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that contains the number of elements specified by <paramref name="count" /> and that ends at <paramref name="startIndex" />; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="source" />. </exception>
	public virtual int LastIndexOf(string source, char value, int startIndex, int count)
	{
		return LastIndexOf(source, value, startIndex, count, CompareOptions.None);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the last occurrence within the section of the source string that contains the specified number of elements and ends at the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that contains the number of elements specified by <paramref name="count" /> and that ends at <paramref name="startIndex" />; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="source" />. </exception>
	public virtual int LastIndexOf(string source, string value, int startIndex, int count)
	{
		return LastIndexOf(source, value, startIndex, count, CompareOptions.None);
	}

	/// <summary>Searches for the specified character and returns the zero-based index of the last occurrence within the section of the source string that contains the specified number of elements and ends at the specified index using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that contains the number of elements specified by <paramref name="count" /> and that ends at <paramref name="startIndex" />, using the specified comparison options; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character.</returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The character to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="source" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	[SecuritySafeCritical]
	public virtual int LastIndexOf(string source, char value, int startIndex, int count, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)) != 0 && options != CompareOptions.Ordinal && options != CompareOptions.OrdinalIgnoreCase)
		{
			throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
		}
		if (source.Length == 0 && (startIndex == -1 || startIndex == 0))
		{
			return -1;
		}
		if (startIndex < 0 || startIndex > source.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (startIndex == source.Length)
		{
			startIndex--;
			if (count > 0)
			{
				count--;
			}
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		if (options == CompareOptions.OrdinalIgnoreCase)
		{
			return source.LastIndexOf(value.ToString(), startIndex, count, StringComparison.OrdinalIgnoreCase);
		}
		return internal_index_switch(source, startIndex, count, value, options, first: false);
	}

	/// <summary>Searches for the specified substring and returns the zero-based index of the last occurrence within the section of the source string that contains the specified number of elements and ends at the specified index using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" />, if found, within the section of <paramref name="source" /> that contains the number of elements specified by <paramref name="count" /> and that ends at <paramref name="startIndex" />, using the specified comparison options; otherwise, -1. Returns <paramref name="startIndex" /> if <paramref name="value" /> is an ignorable character. </returns>
	/// <param name="source">The string to search. </param>
	/// <param name="value">The string to locate within <paramref name="source" />. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <param name="options">A value that defines how <paramref name="source" /> and <paramref name="value" /> should be compared. <paramref name="options" /> is either the enumeration value <see cref="F:System.Globalization.CompareOptions.Ordinal" />, or a bitwise combination of one or more of the following values: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, and <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.-or- <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="source" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="source" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	[SecuritySafeCritical]
	public virtual int LastIndexOf(string source, string value, int startIndex, int count, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)) != 0 && options != CompareOptions.Ordinal && options != CompareOptions.OrdinalIgnoreCase)
		{
			throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
		}
		if (source.Length == 0 && (startIndex == -1 || startIndex == 0))
		{
			if (value.Length != 0)
			{
				return -1;
			}
			return 0;
		}
		if (startIndex < 0 || startIndex > source.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (startIndex == source.Length)
		{
			startIndex--;
			if (count > 0)
			{
				count--;
			}
			if (value.Length == 0 && count >= 0 && startIndex - count + 1 >= 0)
			{
				return startIndex;
			}
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		if (options == CompareOptions.OrdinalIgnoreCase)
		{
			return source.LastIndexOf(value, startIndex, count, StringComparison.OrdinalIgnoreCase);
		}
		return internal_index_switch(source, startIndex, count, value, options, first: false);
	}

	/// <summary>Gets a <see cref="T:System.Globalization.SortKey" /> object for the specified string using the specified <see cref="T:System.Globalization.CompareOptions" /> value.</summary>
	/// <returns>The <see cref="T:System.Globalization.SortKey" /> object that contains the sort key for the specified string.</returns>
	/// <param name="source">The string for which a <see cref="T:System.Globalization.SortKey" /> object is obtained. </param>
	/// <param name="options">A bitwise combination of one or more of the following enumeration values that define how the sort key is calculated: <see cref="F:System.Globalization.CompareOptions.IgnoreCase" />, <see cref="F:System.Globalization.CompareOptions.IgnoreSymbols" />, <see cref="F:System.Globalization.CompareOptions.IgnoreNonSpace" />, <see cref="F:System.Globalization.CompareOptions.IgnoreWidth" />, <see cref="F:System.Globalization.CompareOptions.IgnoreKanaType" />, and <see cref="F:System.Globalization.CompareOptions.StringSort" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> contains an invalid <see cref="T:System.Globalization.CompareOptions" /> value. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public virtual SortKey GetSortKey(string source, CompareOptions options)
	{
		return CreateSortKey(source, options);
	}

	/// <summary>Gets the sort key for the specified string.</summary>
	/// <returns>The <see cref="T:System.Globalization.SortKey" /> object that contains the sort key for the specified string.</returns>
	/// <param name="source">The string for which a <see cref="T:System.Globalization.SortKey" /> object is obtained. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public virtual SortKey GetSortKey(string source)
	{
		return CreateSortKey(source, CompareOptions.None);
	}

	[SecuritySafeCritical]
	private SortKey CreateSortKey(string source, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.StringSort)) != 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
		}
		if (string.IsNullOrEmpty(source))
		{
			source = "\0";
		}
		return CreateSortKeyCore(source, options);
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Globalization.CompareInfo" /> object.</summary>
	/// <returns>true if the specified object is equal to the current <see cref="T:System.Globalization.CompareInfo" />; otherwise, false.</returns>
	/// <param name="value">The object to compare with the current <see cref="T:System.Globalization.CompareInfo" />. </param>
	public override bool Equals(object value)
	{
		if (value is CompareInfo compareInfo)
		{
			return Name == compareInfo.Name;
		}
		return false;
	}

	/// <summary>Serves as a hash function for the current <see cref="T:System.Globalization.CompareInfo" /> for hashing algorithms and data structures, such as a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Globalization.CompareInfo" />.</returns>
	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public virtual int GetHashCode(string source, CompareOptions options)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return options switch
		{
			CompareOptions.Ordinal => source.GetHashCode(), 
			CompareOptions.OrdinalIgnoreCase => TextInfo.GetHashCodeOrdinalIgnoreCase(source), 
			_ => GetHashCodeOfString(source, options, forceRandomizedHashing: false, 0L), 
		};
	}

	internal int GetHashCodeOfString(string source, CompareOptions options)
	{
		return GetHashCodeOfString(source, options, forceRandomizedHashing: false, 0L);
	}

	[SecuritySafeCritical]
	internal int GetHashCodeOfString(string source, CompareOptions options, bool forceRandomizedHashing, long additionalEntropy)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)) != 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid."), "options");
		}
		if (source.Length == 0)
		{
			return 0;
		}
		return GetSortKey(source, options).GetHashCode();
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Globalization.CompareInfo" /> object.</summary>
	/// <returns>A string that represents the current <see cref="T:System.Globalization.CompareInfo" /> object.</returns>
	public override string ToString()
	{
		return "CompareInfo - " + Name;
	}

	private SimpleCollator GetCollator()
	{
		if (collator != null)
		{
			return collator;
		}
		if (collators == null)
		{
			Interlocked.CompareExchange(ref collators, new Dictionary<string, SimpleCollator>(StringComparer.Ordinal), null);
		}
		lock (collators)
		{
			if (!collators.TryGetValue(m_sortName, out collator))
			{
				collator = new SimpleCollator(CultureInfo.GetCultureInfo(m_name));
				collators[m_sortName] = collator;
			}
		}
		return collator;
	}

	private SortKey CreateSortKeyCore(string source, CompareOptions options)
	{
		if (UseManagedCollation)
		{
			return GetCollator().GetSortKey(source, options);
		}
		SortKey sortKey = new SortKey(culture, source, options);
		assign_sortkey(sortKey, source, options);
		return sortKey;
	}

	private int internal_index_switch(string s, int sindex, int count, char c, CompareOptions opt, bool first)
	{
		if (opt == CompareOptions.Ordinal && first)
		{
			return s.IndexOfUnchecked(c, sindex, count);
		}
		if (!UseManagedCollation)
		{
			return internal_index(s, sindex, count, c, opt, first);
		}
		return internal_index_managed(s, sindex, count, c, opt, first);
	}

	private int internal_index_switch(string s1, int sindex, int count, string s2, CompareOptions opt, bool first)
	{
		if (opt == CompareOptions.Ordinal && first)
		{
			return s1.IndexOfUnchecked(s2, sindex, count);
		}
		if (!UseManagedCollation)
		{
			return internal_index(s1, sindex, count, s2, opt, first);
		}
		return internal_index_managed(s1, sindex, count, s2, opt, first);
	}

	private int internal_compare_switch(string str1, int offset1, int length1, string str2, int offset2, int length2, CompareOptions options)
	{
		if (!UseManagedCollation)
		{
			return internal_compare(str1, offset1, length1, str2, offset2, length2, options);
		}
		return internal_compare_managed(str1, offset1, length1, str2, offset2, length2, options);
	}

	private int internal_compare_managed(string str1, int offset1, int length1, string str2, int offset2, int length2, CompareOptions options)
	{
		return GetCollator().Compare(str1, offset1, length1, str2, offset2, length2, options);
	}

	private int internal_index_managed(string s, int sindex, int count, char c, CompareOptions opt, bool first)
	{
		if (!first)
		{
			return GetCollator().LastIndexOf(s, c, sindex, count, opt);
		}
		return GetCollator().IndexOf(s, c, sindex, count, opt);
	}

	private int internal_index_managed(string s1, int sindex, int count, string s2, CompareOptions opt, bool first)
	{
		if (!first)
		{
			return GetCollator().LastIndexOf(s1, s2, sindex, count, opt);
		}
		return GetCollator().IndexOf(s1, s2, sindex, count, opt);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void assign_sortkey(object key, string source, CompareOptions options);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int internal_compare(string str1, int offset1, int length1, string str2, int offset2, int length2, CompareOptions options);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int internal_index(string source, int sindex, int count, char value, CompareOptions options, bool first);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int internal_index(string source, int sindex, int count, string value, CompareOptions options, bool first);

	internal CompareInfo()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
