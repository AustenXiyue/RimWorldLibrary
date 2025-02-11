using System.ComponentModel;

namespace System.Windows.Documents.MsSpellCheckLib;

internal interface IChangeNotifyWrapper : INotifyPropertyChanged
{
	object Value { get; set; }
}
internal interface IChangeNotifyWrapper<T> : IChangeNotifyWrapper, INotifyPropertyChanged
{
	new T Value { get; set; }
}
