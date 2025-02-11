namespace System.Windows.Shell;

/// <summary>Represents a collection of <see cref="T:System.Windows.Shell.ThumbButtonInfo" /> objects that are associated with a <see cref="T:System.Windows.Window" />.</summary>
public class ThumbButtonInfoCollection : FreezableCollection<ThumbButtonInfo>
{
	private static ThumbButtonInfoCollection s_empty;

	internal static ThumbButtonInfoCollection Empty
	{
		get
		{
			if (s_empty == null)
			{
				ThumbButtonInfoCollection thumbButtonInfoCollection = new ThumbButtonInfoCollection();
				thumbButtonInfoCollection.Freeze();
				s_empty = thumbButtonInfoCollection;
			}
			return s_empty;
		}
	}

	/// <summary>Creates a new instance of the collection.</summary>
	/// <returns>The new instance of the collection.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new ThumbButtonInfoCollection();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.ThumbButtonInfoCollection" /> class.</summary>
	public ThumbButtonInfoCollection()
	{
	}
}
