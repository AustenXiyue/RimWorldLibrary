using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilColorF
{
	internal float r;

	internal float g;

	internal float b;

	internal float a;

	public override int GetHashCode()
	{
		return a.GetHashCode() ^ r.GetHashCode() ^ g.GetHashCode() ^ b.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}
}
