namespace System.CodeDom;

/// <summary>Specifies the name and mode for a code region.</summary>
[Serializable]
public class CodeRegionDirective : CodeDirective
{
	private string _regionText;

	/// <summary>Gets or sets the name of the region.</summary>
	/// <returns>The name of the region.</returns>
	public string RegionText
	{
		get
		{
			return _regionText ?? string.Empty;
		}
		set
		{
			_regionText = value;
		}
	}

	/// <summary>Gets or sets the mode for the region directive.</summary>
	/// <returns>One of the <see cref="T:System.CodeDom.CodeRegionMode" /> values. The default is <see cref="F:System.CodeDom.CodeRegionMode.None" />.</returns>
	public CodeRegionMode RegionMode { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeRegionDirective" /> class with default values. </summary>
	public CodeRegionDirective()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeRegionDirective" /> class, specifying its mode and name. </summary>
	/// <param name="regionMode">One of the <see cref="T:System.CodeDom.CodeRegionMode" /> values.</param>
	/// <param name="regionText">The name for the region.</param>
	public CodeRegionDirective(CodeRegionMode regionMode, string regionText)
	{
		RegionText = regionText;
		RegionMode = regionMode;
	}
}
