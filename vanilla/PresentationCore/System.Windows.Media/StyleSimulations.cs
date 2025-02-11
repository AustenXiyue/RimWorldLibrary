namespace System.Windows.Media;

/// <summary>Defines an enumerator class that describes the simulation style of a font.</summary>
[Flags]
public enum StyleSimulations
{
	/// <summary>No font style simulation.</summary>
	None = 0,
	/// <summary>Bold style simulation.</summary>
	BoldSimulation = 1,
	/// <summary>Italic style simulation.</summary>
	ItalicSimulation = 2,
	/// <summary>Bold and Italic style simulation.</summary>
	BoldItalicSimulation = 3
}
