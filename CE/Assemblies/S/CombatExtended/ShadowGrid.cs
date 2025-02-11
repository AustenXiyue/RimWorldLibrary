using Verse;

namespace CombatExtended;

public class ShadowGrid : MapComponent
{
	private readonly short[][] sigGrid;

	private readonly float[][] visGrid;

	private short signature = 1;

	private int lastTick;

	private readonly int sizeX;

	private readonly int sizeZ;

	public ShadowGrid(Map map)
		: base(map)
	{
		sigGrid = new short[map.Size.x][];
		visGrid = new float[map.Size.x][];
		sizeX = map.Size.x;
		sizeZ = map.Size.z;
		for (int i = 0; i < map.Size.x; i++)
		{
			sigGrid[i] = new short[map.Size.z];
			visGrid[i] = new float[map.Size.z];
		}
	}

	public override void MapComponentTick()
	{
		base.MapComponentTick();
	}

	public void Reset()
	{
		signature++;
		lastTick = GenTicks.TicksGame;
	}

	public void Set(int i, int j, float visibility)
	{
		Set(new IntVec3(i, 0, j), visibility);
	}

	public void Set(IntVec3 cell, float visibility)
	{
		if (cell.InBounds(map))
		{
			sigGrid[cell.x][cell.z] = signature;
			visGrid[cell.x][cell.z] = visibility;
		}
	}

	public bool TryGet(int i, int j, out float visibility)
	{
		return TryGet(new IntVec3(i, 0, j), out visibility);
	}

	public bool TryGet(IntVec3 cell, out float visibility)
	{
		if (!cell.InBounds(map) || sigGrid[cell.x][cell.z] != signature)
		{
			visibility = 0f;
			return false;
		}
		visibility = visGrid[cell.x][cell.z];
		return true;
	}
}
