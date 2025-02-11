using System;

namespace MS.Internal.AppModel;

internal interface IContentContainer
{
	void OnContentReady(ContentType contentType, object content, Uri uri, object navState);

	void OnNavigationProgress(Uri uri, long bytesRead, long maxBytes);

	void OnStreamClosed(Uri uri);
}
