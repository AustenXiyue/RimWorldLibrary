using System.Collections.Generic;
using System.IO;
using System.Net;

namespace System.Windows.Media.Imaging;

internal class QueueEntry
{
	internal List<WeakReference> decoders;

	internal Uri inputUri;

	internal Stream inputStream;

	internal Stream outputStream;

	internal string streamPath;

	internal byte[] readBuffer;

	internal long contentLength;

	internal string contentType;

	internal int lastPercent;

	internal WebRequest webRequest;
}
