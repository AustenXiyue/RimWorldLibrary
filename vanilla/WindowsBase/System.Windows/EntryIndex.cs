using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct EntryIndex
{
	private uint _store;

	public bool Found => (_store & 0x80000000u) != 0;

	public uint Index => _store & 0x7FFFFFFF;

	public EntryIndex(uint index)
	{
		_store = index | 0x80000000u;
	}

	public EntryIndex(uint index, bool found)
	{
		_store = index & 0x7FFFFFFF;
		if (found)
		{
			_store |= 2147483648u;
		}
	}
}
