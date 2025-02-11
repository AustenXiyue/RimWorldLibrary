using System.ComponentModel;

namespace System.Windows.Data;

/// <summary>Represents a method that is used to provide custom logic to select the <see cref="T:System.ComponentModel.GroupDescription" /> based on the parent group and its level.</summary>
/// <returns>The <see cref="T:System.ComponentModel.GroupDescription" /> chosen based on the parent group and its level. </returns>
/// <param name="group">The parent group.</param>
/// <param name="level">The level of <paramref name="group" />.</param>
public delegate GroupDescription GroupDescriptionSelectorCallback(CollectionViewGroup group, int level);
