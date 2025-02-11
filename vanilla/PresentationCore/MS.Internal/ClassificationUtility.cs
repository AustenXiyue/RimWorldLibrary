using MS.Internal.Text.TextInterface;

namespace MS.Internal;

internal class ClassificationUtility : IClassification
{
	internal static readonly bool[] ScriptCaretInfo = new bool[64]
	{
		false, false, false, true, false, false, true, false, false, false,
		false, false, false, false, false, true, false, false, false, false,
		false, true, true, true, false, true, true, false, true, true,
		true, false, true, false, true, false, true, false, true, true,
		false, false, false, true, false, false, false, true, true, false,
		false, false, false, true, true, true, true, true, false, false,
		false, false, false, false
	};

	private static ClassificationUtility _classificationUtilityInstance = new ClassificationUtility();

	internal static ClassificationUtility Instance => _classificationUtilityInstance;

	public void GetCharAttribute(int unicodeScalar, out bool isCombining, out bool needsCaretInfo, out bool isIndic, out bool isDigit, out bool isLatin, out bool isStrong)
	{
		CharacterAttribute characterAttribute = Classification.CharAttributeOf(Classification.GetUnicodeClass(unicodeScalar));
		byte itemClass = characterAttribute.ItemClass;
		isCombining = itemClass == 7 || itemClass == 8 || Classification.IsIVS(unicodeScalar);
		isStrong = itemClass == 5;
		int script = characterAttribute.Script;
		needsCaretInfo = ScriptCaretInfo[script];
		ScriptID scriptID = (ScriptID)script;
		isDigit = scriptID == ScriptID.Digit;
		isLatin = scriptID == ScriptID.Latin;
		if (isLatin)
		{
			isIndic = false;
		}
		else
		{
			isIndic = IsScriptIndic(scriptID);
		}
	}

	private static bool IsScriptIndic(ScriptID scriptId)
	{
		if (scriptId == ScriptID.Bengali || scriptId == ScriptID.Devanagari || scriptId == ScriptID.Gurmukhi || scriptId == ScriptID.Gujarati || scriptId == ScriptID.Kannada || scriptId == ScriptID.Malayalam || scriptId == ScriptID.Oriya || scriptId == ScriptID.Tamil || scriptId == ScriptID.Telugu)
		{
			return true;
		}
		return false;
	}
}
