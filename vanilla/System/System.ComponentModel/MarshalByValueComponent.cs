using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace System.ComponentModel;

/// <summary>Implements <see cref="T:System.ComponentModel.IComponent" /> and provides the base implementation for remotable components that are marshaled by value (a copy of the serialized object is passed).</summary>
[TypeConverter(typeof(ComponentConverter))]
[DesignerCategory("Component")]
[Designer("System.Windows.Forms.Design.ComponentDocumentDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner))]
[ComVisible(true)]
public class MarshalByValueComponent : IComponent, IDisposable, IServiceProvider
{
	private static readonly object EventDisposed = new object();

	private ISite site;

	private EventHandlerList events;

	/// <summary>Gets the list of event handlers that are attached to this component.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EventHandlerList" /> that provides the delegates for this component.</returns>
	protected EventHandlerList Events
	{
		get
		{
			if (events == null)
			{
				events = new EventHandlerList();
			}
			return events;
		}
	}

	/// <summary>Gets or sets the site of the component.</summary>
	/// <returns>An object implementing the <see cref="T:System.ComponentModel.ISite" /> interface that represents the site of the component.</returns>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual ISite Site
	{
		get
		{
			return site;
		}
		set
		{
			site = value;
		}
	}

	/// <summary>Gets the container for the component.</summary>
	/// <returns>An object implementing the <see cref="T:System.ComponentModel.IContainer" /> interface that represents the component's container, or null if the component does not have a site.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public virtual IContainer Container => site?.Container;

	/// <summary>Gets a value indicating whether the component is currently in design mode.</summary>
	/// <returns>true if the component is in design mode; otherwise, false.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public virtual bool DesignMode => site?.DesignMode ?? false;

	/// <summary>Adds an event handler to listen to the <see cref="E:System.ComponentModel.MarshalByValueComponent.Disposed" /> event on the component.</summary>
	public event EventHandler Disposed
	{
		add
		{
			Events.AddHandler(EventDisposed, value);
		}
		remove
		{
			Events.RemoveHandler(EventDisposed, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MarshalByValueComponent" /> class.</summary>
	public MarshalByValueComponent()
	{
	}

	~MarshalByValueComponent()
	{
		Dispose(disposing: false);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.ComponentModel.MarshalByValueComponent" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.MarshalByValueComponent" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		lock (this)
		{
			if (site != null && site.Container != null)
			{
				site.Container.Remove(this);
			}
			if (events != null)
			{
				((EventHandler)events[EventDisposed])?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	/// <summary>Gets the implementer of the <see cref="T:System.IServiceProvider" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the implementer of the <see cref="T:System.IServiceProvider" />.</returns>
	/// <param name="service">A <see cref="T:System.Type" /> that represents the type of service you want. </param>
	public virtual object GetService(Type service)
	{
		if (site != null)
		{
			return site.GetService(service);
		}
		return null;
	}

	/// <summary>Returns a <see cref="T:System.String" /> containing the name of the <see cref="T:System.ComponentModel.Component" />, if any. This method should not be overridden.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the name of the <see cref="T:System.ComponentModel.Component" />, if any.null if the <see cref="T:System.ComponentModel.Component" /> is unnamed.</returns>
	public override string ToString()
	{
		ISite site = this.site;
		if (site != null)
		{
			return site.Name + " [" + GetType().FullName + "]";
		}
		return GetType().FullName;
	}
}
