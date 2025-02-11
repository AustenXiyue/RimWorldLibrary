using System;

namespace MS.Internal.Threading;

internal delegate object InternalRealCallDelegate(Delegate method, object args, int numArgs);
