using System.Globalization;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class PlatformCulture
{
	public static CultureInfo Value
	{
		get
		{
			string wPF_UILanguage = SR.WPF_UILanguage;
			Invariant.Assert(!string.IsNullOrEmpty(wPF_UILanguage), "No UILanguage was specified in stringtable.");
			return new CultureInfo(wPF_UILanguage);
		}
	}
}
