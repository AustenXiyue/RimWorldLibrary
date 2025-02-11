using System.IO;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the Base64 decoding transform as defined in Section 6.6.2 of the XMLDSIG specification.</summary>
public class XmlDsigBase64Transform : Transform
{
	private Type[] _inputTypes = new Type[3]
	{
		typeof(Stream),
		typeof(XmlNodeList),
		typeof(XmlDocument)
	};

	private Type[] _outputTypes = new Type[1] { typeof(Stream) };

	private CryptoStream _cs;

	/// <summary>Gets an array of types that are valid inputs to the <see cref="M:System.Security.Cryptography.Xml.XmlDsigBase64Transform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object.</summary>
	/// <returns>An array of valid input types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object; you can pass only objects of one of these types to the <see cref="M:System.Security.Cryptography.Xml.XmlDsigBase64Transform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object.</returns>
	public override Type[] InputTypes => _inputTypes;

	/// <summary>Gets an array of types that are possible outputs from the <see cref="M:System.Security.Cryptography.Xml.XmlDsigBase64Transform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object.</summary>
	/// <returns>An array of valid output types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object; only objects of one of these types are returned from the <see cref="M:System.Security.Cryptography.Xml.XmlDsigBase64Transform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object.</returns>
	public override Type[] OutputTypes => _outputTypes;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> class.</summary>
	public XmlDsigBase64Transform()
	{
		base.Algorithm = "http://www.w3.org/2000/09/xmldsig#base64";
	}

	/// <summary>Parses the specified <see cref="T:System.Xml.XmlNodeList" /> object as transform-specific content of a &lt;Transform&gt; element; this method is not supported because the <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object has no inner XML elements.</summary>
	/// <param name="nodeList">An <see cref="T:System.Xml.XmlNodeList" /> object to load into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object. </param>
	public override void LoadInnerXml(XmlNodeList nodeList)
	{
	}

	/// <summary>Returns an XML representation of the parameters of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object that are suitable to be included as subelements of an XMLDSIG &lt;Transform&gt; element.</summary>
	/// <returns>A list of the XML nodes that represent the transform-specific content needed to describe the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object in an XMLDSIG &lt;Transform&gt; element.</returns>
	protected override XmlNodeList GetInnerXml()
	{
		return null;
	}

	/// <summary>Loads the specified input into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object.</summary>
	/// <param name="obj">The input to load into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="obj" /> parameter is a <see cref="T:System.IO.Stream" /> and it is null. </exception>
	public override void LoadInput(object obj)
	{
		if (obj is Stream)
		{
			LoadStreamInput((Stream)obj);
		}
		else if (obj is XmlNodeList)
		{
			LoadXmlNodeListInput((XmlNodeList)obj);
		}
		else if (obj is XmlDocument)
		{
			LoadXmlNodeListInput(((XmlDocument)obj).SelectNodes("//."));
		}
	}

	private void LoadStreamInput(Stream inputStream)
	{
		if (inputStream == null)
		{
			throw new ArgumentException("obj");
		}
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[1024];
		int num;
		do
		{
			num = inputStream.Read(array, 0, 1024);
			if (num <= 0)
			{
				continue;
			}
			int num2 = 0;
			int i;
			for (i = 0; i < num && !char.IsWhiteSpace((char)array[i]); i++)
			{
			}
			num2 = i;
			for (i++; i < num; i++)
			{
				if (!char.IsWhiteSpace((char)array[i]))
				{
					array[num2] = array[i];
					num2++;
				}
			}
			memoryStream.Write(array, 0, num2);
		}
		while (num > 0);
		memoryStream.Position = 0L;
		_cs = new CryptoStream(memoryStream, new FromBase64Transform(), CryptoStreamMode.Read);
	}

	private void LoadXmlNodeListInput(XmlNodeList nodeList)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (XmlNode node in nodeList)
		{
			XmlNode xmlNode = node.SelectSingleNode("self::text()");
			if (xmlNode != null)
			{
				stringBuilder.Append(xmlNode.OuterXml);
			}
		}
		byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(stringBuilder.ToString());
		int num = 0;
		int i;
		for (i = 0; i < bytes.Length && !char.IsWhiteSpace((char)bytes[i]); i++)
		{
		}
		num = i;
		for (i++; i < bytes.Length; i++)
		{
			if (!char.IsWhiteSpace((char)bytes[i]))
			{
				bytes[num] = bytes[i];
				num++;
			}
		}
		MemoryStream stream = new MemoryStream(bytes, 0, num);
		_cs = new CryptoStream(stream, new FromBase64Transform(), CryptoStreamMode.Read);
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object.</returns>
	public override object GetOutput()
	{
		return _cs;
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object of type <see cref="T:System.IO.Stream" />.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigBase64Transform" /> object of type <see cref="T:System.IO.Stream" />.</returns>
	/// <param name="type">The type of the output to return. <see cref="T:System.IO.Stream" /> is the only valid type for this parameter. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="type" /> parameter is not a <see cref="T:System.IO.Stream" /> object. </exception>
	public override object GetOutput(Type type)
	{
		if (type != typeof(Stream) && !type.IsSubclassOf(typeof(Stream)))
		{
			throw new ArgumentException("The input type was invalid for this transform.", "type");
		}
		return _cs;
	}
}
