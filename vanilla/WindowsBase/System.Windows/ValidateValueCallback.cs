namespace System.Windows;

/// <summary>Represents a method used as a callback that validates the effective value of a  dependency property.</summary>
/// <returns>true if the value was validated; false if the submitted value was invalid.</returns>
/// <param name="value">The value to be validated.</param>
public delegate bool ValidateValueCallback(object value);
