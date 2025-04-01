namespace System.Runtime.CompilerServices;

/// <summary>Specifies whether to wrap exceptions that do not derive from the <see cref="T:System.Exception" /> class with a <see cref="T:System.Runtime.CompilerServices.RuntimeWrappedException" /> object. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class RuntimeCompatibilityAttribute : Attribute
{
	private bool m_wrapNonExceptionThrows;

	/// <summary>Gets or sets a value that indicates whether to wrap exceptions that do not derive from the <see cref="T:System.Exception" /> class with a <see cref="T:System.Runtime.CompilerServices.RuntimeWrappedException" /> object.</summary>
	/// <returns>true if exceptions that do not derive from the <see cref="T:System.Exception" /> class should appear wrapped with a <see cref="T:System.Runtime.CompilerServices.RuntimeWrappedException" /> object; otherwise, false.</returns>
	public bool WrapNonExceptionThrows
	{
		get
		{
			return m_wrapNonExceptionThrows;
		}
		set
		{
			m_wrapNonExceptionThrows = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.RuntimeCompatibilityAttribute" /> class. </summary>
	public RuntimeCompatibilityAttribute()
	{
	}
}
