namespace Iced.Intel.BlockEncoderInternal;

internal sealed class BlockData
{
	internal ulong __dont_use_address;

	internal bool __dont_use_address_initd;

	public bool IsValid;

	public ulong Data;

	public ulong Address
	{
		get
		{
			if (!IsValid)
			{
				ThrowHelper.ThrowInvalidOperationException();
			}
			if (!__dont_use_address_initd)
			{
				ThrowHelper.ThrowInvalidOperationException();
			}
			return __dont_use_address;
		}
	}
}
