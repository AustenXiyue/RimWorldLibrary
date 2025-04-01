using System.Configuration;

namespace System.Runtime.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure serialization by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />.</summary>
public sealed class TypeElement : ConfigurationElement
{
	private ConfigurationPropertyCollection properties;

	private string key = Guid.NewGuid().ToString();

	protected override ConfigurationPropertyCollection Properties
	{
		get
		{
			if (properties == null)
			{
				ConfigurationPropertyCollection configurationPropertyCollection = new ConfigurationPropertyCollection();
				configurationPropertyCollection.Add(new ConfigurationProperty("", typeof(ParameterElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
				configurationPropertyCollection.Add(new ConfigurationProperty("type", typeof(string), string.Empty, null, new StringValidator(0, int.MaxValue, null), ConfigurationPropertyOptions.None));
				configurationPropertyCollection.Add(new ConfigurationProperty("index", typeof(int), 0, null, new IntegerValidator(0, int.MaxValue, rangeIsExclusive: false), ConfigurationPropertyOptions.None));
				properties = configurationPropertyCollection;
			}
			return properties;
		}
	}

	internal string Key => key;

	/// <summary>Gets a collection of parameters.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.Configuration.ParameterElementCollection" /> that contains the parameters for the type. </returns>
	[ConfigurationProperty("", DefaultValue = null, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public ParameterElementCollection Parameters => (ParameterElementCollection)base[""];

	/// <summary>Gets or sets the name of the type.</summary>
	/// <returns>The name of the type.</returns>
	[ConfigurationProperty("type", DefaultValue = "")]
	[StringValidator(MinLength = 0)]
	public string Type
	{
		get
		{
			return (string)base["type"];
		}
		set
		{
			base["type"] = value;
		}
	}

	/// <summary>Gets or sets the position of the element.</summary>
	/// <returns>The position of the element.</returns>
	[ConfigurationProperty("index", DefaultValue = 0)]
	[IntegerValidator(MinValue = 0)]
	public int Index
	{
		get
		{
			return (int)base["index"];
		}
		set
		{
			base["index"] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.TypeElement" /> class.  </summary>
	public TypeElement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.TypeElement" /> class with the specified type name. </summary>
	/// <param name="typeName">The name of the type that uses known types.</param>
	public TypeElement(string typeName)
		: this()
	{
		if (string.IsNullOrEmpty(typeName))
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
		}
		Type = typeName;
	}

	protected override void Reset(ConfigurationElement parentElement)
	{
		TypeElement typeElement = (TypeElement)parentElement;
		key = typeElement.key;
		base.Reset(parentElement);
	}

	internal Type GetType(string rootType, Type[] typeArgs)
	{
		return GetType(rootType, typeArgs, Type, Index, Parameters);
	}

	internal static Type GetType(string rootType, Type[] typeArgs, string type, int index, ParameterElementCollection parameters)
	{
		if (string.IsNullOrEmpty(type))
		{
			if (typeArgs == null || index >= typeArgs.Length)
			{
				int num = ((typeArgs != null) ? typeArgs.Length : 0);
				if (num == 0)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString("For known type configuration, index is out of bound. Root type: '{0}' has {1} type arguments, and index was {2}.", rootType, num, index));
				}
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString("For known type configuration, index is out of bound. Root type: '{0}' has {1} type arguments, and index was {2}.", rootType, num, index));
			}
			return typeArgs[index];
		}
		Type type2 = System.Type.GetType(type, throwOnError: true);
		if (type2.IsGenericTypeDefinition)
		{
			if (parameters.Count != type2.GetGenericArguments().Length)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString("Generic parameter count do not match between known type and configuration. Type is '{0}', known type has {1} parameters, configuration has {2} parameters.", type, type2.GetGenericArguments().Length, parameters.Count));
			}
			Type[] array = new Type[parameters.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = parameters[i].GetType(rootType, typeArgs);
			}
			type2 = type2.MakeGenericType(array);
		}
		return type2;
	}
}
