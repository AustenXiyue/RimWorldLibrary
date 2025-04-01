namespace System.CodeDom;

/// <summary>Represents a specific location within a specific file.</summary>
[Serializable]
public class CodeLinePragma
{
	private string _fileName;

	/// <summary>Gets or sets the name of the associated file.</summary>
	/// <returns>The file name of the associated file.</returns>
	public string FileName
	{
		get
		{
			return _fileName ?? string.Empty;
		}
		set
		{
			_fileName = value;
		}
	}

	/// <summary>Gets or sets the line number of the associated reference.</summary>
	/// <returns>The line number.</returns>
	public int LineNumber { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeLinePragma" /> class. </summary>
	public CodeLinePragma()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeLinePragma" /> class.</summary>
	/// <param name="fileName">The file name of the associated file. </param>
	/// <param name="lineNumber">The line number to store a reference to. </param>
	public CodeLinePragma(string fileName, int lineNumber)
	{
		FileName = fileName;
		LineNumber = lineNumber;
	}
}
