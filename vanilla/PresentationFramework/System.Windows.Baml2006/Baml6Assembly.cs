using System.Reflection;
using MS.Internal.WindowsBase;

namespace System.Windows.Baml2006;

internal class Baml6Assembly
{
	public readonly string Name;

	private Assembly _assembly;

	public Assembly Assembly
	{
		get
		{
			if (_assembly != null)
			{
				return _assembly;
			}
			AssemblyName assemblyName = new AssemblyName(Name);
			_assembly = SafeSecurityHelper.GetLoadedAssembly(assemblyName);
			if (_assembly == null)
			{
				byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
				if (assemblyName.Version != null || assemblyName.CultureInfo != null || publicKeyToken != null)
				{
					try
					{
						_assembly = Assembly.Load(assemblyName.FullName);
					}
					catch
					{
						AssemblyName assemblyName2 = new AssemblyName(assemblyName.Name);
						if (publicKeyToken != null)
						{
							assemblyName2.SetPublicKeyToken(publicKeyToken);
						}
						_assembly = Assembly.Load(assemblyName2);
					}
				}
				else
				{
					_assembly = Assembly.LoadWithPartialName(assemblyName.Name);
				}
			}
			return _assembly;
		}
	}

	public Baml6Assembly(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Name = name;
		_assembly = null;
	}

	public Baml6Assembly(Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		Name = null;
		_assembly = assembly;
	}
}
