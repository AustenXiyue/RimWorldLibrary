using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace MS.Internal.Documents.Application;

[Serializable]
internal sealed class DocumentApplicationJournalEntry : CustomContentState
{
	private object _state;

	private string _displayName;

	public override string JournalEntryName => _displayName;

	public DocumentApplicationJournalEntry(object state, string name)
	{
		Invariant.Assert(state is DocumentApplicationState, "state should be of type DocumentApplicationState");
		_state = state;
		_displayName = name;
	}

	public DocumentApplicationJournalEntry(object state)
		: this(state, null)
	{
	}

	public override void Replay(NavigationService navigationService, NavigationMode mode)
	{
		ContentControl contentControl = (ContentControl)navigationService.INavigatorHost;
		contentControl.ApplyTemplate();
		if (contentControl.Template.FindName("PUIDocumentApplicationDocumentViewer", contentControl) is DocumentApplicationDocumentViewer documentApplicationDocumentViewer)
		{
			if (_state is DocumentApplicationState)
			{
				documentApplicationDocumentViewer.StoredDocumentApplicationState = (DocumentApplicationState)_state;
			}
			if (navigationService.Content != null && navigationService.Content is IDocumentPaginatorSource documentPaginatorSource && documentPaginatorSource.DocumentPaginator.IsPageCountValid)
			{
				documentApplicationDocumentViewer.SetUIToStoredState();
			}
		}
	}
}
