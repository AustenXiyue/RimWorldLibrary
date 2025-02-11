namespace System.Windows.Data;

/// <summary>Represents an attribute that allows the author of a value converter to specify the data types involved in the implementation of the converter.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ValueConversionAttribute : Attribute
{
	private Type _sourceType;

	private Type _targetType;

	private Type _parameterType;

	/// <summary>Gets the type this converter converts.</summary>
	/// <returns>The type this converter converts.</returns>
	public Type SourceType => _sourceType;

	/// <summary>Gets the type this converter converts to.</summary>
	/// <returns>The type this converter converts to.</returns>
	public Type TargetType => _targetType;

	/// <summary>Gets and sets the type of the optional value converter parameter object.</summary>
	/// <returns>The type of the optional value converter parameter object.</returns>
	public Type ParameterType
	{
		get
		{
			return _parameterType;
		}
		set
		{
			_parameterType = value;
		}
	}

	/// <summary>Gets the unique identifier of this <see cref="T:System.Windows.Data.ValueConversionAttribute" /> instance.</summary>
	/// <returns>The unique identifier of this <see cref="T:System.Windows.Data.ValueConversionAttribute" /> instance.</returns>
	public override object TypeId => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.ValueConversionAttribute" /> class with the specified source type and target type.</summary>
	/// <param name="sourceType">The type this converter converts.</param>
	/// <param name="targetType">The type this converter converts to.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="sourceType" /> parameter cannot be null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="targetType" /> parameter cannot be null.</exception>
	public ValueConversionAttribute(Type sourceType, Type targetType)
	{
		if (sourceType == null)
		{
			throw new ArgumentNullException("sourceType");
		}
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		_sourceType = sourceType;
		_targetType = targetType;
	}

	/// <summary>Returns the hash code for this instance of <see cref="T:System.Windows.Data.ValueConversionAttribute" />.</summary>
	/// <returns>The hash code for this instance of <see cref="T:System.Windows.Data.ValueConversionAttribute" />.</returns>
	public override int GetHashCode()
	{
		return _sourceType.GetHashCode() + _targetType.GetHashCode();
	}
}
