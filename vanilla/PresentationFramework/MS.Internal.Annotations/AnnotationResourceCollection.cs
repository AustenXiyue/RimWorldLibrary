using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Annotations;

namespace MS.Internal.Annotations;

internal sealed class AnnotationResourceCollection : AnnotationObservableCollection<AnnotationResource>
{
	public event PropertyChangedEventHandler ItemChanged;

	protected override void ProtectedClearItems()
	{
		List<AnnotationResource> list = new List<AnnotationResource>(this);
		base.Items.Clear();
		OnPropertyChanged(CountString);
		OnPropertyChanged(IndexerName);
		OnCollectionCleared(list);
	}

	protected override void ProtectedSetItem(int index, AnnotationResource item)
	{
		ObservableCollectionSetItem(index, item);
	}

	private void OnCollectionCleared(IEnumerable<AnnotationResource> list)
	{
		foreach (AnnotationResource item in list)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, 0));
		}
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (this.ItemChanged != null)
		{
			this.ItemChanged(sender, e);
		}
	}
}
