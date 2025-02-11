using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Markup;

namespace MS.Internal.AppModel;

internal static class MimeObjectFactory
{
	private static readonly Dictionary<ContentType, StreamToObjectFactoryDelegate> _objectConverters = new Dictionary<ContentType, StreamToObjectFactoryDelegate>(9, new ContentType.WeakComparer());

	private static readonly Dictionary<ContentType, StreamToObjectFactoryDelegateCore> _objectConvertersCore = new Dictionary<ContentType, StreamToObjectFactoryDelegateCore>(9, new ContentType.WeakComparer());

	internal static object GetObjectAndCloseStream(Stream s, ContentType contentType, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter)
	{
		return GetObjectAndCloseStreamCore(s, contentType, baseUri, canUseTopLevelBrowser, sandboxExternalContent, allowAsync, isJournalNavigation, out asyncObjectConverter, isUnsafe: false);
	}

	internal static object GetObjectAndCloseStreamCore(Stream s, ContentType contentType, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter, bool isUnsafe)
	{
		object result = null;
		asyncObjectConverter = null;
		if (contentType != null && _objectConvertersCore.TryGetValue(contentType, out var value))
		{
			result = value(s, baseUri, canUseTopLevelBrowser, sandboxExternalContent, allowAsync, isJournalNavigation, out asyncObjectConverter, isUnsafe);
		}
		return result;
	}

	internal static void RegisterCore(ContentType contentType, StreamToObjectFactoryDelegateCore method)
	{
		_objectConvertersCore[contentType] = method;
	}

	internal static void Register(ContentType contentType, StreamToObjectFactoryDelegate method)
	{
		StreamToObjectFactoryDelegateCore method2 = delegate(Stream s, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter, bool isUnsafe)
		{
			return method(s, baseUri, canUseTopLevelBrowser, sandboxExternalContent, allowAsync, isJournalNavigation, out asyncObjectConverter);
		};
		RegisterCore(contentType, method2);
	}
}
