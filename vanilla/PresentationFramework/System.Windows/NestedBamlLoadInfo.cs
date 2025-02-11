using System.IO;

namespace System.Windows;

internal class NestedBamlLoadInfo
{
	private Uri _BamlUri;

	private Stream _BamlStream;

	private bool _SkipJournaledProperties;

	internal Uri BamlUri
	{
		get
		{
			return _BamlUri;
		}
		set
		{
			_BamlUri = value;
		}
	}

	internal Stream BamlStream => _BamlStream;

	internal bool SkipJournaledProperties => _SkipJournaledProperties;

	internal NestedBamlLoadInfo(Uri uri, Stream stream, bool bSkipJournalProperty)
	{
		_BamlUri = uri;
		_BamlStream = stream;
		_SkipJournaledProperties = bSkipJournalProperty;
	}
}
