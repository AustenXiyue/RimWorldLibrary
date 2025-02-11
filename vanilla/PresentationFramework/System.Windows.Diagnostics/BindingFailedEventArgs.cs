using System.Diagnostics;
using System.Windows.Data;

namespace System.Windows.Diagnostics;

public class BindingFailedEventArgs : EventArgs
{
	public TraceEventType EventType { get; }

	public int Code { get; }

	public string Message { get; }

	public BindingExpressionBase Binding { get; }

	public object[] Parameters { get; }

	internal BindingFailedEventArgs(TraceEventType eventType, int code, string message, BindingExpressionBase binding, params object[] parameters)
	{
		EventType = eventType;
		Code = code;
		Message = message ?? string.Empty;
		Binding = binding;
		Parameters = parameters ?? Array.Empty<object>();
	}
}
