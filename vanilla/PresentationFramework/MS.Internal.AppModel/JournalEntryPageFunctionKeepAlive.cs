using System.Windows.Navigation;

namespace MS.Internal.AppModel;

internal class JournalEntryPageFunctionKeepAlive : JournalEntryPageFunction
{
	private PageFunctionBase _keepAlivePageFunction;

	internal PageFunctionBase KeepAlivePageFunction => _keepAlivePageFunction;

	internal JournalEntryPageFunctionKeepAlive(JournalEntryGroupState jeGroupState, PageFunctionBase pageFunction)
		: base(jeGroupState, pageFunction)
	{
		_keepAlivePageFunction = pageFunction;
	}

	internal override bool IsPageFunction()
	{
		return true;
	}

	internal override bool IsAlive()
	{
		return KeepAlivePageFunction != null;
	}

	internal override PageFunctionBase ResumePageFunction()
	{
		PageFunctionBase keepAlivePageFunction = KeepAlivePageFunction;
		keepAlivePageFunction._Resume = true;
		return keepAlivePageFunction;
	}

	internal override void SaveState(object contentObject)
	{
		Invariant.Assert(_keepAlivePageFunction == contentObject);
	}

	internal override bool Navigate(INavigator navigator, NavigationMode navMode)
	{
		PageFunctionBase content = ((navigator.Content == _keepAlivePageFunction) ? _keepAlivePageFunction : ResumePageFunction());
		return navigator.Navigate(content, new NavigateInfo(base.Source, navMode, this));
	}
}
