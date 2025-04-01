using System.Collections.Specialized;
using System.Configuration;

namespace System.Runtime.Serialization;

internal static class AppSettings
{
	internal const string MaxMimePartsAppSettingsString = "microsoft:xmldictionaryreader:maxmimeparts";

	private const int DefaultMaxMimeParts = 1000;

	private static int maxMimeParts;

	private static volatile bool settingsInitalized = false;

	private static object appSettingsLock = new object();

	internal static int MaxMimeParts
	{
		get
		{
			EnsureSettingsLoaded();
			return maxMimeParts;
		}
	}

	private static void EnsureSettingsLoaded()
	{
		if (settingsInitalized)
		{
			return;
		}
		lock (appSettingsLock)
		{
			if (settingsInitalized)
			{
				return;
			}
			NameValueCollection nameValueCollection = null;
			try
			{
				nameValueCollection = ConfigurationManager.AppSettings;
			}
			catch (ConfigurationErrorsException)
			{
			}
			finally
			{
				if (nameValueCollection == null || !int.TryParse(nameValueCollection["microsoft:xmldictionaryreader:maxmimeparts"], out maxMimeParts))
				{
					maxMimeParts = 1000;
				}
				settingsInitalized = true;
			}
		}
	}
}
