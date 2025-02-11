using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MS.Internal.Controls;

internal class ValidationErrorCollection : ObservableCollection<ValidationError>
{
	public static readonly ReadOnlyObservableCollection<ValidationError> Empty = new ReadOnlyObservableCollection<ValidationError>(new ValidationErrorCollection());
}
