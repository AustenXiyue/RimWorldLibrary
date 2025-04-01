using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Provides a simple implementation of the <see cref="T:System.ComponentModel.Design.IServiceContainer" /> interface. This class cannot be inherited.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class ServiceContainer : IServiceContainer, IServiceProvider, IDisposable
{
	private sealed class ServiceCollection<T> : Dictionary<Type, T>
	{
		private sealed class EmbeddedTypeAwareTypeComparer : IEqualityComparer<Type>
		{
			public bool Equals(Type x, Type y)
			{
				return x.IsEquivalentTo(y);
			}

			public int GetHashCode(Type obj)
			{
				return obj.FullName.GetHashCode();
			}
		}

		private static EmbeddedTypeAwareTypeComparer serviceTypeComparer = new EmbeddedTypeAwareTypeComparer();

		public ServiceCollection()
			: base((IEqualityComparer<Type>)serviceTypeComparer)
		{
		}
	}

	private ServiceCollection<object> services;

	private IServiceProvider parentProvider;

	private static Type[] _defaultServices = new Type[2]
	{
		typeof(IServiceContainer),
		typeof(ServiceContainer)
	};

	private static TraceSwitch TRACESERVICE = new TraceSwitch("TRACESERVICE", "ServiceProvider: Trace service provider requests.");

	private IServiceContainer Container
	{
		get
		{
			IServiceContainer result = null;
			if (parentProvider != null)
			{
				result = (IServiceContainer)parentProvider.GetService(typeof(IServiceContainer));
			}
			return result;
		}
	}

	/// <summary>Gets the default services implemented directly by <see cref="T:System.ComponentModel.Design.ServiceContainer" />.</summary>
	/// <returns>The default services.</returns>
	protected virtual Type[] DefaultServices => _defaultServices;

	private ServiceCollection<object> Services
	{
		get
		{
			if (services == null)
			{
				services = new ServiceCollection<object>();
			}
			return services;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.ServiceContainer" /> class.</summary>
	public ServiceContainer()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.ServiceContainer" /> class using the specified parent service provider.</summary>
	/// <param name="parentProvider">A parent service provider. </param>
	public ServiceContainer(IServiceProvider parentProvider)
	{
		this.parentProvider = parentProvider;
	}

	/// <summary>Adds the specified service to the service container.</summary>
	/// <param name="serviceType">The type of service to add. </param>
	/// <param name="serviceInstance">An instance of the service to add. This object must implement or inherit from the type indicated by the <paramref name="serviceType" /> parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceType" /> or <paramref name="serviceInstance" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A service of type <paramref name="serviceType" /> already exists in the container.</exception>
	public void AddService(Type serviceType, object serviceInstance)
	{
		AddService(serviceType, serviceInstance, promote: false);
	}

	/// <summary>Adds the specified service to the service container.</summary>
	/// <param name="serviceType">The type of service to add. </param>
	/// <param name="serviceInstance">An instance of the service type to add. This object must implement or inherit from the type indicated by the <paramref name="serviceType" /> parameter. </param>
	/// <param name="promote">true if this service should be added to any parent service containers; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceType" /> or <paramref name="serviceInstance" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A service of type <paramref name="serviceType" /> already exists in the container.</exception>
	public virtual void AddService(Type serviceType, object serviceInstance, bool promote)
	{
		if (promote)
		{
			IServiceContainer container = Container;
			if (container != null)
			{
				container.AddService(serviceType, serviceInstance, promote);
				return;
			}
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceInstance == null)
		{
			throw new ArgumentNullException("serviceInstance");
		}
		if (!(serviceInstance is ServiceCreatorCallback) && !serviceInstance.GetType().IsCOMObject && !serviceType.IsAssignableFrom(serviceInstance.GetType()))
		{
			throw new ArgumentException(global::SR.GetString("The service instance must derive from or implement {0}.", serviceType.FullName));
		}
		if (Services.ContainsKey(serviceType))
		{
			throw new ArgumentException(global::SR.GetString("The service {0} already exists in the service container.", serviceType.FullName), "serviceType");
		}
		Services[serviceType] = serviceInstance;
	}

	/// <summary>Adds the specified service to the service container.</summary>
	/// <param name="serviceType">The type of service to add. </param>
	/// <param name="callback">A callback object that can create the service. This allows a service to be declared as available, but delays creation of the object until the service is requested. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceType" /> or <paramref name="callback" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A service of type <paramref name="serviceType" /> already exists in the container.</exception>
	public void AddService(Type serviceType, ServiceCreatorCallback callback)
	{
		AddService(serviceType, callback, promote: false);
	}

	/// <summary>Adds the specified service to the service container.</summary>
	/// <param name="serviceType">The type of service to add. </param>
	/// <param name="callback">A callback object that can create the service. This allows a service to be declared as available, but delays creation of the object until the service is requested. </param>
	/// <param name="promote">true if this service should be added to any parent service containers; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceType" /> or <paramref name="callback" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A service of type <paramref name="serviceType" /> already exists in the container.</exception>
	public virtual void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
	{
		if (promote)
		{
			IServiceContainer container = Container;
			if (container != null)
			{
				container.AddService(serviceType, callback, promote);
				return;
			}
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		if (Services.ContainsKey(serviceType))
		{
			throw new ArgumentException(global::SR.GetString("The service {0} already exists in the service container.", serviceType.FullName), "serviceType");
		}
		Services[serviceType] = callback;
	}

	/// <summary>Disposes this service container.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Disposes this service container.</summary>
	/// <param name="disposing">true if the <see cref="T:System.ComponentModel.Design.ServiceContainer" /> is in the process of being disposed of; otherwise, false.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		ServiceCollection<object> serviceCollection = services;
		services = null;
		if (serviceCollection == null)
		{
			return;
		}
		foreach (object value in serviceCollection.Values)
		{
			if (value is IDisposable)
			{
				((IDisposable)value).Dispose();
			}
		}
	}

	/// <summary>Gets the requested service.</summary>
	/// <returns>An instance of the service if it could be found, or null if it could not be found.</returns>
	/// <param name="serviceType">The type of service to retrieve. </param>
	public virtual object GetService(Type serviceType)
	{
		object value = null;
		Type[] defaultServices = DefaultServices;
		for (int i = 0; i < defaultServices.Length; i++)
		{
			if (serviceType.IsEquivalentTo(defaultServices[i]))
			{
				value = this;
				break;
			}
		}
		if (value == null)
		{
			Services.TryGetValue(serviceType, out value);
		}
		if (value is ServiceCreatorCallback)
		{
			value = ((ServiceCreatorCallback)value)(this, serviceType);
			if (value != null && !value.GetType().IsCOMObject && !serviceType.IsAssignableFrom(value.GetType()))
			{
				value = null;
			}
			Services[serviceType] = value;
		}
		if (value == null && parentProvider != null)
		{
			value = parentProvider.GetService(serviceType);
		}
		return value;
	}

	/// <summary>Removes the specified service type from the service container.</summary>
	/// <param name="serviceType">The type of service to remove. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceType" /> is null.</exception>
	public void RemoveService(Type serviceType)
	{
		RemoveService(serviceType, promote: false);
	}

	/// <summary>Removes the specified service type from the service container.</summary>
	/// <param name="serviceType">The type of service to remove. </param>
	/// <param name="promote">true if this service should be removed from any parent service containers; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serviceType" /> is null.</exception>
	public virtual void RemoveService(Type serviceType, bool promote)
	{
		if (promote)
		{
			IServiceContainer container = Container;
			if (container != null)
			{
				container.RemoveService(serviceType, promote);
				return;
			}
		}
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		Services.Remove(serviceType);
	}
}
