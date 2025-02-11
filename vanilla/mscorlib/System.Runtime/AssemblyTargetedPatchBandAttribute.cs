namespace System.Runtime;

/// <summary>Specifies patch band information for targeted patching of the .NET Framework.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyTargetedPatchBandAttribute : Attribute
{
	private string m_targetedPatchBand;

	/// <summary>Gets the patch band. </summary>
	/// <returns>The patch band information.</returns>
	public string TargetedPatchBand => m_targetedPatchBand;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.AssemblyTargetedPatchBandAttribute" /> class.</summary>
	/// <param name="targetedPatchBand">The patch band.</param>
	public AssemblyTargetedPatchBandAttribute(string targetedPatchBand)
	{
		m_targetedPatchBand = targetedPatchBand;
	}
}
