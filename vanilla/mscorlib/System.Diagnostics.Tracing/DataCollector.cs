using System.Runtime.InteropServices;
using System.Security;

namespace System.Diagnostics.Tracing;

[SecurityCritical]
internal struct DataCollector
{
	[ThreadStatic]
	internal static DataCollector ThreadInstance;

	private unsafe byte* scratchEnd;

	private unsafe EventSource.EventData* datasEnd;

	private unsafe GCHandle* pinsEnd;

	private unsafe EventSource.EventData* datasStart;

	private unsafe byte* scratch;

	private unsafe EventSource.EventData* datas;

	private unsafe GCHandle* pins;

	private byte[] buffer;

	private int bufferPos;

	private int bufferNesting;

	private bool writingScalars;

	internal unsafe void Enable(byte* scratch, int scratchSize, EventSource.EventData* datas, int dataCount, GCHandle* pins, int pinCount)
	{
		datasStart = datas;
		scratchEnd = scratch + scratchSize;
		datasEnd = datas + dataCount;
		pinsEnd = pins + pinCount;
		this.scratch = scratch;
		this.datas = datas;
		this.pins = pins;
		writingScalars = false;
	}

	internal void Disable()
	{
		this = default(DataCollector);
	}

	internal unsafe EventSource.EventData* Finish()
	{
		ScalarsEnd();
		return datas;
	}

	internal unsafe void AddScalar(void* value, int size)
	{
		if (bufferNesting == 0)
		{
			byte* ptr = scratch;
			byte* ptr2 = ptr + size;
			if (scratchEnd < ptr2)
			{
				throw new IndexOutOfRangeException(Environment.GetResourceString("Getting out of bounds during scalar addition."));
			}
			ScalarsBegin();
			scratch = ptr2;
			for (int i = 0; i != size; i++)
			{
				ptr[i] = ((byte*)value)[i];
			}
		}
		else
		{
			int num = bufferPos;
			int num2;
			checked
			{
				bufferPos += size;
				EnsureBuffer();
				num2 = 0;
			}
			while (num2 != size)
			{
				buffer[num] = ((byte*)value)[num2];
				num2++;
				num++;
			}
		}
	}

	internal unsafe void AddBinary(string value, int size)
	{
		if (size > 65535)
		{
			size = 65534;
		}
		if (bufferNesting != 0)
		{
			EnsureBuffer(size + 2);
		}
		AddScalar(&size, 2);
		if (size == 0)
		{
			return;
		}
		if (bufferNesting == 0)
		{
			ScalarsEnd();
			PinArray(value, size);
			return;
		}
		int startIndex = bufferPos;
		checked
		{
			bufferPos += size;
			EnsureBuffer();
			fixed (void* ptr = value)
			{
				Marshal.Copy((IntPtr)ptr, buffer, startIndex, size);
			}
		}
	}

	internal void AddBinary(Array value, int size)
	{
		AddArray(value, size, 1);
	}

	internal unsafe void AddArray(Array value, int length, int itemSize)
	{
		if (length > 65535)
		{
			length = 65535;
		}
		int num = length * itemSize;
		if (bufferNesting != 0)
		{
			EnsureBuffer(num + 2);
		}
		AddScalar(&length, 2);
		checked
		{
			if (length != 0)
			{
				if (bufferNesting == 0)
				{
					ScalarsEnd();
					PinArray(value, num);
					return;
				}
				int dstOffset = bufferPos;
				bufferPos += num;
				EnsureBuffer();
				Buffer.BlockCopy(value, 0, buffer, dstOffset, num);
			}
		}
	}

	internal int BeginBufferedArray()
	{
		BeginBuffered();
		bufferPos += 2;
		return bufferPos;
	}

	internal void EndBufferedArray(int bookmark, int count)
	{
		EnsureBuffer();
		buffer[bookmark - 2] = (byte)count;
		buffer[bookmark - 1] = (byte)(count >> 8);
		EndBuffered();
	}

	internal void BeginBuffered()
	{
		ScalarsEnd();
		bufferNesting++;
	}

	internal void EndBuffered()
	{
		bufferNesting--;
		if (bufferNesting == 0)
		{
			EnsureBuffer();
			PinArray(buffer, bufferPos);
			buffer = null;
			bufferPos = 0;
		}
	}

	private void EnsureBuffer()
	{
		int num = bufferPos;
		if (buffer == null || buffer.Length < num)
		{
			GrowBuffer(num);
		}
	}

	private void EnsureBuffer(int additionalSize)
	{
		int num = bufferPos + additionalSize;
		if (buffer == null || buffer.Length < num)
		{
			GrowBuffer(num);
		}
	}

	private void GrowBuffer(int required)
	{
		int num = ((buffer == null) ? 64 : buffer.Length);
		do
		{
			num *= 2;
		}
		while (num < required);
		Array.Resize(ref buffer, num);
	}

	private unsafe void PinArray(object value, int size)
	{
		GCHandle* ptr = pins;
		if (pinsEnd <= ptr)
		{
			throw new IndexOutOfRangeException(Environment.GetResourceString("Pins are out of range."));
		}
		EventSource.EventData* ptr2 = datas;
		if (datasEnd <= ptr2)
		{
			throw new IndexOutOfRangeException(Environment.GetResourceString("Data descriptors are out of range."));
		}
		pins = ptr + 1;
		datas = ptr2 + 1;
		*ptr = GCHandle.Alloc(value, GCHandleType.Pinned);
		ptr2->m_Ptr = (long)(ulong)(UIntPtr)(void*)ptr->AddrOfPinnedObject();
		ptr2->m_Size = size;
	}

	private unsafe void ScalarsBegin()
	{
		if (!writingScalars)
		{
			EventSource.EventData* ptr = datas;
			if (datasEnd <= ptr)
			{
				throw new IndexOutOfRangeException(Environment.GetResourceString("Data descriptors are out of range."));
			}
			ptr->m_Ptr = (long)(ulong)(UIntPtr)scratch;
			writingScalars = true;
		}
	}

	private unsafe void ScalarsEnd()
	{
		checked
		{
			if (writingScalars)
			{
				EventSource.EventData* ptr = datas;
				ptr->m_Size = (int)(scratch - unchecked((byte*)checked((nuint)ptr->m_Ptr)));
				datas = ptr + 1;
				writingScalars = false;
			}
		}
	}
}
