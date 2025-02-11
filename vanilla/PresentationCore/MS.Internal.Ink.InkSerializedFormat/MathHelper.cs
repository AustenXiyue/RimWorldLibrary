namespace MS.Internal.Ink.InkSerializedFormat;

internal static class MathHelper
{
	internal static int AbsNoThrow(int data)
	{
		if (data >= 0)
		{
			return data;
		}
		return -data;
	}

	internal static long AbsNoThrow(long data)
	{
		if (data >= 0)
		{
			return data;
		}
		return -data;
	}
}
