using UnityEngine;
using Verse;

namespace AncotLibrary;

[StaticConstructorOnStartup]
public class AncotLibraryIcon
{
	public static readonly Texture2D Illustration = ContentFinder<Texture2D>.Get("AncotLibrary/Icon/Illustration");

	public static readonly Texture2D SwitchA = ContentFinder<Texture2D>.Get("AncotLibrary/Gizmos/SwitchA");

	public static readonly Texture2D Gun = ContentFinder<Texture2D>.Get("AncotLibrary/Gizmos/Gun");
}
