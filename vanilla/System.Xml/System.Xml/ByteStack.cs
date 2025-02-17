namespace System.Xml;

internal class ByteStack
{
	private byte[] stack;

	private int growthRate;

	private int top;

	private int size;

	public int Length => top;

	public ByteStack(int growthRate)
	{
		this.growthRate = growthRate;
		top = 0;
		stack = new byte[growthRate];
		size = growthRate;
	}

	public void Push(byte data)
	{
		if (size == top)
		{
			byte[] dst = new byte[size + growthRate];
			if (top > 0)
			{
				Buffer.BlockCopy(stack, 0, dst, 0, top);
			}
			stack = dst;
			size += growthRate;
		}
		stack[top++] = data;
	}

	public byte Pop()
	{
		if (top > 0)
		{
			return stack[--top];
		}
		return 0;
	}

	public byte Peek()
	{
		if (top > 0)
		{
			return stack[top - 1];
		}
		return 0;
	}
}
