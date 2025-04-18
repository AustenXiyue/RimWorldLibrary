using System.Collections.Generic;

namespace HarmonyLib;

public static class CodeInstructionsExtensions
{
	public static bool Matches(this IEnumerable<CodeInstruction> instructions, CodeMatch[] matches)
	{
		return new CodeMatcher(instructions).MatchStartForward(matches).IsValid;
	}
}
