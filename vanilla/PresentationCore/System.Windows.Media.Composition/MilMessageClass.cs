namespace System.Windows.Media.Composition;

internal enum MilMessageClass
{
	Invalid = 0,
	SyncFlushReply = 1,
	Tier = 4,
	CompositionDeviceStateChange = 5,
	PartitionIsZombie = 6,
	SyncModeStatus = 9,
	Presented = 10,
	RenderStatus = 14,
	BadPixelShader = 16,
	Last = 17,
	FORCE_DWORD = -1
}
