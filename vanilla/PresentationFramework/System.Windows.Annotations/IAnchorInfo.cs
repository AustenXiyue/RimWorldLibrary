namespace System.Windows.Annotations;

/// <summary>Provides the capabilities for matching annotations with the corresponding annotated objects.</summary>
public interface IAnchorInfo
{
	/// <summary>Gets the annotation object.</summary>
	/// <returns>The annotation object.</returns>
	Annotation Annotation { get; }

	/// <summary>Gets the anchor of the annotation.</summary>
	/// <returns>The anchor that is resolved.</returns>
	AnnotationResource Anchor { get; }

	/// <summary>Gets the object that represents the location on the tree where the <see cref="P:System.Windows.Annotations.IAnchorInfo.Anchor" /> is resolved. </summary>
	/// <returns>The object that represents the location on the tree where the <see cref="P:System.Windows.Annotations.IAnchorInfo.Anchor" /> is resolved. The type is specified by the type of the annotated object. Sticky notes and highlights in flow or fixed documents always resolve to a <see cref="T:System.Windows.Annotations.TextAnchor" /> object.</returns>
	object ResolvedAnchor { get; }
}
