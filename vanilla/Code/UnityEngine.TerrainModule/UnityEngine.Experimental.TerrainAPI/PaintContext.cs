using System;
using System.Collections.Generic;

namespace UnityEngine.Experimental.TerrainAPI;

public class PaintContext
{
	public interface ITerrainInfo
	{
		Terrain terrain { get; }

		RectInt clippedTerrainPixels { get; }

		RectInt clippedPCPixels { get; }

		bool gatherEnable { get; set; }

		bool scatterEnable { get; set; }

		object userData { get; set; }
	}

	private class TerrainTile : ITerrainInfo
	{
		public Terrain terrain;

		public Vector2Int tileOriginPixels;

		public RectInt clippedTerrainPixels;

		public RectInt clippedPCPixels;

		public object userData;

		public bool gatherEnable;

		public bool scatterEnable;

		Terrain ITerrainInfo.terrain => terrain;

		RectInt ITerrainInfo.clippedTerrainPixels => clippedTerrainPixels;

		RectInt ITerrainInfo.clippedPCPixels => clippedPCPixels;

		bool ITerrainInfo.gatherEnable
		{
			get
			{
				return gatherEnable;
			}
			set
			{
				gatherEnable = value;
			}
		}

		bool ITerrainInfo.scatterEnable
		{
			get
			{
				return scatterEnable;
			}
			set
			{
				scatterEnable = value;
			}
		}

		object ITerrainInfo.userData
		{
			get
			{
				return userData;
			}
			set
			{
				userData = value;
			}
		}

		public static TerrainTile Make(Terrain terrain, int tileOriginPixelsX, int tileOriginPixelsY, RectInt pixelRect, int targetTextureWidth, int targetTextureHeight)
		{
			TerrainTile terrainTile = new TerrainTile
			{
				terrain = terrain,
				gatherEnable = true,
				scatterEnable = true,
				tileOriginPixels = new Vector2Int(tileOriginPixelsX, tileOriginPixelsY),
				clippedTerrainPixels = new RectInt
				{
					x = Mathf.Max(0, pixelRect.x - tileOriginPixelsX),
					y = Mathf.Max(0, pixelRect.y - tileOriginPixelsY),
					xMax = Mathf.Min(targetTextureWidth, pixelRect.xMax - tileOriginPixelsX),
					yMax = Mathf.Min(targetTextureHeight, pixelRect.yMax - tileOriginPixelsY)
				}
			};
			terrainTile.clippedPCPixels = new RectInt(terrainTile.clippedTerrainPixels.x + terrainTile.tileOriginPixels.x - pixelRect.x, terrainTile.clippedTerrainPixels.y + terrainTile.tileOriginPixels.y - pixelRect.y, terrainTile.clippedTerrainPixels.width, terrainTile.clippedTerrainPixels.height);
			if (terrainTile.clippedTerrainPixels.width == 0 || terrainTile.clippedTerrainPixels.height == 0)
			{
				terrainTile.gatherEnable = false;
				terrainTile.scatterEnable = false;
				Debug.LogError("PaintContext.ClipTerrainTiles found 0 content rect");
			}
			return terrainTile;
		}
	}

	private class SplatmapUserData
	{
		public TerrainLayer terrainLayer;

		public int terrainLayerIndex;

		public int mapIndex;

		public int channelIndex;
	}

	[Flags]
	internal enum ToolAction
	{
		None = 0,
		PaintHeightmap = 1,
		PaintTexture = 2,
		PaintHoles = 4
	}

	private struct PaintedTerrain
	{
		public Terrain terrain;

		public ToolAction action;
	}

	private List<TerrainTile> m_TerrainTiles;

	private float m_HeightWorldSpaceMin;

	private float m_HeightWorldSpaceMax;

	private static List<PaintedTerrain> s_PaintedTerrain = new List<PaintedTerrain>();

	public Terrain originTerrain { get; }

	public RectInt pixelRect { get; }

	public int targetTextureWidth { get; }

	public int targetTextureHeight { get; }

	public Vector2 pixelSize { get; }

	public RenderTexture sourceRenderTexture { get; private set; }

	public RenderTexture destinationRenderTexture { get; private set; }

	public RenderTexture oldRenderTexture { get; private set; }

	public int terrainCount => m_TerrainTiles.Count;

	public float heightWorldSpaceMin => m_HeightWorldSpaceMin;

	public float heightWorldSpaceSize => m_HeightWorldSpaceMax - m_HeightWorldSpaceMin;

	public static float kNormalizedHeightScale => 0.4999771f;

	internal static event Action<ITerrainInfo, ToolAction, string> onTerrainTileBeforePaint;

	public Terrain GetTerrain(int terrainIndex)
	{
		return m_TerrainTiles[terrainIndex].terrain;
	}

	public RectInt GetClippedPixelRectInTerrainPixels(int terrainIndex)
	{
		return m_TerrainTiles[terrainIndex].clippedTerrainPixels;
	}

	public RectInt GetClippedPixelRectInRenderTexturePixels(int terrainIndex)
	{
		return m_TerrainTiles[terrainIndex].clippedPCPixels;
	}

	public PaintContext(Terrain terrain, RectInt pixelRect, int targetTextureWidth, int targetTextureHeight, bool texelPadding = true)
	{
		originTerrain = terrain;
		this.pixelRect = pixelRect;
		this.targetTextureWidth = targetTextureWidth;
		this.targetTextureHeight = targetTextureHeight;
		TerrainData terrainData = terrain.terrainData;
		pixelSize = new Vector2(terrainData.size.x / ((float)targetTextureWidth - (texelPadding ? 1f : 0f)), terrainData.size.z / ((float)targetTextureHeight - (texelPadding ? 1f : 0f)));
		FindTerrainTilesUnlimited(texelPadding);
	}

	public static PaintContext CreateFromBounds(Terrain terrain, Rect boundsInTerrainSpace, int inputTextureWidth, int inputTextureHeight, int extraBorderPixels = 0, bool texelPadding = true)
	{
		return new PaintContext(terrain, TerrainPaintUtility.CalcPixelRectFromBounds(terrain, boundsInTerrainSpace, inputTextureWidth, inputTextureHeight, extraBorderPixels, texelPadding), inputTextureWidth, inputTextureHeight, texelPadding);
	}

	private void FindTerrainTilesUnlimited(bool texelPadding)
	{
		float minX = originTerrain.transform.position.x + pixelSize.x * (float)pixelRect.xMin;
		float minZ = originTerrain.transform.position.z + pixelSize.y * (float)pixelRect.yMin;
		float maxX = originTerrain.transform.position.x + pixelSize.x * (float)(pixelRect.xMax - 1);
		float maxZ = originTerrain.transform.position.z + pixelSize.y * (float)(pixelRect.yMax - 1);
		m_HeightWorldSpaceMin = originTerrain.GetPosition().y;
		m_HeightWorldSpaceMax = m_HeightWorldSpaceMin + originTerrain.terrainData.size.y;
		TerrainUtility.TerrainMap.TerrainFilter filter = delegate(Terrain t)
		{
			float x = t.transform.position.x;
			float z = t.transform.position.z;
			float num = t.transform.position.x + t.terrainData.size.x;
			float num2 = t.transform.position.z + t.terrainData.size.z;
			return x <= maxX && num >= minX && z <= maxZ && num2 >= minZ;
		};
		TerrainUtility.TerrainMap terrainMap = TerrainUtility.TerrainMap.CreateFromConnectedNeighbors(originTerrain, filter, fullValidation: false);
		m_TerrainTiles = new List<TerrainTile>();
		if (terrainMap == null)
		{
			return;
		}
		foreach (KeyValuePair<TerrainUtility.TerrainMap.TileCoord, Terrain> terrainTile in terrainMap.m_terrainTiles)
		{
			TerrainUtility.TerrainMap.TileCoord key = terrainTile.Key;
			Terrain value = terrainTile.Value;
			int num3 = key.tileX * (targetTextureWidth - (texelPadding ? 1 : 0));
			int num4 = key.tileZ * (targetTextureHeight - (texelPadding ? 1 : 0));
			RectInt other = new RectInt(num3, num4, targetTextureWidth, targetTextureHeight);
			if (pixelRect.Overlaps(other))
			{
				m_TerrainTiles.Add(TerrainTile.Make(value, num3, num4, pixelRect, targetTextureWidth, targetTextureHeight));
				m_HeightWorldSpaceMin = Mathf.Min(m_HeightWorldSpaceMin, value.GetPosition().y);
				m_HeightWorldSpaceMax = Mathf.Max(m_HeightWorldSpaceMax, value.GetPosition().y + value.terrainData.size.y);
			}
		}
	}

	public void CreateRenderTargets(RenderTextureFormat colorFormat)
	{
		sourceRenderTexture = RenderTexture.GetTemporary(pixelRect.width, pixelRect.height, 0, colorFormat, RenderTextureReadWrite.Linear);
		destinationRenderTexture = RenderTexture.GetTemporary(pixelRect.width, pixelRect.height, 0, colorFormat, RenderTextureReadWrite.Linear);
		sourceRenderTexture.wrapMode = TextureWrapMode.Clamp;
		sourceRenderTexture.filterMode = FilterMode.Point;
		oldRenderTexture = RenderTexture.active;
	}

	public void Cleanup(bool restoreRenderTexture = true)
	{
		if (restoreRenderTexture)
		{
			RenderTexture.active = oldRenderTexture;
		}
		RenderTexture.ReleaseTemporary(sourceRenderTexture);
		RenderTexture.ReleaseTemporary(destinationRenderTexture);
		sourceRenderTexture = null;
		destinationRenderTexture = null;
		oldRenderTexture = null;
	}

	private void GatherInternal(Func<ITerrainInfo, Texture> terrainToTexture, Color defaultColor, string operationName, Material blitMaterial = null, int blitPass = 0, Action<ITerrainInfo> beforeBlit = null, Action<ITerrainInfo> afterBlit = null)
	{
		if (blitMaterial == null)
		{
			blitMaterial = TerrainPaintUtility.GetBlitMaterial();
		}
		RenderTexture.active = sourceRenderTexture;
		GL.Clear(clearDepth: false, clearColor: true, defaultColor);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, pixelRect.width, 0f, pixelRect.height);
		for (int i = 0; i < m_TerrainTiles.Count; i++)
		{
			TerrainTile terrainTile = m_TerrainTiles[i];
			if (!terrainTile.gatherEnable)
			{
				continue;
			}
			Texture texture = terrainToTexture(terrainTile);
			if (texture == null || !terrainTile.gatherEnable)
			{
				continue;
			}
			if (texture.width != targetTextureWidth || texture.height != targetTextureHeight)
			{
				Debug.LogWarning(operationName + " requires the same resolution texture for all Terrains - mismatched Terrains are ignored.", terrainTile.terrain);
				continue;
			}
			beforeBlit?.Invoke(terrainTile);
			if (terrainTile.gatherEnable)
			{
				FilterMode filterMode = texture.filterMode;
				texture.filterMode = FilterMode.Point;
				blitMaterial.SetTexture("_MainTex", texture);
				blitMaterial.SetPass(blitPass);
				TerrainPaintUtility.DrawQuad(terrainTile.clippedPCPixels, terrainTile.clippedTerrainPixels, texture);
				texture.filterMode = filterMode;
				afterBlit?.Invoke(terrainTile);
			}
		}
		GL.PopMatrix();
		RenderTexture.active = oldRenderTexture;
	}

	private void ScatterInternal(Func<ITerrainInfo, RenderTexture> terrainToRT, string operationName, Material blitMaterial = null, int blitPass = 0, Action<ITerrainInfo> beforeBlit = null, Action<ITerrainInfo> afterBlit = null)
	{
		RenderTexture active = RenderTexture.active;
		if (blitMaterial == null)
		{
			blitMaterial = TerrainPaintUtility.GetBlitMaterial();
		}
		for (int i = 0; i < m_TerrainTiles.Count; i++)
		{
			TerrainTile terrainTile = m_TerrainTiles[i];
			if (!terrainTile.scatterEnable)
			{
				continue;
			}
			RenderTexture renderTexture = terrainToRT(terrainTile);
			if (renderTexture == null || !terrainTile.scatterEnable)
			{
				continue;
			}
			if (renderTexture.width != targetTextureWidth || renderTexture.height != targetTextureHeight)
			{
				Debug.LogWarning(operationName + " requires the same resolution for all Terrains - mismatched Terrains are ignored.", terrainTile.terrain);
				continue;
			}
			beforeBlit?.Invoke(terrainTile);
			if (terrainTile.scatterEnable)
			{
				RenderTexture.active = renderTexture;
				GL.PushMatrix();
				GL.LoadPixelMatrix(0f, renderTexture.width, 0f, renderTexture.height);
				FilterMode filterMode = destinationRenderTexture.filterMode;
				destinationRenderTexture.filterMode = FilterMode.Point;
				blitMaterial.SetTexture("_MainTex", destinationRenderTexture);
				blitMaterial.SetPass(blitPass);
				TerrainPaintUtility.DrawQuad(terrainTile.clippedTerrainPixels, terrainTile.clippedPCPixels, destinationRenderTexture);
				destinationRenderTexture.filterMode = filterMode;
				GL.PopMatrix();
				afterBlit?.Invoke(terrainTile);
			}
		}
		RenderTexture.active = active;
	}

	public void Gather(Func<ITerrainInfo, Texture> terrainSource, Color defaultColor, Material blitMaterial = null, int blitPass = 0, Action<ITerrainInfo> beforeBlit = null, Action<ITerrainInfo> afterBlit = null)
	{
		if (terrainSource != null)
		{
			GatherInternal(terrainSource, defaultColor, "PaintContext.Gather", blitMaterial, blitPass, beforeBlit, afterBlit);
		}
	}

	public void Scatter(Func<ITerrainInfo, RenderTexture> terrainDest, Material blitMaterial = null, int blitPass = 0, Action<ITerrainInfo> beforeBlit = null, Action<ITerrainInfo> afterBlit = null)
	{
		if (terrainDest != null)
		{
			ScatterInternal(terrainDest, "PaintContext.Scatter", blitMaterial, blitPass, beforeBlit, afterBlit);
		}
	}

	public void GatherHeightmap()
	{
		Material blitMaterial = TerrainPaintUtility.GetHeightBlitMaterial();
		blitMaterial.SetFloat("_Height_Offset", 0f);
		blitMaterial.SetFloat("_Height_Scale", 1f);
		GatherInternal((ITerrainInfo t) => t.terrain.terrainData.heightmapTexture, new Color(0f, 0f, 0f, 0f), "PaintContext.GatherHeightmap", blitMaterial, 0, delegate(ITerrainInfo t)
		{
			blitMaterial.SetFloat("_Height_Offset", (t.terrain.GetPosition().y - heightWorldSpaceMin) / heightWorldSpaceSize * kNormalizedHeightScale);
			blitMaterial.SetFloat("_Height_Scale", t.terrain.terrainData.size.y / heightWorldSpaceSize);
		});
	}

	public void ScatterHeightmap(string editorUndoName)
	{
		Material blitMaterial = TerrainPaintUtility.GetHeightBlitMaterial();
		blitMaterial.SetFloat("_Height_Offset", 0f);
		blitMaterial.SetFloat("_Height_Scale", 1f);
		ScatterInternal((ITerrainInfo t) => t.terrain.terrainData.heightmapTexture, "PaintContext.ScatterHeightmap", blitMaterial, 0, delegate(ITerrainInfo t)
		{
			PaintContext.onTerrainTileBeforePaint?.Invoke(t, ToolAction.PaintHeightmap, editorUndoName);
			blitMaterial.SetFloat("_Height_Offset", (heightWorldSpaceMin - t.terrain.GetPosition().y) / t.terrain.terrainData.size.y * kNormalizedHeightScale);
			blitMaterial.SetFloat("_Height_Scale", heightWorldSpaceSize / t.terrain.terrainData.size.y);
		}, delegate(ITerrainInfo t)
		{
			t.terrain.terrainData.DirtyHeightmapRegion(t.clippedTerrainPixels, (!t.terrain.drawInstanced) ? TerrainHeightmapSyncControl.HeightOnly : TerrainHeightmapSyncControl.None);
			OnTerrainPainted(t, ToolAction.PaintHeightmap);
		});
	}

	public void GatherHoles()
	{
		GatherInternal((ITerrainInfo t) => t.terrain.terrainData.holesTexture, new Color(0f, 0f, 0f, 0f), "PaintContext.GatherHoles");
	}

	public void ScatterHoles(string editorUndoName)
	{
		ScatterInternal(delegate(ITerrainInfo t)
		{
			PaintContext.onTerrainTileBeforePaint?.Invoke(t, ToolAction.PaintHoles, editorUndoName);
			t.terrain.terrainData.CopyActiveRenderTextureToTexture(TerrainData.HolesTextureName, 0, t.clippedPCPixels, t.clippedTerrainPixels.min, allowDelayedCPUSync: true);
			OnTerrainPainted(t, ToolAction.PaintHoles);
			return (RenderTexture)null;
		}, "PaintContext.ScatterHoles");
	}

	public void GatherNormals()
	{
		GatherInternal((ITerrainInfo t) => t.terrain.normalmapTexture, new Color(0.5f, 0.5f, 0.5f, 0.5f), "PaintContext.GatherNormals");
	}

	private SplatmapUserData GetTerrainLayerUserData(ITerrainInfo context, TerrainLayer terrainLayer = null, bool addLayerIfDoesntExist = false)
	{
		SplatmapUserData splatmapUserData = context.userData as SplatmapUserData;
		if (splatmapUserData != null)
		{
			if (terrainLayer == null || terrainLayer == splatmapUserData.terrainLayer)
			{
				return splatmapUserData;
			}
			splatmapUserData = null;
		}
		if (splatmapUserData == null)
		{
			int num = -1;
			if (terrainLayer != null)
			{
				num = TerrainPaintUtility.FindTerrainLayerIndex(context.terrain, terrainLayer);
				if (num == -1 && addLayerIfDoesntExist)
				{
					num = TerrainPaintUtility.AddTerrainLayer(context.terrain, terrainLayer);
				}
			}
			if (num != -1)
			{
				splatmapUserData = new SplatmapUserData();
				splatmapUserData.terrainLayer = terrainLayer;
				splatmapUserData.terrainLayerIndex = num;
				splatmapUserData.mapIndex = num >> 2;
				splatmapUserData.channelIndex = num & 3;
			}
			context.userData = splatmapUserData;
		}
		return splatmapUserData;
	}

	public void GatherAlphamap(TerrainLayer inputLayer, bool addLayerIfDoesntExist = true)
	{
		if (!(inputLayer == null))
		{
			Material copyTerrainLayerMaterial = TerrainPaintUtility.GetCopyTerrainLayerMaterial();
			Vector4[] layerMasks = new Vector4[4]
			{
				new Vector4(1f, 0f, 0f, 0f),
				new Vector4(0f, 1f, 0f, 0f),
				new Vector4(0f, 0f, 1f, 0f),
				new Vector4(0f, 0f, 0f, 1f)
			};
			GatherInternal(delegate(ITerrainInfo t)
			{
				SplatmapUserData terrainLayerUserData = GetTerrainLayerUserData(t, inputLayer, addLayerIfDoesntExist);
				return (terrainLayerUserData != null) ? TerrainPaintUtility.GetTerrainAlphaMapChecked(t.terrain, terrainLayerUserData.mapIndex) : null;
			}, new Color(0f, 0f, 0f, 0f), "PaintContext.GatherAlphamap", copyTerrainLayerMaterial, 0, delegate(ITerrainInfo t)
			{
				SplatmapUserData terrainLayerUserData2 = GetTerrainLayerUserData(t);
				copyTerrainLayerMaterial.SetVector("_LayerMask", layerMasks[terrainLayerUserData2.channelIndex]);
			});
		}
	}

	public void ScatterAlphamap(string editorUndoName)
	{
		Vector4[] layerMasks = new Vector4[4]
		{
			new Vector4(1f, 0f, 0f, 0f),
			new Vector4(0f, 1f, 0f, 0f),
			new Vector4(0f, 0f, 1f, 0f),
			new Vector4(0f, 0f, 0f, 1f)
		};
		Material copyTerrainLayerMaterial = TerrainPaintUtility.GetCopyTerrainLayerMaterial();
		RenderTextureDescriptor desc = new RenderTextureDescriptor(destinationRenderTexture.width, destinationRenderTexture.height, RenderTextureFormat.ARGB32);
		desc.sRGB = false;
		desc.useMipMap = false;
		desc.autoGenerateMips = false;
		RenderTexture tempTarget = RenderTexture.GetTemporary(desc);
		ScatterInternal(delegate(ITerrainInfo t)
		{
			SplatmapUserData terrainLayerUserData = GetTerrainLayerUserData(t);
			if (terrainLayerUserData != null)
			{
				PaintContext.onTerrainTileBeforePaint?.Invoke(t, ToolAction.PaintTexture, editorUndoName);
				int mapIndex = terrainLayerUserData.mapIndex;
				int channelIndex = terrainLayerUserData.channelIndex;
				Texture2D value = t.terrain.terrainData.alphamapTextures[mapIndex];
				destinationRenderTexture.filterMode = FilterMode.Point;
				sourceRenderTexture.filterMode = FilterMode.Point;
				for (int i = 0; i <= t.terrain.terrainData.alphamapTextureCount; i++)
				{
					if (i != mapIndex)
					{
						int num = ((i == t.terrain.terrainData.alphamapTextureCount) ? mapIndex : i);
						Texture2D texture2D = t.terrain.terrainData.alphamapTextures[num];
						if (texture2D.width != targetTextureWidth || texture2D.height != targetTextureHeight)
						{
							Debug.LogWarning("PaintContext alphamap operations must use the same resolution for all Terrains - mismatched Terrains are ignored.", t.terrain);
						}
						else
						{
							RenderTexture.active = tempTarget;
							GL.PushMatrix();
							GL.LoadPixelMatrix(0f, tempTarget.width, 0f, tempTarget.height);
							copyTerrainLayerMaterial.SetTexture("_MainTex", destinationRenderTexture);
							copyTerrainLayerMaterial.SetTexture("_OldAlphaMapTexture", sourceRenderTexture);
							copyTerrainLayerMaterial.SetTexture("_OriginalTargetAlphaMap", value);
							copyTerrainLayerMaterial.SetTexture("_AlphaMapTexture", texture2D);
							copyTerrainLayerMaterial.SetVector("_LayerMask", (num == mapIndex) ? layerMasks[channelIndex] : Vector4.zero);
							copyTerrainLayerMaterial.SetVector("_OriginalTargetAlphaMask", layerMasks[channelIndex]);
							copyTerrainLayerMaterial.SetPass(1);
							TerrainPaintUtility.DrawQuad2(t.clippedPCPixels, t.clippedPCPixels, destinationRenderTexture, t.clippedTerrainPixels, texture2D);
							GL.PopMatrix();
							t.terrain.terrainData.CopyActiveRenderTextureToTexture(TerrainData.AlphamapTextureName, num, t.clippedPCPixels, t.clippedTerrainPixels.min, allowDelayedCPUSync: true);
						}
					}
				}
				RenderTexture.active = null;
				OnTerrainPainted(t, ToolAction.PaintTexture);
			}
			return (RenderTexture)null;
		}, "PaintContext.ScatterAlphamap", copyTerrainLayerMaterial);
		RenderTexture.ReleaseTemporary(tempTarget);
	}

	private static void OnTerrainPainted(ITerrainInfo tile, ToolAction action)
	{
		for (int i = 0; i < s_PaintedTerrain.Count; i++)
		{
			if (tile.terrain == s_PaintedTerrain[i].terrain)
			{
				PaintedTerrain value = s_PaintedTerrain[i];
				value.action |= action;
				s_PaintedTerrain[i] = value;
				return;
			}
		}
		s_PaintedTerrain.Add(new PaintedTerrain
		{
			terrain = tile.terrain,
			action = action
		});
	}

	public static void ApplyDelayedActions()
	{
		for (int i = 0; i < s_PaintedTerrain.Count; i++)
		{
			PaintedTerrain paintedTerrain = s_PaintedTerrain[i];
			TerrainData terrainData = paintedTerrain.terrain.terrainData;
			if (!(terrainData == null))
			{
				if ((paintedTerrain.action & ToolAction.PaintHeightmap) != 0)
				{
					terrainData.SyncHeightmap();
				}
				if ((paintedTerrain.action & ToolAction.PaintHoles) != 0)
				{
					terrainData.SyncTexture(TerrainData.HolesTextureName);
				}
				if ((paintedTerrain.action & ToolAction.PaintTexture) != 0)
				{
					terrainData.SetBaseMapDirty();
					terrainData.SyncTexture(TerrainData.AlphamapTextureName);
				}
				paintedTerrain.terrain.editorRenderFlags = TerrainRenderFlags.all;
			}
		}
		s_PaintedTerrain.Clear();
	}
}
