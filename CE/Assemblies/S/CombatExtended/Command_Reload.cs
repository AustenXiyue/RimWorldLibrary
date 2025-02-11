using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Command_Reload : Command_Action
{
	private List<Command_Reload> others;

	public CompAmmoUser compAmmo;

	public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
	{
		get
		{
			if (!Controller.settings.RightClickAmmoSelect)
			{
				yield break;
			}
			foreach (FloatMenuOption item in BuildAmmoOptions())
			{
				yield return item;
			}
		}
	}

	public override bool GroupsWith(Gizmo other)
	{
		Command_Reload command_Reload = other as Command_Reload;
		return command_Reload != null;
	}

	public override void MergeWith(Gizmo other)
	{
		Command_Reload item = other as Command_Reload;
		if (others == null)
		{
			others = new List<Command_Reload>();
			others.Add(this);
		}
		others.Add(item);
	}

	public override void ProcessInput(Event ev)
	{
		if (compAmmo == null)
		{
			Log.Error("Command_Reload without ammo comp");
			return;
		}
		if ((compAmmo.UseAmmo && (compAmmo.CompInventory != null || compAmmo.turret != null || compAmmo.parent is Building_AutoloaderCE)) || action == null)
		{
			bool valueOrDefault = compAmmo.turret?.GetMannable()?.MannedNow == true;
			if (Controller.settings.RightClickAmmoSelect && action != null && (compAmmo.turret == null || valueOrDefault))
			{
				base.ProcessInput(ev);
			}
			else
			{
				Find.WindowStack.Add(MakeAmmoMenu());
			}
		}
		else if (compAmmo.SelectedAmmo != compAmmo.CurrentAmmo || compAmmo.CurMagCount < compAmmo.MagSize)
		{
			base.ProcessInput(ev);
		}
		if (!tutorTag.NullOrEmpty())
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDef.Named(tutorTag), KnowledgeAmount.Total);
		}
	}

	[Multiplayer.SyncMethod]
	private static void SetAmmoType(CompAmmoUser user, AmmoDef ammoDef)
	{
		user.SelectedAmmo = ammoDef;
	}

	private FloatMenu MakeAmmoMenu()
	{
		return new FloatMenu(BuildAmmoOptions());
	}

	private List<FloatMenuOption> BuildAmmoOptions()
	{
		if (others == null)
		{
			others = new List<Command_Reload>();
		}
		if (!others.Contains(this))
		{
			others.Add(this);
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		if (compAmmo.UseAmmo)
		{
			Dictionary<AmmoCategoryDef, Action> dictionary = new Dictionary<AmmoCategoryDef, Action>();
			Dictionary<AmmoCategoryDef, int[]> dictionary2 = new Dictionary<AmmoCategoryDef, int[]>();
			bool flag = false;
			foreach (Command_Reload other in others)
			{
				CompAmmoUser user = other.compAmmo;
				foreach (AmmoLink ammoType in user.Props.ammoSet.ammoTypes)
				{
					AmmoDef ammoDef = ammoType.ammo;
					AmmoCategoryDef ammoClass = ammoDef.ammoClass;
					if (!(user.CompInventory?.ammoList?.Any((Thing x) => x.def == ammoDef) ?? true) && !DebugSettings.godMode)
					{
						continue;
					}
					if (!dictionary2.ContainsKey(ammoClass))
					{
						dictionary2.Add(ammoClass, new int[2]);
					}
					dictionary2[ammoClass][0]++;
					Action action = null;
					if (user.CurrentAmmo == ammoDef)
					{
						dictionary2[ammoClass][1]++;
					}
					if (user.SelectedAmmo == ammoDef)
					{
						if (Controller.settings.AutoReloadOnChangeAmmo && user.turret?.GetMannable() == null && user.CurMagCount < user.MagSize)
						{
							action = (Action)Delegate.Combine(action, other.action);
						}
					}
					else
					{
						action = (Action)Delegate.Combine(action, (Action)delegate
						{
							SetAmmoType(user, ammoDef);
						});
						if (Controller.settings.AutoReloadOnChangeAmmo && user.turret?.GetMannable() == null)
						{
							action = (Action)Delegate.Combine(action, other.action);
						}
					}
					if (dictionary.ContainsKey(ammoClass))
					{
						Dictionary<AmmoCategoryDef, Action> dictionary3 = dictionary;
						AmmoCategoryDef key = ammoClass;
						dictionary3[key] = (Action)Delegate.Combine(dictionary3[key], action);
					}
					else
					{
						dictionary.Add(ammoClass, action);
					}
					flag = true;
				}
			}
			if (flag)
			{
				foreach (KeyValuePair<AmmoCategoryDef, Action> item in dictionary)
				{
					list.Add(new FloatMenuOption(others.Except(this).Any() ? ("(" + dictionary2[item.Key][1] + "/" + dictionary2[item.Key][0] + ") " + item.Key.LabelCap) : item.Key.LabelCap, item.Value));
				}
			}
			else
			{
				list.Add(new FloatMenuOption("CE_OutOfAmmo".Translate(), null));
			}
		}
		List<CompAmmoUser> usersToUnload = new List<CompAmmoUser>();
		bool flag2 = false;
		Action a = null;
		foreach (Command_Reload other2 in others)
		{
			CompAmmoUser compAmmoUser = other2.compAmmo;
			if ((compAmmoUser.HasMagazine && compAmmoUser.Wielder != null) || compAmmoUser.turret?.GetMannable()?.MannedNow == true)
			{
				flag2 = true;
				a = (Action)Delegate.Combine(a, other2.action);
				if (compAmmoUser.UseAmmo && compAmmoUser.CurMagCount > 0)
				{
					usersToUnload.Add(compAmmoUser);
				}
			}
		}
		if (usersToUnload.Any())
		{
			list.Add(new FloatMenuOption("CE_UnloadLabel".Translate(), delegate
			{
				SyncedTryUnload(usersToUnload);
			}));
		}
		if (flag2)
		{
			list.Add(new FloatMenuOption("CE_ReloadLabel".Translate(), a));
		}
		return list;
	}

	[Multiplayer.SyncMethod]
	private static void SyncedTryUnload(List<CompAmmoUser> ammoUsers)
	{
		foreach (CompAmmoUser ammoUser in ammoUsers)
		{
			ammoUser.TryUnload(forceUnload: true);
		}
	}
}
