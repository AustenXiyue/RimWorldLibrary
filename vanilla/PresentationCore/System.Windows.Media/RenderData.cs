using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Effects;
using MS.Internal;
using MS.Utility;

namespace System.Windows.Media;

internal class RenderData : Freezable, DUCE.IResource, IDrawingContent
{
	internal struct RecordHeader
	{
		public int Size;

		public MILCMD Id;
	}

	private enum PushType
	{
		BitmapEffect,
		Other
	}

	private byte[] _buffer;

	private int _curOffset;

	private int _bitmapEffectStackDepth;

	private FrugalStructList<object> _dependentResources;

	private DUCE.MultiChannelResource _duceResource;

	internal int BitmapEffectStackDepth
	{
		get
		{
			return _bitmapEffectStackDepth;
		}
		set
		{
			_bitmapEffectStackDepth = value;
		}
	}

	public int DataSize => _curOffset;

	internal RenderData()
	{
		base.CanBeInheritanceContext = false;
	}

	public unsafe void WriteDataRecord(MILCMD id, byte* pbRecord, int cbRecordSize)
	{
		int num;
		RecordHeader recordHeader = default(RecordHeader);
		checked
		{
			num = cbRecordSize + sizeof(RecordHeader);
			int num2 = _curOffset + num;
			if (_buffer == null || num2 > _buffer.Length)
			{
				EnsureBuffer(num2);
			}
			recordHeader.Size = num;
			recordHeader.Id = id;
		}
		Marshal.Copy((nint)(&recordHeader), _buffer, _curOffset, sizeof(RecordHeader));
		Marshal.Copy((nint)pbRecord, _buffer, _curOffset + sizeof(RecordHeader), cbRecordSize);
		_curOffset += num;
	}

	public Rect GetContentBounds(BoundsDrawingContextWalker ctx)
	{
		DrawingContextWalk(ctx);
		return ctx.Bounds;
	}

	public void WalkContent(DrawingContextWalker walker)
	{
		DrawingContextWalk(walker);
	}

	public bool HitTestPoint(Point point)
	{
		HitTestDrawingContextWalker hitTestDrawingContextWalker = new HitTestWithPointDrawingContextWalker(point);
		DrawingContextWalk(hitTestDrawingContextWalker);
		return hitTestDrawingContextWalker.IsHit;
	}

	public IntersectionDetail HitTestGeometry(PathGeometry geometry)
	{
		HitTestDrawingContextWalker hitTestDrawingContextWalker = new HitTestWithGeometryDrawingContextWalker(geometry);
		DrawingContextWalk(hitTestDrawingContextWalker);
		return hitTestDrawingContextWalker.IntersectionDetail;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new RenderData();
	}

	protected override void CloneCore(Freezable source)
	{
		Invariant.Assert(condition: false);
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		Invariant.Assert(condition: false);
	}

	protected override bool FreezeCore(bool isChecking)
	{
		return false;
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		Invariant.Assert(condition: false);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		Invariant.Assert(condition: false);
	}

	public void PropagateChangedHandler(EventHandler handler, bool adding)
	{
		if (adding)
		{
			base.Changed += handler;
		}
		else
		{
			base.Changed -= handler;
		}
		int i = 0;
		for (int count = _dependentResources.Count; i < count; i++)
		{
			if (_dependentResources[i] is Freezable freezable)
			{
				if (adding)
				{
					OnFreezablePropertyChanged(null, freezable);
				}
				else
				{
					OnFreezablePropertyChanged(freezable, null);
				}
			}
			else if (_dependentResources[i] is AnimationClockResource animationClockResource)
			{
				animationClockResource.PropagateChangedHandlersCore(handler, adding);
			}
		}
	}

	internal void BeginTopLevelBitmapEffect(int stackDepth)
	{
		BitmapEffectStackDepth = stackDepth;
	}

	internal void EndTopLevelBitmapEffect()
	{
		BitmapEffectStackDepth = 0;
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_RENDERDATA))
			{
				for (int i = 0; i < _dependentResources.Count; i++)
				{
					if (_dependentResources[i] is DUCE.IResource resource)
					{
						resource.AddRefOnChannel(channel);
					}
				}
				UpdateResource(channel);
			}
			return _duceResource.GetHandle(channel);
		}
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (!_duceResource.ReleaseOnChannel(channel))
			{
				return;
			}
			for (int i = 0; i < _dependentResources.Count; i++)
			{
				if (_dependentResources[i] is DUCE.IResource resource)
				{
					resource.ReleaseOnChannel(channel);
				}
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

	void DUCE.IResource.RemoveChildFromParent(DUCE.IResource parent, DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}

	DUCE.ResourceHandle DUCE.IResource.Get3DHandle(DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}

	public uint AddDependentResource(object o)
	{
		if (o == null)
		{
			return 0u;
		}
		return (uint)(_dependentResources.Add(o) + 1);
	}

	private void UpdateResource(DUCE.Channel channel)
	{
		MarshalToDUCE(channel);
	}

	private unsafe void EnsureBuffer(int cbRequiredSize)
	{
		if (_buffer == null)
		{
			_buffer = ArrayPool<byte>.Shared.Rent(cbRequiredSize);
			return;
		}
		int minimumLength = Math.Max((_buffer.Length << 1) - (_buffer.Length >> 1), cbRequiredSize);
		byte[] array = ArrayPool<byte>.Shared.Rent(minimumLength);
		fixed (byte* buffer = _buffer)
		{
			fixed (byte* destination = array)
			{
				Buffer.MemoryCopy(buffer, destination, _buffer.Length, _buffer.Length);
			}
		}
		byte[] buffer2 = _buffer;
		_buffer = array;
		ArrayPool<byte>.Shared.Return(buffer2);
	}

	private object DependentLookup(uint index)
	{
		if (index == 0)
		{
			return null;
		}
		return _dependentResources[(int)(index - 1)];
	}

	private unsafe void MarshalToDUCE(DUCE.Channel channel)
	{
		DUCE.MILCMD_RENDERDATA mILCMD_RENDERDATA = default(DUCE.MILCMD_RENDERDATA);
		mILCMD_RENDERDATA.Type = MILCMD.MilCmdRenderData;
		mILCMD_RENDERDATA.Handle = _duceResource.GetHandle(channel);
		mILCMD_RENDERDATA.cbData = (uint)DataSize;
		uint cbData = mILCMD_RENDERDATA.cbData;
		channel.BeginCommand((byte*)(&mILCMD_RENDERDATA), sizeof(DUCE.MILCMD_RENDERDATA), (int)cbData);
		Stack<PushType> stack = new Stack<PushType>();
		int num = 0;
		if (_curOffset > 0)
		{
			fixed (byte* buffer = _buffer)
			{
				byte* ptr = buffer;
				RecordHeader* ptr3;
				for (byte* ptr2 = buffer + _curOffset; ptr < ptr2; ptr += ptr3->Size)
				{
					ptr3 = (RecordHeader*)ptr;
					channel.AppendCommandData((byte*)ptr3, sizeof(RecordHeader));
					switch (ptr3->Id)
					{
					case MILCMD.MilDrawLine:
					{
						MILCMD_DRAW_LINE mILCMD_DRAW_LINE = *(MILCMD_DRAW_LINE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_LINE.hPen != 0)
						{
							mILCMD_DRAW_LINE.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_LINE.hPen - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_LINE), 40);
						break;
					}
					case MILCMD.MilDrawLineAnimate:
					{
						MILCMD_DRAW_LINE_ANIMATE mILCMD_DRAW_LINE_ANIMATE = *(MILCMD_DRAW_LINE_ANIMATE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_LINE_ANIMATE.hPen != 0)
						{
							mILCMD_DRAW_LINE_ANIMATE.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_LINE_ANIMATE.hPen - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_LINE_ANIMATE.hPoint0Animations != 0)
						{
							mILCMD_DRAW_LINE_ANIMATE.hPoint0Animations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_LINE_ANIMATE.hPoint0Animations - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_LINE_ANIMATE.hPoint1Animations != 0)
						{
							mILCMD_DRAW_LINE_ANIMATE.hPoint1Animations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_LINE_ANIMATE.hPoint1Animations - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_LINE_ANIMATE), 48);
						break;
					}
					case MILCMD.MilDrawRectangle:
					{
						MILCMD_DRAW_RECTANGLE mILCMD_DRAW_RECTANGLE = *(MILCMD_DRAW_RECTANGLE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_RECTANGLE.hBrush != 0)
						{
							mILCMD_DRAW_RECTANGLE.hBrush = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_RECTANGLE.hBrush - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_RECTANGLE.hPen != 0)
						{
							mILCMD_DRAW_RECTANGLE.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_RECTANGLE.hPen - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_RECTANGLE), 40);
						break;
					}
					case MILCMD.MilDrawRectangleAnimate:
					{
						MILCMD_DRAW_RECTANGLE_ANIMATE mILCMD_DRAW_RECTANGLE_ANIMATE = *(MILCMD_DRAW_RECTANGLE_ANIMATE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_RECTANGLE_ANIMATE.hBrush != 0)
						{
							mILCMD_DRAW_RECTANGLE_ANIMATE.hBrush = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_RECTANGLE_ANIMATE.hBrush - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_RECTANGLE_ANIMATE.hPen != 0)
						{
							mILCMD_DRAW_RECTANGLE_ANIMATE.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_RECTANGLE_ANIMATE.hPen - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_RECTANGLE_ANIMATE.hRectangleAnimations != 0)
						{
							mILCMD_DRAW_RECTANGLE_ANIMATE.hRectangleAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_RECTANGLE_ANIMATE.hRectangleAnimations - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_RECTANGLE_ANIMATE), 48);
						break;
					}
					case MILCMD.MilDrawRoundedRectangle:
					{
						MILCMD_DRAW_ROUNDED_RECTANGLE mILCMD_DRAW_ROUNDED_RECTANGLE = *(MILCMD_DRAW_ROUNDED_RECTANGLE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_ROUNDED_RECTANGLE.hBrush != 0)
						{
							mILCMD_DRAW_ROUNDED_RECTANGLE.hBrush = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ROUNDED_RECTANGLE.hBrush - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ROUNDED_RECTANGLE.hPen != 0)
						{
							mILCMD_DRAW_ROUNDED_RECTANGLE.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ROUNDED_RECTANGLE.hPen - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_ROUNDED_RECTANGLE), 56);
						break;
					}
					case MILCMD.MilDrawRoundedRectangleAnimate:
					{
						MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE = *(MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hBrush != 0)
						{
							mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hBrush = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hBrush - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hPen != 0)
						{
							mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hPen - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRectangleAnimations != 0)
						{
							mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRectangleAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRectangleAnimations - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRadiusXAnimations != 0)
						{
							mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRadiusXAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRadiusXAnimations - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRadiusYAnimations != 0)
						{
							mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRadiusYAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE.hRadiusYAnimations - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE), 72);
						break;
					}
					case MILCMD.MilDrawEllipse:
					{
						MILCMD_DRAW_ELLIPSE mILCMD_DRAW_ELLIPSE = *(MILCMD_DRAW_ELLIPSE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_ELLIPSE.hBrush != 0)
						{
							mILCMD_DRAW_ELLIPSE.hBrush = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ELLIPSE.hBrush - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ELLIPSE.hPen != 0)
						{
							mILCMD_DRAW_ELLIPSE.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ELLIPSE.hPen - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_ELLIPSE), 40);
						break;
					}
					case MILCMD.MilDrawEllipseAnimate:
					{
						MILCMD_DRAW_ELLIPSE_ANIMATE mILCMD_DRAW_ELLIPSE_ANIMATE = *(MILCMD_DRAW_ELLIPSE_ANIMATE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_ELLIPSE_ANIMATE.hBrush != 0)
						{
							mILCMD_DRAW_ELLIPSE_ANIMATE.hBrush = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ELLIPSE_ANIMATE.hBrush - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ELLIPSE_ANIMATE.hPen != 0)
						{
							mILCMD_DRAW_ELLIPSE_ANIMATE.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ELLIPSE_ANIMATE.hPen - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ELLIPSE_ANIMATE.hCenterAnimations != 0)
						{
							mILCMD_DRAW_ELLIPSE_ANIMATE.hCenterAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ELLIPSE_ANIMATE.hCenterAnimations - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ELLIPSE_ANIMATE.hRadiusXAnimations != 0)
						{
							mILCMD_DRAW_ELLIPSE_ANIMATE.hRadiusXAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ELLIPSE_ANIMATE.hRadiusXAnimations - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_ELLIPSE_ANIMATE.hRadiusYAnimations != 0)
						{
							mILCMD_DRAW_ELLIPSE_ANIMATE.hRadiusYAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_ELLIPSE_ANIMATE.hRadiusYAnimations - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_ELLIPSE_ANIMATE), 56);
						break;
					}
					case MILCMD.MilDrawGeometry:
					{
						MILCMD_DRAW_GEOMETRY mILCMD_DRAW_GEOMETRY = *(MILCMD_DRAW_GEOMETRY*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_GEOMETRY.hBrush != 0)
						{
							mILCMD_DRAW_GEOMETRY.hBrush = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_GEOMETRY.hBrush - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_GEOMETRY.hPen != 0)
						{
							mILCMD_DRAW_GEOMETRY.hPen = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_GEOMETRY.hPen - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_GEOMETRY.hGeometry != 0)
						{
							mILCMD_DRAW_GEOMETRY.hGeometry = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_GEOMETRY.hGeometry - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_GEOMETRY), 16);
						break;
					}
					case MILCMD.MilDrawImage:
					{
						MILCMD_DRAW_IMAGE mILCMD_DRAW_IMAGE = *(MILCMD_DRAW_IMAGE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_IMAGE.hImageSource != 0)
						{
							mILCMD_DRAW_IMAGE.hImageSource = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_IMAGE.hImageSource - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_IMAGE), 40);
						break;
					}
					case MILCMD.MilDrawImageAnimate:
					{
						MILCMD_DRAW_IMAGE_ANIMATE mILCMD_DRAW_IMAGE_ANIMATE = *(MILCMD_DRAW_IMAGE_ANIMATE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_IMAGE_ANIMATE.hImageSource != 0)
						{
							mILCMD_DRAW_IMAGE_ANIMATE.hImageSource = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_IMAGE_ANIMATE.hImageSource - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_IMAGE_ANIMATE.hRectangleAnimations != 0)
						{
							mILCMD_DRAW_IMAGE_ANIMATE.hRectangleAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_IMAGE_ANIMATE.hRectangleAnimations - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_IMAGE_ANIMATE), 40);
						break;
					}
					case MILCMD.MilDrawGlyphRun:
					{
						MILCMD_DRAW_GLYPH_RUN mILCMD_DRAW_GLYPH_RUN = *(MILCMD_DRAW_GLYPH_RUN*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_GLYPH_RUN.hForegroundBrush != 0)
						{
							mILCMD_DRAW_GLYPH_RUN.hForegroundBrush = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_GLYPH_RUN.hForegroundBrush - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_GLYPH_RUN.hGlyphRun != 0)
						{
							mILCMD_DRAW_GLYPH_RUN.hGlyphRun = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_GLYPH_RUN.hGlyphRun - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_GLYPH_RUN), 8);
						break;
					}
					case MILCMD.MilDrawDrawing:
					{
						MILCMD_DRAW_DRAWING mILCMD_DRAW_DRAWING = *(MILCMD_DRAW_DRAWING*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_DRAWING.hDrawing != 0)
						{
							mILCMD_DRAW_DRAWING.hDrawing = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_DRAWING.hDrawing - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_DRAWING), 8);
						break;
					}
					case MILCMD.MilDrawVideo:
					{
						MILCMD_DRAW_VIDEO mILCMD_DRAW_VIDEO = *(MILCMD_DRAW_VIDEO*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_VIDEO.hPlayer != 0)
						{
							mILCMD_DRAW_VIDEO.hPlayer = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_VIDEO.hPlayer - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_VIDEO), 40);
						break;
					}
					case MILCMD.MilDrawVideoAnimate:
					{
						MILCMD_DRAW_VIDEO_ANIMATE mILCMD_DRAW_VIDEO_ANIMATE = *(MILCMD_DRAW_VIDEO_ANIMATE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_DRAW_VIDEO_ANIMATE.hPlayer != 0)
						{
							mILCMD_DRAW_VIDEO_ANIMATE.hPlayer = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_VIDEO_ANIMATE.hPlayer - 1)]).GetHandle(channel);
						}
						if (mILCMD_DRAW_VIDEO_ANIMATE.hRectangleAnimations != 0)
						{
							mILCMD_DRAW_VIDEO_ANIMATE.hRectangleAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_DRAW_VIDEO_ANIMATE.hRectangleAnimations - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_DRAW_VIDEO_ANIMATE), 40);
						break;
					}
					case MILCMD.MilPushClip:
					{
						stack.Push(PushType.Other);
						MILCMD_PUSH_CLIP mILCMD_PUSH_CLIP = *(MILCMD_PUSH_CLIP*)(ptr + sizeof(RecordHeader));
						if (mILCMD_PUSH_CLIP.hClipGeometry != 0)
						{
							mILCMD_PUSH_CLIP.hClipGeometry = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_PUSH_CLIP.hClipGeometry - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_CLIP), 8);
						break;
					}
					case MILCMD.MilPushOpacityMask:
					{
						stack.Push(PushType.Other);
						MILCMD_PUSH_OPACITY_MASK mILCMD_PUSH_OPACITY_MASK = *(MILCMD_PUSH_OPACITY_MASK*)(ptr + sizeof(RecordHeader));
						if (mILCMD_PUSH_OPACITY_MASK.hOpacityMask != 0)
						{
							mILCMD_PUSH_OPACITY_MASK.hOpacityMask = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_PUSH_OPACITY_MASK.hOpacityMask - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_OPACITY_MASK), 24);
						break;
					}
					case MILCMD.MilPushOpacity:
					{
						stack.Push(PushType.Other);
						MILCMD_PUSH_OPACITY mILCMD_PUSH_OPACITY = *(MILCMD_PUSH_OPACITY*)(ptr + sizeof(RecordHeader));
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_OPACITY), 8);
						break;
					}
					case MILCMD.MilPushOpacityAnimate:
					{
						stack.Push(PushType.Other);
						MILCMD_PUSH_OPACITY_ANIMATE mILCMD_PUSH_OPACITY_ANIMATE = *(MILCMD_PUSH_OPACITY_ANIMATE*)(ptr + sizeof(RecordHeader));
						if (mILCMD_PUSH_OPACITY_ANIMATE.hOpacityAnimations != 0)
						{
							mILCMD_PUSH_OPACITY_ANIMATE.hOpacityAnimations = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_PUSH_OPACITY_ANIMATE.hOpacityAnimations - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_OPACITY_ANIMATE), 16);
						break;
					}
					case MILCMD.MilPushTransform:
					{
						stack.Push(PushType.Other);
						MILCMD_PUSH_TRANSFORM mILCMD_PUSH_TRANSFORM = *(MILCMD_PUSH_TRANSFORM*)(ptr + sizeof(RecordHeader));
						if (mILCMD_PUSH_TRANSFORM.hTransform != 0)
						{
							mILCMD_PUSH_TRANSFORM.hTransform = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_PUSH_TRANSFORM.hTransform - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_TRANSFORM), 8);
						break;
					}
					case MILCMD.MilPushGuidelineSet:
					{
						stack.Push(PushType.Other);
						MILCMD_PUSH_GUIDELINE_SET mILCMD_PUSH_GUIDELINE_SET = *(MILCMD_PUSH_GUIDELINE_SET*)(ptr + sizeof(RecordHeader));
						if (mILCMD_PUSH_GUIDELINE_SET.hGuidelines != 0)
						{
							mILCMD_PUSH_GUIDELINE_SET.hGuidelines = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_PUSH_GUIDELINE_SET.hGuidelines - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_GUIDELINE_SET), 8);
						break;
					}
					case MILCMD.MilPushGuidelineY1:
					{
						stack.Push(PushType.Other);
						MILCMD_PUSH_GUIDELINE_Y1 mILCMD_PUSH_GUIDELINE_Y2 = *(MILCMD_PUSH_GUIDELINE_Y1*)(ptr + sizeof(RecordHeader));
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_GUIDELINE_Y2), 8);
						break;
					}
					case MILCMD.MilPushGuidelineY2:
					{
						stack.Push(PushType.Other);
						MILCMD_PUSH_GUIDELINE_Y2 mILCMD_PUSH_GUIDELINE_Y = *(MILCMD_PUSH_GUIDELINE_Y2*)(ptr + sizeof(RecordHeader));
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_GUIDELINE_Y), 16);
						break;
					}
					case MILCMD.MilPushEffect:
					{
						stack.Push(PushType.BitmapEffect);
						num++;
						MILCMD_PUSH_EFFECT mILCMD_PUSH_EFFECT = *(MILCMD_PUSH_EFFECT*)(ptr + sizeof(RecordHeader));
						if (mILCMD_PUSH_EFFECT.hEffect != 0)
						{
							mILCMD_PUSH_EFFECT.hEffect = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_PUSH_EFFECT.hEffect - 1)]).GetHandle(channel);
						}
						if (mILCMD_PUSH_EFFECT.hEffectInput != 0)
						{
							mILCMD_PUSH_EFFECT.hEffectInput = (uint)((DUCE.IResource)_dependentResources[(int)(mILCMD_PUSH_EFFECT.hEffectInput - 1)]).GetHandle(channel);
						}
						channel.AppendCommandData((byte*)(&mILCMD_PUSH_EFFECT), 8);
						break;
					}
					case MILCMD.MilPop:
						if (stack.Pop() == PushType.BitmapEffect)
						{
							num--;
						}
						break;
					}
				}
			}
		}
		channel.EndCommand();
	}

	public unsafe void DrawingContextWalk(DrawingContextWalker ctx)
	{
		if (_curOffset <= 0)
		{
			return;
		}
		fixed (byte* buffer = _buffer)
		{
			byte* ptr = buffer;
			RecordHeader* ptr3;
			for (byte* ptr2 = buffer + _curOffset; ptr < ptr2; ptr += ptr3->Size)
			{
				if (ctx.ShouldStopWalking)
				{
					break;
				}
				ptr3 = (RecordHeader*)ptr;
				switch (ptr3->Id)
				{
				case MILCMD.MilDrawLine:
				{
					MILCMD_DRAW_LINE* ptr28 = (MILCMD_DRAW_LINE*)(ptr + sizeof(RecordHeader));
					ctx.DrawLine((Pen)DependentLookup(ptr28->hPen), ptr28->point0, ptr28->point1);
					break;
				}
				case MILCMD.MilDrawLineAnimate:
				{
					MILCMD_DRAW_LINE_ANIMATE* ptr27 = (MILCMD_DRAW_LINE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawLine((ptr27->hPen == 0) ? null : ((Pen)DependentLookup(ptr27->hPen)), (ptr27->hPoint0Animations == 0) ? ptr27->point0 : ((PointAnimationClockResource)DependentLookup(ptr27->hPoint0Animations)).CurrentValue, (ptr27->hPoint1Animations == 0) ? ptr27->point1 : ((PointAnimationClockResource)DependentLookup(ptr27->hPoint1Animations)).CurrentValue);
					break;
				}
				case MILCMD.MilDrawRectangle:
				{
					MILCMD_DRAW_RECTANGLE* ptr13 = (MILCMD_DRAW_RECTANGLE*)(ptr + sizeof(RecordHeader));
					ctx.DrawRectangle((Brush)DependentLookup(ptr13->hBrush), (Pen)DependentLookup(ptr13->hPen), ptr13->rectangle);
					break;
				}
				case MILCMD.MilDrawRectangleAnimate:
				{
					MILCMD_DRAW_RECTANGLE_ANIMATE* ptr19 = (MILCMD_DRAW_RECTANGLE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawRectangle((ptr19->hBrush == 0) ? null : ((Brush)DependentLookup(ptr19->hBrush)), (ptr19->hPen == 0) ? null : ((Pen)DependentLookup(ptr19->hPen)), (ptr19->hRectangleAnimations == 0) ? ptr19->rectangle : ((RectAnimationClockResource)DependentLookup(ptr19->hRectangleAnimations)).CurrentValue);
					break;
				}
				case MILCMD.MilDrawRoundedRectangle:
				{
					MILCMD_DRAW_ROUNDED_RECTANGLE* ptr11 = (MILCMD_DRAW_ROUNDED_RECTANGLE*)(ptr + sizeof(RecordHeader));
					ctx.DrawRoundedRectangle((Brush)DependentLookup(ptr11->hBrush), (Pen)DependentLookup(ptr11->hPen), ptr11->rectangle, ptr11->radiusX, ptr11->radiusY);
					break;
				}
				case MILCMD.MilDrawRoundedRectangleAnimate:
				{
					MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE* ptr26 = (MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawRoundedRectangle((ptr26->hBrush == 0) ? null : ((Brush)DependentLookup(ptr26->hBrush)), (ptr26->hPen == 0) ? null : ((Pen)DependentLookup(ptr26->hPen)), (ptr26->hRectangleAnimations == 0) ? ptr26->rectangle : ((RectAnimationClockResource)DependentLookup(ptr26->hRectangleAnimations)).CurrentValue, (ptr26->hRadiusXAnimations == 0) ? ptr26->radiusX : ((DoubleAnimationClockResource)DependentLookup(ptr26->hRadiusXAnimations)).CurrentValue, (ptr26->hRadiusYAnimations == 0) ? ptr26->radiusY : ((DoubleAnimationClockResource)DependentLookup(ptr26->hRadiusYAnimations)).CurrentValue);
					break;
				}
				case MILCMD.MilDrawEllipse:
				{
					MILCMD_DRAW_ELLIPSE* ptr25 = (MILCMD_DRAW_ELLIPSE*)(ptr + sizeof(RecordHeader));
					ctx.DrawEllipse((Brush)DependentLookup(ptr25->hBrush), (Pen)DependentLookup(ptr25->hPen), ptr25->center, ptr25->radiusX, ptr25->radiusY);
					break;
				}
				case MILCMD.MilDrawEllipseAnimate:
				{
					MILCMD_DRAW_ELLIPSE_ANIMATE* ptr18 = (MILCMD_DRAW_ELLIPSE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawEllipse((ptr18->hBrush == 0) ? null : ((Brush)DependentLookup(ptr18->hBrush)), (ptr18->hPen == 0) ? null : ((Pen)DependentLookup(ptr18->hPen)), (ptr18->hCenterAnimations == 0) ? ptr18->center : ((PointAnimationClockResource)DependentLookup(ptr18->hCenterAnimations)).CurrentValue, (ptr18->hRadiusXAnimations == 0) ? ptr18->radiusX : ((DoubleAnimationClockResource)DependentLookup(ptr18->hRadiusXAnimations)).CurrentValue, (ptr18->hRadiusYAnimations == 0) ? ptr18->radiusY : ((DoubleAnimationClockResource)DependentLookup(ptr18->hRadiusYAnimations)).CurrentValue);
					break;
				}
				case MILCMD.MilDrawGeometry:
				{
					MILCMD_DRAW_GEOMETRY* ptr20 = (MILCMD_DRAW_GEOMETRY*)(ptr + sizeof(RecordHeader));
					ctx.DrawGeometry((Brush)DependentLookup(ptr20->hBrush), (Pen)DependentLookup(ptr20->hPen), (Geometry)DependentLookup(ptr20->hGeometry));
					break;
				}
				case MILCMD.MilDrawImage:
				{
					MILCMD_DRAW_IMAGE* ptr12 = (MILCMD_DRAW_IMAGE*)(ptr + sizeof(RecordHeader));
					ctx.DrawImage((ImageSource)DependentLookup(ptr12->hImageSource), ptr12->rectangle);
					break;
				}
				case MILCMD.MilDrawImageAnimate:
				{
					MILCMD_DRAW_IMAGE_ANIMATE* ptr24 = (MILCMD_DRAW_IMAGE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawImage((ptr24->hImageSource == 0) ? null : ((ImageSource)DependentLookup(ptr24->hImageSource)), (ptr24->hRectangleAnimations == 0) ? ptr24->rectangle : ((RectAnimationClockResource)DependentLookup(ptr24->hRectangleAnimations)).CurrentValue);
					break;
				}
				case MILCMD.MilDrawGlyphRun:
				{
					MILCMD_DRAW_GLYPH_RUN* ptr23 = (MILCMD_DRAW_GLYPH_RUN*)(ptr + sizeof(RecordHeader));
					ctx.DrawGlyphRun((Brush)DependentLookup(ptr23->hForegroundBrush), (GlyphRun)DependentLookup(ptr23->hGlyphRun));
					break;
				}
				case MILCMD.MilDrawDrawing:
				{
					MILCMD_DRAW_DRAWING* ptr22 = (MILCMD_DRAW_DRAWING*)(ptr + sizeof(RecordHeader));
					ctx.DrawDrawing((Drawing)DependentLookup(ptr22->hDrawing));
					break;
				}
				case MILCMD.MilDrawVideo:
				{
					MILCMD_DRAW_VIDEO* ptr21 = (MILCMD_DRAW_VIDEO*)(ptr + sizeof(RecordHeader));
					ctx.DrawVideo((MediaPlayer)DependentLookup(ptr21->hPlayer), ptr21->rectangle);
					break;
				}
				case MILCMD.MilDrawVideoAnimate:
				{
					MILCMD_DRAW_VIDEO_ANIMATE* ptr17 = (MILCMD_DRAW_VIDEO_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawVideo((MediaPlayer)DependentLookup(ptr17->hPlayer), (ptr17->hRectangleAnimations == 0) ? ptr17->rectangle : ((RectAnimationClockResource)DependentLookup(ptr17->hRectangleAnimations)).CurrentValue);
					break;
				}
				case MILCMD.MilPushClip:
				{
					MILCMD_PUSH_CLIP* ptr16 = (MILCMD_PUSH_CLIP*)(ptr + sizeof(RecordHeader));
					ctx.PushClip((Geometry)DependentLookup(ptr16->hClipGeometry));
					break;
				}
				case MILCMD.MilPushOpacityMask:
				{
					MILCMD_PUSH_OPACITY_MASK* ptr15 = (MILCMD_PUSH_OPACITY_MASK*)(ptr + sizeof(RecordHeader));
					ctx.PushOpacityMask((Brush)DependentLookup(ptr15->hOpacityMask));
					break;
				}
				case MILCMD.MilPushOpacity:
				{
					MILCMD_PUSH_OPACITY* ptr14 = (MILCMD_PUSH_OPACITY*)(ptr + sizeof(RecordHeader));
					ctx.PushOpacity(ptr14->opacity);
					break;
				}
				case MILCMD.MilPushOpacityAnimate:
				{
					MILCMD_PUSH_OPACITY_ANIMATE* ptr10 = (MILCMD_PUSH_OPACITY_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.PushOpacity((ptr10->hOpacityAnimations == 0) ? ptr10->opacity : ((DoubleAnimationClockResource)DependentLookup(ptr10->hOpacityAnimations)).CurrentValue);
					break;
				}
				case MILCMD.MilPushTransform:
				{
					MILCMD_PUSH_TRANSFORM* ptr9 = (MILCMD_PUSH_TRANSFORM*)(ptr + sizeof(RecordHeader));
					ctx.PushTransform((Transform)DependentLookup(ptr9->hTransform));
					break;
				}
				case MILCMD.MilPushGuidelineSet:
				{
					MILCMD_PUSH_GUIDELINE_SET* ptr8 = (MILCMD_PUSH_GUIDELINE_SET*)(ptr + sizeof(RecordHeader));
					ctx.PushGuidelineSet((GuidelineSet)DependentLookup(ptr8->hGuidelines));
					break;
				}
				case MILCMD.MilPushGuidelineY1:
				{
					MILCMD_PUSH_GUIDELINE_Y1* ptr7 = (MILCMD_PUSH_GUIDELINE_Y1*)(ptr + sizeof(RecordHeader));
					ctx.PushGuidelineY1(ptr7->coordinate);
					break;
				}
				case MILCMD.MilPushGuidelineY2:
				{
					MILCMD_PUSH_GUIDELINE_Y2* ptr6 = (MILCMD_PUSH_GUIDELINE_Y2*)(ptr + sizeof(RecordHeader));
					ctx.PushGuidelineY2(ptr6->leadingCoordinate, ptr6->offsetToDrivenCoordinate);
					break;
				}
				case MILCMD.MilPushEffect:
				{
					MILCMD_PUSH_EFFECT* ptr5 = (MILCMD_PUSH_EFFECT*)(ptr + sizeof(RecordHeader));
					ctx.PushEffect((BitmapEffect)DependentLookup(ptr5->hEffect), (BitmapEffectInput)DependentLookup(ptr5->hEffectInput));
					break;
				}
				case MILCMD.MilPop:
				{
					MILCMD_POP* ptr4 = (MILCMD_POP*)(ptr + sizeof(RecordHeader));
					ctx.Pop();
					break;
				}
				}
			}
		}
	}

	public unsafe void BaseValueDrawingContextWalk(DrawingContextWalker ctx)
	{
		if (_curOffset <= 0)
		{
			return;
		}
		fixed (byte* buffer = _buffer)
		{
			byte* ptr = buffer;
			RecordHeader* ptr3;
			for (byte* ptr2 = buffer + _curOffset; ptr < ptr2; ptr += ptr3->Size)
			{
				if (ctx.ShouldStopWalking)
				{
					break;
				}
				ptr3 = (RecordHeader*)ptr;
				switch (ptr3->Id)
				{
				case MILCMD.MilDrawLine:
				{
					MILCMD_DRAW_LINE* ptr28 = (MILCMD_DRAW_LINE*)(ptr + sizeof(RecordHeader));
					ctx.DrawLine((Pen)DependentLookup(ptr28->hPen), ptr28->point0, ptr28->point1);
					break;
				}
				case MILCMD.MilDrawLineAnimate:
				{
					MILCMD_DRAW_LINE_ANIMATE* ptr27 = (MILCMD_DRAW_LINE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawLine((Pen)DependentLookup(ptr27->hPen), ptr27->point0, ((PointAnimationClockResource)DependentLookup(ptr27->hPoint0Animations)).AnimationClock, ptr27->point1, ((PointAnimationClockResource)DependentLookup(ptr27->hPoint1Animations)).AnimationClock);
					break;
				}
				case MILCMD.MilDrawRectangle:
				{
					MILCMD_DRAW_RECTANGLE* ptr26 = (MILCMD_DRAW_RECTANGLE*)(ptr + sizeof(RecordHeader));
					ctx.DrawRectangle((Brush)DependentLookup(ptr26->hBrush), (Pen)DependentLookup(ptr26->hPen), ptr26->rectangle);
					break;
				}
				case MILCMD.MilDrawRectangleAnimate:
				{
					MILCMD_DRAW_RECTANGLE_ANIMATE* ptr25 = (MILCMD_DRAW_RECTANGLE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawRectangle((Brush)DependentLookup(ptr25->hBrush), (Pen)DependentLookup(ptr25->hPen), ptr25->rectangle, ((RectAnimationClockResource)DependentLookup(ptr25->hRectangleAnimations)).AnimationClock);
					break;
				}
				case MILCMD.MilDrawRoundedRectangle:
				{
					MILCMD_DRAW_ROUNDED_RECTANGLE* ptr24 = (MILCMD_DRAW_ROUNDED_RECTANGLE*)(ptr + sizeof(RecordHeader));
					ctx.DrawRoundedRectangle((Brush)DependentLookup(ptr24->hBrush), (Pen)DependentLookup(ptr24->hPen), ptr24->rectangle, ptr24->radiusX, ptr24->radiusY);
					break;
				}
				case MILCMD.MilDrawRoundedRectangleAnimate:
				{
					MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE* ptr23 = (MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawRoundedRectangle((Brush)DependentLookup(ptr23->hBrush), (Pen)DependentLookup(ptr23->hPen), ptr23->rectangle, ((RectAnimationClockResource)DependentLookup(ptr23->hRectangleAnimations)).AnimationClock, ptr23->radiusX, ((DoubleAnimationClockResource)DependentLookup(ptr23->hRadiusXAnimations)).AnimationClock, ptr23->radiusY, ((DoubleAnimationClockResource)DependentLookup(ptr23->hRadiusYAnimations)).AnimationClock);
					break;
				}
				case MILCMD.MilDrawEllipse:
				{
					MILCMD_DRAW_ELLIPSE* ptr22 = (MILCMD_DRAW_ELLIPSE*)(ptr + sizeof(RecordHeader));
					ctx.DrawEllipse((Brush)DependentLookup(ptr22->hBrush), (Pen)DependentLookup(ptr22->hPen), ptr22->center, ptr22->radiusX, ptr22->radiusY);
					break;
				}
				case MILCMD.MilDrawEllipseAnimate:
				{
					MILCMD_DRAW_ELLIPSE_ANIMATE* ptr21 = (MILCMD_DRAW_ELLIPSE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawEllipse((Brush)DependentLookup(ptr21->hBrush), (Pen)DependentLookup(ptr21->hPen), ptr21->center, ((PointAnimationClockResource)DependentLookup(ptr21->hCenterAnimations)).AnimationClock, ptr21->radiusX, ((DoubleAnimationClockResource)DependentLookup(ptr21->hRadiusXAnimations)).AnimationClock, ptr21->radiusY, ((DoubleAnimationClockResource)DependentLookup(ptr21->hRadiusYAnimations)).AnimationClock);
					break;
				}
				case MILCMD.MilDrawGeometry:
				{
					MILCMD_DRAW_GEOMETRY* ptr20 = (MILCMD_DRAW_GEOMETRY*)(ptr + sizeof(RecordHeader));
					ctx.DrawGeometry((Brush)DependentLookup(ptr20->hBrush), (Pen)DependentLookup(ptr20->hPen), (Geometry)DependentLookup(ptr20->hGeometry));
					break;
				}
				case MILCMD.MilDrawImage:
				{
					MILCMD_DRAW_IMAGE* ptr19 = (MILCMD_DRAW_IMAGE*)(ptr + sizeof(RecordHeader));
					ctx.DrawImage((ImageSource)DependentLookup(ptr19->hImageSource), ptr19->rectangle);
					break;
				}
				case MILCMD.MilDrawImageAnimate:
				{
					MILCMD_DRAW_IMAGE_ANIMATE* ptr18 = (MILCMD_DRAW_IMAGE_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawImage((ImageSource)DependentLookup(ptr18->hImageSource), ptr18->rectangle, ((RectAnimationClockResource)DependentLookup(ptr18->hRectangleAnimations)).AnimationClock);
					break;
				}
				case MILCMD.MilDrawGlyphRun:
				{
					MILCMD_DRAW_GLYPH_RUN* ptr17 = (MILCMD_DRAW_GLYPH_RUN*)(ptr + sizeof(RecordHeader));
					ctx.DrawGlyphRun((Brush)DependentLookup(ptr17->hForegroundBrush), (GlyphRun)DependentLookup(ptr17->hGlyphRun));
					break;
				}
				case MILCMD.MilDrawDrawing:
				{
					MILCMD_DRAW_DRAWING* ptr16 = (MILCMD_DRAW_DRAWING*)(ptr + sizeof(RecordHeader));
					ctx.DrawDrawing((Drawing)DependentLookup(ptr16->hDrawing));
					break;
				}
				case MILCMD.MilDrawVideo:
				{
					MILCMD_DRAW_VIDEO* ptr15 = (MILCMD_DRAW_VIDEO*)(ptr + sizeof(RecordHeader));
					ctx.DrawVideo((MediaPlayer)DependentLookup(ptr15->hPlayer), ptr15->rectangle);
					break;
				}
				case MILCMD.MilDrawVideoAnimate:
				{
					MILCMD_DRAW_VIDEO_ANIMATE* ptr14 = (MILCMD_DRAW_VIDEO_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.DrawVideo((MediaPlayer)DependentLookup(ptr14->hPlayer), ptr14->rectangle, ((RectAnimationClockResource)DependentLookup(ptr14->hRectangleAnimations)).AnimationClock);
					break;
				}
				case MILCMD.MilPushClip:
				{
					MILCMD_PUSH_CLIP* ptr13 = (MILCMD_PUSH_CLIP*)(ptr + sizeof(RecordHeader));
					ctx.PushClip((Geometry)DependentLookup(ptr13->hClipGeometry));
					break;
				}
				case MILCMD.MilPushOpacityMask:
				{
					MILCMD_PUSH_OPACITY_MASK* ptr12 = (MILCMD_PUSH_OPACITY_MASK*)(ptr + sizeof(RecordHeader));
					ctx.PushOpacityMask((Brush)DependentLookup(ptr12->hOpacityMask));
					break;
				}
				case MILCMD.MilPushOpacity:
				{
					MILCMD_PUSH_OPACITY* ptr11 = (MILCMD_PUSH_OPACITY*)(ptr + sizeof(RecordHeader));
					ctx.PushOpacity(ptr11->opacity);
					break;
				}
				case MILCMD.MilPushOpacityAnimate:
				{
					MILCMD_PUSH_OPACITY_ANIMATE* ptr10 = (MILCMD_PUSH_OPACITY_ANIMATE*)(ptr + sizeof(RecordHeader));
					ctx.PushOpacity(ptr10->opacity, ((DoubleAnimationClockResource)DependentLookup(ptr10->hOpacityAnimations)).AnimationClock);
					break;
				}
				case MILCMD.MilPushTransform:
				{
					MILCMD_PUSH_TRANSFORM* ptr9 = (MILCMD_PUSH_TRANSFORM*)(ptr + sizeof(RecordHeader));
					ctx.PushTransform((Transform)DependentLookup(ptr9->hTransform));
					break;
				}
				case MILCMD.MilPushGuidelineSet:
				{
					MILCMD_PUSH_GUIDELINE_SET* ptr8 = (MILCMD_PUSH_GUIDELINE_SET*)(ptr + sizeof(RecordHeader));
					ctx.PushGuidelineSet((GuidelineSet)DependentLookup(ptr8->hGuidelines));
					break;
				}
				case MILCMD.MilPushGuidelineY1:
				{
					MILCMD_PUSH_GUIDELINE_Y1* ptr7 = (MILCMD_PUSH_GUIDELINE_Y1*)(ptr + sizeof(RecordHeader));
					ctx.PushGuidelineY1(ptr7->coordinate);
					break;
				}
				case MILCMD.MilPushGuidelineY2:
				{
					MILCMD_PUSH_GUIDELINE_Y2* ptr6 = (MILCMD_PUSH_GUIDELINE_Y2*)(ptr + sizeof(RecordHeader));
					ctx.PushGuidelineY2(ptr6->leadingCoordinate, ptr6->offsetToDrivenCoordinate);
					break;
				}
				case MILCMD.MilPushEffect:
				{
					MILCMD_PUSH_EFFECT* ptr5 = (MILCMD_PUSH_EFFECT*)(ptr + sizeof(RecordHeader));
					ctx.PushEffect((BitmapEffect)DependentLookup(ptr5->hEffect), (BitmapEffectInput)DependentLookup(ptr5->hEffectInput));
					break;
				}
				case MILCMD.MilPop:
				{
					MILCMD_POP* ptr4 = (MILCMD_POP*)(ptr + sizeof(RecordHeader));
					ctx.Pop();
					break;
				}
				}
			}
		}
	}
}
