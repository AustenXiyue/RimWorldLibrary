using System.Windows.Controls;

namespace MS.Internal.Controls;

internal class ContentModelTreeEnumerator : ModelTreeEnumerator
{
	private ContentControl _owner;

	protected override bool IsUnchanged => base.Content == _owner.Content;

	internal ContentModelTreeEnumerator(ContentControl contentControl, object content)
		: base(content)
	{
		_owner = contentControl;
	}
}
