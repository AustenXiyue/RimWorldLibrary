using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Provides markup compile and tools support for named XAML elements and for attaching event handlers to them.</summary>
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public interface IComponentConnector
{
	/// <summary>Attaches events and names to compiled content. </summary>
	/// <param name="connectionId">An identifier token to distinguish calls.</param>
	/// <param name="target">The target to connect events and names to.</param>
	void Connect(int connectionId, object target);

	/// <summary>Loads the compiled page of a component.</summary>
	void InitializeComponent();
}
