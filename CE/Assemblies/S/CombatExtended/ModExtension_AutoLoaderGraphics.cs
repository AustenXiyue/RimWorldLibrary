using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class ModExtension_AutoLoaderGraphics : DefModExtension
{
	public GraphicData fullGraphic;

	public GraphicData halfFullGraphic;

	public GraphicData emptyGraphic;

	public SoundDef reloadingSustainer;

	public SoundDef reloadCompleteSound;

	public List<string> allowedTurrets = new List<string>();

	public List<string> allowedTurretTags = new List<string>();
}
