namespace System.Windows.Media.Composition;

internal struct VisualProxy
{
	private struct Proxy
	{
		internal DUCE.Channel Channel;

		internal VisualProxyFlags Flags;

		internal DUCE.ResourceHandle Handle;
	}

	private const int PROXY_NOT_FOUND = -2;

	private const int PROXY_STORED_INLINE = -1;

	private Proxy _head;

	private Proxy[] _tail;

	internal int Count
	{
		get
		{
			if (_tail == null)
			{
				return (_head.Channel != null) ? 1 : 0;
			}
			int num = _tail.Length;
			bool flag = _tail[num - 1].Channel == null;
			return 1 + num - (flag ? 1 : 0);
		}
	}

	internal bool IsOnAnyChannel => Count != 0;

	internal bool IsOnChannel(DUCE.Channel channel)
	{
		int num = Find(channel);
		return num switch
		{
			-2 => false, 
			-1 => !_head.Handle.IsNull, 
			_ => !_tail[num].Handle.IsNull, 
		};
	}

	internal bool CreateOrAddRefOnChannel(object instance, DUCE.Channel channel, DUCE.ResourceType resourceType)
	{
		int num = Find(channel);
		int count = Count;
		switch (num)
		{
		case -2:
			if (_head.Channel == null)
			{
				_head.Channel = channel;
				_head.Flags = VisualProxyFlags.None;
				channel.CreateOrAddRefOnChannel(instance, ref _head.Handle, resourceType);
			}
			else
			{
				if (_tail == null)
				{
					_tail = new Proxy[2];
				}
				else if (count > _tail.Length)
				{
					ResizeTail(2);
				}
				Proxy proxy = default(Proxy);
				proxy.Channel = channel;
				proxy.Flags = VisualProxyFlags.None;
				proxy.Handle = DUCE.ResourceHandle.Null;
				channel.CreateOrAddRefOnChannel(instance, ref proxy.Handle, resourceType);
				_tail[count - 1] = proxy;
			}
			return true;
		case -1:
			channel.CreateOrAddRefOnChannel(instance, ref _head.Handle, resourceType);
			break;
		default:
			channel.CreateOrAddRefOnChannel(instance, ref _tail[num].Handle, resourceType);
			break;
		}
		return false;
	}

	internal bool ReleaseOnChannel(DUCE.Channel channel)
	{
		int num = Find(channel);
		bool flag = false;
		int count = Count;
		if (num == -1)
		{
			if (channel.ReleaseOnChannel(_head.Handle))
			{
				if (count == 1)
				{
					_head = default(Proxy);
				}
				else
				{
					_head = _tail[count - 2];
				}
				flag = true;
			}
		}
		else
		{
			if (num < 0)
			{
				return false;
			}
			if (channel.ReleaseOnChannel(_tail[num].Handle))
			{
				if (num != count - 2)
				{
					_tail[num] = _tail[count - 2];
				}
				flag = true;
			}
		}
		if (flag && _tail != null)
		{
			if (count == 2)
			{
				_tail = null;
			}
			else if (count == _tail.Length)
			{
				ResizeTail(-2);
			}
			else
			{
				_tail[count - 2] = default(Proxy);
			}
		}
		return flag;
	}

	internal DUCE.Channel GetChannel(int index)
	{
		if (index < Count)
		{
			if (index == 0)
			{
				return _head.Channel;
			}
			if (index > 0)
			{
				return _tail[index - 1].Channel;
			}
		}
		return null;
	}

	internal DUCE.ResourceHandle GetHandle(DUCE.Channel channel)
	{
		return GetHandle(Find(channel) + 1);
	}

	internal DUCE.ResourceHandle GetHandle(int index)
	{
		if (index < Count)
		{
			if (index == 0)
			{
				return _head.Handle;
			}
			if (index > 0)
			{
				return _tail[index - 1].Handle;
			}
		}
		return DUCE.ResourceHandle.Null;
	}

	internal VisualProxyFlags GetFlags(DUCE.Channel channel)
	{
		return GetFlags(Find(channel) + 1);
	}

	internal VisualProxyFlags GetFlags(int index)
	{
		if (index < Count)
		{
			if (index == 0)
			{
				return _head.Flags;
			}
			if (index > 0)
			{
				return _tail[index - 1].Flags;
			}
		}
		return VisualProxyFlags.None;
	}

	internal void SetFlags(DUCE.Channel channel, bool value, VisualProxyFlags flags)
	{
		SetFlags(Find(channel) + 1, value, flags);
	}

	internal void SetFlags(int index, bool value, VisualProxyFlags flags)
	{
		if (index < Count)
		{
			if (index == 0)
			{
				_head.Flags = (value ? (_head.Flags | flags) : (_head.Flags & ~flags));
			}
			else if (index > 0)
			{
				_tail[index - 1].Flags = (value ? (_tail[index - 1].Flags | flags) : (_tail[index - 1].Flags & ~flags));
			}
		}
	}

	internal void SetFlagsOnAllChannels(bool value, VisualProxyFlags flags)
	{
		if (_head.Channel != null)
		{
			_head.Flags = (value ? (_head.Flags | flags) : (_head.Flags & ~flags));
			int i = 0;
			for (int num = Count - 1; i < num; i++)
			{
				_tail[i].Flags = (value ? (_tail[i].Flags | flags) : (_tail[i].Flags & ~flags));
			}
		}
	}

	internal bool CheckFlagsOnAllChannels(VisualProxyFlags conjunctionFlags)
	{
		if (_head.Channel != null)
		{
			if ((_head.Flags & conjunctionFlags) != conjunctionFlags)
			{
				return false;
			}
			int i = 0;
			for (int num = Count - 1; i < num; i++)
			{
				if ((_tail[i].Flags & conjunctionFlags) != conjunctionFlags)
				{
					return false;
				}
			}
		}
		return true;
	}

	private int Find(DUCE.Channel channel)
	{
		if (_head.Channel == channel)
		{
			return -1;
		}
		if (_tail != null)
		{
			int i = 0;
			for (int num = Count - 1; i < num; i++)
			{
				if (_tail[i].Channel == channel)
				{
					return i;
				}
			}
		}
		return -2;
	}

	private void ResizeTail(int delta)
	{
		int num = _tail.Length + delta;
		Proxy[] array = new Proxy[num];
		Array.Copy(_tail, array, Math.Min(_tail.Length, num));
		_tail = array;
	}
}
