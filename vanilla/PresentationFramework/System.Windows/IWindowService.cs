namespace System.Windows;

internal interface IWindowService
{
	string Title { get; set; }

	double Height { get; set; }

	double Width { get; set; }

	bool UserResized { get; }
}
