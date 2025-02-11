using System;
using System.ComponentModel;

namespace WinRT.Interop;

[EditorBrowsable(EditorBrowsableState.Never)]
internal delegate int _get_PropertyAsGuid(nint thisPtr, out Guid value);
