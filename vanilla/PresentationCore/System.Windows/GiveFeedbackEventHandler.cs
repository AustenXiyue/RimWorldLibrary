namespace System.Windows;

/// <summary>Represents a method that will handle the feedback routed event from in-process drag-and-drop operations, for instance <see cref="E:System.Windows.UIElement.GiveFeedback" />.</summary>
/// <param name="sender">The object where the event handler is attached.</param>
/// <param name="e">The event data.</param>
public delegate void GiveFeedbackEventHandler(object sender, GiveFeedbackEventArgs e);
