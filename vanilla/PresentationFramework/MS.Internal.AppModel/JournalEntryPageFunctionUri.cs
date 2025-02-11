using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

[Serializable]
internal class JournalEntryPageFunctionUri : JournalEntryPageFunctionSaver, ISerializable
{
	private Uri _markupUri;

	internal JournalEntryPageFunctionUri(JournalEntryGroupState jeGroupState, PageFunctionBase pageFunction, Uri markupUri)
		: base(jeGroupState, pageFunction)
	{
		_markupUri = markupUri;
	}

	protected JournalEntryPageFunctionUri(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_markupUri = (Uri)info.GetValue("_markupUri", typeof(Uri));
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("_markupUri", _markupUri);
	}

	internal override PageFunctionBase ResumePageFunction()
	{
		PageFunctionBase pageFunctionBase = Application.LoadComponent(_markupUri, bSkipJournaledProperties: true) as PageFunctionBase;
		RestoreState(pageFunctionBase);
		return pageFunctionBase;
	}
}
