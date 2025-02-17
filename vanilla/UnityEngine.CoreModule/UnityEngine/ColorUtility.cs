using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Runtime/Export/Math/ColorUtility.bindings.h")]
public class ColorUtility
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction]
	internal static extern bool DoTryParseHtmlColor(string htmlString, out Color32 color);

	public static bool TryParseHtmlString(string htmlString, out Color color)
	{
		Color32 color2;
		bool result = DoTryParseHtmlColor(htmlString, out color2);
		color = color2;
		return result;
	}

	public static string ToHtmlStringRGB(Color color)
	{
		Color32 color2 = new Color32((byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255), 1);
		return UnityString.Format("{0:X2}{1:X2}{2:X2}", color2.r, color2.g, color2.b);
	}

	public static string ToHtmlStringRGBA(Color color)
	{
		Color32 color2 = new Color32((byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.a * 255f), 0, 255));
		return UnityString.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color2.r, color2.g, color2.b, color2.a);
	}
}
