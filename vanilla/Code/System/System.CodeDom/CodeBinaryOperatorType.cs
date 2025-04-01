namespace System.CodeDom;

/// <summary>Defines identifiers for supported binary operators.</summary>
public enum CodeBinaryOperatorType
{
	/// <summary>Addition operator.</summary>
	Add,
	/// <summary>Subtraction operator.</summary>
	Subtract,
	/// <summary>Multiplication operator.</summary>
	Multiply,
	/// <summary>Division operator.</summary>
	Divide,
	/// <summary>Modulus operator.</summary>
	Modulus,
	/// <summary>Assignment operator.</summary>
	Assign,
	/// <summary>Identity not equal operator.</summary>
	IdentityInequality,
	/// <summary>Identity equal operator.</summary>
	IdentityEquality,
	/// <summary>Value equal operator.</summary>
	ValueEquality,
	/// <summary>Bitwise or operator.</summary>
	BitwiseOr,
	/// <summary>Bitwise and operator.</summary>
	BitwiseAnd,
	/// <summary>Boolean or operator. This represents a short circuiting operator. A short circuiting operator will evaluate only as many expressions as necessary before returning a correct value.</summary>
	BooleanOr,
	/// <summary>Boolean and operator. This represents a short circuiting operator. A short circuiting operator will evaluate only as many expressions as necessary before returning a correct value.</summary>
	BooleanAnd,
	/// <summary>Less than operator.</summary>
	LessThan,
	/// <summary>Less than or equal operator.</summary>
	LessThanOrEqual,
	/// <summary>Greater than operator.</summary>
	GreaterThan,
	/// <summary>Greater than or equal operator.</summary>
	GreaterThanOrEqual
}
