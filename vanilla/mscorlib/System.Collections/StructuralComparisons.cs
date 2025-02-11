namespace System.Collections;

/// <summary>Provides objects for performing a structural comparison of two collection objects.</summary>
public static class StructuralComparisons
{
	private static volatile IComparer s_StructuralComparer;

	private static volatile IEqualityComparer s_StructuralEqualityComparer;

	/// <summary>Gets a predefined object that performs a structural comparison of two objects.</summary>
	/// <returns>A predefined object that is used to perform a structural comparison of two collection objects.</returns>
	public static IComparer StructuralComparer
	{
		get
		{
			IComparer comparer = s_StructuralComparer;
			if (comparer == null)
			{
				comparer = (s_StructuralComparer = new StructuralComparer());
			}
			return comparer;
		}
	}

	/// <summary>Gets a predefined object that compares two objects for structural equality.</summary>
	/// <returns>A predefined object that is used to compare two collection objects for structural equality.</returns>
	public static IEqualityComparer StructuralEqualityComparer
	{
		get
		{
			IEqualityComparer equalityComparer = s_StructuralEqualityComparer;
			if (equalityComparer == null)
			{
				equalityComparer = (s_StructuralEqualityComparer = new StructuralEqualityComparer());
			}
			return equalityComparer;
		}
	}
}
