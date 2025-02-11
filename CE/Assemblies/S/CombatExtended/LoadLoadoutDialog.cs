using System;
using System.IO;
using Verse;

namespace CombatExtended;

internal class LoadLoadoutDialog : FileListDialog
{
	protected override bool ShouldDoTypeInField => false;

	internal LoadLoadoutDialog(string storageTypeName, Action<FileInfo, FileListDialog> fileAction)
		: base(storageTypeName, fileAction)
	{
		interactButLabel = "LoadGameButton".Translate();
	}
}
