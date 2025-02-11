using System.Runtime.InteropServices;

namespace System.Windows.Media;

internal class ColorTransform
{
	private ColorTransformHelper _colorTransformHelper;

	private uint _inputColorType;

	private uint _outputColorType;

	private ColorTransform()
	{
	}

	internal ColorTransform(ColorContext srcContext, ColorContext dstContext)
	{
		InitializeICM();
		if (srcContext == null)
		{
			srcContext = new ColorContext(PixelFormats.Bgra32);
		}
		if (dstContext == null)
		{
			dstContext = new ColorContext(PixelFormats.Bgra32);
		}
		_inputColorType = srcContext.ColorType;
		_outputColorType = dstContext.ColorType;
		_colorTransformHelper.CreateTransform(srcContext.ProfileHandle, dstContext.ProfileHandle);
	}

	internal ColorTransform(SafeMILHandle bitmapSource, ColorContext srcContext, ColorContext dstContext, PixelFormat pixelFormat)
	{
		InitializeICM();
		if (srcContext == null)
		{
			srcContext = new ColorContext(pixelFormat);
		}
		if (dstContext == null)
		{
			dstContext = new ColorContext(pixelFormat);
		}
		_inputColorType = srcContext.ColorType;
		_outputColorType = dstContext.ColorType;
		if (srcContext.ProfileHandle != null && !srcContext.ProfileHandle.IsInvalid && dstContext.ProfileHandle != null && !dstContext.ProfileHandle.IsInvalid)
		{
			_colorTransformHelper.CreateTransform(srcContext.ProfileHandle, dstContext.ProfileHandle);
		}
	}

	internal void Translate(float[] srcValue, float[] dstValue)
	{
		_ = new nint[2];
		nint num = IntPtr.Zero;
		nint num2 = IntPtr.Zero;
		try
		{
			uint numColors = 1u;
			long val = ICM2Color(srcValue);
			num = Marshal.AllocHGlobal(64);
			Marshal.WriteInt64(num, val);
			num2 = Marshal.AllocHGlobal(64);
			long val2 = 0L;
			Marshal.WriteInt64(num2, val2);
			_colorTransformHelper.TranslateColors(num, numColors, _inputColorType, num2, _outputColorType);
			val2 = Marshal.ReadInt64(num2);
			for (int i = 0; i < dstValue.GetLength(0); i++)
			{
				int num3 = 0xFFFF & (int)(val2 >> 16 * i);
				float num4 = (float)(uint)(num3 & 0x7FFFFFFF) / 65536f;
				if ((uint)num3 < 0u)
				{
					dstValue[i] = 0f - num4;
				}
				else
				{
					dstValue[i] = num4;
				}
			}
		}
		finally
		{
			Marshal.FreeHGlobal(num);
			Marshal.FreeHGlobal(num2);
		}
	}

	private void InitializeICM()
	{
		_colorTransformHelper = new ColorTransformHelper();
	}

	private long ICM2Color(float[] srcValue)
	{
		if (srcValue.GetLength(0) < 3 || srcValue.GetLength(0) > 8)
		{
			throw new NotSupportedException();
		}
		if (srcValue.GetLength(0) <= 4)
		{
			ushort[] array = new ushort[4];
			array[0] = (array[1] = (array[2] = (array[3] = 0)));
			for (int i = 0; i < srcValue.GetLength(0); i++)
			{
				if ((double)srcValue[i] >= 1.0)
				{
					array[i] = ushort.MaxValue;
				}
				else if ((double)srcValue[i] <= 0.0)
				{
					array[i] = 0;
				}
				else
				{
					array[i] = (ushort)(srcValue[i] * 65535f);
				}
			}
			return (long)(((ulong)array[3] << 48) + ((ulong)array[2] << 32) + ((ulong)array[1] << 16) + array[0]);
		}
		byte[] array2 = new byte[8];
		array2[0] = (array2[1] = (array2[2] = (array2[3] = (array2[4] = (array2[5] = (array2[6] = (array2[7] = 0)))))));
		for (int j = 0; j < srcValue.GetLength(0); j++)
		{
			if ((double)srcValue[j] >= 1.0)
			{
				array2[j] = byte.MaxValue;
			}
			else if ((double)srcValue[j] <= 0.0)
			{
				array2[j] = 0;
			}
			else
			{
				array2[j] = (byte)(srcValue[j] * 255f);
			}
		}
		return (long)(((ulong)array2[7] << 56) + ((ulong)array2[6] << 48) + ((ulong)array2[5] << 40) + ((ulong)array2[4] << 32) + ((ulong)array2[3] << 24) + ((ulong)array2[2] << 16) + ((ulong)array2[1] << 8) + array2[0]);
	}
}
