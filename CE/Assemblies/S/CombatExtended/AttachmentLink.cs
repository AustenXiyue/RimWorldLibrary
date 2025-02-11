using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace CombatExtended;

public class AttachmentLink
{
	public AttachmentDef attachment;

	public Vector2 drawScale = Vector2.one;

	public Vector2 drawOffset = Vector2.zero;

	public List<StatModifier> statOffsets;

	public List<StatModifier> statMultipliers;

	public List<StatModifier> statReplacers;

	private WeaponPlatformDef parent;

	public Mesh meshTop;

	public Mesh meshBot;

	public Mesh meshFlipTop;

	public Mesh meshFlipBot;

	private Texture2D _UIAttachmentTex = null;

	private Texture2D _UIOutlineTex = null;

	public WeaponPlatformDef Parent => parent;

	public bool HasOutline => attachment.outlineGraphicData != null;

	public bool HasAttachmentMat => attachment.attachmentGraphicData != null;

	public Material AttachmentMat => attachment.attachmentGraphicData.Graphic.MatSingle;

	public Material OutlineMat => attachment.outlineGraphicData.Graphic.MatSingle;

	public bool HasDrawOffset => drawOffset.x != 0f || drawOffset.y != 0f;

	public Texture2D UIAttachmentTex
	{
		get
		{
			if (_UIAttachmentTex == null)
			{
				_UIAttachmentTex = (Texture2D)AttachmentMat.mainTexture;
			}
			return _UIAttachmentTex;
		}
	}

	public Texture2D UIOutlineTex
	{
		get
		{
			if (_UIOutlineTex == null)
			{
				_UIOutlineTex = (Texture2D)OutlineMat.mainTexture;
			}
			return _UIOutlineTex;
		}
	}

	public void PrepareTexture(WeaponPlatformDef parent)
	{
		this.parent = parent;
		meshTop = CE_MeshMaker.NewPlaneMesh(drawOffset, drawScale);
		meshBot = CE_MeshMaker.NewPlaneMesh(drawOffset, drawScale, -0.03f);
		meshFlipTop = CE_MeshMaker.NewPlaneMesh(drawOffset, drawScale, -0f, flipped: true);
		meshFlipBot = CE_MeshMaker.NewPlaneMesh(drawOffset, drawScale, -0.03f, flipped: true);
	}

	public bool CompatibleWith(AttachmentLink other)
	{
		return parent.AttachmentsCompatible(attachment, other.attachment);
	}
}
