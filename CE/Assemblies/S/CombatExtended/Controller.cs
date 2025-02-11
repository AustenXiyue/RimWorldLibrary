using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CombatExtended.Compatibility;
using CombatExtended.HarmonyCE;
using CombatExtended.Loader;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Controller : Mod
{
	public static List<ISettingsCE> settingList = new List<ISettingsCE>();

	public static Settings settings;

	public static Controller instance;

	public static ModContentPack content;

	private static Patches patches;

	private Vector2 scrollPosition;

	public override void DoSettingsWindowContents(Rect inRect)
	{
		Rect rect = inRect.ContractedBy(20f);
		rect.height = 800f;
		rect.x += 10f;
		Widgets.BeginScrollView(inRect, ref scrollPosition, rect);
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.ColumnWidth = (rect.width - 17f) / 2f;
		listing_Standard.Begin(rect);
		foreach (ISettingsCE setting in settingList)
		{
			setting.DoWindowContents(listing_Standard);
		}
		listing_Standard.End();
		Widgets.EndScrollView();
	}

	public Controller(ModContentPack content)
		: base(content)
	{
		patches = new Patches();
		instance = this;
		Controller.content = content;
		settings = GetSettings<Settings>();
		settingList.Add(settings);
		PostLoad();
	}

	public void PostLoad()
	{
		HarmonyBase.InitPatches();
		Queue<Assembly> queue = new Queue<Assembly>(content.assemblies.loadedAssemblies);
		List<IModPart> list = new List<IModPart>();
		while (queue.Any())
		{
			Assembly assembly = queue.Dequeue();
			foreach (Type item3 in from x in assembly.GetTypes()
				where typeof(IModPart).IsAssignableFrom(x) && !x.IsAbstract
				select x)
			{
				IModPart item = (IModPart)item3.GetConstructor(new Type[0]).Invoke(new object[0]);
				list.Add(item);
			}
		}
		foreach (IModPart item4 in list)
		{
			Log.Message("CE: Loading Mod Part");
			Type settingsType = item4.GetSettingsType();
			ISettingsCE item2 = null;
			if (settingsType != null)
			{
				item2 = ((!typeof(ModSettings).IsAssignableFrom(settingsType)) ? ((ISettingsCE)settingsType.GetConstructor(new Type[0]).Invoke(new object[0])) : ((ISettingsCE)typeof(Controller).GetMethod("GetSettings").MakeGenericMethod(settingsType).Invoke(instance, null)));
				settingList.Add(item2);
			}
			item4.PostLoad(content, item2);
		}
		LongEventHandler.QueueLongEvent(LoadoutPropertiesExtension.Reset, "CE_LongEvent_LoadoutProperties", doAsynchronously: false, null);
		LongEventHandler.QueueLongEvent(AmmoInjector.Inject, "CE_LongEvent_AmmoInjector", doAsynchronously: false, null);
		LongEventHandler.QueueLongEvent(BoundsInjector.Inject, "CE_LongEvent_BoundingBoxes", doAsynchronously: false, null);
		LongEventHandler.QueueLongEvent(DefUtility.Initialize, "CE_LongEvent_BoundingBoxes", doAsynchronously: false, null);
		Log.Message("Combat Extended :: initialized");
		if (settings.ShowTutorialPopup && !Prefs.AdaptiveTrainingEnabled)
		{
			LongEventHandler.QueueLongEvent(DoTutorialPopup, "CE_LongEvent_TutorialPopup", doAsynchronously: false, null);
		}
		LongEventHandler.QueueLongEvent(patches.Install, "CE_LongEvent_CompatibilityPatches", doAsynchronously: false, null);
	}

	public override string SettingsCategory()
	{
		return "Combat Extended";
	}

	private static void DoTutorialPopup()
	{
		Action buttonBAction = delegate
		{
			Prefs.AdaptiveTrainingEnabled = true;
			settings.ShowTutorialPopup = false;
			settings.Write();
		};
		Action buttonAAction = delegate
		{
			settings.ShowTutorialPopup = false;
			settings.Write();
		};
		Dialog_MessageBox window = new Dialog_MessageBox("CE_EnableTutorText".Translate(), "CE_EnableTutorDisable".Translate(), buttonAAction, "CE_EnableTutorEnable".Translate(), buttonBAction, null, buttonADestructive: true);
		Find.WindowStack.Add(window);
	}
}
