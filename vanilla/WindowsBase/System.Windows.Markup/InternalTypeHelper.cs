using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Markup;

/// <summary>Abstract class used internally by the WPF XAML compiler to support the use of internal types.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class InternalTypeHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.InternalTypeHelper" /> class.</summary>
	protected InternalTypeHelper()
	{
	}

	/// <summary>When overridden in a derived (generated) class, creates an instance of an internal type.</summary>
	/// <returns>The created instance. </returns>
	/// <param name="type">The <see cref="T:System.Type" /> to create.</param>
	/// <param name="culture">Culture specific information. </param>
	protected internal abstract object CreateInstance(Type type, CultureInfo culture);

	/// <summary>When overridden in a derived (generated) class, gets the value of an internal property on the target object</summary>
	/// <returns>The value of the property.</returns>
	/// <param name="propertyInfo">Property information for the property to get. </param>
	/// <param name="target">The object that holds the desired property value.</param>
	/// <param name="culture">Culture specific information. </param>
	protected internal abstract object GetPropertyValue(PropertyInfo propertyInfo, object target, CultureInfo culture);

	/// <summary>When overridden in a derived (generated) class, sets the value on an internal property on the target object.</summary>
	/// <param name="propertyInfo">Property information for the property to set. </param>
	/// <param name="target">The object that holds the desired property value.</param>
	/// <param name="value">The value to set.</param>
	/// <param name="culture">Culture specific information. </param>
	protected internal abstract void SetPropertyValue(PropertyInfo propertyInfo, object target, object value, CultureInfo culture);

	/// <summary>When overridden in a derived (generated) class, creates an event delegate referencing a non-public handler method.</summary>
	/// <returns>The delegate reference.</returns>
	/// <param name="delegateType">The <see cref="T:System.Type" /> of the delegate. </param>
	/// <param name="target">The target where the handler is attached.</param>
	/// <param name="handler">The name of the handler implementation.</param>
	protected internal abstract Delegate CreateDelegate(Type delegateType, object target, string handler);

	/// <summary>When overridden in a derived (generated) class, attaches an event handler delegate to an internal event.</summary>
	/// <param name="eventInfo">The event information for the event (CLR reflection information). </param>
	/// <param name="target">The target where the handler is attached.</param>
	/// <param name="handler">The event handler.</param>
	protected internal abstract void AddEventHandler(EventInfo eventInfo, object target, Delegate handler);
}
