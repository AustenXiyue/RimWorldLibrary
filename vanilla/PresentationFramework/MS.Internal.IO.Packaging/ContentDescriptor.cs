namespace MS.Internal.IO.Packaging;

internal class ContentDescriptor
{
	internal const string ResourceKeyName = "Dictionary";

	internal const string ResourceName = "ElementTable";

	private bool _hasIndexableContent;

	private bool _isInline;

	private string _contentProp;

	private string _titleProp;

	internal bool HasIndexableContent
	{
		get
		{
			return _hasIndexableContent;
		}
		set
		{
			_hasIndexableContent = value;
		}
	}

	internal bool IsInline
	{
		get
		{
			return _isInline;
		}
		set
		{
			_isInline = value;
		}
	}

	internal string ContentProp
	{
		get
		{
			return _contentProp;
		}
		set
		{
			_contentProp = value;
		}
	}

	internal string TitleProp
	{
		get
		{
			return _titleProp;
		}
		set
		{
			_titleProp = value;
		}
	}

	internal ContentDescriptor(bool hasIndexableContent, bool isInline, string contentProp, string titleProp)
	{
		HasIndexableContent = hasIndexableContent;
		IsInline = isInline;
		ContentProp = contentProp;
		TitleProp = titleProp;
	}

	internal ContentDescriptor(bool hasIndexableContent)
	{
		HasIndexableContent = hasIndexableContent;
		IsInline = false;
		ContentProp = null;
		TitleProp = null;
	}
}
