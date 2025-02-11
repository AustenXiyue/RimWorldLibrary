using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>Delegate used to select the group style as a function of the parent group and its level.</summary>
/// <returns>The appropriate group style.</returns>
/// <param name="group">Group whose style is to be selected.</param>
/// <param name="level">Level of the group.</param>
public delegate GroupStyle GroupStyleSelector(CollectionViewGroup group, int level);
