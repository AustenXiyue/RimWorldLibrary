using CombatExtended.Loader;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Settings : ModSettings, ISettingsCE
{
	private bool bipodMechanics = true;

	private bool autosetup = true;

	private bool showCasings = true;

	private bool createCasingsFilth = true;

	private bool recoilAnim = true;

	private bool showTaunts = true;

	private bool allowMeleeHunting = false;

	private bool smokeEffects = true;

	private bool mergeExplosions = true;

	private bool turretsBreakShields = true;

	private bool showBackpacks = true;

	private bool showTacticalVests = true;

	private bool genericammo = false;

	private bool partialstats = true;

	private bool enableExtraEffects = true;

	private bool showExtraTooltips = false;

	private bool showExtraStats = false;

	private bool fragmentsFromWalls = false;

	private bool fasterRepeatShots = true;

	public bool ShowTutorialPopup = true;

	private bool enableAmmoSystem = true;

	private bool rightClickAmmoSelect = true;

	private bool autoReloadOnChangeAmmo = true;

	private bool autoTakeAmmo = true;

	private bool showCaliberOnGuns = true;

	private bool reuseNeolithicProjectiles = true;

	private bool realisticCookOff = true;

	private bool debuggingMode = false;

	private bool debugVerbose = false;

	private bool debugDrawPartialLoSChecks = false;

	private bool debugEnableInventoryValidation = false;

	private bool debugDrawTargetCoverChecks = false;

	private bool debugGenClosetPawn = false;

	private bool debugMuzzleFlash = false;

	private bool debugShowTreeCollisionChance = false;

	private bool debugShowSuppressionBuildup = false;

	private bool debugDrawInterceptChecks = false;

	private bool debugDisplayDangerBuildup = false;

	private bool debugDisplayCellCoverRating = false;

	private bool debugDisplayAttritionInfo = false;

	private bool debugAutopatcherLogger = false;

	private bool enableApparelAutopatcher = false;

	private bool enableWeaponAutopatcher = false;

	private bool enableWeaponToughnessAutopatcher = true;

	private bool enableRaceAutopatcher = true;

	private bool enablePawnKindAutopatcher = true;

	private bool lastAmmoSystemStatus;

	public bool patchArmorDamage = true;

	public bool ShowCasings => showCasings;

	public bool BipodMechanics => bipodMechanics;

	public bool GenericAmmo => genericammo;

	public bool AutoSetUp => autosetup;

	public bool ShowTaunts => showTaunts;

	public bool AllowMeleeHunting => allowMeleeHunting;

	public bool SmokeEffects => smokeEffects;

	public bool MergeExplosions => mergeExplosions;

	public bool TurretsBreakShields => turretsBreakShields;

	public bool ShowBackpacks => showBackpacks;

	public bool ShowTacticalVests => showTacticalVests;

	public bool PartialStat => partialstats;

	public bool EnableExtraEffects => enableExtraEffects;

	public bool ShowExtraTooltips => showExtraTooltips;

	public bool ShowExtraStats => showExtraStats;

	public bool EnableAmmoSystem => enableAmmoSystem;

	public bool RightClickAmmoSelect => rightClickAmmoSelect;

	public bool AutoReloadOnChangeAmmo => autoReloadOnChangeAmmo;

	public bool AutoTakeAmmo => autoTakeAmmo;

	public bool ShowCaliberOnGuns => showCaliberOnGuns;

	public bool ReuseNeolithicProjectiles => reuseNeolithicProjectiles;

	public bool RealisticCookOff => realisticCookOff;

	public bool DebuggingMode => debuggingMode;

	public bool DebugVerbose => debugVerbose;

	public bool DebugDrawInterceptChecks => debugDrawInterceptChecks && debuggingMode;

	public bool DebugDrawPartialLoSChecks => debugDrawPartialLoSChecks && debuggingMode;

	public bool DebugEnableInventoryValidation => debugEnableInventoryValidation && debuggingMode;

	public bool DebugDrawTargetCoverChecks => debugDrawTargetCoverChecks && debuggingMode;

	public bool DebugMuzzleFlash => debugMuzzleFlash && debuggingMode;

	public bool DebugShowTreeCollisionChance => debugShowTreeCollisionChance && debuggingMode;

	public bool DebugShowSuppressionBuildup => debugShowSuppressionBuildup && debuggingMode;

	public bool DebugGenClosetPawn => debugGenClosetPawn && debuggingMode;

	public bool DebugDisplayDangerBuildup => debugDisplayDangerBuildup && debuggingMode;

	public bool DebugDisplayCellCoverRating => debugDisplayCellCoverRating && debuggingMode;

	public bool DebugDisplayAttritionInfo => debugDisplayAttritionInfo && debuggingMode;

	public bool DebugAutopatcherLogger => debugAutopatcherLogger;

	public bool EnableApparelAutopatcher => enableApparelAutopatcher;

	public bool EnableWeaponAutopatcher => enableWeaponAutopatcher;

	public bool EnableWeaponToughnessAutopatcher => enableWeaponToughnessAutopatcher;

	public bool EnableRaceAutopatcher => enableRaceAutopatcher;

	public bool EnablePawnKindAutopatcher => enablePawnKindAutopatcher;

	public bool FragmentsFromWalls => fragmentsFromWalls;

	public bool FasterRepeatShots => fasterRepeatShots;

	public bool CreateCasingsFilth => createCasingsFilth;

	public bool RecoilAnim => recoilAnim;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref showCasings, "showCasings", defaultValue: true);
		Scribe_Values.Look(ref createCasingsFilth, "createCasingsFilth", defaultValue: true);
		Scribe_Values.Look(ref recoilAnim, "recoilAnim", defaultValue: true);
		Scribe_Values.Look(ref showTaunts, "showTaunts", defaultValue: true);
		Scribe_Values.Look(ref allowMeleeHunting, "allowMeleeHunting", defaultValue: false);
		Scribe_Values.Look(ref smokeEffects, "smokeEffects", defaultValue: true);
		Scribe_Values.Look(ref mergeExplosions, "mergeExplosions", defaultValue: true);
		Scribe_Values.Look(ref turretsBreakShields, "turretsBreakShields", defaultValue: true);
		Scribe_Values.Look(ref showBackpacks, "showBackpacks", defaultValue: true);
		Scribe_Values.Look(ref showTacticalVests, "showTacticalVests", defaultValue: true);
		Scribe_Values.Look(ref partialstats, "PartialArmor", defaultValue: true);
		Scribe_Values.Look(ref enableExtraEffects, "enableExtraEffects", defaultValue: true);
		Scribe_Values.Look(ref showExtraTooltips, "showExtraTooltips", defaultValue: false);
		Scribe_Values.Look(ref showExtraStats, "showExtraStats", defaultValue: false);
		Scribe_Values.Look(ref debugAutopatcherLogger, "debugAutopatcherLogger", defaultValue: false);
		Scribe_Values.Look(ref enableWeaponAutopatcher, "enableWeaponAutopatcher", defaultValue: false);
		Scribe_Values.Look(ref enableWeaponToughnessAutopatcher, "enableWeaponToughnessAutopatcher", defaultValue: true);
		Scribe_Values.Look(ref enableApparelAutopatcher, "enableApparelAutopatcher", defaultValue: false);
		Scribe_Values.Look(ref enableRaceAutopatcher, "enableRaceAutopatcher", defaultValue: true);
		Scribe_Values.Look(ref enablePawnKindAutopatcher, "enablePawnKindAutopatcher", defaultValue: true);
		Scribe_Values.Look(ref enableAmmoSystem, "enableAmmoSystem", defaultValue: true);
		Scribe_Values.Look(ref rightClickAmmoSelect, "rightClickAmmoSelect", defaultValue: true);
		Scribe_Values.Look(ref autoReloadOnChangeAmmo, "autoReloadOnChangeAmmo", defaultValue: true);
		Scribe_Values.Look(ref autoTakeAmmo, "autoTakeAmmo", defaultValue: true);
		Scribe_Values.Look(ref showCaliberOnGuns, "showCaliberOnGuns", defaultValue: true);
		Scribe_Values.Look(ref reuseNeolithicProjectiles, "reuseNeolithicProjectiles", defaultValue: true);
		Scribe_Values.Look(ref realisticCookOff, "realisticCookOff", defaultValue: true);
		Scribe_Values.Look(ref genericammo, "genericAmmo", defaultValue: false);
		Scribe_Values.Look(ref ShowTutorialPopup, "ShowTutorialPopup", defaultValue: true);
		Scribe_Values.Look(ref bipodMechanics, "bipodMechs", defaultValue: true);
		Scribe_Values.Look(ref autosetup, "autosetup", defaultValue: true);
		Scribe_Values.Look(ref fragmentsFromWalls, "fragmentsFromWalls", defaultValue: false);
		Scribe_Values.Look(ref fasterRepeatShots, "fasterRepeatShots", defaultValue: false);
		lastAmmoSystemStatus = enableAmmoSystem;
	}

	public void DoWindowContents(Listing_Standard list)
	{
		Text.Font = GameFont.Medium;
		list.Label("CE_Settings_HeaderGeneral".Translate());
		Text.Font = GameFont.Small;
		list.Gap();
		list.CheckboxLabeled("CE_Settings_PartialStats_Title".Translate(), ref partialstats, "CE_Settings_PartialStats_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_ShowCasings_Title".Translate(), ref showCasings, "CE_Settings_ShowCasings_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_СreateCasingsFilth_Title".Translate(), ref createCasingsFilth, "CE_Settings_СreateCasingsFilth_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_RecoilAnim_Title".Translate(), ref recoilAnim, "CE_Settings_RecoilAnim_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_ShowTaunts_Title".Translate(), ref showTaunts, "CE_Settings_ShowTaunts_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_AllowMeleeHunting_Title".Translate(), ref allowMeleeHunting, "CE_Settings_AllowMeleeHunting_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_SmokeEffects_Title".Translate(), ref smokeEffects, "CE_Settings_SmokeEffects_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_MergeExplosions_Title".Translate(), ref mergeExplosions, "CE_Settings_MergeExplosions_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_TurretsBreakShields_Title".Translate(), ref turretsBreakShields, "CE_Settings_TurretsBreakShields_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_ShowExtraTooltips_Title".Translate(), ref showExtraTooltips, "CE_Settings_ShowExtraTooltips_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_ShowExtraStats_Title".Translate(), ref showExtraStats, "CE_Settings_ShowExtraStats_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_FasterRepeatShots_Title".Translate(), ref fasterRepeatShots, "CE_Settings_FasterRepeatShots_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_EnableExtraEffects_Title".Translate(), ref enableExtraEffects, "CE_Settings_EnableExtraEffects_Desc".Translate());
		if (Current.Game == null)
		{
			list.CheckboxLabeled("CE_Settings_ShowBackpacks_Title".Translate(), ref showBackpacks, "CE_Settings_ShowBackpacks_Desc".Translate());
			list.CheckboxLabeled("CE_Settings_ShowWebbing_Title".Translate(), ref showTacticalVests, "CE_Settings_ShowWebbing_Desc".Translate());
			list.CheckboxLabeled("CE_Settings_FragmentsFromWalls_Title".Translate(), ref fragmentsFromWalls, "CE_Settings_FragmentsFromWalls_Desc".Translate());
		}
		else
		{
			list.GapLine();
			Text.Font = GameFont.Medium;
			list.Label("CE_Settings_MainMenuOnly_Title".Translate(), -1f, "CE_Settings_MainMenuOnly_Desc".Translate());
			Text.Font = GameFont.Small;
			list.Gap();
			list.Label("CE_Settings_ShowBackpacks_Title".Translate(), -1f, "CE_Settings_ShowBackpacks_Desc".Translate());
			list.Label("CE_Settings_ShowWebbing_Title".Translate(), -1f, "CE_Settings_ShowWebbing_Desc".Translate());
			list.Gap();
		}
		list.GapLine();
		Text.Font = GameFont.Medium;
		list.Label("CE_Settings_HeaderAutopatcher".Translate());
		Text.Font = GameFont.Small;
		list.Gap();
		list.CheckboxLabeled("CE_Settings_VerboseAutopatcher_Title".Translate(), ref debugAutopatcherLogger, "CE_Settings_VerboseAutopatcher_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_ApparelAutopatcher_Title".Translate(), ref enableApparelAutopatcher, "CE_Settings_ApparelAutopatcher_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_RaceAutopatcher_Title".Translate(), ref enableRaceAutopatcher, "CE_Settings_RaceAutopatcher_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_WeaponAutopatcher_Title".Translate(), ref enableWeaponAutopatcher, "CE_Settings_WeaponAutopatcher_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_ToughnessAutopatcher_Title".Translate(), ref enableWeaponToughnessAutopatcher, "CE_Settings_ToughnessAutopatcher_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_PawnkindAutopatcher_Title".Translate(), ref enablePawnKindAutopatcher, "CE_Settings_PawnkindAutopatcher_Desc".Translate());
		list.NewColumn();
		Text.Font = GameFont.Medium;
		list.Label("CE_Settings_HeaderAmmo".Translate());
		Text.Font = GameFont.Small;
		list.Gap();
		list.CheckboxLabeled("CE_Settings_EnableAmmoSystem_Title".Translate(), ref enableAmmoSystem, "CE_Settings_EnableAmmoSystem_Desc".Translate());
		list.GapLine();
		if (enableAmmoSystem)
		{
			list.CheckboxLabeled("CE_Settings_RightClickAmmoSelect_Title".Translate(), ref rightClickAmmoSelect, "CE_Settings_RightClickAmmoSelect_Desc".Translate());
			list.CheckboxLabeled("CE_Settings_AutoReloadOnChangeAmmo_Title".Translate(), ref autoReloadOnChangeAmmo, "CE_Settings_AutoReloadOnChangeAmmo_Desc".Translate());
			list.CheckboxLabeled("CE_Settings_AutoTakeAmmo_Title".Translate(), ref autoTakeAmmo, "CE_Settings_AutoTakeAmmo_Desc".Translate());
			list.CheckboxLabeled("CE_Settings_ShowCaliberOnGuns_Title".Translate(), ref showCaliberOnGuns, "CE_Settings_ShowCaliberOnGuns_Desc".Translate());
			list.CheckboxLabeled("CE_Settings_ReuseNeolithicProjectiles_Title".Translate(), ref reuseNeolithicProjectiles, "CE_Settings_ReuseNeolithicProjectiles_Desc".Translate());
			list.CheckboxLabeled("CE_Settings_RealisticCookOff_Title".Translate(), ref realisticCookOff, "CE_Settings_RealisticCookOff_Desc".Translate());
			list.CheckboxLabeled("CE_Settings_GenericAmmo".Translate(), ref genericammo, "CE_Settings_GenericAmmo_Desc".Translate());
		}
		else
		{
			GUI.contentColor = Color.gray;
			list.Label("CE_Settings_RightClickAmmoSelect_Title".Translate());
			list.Label("CE_Settings_AutoReloadOnChangeAmmo_Title".Translate());
			list.Label("CE_Settings_AutoTakeAmmo_Title".Translate());
			list.Label("CE_Settings_ShowCaliberOnGuns_Title".Translate());
			list.Label("CE_Settings_ReuseNeolithicProjectiles_Title".Translate());
			list.Label("CE_Settings_RealisticCookOff_Title".Translate());
			GUI.contentColor = Color.white;
		}
		Text.Font = GameFont.Medium;
		list.Label("CE_Settings_BipodSettings".Translate());
		Text.Font = GameFont.Small;
		list.CheckboxLabeled("CE_Settings_BipodMechanics_Title".Translate(), ref bipodMechanics, "CE_Settings_BipodMechanics_Desc".Translate());
		list.CheckboxLabeled("CE_Settings_BipodAutoSetUp_Title".Translate(), ref autosetup, "CE_Settings_BipodAutoSetUp_Desc".Translate());
		if (lastAmmoSystemStatus != enableAmmoSystem)
		{
			AmmoInjector.Inject();
			AmmoInjector.AddRemoveCaliberFromGunRecipes();
			lastAmmoSystemStatus = enableAmmoSystem;
			TutorUtility.DoModalDialogIfNotKnown(CE_ConceptDefOf.CE_AmmoSettings);
		}
		else if (AmmoInjector.gunRecipesShowCaliber != showCaliberOnGuns)
		{
			AmmoInjector.AddRemoveCaliberFromGunRecipes();
		}
	}
}
