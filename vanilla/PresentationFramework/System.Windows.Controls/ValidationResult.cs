namespace System.Windows.Controls;

/// <summary>Represents the result returned by the <see cref="T:System.Windows.Controls.ValidationRule" />.<see cref="M:System.Windows.Controls.ValidationRule.Validate(System.Object,System.Globalization.CultureInfo)" /> method that indicates whether the checked value passed the <see cref="T:System.Windows.Controls.ValidationRule" />.</summary>
public class ValidationResult
{
	private bool _isValid;

	private object _errorContent;

	private static readonly ValidationResult s_valid = new ValidationResult(isValid: true, null);

	/// <summary>Gets a value that indicates whether the value checked against the <see cref="T:System.Windows.Controls.ValidationRule" /> is valid.</summary>
	/// <returns>true if the value is valid; otherwise, false. The default value is false.</returns>
	public bool IsValid => _isValid;

	/// <summary>Gets an object that provides additional information about the invalidity.</summary>
	/// <returns>An object that provides additional information about the invalidity.</returns>
	public object ErrorContent => _errorContent;

	/// <summary>Gets a valid instance of <see cref="T:System.Windows.Controls.ValidationResult" />.</summary>
	/// <returns>A valid instance of <see cref="T:System.Windows.Controls.ValidationResult" />.</returns>
	public static ValidationResult ValidResult => s_valid;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ValidationResult" /> class.</summary>
	/// <param name="isValid">Whether or not the value checked against the <see cref="T:System.Windows.Controls.ValidationRule" /> is valid.</param>
	/// <param name="errorContent">Information about the invalidity.</param>
	public ValidationResult(bool isValid, object errorContent)
	{
		_isValid = isValid;
		_errorContent = errorContent;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Controls.ValidationResult" /> objects for value equality.</summary>
	/// <returns>true if the two objects are equal; otherwise, false.</returns>
	/// <param name="left">The first instance to compare.</param>
	/// <param name="right">The second instance to compare.</param>
	public static bool operator ==(ValidationResult left, ValidationResult right)
	{
		return object.Equals(left, right);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Controls.ValidationResult" /> objects for value inequality.</summary>
	/// <returns>true if the values are not equal; otherwise, false.</returns>
	/// <param name="left">The first instance to compare.</param>
	/// <param name="right">The second instance to compare.</param>
	public static bool operator !=(ValidationResult left, ValidationResult right)
	{
		return !object.Equals(left, right);
	}

	/// <summary>Compares the specified instance and the current instance of <see cref="T:System.Windows.Controls.ValidationResult" /> for value equality.</summary>
	/// <returns>true if <paramref name="obj" /> and this instance of <see cref="T:System.Windows.Controls.ValidationResult" />.have the same values.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.Controls.ValidationResult" /> instance to compare.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		ValidationResult validationResult = obj as ValidationResult;
		if (validationResult != null)
		{
			if (IsValid == validationResult.IsValid)
			{
				return ErrorContent == validationResult.ErrorContent;
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.Controls.ValidationResult" />.</summary>
	/// <returns>The hash code for this <see cref="T:System.Windows.Controls.ValidationResult" />.</returns>
	public override int GetHashCode()
	{
		return IsValid.GetHashCode() ^ ((ErrorContent == null) ? ((object)int.MinValue) : ErrorContent).GetHashCode();
	}
}
