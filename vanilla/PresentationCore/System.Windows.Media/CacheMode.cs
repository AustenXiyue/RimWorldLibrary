using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Converters;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides a base implementation for caching a <see cref="T:System.Windows.UIElement" />.</summary>
[TypeConverter(typeof(CacheModeConverter))]
[ValueSerializer(typeof(CacheModeValueSerializer))]
public abstract class CacheMode : Animatable, DUCE.IResource
{
	internal CacheMode()
	{
	}

	internal static CacheMode Parse(string value)
	{
		CacheMode cacheMode = null;
		if (value == "BitmapCache")
		{
			return new BitmapCache();
		}
		throw new FormatException(SR.Parsers_IllegalToken);
	}

	internal virtual bool CanSerializeToString()
	{
		return false;
	}

	internal virtual string ConvertToString(string format, IFormatProvider provider)
	{
		return base.ToString();
	}

	/// <summary>Creates a modifiable clone of the <see cref="T:System.Windows.Media.CacheMode" />, making deep copies of the object's values. When copying the object's dependency properties, this method copies expressions (which might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new CacheMode Clone()
	{
		return (CacheMode)base.Clone();
	}

	/// <summary>Creates a modifiable clone (deep copy) of the <see cref="T:System.Windows.Media.CacheMode" /> using its current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new CacheMode CloneCurrentValue()
	{
		return (CacheMode)base.CloneCurrentValue();
	}

	internal abstract DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return AddRefOnChannelCore(channel);
		}
	}

	internal abstract void ReleaseOnChannelCore(DUCE.Channel channel);

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			ReleaseOnChannelCore(channel);
		}
	}

	internal abstract DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return GetHandleCore(channel);
		}
	}

	internal abstract int GetChannelCountCore();

	int DUCE.IResource.GetChannelCount()
	{
		return GetChannelCountCore();
	}

	internal abstract DUCE.Channel GetChannelCore(int index);

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return GetChannelCore(index);
	}
}
