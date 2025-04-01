using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Encapsulates zero or more components.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class Container : IContainer, IDisposable
{
	private class Site : ISite, IServiceProvider
	{
		private IComponent component;

		private Container container;

		private string name;

		public IComponent Component => component;

		public IContainer Container => container;

		public bool DesignMode => false;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (value == null || name == null || !value.Equals(name))
				{
					container.ValidateName(component, value);
					name = value;
				}
			}
		}

		internal Site(IComponent component, Container container, string name)
		{
			this.component = component;
			this.container = container;
			this.name = name;
		}

		public object GetService(Type service)
		{
			if (!(service == typeof(ISite)))
			{
				return container.GetService(service);
			}
			return this;
		}
	}

	private ISite[] sites;

	private int siteCount;

	private ComponentCollection components;

	private ContainerFilterService filter;

	private bool checkedFilter;

	private object syncObj = new object();

	/// <summary>Gets all the components in the <see cref="T:System.ComponentModel.Container" />.</summary>
	/// <returns>A collection that contains the components in the <see cref="T:System.ComponentModel.Container" />.</returns>
	public virtual ComponentCollection Components
	{
		get
		{
			lock (syncObj)
			{
				if (components == null)
				{
					IComponent[] array = new IComponent[siteCount];
					for (int i = 0; i < siteCount; i++)
					{
						array[i] = sites[i].Component;
					}
					components = new ComponentCollection(array);
					if (filter == null && checkedFilter)
					{
						checkedFilter = false;
					}
				}
				if (!checkedFilter)
				{
					filter = GetService(typeof(ContainerFilterService)) as ContainerFilterService;
					checkedFilter = true;
				}
				if (filter != null)
				{
					ComponentCollection componentCollection = filter.FilterComponents(components);
					if (componentCollection != null)
					{
						components = componentCollection;
					}
				}
				return components;
			}
		}
	}

	/// <summary>Releases unmanaged resources and performs other cleanup operations before the <see cref="T:System.ComponentModel.Container" /> is reclaimed by garbage collection.</summary>
	~Container()
	{
		Dispose(disposing: false);
	}

	/// <summary>Adds the specified <see cref="T:System.ComponentModel.Component" /> to the <see cref="T:System.ComponentModel.Container" />. The component is unnamed.</summary>
	/// <param name="component">The component to add. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual void Add(IComponent component)
	{
		Add(component, null);
	}

	/// <summary>Adds the specified <see cref="T:System.ComponentModel.Component" /> to the <see cref="T:System.ComponentModel.Container" /> and assigns it a name.</summary>
	/// <param name="component">The component to add. </param>
	/// <param name="name">The unique, case-insensitive name to assign to the component.-or- null, which leaves the component unnamed. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not unique.</exception>
	public virtual void Add(IComponent component, string name)
	{
		lock (syncObj)
		{
			if (component == null)
			{
				return;
			}
			ISite site = component.Site;
			if (site != null && site.Container == this)
			{
				return;
			}
			if (sites == null)
			{
				sites = new ISite[4];
			}
			else
			{
				ValidateName(component, name);
				if (sites.Length == siteCount)
				{
					ISite[] destinationArray = new ISite[siteCount * 2];
					Array.Copy(sites, 0, destinationArray, 0, siteCount);
					sites = destinationArray;
				}
			}
			site?.Container.Remove(component);
			ISite site2 = CreateSite(component, name);
			sites[siteCount++] = site2;
			component.Site = site2;
			components = null;
		}
	}

	/// <summary>Creates a site <see cref="T:System.ComponentModel.ISite" /> for the given <see cref="T:System.ComponentModel.IComponent" /> and assigns the given name to the site.</summary>
	/// <returns>The newly created site.</returns>
	/// <param name="component">The <see cref="T:System.ComponentModel.IComponent" /> to create a site for. </param>
	/// <param name="name">The name to assign to <paramref name="component" />, or null to skip the name assignment. </param>
	protected virtual ISite CreateSite(IComponent component, string name)
	{
		return new Site(component, this, name);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.ComponentModel.Container" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Container" />, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		lock (syncObj)
		{
			while (siteCount > 0)
			{
				ISite obj = sites[--siteCount];
				obj.Component.Site = null;
				obj.Component.Dispose();
			}
			sites = null;
			components = null;
		}
	}

	/// <summary>Gets the service object of the specified type, if it is available.</summary>
	/// <returns>An <see cref="T:System.Object" /> implementing the requested service, or null if the service cannot be resolved.</returns>
	/// <param name="service">The <see cref="T:System.Type" /> of the service to retrieve. </param>
	protected virtual object GetService(Type service)
	{
		if (!(service == typeof(IContainer)))
		{
			return null;
		}
		return this;
	}

	/// <summary>Removes a component from the <see cref="T:System.ComponentModel.Container" />.</summary>
	/// <param name="component">The component to remove. </param>
	public virtual void Remove(IComponent component)
	{
		Remove(component, preserveSite: false);
	}

	private void Remove(IComponent component, bool preserveSite)
	{
		lock (syncObj)
		{
			if (component == null)
			{
				return;
			}
			ISite site = component.Site;
			if (site == null || site.Container != this)
			{
				return;
			}
			if (!preserveSite)
			{
				component.Site = null;
			}
			for (int i = 0; i < siteCount; i++)
			{
				if (sites[i] == site)
				{
					siteCount--;
					Array.Copy(sites, i + 1, sites, i, siteCount - i);
					sites[siteCount] = null;
					components = null;
					break;
				}
			}
		}
	}

	/// <summary>Removes a component from the <see cref="T:System.ComponentModel.Container" /> without setting <see cref="P:System.ComponentModel.IComponent.Site" /> to null.</summary>
	/// <param name="component">The component to remove.</param>
	protected void RemoveWithoutUnsiting(IComponent component)
	{
		Remove(component, preserveSite: true);
	}

	/// <summary>Determines whether the component name is unique for this container.</summary>
	/// <param name="component">The named component.</param>
	/// <param name="name">The component name to validate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not unique.</exception>
	protected virtual void ValidateName(IComponent component, string name)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		if (name == null)
		{
			return;
		}
		for (int i = 0; i < Math.Min(siteCount, sites.Length); i++)
		{
			ISite site = sites[i];
			if (site != null && site.Name != null && string.Equals(site.Name, name, StringComparison.OrdinalIgnoreCase) && site.Component != component && ((InheritanceAttribute)TypeDescriptor.GetAttributes(site.Component)[typeof(InheritanceAttribute)]).InheritanceLevel != InheritanceLevel.InheritedReadOnly)
			{
				throw new ArgumentException(global::SR.GetString("Duplicate component name '{0}'.  Component names must be unique and case-insensitive.", name));
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Container" /> class. </summary>
	public Container()
	{
	}
}
