using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Globalization;

/// <summary>Contains information about the country/region.</summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public class RegionInfo
{
	private static RegionInfo currentRegion;

	private int regionId;

	private string iso2Name;

	private string iso3Name;

	private string win3Name;

	private string englishName;

	private string nativeName;

	private string currencySymbol;

	private string isoCurrencySymbol;

	private string currencyEnglishName;

	private string currencyNativeName;

	/// <summary>Gets the <see cref="T:System.Globalization.RegionInfo" /> that represents the country/region used by the current thread.</summary>
	/// <returns>The <see cref="T:System.Globalization.RegionInfo" /> that represents the country/region used by the current thread.</returns>
	public static RegionInfo CurrentRegion
	{
		get
		{
			RegionInfo regionInfo = currentRegion;
			if (regionInfo == null)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				if (currentCulture != null)
				{
					regionInfo = new RegionInfo(currentCulture);
				}
				if (Interlocked.CompareExchange(ref currentRegion, regionInfo, null) != null)
				{
					regionInfo = currentRegion;
				}
			}
			return regionInfo;
		}
	}

	/// <summary>Gets the name, in English, of the currency used in the country/region.</summary>
	/// <returns>The name, in English, of the currency used in the country/region.</returns>
	[ComVisible(false)]
	public virtual string CurrencyEnglishName => currencyEnglishName;

	/// <summary>Gets the currency symbol associated with the country/region.</summary>
	/// <returns>The currency symbol associated with the country/region.</returns>
	public virtual string CurrencySymbol => currencySymbol;

	/// <summary>Gets the full name of the country/region in the language of the localized version of .NET Framework.</summary>
	/// <returns>The full name of the country/region in the language of the localized version of .NET Framework.</returns>
	[MonoTODO("DisplayName currently only returns the EnglishName")]
	public virtual string DisplayName => englishName;

	/// <summary>Gets the full name of the country/region in English.</summary>
	/// <returns>The full name of the country/region in English.</returns>
	public virtual string EnglishName => englishName;

	/// <summary>Gets a unique identification number for a geographical region, country, city, or location.</summary>
	/// <returns>A 32-bit signed number that uniquely identifies a geographical location.</returns>
	[ComVisible(false)]
	public virtual int GeoId => regionId;

	/// <summary>Gets a value indicating whether the country/region uses the metric system for measurements.</summary>
	/// <returns>true if the country/region uses the metric system for measurements; otherwise, false.</returns>
	public virtual bool IsMetric
	{
		get
		{
			string text = iso2Name;
			if (text == "US" || text == "UK")
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>Gets the three-character ISO 4217 currency symbol associated with the country/region.</summary>
	/// <returns>The three-character ISO 4217 currency symbol associated with the country/region.</returns>
	public virtual string ISOCurrencySymbol => isoCurrencySymbol;

	/// <summary>Gets the name of a country/region formatted in the native language of the country/region.</summary>
	/// <returns>The native name of the country/region formatted in the language associated with the ISO 3166 country/region code. </returns>
	[ComVisible(false)]
	public virtual string NativeName => nativeName;

	/// <summary>Gets the name of the currency used in the country/region, formatted in the native language of the country/region. </summary>
	/// <returns>The native name of the currency used in the country/region, formatted in the language associated with the ISO 3166 country/region code. </returns>
	[ComVisible(false)]
	public virtual string CurrencyNativeName => currencyNativeName;

	/// <summary>Gets the name or ISO 3166 two-letter country/region code for the current <see cref="T:System.Globalization.RegionInfo" /> object.</summary>
	/// <returns>The value specified by the <paramref name="name" /> parameter of the <see cref="M:System.Globalization.RegionInfo.#ctor(System.String)" /> constructor. The return value is in uppercase.-or-The two-letter code defined in ISO 3166 for the country/region specified by the <paramref name="culture" /> parameter of the <see cref="M:System.Globalization.RegionInfo.#ctor(System.Int32)" /> constructor. The return value is in uppercase.</returns>
	public virtual string Name => iso2Name;

	/// <summary>Gets the three-letter code defined in ISO 3166 for the country/region.</summary>
	/// <returns>The three-letter code defined in ISO 3166 for the country/region.</returns>
	public virtual string ThreeLetterISORegionName => iso3Name;

	/// <summary>Gets the three-letter code assigned by Windows to the country/region represented by this <see cref="T:System.Globalization.RegionInfo" />.</summary>
	/// <returns>The three-letter code assigned by Windows to the country/region represented by this <see cref="T:System.Globalization.RegionInfo" />.</returns>
	public virtual string ThreeLetterWindowsRegionName => win3Name;

	/// <summary>Gets the two-letter code defined in ISO 3166 for the country/region.</summary>
	/// <returns>The two-letter code defined in ISO 3166 for the country/region.</returns>
	public virtual string TwoLetterISORegionName => iso2Name;

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.RegionInfo" /> class based on the country/region associated with the specified culture identifier.</summary>
	/// <param name="culture">A culture identifier. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="culture" /> specifies either an invariant, custom, or neutral culture.</exception>
	public RegionInfo(int culture)
	{
		if (!GetByTerritory(CultureInfo.GetCultureInfo(culture)))
		{
			throw new ArgumentException(string.Format("Region ID {0} (0x{0:X4}) is not a supported region.", culture), "culture");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.RegionInfo" /> class based on the country/region or specific culture, specified by name.</summary>
	/// <param name="name">A string that contains a two-letter code defined in ISO 3166 for country/region.-or-A string that contains the culture name for a specific culture, custom culture, or Windows-only culture. If the culture name is not in RFC 4646 format, your application should specify the entire culture name instead of just the country/region. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not a valid country/region name or specific culture name.</exception>
	public RegionInfo(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException();
		}
		if (construct_internal_region_from_name(name.ToUpperInvariant()) || GetByTerritory(CultureInfo.GetCultureInfo(name)))
		{
			return;
		}
		throw new ArgumentException($"Region name {name} is not supported.", "name");
	}

	private RegionInfo(CultureInfo ci)
	{
		if (ci.LCID == 127)
		{
			regionId = 244;
			iso2Name = "IV";
			iso3Name = "ivc";
			win3Name = "IVC";
			nativeName = (englishName = "Invariant Country");
			currencySymbol = "¤";
			isoCurrencySymbol = "XDR";
			currencyEnglishName = (currencyNativeName = "International Monetary Fund");
		}
		else
		{
			if (ci.Territory == null)
			{
				throw new NotImplementedException("Neutral region info");
			}
			construct_internal_region_from_name(ci.Territory.ToUpperInvariant());
		}
	}

	private bool GetByTerritory(CultureInfo ci)
	{
		if (ci == null)
		{
			throw new Exception("INTERNAL ERROR: should not happen.");
		}
		if (ci.IsNeutralCulture || ci.Territory == null)
		{
			return false;
		}
		return construct_internal_region_from_name(ci.Territory.ToUpperInvariant());
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern bool construct_internal_region_from_name(string name);

	/// <summary>Determines whether the specified object is the same instance as the current <see cref="T:System.Globalization.RegionInfo" />.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is a <see cref="T:System.Globalization.RegionInfo" /> object and its <see cref="P:System.Globalization.RegionInfo.Name" /> property is the same as the <see cref="P:System.Globalization.RegionInfo.Name" /> property of the current <see cref="T:System.Globalization.RegionInfo" /> object; otherwise, false.</returns>
	/// <param name="value">The object to compare with the current <see cref="T:System.Globalization.RegionInfo" />. </param>
	public override bool Equals(object value)
	{
		if (value is RegionInfo regionInfo)
		{
			return Name == regionInfo.Name;
		}
		return false;
	}

	/// <summary>Serves as a hash function for the current <see cref="T:System.Globalization.RegionInfo" />, suitable for hashing algorithms and data structures, such as a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Globalization.RegionInfo" />.</returns>
	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	/// <summary>Returns a string containing the culture name or ISO 3166 two-letter country/region codes specified for the current <see cref="T:System.Globalization.RegionInfo" />.</summary>
	/// <returns>A string containing the culture name or ISO 3166 two-letter country/region codes defined for the current <see cref="T:System.Globalization.RegionInfo" />.</returns>
	public override string ToString()
	{
		return Name;
	}

	internal static void ClearCachedData()
	{
		currentRegion = null;
	}
}
