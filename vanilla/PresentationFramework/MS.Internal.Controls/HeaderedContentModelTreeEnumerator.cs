using System.Windows.Controls;

namespace MS.Internal.Controls;

internal class HeaderedContentModelTreeEnumerator : ModelTreeEnumerator
{
	private HeaderedContentControl _owner;

	private object _content;

	protected override object Current
	{
		get
		{
			if (base.Index == 1 && _content != null)
			{
				return _content;
			}
			return base.Current;
		}
	}

	protected override bool IsUnchanged
	{
		get
		{
			if (base.Content == _owner.Header)
			{
				return _content == _owner.Content;
			}
			return false;
		}
	}

	internal HeaderedContentModelTreeEnumerator(HeaderedContentControl headeredContentControl, object content, object header)
		: base(header)
	{
		_owner = headeredContentControl;
		_content = content;
	}

	protected override bool MoveNext()
	{
		if (_content != null)
		{
			if (base.Index == 0)
			{
				base.Index++;
				VerifyUnchanged();
				return true;
			}
			if (base.Index == 1)
			{
				base.Index++;
				return false;
			}
		}
		return base.MoveNext();
	}
}
