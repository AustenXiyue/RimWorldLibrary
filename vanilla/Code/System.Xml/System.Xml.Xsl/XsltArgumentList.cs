using System.Collections;

namespace System.Xml.Xsl;

/// <summary>Contains a variable number of arguments which are either XSLT parameters or extension objects.</summary>
public class XsltArgumentList
{
	private Hashtable parameters = new Hashtable();

	private Hashtable extensions = new Hashtable();

	internal XsltMessageEncounteredEventHandler xsltMessageEncountered;

	/// <summary>Occurs when a message is specified in the style sheet by the xsl:message element. </summary>
	public event XsltMessageEncounteredEventHandler XsltMessageEncountered
	{
		add
		{
			xsltMessageEncountered = (XsltMessageEncounteredEventHandler)Delegate.Combine(xsltMessageEncountered, value);
		}
		remove
		{
			xsltMessageEncountered = (XsltMessageEncounteredEventHandler)Delegate.Remove(xsltMessageEncountered, value);
		}
	}

	/// <summary>Implements a new instance of the <see cref="T:System.Xml.Xsl.XsltArgumentList" />.</summary>
	public XsltArgumentList()
	{
	}

	/// <summary>Gets the parameter associated with the namespace qualified name.</summary>
	/// <returns>The parameter object or null if one was not found.</returns>
	/// <param name="name">The name of the parameter. <see cref="T:System.Xml.Xsl.XsltArgumentList" /> does not check to ensure the name passed is a valid local name; however, the name cannot be null. </param>
	/// <param name="namespaceUri">The namespace URI associated with the parameter. </param>
	public object GetParam(string name, string namespaceUri)
	{
		return parameters[new XmlQualifiedName(name, namespaceUri)];
	}

	/// <summary>Gets the object associated with the given namespace.</summary>
	/// <returns>The namespace URI object or null if one was not found.</returns>
	/// <param name="namespaceUri">The namespace URI of the object. </param>
	public object GetExtensionObject(string namespaceUri)
	{
		return extensions[namespaceUri];
	}

	/// <summary>Adds a parameter to the <see cref="T:System.Xml.Xsl.XsltArgumentList" /> and associates it with the namespace qualified name.</summary>
	/// <param name="name">The name to associate with the parameter. </param>
	/// <param name="namespaceUri">The namespace URI to associate with the parameter. To use the default namespace, specify an empty string. </param>
	/// <param name="parameter">The parameter value or object to add to the list. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="namespaceUri" /> is either null or http://www.w3.org/1999/XSL/Transform.The <paramref name="name" /> is not a valid name according to the W3C XML specification.The <paramref name="namespaceUri" /> already has a parameter associated with it. </exception>
	public void AddParam(string name, string namespaceUri, object parameter)
	{
		CheckArgumentNull(name, "name");
		CheckArgumentNull(namespaceUri, "namespaceUri");
		CheckArgumentNull(parameter, "parameter");
		XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(name, namespaceUri);
		xmlQualifiedName.Verify();
		parameters.Add(xmlQualifiedName, parameter);
	}

	/// <summary>Adds a new object to the <see cref="T:System.Xml.Xsl.XsltArgumentList" /> and associates it with the namespace URI.</summary>
	/// <param name="namespaceUri">The namespace URI to associate with the object. To use the default namespace, specify an empty string. </param>
	/// <param name="extension">The object to add to the list. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="namespaceUri" /> is either null or http://www.w3.org/1999/XSL/Transform The <paramref name="namespaceUri" /> already has an extension object associated with it. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have sufficient permissions to call this method. </exception>
	public void AddExtensionObject(string namespaceUri, object extension)
	{
		CheckArgumentNull(namespaceUri, "namespaceUri");
		CheckArgumentNull(extension, "extension");
		extensions.Add(namespaceUri, extension);
	}

	/// <summary>Removes the parameter from the <see cref="T:System.Xml.Xsl.XsltArgumentList" />.</summary>
	/// <returns>The parameter object or null if one was not found.</returns>
	/// <param name="name">The name of the parameter to remove. <see cref="T:System.Xml.Xsl.XsltArgumentList" /> does not check to ensure the name passed is a valid local name; however, the name cannot be null. </param>
	/// <param name="namespaceUri">The namespace URI of the parameter to remove. </param>
	public object RemoveParam(string name, string namespaceUri)
	{
		XmlQualifiedName key = new XmlQualifiedName(name, namespaceUri);
		object result = parameters[key];
		parameters.Remove(key);
		return result;
	}

	/// <summary>Removes the object with the namespace URI from the <see cref="T:System.Xml.Xsl.XsltArgumentList" />.</summary>
	/// <returns>The object with the namespace URI or null if one was not found.</returns>
	/// <param name="namespaceUri">The namespace URI associated with the object to remove. </param>
	public object RemoveExtensionObject(string namespaceUri)
	{
		object result = extensions[namespaceUri];
		extensions.Remove(namespaceUri);
		return result;
	}

	/// <summary>Removes all parameters and extension objects from the <see cref="T:System.Xml.Xsl.XsltArgumentList" />.</summary>
	public void Clear()
	{
		parameters.Clear();
		extensions.Clear();
		xsltMessageEncountered = null;
	}

	private static void CheckArgumentNull(object param, string paramName)
	{
		if (param == null)
		{
			throw new ArgumentNullException(paramName);
		}
	}
}
