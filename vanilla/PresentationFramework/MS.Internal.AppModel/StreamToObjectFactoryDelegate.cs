using System;
using System.IO;
using System.Windows.Markup;

namespace MS.Internal.AppModel;

internal delegate object StreamToObjectFactoryDelegate(Stream s, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter);
