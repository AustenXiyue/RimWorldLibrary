using System.Reflection;
using System.Threading;

namespace System.Xml.Serialization;

/// <summary>An abstract class that is the base class for <see cref="T:System.Xml.Serialization.XmlSerializationReader" /> and <see cref="T:System.Xml.Serialization.XmlSerializationWriter" /> and that contains methods common to both of these types.</summary>
public abstract class XmlSerializationGeneratedCode
{
	private TempAssembly tempAssembly;

	private int threadCode;

	private ResolveEventHandler assemblyResolver;

	internal void Init(TempAssembly tempAssembly)
	{
		this.tempAssembly = tempAssembly;
		if (tempAssembly != null && tempAssembly.NeedAssembyResolve)
		{
			threadCode = Thread.CurrentThread.GetHashCode();
			assemblyResolver = OnAssemblyResolve;
			AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver;
		}
	}

	internal void Dispose()
	{
		if (assemblyResolver != null)
		{
			AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolver;
		}
		assemblyResolver = null;
	}

	internal Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
	{
		if (tempAssembly != null && Thread.CurrentThread.GetHashCode() == threadCode)
		{
			return tempAssembly.GetReferencedAssembly(args.Name);
		}
		return null;
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Xml.Serialization.XmlSerializationGeneratedCode" /> class. </summary>
	protected XmlSerializationGeneratedCode()
	{
	}
}
