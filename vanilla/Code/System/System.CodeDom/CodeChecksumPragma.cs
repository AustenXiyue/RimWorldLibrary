namespace System.CodeDom;

/// <summary>Represents a code checksum pragma code entity.  </summary>
[Serializable]
public class CodeChecksumPragma : CodeDirective
{
	private string _fileName;

	/// <summary>Gets or sets the path to the checksum file.</summary>
	/// <returns>The path to the checksum file.</returns>
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

	/// <summary>Gets or sets a GUID that identifies the checksum algorithm to use.</summary>
	/// <returns>A <see cref="T:System.Guid" /> that identifies the checksum algorithm to use.</returns>
	public Guid ChecksumAlgorithmId { get; set; }

	/// <summary>Gets or sets the value of the data for the checksum calculation.</summary>
	/// <returns>A byte array that contains the data for the checksum calculation.</returns>
	public byte[] ChecksumData { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeChecksumPragma" /> class. </summary>
	public CodeChecksumPragma()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeChecksumPragma" /> class using a file name, a GUID representing the checksum algorithm, and a byte stream representing the checksum data.</summary>
	/// <param name="fileName">The path to the checksum file.</param>
	/// <param name="checksumAlgorithmId">A <see cref="T:System.Guid" /> that identifies the checksum algorithm to use.</param>
	/// <param name="checksumData">A byte array that contains the checksum data.</param>
	public CodeChecksumPragma(string fileName, Guid checksumAlgorithmId, byte[] checksumData)
	{
		_fileName = fileName;
		ChecksumAlgorithmId = checksumAlgorithmId;
		ChecksumData = checksumData;
	}
}
