using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Defines the out-of-band data for a call.</summary>
[Serializable]
[ComVisible(true)]
public class Header
{
	/// <summary>Indicates the XML namespace that the current <see cref="T:System.Runtime.Remoting.Messaging.Header" /> belongs to.</summary>
	public string HeaderNamespace;

	/// <summary>Indicates whether the receiving end must understand the out-of-band data.</summary>
	public bool MustUnderstand;

	/// <summary>Contains the name of the <see cref="T:System.Runtime.Remoting.Messaging.Header" />.</summary>
	public string Name;

	/// <summary>Contains the value for the <see cref="T:System.Runtime.Remoting.Messaging.Header" />.</summary>
	public object Value;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.Header" /> class with the given name and value.</summary>
	/// <param name="_Name">The name of the <see cref="T:System.Runtime.Remoting.Messaging.Header" />. </param>
	/// <param name="_Value">The object that contains the value for the <see cref="T:System.Runtime.Remoting.Messaging.Header" />. </param>
	public Header(string _Name, object _Value)
		: this(_Name, _Value, _MustUnderstand: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.Header" /> class with the given name, value, and additional configuration information.</summary>
	/// <param name="_Name">The name of the <see cref="T:System.Runtime.Remoting.Messaging.Header" />. </param>
	/// <param name="_Value">The object that contains the value for the <see cref="T:System.Runtime.Remoting.Messaging.Header" />. </param>
	/// <param name="_MustUnderstand">Indicates whether the receiving end must understand the out-of-band data. </param>
	public Header(string _Name, object _Value, bool _MustUnderstand)
		: this(_Name, _Value, _MustUnderstand, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.Header" /> class.</summary>
	/// <param name="_Name">The name of the <see cref="T:System.Runtime.Remoting.Messaging.Header" />. </param>
	/// <param name="_Value">The object that contains the value of the <see cref="T:System.Runtime.Remoting.Messaging.Header" />. </param>
	/// <param name="_MustUnderstand">Indicates whether the receiving end must understand out-of-band data. </param>
	/// <param name="_HeaderNamespace">The <see cref="T:System.Runtime.Remoting.Messaging.Header" /> XML namespace. </param>
	public Header(string _Name, object _Value, bool _MustUnderstand, string _HeaderNamespace)
	{
		Name = _Name;
		Value = _Value;
		MustUnderstand = _MustUnderstand;
		HeaderNamespace = _HeaderNamespace;
	}
}
