using System.Collections;
using System.Collections.Specialized;

namespace System.CodeDom.Compiler;

/// <summary>Represents a set of options used by a code generator.</summary>
public class CodeGeneratorOptions
{
	private readonly IDictionary _options = new ListDictionary();

	/// <summary>Gets or sets the object at the specified index.</summary>
	/// <returns>The object associated with the specified name. If no object associated with the specified name exists in the collection, null.</returns>
	/// <param name="index">The name associated with the object to retrieve. </param>
	public object this[string index]
	{
		get
		{
			return _options[index];
		}
		set
		{
			_options[index] = value;
		}
	}

	/// <summary>Gets or sets the string to use for indentations.</summary>
	/// <returns>A string containing the characters to use for indentations.</returns>
	public string IndentString
	{
		get
		{
			object obj = _options["IndentString"];
			if (obj == null)
			{
				return "    ";
			}
			return (string)obj;
		}
		set
		{
			_options["IndentString"] = value;
		}
	}

	/// <summary>Gets or sets the style to use for bracing.</summary>
	/// <returns>A string containing the bracing style to use.</returns>
	public string BracingStyle
	{
		get
		{
			object obj = _options["BracingStyle"];
			if (obj == null)
			{
				return "Block";
			}
			return (string)obj;
		}
		set
		{
			_options["BracingStyle"] = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to append an else, catch, or finally block, including brackets, at the closing line of each previous if or try block.</summary>
	/// <returns>true if an else should be appended; otherwise, false. The default value of this property is false.</returns>
	public bool ElseOnClosing
	{
		get
		{
			object obj = _options["ElseOnClosing"];
			if (obj == null)
			{
				return false;
			}
			return (bool)obj;
		}
		set
		{
			_options["ElseOnClosing"] = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to insert blank lines between members.</summary>
	/// <returns>true if blank lines should be inserted; otherwise, false. By default, the value of this property is true.</returns>
	public bool BlankLinesBetweenMembers
	{
		get
		{
			object obj = _options["BlankLinesBetweenMembers"];
			if (obj == null)
			{
				return true;
			}
			return (bool)obj;
		}
		set
		{
			_options["BlankLinesBetweenMembers"] = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to generate members in the order in which they occur in member collections.</summary>
	/// <returns>true to generate the members in the order in which they occur in the member collection; otherwise, false. The default value of this property is false.</returns>
	public bool VerbatimOrder
	{
		get
		{
			object obj = _options["VerbatimOrder"];
			if (obj == null)
			{
				return false;
			}
			return (bool)obj;
		}
		set
		{
			_options["VerbatimOrder"] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> class.</summary>
	public CodeGeneratorOptions()
	{
	}
}
