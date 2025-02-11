using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal struct PROPVARIANT
{
	[FieldOffset(0)]
	internal ushort varType;

	[FieldOffset(2)]
	internal ushort wReserved1;

	[FieldOffset(4)]
	internal ushort wReserved2;

	[FieldOffset(6)]
	internal ushort wReserved3;

	[FieldOffset(8)]
	internal byte bVal;

	[FieldOffset(8)]
	internal sbyte cVal;

	[FieldOffset(8)]
	internal ushort uiVal;

	[FieldOffset(8)]
	internal short iVal;

	[FieldOffset(8)]
	internal uint uintVal;

	[FieldOffset(8)]
	internal int intVal;

	[FieldOffset(8)]
	internal ulong ulVal;

	[FieldOffset(8)]
	internal long lVal;

	[FieldOffset(8)]
	internal float fltVal;

	[FieldOffset(8)]
	internal double dblVal;

	[FieldOffset(8)]
	internal short boolVal;

	[FieldOffset(8)]
	internal nint pclsidVal;

	[FieldOffset(8)]
	internal nint pszVal;

	[FieldOffset(8)]
	internal nint pwszVal;

	[FieldOffset(8)]
	internal nint punkVal;

	[FieldOffset(8)]
	internal PROPARRAY ca;

	[FieldOffset(8)]
	internal FILETIME filetime;

	internal bool RequiresSyncObject => varType == 13;

	private unsafe static void CopyBytes(byte* pbTo, int cbTo, byte* pbFrom, int cbFrom)
	{
		if (cbFrom > cbTo)
		{
			throw new InvalidOperationException(SR.Image_InsufficientBufferSize);
		}
		for (int i = 0; i < cbFrom; i++)
		{
			pbTo[i] = pbFrom[i];
		}
	}

	internal void InitVector(Array array, Type type, VarEnum varEnum)
	{
		Init(array, type, varEnum | VarEnum.VT_VECTOR);
	}

	internal unsafe void Init(Array array, Type type, VarEnum vt)
	{
		varType = (ushort)vt;
		ca.cElems = 0u;
		ca.pElems = IntPtr.Zero;
		int length = array.Length;
		if (length <= 0)
		{
			return;
		}
		long num = Marshal.SizeOf(type) * length;
		nint num2 = IntPtr.Zero;
		GCHandle gCHandle = default(GCHandle);
		try
		{
			num2 = Marshal.AllocCoTaskMem((int)num);
			gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			CopyBytes((byte*)num2, (int)num, (byte*)gCHandle.AddrOfPinnedObject(), (int)num);
			ca.cElems = (uint)length;
			ca.pElems = num2;
			num2 = IntPtr.Zero;
		}
		finally
		{
			if (gCHandle.IsAllocated)
			{
				gCHandle.Free();
			}
			if (num2 != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(num2);
			}
		}
	}

	internal unsafe void Init(string[] value, bool fAscii)
	{
		varType = (ushort)(fAscii ? 30u : 31u);
		varType |= 4096;
		ca.cElems = 0u;
		ca.pElems = IntPtr.Zero;
		int num = value.Length;
		if (num <= 0)
		{
			return;
		}
		nint num2 = IntPtr.Zero;
		int num3 = 0;
		num3 = sizeof(nint);
		long num4 = num3 * num;
		int i = 0;
		try
		{
			nint zero = IntPtr.Zero;
			num2 = Marshal.AllocCoTaskMem((int)num4);
			for (i = 0; i < num; i++)
			{
				zero = ((!fAscii) ? Marshal.StringToCoTaskMemUni(value[i]) : Marshal.StringToCoTaskMemAnsi(value[i]));
				Marshal.WriteIntPtr(num2, i * num3, zero);
			}
			ca.cElems = (uint)num;
			ca.pElems = num2;
			num2 = IntPtr.Zero;
		}
		finally
		{
			if (num2 != IntPtr.Zero)
			{
				for (int j = 0; j < i; j++)
				{
					Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(num2, j * num3));
				}
				Marshal.FreeCoTaskMem(num2);
			}
		}
	}

	internal void Init(object value)
	{
		if (value == null)
		{
			varType = 0;
			return;
		}
		if (value is Array)
		{
			Type type = value.GetType();
			if (type == typeof(sbyte[]))
			{
				InitVector(value as Array, typeof(sbyte), VarEnum.VT_I1);
				return;
			}
			if (type == typeof(byte[]))
			{
				InitVector(value as Array, typeof(byte), VarEnum.VT_UI1);
				return;
			}
			if (value is char[])
			{
				varType = 30;
				pszVal = Marshal.StringToCoTaskMemAnsi(new string(value as char[]));
				return;
			}
			if (value is char[][])
			{
				char[][] array = value as char[][];
				string[] array2 = new string[array.GetLength(0)];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = new string(array[i]);
				}
				Init(array2, fAscii: true);
				return;
			}
			if (type == typeof(short[]))
			{
				InitVector(value as Array, typeof(short), VarEnum.VT_I2);
				return;
			}
			if (type == typeof(ushort[]))
			{
				InitVector(value as Array, typeof(ushort), VarEnum.VT_UI2);
				return;
			}
			if (type == typeof(int[]))
			{
				InitVector(value as Array, typeof(int), VarEnum.VT_I4);
				return;
			}
			if (type == typeof(uint[]))
			{
				InitVector(value as Array, typeof(uint), VarEnum.VT_UI4);
				return;
			}
			if (type == typeof(long[]))
			{
				InitVector(value as Array, typeof(long), VarEnum.VT_I8);
				return;
			}
			if (type == typeof(ulong[]))
			{
				InitVector(value as Array, typeof(ulong), VarEnum.VT_UI8);
				return;
			}
			if (value is float[])
			{
				InitVector(value as Array, typeof(float), VarEnum.VT_R4);
				return;
			}
			if (value is double[])
			{
				InitVector(value as Array, typeof(double), VarEnum.VT_R8);
				return;
			}
			if (value is Guid[])
			{
				InitVector(value as Array, typeof(Guid), VarEnum.VT_CLSID);
				return;
			}
			if (value is string[])
			{
				Init(value as string[], fAscii: false);
				return;
			}
			if (value is bool[])
			{
				bool[] array3 = value as bool[];
				short[] array4 = new short[array3.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					array4[j] = (short)(array3[j] ? (-1) : 0);
				}
				InitVector(array4, typeof(short), VarEnum.VT_BOOL);
				return;
			}
			throw new InvalidOperationException(SR.Image_PropertyNotSupported);
		}
		Type type2 = value.GetType();
		if (value is string)
		{
			varType = 31;
			pwszVal = Marshal.StringToCoTaskMemUni(value as string);
			return;
		}
		if (type2 == typeof(sbyte))
		{
			varType = 16;
			cVal = (sbyte)value;
			return;
		}
		if (type2 == typeof(byte))
		{
			varType = 17;
			bVal = (byte)value;
			return;
		}
		if (type2 == typeof(FILETIME))
		{
			varType = 64;
			filetime = (FILETIME)value;
			return;
		}
		if (value is char)
		{
			varType = 30;
			Span<char> span = stackalloc char[1] { (char)value };
			pszVal = Marshal.StringToCoTaskMemAnsi(new string(span));
			return;
		}
		if (type2 == typeof(short))
		{
			varType = 2;
			iVal = (short)value;
			return;
		}
		if (type2 == typeof(ushort))
		{
			varType = 18;
			uiVal = (ushort)value;
			return;
		}
		if (type2 == typeof(int))
		{
			varType = 3;
			intVal = (int)value;
			return;
		}
		if (type2 == typeof(uint))
		{
			varType = 19;
			uintVal = (uint)value;
			return;
		}
		if (type2 == typeof(long))
		{
			varType = 20;
			lVal = (long)value;
			return;
		}
		if (type2 == typeof(ulong))
		{
			varType = 21;
			ulVal = (ulong)value;
			return;
		}
		if (value is float)
		{
			varType = 4;
			fltVal = (float)value;
			return;
		}
		if (value is double)
		{
			varType = 5;
			dblVal = (double)value;
			return;
		}
		if (value is Guid guid)
		{
			byte[] array5 = guid.ToByteArray();
			varType = 72;
			pclsidVal = Marshal.AllocCoTaskMem(array5.Length);
			Marshal.Copy(array5, 0, pclsidVal, array5.Length);
			return;
		}
		if (value is bool)
		{
			varType = 11;
			boolVal = (short)(((bool)value) ? (-1) : 0);
			return;
		}
		if (value is BitmapMetadataBlob)
		{
			Init((value as BitmapMetadataBlob).InternalGetBlobValue(), typeof(byte), VarEnum.VT_BLOB);
			return;
		}
		if (value is BitmapMetadata)
		{
			nint ppvObject = IntPtr.Zero;
			SafeMILHandle internalMetadataHandle = (value as BitmapMetadata).InternalMetadataHandle;
			if (internalMetadataHandle == null || internalMetadataHandle.IsInvalid)
			{
				throw new NotImplementedException();
			}
			Guid guid2 = MILGuidData.IID_IWICMetadataQueryReader;
			HRESULT.Check(UnsafeNativeMethods.MILUnknown.QueryInterface(internalMetadataHandle, ref guid2, out ppvObject));
			varType = 13;
			punkVal = ppvObject;
			return;
		}
		throw new InvalidOperationException(SR.Image_PropertyNotSupported);
	}

	internal unsafe void Clear()
	{
		VarEnum varEnum = (VarEnum)varType;
		if ((varEnum & VarEnum.VT_VECTOR) != 0 || varEnum == VarEnum.VT_BLOB)
		{
			if (ca.pElems != IntPtr.Zero)
			{
				switch (varEnum & (VarEnum)(-4097))
				{
				case VarEnum.VT_UNKNOWN:
				{
					nint pElems2 = ca.pElems;
					int num3 = 0;
					num3 = sizeof(nint);
					for (uint num4 = 0u; num4 < ca.cElems; num4++)
					{
						UnsafeNativeMethods.MILUnknown.Release(Marshal.ReadIntPtr(pElems2, (int)(num4 * num3)));
					}
					break;
				}
				case VarEnum.VT_LPSTR:
				case VarEnum.VT_LPWSTR:
				{
					nint pElems = ca.pElems;
					int num = 0;
					num = sizeof(nint);
					for (uint num2 = 0u; num2 < ca.cElems; num2++)
					{
						Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(pElems, (int)(num2 * num)));
					}
					break;
				}
				}
				Marshal.FreeCoTaskMem(ca.pElems);
			}
		}
		else
		{
			switch (varEnum)
			{
			case VarEnum.VT_LPSTR:
			case VarEnum.VT_LPWSTR:
			case VarEnum.VT_CLSID:
				Marshal.FreeCoTaskMem(pwszVal);
				break;
			case VarEnum.VT_UNKNOWN:
				UnsafeNativeMethods.MILUnknown.Release(punkVal);
				break;
			}
		}
		varEnum = VarEnum.VT_EMPTY;
	}

	internal unsafe object ToObject(object syncObject)
	{
		VarEnum varEnum = (VarEnum)varType;
		if ((varEnum & VarEnum.VT_VECTOR) != 0)
		{
			switch (varEnum & (VarEnum)(-4097))
			{
			case VarEnum.VT_EMPTY:
				return null;
			case VarEnum.VT_I1:
			{
				sbyte[] array2 = new sbyte[ca.cElems];
				for (int j = 0; j < ca.cElems; j++)
				{
					array2[j] = (sbyte)Marshal.ReadByte(ca.pElems, j);
				}
				return array2;
			}
			case VarEnum.VT_UI1:
			{
				byte[] array9 = new byte[ca.cElems];
				Marshal.Copy(ca.pElems, array9, 0, (int)ca.cElems);
				return array9;
			}
			case VarEnum.VT_I2:
			{
				short[] array8 = new short[ca.cElems];
				Marshal.Copy(ca.pElems, array8, 0, (int)ca.cElems);
				return array8;
			}
			case VarEnum.VT_UI2:
			{
				ushort[] array12 = new ushort[ca.cElems];
				for (int num3 = 0; num3 < ca.cElems; num3++)
				{
					array12[num3] = (ushort)Marshal.ReadInt16(ca.pElems, num3 * 2);
				}
				return array12;
			}
			case VarEnum.VT_I4:
			{
				int[] array11 = new int[ca.cElems];
				Marshal.Copy(ca.pElems, array11, 0, (int)ca.cElems);
				return array11;
			}
			case VarEnum.VT_UI4:
			{
				uint[] array5 = new uint[ca.cElems];
				for (int l = 0; l < ca.cElems; l++)
				{
					array5[l] = (uint)Marshal.ReadInt32(ca.pElems, l * 4);
				}
				return array5;
			}
			case VarEnum.VT_I8:
			{
				long[] array4 = new long[ca.cElems];
				Marshal.Copy(ca.pElems, array4, 0, (int)ca.cElems);
				return array4;
			}
			case VarEnum.VT_UI8:
			{
				ulong[] array15 = new ulong[ca.cElems];
				for (int num4 = 0; num4 < ca.cElems; num4++)
				{
					array15[num4] = (ulong)Marshal.ReadInt64(ca.pElems, num4 * 8);
				}
				return array15;
			}
			case VarEnum.VT_R4:
			{
				float[] array14 = new float[ca.cElems];
				Marshal.Copy(ca.pElems, array14, 0, (int)ca.cElems);
				return array14;
			}
			case VarEnum.VT_R8:
			{
				double[] array13 = new double[ca.cElems];
				Marshal.Copy(ca.pElems, array13, 0, (int)ca.cElems);
				return array13;
			}
			case VarEnum.VT_BOOL:
			{
				bool[] array10 = new bool[ca.cElems];
				for (int n = 0; n < ca.cElems; n++)
				{
					array10[n] = Marshal.ReadInt16(ca.pElems, n * 2) != 0;
				}
				return array10;
			}
			case VarEnum.VT_CLSID:
			{
				Guid[] array6 = new Guid[ca.cElems];
				for (int m = 0; m < ca.cElems; m++)
				{
					byte[] array7 = new byte[16];
					Marshal.Copy(ca.pElems, array7, m * 16, 16);
					array6[m] = new Guid(array7);
				}
				return array6;
			}
			case VarEnum.VT_LPSTR:
			{
				string[] array3 = new string[ca.cElems];
				int num2 = 0;
				num2 = sizeof(nint);
				for (int k = 0; k < ca.cElems; k++)
				{
					nint ptr2 = Marshal.ReadIntPtr(ca.pElems, k * num2);
					array3[k] = Marshal.PtrToStringAnsi(ptr2);
				}
				return array3;
			}
			case VarEnum.VT_LPWSTR:
			{
				string[] array = new string[ca.cElems];
				int num = 0;
				num = sizeof(nint);
				for (int i = 0; i < ca.cElems; i++)
				{
					nint ptr = Marshal.ReadIntPtr(ca.pElems, i * num);
					array[i] = Marshal.PtrToStringUni(ptr);
				}
				return array;
			}
			}
		}
		else
		{
			switch (varEnum)
			{
			case VarEnum.VT_EMPTY:
				return null;
			case VarEnum.VT_I1:
				return cVal;
			case VarEnum.VT_UI1:
				return bVal;
			case VarEnum.VT_I2:
				return iVal;
			case VarEnum.VT_UI2:
				return uiVal;
			case VarEnum.VT_I4:
				return intVal;
			case VarEnum.VT_UI4:
				return uintVal;
			case VarEnum.VT_I8:
				return lVal;
			case VarEnum.VT_UI8:
				return ulVal;
			case VarEnum.VT_R4:
				return fltVal;
			case VarEnum.VT_R8:
				return dblVal;
			case VarEnum.VT_FILETIME:
				return filetime;
			case VarEnum.VT_BOOL:
				return boolVal != 0;
			case VarEnum.VT_CLSID:
			{
				byte[] array17 = new byte[16];
				Marshal.Copy(pclsidVal, array17, 0, 16);
				return new Guid(array17);
			}
			case VarEnum.VT_LPSTR:
				return Marshal.PtrToStringAnsi(pszVal);
			case VarEnum.VT_LPWSTR:
				return Marshal.PtrToStringUni(pwszVal);
			case VarEnum.VT_BLOB:
			{
				byte[] array16 = new byte[ca.cElems];
				Marshal.Copy(ca.pElems, array16, 0, (int)ca.cElems);
				return new BitmapMetadataBlob(array16);
			}
			case VarEnum.VT_UNKNOWN:
			{
				nint ppvObject = IntPtr.Zero;
				Guid guid = MILGuidData.IID_IWICMetadataQueryWriter;
				Guid guid2 = MILGuidData.IID_IWICMetadataQueryReader;
				try
				{
					if (UnsafeNativeMethods.MILUnknown.QueryInterface(punkVal, ref guid, out ppvObject) == 0)
					{
						SafeMILHandle metadataHandle = new SafeMILHandle(ppvObject);
						ppvObject = IntPtr.Zero;
						return new BitmapMetadata(metadataHandle, readOnly: false, fixedSize: false, syncObject);
					}
					int num5 = UnsafeNativeMethods.MILUnknown.QueryInterface(punkVal, ref guid2, out ppvObject);
					if (num5 == 0)
					{
						SafeMILHandle metadataHandle2 = new SafeMILHandle(ppvObject);
						ppvObject = IntPtr.Zero;
						return new BitmapMetadata(metadataHandle2, readOnly: true, fixedSize: false, syncObject);
					}
					HRESULT.Check(num5);
				}
				finally
				{
					if (ppvObject != IntPtr.Zero)
					{
						UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ppvObject);
					}
				}
				break;
			}
			}
		}
		throw new NotSupportedException(SR.Image_PropertyNotSupported);
	}
}
