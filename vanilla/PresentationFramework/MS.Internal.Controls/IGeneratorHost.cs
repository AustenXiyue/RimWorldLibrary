using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MS.Internal.Controls;

internal interface IGeneratorHost
{
	ItemCollection View { get; }

	int AlternationCount { get; }

	bool IsItemItsOwnContainer(object item);

	DependencyObject GetContainerForItem(object item);

	void PrepareItemContainer(DependencyObject container, object item);

	void ClearContainerForItem(DependencyObject container, object item);

	bool IsHostForItemContainer(DependencyObject container);

	GroupStyle GetGroupStyle(CollectionViewGroup group, int level);

	void SetIsGrouping(bool isGrouping);
}
