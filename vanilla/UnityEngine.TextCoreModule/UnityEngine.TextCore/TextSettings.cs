using System;
using System.Collections.Generic;

namespace UnityEngine.TextCore;

[Serializable]
internal class TextSettings : ScriptableObject
{
	public class LineBreakingTable
	{
		public Dictionary<int, char> leadingCharacters;

		public Dictionary<int, char> followingCharacters;
	}

	private const string k_DefaultLeadingCharacters = "([｛〔〈《「『【〘〖〝‘“｟«$—…‥〳〴〵\\［（{£¥\"々〇〉》」＄｠￥￦ #";

	private const string k_DefaultFollowingCharacters = ")]｝〕〉》」』】〙〗〟’”｠»ヽヾーァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻‐゠–〜?!‼⁇⁈⁉・、%,.:;。！？］）：；＝}¢°\"†‡℃〆％，．";

	private static TextSettings s_Instance;

	[SerializeField]
	private int m_missingGlyphCharacter;

	[SerializeField]
	private bool m_warningsDisabled = true;

	[SerializeField]
	private FontAsset m_defaultFontAsset;

	[SerializeField]
	private string m_defaultFontAssetPath;

	[SerializeField]
	private List<FontAsset> m_fallbackFontAssets;

	[SerializeField]
	private bool m_matchMaterialPreset;

	[SerializeField]
	private TextSpriteAsset m_defaultSpriteAsset;

	[SerializeField]
	private string m_defaultSpriteAssetPath;

	[SerializeField]
	private string m_defaultColorGradientPresetsPath;

	[SerializeField]
	private TextStyleSheet m_defaultStyleSheet;

	[SerializeField]
	private TextAsset m_leadingCharacters = null;

	[SerializeField]
	private TextAsset m_followingCharacters = null;

	[SerializeField]
	private LineBreakingTable m_linebreakingRules;

	public static int missingGlyphCharacter
	{
		get
		{
			return instance.m_missingGlyphCharacter;
		}
		set
		{
			instance.m_missingGlyphCharacter = value;
		}
	}

	public static bool warningsDisabled
	{
		get
		{
			return instance.m_warningsDisabled;
		}
		set
		{
			instance.m_warningsDisabled = value;
		}
	}

	public static FontAsset defaultFontAsset
	{
		get
		{
			return instance.m_defaultFontAsset;
		}
		set
		{
			instance.m_defaultFontAsset = value;
		}
	}

	public static string defaultFontAssetPath
	{
		get
		{
			return instance.m_defaultFontAssetPath;
		}
		set
		{
			instance.m_defaultFontAssetPath = value;
		}
	}

	public static List<FontAsset> fallbackFontAssets
	{
		get
		{
			return instance.m_fallbackFontAssets;
		}
		set
		{
			instance.m_fallbackFontAssets = value;
		}
	}

	public static bool matchMaterialPreset
	{
		get
		{
			return instance.m_matchMaterialPreset;
		}
		set
		{
			instance.m_matchMaterialPreset = value;
		}
	}

	public static TextSpriteAsset defaultSpriteAsset
	{
		get
		{
			return instance.m_defaultSpriteAsset;
		}
		set
		{
			instance.m_defaultSpriteAsset = value;
		}
	}

	public static string defaultSpriteAssetPath
	{
		get
		{
			return instance.m_defaultSpriteAssetPath;
		}
		set
		{
			instance.m_defaultSpriteAssetPath = value;
		}
	}

	public static string defaultColorGradientPresetsPath
	{
		get
		{
			return instance.m_defaultColorGradientPresetsPath;
		}
		set
		{
			instance.m_defaultColorGradientPresetsPath = value;
		}
	}

	public static TextStyleSheet defaultStyleSheet
	{
		get
		{
			return instance.m_defaultStyleSheet;
		}
		set
		{
			instance.m_defaultStyleSheet = value;
			TextStyleSheet.LoadDefaultStyleSheet();
		}
	}

	public static LineBreakingTable linebreakingRules
	{
		get
		{
			if (instance.m_linebreakingRules == null)
			{
				LoadLinebreakingRules();
			}
			return instance.m_linebreakingRules;
		}
	}

	public static TextSettings instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = Resources.Load<TextSettings>("TextSettings") ?? ScriptableObject.CreateInstance<TextSettings>();
			}
			return s_Instance;
		}
	}

	public static void LoadLinebreakingRules()
	{
		if (!(instance == null))
		{
			if (s_Instance.m_linebreakingRules == null)
			{
				s_Instance.m_linebreakingRules = new LineBreakingTable();
			}
			s_Instance.m_linebreakingRules.leadingCharacters = ((s_Instance.m_leadingCharacters != null) ? GetCharacters(s_Instance.m_leadingCharacters.text) : GetCharacters("([｛〔〈《「『【〘〖〝‘“｟«$—…‥〳〴〵\\［（{£¥\"々〇〉》」＄｠￥￦ #"));
			s_Instance.m_linebreakingRules.followingCharacters = ((s_Instance.m_followingCharacters != null) ? GetCharacters(s_Instance.m_followingCharacters.text) : GetCharacters(")]｝〕〉》」』】〙〗〟’”｠»ヽヾーァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻‐゠–〜?!‼⁇⁈⁉・、%,.:;。！？］）：；＝}¢°\"†‡℃〆％，．"));
		}
	}

	private static Dictionary<int, char> GetCharacters(string text)
	{
		Dictionary<int, char> dictionary = new Dictionary<int, char>();
		foreach (char c in text)
		{
			if (!dictionary.ContainsKey(c))
			{
				dictionary.Add(c, c);
			}
		}
		return dictionary;
	}
}
