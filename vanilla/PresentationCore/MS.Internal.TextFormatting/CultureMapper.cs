using System;
using System.Globalization;
using System.Windows.Markup;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

internal static class CultureMapper
{
	private class CachedCultureMap
	{
		private CultureInfo _originalCulture;

		private CultureInfo _specificCulture;

		public CultureInfo OriginalCulture => _originalCulture;

		public CultureInfo SpecificCulture => _specificCulture;

		public CachedCultureMap(CultureInfo originalCulture, CultureInfo specificCulture)
		{
			_originalCulture = originalCulture;
			_specificCulture = specificCulture;
		}
	}

	private static CachedCultureMap _cachedCultureMap;

	public static CultureInfo GetSpecificCulture(CultureInfo runCulture)
	{
		CultureInfo cultureInfo = TypeConverterHelper.InvariantEnglishUS;
		if (runCulture != null)
		{
			CachedCultureMap cachedCultureMap = _cachedCultureMap;
			if (cachedCultureMap != null && cachedCultureMap.OriginalCulture == runCulture)
			{
				return cachedCultureMap.SpecificCulture;
			}
			if (runCulture != CultureInfo.InvariantCulture)
			{
				if (!runCulture.IsNeutralCulture)
				{
					cultureInfo = runCulture;
				}
				else
				{
					string name = runCulture.Name;
					if (!string.IsNullOrEmpty(name))
					{
						try
						{
							cultureInfo = SafeSecurityHelper.GetCultureInfoByIetfLanguageTag(CultureInfo.CreateSpecificCulture(name).IetfLanguageTag);
						}
						catch (ArgumentException)
						{
							cultureInfo = TypeConverterHelper.InvariantEnglishUS;
						}
					}
				}
			}
			_cachedCultureMap = new CachedCultureMap(runCulture, cultureInfo);
		}
		return cultureInfo;
	}
}
