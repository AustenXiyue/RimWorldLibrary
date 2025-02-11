namespace System.Windows.Documents;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.Documents.DocumentPaginator.GetPageCompleted" /> event of a <see cref="T:System.Windows.Documents.FixedDocument" /> or other classes implementing <see cref="T:System.Windows.Documents.DocumentPaginator" />. </summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void GetPageCompletedEventHandler(object sender, GetPageCompletedEventArgs e);
