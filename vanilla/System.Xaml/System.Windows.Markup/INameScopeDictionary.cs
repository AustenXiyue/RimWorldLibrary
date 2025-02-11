using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Markup;

/// <summary>Unifies enumerable, collection, and dictionary support that are useful for exposing a dictionary of names in a XAML namescope.</summary>
public interface INameScopeDictionary : INameScope, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
}
