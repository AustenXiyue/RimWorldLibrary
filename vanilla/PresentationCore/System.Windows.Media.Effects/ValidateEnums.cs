namespace System.Windows.Media.Effects;

internal static class ValidateEnums
{
	public static bool IsShaderRenderModeValid(object valueObject)
	{
		ShaderRenderMode shaderRenderMode = (ShaderRenderMode)valueObject;
		if (shaderRenderMode != 0 && shaderRenderMode != ShaderRenderMode.SoftwareOnly)
		{
			return shaderRenderMode == ShaderRenderMode.HardwareOnly;
		}
		return true;
	}

	public static bool IsKernelTypeValid(object valueObject)
	{
		KernelType kernelType = (KernelType)valueObject;
		if (kernelType != 0)
		{
			return kernelType == KernelType.Box;
		}
		return true;
	}

	public static bool IsEdgeProfileValid(object valueObject)
	{
		EdgeProfile edgeProfile = (EdgeProfile)valueObject;
		if (edgeProfile != 0 && edgeProfile != EdgeProfile.CurvedIn && edgeProfile != EdgeProfile.CurvedOut)
		{
			return edgeProfile == EdgeProfile.BulgedUp;
		}
		return true;
	}

	public static bool IsRenderingBiasValid(object valueObject)
	{
		RenderingBias renderingBias = (RenderingBias)valueObject;
		if (renderingBias != 0)
		{
			return renderingBias == RenderingBias.Quality;
		}
		return true;
	}
}
