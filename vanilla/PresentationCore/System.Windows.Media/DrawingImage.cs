using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary> An <see cref="T:System.Windows.Media.ImageSource" /> that uses a <see cref="T:System.Windows.Media.Drawing" /> for content. </summary>
public sealed class DrawingImage : ImageSource
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingImage.Drawing" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingImage.Drawing" /> dependency property.</returns>
	public static readonly DependencyProperty DrawingProperty;

	internal DUCE.MultiChannelResource _duceResource;

	/// <summary> Gets the width of the <see cref="T:System.Windows.Media.DrawingImage" />. </summary>
	/// <returns>The width of the <see cref="T:System.Windows.Media.DrawingImage" />.</returns>
	public override double Width
	{
		get
		{
			ReadPreamble();
			return Size.Width;
		}
	}

	/// <summary> Gets the height of the <see cref="T:System.Windows.Media.DrawingImage" />.</summary>
	/// <returns>The height of the <see cref="T:System.Windows.Media.DrawingImage" />.</returns>
	public override double Height
	{
		get
		{
			ReadPreamble();
			return Size.Height;
		}
	}

	/// <summary>Gets the metadata of the <see cref="T:System.Windows.Media.DrawingImage" />.</summary>
	/// <returns>The metadata of the <see cref="T:System.Windows.Media.DrawingImage" />.</returns>
	public override ImageMetadata Metadata
	{
		get
		{
			ReadPreamble();
			return null;
		}
	}

	internal override Size Size
	{
		get
		{
			Drawing drawing = Drawing;
			if (drawing != null)
			{
				Size size = drawing.GetBounds().Size;
				if (!size.IsEmpty)
				{
					return size;
				}
				return default(Size);
			}
			return default(Size);
		}
	}

	/// <summary> Gets or sets the drawing content for the <see cref="T:System.Windows.Media.DrawingImage" />.</summary>
	/// <returns>The drawing content for the <see cref="T:System.Windows.Media.DrawingImage" />. The default value is null.</returns>
	public Drawing Drawing
	{
		get
		{
			return (Drawing)GetValue(DrawingProperty);
		}
		set
		{
			SetValueInternal(DrawingProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DrawingImage" /> class. </summary>
	public DrawingImage()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DrawingImage" /> class that has the specified <see cref="P:System.Windows.Media.DrawingImage.Drawing" />.  </summary>
	/// <param name="drawing">The <see cref="P:System.Windows.Media.DrawingImage.Drawing" /> of the new <see cref="T:System.Windows.Media.DrawingImage" /> instance.</param>
	public DrawingImage(Drawing drawing)
	{
		Drawing = drawing;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DrawingImage" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DrawingImage Clone()
	{
		return (DrawingImage)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DrawingImage" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DrawingImage CloneCurrentValue()
	{
		return (DrawingImage)base.CloneCurrentValue();
	}

	private static void DrawingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		DrawingImage drawingImage = (DrawingImage)d;
		Drawing resource = (Drawing)e.OldValue;
		Drawing resource2 = (Drawing)e.NewValue;
		if (drawingImage.Dispatcher != null)
		{
			DUCE.IResource resource3 = drawingImage;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					drawingImage.ReleaseResource(resource, channel);
					drawingImage.AddRefResource(resource2, channel);
				}
			}
		}
		drawingImage.PropertyChanged(DrawingProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DrawingImage();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle hDrawing = ((DUCE.IResource)Drawing)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.MILCMD_DRAWINGIMAGE mILCMD_DRAWINGIMAGE = default(DUCE.MILCMD_DRAWINGIMAGE);
			mILCMD_DRAWINGIMAGE.Type = MILCMD.MilCmdDrawingImage;
			mILCMD_DRAWINGIMAGE.Handle = _duceResource.GetHandle(channel);
			mILCMD_DRAWINGIMAGE.hDrawing = hDrawing;
			channel.SendCommand((byte*)(&mILCMD_DRAWINGIMAGE), sizeof(DUCE.MILCMD_DRAWINGIMAGE));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_DRAWINGIMAGE))
		{
			((DUCE.IResource)Drawing)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)Drawing)?.ReleaseOnChannel(channel);
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

	static DrawingImage()
	{
		Type typeFromHandle = typeof(DrawingImage);
		DrawingProperty = Animatable.RegisterProperty("Drawing", typeof(Drawing), typeFromHandle, null, DrawingPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
