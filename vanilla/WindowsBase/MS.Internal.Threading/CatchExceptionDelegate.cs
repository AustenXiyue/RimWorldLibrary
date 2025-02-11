using System;

namespace MS.Internal.Threading;

internal delegate bool CatchExceptionDelegate(object source, Exception e, Delegate catchHandler);
