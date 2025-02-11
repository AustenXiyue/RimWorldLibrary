using System;
using UnityEngine;

namespace CombatExtended;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CE_DebugTooltip : Attribute
{
	public readonly CE_DebugTooltipType tooltipType;

	public readonly KeyCode altKey;

	public CE_DebugTooltip(CE_DebugTooltipType tooltipType, KeyCode altKey = KeyCode.None)
	{
		this.tooltipType = tooltipType;
		this.altKey = altKey;
	}
}
