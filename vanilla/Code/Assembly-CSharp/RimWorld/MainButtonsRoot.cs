using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class MainButtonsRoot
{
	public MainTabsRoot tabs = new MainTabsRoot();

	private List<MainButtonDef> allButtonsInOrder;

	private int VisibleButtonsCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < allButtonsInOrder.Count; i++)
			{
				if (allButtonsInOrder[i].buttonVisible)
				{
					num++;
				}
			}
			return num;
		}
	}

	public MainButtonsRoot()
	{
		allButtonsInOrder = DefDatabase<MainButtonDef>.AllDefs.OrderBy((MainButtonDef x) => x.order).ToList();
	}

	public void MainButtonsOnGUI()
	{
		if (Event.current.type == EventType.Layout)
		{
			return;
		}
		DoButtons();
		for (int i = 0; i < allButtonsInOrder.Count; i++)
		{
			if (!allButtonsInOrder[i].Worker.Disabled && allButtonsInOrder[i].hotKey != null && allButtonsInOrder[i].hotKey.KeyDownEvent)
			{
				Event.current.Use();
				allButtonsInOrder[i].Worker.InterfaceTryActivate();
				break;
			}
		}
	}

	public void HandleLowPriorityShortcuts()
	{
		tabs.HandleLowPriorityShortcuts();
		if (WorldRendererUtility.WorldRenderedNow && Current.ProgramState == ProgramState.Playing && Find.CurrentMap != null && KeyBindingDefOf.Cancel.KeyDownEvent)
		{
			Event.current.Use();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}

	private void DoButtons()
	{
		float num = 0f;
		for (int i = 0; i < allButtonsInOrder.Count; i++)
		{
			if (allButtonsInOrder[i].Worker.Visible)
			{
				num += (allButtonsInOrder[i].minimized ? 0.5f : 1f);
			}
		}
		GUI.color = Color.white;
		int num2 = (int)((float)UI.screenWidth / num);
		int num3 = num2 / 2;
		int num4 = allButtonsInOrder.FindLastIndex((MainButtonDef x) => x.Worker.Visible);
		int num5 = 0;
		for (int j = 0; j < allButtonsInOrder.Count; j++)
		{
			if (allButtonsInOrder[j].Worker.Visible)
			{
				int num6 = (allButtonsInOrder[j].minimized ? num3 : num2);
				if (j == num4)
				{
					num6 = UI.screenWidth - num5;
				}
				Rect rect = new Rect(num5, UI.screenHeight - 35, num6, 36f);
				allButtonsInOrder[j].Worker.DoButton(rect);
				num5 += num6;
			}
		}
	}
}
