using System;
using UnityEngine;

namespace TMPro;

[Serializable]
public class TMP_SpriteCharacter : TMP_TextElement
{
	[SerializeField]
	private string m_Name;

	[SerializeField]
	private int m_HashCode;

	public string name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(value == m_Name))
			{
				m_Name = value;
				m_HashCode = TMP_TextParsingUtilities.GetHashCodeCaseSensitive(m_Name);
			}
		}
	}

	public int hashCode => m_HashCode;

	public TMP_SpriteCharacter()
	{
		m_ElementType = TextElementType.Sprite;
	}

	public TMP_SpriteCharacter(uint unicode, TMP_SpriteGlyph glyph)
	{
		m_ElementType = TextElementType.Sprite;
		base.unicode = unicode;
		base.glyphIndex = glyph.index;
		base.glyph = glyph;
		base.scale = 1f;
	}

	public TMP_SpriteCharacter(uint unicode, TMP_SpriteAsset spriteAsset, TMP_SpriteGlyph glyph)
	{
		m_ElementType = TextElementType.Sprite;
		base.unicode = unicode;
		base.textAsset = spriteAsset;
		base.glyph = glyph;
		base.glyphIndex = glyph.index;
		base.scale = 1f;
	}

	internal TMP_SpriteCharacter(uint unicode, uint glyphIndex)
	{
		m_ElementType = TextElementType.Sprite;
		base.unicode = unicode;
		base.textAsset = null;
		base.glyph = null;
		base.glyphIndex = glyphIndex;
		base.scale = 1f;
	}
}
