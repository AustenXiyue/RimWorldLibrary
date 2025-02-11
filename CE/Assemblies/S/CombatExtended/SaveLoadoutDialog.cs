using System;
using System.IO;
using Verse;

namespace CombatExtended;

internal class SaveLoadoutDialog : FileListDialog
{
	protected override bool ShouldDoTypeInField => true;

	internal SaveLoadoutDialog(string storageTypeName, Action<FileInfo, FileListDialog> fileAction, string initialSaveFilename)
		: base(storageTypeName, fileAction)
	{
		interactButLabel = "OverwriteButton".Translate();
		typingName = initialSaveFilename;
	}
}
