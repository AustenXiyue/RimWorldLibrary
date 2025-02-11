using System.Reflection;

namespace System;

internal static class AppContextDefaultValues
{
	public static void PopulateDefaultValues()
	{
		ParseTargetFrameworkName(out var identifier, out var profile, out var version);
		PopulateDefaultValuesPartial(identifier, profile, version);
	}

	private static void ParseTargetFrameworkName(out string identifier, out string profile, out int version)
	{
		string text = GetTargetFrameworkMoniker();
		if (text == null)
		{
			text = ".NETCoreApp,Version=v3.0";
		}
		if (!TryParseFrameworkName(text, out identifier, out version, out profile))
		{
			identifier = string.Empty;
		}
	}

	private static string GetTargetFrameworkMoniker()
	{
		try
		{
			object obj = typeof(AppDomain).GetProperty("SetupInformation")?.GetValue(AppDomain.CurrentDomain);
			PropertyInfo propertyInfo = Type.GetType("System.AppDomainSetup")?.GetProperty("TargetFrameworkName");
			return (obj != null) ? (propertyInfo?.GetValue(obj) as string) : null;
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static bool TryParseFrameworkName(string frameworkName, out string identifier, out int version, out string profile)
	{
		identifier = (profile = string.Empty);
		version = 0;
		if (frameworkName == null || frameworkName.Length == 0)
		{
			return false;
		}
		string[] array = frameworkName.Split(',');
		version = 0;
		if (array.Length < 2 || array.Length > 3)
		{
			return false;
		}
		identifier = array[0].Trim();
		if (identifier.Length == 0)
		{
			return false;
		}
		bool flag = false;
		profile = null;
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			if (array2.Length != 2)
			{
				return false;
			}
			string text = array2[0].Trim();
			string text2 = array2[1].Trim();
			if (text.Equals("Version", StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
				if (text2.Length > 0 && (text2[0] == 'v' || text2[0] == 'V'))
				{
					text2 = text2.Substring(1);
				}
				Version version2 = new Version(text2);
				version = version2.Major * 10000;
				if (version2.Minor > 0)
				{
					version += version2.Minor * 100;
				}
				if (version2.Build > 0)
				{
					version += version2.Build;
				}
			}
			else
			{
				if (!text.Equals("Profile", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				if (!string.IsNullOrEmpty(text2))
				{
					profile = text2;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	private static void PopulateDefaultValuesPartial(string platformIdentifier, string profile, int targetFrameworkVersion)
	{
		LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", initialValue: true);
		if (!(platformIdentifier == ".NETFramework"))
		{
			if (platformIdentifier == ".NETCoreApp")
			{
				InitializeNetFxSwitchDefaultsForNetCoreRuntime();
			}
			return;
		}
		if (targetFrameworkVersion <= 40502)
		{
			LocalAppContext.DefineSwitchDefault("Switch.MS.Internal.DoNotApplyLayoutRoundingToMarginsAndBorderThickness", initialValue: true);
		}
		if (targetFrameworkVersion <= 40602)
		{
			LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Controls.Grid.StarDefinitionsCanExceedAvailableSpace", initialValue: true);
		}
		if (targetFrameworkVersion <= 40700)
		{
			LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Controls.TabControl.SelectionPropertiesCanLagBehindSelectionChangedEvent", initialValue: true);
		}
		if (targetFrameworkVersion <= 40701)
		{
			LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Data.DoNotUseFollowParentWhenBindingToADODataRelation", initialValue: true);
		}
		if (40000 <= targetFrameworkVersion && targetFrameworkVersion <= 40702)
		{
			LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Data.Binding.IListIndexerHidesCustomIndexer", initialValue: true);
		}
	}

	private static void InitializeNetFxSwitchDefaultsForNetCoreRuntime()
	{
		LocalAppContext.DefineSwitchDefault("Switch.MS.Internal.DoNotApplyLayoutRoundingToMarginsAndBorderThickness", initialValue: false);
		LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Controls.Grid.StarDefinitionsCanExceedAvailableSpace", initialValue: false);
		LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Controls.TabControl.SelectionPropertiesCanLagBehindSelectionChangedEvent", initialValue: false);
		LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Data.DoNotUseFollowParentWhenBindingToADODataRelation", initialValue: false);
		LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Data.Binding.IListIndexerHidesCustomIndexer", initialValue: false);
		LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Baml2006.AppendLocalAssemblyVersionForSourceUri", initialValue: false);
		LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Controls.KeyboardNavigationFromHyperlinkInItemsControlIsNotRelativeToFocusedElement", initialValue: false);
		LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Automation.Peers.ItemAutomationPeerKeepsItsItemAlive", initialValue: false);
	}
}
