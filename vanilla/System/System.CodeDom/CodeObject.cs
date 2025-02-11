using System.Collections;
using System.Collections.Specialized;

namespace System.CodeDom;

/// <summary>Provides a common base class for most Code Document Object Model (CodeDOM) objects.</summary>
[Serializable]
public class CodeObject
{
	private IDictionary _userData;

	/// <summary>Gets the user-definable data for the current object.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> containing user data for the current object.</returns>
	public IDictionary UserData => _userData ?? (_userData = new ListDictionary());

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeObject" /> class.</summary>
	public CodeObject()
	{
	}
}
