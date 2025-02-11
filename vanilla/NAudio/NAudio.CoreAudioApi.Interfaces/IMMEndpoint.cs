using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("1BE09788-6894-4089-8586-9A2A6C265AC5")]
internal interface IMMEndpoint
{
	int GetDataFlow(out DataFlow dataFlow);
}
