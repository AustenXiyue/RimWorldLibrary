using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Media3D;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Effects;

/// <summary>Provides a custom bitmap effect by using a <see cref="T:System.Windows.Media.Effects.PixelShader" />. </summary>
public abstract class ShaderEffect : Effect
{
	private struct SamplerData
	{
		public Brush _brush;

		public SamplingMode _samplingMode;
	}

	private const SamplingMode _defaultSamplingMode = SamplingMode.Auto;

	private double _topPadding;

	private double _bottomPadding;

	private double _leftPadding;

	private double _rightPadding;

	private List<MilColorF?> _floatRegisters;

	private List<MilColorI?> _intRegisters;

	private List<bool?> _boolRegisters;

	private List<SamplerData?> _samplerData;

	private uint _floatCount;

	private uint _intCount;

	private uint _boolCount;

	private uint _samplerCount;

	private int _ddxUvDdyUvRegisterIndex = -1;

	private const int PS_2_0_FLOAT_REGISTER_LIMIT = 32;

	private const int PS_3_0_FLOAT_REGISTER_LIMIT = 224;

	private const int PS_3_0_INT_REGISTER_LIMIT = 16;

	private const int PS_3_0_BOOL_REGISTER_LIMIT = 16;

	private const int PS_2_0_SAMPLER_LIMIT = 4;

	private const int PS_3_0_SAMPLER_LIMIT = 8;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.ShaderEffect.PixelShader" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.ShaderEffect.PixelShader" /> dependency property. </returns>
	protected static readonly DependencyProperty PixelShaderProperty;

	internal DUCE.MultiChannelResource _duceResource;

	/// <summary>Gets or sets a value indicating that the effect's output texture is larger than its input texture along the top edge.</summary>
	/// <returns>The padding along the top edge of the effect.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The provided value is less than 0.</exception>
	protected double PaddingTop
	{
		get
		{
			ReadPreamble();
			return _topPadding;
		}
		set
		{
			WritePreamble();
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("value", value, SR.Effect_ShaderEffectPadding);
			}
			_topPadding = value;
			RegisterForAsyncUpdateResource();
			WritePostscript();
		}
	}

	/// <summary>Gets or sets a value indicating that the effect's output texture is larger than its input texture along the bottom edge. </summary>
	/// <returns>The padding along the bottom edge of the effect. </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The provided value is less than 0.</exception>
	protected double PaddingBottom
	{
		get
		{
			ReadPreamble();
			return _bottomPadding;
		}
		set
		{
			WritePreamble();
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("value", value, SR.Effect_ShaderEffectPadding);
			}
			_bottomPadding = value;
			RegisterForAsyncUpdateResource();
			WritePostscript();
		}
	}

	/// <summary>Gets or sets a value indicating that the effect's output texture is larger than its input texture along the left edge. </summary>
	/// <returns>The padding along the left edge of the effect. </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The provided value is less than 0.</exception>
	protected double PaddingLeft
	{
		get
		{
			ReadPreamble();
			return _leftPadding;
		}
		set
		{
			WritePreamble();
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("value", value, SR.Effect_ShaderEffectPadding);
			}
			_leftPadding = value;
			RegisterForAsyncUpdateResource();
			WritePostscript();
		}
	}

	/// <summary>Gets or sets a value indicating that the effect's output texture is larger than its input texture along the right edge.</summary>
	/// <returns>The padding along the right edge of the effect.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The provided value is less than 0.</exception>
	protected double PaddingRight
	{
		get
		{
			ReadPreamble();
			return _rightPadding;
		}
		set
		{
			WritePreamble();
			if (value < 0.0)
			{
				throw new ArgumentOutOfRangeException("value", value, SR.Effect_ShaderEffectPadding);
			}
			_rightPadding = value;
			RegisterForAsyncUpdateResource();
			WritePostscript();
		}
	}

	/// <summary>Gets or sets a value that indicates the shader register to use for the partial derivatives of the texture coordinates with respect to screen space. </summary>
	/// <returns>The index of the register that contains the partial derivatives.</returns>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to set the <see cref="P:System.Windows.Media.Effects.ShaderEffect.DdxUvDdyUvRegisterIndex" /> property more than one time or after initial processing of the effect. </exception>
	protected int DdxUvDdyUvRegisterIndex
	{
		get
		{
			ReadPreamble();
			return _ddxUvDdyUvRegisterIndex;
		}
		set
		{
			WritePreamble();
			_ddxUvDdyUvRegisterIndex = value;
			WritePostscript();
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Effects.PixelShader" /> to use for the effect. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Effects.PixelShader" /> for the effect. </returns>
	protected PixelShader PixelShader
	{
		get
		{
			return (PixelShader)GetValue(PixelShaderProperty);
		}
		set
		{
			SetValueInternal(PixelShaderProperty, value);
		}
	}

	internal override Rect GetRenderBounds(Rect contentBounds)
	{
		Point point = default(Point);
		Point point2 = default(Point);
		point.X = contentBounds.TopLeft.X - PaddingLeft;
		point.Y = contentBounds.TopLeft.Y - PaddingTop;
		point2.X = contentBounds.BottomRight.X + PaddingRight;
		point2.Y = contentBounds.BottomRight.Y + PaddingBottom;
		return new Rect(point, point2);
	}

	private void PixelShaderPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		PixelShader pixelShader = (PixelShader)e.OldValue;
		if (pixelShader != null)
		{
			pixelShader._shaderBytecodeChanged -= OnPixelShaderBytecodeChanged;
		}
		PixelShader pixelShader2 = (PixelShader)e.NewValue;
		if (pixelShader2 != null)
		{
			pixelShader2._shaderBytecodeChanged += OnPixelShaderBytecodeChanged;
		}
		OnPixelShaderBytecodeChanged(PixelShader, null);
	}

	private void OnPixelShaderBytecodeChanged(object sender, EventArgs e)
	{
		PixelShader pixelShader = (PixelShader)sender;
		if (pixelShader != null && pixelShader.ShaderMajorVersion == 2 && pixelShader.ShaderMinorVersion == 0 && UsesPS30OnlyRegisters())
		{
			throw new InvalidOperationException(SR.Effect_20ShaderUsing30Registers);
		}
	}

	private bool UsesPS30OnlyRegisters()
	{
		if (_intCount != 0 || _intRegisters != null || _boolCount != 0 || _boolRegisters != null)
		{
			return true;
		}
		if (_floatRegisters != null)
		{
			for (int i = 32; i < _floatRegisters.Count; i++)
			{
				if (_floatRegisters[i].HasValue)
				{
					return true;
				}
			}
		}
		if (_samplerData != null)
		{
			for (int j = 4; j < _samplerData.Count; j++)
			{
				if (_samplerData[j].HasValue)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Notifies the effect that the shader constant or sampler corresponding to the specified dependency property should be updated. </summary>
	/// <param name="dp">The dependency property to be updated. </param>
	protected void UpdateShaderValue(DependencyProperty dp)
	{
		if (dp != null)
		{
			WritePreamble();
			object value = GetValue(dp);
			dp.GetMetadata(this)?.PropertyChangedCallback?.Invoke(this, new DependencyPropertyChangedEventArgs(dp, value, value));
			WritePostscript();
		}
	}

	/// <summary>Associates a dependency property value with a pixel shader's float constant register. </summary>
	/// <returns>A <see cref="T:System.Windows.PropertyChangedCallback" /> delegate that associates a dependency property and the shader constant register specified by <paramref name="floatRegisterIndex" />.</returns>
	/// <param name="floatRegisterIndex">The index of the shader register associated with the dependency property. </param>
	/// <exception cref="T:System.InvalidOperationException">The dependency property is an unknown type. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="floatRegisterIndex" /> is greater than or equal to 32, or <paramref name="floatRegisterIndex" /> is less than 0. </exception>
	protected static PropertyChangedCallback PixelShaderConstantCallback(int floatRegisterIndex)
	{
		return delegate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			if (obj is ShaderEffect shaderEffect)
			{
				shaderEffect.UpdateShaderConstant(args.Property, args.NewValue, floatRegisterIndex);
			}
		};
	}

	/// <summary>Associates a dependency property value with a pixel shader's sampler register. </summary>
	/// <returns>A <see cref="T:System.Windows.PropertyChangedCallback" /> delegate that associates a dependency property and the shader sampler register specified by <paramref name="samplerRegisterIndex" />. </returns>
	/// <param name="samplerRegisterIndex">The index of the shader sampler associated with the dependency property.</param>
	protected static PropertyChangedCallback PixelShaderSamplerCallback(int samplerRegisterIndex)
	{
		return PixelShaderSamplerCallback(samplerRegisterIndex, SamplingMode.Auto);
	}

	/// <summary>Associates a dependency property value with a pixel shader's sampler register and a <see cref="T:System.Windows.Media.Effects.SamplingMode" />. </summary>
	/// <returns>A <see cref="T:System.Windows.PropertyChangedCallback" /> delegate that associates a dependency property and the shader sampler register specified by <paramref name="samplerRegisterIndex" />. </returns>
	/// <param name="samplerRegisterIndex">The index of the shader sampler associated with the dependency property.</param>
	/// <param name="samplingMode">The <see cref="T:System.Windows.Media.Effects.SamplingMode" /> for the shader sampler.</param>
	protected static PropertyChangedCallback PixelShaderSamplerCallback(int samplerRegisterIndex, SamplingMode samplingMode)
	{
		return delegate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			if (obj is ShaderEffect shaderEffect && args.IsAValueChange)
			{
				shaderEffect.UpdateShaderSampler(args.Property, args.NewValue, samplerRegisterIndex, samplingMode);
			}
		};
	}

	/// <summary>Associates a dependency property with a shader sampler register.</summary>
	/// <returns>A dependency property associated with the shader sampler specified by <paramref name="samplerRegisterIndex" />. </returns>
	/// <param name="dpName">The name of the dependency property.</param>
	/// <param name="ownerType">The type of the effect that has the dependency property. </param>
	/// <param name="samplerRegisterIndex">The index of the shader sampler associated with the dependency property.</param>
	protected static DependencyProperty RegisterPixelShaderSamplerProperty(string dpName, Type ownerType, int samplerRegisterIndex)
	{
		return RegisterPixelShaderSamplerProperty(dpName, ownerType, samplerRegisterIndex, SamplingMode.Auto);
	}

	/// <summary>Associates a dependency property with a shader sampler register and a <see cref="T:System.Windows.Media.Effects.SamplingMode" />.</summary>
	/// <returns>A dependency property associated with the shader sampler specified by <paramref name="samplerRegisterIndex" />. </returns>
	/// <param name="dpName">The name of the dependency property.</param>
	/// <param name="ownerType">The type of the effect that has the dependency property. </param>
	/// <param name="samplerRegisterIndex">The index of the shader sampler associated with the dependency property.</param>
	/// <param name="samplingMode">The <see cref="T:System.Windows.Media.Effects.SamplingMode" /> for the shader sampler.</param>
	protected static DependencyProperty RegisterPixelShaderSamplerProperty(string dpName, Type ownerType, int samplerRegisterIndex, SamplingMode samplingMode)
	{
		return DependencyProperty.Register(dpName, typeof(Brush), ownerType, new UIPropertyMetadata(Effect.ImplicitInput, PixelShaderSamplerCallback(samplerRegisterIndex, samplingMode)));
	}

	private void UpdateShaderConstant(DependencyProperty dp, object newValue, int registerIndex)
	{
		WritePreamble();
		Type type = DetermineShaderConstantType(dp.PropertyType, PixelShader);
		if (type == null)
		{
			throw new InvalidOperationException(SR.Format(SR.Effect_ShaderConstantType, dp.PropertyType.Name));
		}
		int num = 32;
		string resourceKey = "Effect_Shader20ConstantRegisterLimit";
		if (PixelShader != null && PixelShader.ShaderMajorVersion >= 3)
		{
			if (type == typeof(float))
			{
				num = 224;
				resourceKey = "Effect_Shader30FloatConstantRegisterLimit";
			}
			else if (type == typeof(int))
			{
				num = 16;
				resourceKey = "Effect_Shader30IntConstantRegisterLimit";
			}
			else if (type == typeof(bool))
			{
				num = 16;
				resourceKey = "Effect_Shader30BoolConstantRegisterLimit";
			}
		}
		if (registerIndex >= num || registerIndex < 0)
		{
			throw new ArgumentException(SR.GetResourceString(resourceKey), "dp");
		}
		if (type == typeof(float))
		{
			ConvertValueToMilColorF(newValue, out var newVal);
			StashInPosition(ref _floatRegisters, registerIndex, newVal, num, ref _floatCount);
		}
		else if (type == typeof(int))
		{
			ConvertValueToMilColorI(newValue, out var newVal2);
			StashInPosition(ref _intRegisters, registerIndex, newVal2, num, ref _intCount);
		}
		else if (type == typeof(bool))
		{
			StashInPosition(ref _boolRegisters, registerIndex, (bool)newValue, num, ref _boolCount);
		}
		PropertyChanged(dp);
		WritePostscript();
	}

	private void UpdateShaderSampler(DependencyProperty dp, object newValue, int registerIndex, SamplingMode samplingMode)
	{
		WritePreamble();
		if (newValue != null && !(newValue is VisualBrush) && !(newValue is BitmapCacheBrush) && !(newValue is ImplicitInputBrush) && !(newValue is ImageBrush))
		{
			throw new ArgumentException(SR.Effect_ShaderSamplerType, "dp");
		}
		int num = 4;
		string resourceKey = "Effect_Shader20SamplerRegisterLimit";
		if (PixelShader != null && PixelShader.ShaderMajorVersion >= 3)
		{
			num = 8;
			resourceKey = "Effect_Shader30SamplerRegisterLimit";
		}
		if (registerIndex >= num || registerIndex < 0)
		{
			throw new ArgumentException(SR.GetResourceString(resourceKey));
		}
		SamplerData samplerData = default(SamplerData);
		samplerData._brush = (Brush)newValue;
		samplerData._samplingMode = samplingMode;
		SamplerData newSampler = samplerData;
		StashSamplerDataInPosition(registerIndex, newSampler, num);
		PropertyChanged(dp);
		WritePostscript();
	}

	private static void StashInPosition<T>(ref List<T?> list, int position, T value, int maxIndex, ref uint count) where T : struct
	{
		if (list == null)
		{
			list = new List<T?>(maxIndex);
		}
		if (list.Count <= position)
		{
			int num = position - list.Count + 1;
			for (int i = 0; i < num; i++)
			{
				list.Add(null);
			}
		}
		if (!list[position].HasValue)
		{
			count++;
		}
		list[position] = value;
	}

	private void StashSamplerDataInPosition(int position, SamplerData newSampler, int maxIndex)
	{
		if (_samplerData == null)
		{
			_samplerData = new List<SamplerData?>(maxIndex);
		}
		if (_samplerData.Count <= position)
		{
			int num = position - _samplerData.Count + 1;
			for (int i = 0; i < num; i++)
			{
				_samplerData.Add(null);
			}
		}
		if (!_samplerData[position].HasValue)
		{
			_samplerCount++;
		}
		if (base.Dispatcher != null)
		{
			SamplerData? samplerData = _samplerData[position];
			Brush resource = null;
			if (samplerData.HasValue)
			{
				resource = samplerData.Value._brush;
			}
			Brush brush = newSampler._brush;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = ((DUCE.IResource)this).GetChannelCount();
				for (int j = 0; j < channelCount; j++)
				{
					DUCE.Channel channel = ((DUCE.IResource)this).GetChannel(j);
					ReleaseResource(resource, channel);
					AddRefResource(brush, channel);
				}
			}
		}
		_samplerData[position] = newSampler;
	}

	private unsafe void ManualUpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (!skipOnChannelCheck && !_duceResource.IsOnChannel(channel))
		{
			return;
		}
		if (PixelShader == null)
		{
			throw new InvalidOperationException(SR.Effect_ShaderPixelShaderSet);
		}
		DUCE.MILCMD_SHADEREFFECT mILCMD_SHADEREFFECT = default(DUCE.MILCMD_SHADEREFFECT);
		mILCMD_SHADEREFFECT.Type = MILCMD.MilCmdShaderEffect;
		mILCMD_SHADEREFFECT.Handle = _duceResource.GetHandle(channel);
		mILCMD_SHADEREFFECT.TopPadding = _topPadding;
		mILCMD_SHADEREFFECT.BottomPadding = _bottomPadding;
		mILCMD_SHADEREFFECT.LeftPadding = _leftPadding;
		mILCMD_SHADEREFFECT.RightPadding = _rightPadding;
		mILCMD_SHADEREFFECT.DdxUvDdyUvRegisterIndex = DdxUvDdyUvRegisterIndex;
		mILCMD_SHADEREFFECT.hPixelShader = ((DUCE.IResource)PixelShader).GetHandle(channel);
		checked
		{
			mILCMD_SHADEREFFECT.ShaderConstantFloatRegistersSize = 2 * _floatCount;
			mILCMD_SHADEREFFECT.DependencyPropertyFloatValuesSize = 16 * _floatCount;
			mILCMD_SHADEREFFECT.ShaderConstantIntRegistersSize = 2 * _intCount;
			mILCMD_SHADEREFFECT.DependencyPropertyIntValuesSize = 16 * _intCount;
			mILCMD_SHADEREFFECT.ShaderConstantBoolRegistersSize = 2 * _boolCount;
			mILCMD_SHADEREFFECT.DependencyPropertyBoolValuesSize = 4 * _boolCount;
			mILCMD_SHADEREFFECT.ShaderSamplerRegistrationInfoSize = 8 * _samplerCount;
			mILCMD_SHADEREFFECT.DependencyPropertySamplerValuesSize = (uint)(sizeof(DUCE.ResourceHandle) * _samplerCount);
			channel.BeginCommand(unchecked((byte*)(&mILCMD_SHADEREFFECT)), sizeof(DUCE.MILCMD_SHADEREFFECT), (int)(mILCMD_SHADEREFFECT.ShaderConstantFloatRegistersSize + mILCMD_SHADEREFFECT.DependencyPropertyFloatValuesSize + mILCMD_SHADEREFFECT.ShaderConstantIntRegistersSize + mILCMD_SHADEREFFECT.DependencyPropertyIntValuesSize + mILCMD_SHADEREFFECT.ShaderConstantBoolRegistersSize + mILCMD_SHADEREFFECT.DependencyPropertyBoolValuesSize + mILCMD_SHADEREFFECT.ShaderSamplerRegistrationInfoSize + mILCMD_SHADEREFFECT.DependencyPropertySamplerValuesSize));
			AppendRegisters(channel, _floatRegisters);
			if (_floatRegisters != null)
			{
				for (int i = 0; i < _floatRegisters.Count; i++)
				{
					MilColorF? milColorF = _floatRegisters[i];
					if (milColorF.HasValue)
					{
						MilColorF value = milColorF.Value;
						channel.AppendCommandData(unchecked((byte*)(&value)), sizeof(MilColorF));
					}
				}
			}
			AppendRegisters(channel, _intRegisters);
			if (_intRegisters != null)
			{
				for (int j = 0; j < _intRegisters.Count; j++)
				{
					MilColorI? milColorI = _intRegisters[j];
					if (milColorI.HasValue)
					{
						MilColorI value2 = milColorI.Value;
						channel.AppendCommandData(unchecked((byte*)(&value2)), sizeof(MilColorI));
					}
				}
			}
			AppendRegisters(channel, _boolRegisters);
			if (_boolRegisters != null)
			{
				for (int k = 0; k < _boolRegisters.Count; k++)
				{
					bool? flag = _boolRegisters[k];
					if (flag.HasValue)
					{
						int num = (flag.Value ? 1 : 0);
						channel.AppendCommandData(unchecked((byte*)(&num)), 4);
					}
				}
			}
		}
		if (_samplerCount != 0)
		{
			int count = _samplerData.Count;
			for (int l = 0; l < count; l = checked(l + 1))
			{
				SamplerData? samplerData = _samplerData[l];
				if (samplerData.HasValue)
				{
					SamplerData value3 = samplerData.Value;
					channel.AppendCommandData((byte*)(&l), 4);
					int samplingMode = (int)value3._samplingMode;
					channel.AppendCommandData((byte*)(&samplingMode), 4);
				}
			}
		}
		if (_samplerCount != 0)
		{
			for (int m = 0; m < _samplerData.Count; m = checked(m + 1))
			{
				SamplerData? samplerData2 = _samplerData[m];
				if (samplerData2.HasValue)
				{
					SamplerData value4 = samplerData2.Value;
					DUCE.ResourceHandle resourceHandle = ((value4._brush != null) ? ((DUCE.IResource)value4._brush).GetHandle(channel) : DUCE.ResourceHandle.Null);
					channel.AppendCommandData((byte*)(&resourceHandle), sizeof(DUCE.ResourceHandle));
				}
			}
		}
		channel.EndCommand();
	}

	private unsafe void AppendRegisters<T>(DUCE.Channel channel, List<T?> list) where T : struct
	{
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].HasValue)
			{
				short num = (short)i;
				channel.AppendCommandData((byte*)(&num), 2);
			}
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_SHADEREFFECT))
		{
			if (_samplerCount != 0)
			{
				int count = _samplerData.Count;
				for (int i = 0; i < count; i++)
				{
					SamplerData? samplerData = _samplerData[i];
					if (samplerData.HasValue)
					{
						((DUCE.IResource)samplerData.Value._brush)?.AddRefOnChannel(channel);
					}
				}
			}
			((DUCE.IResource)PixelShader)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (!_duceResource.ReleaseOnChannel(channel))
		{
			return;
		}
		if (_samplerCount != 0)
		{
			int count = _samplerData.Count;
			for (int i = 0; i < count; i++)
			{
				SamplerData? samplerData = _samplerData[i];
				if (samplerData.HasValue)
				{
					((DUCE.IResource)samplerData.Value._brush)?.ReleaseOnChannel(channel);
				}
			}
		}
		((DUCE.IResource)PixelShader)?.ReleaseOnChannel(channel);
		ReleaseOnChannelAnimations(channel);
	}

	internal static Type DetermineShaderConstantType(Type type, PixelShader pixelShader)
	{
		Type result = null;
		if (type == typeof(double) || type == typeof(float) || type == typeof(Color) || type == typeof(Point) || type == typeof(Size) || type == typeof(Vector) || type == typeof(Point3D) || type == typeof(Vector3D) || type == typeof(Point4D))
		{
			result = typeof(float);
		}
		else if (pixelShader != null && pixelShader.ShaderMajorVersion >= 3)
		{
			if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(long) || type == typeof(ulong) || type == typeof(short) || type == typeof(ushort) || type == typeof(char))
			{
				result = typeof(int);
			}
			else if (type == typeof(bool))
			{
				result = typeof(bool);
			}
		}
		return result;
	}

	internal static void ConvertValueToMilColorF(object value, out MilColorF newVal)
	{
		Type type = value.GetType();
		if (type == typeof(double) || type == typeof(float))
		{
			float a = ((type == typeof(double)) ? ((float)(double)value) : ((float)value));
			newVal.r = (newVal.g = (newVal.b = (newVal.a = a)));
		}
		else if (type == typeof(Color))
		{
			Color color = (Color)value;
			newVal.r = (float)(int)color.R / 255f;
			newVal.g = (float)(int)color.G / 255f;
			newVal.b = (float)(int)color.B / 255f;
			newVal.a = (float)(int)color.A / 255f;
		}
		else if (type == typeof(Point))
		{
			Point point = (Point)value;
			newVal.r = (float)point.X;
			newVal.g = (float)point.Y;
			newVal.b = 1f;
			newVal.a = 1f;
		}
		else if (type == typeof(Size))
		{
			Size size = (Size)value;
			newVal.r = (float)size.Width;
			newVal.g = (float)size.Height;
			newVal.b = 1f;
			newVal.a = 1f;
		}
		else if (type == typeof(Vector))
		{
			Vector vector = (Vector)value;
			newVal.r = (float)vector.X;
			newVal.g = (float)vector.Y;
			newVal.b = 1f;
			newVal.a = 1f;
		}
		else if (type == typeof(Point3D))
		{
			Point3D point3D = (Point3D)value;
			newVal.r = (float)point3D.X;
			newVal.g = (float)point3D.Y;
			newVal.b = (float)point3D.Z;
			newVal.a = 1f;
		}
		else if (type == typeof(Vector3D))
		{
			Vector3D vector3D = (Vector3D)value;
			newVal.r = (float)vector3D.X;
			newVal.g = (float)vector3D.Y;
			newVal.b = (float)vector3D.Z;
			newVal.a = 1f;
		}
		else if (type == typeof(Point4D))
		{
			Point4D point4D = (Point4D)value;
			newVal.r = (float)point4D.X;
			newVal.g = (float)point4D.Y;
			newVal.b = (float)point4D.Z;
			newVal.a = (float)point4D.W;
		}
		else
		{
			newVal.r = (newVal.b = (newVal.g = (newVal.a = 1f)));
		}
	}

	internal static void ConvertValueToMilColorI(object value, out MilColorI newVal)
	{
		Type type = value.GetType();
		int num = 1;
		num = (int)((type == typeof(long)) ? ((long)value) : ((type == typeof(ulong)) ? ((int)(ulong)value) : ((type == typeof(uint)) ? ((int)(uint)value) : ((type == typeof(short)) ? ((short)value) : ((type == typeof(ushort)) ? ((ushort)value) : ((type == typeof(byte)) ? ((byte)value) : ((type == typeof(sbyte)) ? ((sbyte)value) : ((!(type == typeof(char))) ? ((int)value) : ((char)value)))))))));
		newVal.r = (newVal.g = (newVal.b = (newVal.a = num)));
	}

	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		ShaderEffect effect = (ShaderEffect)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(effect);
	}

	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to be cloned.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		ShaderEffect effect = (ShaderEffect)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(effect);
	}

	/// <param name="sourceFreezable">The instance to copy.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		ShaderEffect effect = (ShaderEffect)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(effect);
	}

	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		ShaderEffect effect = (ShaderEffect)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(effect);
	}

	private void CopyCommon(ShaderEffect effect)
	{
		_topPadding = effect._topPadding;
		_bottomPadding = effect._bottomPadding;
		_leftPadding = effect._leftPadding;
		_rightPadding = effect._rightPadding;
		if (_floatRegisters != null)
		{
			_floatRegisters = new List<MilColorF?>(effect._floatRegisters);
		}
		if (_samplerData != null)
		{
			_samplerData = new List<SamplerData?>(effect._samplerData);
		}
		_floatCount = effect._floatCount;
		_samplerCount = effect._samplerCount;
		_ddxUvDdyUvRegisterIndex = effect._ddxUvDdyUvRegisterIndex;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.ShaderEffect" /> object, making deep copies of this object's values. When copying this object's dependency properties, this method copies resource references and data bindings (which may no longer resolve), but not animations or their current values.  </summary>
	/// <returns>A modifiable clone of this instance. The returned clone is effectively a deep copy of the current object. The clone's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false.</returns>
	public new ShaderEffect Clone()
	{
		return (ShaderEffect)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.ShaderEffect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are copied. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ShaderEffect CloneCurrentValue()
	{
		return (ShaderEffect)base.CloneCurrentValue();
	}

	private static void PixelShaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ShaderEffect shaderEffect = (ShaderEffect)d;
		shaderEffect.PixelShaderPropertyChangedHook(e);
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		PixelShader resource = (PixelShader)e.OldValue;
		PixelShader resource2 = (PixelShader)e.NewValue;
		if (shaderEffect.Dispatcher != null)
		{
			DUCE.IResource resource3 = shaderEffect;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					shaderEffect.ReleaseResource(resource, channel);
					shaderEffect.AddRefResource(resource2, channel);
				}
			}
		}
		shaderEffect.PropertyChanged(PixelShaderProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return (Freezable)Activator.CreateInstance(GetType());
	}

	internal override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		ManualUpdateResource(channel, skipOnChannelCheck);
		base.UpdateResource(channel, skipOnChannelCheck);
	}

	private DUCE.ResourceHandle GeneratedAddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_SHADEREFFECT))
		{
			((DUCE.IResource)PixelShader)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	private void GeneratedReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)PixelShader)?.ReleaseOnChannel(channel);
			ReleaseOnChannelAnimations(channel);
		}
	}

	internal override DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource.GetHandle(channel);
	}

	internal override int GetChannelCountCore()
	{
		return _duceResource.GetChannelCount();
	}

	internal override DUCE.Channel GetChannelCore(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static ShaderEffect()
	{
		Type typeFromHandle = typeof(ShaderEffect);
		PixelShaderProperty = Animatable.RegisterProperty("PixelShader", typeof(PixelShader), typeFromHandle, null, PixelShaderPropertyChanged, null, isIndependentlyAnimated: false, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.ShaderEffect" /> class. </summary>
	protected ShaderEffect()
	{
	}
}
