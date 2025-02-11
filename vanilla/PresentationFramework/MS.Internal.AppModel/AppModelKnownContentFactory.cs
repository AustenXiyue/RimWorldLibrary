using System;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using MS.Internal.Resources;

namespace MS.Internal.AppModel;

internal static class AppModelKnownContentFactory
{
	internal static object BamlConverter(Stream stream, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter)
	{
		return BamlConverterCore(stream, baseUri, canUseTopLevelBrowser, sandboxExternalContent, allowAsync, isJournalNavigation, out asyncObjectConverter, isUnsafe: false);
	}

	internal static object BamlConverterCore(Stream stream, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter, bool isUnsafe)
	{
		asyncObjectConverter = null;
		if (isUnsafe)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlIsNotSupportedOutsideOfApplicationResources));
		}
		if (!BaseUriHelper.IsPackApplicationUri(baseUri))
		{
			throw new InvalidOperationException(SR.BamlIsNotSupportedOutsideOfApplicationResources);
		}
		BaseUriHelper.GetAssemblyNameAndPart(PackUriHelper.GetPartUri(baseUri), out var partName, out var _, out var _, out var _);
		if (ContentFileHelper.IsContentFile(partName))
		{
			throw new InvalidOperationException(SR.BamlIsNotSupportedOutsideOfApplicationResources);
		}
		ParserContext parserContext = new ParserContext();
		parserContext.BaseUri = baseUri;
		parserContext.SkipJournaledProperties = isJournalNavigation;
		return Application.LoadBamlStreamWithSyncInfo(stream, parserContext);
	}

	internal static object XamlConverter(Stream stream, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter)
	{
		return XamlConverterCore(stream, baseUri, canUseTopLevelBrowser, sandboxExternalContent, allowAsync, isJournalNavigation, out asyncObjectConverter, isUnsafe: false);
	}

	internal static object XamlConverterCore(Stream stream, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter, bool isUnsafe)
	{
		asyncObjectConverter = null;
		if (sandboxExternalContent)
		{
			if (SecurityHelper.AreStringTypesEqual(baseUri.Scheme, BaseUriHelper.PackAppBaseUri.Scheme))
			{
				baseUri = BaseUriHelper.ConvertPackUriToAbsoluteExternallyVisibleUri(baseUri);
			}
			stream.Close();
			return new WebBrowser
			{
				Source = baseUri
			};
		}
		ParserContext parserContext = new ParserContext();
		parserContext.BaseUri = baseUri;
		parserContext.SkipJournaledProperties = isJournalNavigation;
		if (allowAsync)
		{
			XamlReader xamlReader = (asyncObjectConverter = new XamlReader());
			xamlReader.LoadCompleted += OnParserComplete;
			if (isUnsafe)
			{
				parserContext.FromRestrictiveReader = true;
			}
			return xamlReader.LoadAsync(stream, parserContext);
		}
		return XamlReader.Load(stream, parserContext, isUnsafe);
	}

	private static void OnParserComplete(object sender, AsyncCompletedEventArgs args)
	{
		if (!args.Cancelled && args.Error != null)
		{
			throw args.Error;
		}
	}

	internal static object HtmlXappConverter(Stream stream, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter)
	{
		return HtmlXappConverterCore(stream, baseUri, canUseTopLevelBrowser, sandboxExternalContent, allowAsync, isJournalNavigation, out asyncObjectConverter, isUnsafe: false);
	}

	internal static object HtmlXappConverterCore(Stream stream, Uri baseUri, bool canUseTopLevelBrowser, bool sandboxExternalContent, bool allowAsync, bool isJournalNavigation, out XamlReader asyncObjectConverter, bool isUnsafe)
	{
		asyncObjectConverter = null;
		if (isUnsafe)
		{
			throw new InvalidOperationException(SR.Format(SR.BamlIsNotSupportedOutsideOfApplicationResources));
		}
		if (canUseTopLevelBrowser)
		{
			return null;
		}
		if (SecurityHelper.AreStringTypesEqual(baseUri.Scheme, BaseUriHelper.PackAppBaseUri.Scheme))
		{
			baseUri = BaseUriHelper.ConvertPackUriToAbsoluteExternallyVisibleUri(baseUri);
		}
		stream.Close();
		return new WebBrowser
		{
			Source = baseUri
		};
	}
}
