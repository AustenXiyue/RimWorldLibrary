using System.Runtime.InteropServices;
using System.Windows.Media.Composition;
using System.Windows.Media.Media3D;
using MS.Internal;

namespace System.Windows.Media;

internal static class MILUtilities
{
	internal struct MILRect3D
	{
		public float X;

		public float Y;

		public float Z;

		public float LengthX;

		public float LengthY;

		public float LengthZ;

		public MILRect3D(ref Rect3D rect)
		{
			X = (float)rect.X;
			Y = (float)rect.Y;
			Z = (float)rect.Z;
			LengthX = (float)rect.SizeX;
			LengthY = (float)rect.SizeY;
			LengthZ = (float)rect.SizeZ;
		}
	}

	internal struct MilRectF
	{
		public float Left;

		public float Top;

		public float Right;

		public float Bottom;
	}

	internal static readonly D3DMATRIX D3DMATRIXIdentity = new D3DMATRIX(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);

	internal unsafe static void ConvertToD3DMATRIX(Matrix* matrix, D3DMATRIX* d3dMatrix)
	{
		*d3dMatrix = D3DMATRIXIdentity;
		*(float*)d3dMatrix = (float)(*(double*)matrix);
		*(float*)((byte*)d3dMatrix + 4) = (float)(*(double*)((byte*)matrix + 8));
		*(float*)((byte*)d3dMatrix + (nint)4 * (nint)4) = (float)(*(double*)((byte*)matrix + (nint)2 * (nint)8));
		*(float*)((byte*)d3dMatrix + (nint)5 * (nint)4) = (float)(*(double*)((byte*)matrix + (nint)3 * (nint)8));
		*(float*)((byte*)d3dMatrix + (nint)12 * (nint)4) = (float)(*(double*)((byte*)matrix + (nint)4 * (nint)8));
		*(float*)((byte*)d3dMatrix + (nint)13 * (nint)4) = (float)(*(double*)((byte*)matrix + (nint)5 * (nint)8));
	}

	internal unsafe static void ConvertFromD3DMATRIX(D3DMATRIX* d3dMatrix, Matrix* matrix)
	{
		*(double*)matrix = *(float*)d3dMatrix;
		*(double*)((byte*)matrix + 8) = *(float*)((byte*)d3dMatrix + 4);
		*(double*)((byte*)matrix + (nint)2 * (nint)8) = *(float*)((byte*)d3dMatrix + (nint)4 * (nint)4);
		*(double*)((byte*)matrix + (nint)3 * (nint)8) = *(float*)((byte*)d3dMatrix + (nint)5 * (nint)4);
		*(double*)((byte*)matrix + (nint)4 * (nint)8) = *(float*)((byte*)d3dMatrix + (nint)12 * (nint)4);
		*(double*)((byte*)matrix + (nint)5 * (nint)8) = *(float*)((byte*)d3dMatrix + (nint)13 * (nint)4);
		*(int*)((byte*)matrix + (nint)6 * (nint)8) = 4;
	}

	[DllImport("wpfgfx_cor3.dll")]
	private static extern int MIL3DCalcProjected2DBounds(ref D3DMATRIX pFullTransform3D, ref MILRect3D pboxBounds, out MilRectF prcDestRect);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilUtility_CopyPixelBuffer", PreserveSig = false)]
	internal unsafe static extern void MILCopyPixelBuffer(byte* pOutputBuffer, uint outputBufferSize, uint outputBufferStride, uint outputBufferOffsetInBits, byte* pInputBuffer, uint inputBufferSize, uint inputBufferStride, uint inputBufferOffsetInBits, uint height, uint copyWidthInBits);

	internal static Rect ProjectBounds(ref Matrix3D viewProjMatrix, ref Rect3D originalBox)
	{
		D3DMATRIX pFullTransform3D = CompositionResourceManager.Matrix3DToD3DMATRIX(viewProjMatrix);
		MILRect3D pboxBounds = new MILRect3D(ref originalBox);
		MilRectF prcDestRect = default(MilRectF);
		HRESULT.Check(MIL3DCalcProjected2DBounds(ref pFullTransform3D, ref pboxBounds, out prcDestRect));
		if (prcDestRect.Left == prcDestRect.Right || prcDestRect.Top == prcDestRect.Bottom)
		{
			return Rect.Empty;
		}
		return new Rect(prcDestRect.Left, prcDestRect.Top, prcDestRect.Right - prcDestRect.Left, prcDestRect.Bottom - prcDestRect.Top);
	}
}
