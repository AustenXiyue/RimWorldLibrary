using System.Runtime.InteropServices;

namespace System.Configuration.Assemblies;

/// <summary>Represents a hash of an assembly manifest's contents.</summary>
[Serializable]
[Obsolete]
[ComVisible(true)]
public struct AssemblyHash : ICloneable
{
	private AssemblyHashAlgorithm _algorithm;

	private byte[] _value;

	/// <summary>An empty <see cref="T:System.Configuration.Assemblies.AssemblyHash" /> object.</summary>
	[Obsolete]
	public static readonly AssemblyHash Empty = new AssemblyHash(AssemblyHashAlgorithm.None, null);

	/// <summary>Gets or sets the hash algorithm.</summary>
	/// <returns>An assembly hash algorithm.</returns>
	[Obsolete]
	public AssemblyHashAlgorithm Algorithm
	{
		get
		{
			return _algorithm;
		}
		set
		{
			_algorithm = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.Assemblies.AssemblyHash" /> structure with the specified hash algorithm and the hash value.</summary>
	/// <param name="algorithm">The algorithm used to generate the hash. Values for this parameter come from the <see cref="T:System.Configuration.Assemblies.AssemblyHashAlgorithm" /> enumeration. </param>
	/// <param name="value">The hash value. </param>
	[Obsolete]
	public AssemblyHash(AssemblyHashAlgorithm algorithm, byte[] value)
	{
		_algorithm = algorithm;
		if (value != null)
		{
			_value = (byte[])value.Clone();
		}
		else
		{
			_value = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.Assemblies.AssemblyHash" /> structure with the specified hash value. The hash algorithm defaults to <see cref="F:System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA1" />.</summary>
	/// <param name="value">The hash value. </param>
	[Obsolete]
	public AssemblyHash(byte[] value)
		: this(AssemblyHashAlgorithm.SHA1, value)
	{
	}

	/// <summary>Clones this object.</summary>
	/// <returns>An exact copy of this object.</returns>
	[Obsolete]
	public object Clone()
	{
		return new AssemblyHash(_algorithm, _value);
	}

	/// <summary>Gets the hash value.</summary>
	/// <returns>The hash value.</returns>
	[Obsolete]
	public byte[] GetValue()
	{
		return _value;
	}

	/// <summary>Sets the hash value.</summary>
	/// <param name="value">The hash value. </param>
	[Obsolete]
	public void SetValue(byte[] value)
	{
		_value = value;
	}
}
