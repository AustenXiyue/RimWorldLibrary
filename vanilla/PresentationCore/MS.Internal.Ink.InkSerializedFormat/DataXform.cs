namespace MS.Internal.Ink.InkSerializedFormat;

internal abstract class DataXform
{
	internal abstract void Transform(int data, ref int xfData, ref int extra);

	internal abstract void ResetState();

	internal abstract int InverseTransform(int xfData, int extra);
}
