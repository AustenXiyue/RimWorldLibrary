using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary> Paints an area with a solid color. </summary>
public sealed class SolidColorBrush : Brush
{
	internal enum SerializationBrushType : byte
	{
		Unknown,
		KnownSolidColor,
		OtherColor
	}

	internal class TwoWayDictionary<TKey, TValue>
	{
		private Dictionary<TKey, TValue> fwdDictionary;

		private Dictionary<TValue, List<TKey>> revDictionary;

		public TValue this[TKey key]
		{
			get
			{
				return fwdDictionary[key];
			}
			set
			{
				fwdDictionary[key] = value;
				if (!revDictionary.TryGetValue(value, out var value2))
				{
					value2 = new List<TKey>();
					revDictionary[value] = value2;
				}
				value2.Add(key);
			}
		}

		public TwoWayDictionary()
			: this((IEqualityComparer<TKey>)null, (IEqualityComparer<TValue>)null)
		{
		}

		public TwoWayDictionary(IEqualityComparer<TKey> keyComparer)
			: this(keyComparer, (IEqualityComparer<TValue>)null)
		{
		}

		public TwoWayDictionary(IEqualityComparer<TValue> valueComparer)
			: this((IEqualityComparer<TKey>)null, valueComparer)
		{
		}

		public TwoWayDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			fwdDictionary = new Dictionary<TKey, TValue>(keyComparer);
			revDictionary = new Dictionary<TValue, List<TKey>>(valueComparer);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return fwdDictionary.TryGetValue(key, out value);
		}

		public bool TryGetKeys(TValue value, out List<TKey> keys)
		{
			return revDictionary.TryGetValue(value, out keys);
		}

		public bool ContainsValue(TValue value)
		{
			return revDictionary.ContainsKey(value);
		}

		public bool ContainsKey(TKey key)
		{
			return fwdDictionary.ContainsKey(key);
		}

		public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
		{
			return fwdDictionary.GetEnumerator();
		}
	}

	private static TwoWayDictionary<SolidColorBrush, string> s_knownSolidColorBrushStringCache;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.SolidColorBrush.Color" /> dependency property.</returns>
	public static readonly DependencyProperty ColorProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Color s_Color;

	/// <summary>Gets or sets the color of this <see cref="T:System.Windows.Media.SolidColorBrush" />.  </summary>
	/// <returns>The brush's color. The default value is <see cref="P:System.Windows.Media.Colors.Transparent" />.</returns>
	public Color Color
	{
		get
		{
			return (Color)GetValue(ColorProperty);
		}
		set
		{
			SetValueInternal(ColorProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.SolidColorBrush" /> class with no color or animations. </summary>
	public SolidColorBrush()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.SolidColorBrush" /> class with the specified <see cref="T:System.Windows.Media.Color" />. </summary>
	/// <param name="color">The color to apply to the fill.</param>
	public SolidColorBrush(Color color)
	{
		Color = color;
	}

	[FriendAccessAllowed]
	internal static bool SerializeOn(BinaryWriter writer, string stringValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		KnownColor knownColor = KnownColors.ColorStringToKnownColor(stringValue);
		lock (s_knownSolidColorBrushStringCache)
		{
			if (s_knownSolidColorBrushStringCache.ContainsValue(stringValue))
			{
				knownColor = KnownColors.ArgbStringToKnownColor(stringValue);
			}
		}
		if (knownColor != KnownColor.UnknownColor)
		{
			writer.Write((byte)1);
			writer.Write((uint)knownColor);
			return true;
		}
		stringValue = stringValue.Trim();
		if (stringValue.Length > 3)
		{
			writer.Write((byte)2);
			writer.Write(stringValue);
			return true;
		}
		return false;
	}

	/// <summary>This member supports the WPF infrastructure and is not intended to be used directly from your code. </summary>
	/// <returns>The deserialized <see cref="T:System.Windows.Media.SolidColorBrush" />.</returns>
	/// <param name="reader">The binary reader to read the <see cref="T:System.Windows.Media.SolidColorBrush" /> from.</param>
	public static object DeserializeFrom(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return DeserializeFrom(reader, null);
	}

	internal static object DeserializeFrom(BinaryReader reader, ITypeDescriptorContext context)
	{
		switch ((SerializationBrushType)reader.ReadByte())
		{
		case SerializationBrushType.KnownSolidColor:
		{
			SolidColorBrush solidColorBrush = KnownColors.SolidColorBrushFromUint(reader.ReadUInt32());
			lock (s_knownSolidColorBrushStringCache)
			{
				if (!s_knownSolidColorBrushStringCache.ContainsKey(solidColorBrush))
				{
					string value = solidColorBrush.Color.ConvertToString(null, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS);
					s_knownSolidColorBrushStringCache[solidColorBrush] = value;
				}
			}
			return solidColorBrush;
		}
		case SerializationBrushType.OtherColor:
		{
			string text = reader.ReadString();
			return new BrushConverter().ConvertFromInvariantString(context, text);
		}
		default:
			throw new Exception(SR.BrushUnknownBamlType);
		}
	}

	internal override bool CanSerializeToString()
	{
		if (base.HasAnimatedProperties || HasAnyExpression() || !base.Transform.IsIdentity || !DoubleUtil.AreClose(base.Opacity, 1.0))
		{
			return false;
		}
		return true;
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		string text = Color.ConvertToString(format, provider);
		if (format == null && provider == System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS && KnownColors.IsKnownSolidColorBrush(this))
		{
			lock (s_knownSolidColorBrushStringCache)
			{
				string value = null;
				if (s_knownSolidColorBrushStringCache.TryGetValue(this, out value))
				{
					text = value;
				}
				else
				{
					s_knownSolidColorBrushStringCache[this] = text;
				}
			}
		}
		return text;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.SolidColorBrush" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new SolidColorBrush Clone()
	{
		return (SolidColorBrush)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.SolidColorBrush" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new SolidColorBrush CloneCurrentValue()
	{
		return (SolidColorBrush)base.CloneCurrentValue();
	}

	private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SolidColorBrush)d).PropertyChanged(ColorProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new SolidColorBrush();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			Transform relativeTransform = base.RelativeTransform;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hRelativeTransform = ((relativeTransform != null && relativeTransform != Transform.Identity) ? ((DUCE.IResource)relativeTransform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Brush.OpacityProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(ColorProperty, channel);
			DUCE.MILCMD_SOLIDCOLORBRUSH mILCMD_SOLIDCOLORBRUSH = default(DUCE.MILCMD_SOLIDCOLORBRUSH);
			mILCMD_SOLIDCOLORBRUSH.Type = MILCMD.MilCmdSolidColorBrush;
			mILCMD_SOLIDCOLORBRUSH.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_SOLIDCOLORBRUSH.Opacity = base.Opacity;
			}
			mILCMD_SOLIDCOLORBRUSH.hOpacityAnimations = animationResourceHandle;
			mILCMD_SOLIDCOLORBRUSH.hTransform = hTransform;
			mILCMD_SOLIDCOLORBRUSH.hRelativeTransform = hRelativeTransform;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_SOLIDCOLORBRUSH.Color = CompositionResourceManager.ColorToMilColorF(Color);
			}
			mILCMD_SOLIDCOLORBRUSH.hColorAnimations = animationResourceHandle2;
			channel.SendCommand((byte*)(&mILCMD_SOLIDCOLORBRUSH), sizeof(DUCE.MILCMD_SOLIDCOLORBRUSH));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_SOLIDCOLORBRUSH))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)base.Transform)?.ReleaseOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.ReleaseOnChannel(channel);
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

	static SolidColorBrush()
	{
		s_knownSolidColorBrushStringCache = new TwoWayDictionary<SolidColorBrush, string>((IEqualityComparer<SolidColorBrush>)ReferenceEqualityComparer.Instance);
		s_Color = Colors.Transparent;
		Type typeFromHandle = typeof(SolidColorBrush);
		ColorProperty = Animatable.RegisterProperty("Color", typeof(Color), typeFromHandle, Colors.Transparent, ColorPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
