using System;

namespace MonoMod.Logs;

[Flags]
internal enum LogLevelFilter
{
	None = 0,
	Spam = 1,
	Trace = 2,
	Info = 4,
	Warning = 8,
	Error = 0x10,
	Assert = 0x20,
	DefaultFilter = -2
}
