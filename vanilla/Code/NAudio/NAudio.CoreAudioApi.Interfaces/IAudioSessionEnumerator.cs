using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
internal interface IAudioSessionEnumerator
{
	int GetCount(out int sessionCount);

	int GetSession(int sessionCount, out IAudioSessionControl session);
}
