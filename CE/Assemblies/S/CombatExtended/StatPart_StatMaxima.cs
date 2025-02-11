using System.Runtime.CompilerServices;
using UnityEngine;

namespace CombatExtended;

public class StatPart_StatMaxima : StatPart_StatSelect
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override float Select(float first, float second)
	{
		return Mathf.Max(first, second);
	}
}
