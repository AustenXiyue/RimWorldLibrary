using System;

namespace MS.Internal.AppModel;

[Serializable]
internal enum JournalEntryType : byte
{
	Navigable,
	UiLess
}
