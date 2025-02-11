using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>A block-level flow content element used for grouping other <see cref="T:System.Windows.Documents.Block" /> elements.</summary>
[ContentProperty("Blocks")]
public class Section : Block
{
	internal const string HasTrailingParagraphBreakOnPastePropertyName = "HasTrailingParagraphBreakOnPaste";

	private bool _ignoreTrailingParagraphBreakOnPaste;

	/// <summary>Gets or sets a value that indicates whether or not a trailing paragraph break should be inserted after the last paragraph when placing the contents of a root <see cref="T:System.Windows.Documents.Section" /> element on the clipboard.</summary>
	/// <returns>true to indicate that a trailing paragraph break should be included; otherwise, false.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DefaultValue(true)]
	public bool HasTrailingParagraphBreakOnPaste
	{
		get
		{
			return !_ignoreTrailingParagraphBreakOnPaste;
		}
		set
		{
			_ignoreTrailingParagraphBreakOnPaste = !value;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Documents.BlockCollection" /> containing the top-level <see cref="T:System.Windows.Documents.Block" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.Section" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.BlockCollection" /> containing the <see cref="T:System.Windows.Documents.Block" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.Section" />This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public BlockCollection Blocks => new BlockCollection(this, isOwnerParent: true);

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.Section" /> class. </summary>
	public Section()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Section" /> class, taking a specified <see cref="T:System.Windows.Documents.Block" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.Section" />.</summary>
	/// <param name="block">A <see cref="T:System.Windows.Documents.Block" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.Section" />.</param>
	public Section(Block block)
	{
		if (block == null)
		{
			throw new ArgumentNullException("block");
		}
		Blocks.Add(block);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Documents.Section.Blocks" /> property should be serialized; otherwise, false.</returns>
	/// <param name="manager">A serialization service manager object for this object.</param>
	/// <exception cref="T:System.NullReferenceException">Raised when <paramref name="manager" /> is null.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeBlocks(XamlDesignerSerializationManager manager)
	{
		if (manager != null)
		{
			return manager.XmlWriter == null;
		}
		return false;
	}
}
