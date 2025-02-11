namespace System.Windows.Navigation;

internal class PageFunctionReturnInfo : NavigateInfo
{
	private object _returnEventArgs;

	private PageFunctionBase _finishingChildPageFunction;

	internal object ReturnEventArgs => _returnEventArgs;

	internal PageFunctionBase FinishingChildPageFunction => _finishingChildPageFunction;

	internal PageFunctionReturnInfo(PageFunctionBase finishingChildPageFunction, Uri source, NavigationMode navigationMode, JournalEntry journalEntry, object returnEventArgs)
		: base(source, navigationMode, journalEntry)
	{
		_returnEventArgs = returnEventArgs;
		_finishingChildPageFunction = finishingChildPageFunction;
	}
}
