using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

[Serializable]
internal abstract class JournalEntryPageFunctionSaver : JournalEntryPageFunction, ISerializable
{
	private ReturnEventSaver _returnEventSaver;

	internal JournalEntryPageFunctionSaver(JournalEntryGroupState jeGroupState, PageFunctionBase pageFunction)
		: base(jeGroupState, pageFunction)
	{
	}

	protected JournalEntryPageFunctionSaver(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_returnEventSaver = (ReturnEventSaver)info.GetValue("_returnEventSaver", typeof(ReturnEventSaver));
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("_returnEventSaver", _returnEventSaver);
	}

	internal override void SaveState(object contentObject)
	{
		PageFunctionBase pageFunctionBase = (PageFunctionBase)contentObject;
		_returnEventSaver = pageFunctionBase._Saver;
		base.SaveState(contentObject);
	}

	internal override void RestoreState(object contentObject)
	{
		if (contentObject == null)
		{
			throw new ArgumentNullException("contentObject");
		}
		PageFunctionBase pageFunctionBase = (PageFunctionBase)contentObject;
		if (pageFunctionBase == null)
		{
			throw new Exception(SR.Format(SR.InvalidPageFunctionType, contentObject.GetType()));
		}
		pageFunctionBase.ParentPageFunctionId = base.ParentPageFunctionId;
		pageFunctionBase.PageFunctionId = base.PageFunctionId;
		pageFunctionBase._Saver = _returnEventSaver;
		pageFunctionBase._Resume = true;
		base.RestoreState(pageFunctionBase);
	}

	internal override bool Navigate(INavigator navigator, NavigationMode navMode)
	{
		NavigationService navigationService = ((navigator is IDownloader downloader) ? downloader.Downloader : null);
		PageFunctionBase content = ((navigationService != null && navigationService.ContentId == base.ContentId) ? ((PageFunctionBase)navigationService.Content) : ResumePageFunction());
		return navigator.Navigate(content, new NavigateInfo(base.Source, navMode, this));
	}
}
