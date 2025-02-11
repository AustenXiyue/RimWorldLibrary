using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides the base implementation for the <see cref="T:System.ComponentModel.INestedContainer" /> interface, which enables containers to have an owning component.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class NestedContainer : Container, INestedContainer, IContainer, IDisposable
{
	private class Site : INestedSite, ISite, IServiceProvider
	{
		private IComponent component;

		private NestedContainer container;

		private string name;

		public IComponent Component => component;

		public IContainer Container => container;

		public bool DesignMode
		{
			get
			{
				IComponent owner = container.Owner;
				if (owner != null && owner.Site != null)
				{
					return owner.Site.DesignMode;
				}
				return false;
			}
		}

		public string FullName
		{
			get
			{
				if (name != null)
				{
					string ownerName = container.OwnerName;
					string text = name;
					if (ownerName != null)
					{
						text = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ownerName, text);
					}
					return text;
				}
				return name;
			}
		}

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

		internal Site(IComponent component, NestedContainer container, string name)
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

	private IComponent _owner;

	/// <summary>Gets the owning component for this nested container.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.IComponent" /> that owns this nested container.</returns>
	public IComponent Owner => _owner;

	/// <summary>Gets the name of the owning component.</summary>
	/// <returns>The name of the owning component.</returns>
	protected virtual string OwnerName
	{
		get
		{
			string result = null;
			if (_owner != null && _owner.Site != null)
			{
				result = ((!(_owner.Site is INestedSite nestedSite)) ? _owner.Site.Name : nestedSite.FullName);
			}
			return result;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.NestedContainer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.ComponentModel.IComponent" /> that owns this nested container.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="owner" /> is null.</exception>
	public NestedContainer(IComponent owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		_owner = owner;
		_owner.Disposed += OnOwnerDisposed;
	}

	/// <summary>Creates a site for the component within the container.</summary>
	/// <returns>The newly created <see cref="T:System.ComponentModel.ISite" />.</returns>
	/// <param name="component">The <see cref="T:System.ComponentModel.IComponent" /> to create a site for.</param>
	/// <param name="name">The name to assign to <paramref name="component" />, or null to skip the name assignment.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> is null.</exception>
	protected override ISite CreateSite(IComponent component, string name)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		return new Site(component, this, name);
	}

	/// <summary>Releases the resources used by the nested container.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_owner.Disposed -= OnOwnerDisposed;
		}
		base.Dispose(disposing);
	}

	/// <summary>Gets the service object of the specified type, if it is available.</summary>
	/// <returns>An <see cref="T:System.Object" /> that implements the requested service, or null if the service cannot be resolved.</returns>
	/// <param name="service">The <see cref="T:System.Type" /> of the service to retrieve.</param>
	protected override object GetService(Type service)
	{
		if (service == typeof(INestedContainer))
		{
			return this;
		}
		return base.GetService(service);
	}

	private void OnOwnerDisposed(object sender, EventArgs e)
	{
		Dispose();
	}
}
