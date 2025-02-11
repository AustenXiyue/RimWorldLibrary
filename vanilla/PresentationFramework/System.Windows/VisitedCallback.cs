namespace System.Windows;

internal delegate bool VisitedCallback<T>(DependencyObject d, T data, bool visitedViaVisualTree);
