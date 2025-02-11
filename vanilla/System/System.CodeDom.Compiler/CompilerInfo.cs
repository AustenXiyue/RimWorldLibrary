using System.Collections.Generic;
using System.Reflection;

namespace System.CodeDom.Compiler;

/// <summary>Represents the configuration settings of a language provider. This class cannot be inherited.</summary>
public sealed class CompilerInfo
{
	internal readonly IDictionary<string, string> _providerOptions = new Dictionary<string, string>();

	internal string _codeDomProviderTypeName;

	internal CompilerParameters _compilerParams;

	internal string[] _compilerLanguages;

	internal string[] _compilerExtensions;

	private Type _type;

	/// <summary>Gets the type of the configured <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> implementation.</summary>
	/// <returns>A read-only <see cref="T:System.Type" /> instance that represents the configured language provider type.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationException">The language provider is not configured on this computer. </exception>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">Cannot locate the type because it is a null or empty string.-or-Cannot locate the type because the name for the <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> cannot be found in the configuration file.</exception>
	public Type CodeDomProviderType
	{
		get
		{
			if (_type == null)
			{
				lock (this)
				{
					if (_type == null)
					{
						_type = Type.GetType(_codeDomProviderTypeName);
					}
				}
			}
			return _type;
		}
	}

	/// <summary>Returns a value indicating whether the language provider implementation is configured on the computer.</summary>
	/// <returns>true if the language provider implementation type is configured on the computer; otherwise, false.</returns>
	public bool IsCodeDomProviderTypeValid => Type.GetType(_codeDomProviderTypeName) != null;

	internal CompilerParameters CompilerParams => _compilerParams;

	internal IDictionary<string, string> ProviderOptions => _providerOptions;

	private CompilerInfo()
	{
	}

	/// <summary>Gets the language names supported by the language provider.</summary>
	/// <returns>An array of language names supported by the language provider.</returns>
	public string[] GetLanguages()
	{
		return CloneCompilerLanguages();
	}

	/// <summary>Returns the file name extensions supported by the language provider.</summary>
	/// <returns>An array of file name extensions supported by the language provider.</returns>
	public string[] GetExtensions()
	{
		return CloneCompilerExtensions();
	}

	/// <summary>Returns a <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> instance for the current language provider settings.</summary>
	/// <returns>A CodeDOM provider associated with the language provider configuration. </returns>
	public CodeDomProvider CreateProvider()
	{
		if (_providerOptions.Count > 0)
		{
			ConstructorInfo constructor = CodeDomProviderType.GetConstructor(new Type[1] { typeof(IDictionary<string, string>) });
			if (constructor != null)
			{
				return (CodeDomProvider)constructor.Invoke(new object[1] { _providerOptions });
			}
		}
		return (CodeDomProvider)Activator.CreateInstance(CodeDomProviderType);
	}

	/// <summary>Returns a <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> instance for the current language provider settings and specified options.</summary>
	/// <returns>A CodeDOM provider associated with the language provider configuration and specified options.</returns>
	/// <param name="providerOptions">A collection of provider options from the configuration file.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="providerOptions " />is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The provider does not support options.</exception>
	public CodeDomProvider CreateProvider(IDictionary<string, string> providerOptions)
	{
		if (providerOptions == null)
		{
			throw new ArgumentNullException("providerOptions");
		}
		ConstructorInfo constructor = CodeDomProviderType.GetConstructor(new Type[1] { typeof(IDictionary<string, string>) });
		if (constructor != null)
		{
			return (CodeDomProvider)constructor.Invoke(new object[1] { providerOptions });
		}
		throw new InvalidOperationException(global::SR.Format("This CodeDomProvider type does not have a constructor that takes providerOptions - \"{0}\"", CodeDomProviderType.ToString()));
	}

	/// <summary>Gets the configured compiler settings for the language provider implementation.</summary>
	/// <returns>A read-only <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> instance that contains the compiler options and settings configured for the language provider. </returns>
	public CompilerParameters CreateDefaultCompilerParameters()
	{
		return CloneCompilerParameters();
	}

	internal CompilerInfo(CompilerParameters compilerParams, string codeDomProviderTypeName, string[] compilerLanguages, string[] compilerExtensions)
	{
		_compilerLanguages = compilerLanguages;
		_compilerExtensions = compilerExtensions;
		_codeDomProviderTypeName = codeDomProviderTypeName;
		_compilerParams = compilerParams ?? new CompilerParameters();
	}

	internal CompilerInfo(CompilerParameters compilerParams, string codeDomProviderTypeName)
	{
		_codeDomProviderTypeName = codeDomProviderTypeName;
		_compilerParams = compilerParams ?? new CompilerParameters();
	}

	/// <summary>Returns the hash code for the current instance.</summary>
	/// <returns>A 32-bit signed integer hash code for the current <see cref="T:System.CodeDom.Compiler.CompilerInfo" /> instance, suitable for use in hashing algorithms and data structures such as a hash table. </returns>
	public override int GetHashCode()
	{
		return _codeDomProviderTypeName.GetHashCode();
	}

	/// <summary>Determines whether the specified object represents the same language provider and compiler settings as the current <see cref="T:System.CodeDom.Compiler.CompilerInfo" />.</summary>
	/// <returns>true if <paramref name="o" /> is a <see cref="T:System.CodeDom.Compiler.CompilerInfo" /> object and its value is the same as this instance; otherwise, false.</returns>
	/// <param name="o">The object to compare with the current <see cref="T:System.CodeDom.Compiler.CompilerInfo" />. </param>
	public override bool Equals(object o)
	{
		if (!(o is CompilerInfo compilerInfo))
		{
			return false;
		}
		if (CodeDomProviderType == compilerInfo.CodeDomProviderType && CompilerParams.WarningLevel == compilerInfo.CompilerParams.WarningLevel && CompilerParams.IncludeDebugInformation == compilerInfo.CompilerParams.IncludeDebugInformation)
		{
			return CompilerParams.CompilerOptions == compilerInfo.CompilerParams.CompilerOptions;
		}
		return false;
	}

	private CompilerParameters CloneCompilerParameters()
	{
		return new CompilerParameters
		{
			IncludeDebugInformation = _compilerParams.IncludeDebugInformation,
			TreatWarningsAsErrors = _compilerParams.TreatWarningsAsErrors,
			WarningLevel = _compilerParams.WarningLevel,
			CompilerOptions = _compilerParams.CompilerOptions
		};
	}

	private string[] CloneCompilerLanguages()
	{
		return (string[])_compilerLanguages.Clone();
	}

	private string[] CloneCompilerExtensions()
	{
		return (string[])_compilerExtensions.Clone();
	}
}
