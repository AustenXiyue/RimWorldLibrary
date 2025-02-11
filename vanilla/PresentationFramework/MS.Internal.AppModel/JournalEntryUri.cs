using System;
using System.Runtime.Serialization;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

[Serializable]
internal class JournalEntryUri : JournalEntry, ISerializable
{
	internal JournalEntryUri(JournalEntryGroupState jeGroupState, Uri uri)
		: base(jeGroupState, uri)
	{
	}

	protected JournalEntryUri(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal override void SaveState(object contentObject)
	{
		Invariant.Assert(base.Source != null, "Can't journal by Uri without a Uri.");
		base.SaveState(contentObject);
	}
}
