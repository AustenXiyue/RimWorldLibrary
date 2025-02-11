namespace System.Windows.Annotations;

/// <summary>Represents the method that handles the <see cref="E:System.Windows.Annotations.Annotation.AnchorChanged" /> or <see cref="E:System.Windows.Annotations.Annotation.CargoChanged" /> events raised by the <see cref="T:System.Windows.Annotations.Annotation" /> class.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void AnnotationResourceChangedEventHandler(object sender, AnnotationResourceChangedEventArgs e);
