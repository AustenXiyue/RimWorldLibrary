using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using MS.Internal;
using MS.Internal.Interop;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Media.Composition;

internal class DUCE
{
	private static class UnsafeNativeMethods
	{
		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilResource_CreateOrAddRefOnChannel(nint pChannel, ResourceType resourceType, ref ResourceHandle hResource);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilResource_DuplicateHandle(nint pSourceChannel, ResourceHandle original, nint pTargetChannel, ref ResourceHandle duplicate);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilConnection_CreateChannel(nint pTransport, nint hChannel, out nint channelHandle);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilConnection_DestroyChannel(nint channelHandle);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilChannel_CloseBatch")]
		internal static extern int MilConnection_CloseBatch(nint channelHandle);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilChannel_CommitChannel")]
		internal static extern int MilConnection_CommitChannel(nint channelHandle);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int WgxConnection_SameThreadPresent(nint pConnection);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilChannel_GetMarshalType(nint channelHandle, out ChannelMarshalType marshalType);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilResource_SendCommand(byte* pbData, uint cbSize, bool sendInSeparateBatch, nint pChannel);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilChannel_BeginCommand(nint pChannel, byte* pbData, uint cbSize, uint cbExtra);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilChannel_AppendCommandData(nint pChannel, byte* pbData, uint cbSize);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilChannel_EndCommand(nint pChannel);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilResource_SendCommandMedia(ResourceHandle handle, SafeMediaHandle pMedia, nint pChannel, bool notifyUceDirect);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilResource_SendCommandBitmapSource(ResourceHandle handle, BitmapSourceSafeMILHandle pBitmapSource, nint pChannel);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilResource_ReleaseOnChannel(nint pChannel, ResourceHandle hResource, out int deleted);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilChannel_SetNotificationWindow(nint pChannel, nint hwnd, WindowMessage message);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilComposition_WaitForNextMessage(nint pChannel, int nCount, nint[] handles, int bWaitAll, uint waitTimeout, out int waitReturn);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilComposition_PeekNextMessage(nint pChannel, out MilMessage.Message message, nint messageSize, out int messageRetrieved);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilResource_GetRefCountOnChannel(nint pChannel, ResourceHandle hResource, out uint refCount);
	}

	internal static class MilMessage
	{
		internal enum Type
		{
			Invalid = 0,
			SyncFlushReply = 1,
			Caps = 4,
			PartitionIsZombie = 6,
			SyncModeStatus = 9,
			Presented = 10,
			BadPixelShader = 16,
			ForceDWORD = -1
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct CapsData
		{
			[FieldOffset(0)]
			internal int CommonMinimumCaps;

			[FieldOffset(4)]
			internal uint DisplayUniqueness;

			[FieldOffset(8)]
			internal MilGraphicsAccelerationCaps Caps;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct PartitionIsZombieStatus
		{
			[FieldOffset(0)]
			internal int HRESULTFailureCode;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct SyncModeStatus
		{
			[FieldOffset(0)]
			internal int Enabled;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct Presented
		{
			[FieldOffset(0)]
			internal MIL_PRESENTATION_RESULTS PresentationResults;

			[FieldOffset(4)]
			internal int RefreshRate;

			[FieldOffset(8)]
			internal long PresentationTime;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct Message
		{
			[FieldOffset(0)]
			internal Type Type;

			[FieldOffset(4)]
			internal int Reserved;

			[FieldOffset(8)]
			internal CapsData Caps;

			[FieldOffset(8)]
			internal PartitionIsZombieStatus HRESULTFailure;

			[FieldOffset(8)]
			internal Presented Presented;

			[FieldOffset(8)]
			internal SyncModeStatus SyncModeStatus;
		}
	}

	internal struct ChannelSet
	{
		internal Channel Channel;

		internal Channel OutOfBandChannel;
	}

	internal sealed class Channel
	{
		private nint _hChannel;

		private Channel _referenceChannel;

		private bool _isSynchronous;

		private bool _isOutOfBandChannel;

		private nint _pConnection;

		internal bool IsConnected => MediaContext.CurrentMediaContext.IsConnected;

		internal ChannelMarshalType MarshalType
		{
			get
			{
				Invariant.Assert(_hChannel != IntPtr.Zero);
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilChannel_GetMarshalType(_hChannel, out var marshalType));
				return marshalType;
			}
		}

		internal bool IsSynchronous => _isSynchronous;

		internal bool IsOutOfBandChannel => _isOutOfBandChannel;

		public Channel(Channel referenceChannel, bool isOutOfBandChannel, nint pConnection, bool isSynchronous)
		{
			nint hChannel = IntPtr.Zero;
			_referenceChannel = referenceChannel;
			_pConnection = pConnection;
			_isOutOfBandChannel = isOutOfBandChannel;
			_isSynchronous = isSynchronous;
			if (referenceChannel != null)
			{
				hChannel = referenceChannel._hChannel;
			}
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilConnection_CreateChannel(_pConnection, hChannel, out _hChannel));
		}

		internal void Commit()
		{
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilConnection_CommitChannel(_hChannel));
			}
		}

		internal void CloseBatch()
		{
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilConnection_CloseBatch(_hChannel));
			}
		}

		internal void SyncFlush()
		{
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(MilCoreApi.MilComposition_SyncFlush(_hChannel));
			}
		}

		internal void Close()
		{
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilConnection_CloseBatch(_hChannel));
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilConnection_CommitChannel(_hChannel));
			}
			_referenceChannel = null;
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilConnection_DestroyChannel(_hChannel));
				_hChannel = IntPtr.Zero;
			}
		}

		internal void Present()
		{
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.WgxConnection_SameThreadPresent(_pConnection));
		}

		internal bool CreateOrAddRefOnChannel(object instance, ref ResourceHandle handle, ResourceType resourceType)
		{
			bool isNull = handle.IsNull;
			Invariant.Assert(_hChannel != IntPtr.Zero);
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilResource_CreateOrAddRefOnChannel(_hChannel, resourceType, ref handle));
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.PERF_LOW))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.CreateOrAddResourceOnChannel, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.PERF_LOW, PerfService.GetPerfElementID(instance), _hChannel, (uint)handle, (uint)resourceType);
			}
			return isNull;
		}

		internal ResourceHandle DuplicateHandle(ResourceHandle original, Channel targetChannel)
		{
			ResourceHandle duplicate = ResourceHandle.Null;
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilResource_DuplicateHandle(_hChannel, original, targetChannel._hChannel, ref duplicate));
			return duplicate;
		}

		internal bool ReleaseOnChannel(ResourceHandle handle)
		{
			Invariant.Assert(_hChannel != IntPtr.Zero);
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilResource_ReleaseOnChannel(_hChannel, handle, out var deleted));
			if (deleted != 0 && EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.PERF_LOW))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.ReleaseOnChannel, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.PERF_LOW, _hChannel, (uint)handle);
			}
			return deleted != 0;
		}

		internal uint GetRefCount(ResourceHandle handle)
		{
			Invariant.Assert(_hChannel != IntPtr.Zero);
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilResource_GetRefCountOnChannel(_hChannel, handle, out var refCount));
			return refCount;
		}

		internal unsafe void SendCommand(byte* pCommandData, int cSize)
		{
			SendCommand(pCommandData, cSize, sendInSeparateBatch: false);
		}

		internal unsafe void SendCommand(byte* pCommandData, int cSize, bool sendInSeparateBatch)
		{
			Invariant.Assert(pCommandData != null && cSize > 0);
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilResource_SendCommand(pCommandData, checked((uint)cSize), sendInSeparateBatch, _hChannel));
			}
		}

		internal unsafe void BeginCommand(byte* pbCommandData, int cbSize, int cbExtra)
		{
			Invariant.Assert(cbSize > 0);
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(checked(UnsafeNativeMethods.MilChannel_BeginCommand(_hChannel, pbCommandData, (uint)cbSize, (uint)cbExtra)));
			}
		}

		internal unsafe void AppendCommandData(byte* pbCommandData, int cbSize)
		{
			Invariant.Assert(pbCommandData != null && cbSize > 0);
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilChannel_AppendCommandData(_hChannel, pbCommandData, checked((uint)cbSize)));
			}
		}

		internal void EndCommand()
		{
			if (_hChannel != IntPtr.Zero)
			{
				MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilChannel_EndCommand(_hChannel));
			}
		}

		internal void SendCommandBitmapSource(ResourceHandle imageHandle, BitmapSourceSafeMILHandle pBitmapSource)
		{
			Invariant.Assert(pBitmapSource != null && !pBitmapSource.IsInvalid);
			Invariant.Assert(_hChannel != IntPtr.Zero);
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilResource_SendCommandBitmapSource(imageHandle, pBitmapSource, _hChannel));
		}

		internal void SendCommandMedia(ResourceHandle mediaHandle, SafeMediaHandle pMedia, bool notifyUceDirect)
		{
			Invariant.Assert(pMedia != null && !pMedia.IsInvalid);
			Invariant.Assert(_hChannel != IntPtr.Zero);
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilResource_SendCommandMedia(mediaHandle, pMedia, _hChannel, notifyUceDirect));
		}

		internal void SetNotificationWindow(nint hwnd, WindowMessage message)
		{
			Invariant.Assert(_hChannel != IntPtr.Zero);
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilChannel_SetNotificationWindow(_hChannel, hwnd, message));
		}

		internal void WaitForNextMessage()
		{
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilComposition_WaitForNextMessage(_hChannel, 0, null, 1, uint.MaxValue, out var _));
		}

		internal unsafe bool PeekNextMessage(out MilMessage.Message message)
		{
			Invariant.Assert(_hChannel != IntPtr.Zero);
			MS.Internal.HRESULT.Check(UnsafeNativeMethods.MilComposition_PeekNextMessage(_hChannel, out message, sizeof(MilMessage.Message), out var messageRetrieved));
			return messageRetrieved != 0;
		}
	}

	internal struct Resource
	{
		public static readonly Resource Null = new Resource(ResourceHandle.Null);

		private ResourceHandle _handle;

		public ResourceHandle Handle => _handle;

		public Resource(ResourceHandle h)
		{
			_handle = h;
		}

		public bool CreateOrAddRefOnChannel(object instance, Channel channel, ResourceType type)
		{
			return channel.CreateOrAddRefOnChannel(instance, ref _handle, type);
		}

		public bool ReleaseOnChannel(Channel channel)
		{
			if (channel.ReleaseOnChannel(_handle))
			{
				_handle = ResourceHandle.Null;
				return true;
			}
			return false;
		}

		public bool IsOnChannel(Channel channel)
		{
			return !_handle.IsNull;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct ResourceHandle
	{
		public static readonly ResourceHandle Null = new ResourceHandle(0u);

		[FieldOffset(0)]
		private uint _handle;

		public bool IsNull => _handle == 0;

		public static explicit operator uint(ResourceHandle r)
		{
			return r._handle;
		}

		public ResourceHandle(uint handle)
		{
			_handle = handle;
		}
	}

	internal struct Map<ValueType>
	{
		private struct Entry
		{
			public object _key;

			public ValueType _value;

			public Entry(object k, ValueType v)
			{
				_key = k;
				_value = v;
			}
		}

		private const int FOUND_IN_INLINE_STORAGE = -1;

		private const int NOT_FOUND = -2;

		private Entry _first;

		private List<Entry> _others;

		public bool IsEmpty()
		{
			if (_first._key != null)
			{
				return false;
			}
			if (_others != null)
			{
				return false;
			}
			return true;
		}

		private int Find(object key)
		{
			int result = -2;
			if (_first._key != null)
			{
				if (_first._key == key)
				{
					result = -1;
				}
				else if (_others != null)
				{
					for (int i = 0; i < _others.Count; i++)
					{
						if (_others[i]._key == key)
						{
							result = i;
							break;
						}
					}
				}
			}
			return result;
		}

		public void Set(object key, ValueType value)
		{
			int num = Find(key);
			switch (num)
			{
			case -1:
				_first._value = value;
				break;
			case -2:
				if (_first._key == null)
				{
					_first = new Entry(key, value);
					break;
				}
				if (_others == null)
				{
					_others = new List<Entry>(2);
				}
				_others.Add(new Entry(key, value));
				break;
			default:
				_others[num] = new Entry(key, value);
				break;
			}
		}

		public bool Remove(object key)
		{
			int num = Find(key);
			if (num == -1)
			{
				if (_others != null)
				{
					int num2 = _others.Count - 1;
					_first = _others[num2];
					if (num2 == 0)
					{
						_others = null;
					}
					else
					{
						_others.RemoveAt(num2);
					}
				}
				else
				{
					_first = default(Entry);
				}
				return true;
			}
			if (num >= 0)
			{
				if (_others.Count == 1)
				{
					_others = null;
				}
				else
				{
					_others.RemoveAt(num);
				}
				return true;
			}
			return false;
		}

		public bool Get(object key, out ValueType value)
		{
			int num = Find(key);
			value = default(ValueType);
			if (num == -1)
			{
				value = _first._value;
				return true;
			}
			if (num >= 0)
			{
				value = _others[num]._value;
				return true;
			}
			return false;
		}

		public int Count()
		{
			if (_first._key == null)
			{
				return 0;
			}
			if (_others == null)
			{
				return 1;
			}
			return _others.Count + 1;
		}

		public object Get(int index)
		{
			if (index >= Count())
			{
				return null;
			}
			if (index == 0)
			{
				return _first._key;
			}
			return _others[index - 1]._key;
		}
	}

	internal struct Map
	{
		private struct Entry
		{
			public object _key;

			public ResourceHandle _value;

			public Entry(object k, ResourceHandle v)
			{
				_key = k;
				_value = v;
			}
		}

		private const int FOUND_IN_INLINE_STORAGE = -1;

		private const int NOT_FOUND = -2;

		private Entry _head;

		public bool IsEmpty()
		{
			return _head._key == null;
		}

		private int Find(object key)
		{
			int result = -2;
			if (_head._key != null)
			{
				if (_head._key == key)
				{
					result = -1;
				}
				else if (_head._value.IsNull)
				{
					List<Entry> list = (List<Entry>)_head._key;
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i]._key == key)
						{
							result = i;
							break;
						}
					}
				}
			}
			return result;
		}

		public void Set(object key, ResourceHandle value)
		{
			int num = Find(key);
			switch (num)
			{
			case -1:
				_head._value = value;
				break;
			case -2:
				if (_head._key == null)
				{
					_head = new Entry(key, value);
				}
				else if (!_head._value.IsNull)
				{
					List<Entry> list = new List<Entry>(2);
					list.Add(_head);
					list.Add(new Entry(key, value));
					_head._key = list;
					_head._value = ResourceHandle.Null;
				}
				else
				{
					((List<Entry>)_head._key).Add(new Entry(key, value));
				}
				break;
			default:
				((List<Entry>)_head._key)[num] = new Entry(key, value);
				break;
			}
		}

		public bool Remove(object key)
		{
			int num = Find(key);
			if (num == -1)
			{
				_head = default(Entry);
				return true;
			}
			if (num >= 0)
			{
				List<Entry> list = (List<Entry>)_head._key;
				if (Count() == 2)
				{
					_head = list[1 - num];
				}
				else
				{
					list.RemoveAt(num);
				}
				return true;
			}
			return false;
		}

		public bool Get(object key, out ResourceHandle value)
		{
			int num = Find(key);
			value = ResourceHandle.Null;
			if (num == -1)
			{
				value = _head._value;
				return true;
			}
			if (num >= 0)
			{
				value = ((List<Entry>)_head._key)[num]._value;
				return true;
			}
			return false;
		}

		public int Count()
		{
			if (_head._key == null)
			{
				return 0;
			}
			if (!_head._value.IsNull)
			{
				return 1;
			}
			return ((List<Entry>)_head._key).Count;
		}

		public object Get(int index)
		{
			if (index >= Count() || index < 0)
			{
				return null;
			}
			if (Count() == 1)
			{
				return _head._key;
			}
			return ((List<Entry>)_head._key)[index]._key;
		}
	}

	internal class ShareableDUCEMultiChannelResource
	{
		public MultiChannelResource _duceResource;
	}

	internal struct MultiChannelResource
	{
		private Map _map;

		public bool IsOnAnyChannel => !_map.IsEmpty();

		public bool CreateOrAddRefOnChannel(object instance, Channel channel, ResourceType type)
		{
			ResourceHandle value;
			bool num = _map.Get(channel, out value);
			bool result = channel.CreateOrAddRefOnChannel(instance, ref value, type);
			if (!num)
			{
				_map.Set(channel, value);
			}
			return result;
		}

		public ResourceHandle DuplicateHandle(Channel sourceChannel, Channel targetChannel)
		{
			ResourceHandle @null = ResourceHandle.Null;
			ResourceHandle value = ResourceHandle.Null;
			_map.Get(sourceChannel, out value);
			@null = sourceChannel.DuplicateHandle(value, targetChannel);
			if (!@null.IsNull)
			{
				_map.Set(targetChannel, @null);
			}
			return @null;
		}

		public bool ReleaseOnChannel(Channel channel)
		{
			_map.Get(channel, out var value);
			if (channel.ReleaseOnChannel(value))
			{
				_map.Remove(channel);
				return true;
			}
			return false;
		}

		public ResourceHandle GetHandle(Channel channel)
		{
			if (channel != null)
			{
				_map.Get(channel, out var value);
				return value;
			}
			return ResourceHandle.Null;
		}

		public bool IsOnChannel(Channel channel)
		{
			return !GetHandle(channel).IsNull;
		}

		public int GetChannelCount()
		{
			return _map.Count();
		}

		public Channel GetChannel(int index)
		{
			return _map.Get(index) as Channel;
		}

		public uint GetRefCountOnChannel(Channel channel)
		{
			_map.Get(channel, out var value);
			return channel.GetRefCount(value);
		}
	}

	internal static class CompositionNode
	{
		internal unsafe static void SetTransform(ResourceHandle hCompositionNode, ResourceHandle hTransform, Channel channel)
		{
			MILCMD_VISUAL_SETTRANSFORM mILCMD_VISUAL_SETTRANSFORM = default(MILCMD_VISUAL_SETTRANSFORM);
			mILCMD_VISUAL_SETTRANSFORM.Type = MILCMD.MilCmdVisualSetTransform;
			mILCMD_VISUAL_SETTRANSFORM.Handle = hCompositionNode;
			mILCMD_VISUAL_SETTRANSFORM.hTransform = hTransform;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETTRANSFORM), sizeof(MILCMD_VISUAL_SETTRANSFORM));
		}

		internal unsafe static void SetEffect(ResourceHandle hCompositionNode, ResourceHandle hEffect, Channel channel)
		{
			MILCMD_VISUAL_SETEFFECT mILCMD_VISUAL_SETEFFECT = default(MILCMD_VISUAL_SETEFFECT);
			mILCMD_VISUAL_SETEFFECT.Type = MILCMD.MilCmdVisualSetEffect;
			mILCMD_VISUAL_SETEFFECT.Handle = hCompositionNode;
			mILCMD_VISUAL_SETEFFECT.hEffect = hEffect;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETEFFECT), sizeof(MILCMD_VISUAL_SETEFFECT));
		}

		internal unsafe static void SetCacheMode(ResourceHandle hCompositionNode, ResourceHandle hCacheMode, Channel channel)
		{
			MILCMD_VISUAL_SETCACHEMODE mILCMD_VISUAL_SETCACHEMODE = default(MILCMD_VISUAL_SETCACHEMODE);
			mILCMD_VISUAL_SETCACHEMODE.Type = MILCMD.MilCmdVisualSetCacheMode;
			mILCMD_VISUAL_SETCACHEMODE.Handle = hCompositionNode;
			mILCMD_VISUAL_SETCACHEMODE.hCacheMode = hCacheMode;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETCACHEMODE), sizeof(MILCMD_VISUAL_SETCACHEMODE));
		}

		internal unsafe static void SetOffset(ResourceHandle hCompositionNode, double offsetX, double offsetY, Channel channel)
		{
			MILCMD_VISUAL_SETOFFSET mILCMD_VISUAL_SETOFFSET = default(MILCMD_VISUAL_SETOFFSET);
			mILCMD_VISUAL_SETOFFSET.Type = MILCMD.MilCmdVisualSetOffset;
			mILCMD_VISUAL_SETOFFSET.Handle = hCompositionNode;
			mILCMD_VISUAL_SETOFFSET.offsetX = offsetX;
			mILCMD_VISUAL_SETOFFSET.offsetY = offsetY;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETOFFSET), sizeof(MILCMD_VISUAL_SETOFFSET));
		}

		internal unsafe static void SetContent(ResourceHandle hCompositionNode, ResourceHandle hContent, Channel channel)
		{
			MILCMD_VISUAL_SETCONTENT mILCMD_VISUAL_SETCONTENT = default(MILCMD_VISUAL_SETCONTENT);
			mILCMD_VISUAL_SETCONTENT.Type = MILCMD.MilCmdVisualSetContent;
			mILCMD_VISUAL_SETCONTENT.Handle = hCompositionNode;
			mILCMD_VISUAL_SETCONTENT.hContent = hContent;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETCONTENT), sizeof(MILCMD_VISUAL_SETCONTENT));
		}

		internal unsafe static void SetAlpha(ResourceHandle hCompositionNode, double alpha, Channel channel)
		{
			MILCMD_VISUAL_SETALPHA mILCMD_VISUAL_SETALPHA = default(MILCMD_VISUAL_SETALPHA);
			mILCMD_VISUAL_SETALPHA.Type = MILCMD.MilCmdVisualSetAlpha;
			mILCMD_VISUAL_SETALPHA.Handle = hCompositionNode;
			mILCMD_VISUAL_SETALPHA.alpha = alpha;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETALPHA), sizeof(MILCMD_VISUAL_SETALPHA));
		}

		internal unsafe static void SetAlphaMask(ResourceHandle hCompositionNode, ResourceHandle hAlphaMaskBrush, Channel channel)
		{
			MILCMD_VISUAL_SETALPHAMASK mILCMD_VISUAL_SETALPHAMASK = default(MILCMD_VISUAL_SETALPHAMASK);
			mILCMD_VISUAL_SETALPHAMASK.Type = MILCMD.MilCmdVisualSetAlphaMask;
			mILCMD_VISUAL_SETALPHAMASK.Handle = hCompositionNode;
			mILCMD_VISUAL_SETALPHAMASK.hAlphaMask = hAlphaMaskBrush;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETALPHAMASK), sizeof(MILCMD_VISUAL_SETALPHAMASK));
		}

		internal unsafe static void SetScrollableAreaClip(ResourceHandle hCompositionNode, Rect? clip, Channel channel)
		{
			MILCMD_VISUAL_SETSCROLLABLEAREACLIP mILCMD_VISUAL_SETSCROLLABLEAREACLIP = default(MILCMD_VISUAL_SETSCROLLABLEAREACLIP);
			mILCMD_VISUAL_SETSCROLLABLEAREACLIP.Type = MILCMD.MilCmdVisualSetScrollableAreaClip;
			mILCMD_VISUAL_SETSCROLLABLEAREACLIP.Handle = hCompositionNode;
			mILCMD_VISUAL_SETSCROLLABLEAREACLIP.IsEnabled = (clip.HasValue ? 1u : 0u);
			if (clip.HasValue)
			{
				mILCMD_VISUAL_SETSCROLLABLEAREACLIP.Clip = clip.Value;
			}
			else
			{
				mILCMD_VISUAL_SETSCROLLABLEAREACLIP.Clip = Rect.Empty;
			}
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETSCROLLABLEAREACLIP), sizeof(MILCMD_VISUAL_SETSCROLLABLEAREACLIP));
		}

		internal unsafe static void SetClip(ResourceHandle hCompositionNode, ResourceHandle hClip, Channel channel)
		{
			MILCMD_VISUAL_SETCLIP mILCMD_VISUAL_SETCLIP = default(MILCMD_VISUAL_SETCLIP);
			mILCMD_VISUAL_SETCLIP.Type = MILCMD.MilCmdVisualSetClip;
			mILCMD_VISUAL_SETCLIP.Handle = hCompositionNode;
			mILCMD_VISUAL_SETCLIP.hClip = hClip;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETCLIP), sizeof(MILCMD_VISUAL_SETCLIP));
		}

		internal unsafe static void SetRenderOptions(ResourceHandle hCompositionNode, MilRenderOptions renderOptions, Channel channel)
		{
			MILCMD_VISUAL_SETRENDEROPTIONS mILCMD_VISUAL_SETRENDEROPTIONS = default(MILCMD_VISUAL_SETRENDEROPTIONS);
			mILCMD_VISUAL_SETRENDEROPTIONS.Type = MILCMD.MilCmdVisualSetRenderOptions;
			mILCMD_VISUAL_SETRENDEROPTIONS.Handle = hCompositionNode;
			mILCMD_VISUAL_SETRENDEROPTIONS.renderOptions = renderOptions;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_SETRENDEROPTIONS), sizeof(MILCMD_VISUAL_SETRENDEROPTIONS));
		}

		internal unsafe static void RemoveChild(ResourceHandle hCompositionNode, ResourceHandle hChild, Channel channel)
		{
			MILCMD_VISUAL_REMOVECHILD mILCMD_VISUAL_REMOVECHILD = default(MILCMD_VISUAL_REMOVECHILD);
			mILCMD_VISUAL_REMOVECHILD.Type = MILCMD.MilCmdVisualRemoveChild;
			mILCMD_VISUAL_REMOVECHILD.Handle = hCompositionNode;
			mILCMD_VISUAL_REMOVECHILD.hChild = hChild;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_REMOVECHILD), sizeof(MILCMD_VISUAL_REMOVECHILD));
		}

		internal unsafe static void RemoveAllChildren(ResourceHandle hCompositionNode, Channel channel)
		{
			MILCMD_VISUAL_REMOVEALLCHILDREN mILCMD_VISUAL_REMOVEALLCHILDREN = default(MILCMD_VISUAL_REMOVEALLCHILDREN);
			mILCMD_VISUAL_REMOVEALLCHILDREN.Type = MILCMD.MilCmdVisualRemoveAllChildren;
			mILCMD_VISUAL_REMOVEALLCHILDREN.Handle = hCompositionNode;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_REMOVEALLCHILDREN), sizeof(MILCMD_VISUAL_REMOVEALLCHILDREN));
		}

		internal unsafe static void InsertChildAt(ResourceHandle hCompositionNode, ResourceHandle hChild, uint iPosition, Channel channel)
		{
			MILCMD_VISUAL_INSERTCHILDAT mILCMD_VISUAL_INSERTCHILDAT = default(MILCMD_VISUAL_INSERTCHILDAT);
			mILCMD_VISUAL_INSERTCHILDAT.Type = MILCMD.MilCmdVisualInsertChildAt;
			mILCMD_VISUAL_INSERTCHILDAT.Handle = hCompositionNode;
			mILCMD_VISUAL_INSERTCHILDAT.hChild = hChild;
			mILCMD_VISUAL_INSERTCHILDAT.index = iPosition;
			channel.SendCommand((byte*)(&mILCMD_VISUAL_INSERTCHILDAT), sizeof(MILCMD_VISUAL_INSERTCHILDAT));
		}

		internal unsafe static void SetGuidelineCollection(ResourceHandle hCompositionNode, DoubleCollection guidelinesX, DoubleCollection guidelinesY, Channel channel)
		{
			int num = guidelinesX?.Count ?? 0;
			int num2 = guidelinesY?.Count ?? 0;
			checked
			{
				int num3 = num + num2;
				MILCMD_VISUAL_SETGUIDELINECOLLECTION mILCMD_VISUAL_SETGUIDELINECOLLECTION = default(MILCMD_VISUAL_SETGUIDELINECOLLECTION);
				mILCMD_VISUAL_SETGUIDELINECOLLECTION.Type = MILCMD.MilCmdVisualSetGuidelineCollection;
				mILCMD_VISUAL_SETGUIDELINECOLLECTION.Handle = hCompositionNode;
				mILCMD_VISUAL_SETGUIDELINECOLLECTION.countX = (ushort)num;
				mILCMD_VISUAL_SETGUIDELINECOLLECTION.countY = (ushort)num2;
				if (num == 0 && num2 == 0)
				{
					channel.SendCommand(unchecked((byte*)(&mILCMD_VISUAL_SETGUIDELINECOLLECTION)), sizeof(MILCMD_VISUAL_SETGUIDELINECOLLECTION));
					return;
				}
				double[] array = new double[num3];
				if (num != 0)
				{
					guidelinesX.CopyTo(array, 0);
					Array.Sort(array, 0, num);
				}
				if (num2 != 0)
				{
					guidelinesY.CopyTo(array, num);
					Array.Sort(array, num, num2);
				}
				float[] array2 = new float[num3];
				for (int i = 0; i < num3; i++)
				{
					array2[i] = (float)array[i];
				}
				channel.BeginCommand(unchecked((byte*)(&mILCMD_VISUAL_SETGUIDELINECOLLECTION)), sizeof(MILCMD_VISUAL_SETGUIDELINECOLLECTION), 4 * num3);
				fixed (float* pbCommandData = array2)
				{
					channel.AppendCommandData(unchecked((byte*)pbCommandData), 4 * num3);
				}
				channel.EndCommand();
			}
		}
	}

	internal static class Viewport3DVisualNode
	{
		internal unsafe static void SetCamera(ResourceHandle hCompositionNode, ResourceHandle hCamera, Channel channel)
		{
			MILCMD_VIEWPORT3DVISUAL_SETCAMERA mILCMD_VIEWPORT3DVISUAL_SETCAMERA = default(MILCMD_VIEWPORT3DVISUAL_SETCAMERA);
			mILCMD_VIEWPORT3DVISUAL_SETCAMERA.Type = MILCMD.MilCmdViewport3DVisualSetCamera;
			mILCMD_VIEWPORT3DVISUAL_SETCAMERA.Handle = hCompositionNode;
			mILCMD_VIEWPORT3DVISUAL_SETCAMERA.hCamera = hCamera;
			channel.SendCommand((byte*)(&mILCMD_VIEWPORT3DVISUAL_SETCAMERA), sizeof(MILCMD_VIEWPORT3DVISUAL_SETCAMERA));
		}

		internal unsafe static void SetViewport(ResourceHandle hCompositionNode, Rect viewport, Channel channel)
		{
			MILCMD_VIEWPORT3DVISUAL_SETVIEWPORT mILCMD_VIEWPORT3DVISUAL_SETVIEWPORT = default(MILCMD_VIEWPORT3DVISUAL_SETVIEWPORT);
			mILCMD_VIEWPORT3DVISUAL_SETVIEWPORT.Type = MILCMD.MilCmdViewport3DVisualSetViewport;
			mILCMD_VIEWPORT3DVISUAL_SETVIEWPORT.Handle = hCompositionNode;
			mILCMD_VIEWPORT3DVISUAL_SETVIEWPORT.Viewport = viewport;
			channel.SendCommand((byte*)(&mILCMD_VIEWPORT3DVISUAL_SETVIEWPORT), sizeof(MILCMD_VIEWPORT3DVISUAL_SETVIEWPORT));
		}

		internal unsafe static void Set3DChild(ResourceHandle hCompositionNode, ResourceHandle hVisual3D, Channel channel)
		{
			MILCMD_VIEWPORT3DVISUAL_SET3DCHILD mILCMD_VIEWPORT3DVISUAL_SET3DCHILD = default(MILCMD_VIEWPORT3DVISUAL_SET3DCHILD);
			mILCMD_VIEWPORT3DVISUAL_SET3DCHILD.Type = MILCMD.MilCmdViewport3DVisualSet3DChild;
			mILCMD_VIEWPORT3DVISUAL_SET3DCHILD.Handle = hCompositionNode;
			mILCMD_VIEWPORT3DVISUAL_SET3DCHILD.hChild = hVisual3D;
			channel.SendCommand((byte*)(&mILCMD_VIEWPORT3DVISUAL_SET3DCHILD), sizeof(MILCMD_VIEWPORT3DVISUAL_SET3DCHILD));
		}
	}

	internal static class Visual3DNode
	{
		internal unsafe static void RemoveChild(ResourceHandle hCompositionNode, ResourceHandle hChild, Channel channel)
		{
			MILCMD_VISUAL3D_REMOVECHILD mILCMD_VISUAL3D_REMOVECHILD = default(MILCMD_VISUAL3D_REMOVECHILD);
			mILCMD_VISUAL3D_REMOVECHILD.Type = MILCMD.MilCmdVisual3DRemoveChild;
			mILCMD_VISUAL3D_REMOVECHILD.Handle = hCompositionNode;
			mILCMD_VISUAL3D_REMOVECHILD.hChild = hChild;
			channel.SendCommand((byte*)(&mILCMD_VISUAL3D_REMOVECHILD), sizeof(MILCMD_VISUAL3D_REMOVECHILD));
		}

		internal unsafe static void RemoveAllChildren(ResourceHandle hCompositionNode, Channel channel)
		{
			MILCMD_VISUAL3D_REMOVEALLCHILDREN mILCMD_VISUAL3D_REMOVEALLCHILDREN = default(MILCMD_VISUAL3D_REMOVEALLCHILDREN);
			mILCMD_VISUAL3D_REMOVEALLCHILDREN.Type = MILCMD.MilCmdVisual3DRemoveAllChildren;
			mILCMD_VISUAL3D_REMOVEALLCHILDREN.Handle = hCompositionNode;
			channel.SendCommand((byte*)(&mILCMD_VISUAL3D_REMOVEALLCHILDREN), sizeof(MILCMD_VISUAL3D_REMOVEALLCHILDREN));
		}

		internal unsafe static void InsertChildAt(ResourceHandle hCompositionNode, ResourceHandle hChild, uint iPosition, Channel channel)
		{
			MILCMD_VISUAL3D_INSERTCHILDAT mILCMD_VISUAL3D_INSERTCHILDAT = default(MILCMD_VISUAL3D_INSERTCHILDAT);
			mILCMD_VISUAL3D_INSERTCHILDAT.Type = MILCMD.MilCmdVisual3DInsertChildAt;
			mILCMD_VISUAL3D_INSERTCHILDAT.Handle = hCompositionNode;
			mILCMD_VISUAL3D_INSERTCHILDAT.hChild = hChild;
			mILCMD_VISUAL3D_INSERTCHILDAT.index = iPosition;
			channel.SendCommand((byte*)(&mILCMD_VISUAL3D_INSERTCHILDAT), sizeof(MILCMD_VISUAL3D_INSERTCHILDAT));
		}

		internal unsafe static void SetContent(ResourceHandle hCompositionNode, ResourceHandle hContent, Channel channel)
		{
			MILCMD_VISUAL3D_SETCONTENT mILCMD_VISUAL3D_SETCONTENT = default(MILCMD_VISUAL3D_SETCONTENT);
			mILCMD_VISUAL3D_SETCONTENT.Type = MILCMD.MilCmdVisual3DSetContent;
			mILCMD_VISUAL3D_SETCONTENT.Handle = hCompositionNode;
			mILCMD_VISUAL3D_SETCONTENT.hContent = hContent;
			channel.SendCommand((byte*)(&mILCMD_VISUAL3D_SETCONTENT), sizeof(MILCMD_VISUAL3D_SETCONTENT));
		}

		internal unsafe static void SetTransform(ResourceHandle hCompositionNode, ResourceHandle hTransform, Channel channel)
		{
			MILCMD_VISUAL3D_SETTRANSFORM mILCMD_VISUAL3D_SETTRANSFORM = default(MILCMD_VISUAL3D_SETTRANSFORM);
			mILCMD_VISUAL3D_SETTRANSFORM.Type = MILCMD.MilCmdVisual3DSetTransform;
			mILCMD_VISUAL3D_SETTRANSFORM.Handle = hCompositionNode;
			mILCMD_VISUAL3D_SETTRANSFORM.hTransform = hTransform;
			channel.SendCommand((byte*)(&mILCMD_VISUAL3D_SETTRANSFORM), sizeof(MILCMD_VISUAL3D_SETTRANSFORM));
		}
	}

	internal static class CompositionTarget
	{
		internal unsafe static void HwndInitialize(ResourceHandle hCompositionTarget, nint hWnd, int nWidth, int nHeight, bool softwareOnly, int dpiAwarenessContext, DpiScale dpiScale, Channel channel)
		{
			MILCMD_HWNDTARGET_CREATE mILCMD_HWNDTARGET_CREATE = default(MILCMD_HWNDTARGET_CREATE);
			mILCMD_HWNDTARGET_CREATE.Type = MILCMD.MilCmdHwndTargetCreate;
			mILCMD_HWNDTARGET_CREATE.Handle = hCompositionTarget;
			nuint num = new UIntPtr(((IntPtr)hWnd).ToPointer());
			mILCMD_HWNDTARGET_CREATE.hwnd = num;
			mILCMD_HWNDTARGET_CREATE.width = (uint)nWidth;
			mILCMD_HWNDTARGET_CREATE.height = (uint)nHeight;
			mILCMD_HWNDTARGET_CREATE.clearColor.b = 0f;
			mILCMD_HWNDTARGET_CREATE.clearColor.r = 0f;
			mILCMD_HWNDTARGET_CREATE.clearColor.g = 0f;
			mILCMD_HWNDTARGET_CREATE.clearColor.a = 1f;
			mILCMD_HWNDTARGET_CREATE.flags = 12u;
			if (softwareOnly)
			{
				mILCMD_HWNDTARGET_CREATE.flags |= 1u;
			}
			bool? enableMultiMonitorDisplayClipping = CoreCompatibilityPreferences.EnableMultiMonitorDisplayClipping;
			if (enableMultiMonitorDisplayClipping.HasValue)
			{
				mILCMD_HWNDTARGET_CREATE.flags |= 32768u;
				if (!enableMultiMonitorDisplayClipping.Value)
				{
					mILCMD_HWNDTARGET_CREATE.flags |= 16384u;
				}
			}
			if (CoreAppContextSwitches.DisableDirtyRectangles)
			{
				mILCMD_HWNDTARGET_CREATE.flags |= 65536u;
			}
			mILCMD_HWNDTARGET_CREATE.hBitmap = ResourceHandle.Null;
			mILCMD_HWNDTARGET_CREATE.stride = 0u;
			mILCMD_HWNDTARGET_CREATE.ePixelFormat = 0u;
			mILCMD_HWNDTARGET_CREATE.hSection = 0uL;
			mILCMD_HWNDTARGET_CREATE.masterDevice = 0uL;
			mILCMD_HWNDTARGET_CREATE.DpiAwarenessContext = dpiAwarenessContext;
			mILCMD_HWNDTARGET_CREATE.DpiX = dpiScale.DpiScaleX;
			mILCMD_HWNDTARGET_CREATE.DpiY = dpiScale.DpiScaleY;
			channel.SendCommand((byte*)(&mILCMD_HWNDTARGET_CREATE), sizeof(MILCMD_HWNDTARGET_CREATE), sendInSeparateBatch: false);
		}

		internal unsafe static void ProcessDpiChanged(ResourceHandle hCompositionTarget, DpiScale dpiScale, bool afterParent, Channel channel)
		{
			MILCMD_HWNDTARGET_DPICHANGED mILCMD_HWNDTARGET_DPICHANGED = default(MILCMD_HWNDTARGET_DPICHANGED);
			mILCMD_HWNDTARGET_DPICHANGED.Type = MILCMD.MilCmdHwndTargetDpiChanged;
			mILCMD_HWNDTARGET_DPICHANGED.Handle = hCompositionTarget;
			mILCMD_HWNDTARGET_DPICHANGED.DpiX = dpiScale.DpiScaleX;
			mILCMD_HWNDTARGET_DPICHANGED.DpiY = dpiScale.DpiScaleY;
			mILCMD_HWNDTARGET_DPICHANGED.AfterParent = (afterParent ? 1u : 0u);
			channel.SendCommand((byte*)(&mILCMD_HWNDTARGET_DPICHANGED), sizeof(MILCMD_HWNDTARGET_DPICHANGED), sendInSeparateBatch: false);
		}

		internal unsafe static void PrintInitialize(ResourceHandle hCompositionTarget, nint pRenderTarget, int nWidth, int nHeight, Channel channel)
		{
			MILCMD_GENERICTARGET_CREATE mILCMD_GENERICTARGET_CREATE = default(MILCMD_GENERICTARGET_CREATE);
			mILCMD_GENERICTARGET_CREATE.Type = MILCMD.MilCmdGenericTargetCreate;
			mILCMD_GENERICTARGET_CREATE.Handle = hCompositionTarget;
			mILCMD_GENERICTARGET_CREATE.hwnd = 0uL;
			mILCMD_GENERICTARGET_CREATE.pRenderTarget = (ulong)pRenderTarget;
			mILCMD_GENERICTARGET_CREATE.width = (uint)nWidth;
			mILCMD_GENERICTARGET_CREATE.height = (uint)nHeight;
			mILCMD_GENERICTARGET_CREATE.dummy = 0u;
			channel.SendCommand((byte*)(&mILCMD_GENERICTARGET_CREATE), sizeof(MILCMD_GENERICTARGET_CREATE), sendInSeparateBatch: false);
		}

		internal unsafe static void SetClearColor(ResourceHandle hCompositionTarget, Color color, Channel channel)
		{
			MILCMD_TARGET_SETCLEARCOLOR mILCMD_TARGET_SETCLEARCOLOR = default(MILCMD_TARGET_SETCLEARCOLOR);
			mILCMD_TARGET_SETCLEARCOLOR.Type = MILCMD.MilCmdTargetSetClearColor;
			mILCMD_TARGET_SETCLEARCOLOR.Handle = hCompositionTarget;
			mILCMD_TARGET_SETCLEARCOLOR.clearColor.b = color.ScB;
			mILCMD_TARGET_SETCLEARCOLOR.clearColor.r = color.ScR;
			mILCMD_TARGET_SETCLEARCOLOR.clearColor.g = color.ScG;
			mILCMD_TARGET_SETCLEARCOLOR.clearColor.a = color.ScA;
			channel.SendCommand((byte*)(&mILCMD_TARGET_SETCLEARCOLOR), sizeof(MILCMD_TARGET_SETCLEARCOLOR));
		}

		internal unsafe static void SetRenderingMode(ResourceHandle hCompositionTarget, MILRTInitializationFlags nRenderingMode, Channel channel)
		{
			MILCMD_TARGET_SETFLAGS mILCMD_TARGET_SETFLAGS = default(MILCMD_TARGET_SETFLAGS);
			mILCMD_TARGET_SETFLAGS.Type = MILCMD.MilCmdTargetSetFlags;
			mILCMD_TARGET_SETFLAGS.Handle = hCompositionTarget;
			mILCMD_TARGET_SETFLAGS.flags = (uint)nRenderingMode;
			channel.SendCommand((byte*)(&mILCMD_TARGET_SETFLAGS), sizeof(MILCMD_TARGET_SETFLAGS));
		}

		internal unsafe static void SetRoot(ResourceHandle hCompositionTarget, ResourceHandle hRoot, Channel channel)
		{
			MILCMD_TARGET_SETROOT mILCMD_TARGET_SETROOT = default(MILCMD_TARGET_SETROOT);
			mILCMD_TARGET_SETROOT.Type = MILCMD.MilCmdTargetSetRoot;
			mILCMD_TARGET_SETROOT.Handle = hCompositionTarget;
			mILCMD_TARGET_SETROOT.hRoot = hRoot;
			channel.SendCommand((byte*)(&mILCMD_TARGET_SETROOT), sizeof(MILCMD_TARGET_SETROOT));
		}

		internal unsafe static void UpdateWindowSettings(ResourceHandle hCompositionTarget, MS.Win32.NativeMethods.RECT windowRect, Color colorKey, float constantAlpha, MILWindowLayerType windowLayerType, MILTransparencyFlags transparencyMode, bool isChild, bool isRTL, bool renderingEnabled, int disableCookie, Channel channel)
		{
			MILCMD_TARGET_UPDATEWINDOWSETTINGS mILCMD_TARGET_UPDATEWINDOWSETTINGS = default(MILCMD_TARGET_UPDATEWINDOWSETTINGS);
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.Type = MILCMD.MilCmdTargetUpdateWindowSettings;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.Handle = hCompositionTarget;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.renderingEnabled = (renderingEnabled ? 1u : 0u);
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.disableCookie = (uint)disableCookie;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.windowRect = windowRect;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.colorKey.b = colorKey.ScB;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.colorKey.r = colorKey.ScR;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.colorKey.g = colorKey.ScG;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.colorKey.a = colorKey.ScA;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.constantAlpha = constantAlpha;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.transparencyMode = transparencyMode;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.windowLayerType = windowLayerType;
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.isChild = (isChild ? 1u : 0u);
			mILCMD_TARGET_UPDATEWINDOWSETTINGS.isRTL = (isRTL ? 1u : 0u);
			channel.SendCommand((byte*)(&mILCMD_TARGET_UPDATEWINDOWSETTINGS), sizeof(MILCMD_TARGET_UPDATEWINDOWSETTINGS));
		}

		internal unsafe static void Invalidate(ResourceHandle hCompositionTarget, ref MS.Win32.NativeMethods.RECT pRect, Channel channel)
		{
			MILCMD_TARGET_INVALIDATE mILCMD_TARGET_INVALIDATE = default(MILCMD_TARGET_INVALIDATE);
			mILCMD_TARGET_INVALIDATE.Type = MILCMD.MilCmdTargetInvalidate;
			mILCMD_TARGET_INVALIDATE.Handle = hCompositionTarget;
			mILCMD_TARGET_INVALIDATE.rc = pRect;
			channel.SendCommand((byte*)(&mILCMD_TARGET_INVALIDATE), sizeof(MILCMD_TARGET_INVALIDATE), sendInSeparateBatch: false);
			channel.CloseBatch();
			channel.Commit();
		}
	}

	internal static class ETWEvent
	{
		internal unsafe static void RaiseEvent(ResourceHandle hEtwEvent, uint id, Channel channel)
		{
			MILCMD_ETWEVENTRESOURCE mILCMD_ETWEVENTRESOURCE = default(MILCMD_ETWEVENTRESOURCE);
			mILCMD_ETWEVENTRESOURCE.Type = MILCMD.MilCmdEtwEventResource;
			mILCMD_ETWEVENTRESOURCE.Handle = hEtwEvent;
			mILCMD_ETWEVENTRESOURCE.id = id;
			channel.SendCommand((byte*)(&mILCMD_ETWEVENTRESOURCE), sizeof(MILCMD_ETWEVENTRESOURCE));
		}
	}

	internal interface IResource
	{
		ResourceHandle AddRefOnChannel(Channel channel);

		int GetChannelCount();

		Channel GetChannel(int index);

		void ReleaseOnChannel(Channel channel);

		ResourceHandle GetHandle(Channel channel);

		ResourceHandle Get3DHandle(Channel channel);

		void RemoveChildFromParent(IResource parent, Channel channel);
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_PARTITION_REGISTERFORNOTIFICATIONS
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal uint Enable;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_CHANNEL_REQUESTTIER
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal uint ReturnCommonMinimum;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_PARTITION_SETVBLANKSYNCMODE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal uint Enable;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_PARTITION_NOTIFYPRESENT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ulong FrameTime;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_D3DIMAGE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ulong pInteropDeviceBitmap;

		[FieldOffset(16)]
		internal ulong pSoftwareBitmap;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_D3DIMAGE_PRESENT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ulong hEvent;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_BITMAP_INVALIDATE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint UseDirtyRect;

		[FieldOffset(12)]
		internal MS.Win32.NativeMethods.RECT DirtyRect;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DOUBLERESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_COLORRESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_POINTRESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal Point Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_RECTRESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal Rect Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_SIZERESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal Size Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_MATRIXRESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilMatrix3x2D Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_POINT3DRESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilPoint3F Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VECTOR3DRESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilPoint3F Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_QUATERNIONRESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilQuaternionF Value;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_RENDERDATA
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint cbData;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_ETWEVENTRESOURCE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint id;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETOFFSET
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double offsetX;

		[FieldOffset(16)]
		internal double offsetY;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETTRANSFORM
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hTransform;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETEFFECT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hEffect;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETCACHEMODE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hCacheMode;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETCLIP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hClip;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETALPHA
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double alpha;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETRENDEROPTIONS
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilRenderOptions renderOptions;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETCONTENT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hContent;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETALPHAMASK
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hAlphaMask;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_REMOVEALLCHILDREN
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_REMOVECHILD
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hChild;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_INSERTCHILDAT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hChild;

		[FieldOffset(12)]
		internal uint index;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETGUIDELINECOLLECTION
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ushort countX;

		[FieldOffset(12)]
		internal ushort countY;

		[FieldOffset(15)]
		private byte BYTEPacking0;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL_SETSCROLLABLEAREACLIP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal Rect Clip;

		[FieldOffset(40)]
		internal uint IsEnabled;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VIEWPORT3DVISUAL_SETCAMERA
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hCamera;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VIEWPORT3DVISUAL_SETVIEWPORT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal Rect Viewport;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VIEWPORT3DVISUAL_SET3DCHILD
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hChild;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL3D_SETCONTENT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hContent;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL3D_SETTRANSFORM
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hTransform;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL3D_REMOVEALLCHILDREN
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL3D_REMOVECHILD
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hChild;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUAL3D_INSERTCHILDAT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hChild;

		[FieldOffset(12)]
		internal uint index;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_HWNDTARGET_CREATE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ulong hwnd;

		[FieldOffset(16)]
		internal ulong hSection;

		[FieldOffset(24)]
		internal ulong masterDevice;

		[FieldOffset(32)]
		internal uint width;

		[FieldOffset(36)]
		internal uint height;

		[FieldOffset(40)]
		internal MilColorF clearColor;

		[FieldOffset(56)]
		internal uint flags;

		[FieldOffset(60)]
		internal ResourceHandle hBitmap;

		[FieldOffset(64)]
		internal uint stride;

		[FieldOffset(68)]
		internal uint ePixelFormat;

		[FieldOffset(72)]
		internal int DpiAwarenessContext;

		[FieldOffset(76)]
		internal double DpiX;

		[FieldOffset(84)]
		internal double DpiY;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_HWNDTARGET_SUPPRESSLAYERED
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint Suppress;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TARGET_UPDATEWINDOWSETTINGS
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MS.Win32.NativeMethods.RECT windowRect;

		[FieldOffset(24)]
		internal MILWindowLayerType windowLayerType;

		[FieldOffset(28)]
		internal MILTransparencyFlags transparencyMode;

		[FieldOffset(32)]
		internal float constantAlpha;

		[FieldOffset(36)]
		internal uint isChild;

		[FieldOffset(40)]
		internal uint isRTL;

		[FieldOffset(44)]
		internal uint renderingEnabled;

		[FieldOffset(48)]
		internal MilColorF colorKey;

		[FieldOffset(64)]
		internal uint disableCookie;

		[FieldOffset(68)]
		internal uint gdiBlt;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_GENERICTARGET_CREATE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ulong hwnd;

		[FieldOffset(16)]
		internal ulong pRenderTarget;

		[FieldOffset(24)]
		internal uint width;

		[FieldOffset(28)]
		internal uint height;

		[FieldOffset(32)]
		internal uint dummy;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TARGET_SETROOT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hRoot;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TARGET_SETCLEARCOLOR
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF clearColor;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TARGET_INVALIDATE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MS.Win32.NativeMethods.RECT rc;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TARGET_SETFLAGS
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint flags;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_HWNDTARGET_DPICHANGED
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double DpiX;

		[FieldOffset(16)]
		internal double DpiY;

		[FieldOffset(24)]
		internal uint AfterParent;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_GLYPHRUN_CREATE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ulong pIDWriteFont;

		[FieldOffset(16)]
		internal ushort GlyphRunFlags;

		[FieldOffset(20)]
		internal MilPoint2F Origin;

		[FieldOffset(28)]
		internal float MuSize;

		[FieldOffset(32)]
		internal Rect ManagedBounds;

		[FieldOffset(64)]
		internal ushort GlyphCount;

		[FieldOffset(68)]
		internal ushort BidiLevel;

		[FieldOffset(72)]
		internal ushort DWriteTextMeasuringMethod;

		[FieldOffset(75)]
		private byte BYTEPacking0;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DOUBLEBUFFEREDBITMAP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ulong SwDoubleBufferedBitmap;

		[FieldOffset(16)]
		internal uint UseBackBuffer;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ulong CopyCompletedEvent;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal uint ShouldRenderEvenWhenNoDisplayDevicesAreAvailable;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_AXISANGLEROTATION3D
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double angle;

		[FieldOffset(16)]
		internal MilPoint3F axis;

		[FieldOffset(28)]
		internal ResourceHandle hAxisAnimations;

		[FieldOffset(32)]
		internal ResourceHandle hAngleAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_QUATERNIONROTATION3D
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilQuaternionF quaternion;

		[FieldOffset(24)]
		internal ResourceHandle hQuaternionAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_PERSPECTIVECAMERA
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double nearPlaneDistance;

		[FieldOffset(16)]
		internal double farPlaneDistance;

		[FieldOffset(24)]
		internal double fieldOfView;

		[FieldOffset(32)]
		internal MilPoint3F position;

		[FieldOffset(44)]
		internal ResourceHandle htransform;

		[FieldOffset(48)]
		internal MilPoint3F lookDirection;

		[FieldOffset(60)]
		internal ResourceHandle hNearPlaneDistanceAnimations;

		[FieldOffset(64)]
		internal MilPoint3F upDirection;

		[FieldOffset(76)]
		internal ResourceHandle hFarPlaneDistanceAnimations;

		[FieldOffset(80)]
		internal ResourceHandle hPositionAnimations;

		[FieldOffset(84)]
		internal ResourceHandle hLookDirectionAnimations;

		[FieldOffset(88)]
		internal ResourceHandle hUpDirectionAnimations;

		[FieldOffset(92)]
		internal ResourceHandle hFieldOfViewAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_ORTHOGRAPHICCAMERA
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double nearPlaneDistance;

		[FieldOffset(16)]
		internal double farPlaneDistance;

		[FieldOffset(24)]
		internal double width;

		[FieldOffset(32)]
		internal MilPoint3F position;

		[FieldOffset(44)]
		internal ResourceHandle htransform;

		[FieldOffset(48)]
		internal MilPoint3F lookDirection;

		[FieldOffset(60)]
		internal ResourceHandle hNearPlaneDistanceAnimations;

		[FieldOffset(64)]
		internal MilPoint3F upDirection;

		[FieldOffset(76)]
		internal ResourceHandle hFarPlaneDistanceAnimations;

		[FieldOffset(80)]
		internal ResourceHandle hPositionAnimations;

		[FieldOffset(84)]
		internal ResourceHandle hLookDirectionAnimations;

		[FieldOffset(88)]
		internal ResourceHandle hUpDirectionAnimations;

		[FieldOffset(92)]
		internal ResourceHandle hWidthAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_MATRIXCAMERA
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal D3DMATRIX viewMatrix;

		[FieldOffset(72)]
		internal D3DMATRIX projectionMatrix;

		[FieldOffset(136)]
		internal ResourceHandle htransform;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_MODEL3DGROUP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle htransform;

		[FieldOffset(12)]
		internal uint ChildrenSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_AMBIENTLIGHT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF color;

		[FieldOffset(24)]
		internal ResourceHandle htransform;

		[FieldOffset(28)]
		internal ResourceHandle hColorAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DIRECTIONALLIGHT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF color;

		[FieldOffset(24)]
		internal MilPoint3F direction;

		[FieldOffset(36)]
		internal ResourceHandle htransform;

		[FieldOffset(40)]
		internal ResourceHandle hColorAnimations;

		[FieldOffset(44)]
		internal ResourceHandle hDirectionAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_POINTLIGHT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF color;

		[FieldOffset(24)]
		internal double range;

		[FieldOffset(32)]
		internal double constantAttenuation;

		[FieldOffset(40)]
		internal double linearAttenuation;

		[FieldOffset(48)]
		internal double quadraticAttenuation;

		[FieldOffset(56)]
		internal MilPoint3F position;

		[FieldOffset(68)]
		internal ResourceHandle htransform;

		[FieldOffset(72)]
		internal ResourceHandle hColorAnimations;

		[FieldOffset(76)]
		internal ResourceHandle hPositionAnimations;

		[FieldOffset(80)]
		internal ResourceHandle hRangeAnimations;

		[FieldOffset(84)]
		internal ResourceHandle hConstantAttenuationAnimations;

		[FieldOffset(88)]
		internal ResourceHandle hLinearAttenuationAnimations;

		[FieldOffset(92)]
		internal ResourceHandle hQuadraticAttenuationAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_SPOTLIGHT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF color;

		[FieldOffset(24)]
		internal double range;

		[FieldOffset(32)]
		internal double constantAttenuation;

		[FieldOffset(40)]
		internal double linearAttenuation;

		[FieldOffset(48)]
		internal double quadraticAttenuation;

		[FieldOffset(56)]
		internal double outerConeAngle;

		[FieldOffset(64)]
		internal double innerConeAngle;

		[FieldOffset(72)]
		internal MilPoint3F position;

		[FieldOffset(84)]
		internal ResourceHandle htransform;

		[FieldOffset(88)]
		internal MilPoint3F direction;

		[FieldOffset(100)]
		internal ResourceHandle hColorAnimations;

		[FieldOffset(104)]
		internal ResourceHandle hPositionAnimations;

		[FieldOffset(108)]
		internal ResourceHandle hRangeAnimations;

		[FieldOffset(112)]
		internal ResourceHandle hConstantAttenuationAnimations;

		[FieldOffset(116)]
		internal ResourceHandle hLinearAttenuationAnimations;

		[FieldOffset(120)]
		internal ResourceHandle hQuadraticAttenuationAnimations;

		[FieldOffset(124)]
		internal ResourceHandle hDirectionAnimations;

		[FieldOffset(128)]
		internal ResourceHandle hOuterConeAngleAnimations;

		[FieldOffset(132)]
		internal ResourceHandle hInnerConeAngleAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_GEOMETRYMODEL3D
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle htransform;

		[FieldOffset(12)]
		internal ResourceHandle hgeometry;

		[FieldOffset(16)]
		internal ResourceHandle hmaterial;

		[FieldOffset(20)]
		internal ResourceHandle hbackMaterial;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_MESHGEOMETRY3D
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint PositionsSize;

		[FieldOffset(12)]
		internal uint NormalsSize;

		[FieldOffset(16)]
		internal uint TextureCoordinatesSize;

		[FieldOffset(20)]
		internal uint TriangleIndicesSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_MATERIALGROUP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint ChildrenSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DIFFUSEMATERIAL
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF color;

		[FieldOffset(24)]
		internal MilColorF ambientColor;

		[FieldOffset(40)]
		internal ResourceHandle hbrush;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_SPECULARMATERIAL
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF color;

		[FieldOffset(24)]
		internal double specularPower;

		[FieldOffset(32)]
		internal ResourceHandle hbrush;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_EMISSIVEMATERIAL
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilColorF color;

		[FieldOffset(24)]
		internal ResourceHandle hbrush;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TRANSFORM3DGROUP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint ChildrenSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TRANSLATETRANSFORM3D
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double offsetX;

		[FieldOffset(16)]
		internal double offsetY;

		[FieldOffset(24)]
		internal double offsetZ;

		[FieldOffset(32)]
		internal ResourceHandle hOffsetXAnimations;

		[FieldOffset(36)]
		internal ResourceHandle hOffsetYAnimations;

		[FieldOffset(40)]
		internal ResourceHandle hOffsetZAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_SCALETRANSFORM3D
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double scaleX;

		[FieldOffset(16)]
		internal double scaleY;

		[FieldOffset(24)]
		internal double scaleZ;

		[FieldOffset(32)]
		internal double centerX;

		[FieldOffset(40)]
		internal double centerY;

		[FieldOffset(48)]
		internal double centerZ;

		[FieldOffset(56)]
		internal ResourceHandle hScaleXAnimations;

		[FieldOffset(60)]
		internal ResourceHandle hScaleYAnimations;

		[FieldOffset(64)]
		internal ResourceHandle hScaleZAnimations;

		[FieldOffset(68)]
		internal ResourceHandle hCenterXAnimations;

		[FieldOffset(72)]
		internal ResourceHandle hCenterYAnimations;

		[FieldOffset(76)]
		internal ResourceHandle hCenterZAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_ROTATETRANSFORM3D
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double centerX;

		[FieldOffset(16)]
		internal double centerY;

		[FieldOffset(24)]
		internal double centerZ;

		[FieldOffset(32)]
		internal ResourceHandle hCenterXAnimations;

		[FieldOffset(36)]
		internal ResourceHandle hCenterYAnimations;

		[FieldOffset(40)]
		internal ResourceHandle hCenterZAnimations;

		[FieldOffset(44)]
		internal ResourceHandle hrotation;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_MATRIXTRANSFORM3D
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal D3DMATRIX matrix;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_PIXELSHADER
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ShaderRenderMode ShaderRenderMode;

		[FieldOffset(12)]
		internal uint PixelShaderBytecodeSize;

		[FieldOffset(16)]
		internal uint CompileSoftwareShader;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_IMPLICITINPUTBRUSH
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(20)]
		internal ResourceHandle hTransform;

		[FieldOffset(24)]
		internal ResourceHandle hRelativeTransform;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_BLUREFFECT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Radius;

		[FieldOffset(16)]
		internal ResourceHandle hRadiusAnimations;

		[FieldOffset(20)]
		internal KernelType KernelType;

		[FieldOffset(24)]
		internal RenderingBias RenderingBias;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DROPSHADOWEFFECT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double ShadowDepth;

		[FieldOffset(16)]
		internal MilColorF Color;

		[FieldOffset(32)]
		internal double Direction;

		[FieldOffset(40)]
		internal double Opacity;

		[FieldOffset(48)]
		internal double BlurRadius;

		[FieldOffset(56)]
		internal ResourceHandle hShadowDepthAnimations;

		[FieldOffset(60)]
		internal ResourceHandle hColorAnimations;

		[FieldOffset(64)]
		internal ResourceHandle hDirectionAnimations;

		[FieldOffset(68)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(72)]
		internal ResourceHandle hBlurRadiusAnimations;

		[FieldOffset(76)]
		internal RenderingBias RenderingBias;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_SHADEREFFECT
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double TopPadding;

		[FieldOffset(16)]
		internal double BottomPadding;

		[FieldOffset(24)]
		internal double LeftPadding;

		[FieldOffset(32)]
		internal double RightPadding;

		[FieldOffset(40)]
		internal ResourceHandle hPixelShader;

		[FieldOffset(44)]
		internal int DdxUvDdyUvRegisterIndex;

		[FieldOffset(48)]
		internal uint ShaderConstantFloatRegistersSize;

		[FieldOffset(52)]
		internal uint DependencyPropertyFloatValuesSize;

		[FieldOffset(56)]
		internal uint ShaderConstantIntRegistersSize;

		[FieldOffset(60)]
		internal uint DependencyPropertyIntValuesSize;

		[FieldOffset(64)]
		internal uint ShaderConstantBoolRegistersSize;

		[FieldOffset(68)]
		internal uint DependencyPropertyBoolValuesSize;

		[FieldOffset(72)]
		internal uint ShaderSamplerRegistrationInfoSize;

		[FieldOffset(76)]
		internal uint DependencyPropertySamplerValuesSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DRAWINGIMAGE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hDrawing;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TRANSFORMGROUP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint ChildrenSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_TRANSLATETRANSFORM
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double X;

		[FieldOffset(16)]
		internal double Y;

		[FieldOffset(24)]
		internal ResourceHandle hXAnimations;

		[FieldOffset(28)]
		internal ResourceHandle hYAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_SCALETRANSFORM
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double ScaleX;

		[FieldOffset(16)]
		internal double ScaleY;

		[FieldOffset(24)]
		internal double CenterX;

		[FieldOffset(32)]
		internal double CenterY;

		[FieldOffset(40)]
		internal ResourceHandle hScaleXAnimations;

		[FieldOffset(44)]
		internal ResourceHandle hScaleYAnimations;

		[FieldOffset(48)]
		internal ResourceHandle hCenterXAnimations;

		[FieldOffset(52)]
		internal ResourceHandle hCenterYAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_SKEWTRANSFORM
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double AngleX;

		[FieldOffset(16)]
		internal double AngleY;

		[FieldOffset(24)]
		internal double CenterX;

		[FieldOffset(32)]
		internal double CenterY;

		[FieldOffset(40)]
		internal ResourceHandle hAngleXAnimations;

		[FieldOffset(44)]
		internal ResourceHandle hAngleYAnimations;

		[FieldOffset(48)]
		internal ResourceHandle hCenterXAnimations;

		[FieldOffset(52)]
		internal ResourceHandle hCenterYAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_ROTATETRANSFORM
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Angle;

		[FieldOffset(16)]
		internal double CenterX;

		[FieldOffset(24)]
		internal double CenterY;

		[FieldOffset(32)]
		internal ResourceHandle hAngleAnimations;

		[FieldOffset(36)]
		internal ResourceHandle hCenterXAnimations;

		[FieldOffset(40)]
		internal ResourceHandle hCenterYAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_MATRIXTRANSFORM
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal MilMatrix3x2D Matrix;

		[FieldOffset(56)]
		internal ResourceHandle hMatrixAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_LINEGEOMETRY
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal Point StartPoint;

		[FieldOffset(24)]
		internal Point EndPoint;

		[FieldOffset(40)]
		internal ResourceHandle hTransform;

		[FieldOffset(44)]
		internal ResourceHandle hStartPointAnimations;

		[FieldOffset(48)]
		internal ResourceHandle hEndPointAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_RECTANGLEGEOMETRY
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double RadiusX;

		[FieldOffset(16)]
		internal double RadiusY;

		[FieldOffset(24)]
		internal Rect Rect;

		[FieldOffset(56)]
		internal ResourceHandle hTransform;

		[FieldOffset(60)]
		internal ResourceHandle hRadiusXAnimations;

		[FieldOffset(64)]
		internal ResourceHandle hRadiusYAnimations;

		[FieldOffset(68)]
		internal ResourceHandle hRectAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_ELLIPSEGEOMETRY
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double RadiusX;

		[FieldOffset(16)]
		internal double RadiusY;

		[FieldOffset(24)]
		internal Point Center;

		[FieldOffset(40)]
		internal ResourceHandle hTransform;

		[FieldOffset(44)]
		internal ResourceHandle hRadiusXAnimations;

		[FieldOffset(48)]
		internal ResourceHandle hRadiusYAnimations;

		[FieldOffset(52)]
		internal ResourceHandle hCenterAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_GEOMETRYGROUP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hTransform;

		[FieldOffset(12)]
		internal FillRule FillRule;

		[FieldOffset(16)]
		internal uint ChildrenSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_COMBINEDGEOMETRY
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hTransform;

		[FieldOffset(12)]
		internal GeometryCombineMode GeometryCombineMode;

		[FieldOffset(16)]
		internal ResourceHandle hGeometry1;

		[FieldOffset(20)]
		internal ResourceHandle hGeometry2;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_PATHGEOMETRY
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hTransform;

		[FieldOffset(12)]
		internal FillRule FillRule;

		[FieldOffset(16)]
		internal uint FiguresSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_SOLIDCOLORBRUSH
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal MilColorF Color;

		[FieldOffset(32)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(36)]
		internal ResourceHandle hTransform;

		[FieldOffset(40)]
		internal ResourceHandle hRelativeTransform;

		[FieldOffset(44)]
		internal ResourceHandle hColorAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_LINEARGRADIENTBRUSH
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal Point StartPoint;

		[FieldOffset(32)]
		internal Point EndPoint;

		[FieldOffset(48)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(52)]
		internal ResourceHandle hTransform;

		[FieldOffset(56)]
		internal ResourceHandle hRelativeTransform;

		[FieldOffset(60)]
		internal ColorInterpolationMode ColorInterpolationMode;

		[FieldOffset(64)]
		internal BrushMappingMode MappingMode;

		[FieldOffset(68)]
		internal GradientSpreadMethod SpreadMethod;

		[FieldOffset(72)]
		internal uint GradientStopsSize;

		[FieldOffset(76)]
		internal ResourceHandle hStartPointAnimations;

		[FieldOffset(80)]
		internal ResourceHandle hEndPointAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_RADIALGRADIENTBRUSH
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal Point Center;

		[FieldOffset(32)]
		internal double RadiusX;

		[FieldOffset(40)]
		internal double RadiusY;

		[FieldOffset(48)]
		internal Point GradientOrigin;

		[FieldOffset(64)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(68)]
		internal ResourceHandle hTransform;

		[FieldOffset(72)]
		internal ResourceHandle hRelativeTransform;

		[FieldOffset(76)]
		internal ColorInterpolationMode ColorInterpolationMode;

		[FieldOffset(80)]
		internal BrushMappingMode MappingMode;

		[FieldOffset(84)]
		internal GradientSpreadMethod SpreadMethod;

		[FieldOffset(88)]
		internal uint GradientStopsSize;

		[FieldOffset(92)]
		internal ResourceHandle hCenterAnimations;

		[FieldOffset(96)]
		internal ResourceHandle hRadiusXAnimations;

		[FieldOffset(100)]
		internal ResourceHandle hRadiusYAnimations;

		[FieldOffset(104)]
		internal ResourceHandle hGradientOriginAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_IMAGEBRUSH
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal Rect Viewport;

		[FieldOffset(48)]
		internal Rect Viewbox;

		[FieldOffset(80)]
		internal double CacheInvalidationThresholdMinimum;

		[FieldOffset(88)]
		internal double CacheInvalidationThresholdMaximum;

		[FieldOffset(96)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(100)]
		internal ResourceHandle hTransform;

		[FieldOffset(104)]
		internal ResourceHandle hRelativeTransform;

		[FieldOffset(108)]
		internal BrushMappingMode ViewportUnits;

		[FieldOffset(112)]
		internal BrushMappingMode ViewboxUnits;

		[FieldOffset(116)]
		internal ResourceHandle hViewportAnimations;

		[FieldOffset(120)]
		internal ResourceHandle hViewboxAnimations;

		[FieldOffset(124)]
		internal Stretch Stretch;

		[FieldOffset(128)]
		internal TileMode TileMode;

		[FieldOffset(132)]
		internal AlignmentX AlignmentX;

		[FieldOffset(136)]
		internal AlignmentY AlignmentY;

		[FieldOffset(140)]
		internal CachingHint CachingHint;

		[FieldOffset(144)]
		internal ResourceHandle hImageSource;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DRAWINGBRUSH
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal Rect Viewport;

		[FieldOffset(48)]
		internal Rect Viewbox;

		[FieldOffset(80)]
		internal double CacheInvalidationThresholdMinimum;

		[FieldOffset(88)]
		internal double CacheInvalidationThresholdMaximum;

		[FieldOffset(96)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(100)]
		internal ResourceHandle hTransform;

		[FieldOffset(104)]
		internal ResourceHandle hRelativeTransform;

		[FieldOffset(108)]
		internal BrushMappingMode ViewportUnits;

		[FieldOffset(112)]
		internal BrushMappingMode ViewboxUnits;

		[FieldOffset(116)]
		internal ResourceHandle hViewportAnimations;

		[FieldOffset(120)]
		internal ResourceHandle hViewboxAnimations;

		[FieldOffset(124)]
		internal Stretch Stretch;

		[FieldOffset(128)]
		internal TileMode TileMode;

		[FieldOffset(132)]
		internal AlignmentX AlignmentX;

		[FieldOffset(136)]
		internal AlignmentY AlignmentY;

		[FieldOffset(140)]
		internal CachingHint CachingHint;

		[FieldOffset(144)]
		internal ResourceHandle hDrawing;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VISUALBRUSH
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal Rect Viewport;

		[FieldOffset(48)]
		internal Rect Viewbox;

		[FieldOffset(80)]
		internal double CacheInvalidationThresholdMinimum;

		[FieldOffset(88)]
		internal double CacheInvalidationThresholdMaximum;

		[FieldOffset(96)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(100)]
		internal ResourceHandle hTransform;

		[FieldOffset(104)]
		internal ResourceHandle hRelativeTransform;

		[FieldOffset(108)]
		internal BrushMappingMode ViewportUnits;

		[FieldOffset(112)]
		internal BrushMappingMode ViewboxUnits;

		[FieldOffset(116)]
		internal ResourceHandle hViewportAnimations;

		[FieldOffset(120)]
		internal ResourceHandle hViewboxAnimations;

		[FieldOffset(124)]
		internal Stretch Stretch;

		[FieldOffset(128)]
		internal TileMode TileMode;

		[FieldOffset(132)]
		internal AlignmentX AlignmentX;

		[FieldOffset(136)]
		internal AlignmentY AlignmentY;

		[FieldOffset(140)]
		internal CachingHint CachingHint;

		[FieldOffset(144)]
		internal ResourceHandle hVisual;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_BITMAPCACHEBRUSH
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(20)]
		internal ResourceHandle hTransform;

		[FieldOffset(24)]
		internal ResourceHandle hRelativeTransform;

		[FieldOffset(28)]
		internal ResourceHandle hBitmapCache;

		[FieldOffset(32)]
		internal ResourceHandle hInternalTarget;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DASHSTYLE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Offset;

		[FieldOffset(16)]
		internal ResourceHandle hOffsetAnimations;

		[FieldOffset(20)]
		internal uint DashesSize;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_PEN
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Thickness;

		[FieldOffset(16)]
		internal double MiterLimit;

		[FieldOffset(24)]
		internal ResourceHandle hBrush;

		[FieldOffset(28)]
		internal ResourceHandle hThicknessAnimations;

		[FieldOffset(32)]
		internal PenLineCap StartLineCap;

		[FieldOffset(36)]
		internal PenLineCap EndLineCap;

		[FieldOffset(40)]
		internal PenLineCap DashCap;

		[FieldOffset(44)]
		internal PenLineJoin LineJoin;

		[FieldOffset(48)]
		internal ResourceHandle hDashStyle;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_GEOMETRYDRAWING
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hBrush;

		[FieldOffset(12)]
		internal ResourceHandle hPen;

		[FieldOffset(16)]
		internal ResourceHandle hGeometry;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_GLYPHRUNDRAWING
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal ResourceHandle hGlyphRun;

		[FieldOffset(12)]
		internal ResourceHandle hForegroundBrush;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_IMAGEDRAWING
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal Rect Rect;

		[FieldOffset(40)]
		internal ResourceHandle hImageSource;

		[FieldOffset(44)]
		internal ResourceHandle hRectAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_VIDEODRAWING
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal Rect Rect;

		[FieldOffset(40)]
		internal ResourceHandle hPlayer;

		[FieldOffset(44)]
		internal ResourceHandle hRectAnimations;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_DRAWINGGROUP
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double Opacity;

		[FieldOffset(16)]
		internal uint ChildrenSize;

		[FieldOffset(20)]
		internal ResourceHandle hClipGeometry;

		[FieldOffset(24)]
		internal ResourceHandle hOpacityAnimations;

		[FieldOffset(28)]
		internal ResourceHandle hOpacityMask;

		[FieldOffset(32)]
		internal ResourceHandle hTransform;

		[FieldOffset(36)]
		internal ResourceHandle hGuidelineSet;

		[FieldOffset(40)]
		internal EdgeMode EdgeMode;

		[FieldOffset(44)]
		internal BitmapScalingMode bitmapScalingMode;

		[FieldOffset(48)]
		internal ClearTypeHint ClearTypeHint;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_GUIDELINESET
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal uint GuidelinesXSize;

		[FieldOffset(12)]
		internal uint GuidelinesYSize;

		[FieldOffset(16)]
		internal uint IsDynamic;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MILCMD_BITMAPCACHE
	{
		[FieldOffset(0)]
		internal MILCMD Type;

		[FieldOffset(4)]
		internal ResourceHandle Handle;

		[FieldOffset(8)]
		internal double RenderAtScale;

		[FieldOffset(16)]
		internal ResourceHandle hRenderAtScaleAnimations;

		[FieldOffset(20)]
		internal uint SnapsToDevicePixels;

		[FieldOffset(24)]
		internal uint EnableClearType;
	}

	internal enum ResourceType
	{
		TYPE_NULL,
		TYPE_MEDIAPLAYER,
		TYPE_ROTATION3D,
		TYPE_AXISANGLEROTATION3D,
		TYPE_QUATERNIONROTATION3D,
		TYPE_CAMERA,
		TYPE_PROJECTIONCAMERA,
		TYPE_PERSPECTIVECAMERA,
		TYPE_ORTHOGRAPHICCAMERA,
		TYPE_MATRIXCAMERA,
		TYPE_MODEL3D,
		TYPE_MODEL3DGROUP,
		TYPE_LIGHT,
		TYPE_AMBIENTLIGHT,
		TYPE_DIRECTIONALLIGHT,
		TYPE_POINTLIGHTBASE,
		TYPE_POINTLIGHT,
		TYPE_SPOTLIGHT,
		TYPE_GEOMETRYMODEL3D,
		TYPE_GEOMETRY3D,
		TYPE_MESHGEOMETRY3D,
		TYPE_MATERIAL,
		TYPE_MATERIALGROUP,
		TYPE_DIFFUSEMATERIAL,
		TYPE_SPECULARMATERIAL,
		TYPE_EMISSIVEMATERIAL,
		TYPE_TRANSFORM3D,
		TYPE_TRANSFORM3DGROUP,
		TYPE_AFFINETRANSFORM3D,
		TYPE_TRANSLATETRANSFORM3D,
		TYPE_SCALETRANSFORM3D,
		TYPE_ROTATETRANSFORM3D,
		TYPE_MATRIXTRANSFORM3D,
		TYPE_PIXELSHADER,
		TYPE_IMPLICITINPUTBRUSH,
		TYPE_EFFECT,
		TYPE_BLUREFFECT,
		TYPE_DROPSHADOWEFFECT,
		TYPE_SHADEREFFECT,
		TYPE_VISUAL,
		TYPE_VIEWPORT3DVISUAL,
		TYPE_VISUAL3D,
		TYPE_GLYPHRUN,
		TYPE_RENDERDATA,
		TYPE_DRAWINGCONTEXT,
		TYPE_RENDERTARGET,
		TYPE_HWNDRENDERTARGET,
		TYPE_GENERICRENDERTARGET,
		TYPE_ETWEVENTRESOURCE,
		TYPE_DOUBLERESOURCE,
		TYPE_COLORRESOURCE,
		TYPE_POINTRESOURCE,
		TYPE_RECTRESOURCE,
		TYPE_SIZERESOURCE,
		TYPE_MATRIXRESOURCE,
		TYPE_POINT3DRESOURCE,
		TYPE_VECTOR3DRESOURCE,
		TYPE_QUATERNIONRESOURCE,
		TYPE_IMAGESOURCE,
		TYPE_DRAWINGIMAGE,
		TYPE_TRANSFORM,
		TYPE_TRANSFORMGROUP,
		TYPE_TRANSLATETRANSFORM,
		TYPE_SCALETRANSFORM,
		TYPE_SKEWTRANSFORM,
		TYPE_ROTATETRANSFORM,
		TYPE_MATRIXTRANSFORM,
		TYPE_GEOMETRY,
		TYPE_LINEGEOMETRY,
		TYPE_RECTANGLEGEOMETRY,
		TYPE_ELLIPSEGEOMETRY,
		TYPE_GEOMETRYGROUP,
		TYPE_COMBINEDGEOMETRY,
		TYPE_PATHGEOMETRY,
		TYPE_BRUSH,
		TYPE_SOLIDCOLORBRUSH,
		TYPE_GRADIENTBRUSH,
		TYPE_LINEARGRADIENTBRUSH,
		TYPE_RADIALGRADIENTBRUSH,
		TYPE_TILEBRUSH,
		TYPE_IMAGEBRUSH,
		TYPE_DRAWINGBRUSH,
		TYPE_VISUALBRUSH,
		TYPE_BITMAPCACHEBRUSH,
		TYPE_DASHSTYLE,
		TYPE_PEN,
		TYPE_DRAWING,
		TYPE_GEOMETRYDRAWING,
		TYPE_GLYPHRUNDRAWING,
		TYPE_IMAGEDRAWING,
		TYPE_VIDEODRAWING,
		TYPE_DRAWINGGROUP,
		TYPE_GUIDELINESET,
		TYPE_CACHEMODE,
		TYPE_BITMAPCACHE,
		TYPE_BITMAPSOURCE,
		TYPE_DOUBLEBUFFEREDBITMAP,
		TYPE_D3DIMAGE
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MIL_GRADIENTSTOP
	{
		[FieldOffset(0)]
		internal double Position;

		[FieldOffset(8)]
		internal MilColorF Color;
	}

	internal const uint waitInfinite = uint.MaxValue;

	internal unsafe static void CopyBytes(byte* pbTo, byte* pbFrom, int cbData)
	{
		for (int i = 0; i < cbData / 4; i++)
		{
			*(int*)(pbTo + (nint)i * (nint)4) = *(int*)(pbFrom + (nint)i * (nint)4);
		}
	}

	internal unsafe static void NotifyPolicyChangeForNonInteractiveMode(bool forceRender, Channel channel)
	{
		MILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE mILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE = default(MILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE);
		mILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE.Type = MILCMD.MilCmdPartitionNotifyPolicyChangeForNonInteractiveMode;
		mILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable = (forceRender ? 1u : 0u);
		MILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE mILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE2 = mILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE;
		channel.SendCommand((byte*)(&mILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE2), sizeof(MILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE), sendInSeparateBatch: false);
	}
}
