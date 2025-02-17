namespace Mono.Cecil;

internal class MarshalInfo
{
	internal NativeType native;

	public NativeType NativeType
	{
		get
		{
			return native;
		}
		set
		{
			native = value;
		}
	}

	public MarshalInfo(NativeType native)
	{
		this.native = native;
	}
}
