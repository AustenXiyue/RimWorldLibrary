using System;

namespace Mono.Unix.Native;

[Map]
[CLSCompliant(false)]
public enum LockType : short
{
	F_RDLCK,
	F_WRLCK,
	F_UNLCK
}
