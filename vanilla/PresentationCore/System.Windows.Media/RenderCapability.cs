namespace System.Windows.Media;

/// <summary>Enables WPF applications to query for the current rendering tier for their associated <see cref="T:System.Windows.Threading.Dispatcher" /> object and to register for notification of changes. </summary>
public static class RenderCapability
{
	private const string IsShaderEffectSoftwareRenderingSupported_Deprecated = "IsShaderEffectSoftwareRenderingSupported property is deprecated.  Use IsPixelShaderVersionSupportedInSoftware static method instead.";

	/// <summary>Gets a value that indicates the rendering tier for the current thread.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value whose high-order word corresponds to the rendering tier for the current thread.</returns>
	public static int Tier => MediaContext.CurrentMediaContext.Tier;

	/// <summary>Gets a value that indicates whether the system can render bitmap effects in software.</summary>
	/// <returns>true if the system can render bitmap effects in software; otherwise, false. </returns>
	[Obsolete("IsShaderEffectSoftwareRenderingSupported property is deprecated.  Use IsPixelShaderVersionSupportedInSoftware static method instead.")]
	public static bool IsShaderEffectSoftwareRenderingSupported => MediaContext.CurrentMediaContext.HasSSE2Support;

	/// <summary>Gets the maximum width and height for bitmap creation of the underlying hardware device.  </summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> that represents the maximum width and height for hardware bitmap creation.</returns>
	public static Size MaxHardwareTextureSize => MediaContext.CurrentMediaContext.MaxTextureSize;

	/// <summary>Occurs when the rendering tier has changed for the <see cref="T:System.Windows.Threading.Dispatcher" /> object of the current thread.</summary>
	public static event EventHandler TierChanged
	{
		add
		{
			MediaContext.CurrentMediaContext.TierChanged += value;
		}
		remove
		{
			MediaContext.CurrentMediaContext.TierChanged -= value;
		}
	}

	/// <summary>Gets a value that indicates whether the specified pixel shader version is supported. </summary>
	/// <returns>true if the pixel shader version is supported by the current version of WPF; otherwise, false. </returns>
	/// <param name="majorVersionRequested">The major version of the pixel shader.</param>
	/// <param name="minorVersionRequested">The minor version of the pixel shader.</param>
	public static bool IsPixelShaderVersionSupported(short majorVersionRequested, short minorVersionRequested)
	{
		bool result = false;
		if ((majorVersionRequested == 2 && minorVersionRequested == 0) || (majorVersionRequested == 3 && minorVersionRequested == 0))
		{
			MediaContext currentMediaContext = MediaContext.CurrentMediaContext;
			byte b = (byte)((currentMediaContext.PixelShaderVersion >> 8) & 0xFF);
			byte b2 = (byte)(currentMediaContext.PixelShaderVersion & 0xFF);
			if (b >= majorVersionRequested)
			{
				result = true;
			}
			else if (b == majorVersionRequested && b2 >= minorVersionRequested)
			{
				result = true;
			}
		}
		return result;
	}

	/// <summary>Gets a value that indicates whether the specified pixel shader version can be rendered in software on the current system. </summary>
	/// <returns>true if the pixel shader can be rendered in software on the current system; otherwise, false.</returns>
	/// <param name="majorVersionRequested">The major version of the pixel shader.</param>
	/// <param name="minorVersionRequested">The minor version of the pixel shader.</param>
	public static bool IsPixelShaderVersionSupportedInSoftware(short majorVersionRequested, short minorVersionRequested)
	{
		bool result = false;
		if (majorVersionRequested == 2 && minorVersionRequested == 0)
		{
			result = MediaContext.CurrentMediaContext.HasSSE2Support;
		}
		return result;
	}

	/// <summary>Gets the maximum number of instruction slots supported by the specified pixel shader version.</summary>
	/// <returns>96 for Pixel Shader 2.0, 512 or greater for Pixel Shader 3.0, or 0 for any other version of Pixel Shader.</returns>
	/// <param name="majorVersionRequested">The major version of the pixel shader.</param>
	/// <param name="minorVersionRequested">The minor version of the pixel shader.</param>
	public static int MaxPixelShaderInstructionSlots(short majorVersionRequested, short minorVersionRequested)
	{
		if (majorVersionRequested == 2 && minorVersionRequested == 0)
		{
			return 96;
		}
		if (majorVersionRequested == 3 && minorVersionRequested == 0)
		{
			return (int)MediaContext.CurrentMediaContext.MaxPixelShader30InstructionSlots;
		}
		return 0;
	}
}
