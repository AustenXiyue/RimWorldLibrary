using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class CE_DebugTooltipHelper : GameComponent
{
	private StringBuilder builder = new StringBuilder();

	private static List<Pair<Func<Map, IntVec3, string>, KeyCode>> mapCallbacks;

	private static List<Pair<Func<World, int, string>, KeyCode>> worldCallbacks;

	private static readonly Rect MouseRect;

	static CE_DebugTooltipHelper()
	{
		mapCallbacks = new List<Pair<Func<Map, IntVec3, string>, KeyCode>>();
		worldCallbacks = new List<Pair<Func<World, int, string>, KeyCode>>();
		MouseRect = new Rect(0f, 0f, 50f, 50f);
		IEnumerable<MethodInfo> enumerable = typeof(CE_DebugTooltipHelper).Assembly.GetTypes().SelectMany((Type t) => t.GetMethods(AccessTools.all)).Where(delegate(MethodInfo m)
		{
			try
			{
				return m.HasAttribute<CE_DebugTooltip>() && m.IsStatic;
			}
			catch (TargetInvocationException)
			{
				return false;
			}
		});
		foreach (MethodInfo m2 in enumerable)
		{
			CE_DebugTooltip cE_DebugTooltip = m2.TryGetAttribute<CE_DebugTooltip>();
			if (cE_DebugTooltip.tooltipType == CE_DebugTooltipType.World)
			{
				ParameterInfo[] parameters = m2.GetParameters();
				if (parameters[0].ParameterType != typeof(World) || parameters[1].ParameterType != typeof(int))
				{
					Log.Error("CE: Error processing debug tooltip " + m2.GetType().Name + ":" + m2.Name + " " + GeneralExtensions.FullDescription((MethodBase)m2) + " need to have (World, int) as parameters, skipped");
				}
				else
				{
					worldCallbacks.Add(new Pair<Func<World, int, string>, KeyCode>((World world, int tile) => (string)m2.Invoke(null, new object[2] { world, tile }), cE_DebugTooltip.altKey));
				}
			}
			else
			{
				if (cE_DebugTooltip.tooltipType != CE_DebugTooltipType.Map)
				{
					continue;
				}
				ParameterInfo[] parameters2 = m2.GetParameters();
				if (parameters2[0].ParameterType != typeof(Map) || parameters2[1].ParameterType != typeof(IntVec3))
				{
					Log.Error("CE: Error processing debug tooltip " + m2.GetType().Name + ":" + m2.Name + " " + GeneralExtensions.FullDescription((MethodBase)m2) + " need to have (Map, IntVec3) as parameters, skipped");
				}
				else
				{
					mapCallbacks.Add(new Pair<Func<Map, IntVec3, string>, KeyCode>((Map map, IntVec3 cell) => (string)m2.Invoke(null, new object[2] { map, cell }), cE_DebugTooltip.altKey));
				}
			}
		}
	}

	public CE_DebugTooltipHelper(Game game)
	{
	}

	public override void GameComponentUpdate()
	{
		base.GameComponentUpdate();
		if (!Controller.settings.ShowExtraTooltips || !Input.anyKey || Find.CurrentMap == null || Current.ProgramState != ProgramState.Playing)
		{
			return;
		}
		Rect mouseRect = MouseRect;
		mouseRect.center = Event.current.mousePosition;
		Camera worldCamera = Find.WorldCamera;
		if (!worldCamera.gameObject.activeInHierarchy)
		{
			IntVec3 intVec = UI.MouseCell();
			if (intVec.InBounds(Find.CurrentMap))
			{
				TryMapTooltips(mouseRect, intVec);
			}
		}
		else
		{
			int num = GenWorld.MouseTile();
			if (num != -1)
			{
				TryWorldTooltips(mouseRect, num);
			}
		}
	}

	private void TryMapTooltips(Rect mouseRect, IntVec3 mouseCell)
	{
		bool flag = false;
		for (int i = 0; i < mapCallbacks.Count; i++)
		{
			Pair<Func<Map, IntVec3, string>, KeyCode> pair = mapCallbacks[i];
			if (Input.GetKey((pair.Second == KeyCode.None) ? KeyCode.LeftShift : pair.Second))
			{
				string text;
				try
				{
					text = pair.First(Find.CurrentMap, mouseCell);
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
					text = "Debug Callback failed (see log for details)";
				}
				if (!text.NullOrEmpty())
				{
					DoTipSignal(mouseRect, text);
				}
				if (!flag)
				{
					GenUI.RenderMouseoverBracket();
					flag = true;
				}
			}
		}
	}

	private void TryWorldTooltips(Rect mouseRect, int tile)
	{
		for (int i = 0; i < worldCallbacks.Count; i++)
		{
			Pair<Func<World, int, string>, KeyCode> pair = worldCallbacks[i];
			if (Input.GetKey((pair.Second == KeyCode.None) ? KeyCode.LeftShift : pair.Second))
			{
				string text = pair.First(Find.World, tile);
				if (!text.NullOrEmpty())
				{
					DoTipSignal(mouseRect, text);
				}
			}
		}
	}

	private TipSignal DoTipSignal(Rect rect, string message)
	{
		builder.Clear();
		builder.Append("<color=orange>CE_DEBUG_TIP:</color>. ");
		builder.Append(message);
		TipSignal tipSignal = default(TipSignal);
		tipSignal.text = message;
		tipSignal.priority = (TooltipPriority)3;
		TooltipHandler.TipRegion(rect, tipSignal);
		return tipSignal;
	}
}
