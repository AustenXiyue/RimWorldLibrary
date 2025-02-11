namespace System.Xaml;

/// <summary>Describes a service where a XAML writer can use reported line information and then include the information in the output.</summary>
public interface IXamlLineInfoConsumer
{
	/// <summary>Gets a value that determines whether a line information service should provide values and therefore, should also call <see cref="M:System.Xaml.IXamlLineInfoConsumer.SetLineInfo(System.Int32,System.Int32)" /> when relevant.</summary>
	/// <returns>true if line information is used by the implementation; otherwise, false.</returns>
	bool ShouldProvideLineInfo { get; }

	/// <summary>Collects line information.</summary>
	/// <param name="lineNumber">The line number to use in the output.</param>
	/// <param name="linePosition">The line position to use in the output.</param>
	void SetLineInfo(int lineNumber, int linePosition);
}
