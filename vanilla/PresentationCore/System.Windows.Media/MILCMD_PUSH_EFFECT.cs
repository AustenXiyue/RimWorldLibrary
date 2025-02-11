using System.Runtime.InteropServices;

namespace System.Windows.Media;

[StructLayout(LayoutKind.Explicit)]
internal struct MILCMD_PUSH_EFFECT
{
	[FieldOffset(0)]
	public uint hEffect;

	[FieldOffset(4)]
	public uint hEffectInput;

	public MILCMD_PUSH_EFFECT(uint hEffect, uint hEffectInput)
	{
		this.hEffect = hEffect;
		this.hEffectInput = hEffectInput;
	}
}
