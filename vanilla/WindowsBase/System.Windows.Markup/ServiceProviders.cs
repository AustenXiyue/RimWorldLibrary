using System.Collections.Generic;
using System.ComponentModel;
using MS.Internal.WindowsBase;

namespace System.Windows.Markup;

/// <summary>Provides an implementation for the <see cref="T:System.IServiceProvider" /> interface with methods that enable adding services.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[Browsable(false)]
public class ServiceProviders : IServiceProvider
{
	private Dictionary<Type, object> _objDict = new Dictionary<Type, object>();

	/// <summary>Gets the service object of the specified type.</summary>
	/// <returns>A service implementation for the type <paramref name="serviceType" />. May be null if there is no service stored for type <paramref name="serviceType" />.</returns>
	/// <param name="serviceType">The type of service object to get.</param>
	public object GetService(Type serviceType)
	{
		if (_objDict.ContainsKey(serviceType))
		{
			return _objDict[serviceType];
		}
		return null;
	}

	/// <summary>Adds a service to the list. </summary>
	/// <param name="serviceType">Service type of the new service.</param>
	/// <param name="service">The service implementation class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceType" /> or <paramref name="service" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Attempted to add a service that already exists in the dictionary.</exception>
	public void AddService(Type serviceType, object service)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (!_objDict.ContainsKey(serviceType))
		{
			_objDict.Add(serviceType, service);
		}
		else if (_objDict[serviceType] != service)
		{
			throw new ArgumentException(SR.ServiceTypeAlreadyAdded, "serviceType");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ServiceProviders" /> class. </summary>
	public ServiceProviders()
	{
	}
}
