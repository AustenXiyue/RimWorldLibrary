using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Provides access to the metadata and MSIL for the body of a method.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public class MethodBody
{
	private ExceptionHandlingClause[] clauses;

	private LocalVariableInfo[] locals;

	private byte[] il;

	private bool init_locals;

	private int sig_token;

	private int max_stack;

	/// <summary>Gets a list that includes all the exception-handling clauses in the method body.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IList`1" /> of <see cref="T:System.Reflection.ExceptionHandlingClause" /> objects representing the exception-handling clauses in the body of the method.</returns>
	public virtual IList<ExceptionHandlingClause> ExceptionHandlingClauses => Array.AsReadOnly(clauses);

	/// <summary>Gets the list of local variables declared in the method body.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IList`1" /> of <see cref="T:System.Reflection.LocalVariableInfo" /> objects that describe the local variables declared in the method body.</returns>
	public virtual IList<LocalVariableInfo> LocalVariables => Array.AsReadOnly(locals);

	/// <summary>Gets a value indicating whether local variables in the method body are initialized to the default values for their types.</summary>
	/// <returns>true if the method body contains code to initialize local variables to null for reference types, or to the zero-initialized value for value types; otherwise, false.</returns>
	public virtual bool InitLocals => init_locals;

	/// <summary>Gets a metadata token for the signature that describes the local variables for the method in metadata.</summary>
	/// <returns>An integer that represents the metadata token.</returns>
	public virtual int LocalSignatureMetadataToken => sig_token;

	/// <summary>Gets the maximum number of items on the operand stack when the method is executing.</summary>
	/// <returns>The maximum number of items on the operand stack when the method is executing.</returns>
	public virtual int MaxStackSize => max_stack;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.MethodBody" /> class.</summary>
	protected MethodBody()
	{
	}

	/// <summary>Returns the MSIL for the method body, as an array of bytes.</summary>
	/// <returns>An array of type <see cref="T:System.Byte" /> that contains the MSIL for the method body. </returns>
	public virtual byte[] GetILAsByteArray()
	{
		return il;
	}
}
