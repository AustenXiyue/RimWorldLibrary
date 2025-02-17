namespace System.Windows.Markup;

/// <summary>Provides methods used internally by the WPF XAML parser to attach events and event setters in compiled XAML. </summary>
public interface IStyleConnector
{
	/// <summary>Attaches events on event setters and templates in compiled content. </summary>
	/// <param name="connectionId">The unique connection ID for event wiring purposes. </param>
	/// <param name="target">The target for event wiring.</param>
	void Connect(int connectionId, object target);
}
