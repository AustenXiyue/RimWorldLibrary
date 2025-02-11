namespace System.Windows.Annotations;

/// <summary>Specifies the actions that occur with <see cref="T:System.Windows.Annotations.Annotation" /> author, anchor, and cargo resources.</summary>
public enum AnnotationAction
{
	/// <summary>The component was added to the annotation.</summary>
	Added,
	/// <summary>The component was removed from the annotation.</summary>
	Removed,
	/// <summary>The component was modified within the annotation.</summary>
	Modified
}
