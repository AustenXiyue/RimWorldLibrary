using System.Runtime.InteropServices;

namespace System.ComponentModel;

/// <summary>Provides the base implementation for the <see cref="T:System.ComponentModel.IComponent" /> interface and enables object sharing between applications.</summary>
[DesignerCategory("Component")]
[ClassInterface(ClassInterfaceType.AutoDispatch)]
[ComVisible(true)]
public class Component : MarshalByRefObject, IComponent, IDisposable
{
	private static readonly object EventDisposed = new object();

	private ISite site;

	private EventHandlerList events;

	/// <summary>Gets a value indicating whether the component can raise an event.</summary>
	/// <returns>true if the component can raise events; otherwise, false. The default is true.</returns>
	protected virtual bool CanRaiseEvents => true;

	internal bool CanRaiseEventsInternal => CanRaiseEvents;

	/// <summary>Gets the list of event handlers that are attached to this <see cref="T:System.ComponentModel.Component" />.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EventHandlerList" /> that provides the delegates for this component.</returns>
	protected EventHandlerList Events
	{
		get
		{
			if (events == null)
			{
				events = new EventHandlerList(this);
			}
			return events;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.ComponentModel.ISite" /> of the <see cref="T:System.ComponentModel.Component" />.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.ISite" /> associated with the <see cref="T:System.ComponentModel.Component" />, or null if the <see cref="T:System.ComponentModel.Component" /> is not encapsulated in an <see cref="T:System.ComponentModel.IContainer" />, the <see cref="T:System.ComponentModel.Component" /> does not have an <see cref="T:System.ComponentModel.ISite" /> associated with it, or the <see cref="T:System.ComponentModel.Component" /> is removed from its <see cref="T:System.ComponentModel.IContainer" />.</returns>
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

	/// <summary>Gets the <see cref="T:System.ComponentModel.IContainer" /> that contains the <see cref="T:System.ComponentModel.Component" />.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.IContainer" /> that contains the <see cref="T:System.ComponentModel.Component" />, if any, or null if the <see cref="T:System.ComponentModel.Component" /> is not encapsulated in an <see cref="T:System.ComponentModel.IContainer" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public IContainer Container => site?.Container;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.ComponentModel.Component" /> is currently in design mode.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.Component" /> is in design mode; otherwise, false.</returns>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	protected bool DesignMode => site?.DesignMode ?? false;

	/// <summary>Occurs when the component is disposed by a call to the <see cref="M:System.ComponentModel.Component.Dispose" /> method. </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
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

	/// <summary>Releases unmanaged resources and performs other cleanup operations before the <see cref="T:System.ComponentModel.Component" /> is reclaimed by garbage collection.</summary>
	~Component()
	{
		Dispose(disposing: false);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.ComponentModel.Component" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Component" /> and optionally releases the managed resources.</summary>
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

	/// <summary>Returns an object that represents a service provided by the <see cref="T:System.ComponentModel.Component" /> or by its <see cref="T:System.ComponentModel.Container" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents a service provided by the <see cref="T:System.ComponentModel.Component" />, or null if the <see cref="T:System.ComponentModel.Component" /> does not provide the specified service.</returns>
	/// <param name="service">A service provided by the <see cref="T:System.ComponentModel.Component" />. </param>
	protected virtual object GetService(Type service)
	{
		return site?.GetService(service);
	}

	/// <summary>Returns a <see cref="T:System.String" /> containing the name of the <see cref="T:System.ComponentModel.Component" />, if any. This method should not be overridden.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the name of the <see cref="T:System.ComponentModel.Component" />, if any, or null if the <see cref="T:System.ComponentModel.Component" /> is unnamed.</returns>
	public override string ToString()
	{
		ISite site = this.site;
		if (site != null)
		{
			return site.Name + " [" + GetType().FullName + "]";
		}
		return GetType().FullName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Component" /> class. </summary>
	public Component()
	{
	}
}
