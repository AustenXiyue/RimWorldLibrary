using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Rendering;

namespace UnityEngine;

[NativeHeader("Runtime/Misc/PlayerSettings.h")]
[NativeHeader("Runtime/Graphics/QualitySettings.h")]
[StaticAccessor("GetQualitySettings()", StaticAccessorType.Dot)]
public sealed class QualitySettings : Object
{
	[Obsolete("Use GetQualityLevel and SetQualityLevel", false)]
	public static QualityLevel currentLevel
	{
		get
		{
			return (QualityLevel)GetQualityLevel();
		}
		set
		{
			SetQualityLevel((int)value, applyExpensiveChanges: true);
		}
	}

	public static extern int pixelLightCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("ShadowQuality")]
	public static extern ShadowQuality shadows
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern ShadowProjection shadowProjection
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int shadowCascades
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern float shadowDistance
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("ShadowResolution")]
	public static extern ShadowResolution shadowResolution
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("ShadowmaskMode")]
	public static extern ShadowmaskMode shadowmaskMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern float shadowNearPlaneOffset
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern float shadowCascade2Split
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static Vector3 shadowCascade4Split
	{
		get
		{
			get_shadowCascade4Split_Injected(out var ret);
			return ret;
		}
		set
		{
			set_shadowCascade4Split_Injected(ref value);
		}
	}

	[NativeProperty("LODBias")]
	public static extern float lodBias
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("AnisotropicTextures")]
	public static extern AnisotropicFiltering anisotropicFiltering
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int masterTextureLimit
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int maximumLODLevel
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int particleRaycastBudget
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool softParticles
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool softVegetation
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int vSyncCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int antiAliasing
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int asyncUploadTimeSlice
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int asyncUploadBufferSize
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool asyncUploadPersistentBuffer
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool realtimeReflectionProbes
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool billboardsFaceCameraPosition
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern float resolutionScalingFixedDPIFactor
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeName("RenderPipeline")]
	private static extern ScriptableObject INTERNAL_renderPipeline
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static RenderPipelineAsset renderPipeline
	{
		get
		{
			return INTERNAL_renderPipeline as RenderPipelineAsset;
		}
		set
		{
			INTERNAL_renderPipeline = value;
		}
	}

	[Obsolete("blendWeights is obsolete. Use skinWeights instead (UnityUpgradable) -> skinWeights", true)]
	public static extern BlendWeights blendWeights
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetSkinWeights")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("SetSkinWeights")]
		set;
	}

	public static extern SkinWeights skinWeights
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool streamingMipmapsActive
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern float streamingMipmapsMemoryBudget
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int streamingMipmapsRenderersPerFrame
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static extern int streamingMipmapsMaxLevelReduction
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool streamingMipmapsAddAllCameras
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern int streamingMipmapsMaxFileIORequests
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[StaticAccessor("QualitySettingsScripting", StaticAccessorType.DoubleColon)]
	public static extern int maxQueuedFrames
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("QualitySettingsNames")]
	public static extern string[] names
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static extern ColorSpace desiredColorSpace
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetColorSpace")]
		[StaticAccessor("GetPlayerSettings()", StaticAccessorType.Dot)]
		get;
	}

	public static extern ColorSpace activeColorSpace
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetColorSpace")]
		[StaticAccessor("GetPlayerSettings()", StaticAccessorType.Dot)]
		get;
	}

	public static void IncreaseLevel([DefaultValue("false")] bool applyExpensiveChanges)
	{
		SetQualityLevel(GetQualityLevel() + 1, applyExpensiveChanges);
	}

	public static void DecreaseLevel([DefaultValue("false")] bool applyExpensiveChanges)
	{
		SetQualityLevel(GetQualityLevel() - 1, applyExpensiveChanges);
	}

	public static void SetQualityLevel(int index)
	{
		SetQualityLevel(index, applyExpensiveChanges: true);
	}

	public static void IncreaseLevel()
	{
		IncreaseLevel(applyExpensiveChanges: false);
	}

	public static void DecreaseLevel()
	{
		DecreaseLevel(applyExpensiveChanges: false);
	}

	private QualitySettings()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetRenderPipelineAssetAt")]
	internal static extern ScriptableObject InternalGetRenderPipelineAssetAt(int index);

	public static RenderPipelineAsset GetRenderPipelineAssetAt(int index)
	{
		if (index < 0 || index >= names.Length)
		{
			throw new IndexOutOfRangeException(string.Format("{0} is out of range [0..{1}[", "index", names.Length));
		}
		return InternalGetRenderPipelineAssetAt(index) as RenderPipelineAsset;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetCurrentIndex")]
	public static extern int GetQualityLevel();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("SetCurrentIndex")]
	public static extern void SetQualityLevel(int index, [DefaultValue("true")] bool applyExpensiveChanges);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void get_shadowCascade4Split_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void set_shadowCascade4Split_Injected(ref Vector3 value);
}
