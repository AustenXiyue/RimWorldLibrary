using System.IO;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Navigation;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Effects;

/// <summary>Provides a managed wrapper around a High Level Shading Language (HLSL) pixel shader.</summary>
public sealed class PixelShader : Animatable, DUCE.IResource
{
	private SecurityCriticalData<byte[]> _shaderBytecode;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.PixelShader.UriSource" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.PixelShader.UriSource" /> dependency property.</returns>
	public static readonly DependencyProperty UriSourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.PixelShader.ShaderRenderMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.PixelShader.ShaderRenderMode" /> dependency property.</returns>
	public static readonly DependencyProperty ShaderRenderModeProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Uri s_UriSource;

	internal const ShaderRenderMode c_ShaderRenderMode = ShaderRenderMode.Auto;

	internal short ShaderMajorVersion { get; private set; }

	internal short ShaderMinorVersion { get; private set; }

	/// <summary>Gets or sets a Pack URI reference to HLSL bytecode in the assembly. </summary>
	/// <returns>The Pack URI reference to HLSL bytecode in the assembly. </returns>
	public Uri UriSource
	{
		get
		{
			return (Uri)GetValue(UriSourceProperty);
		}
		set
		{
			SetValueInternal(UriSourceProperty, value);
		}
	}

	/// <summary>Gets or sets a value indicating whether to use hardware or software rendering. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Effects.ShaderRenderMode" /> value that indicates whether to force the use of hardware or software rendering for the effect.</returns>
	public ShaderRenderMode ShaderRenderMode
	{
		get
		{
			return (ShaderRenderMode)GetValue(ShaderRenderModeProperty);
		}
		set
		{
			SetValueInternal(ShaderRenderModeProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Occurs when the render thread cannot process the pixel shader.  </summary>
	public static event EventHandler InvalidPixelShaderEncountered
	{
		add
		{
			MediaContext.CurrentMediaContext.InvalidPixelShaderEncountered += value;
		}
		remove
		{
			MediaContext.CurrentMediaContext.InvalidPixelShaderEncountered -= value;
		}
	}

	internal event EventHandler _shaderBytecodeChanged;

	/// <summary>Assigns the <see cref="T:System.IO.Stream" /> to use as the source of HLSL bytecode.</summary>
	/// <param name="source">The stream to read the HLSL bytecode from.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public void SetStreamSource(Stream source)
	{
		WritePreamble();
		LoadPixelShaderFromStreamIntoMemory(source);
		WritePostscript();
	}

	private void UriSourcePropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		Uri uri = (Uri)e.NewValue;
		Stream stream = null;
		try
		{
			if (uri != null)
			{
				if (!uri.IsAbsoluteUri)
				{
					uri = BaseUriHelper.GetResolvedUri(BaseUriHelper.BaseUri, uri);
				}
				if (!uri.IsFile && !PackUriHelper.IsPackUri(uri))
				{
					throw new ArgumentException(SR.Effect_SourceUriMustBeFileOrPack);
				}
				stream = WpfWebRequestHelper.CreateRequestAndGetResponseStream(uri);
			}
			LoadPixelShaderFromStreamIntoMemory(stream);
		}
		finally
		{
			stream?.Dispose();
		}
	}

	private void LoadPixelShaderFromStreamIntoMemory(Stream source)
	{
		_shaderBytecode = new SecurityCriticalData<byte[]>(null);
		if (source != null)
		{
			if (!source.CanSeek)
			{
				throw new InvalidOperationException(SR.Effect_ShaderSeekableStream);
			}
			int num = (int)source.Length;
			if (num % 4 != 0)
			{
				throw new InvalidOperationException(SR.Effect_ShaderBytecodeSize);
			}
			BinaryReader binaryReader = new BinaryReader(source);
			_shaderBytecode = new SecurityCriticalData<byte[]>(new byte[num]);
			binaryReader.Read(_shaderBytecode.Value, 0, num);
			if (_shaderBytecode.Value != null && _shaderBytecode.Value.Length > 3)
			{
				ShaderMajorVersion = _shaderBytecode.Value[1];
				ShaderMinorVersion = _shaderBytecode.Value[0];
			}
			else
			{
				short shaderMajorVersion = (ShaderMinorVersion = 0);
				ShaderMajorVersion = shaderMajorVersion;
			}
		}
		RegisterForAsyncUpdateResource();
		if (this._shaderBytecodeChanged != null)
		{
			this._shaderBytecodeChanged(this, null);
		}
	}

	private unsafe void ManualUpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (!skipOnChannelCheck && !_duceResource.IsOnChannel(channel))
		{
			return;
		}
		DUCE.MILCMD_PIXELSHADER mILCMD_PIXELSHADER = default(DUCE.MILCMD_PIXELSHADER);
		mILCMD_PIXELSHADER.Type = MILCMD.MilCmdPixelShader;
		mILCMD_PIXELSHADER.Handle = _duceResource.GetHandle(channel);
		checked
		{
			mILCMD_PIXELSHADER.PixelShaderBytecodeSize = ((_shaderBytecode.Value != null) ? ((uint)_shaderBytecode.Value.Length) : 0u);
			mILCMD_PIXELSHADER.ShaderRenderMode = ShaderRenderMode;
			mILCMD_PIXELSHADER.CompileSoftwareShader = CompositionResourceManager.BooleanToUInt32(ShaderMajorVersion != 3 || ShaderMinorVersion != 0);
			channel.BeginCommand(unchecked((byte*)(&mILCMD_PIXELSHADER)), sizeof(DUCE.MILCMD_PIXELSHADER), (int)mILCMD_PIXELSHADER.PixelShaderBytecodeSize);
			if (mILCMD_PIXELSHADER.PixelShaderBytecodeSize != 0)
			{
				fixed (byte* value = _shaderBytecode.Value)
				{
					channel.AppendCommandData(value, (int)mILCMD_PIXELSHADER.PixelShaderBytecodeSize);
				}
			}
			channel.EndCommand();
		}
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		PixelShader shader = (PixelShader)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(shader);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		PixelShader shader = (PixelShader)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(shader);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		PixelShader shader = (PixelShader)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(shader);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		PixelShader shader = (PixelShader)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(shader);
	}

	private void CopyCommon(PixelShader shader)
	{
		byte[] value = shader._shaderBytecode.Value;
		byte[] array = null;
		if (value != null)
		{
			array = new byte[value.Length];
			value.CopyTo(array, 0);
		}
		_shaderBytecode = new SecurityCriticalData<byte[]>(array);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.PixelShader" /> object, making deep copies of this object's values. When copying this object's dependency properties, this method copies resource references and data bindings (which may no longer resolve), but not animations or their current values. </summary>
	/// <returns>A modifiable clone of this instance. The returned clone is effectively a deep copy of the current object. The clone's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false.</returns>
	public new PixelShader Clone()
	{
		return (PixelShader)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.PixelShader" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are copied. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PixelShader CloneCurrentValue()
	{
		return (PixelShader)base.CloneCurrentValue();
	}

	private static void UriSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		PixelShader obj = (PixelShader)d;
		obj.UriSourcePropertyChangedHook(e);
		obj.PropertyChanged(UriSourceProperty);
	}

	private static void ShaderRenderModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PixelShader)d).PropertyChanged(ShaderRenderModeProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new PixelShader();
	}

	internal override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		ManualUpdateResource(channel, skipOnChannelCheck);
		base.UpdateResource(channel, skipOnChannelCheck);
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_PIXELSHADER))
			{
				AddRefOnChannelAnimations(channel);
				UpdateResource(channel, skipOnChannelCheck: true);
			}
			return _duceResource.GetHandle(channel);
		}
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.ReleaseOnChannel(channel))
			{
				ReleaseOnChannelAnimations(channel);
			}
		}
	}

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return _duceResource.GetHandle(channel);
		}
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _duceResource.GetChannelCount();
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static PixelShader()
	{
		Type typeFromHandle = typeof(PixelShader);
		UriSourceProperty = Animatable.RegisterProperty("UriSource", typeof(Uri), typeFromHandle, null, UriSourcePropertyChanged, null, isIndependentlyAnimated: false, null);
		ShaderRenderModeProperty = Animatable.RegisterProperty("ShaderRenderMode", typeof(ShaderRenderMode), typeFromHandle, ShaderRenderMode.Auto, ShaderRenderModePropertyChanged, ValidateEnums.IsShaderRenderModeValid, isIndependentlyAnimated: false, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.PixelShader" /> class. </summary>
	public PixelShader()
	{
	}
}
