namespace MS.Internal.Ink.InkSerializedFormat;

internal class DeltaDelta : DataXform
{
	private long _d_i_1;

	private long _d_i_2;

	internal DeltaDelta()
	{
	}

	internal override void Transform(int data, ref int xfData, ref int extra)
	{
		long num = data + _d_i_2 - (_d_i_1 << 1);
		_d_i_2 = _d_i_1;
		_d_i_1 = data;
		if (int.MaxValue >= MathHelper.AbsNoThrow(num))
		{
			extra = 0;
			xfData = (int)num;
			return;
		}
		long num2 = MathHelper.AbsNoThrow(num);
		extra = (int)(num2 >> 32);
		extra = (extra << 1) | ((num < 0) ? 1 : 0);
		xfData = (int)(0xFFFFFFFFu & num2);
	}

	internal override void ResetState()
	{
		_d_i_1 = 0L;
		_d_i_2 = 0L;
	}

	internal override int InverseTransform(int xfData, int extra)
	{
		long num2;
		if (extra != 0)
		{
			bool num = (extra & 1) != 0;
			num2 = ((long)extra >> 1 << 32) | (0xFFFFFFFFu & xfData);
			num2 = (num ? (-num2) : num2);
		}
		else
		{
			num2 = xfData;
		}
		long num3 = num2 - _d_i_2 + (_d_i_1 << 1);
		_d_i_2 = _d_i_1;
		_d_i_1 = num3;
		return (int)num3;
	}
}
