namespace MS.Internal.Shaping;

internal abstract class UshortBuffer
{
	protected int _leap;

	public abstract ushort this[int index] { get; set; }

	public abstract int Length { get; }

	public virtual ushort[] ToArray()
	{
		return null;
	}

	public virtual ushort[] GetSubsetCopy(int index, int count)
	{
		return null;
	}

	public virtual void Insert(int index, int count, int length)
	{
	}

	public virtual void Remove(int index, int count, int length)
	{
	}
}
