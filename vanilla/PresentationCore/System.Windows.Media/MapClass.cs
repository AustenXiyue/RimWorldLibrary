using System.Windows.Media.Composition;

namespace System.Windows.Media;

internal class MapClass
{
	public DUCE.Map<bool> _map_ofBrushes;

	internal bool IsEmpty => _map_ofBrushes.IsEmpty();

	internal MapClass()
	{
		_map_ofBrushes = default(DUCE.Map<bool>);
	}
}
