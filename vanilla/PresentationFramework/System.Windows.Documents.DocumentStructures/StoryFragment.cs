using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents all or part of a story within an XPS document.</summary>
[ContentProperty("BlockElementList")]
public class StoryFragment : IAddChild, IEnumerable<BlockElement>, IEnumerable
{
	private List<BlockElement> _elementList;

	private string _storyName;

	private string _fragmentName;

	private string _fragmentType;

	/// <summary>Gets or sets the name of the story. </summary>
	/// <returns>A <see cref="T:System.String" /> representing the name of the story.</returns>
	public string StoryName
	{
		get
		{
			return _storyName;
		}
		set
		{
			_storyName = value;
		}
	}

	/// <summary>Gets or sets the name of the story fragment. </summary>
	/// <returns>A <see cref="T:System.String" /> representing the name of this fragment. </returns>
	public string FragmentName
	{
		get
		{
			return _fragmentName;
		}
		set
		{
			_fragmentName = value;
		}
	}

	/// <summary>Gets or sets the type of fragment. </summary>
	/// <returns>A <see cref="T:System.String" /> representing the type of fragment.</returns>
	public string FragmentType
	{
		get
		{
			return _fragmentType;
		}
		set
		{
			_fragmentType = value;
		}
	}

	internal List<BlockElement> BlockElementList => _elementList;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.StoryFragment" /> class. </summary>
	public StoryFragment()
	{
		_elementList = new List<BlockElement>();
	}

	/// <summary>Add a block to the story fragment.</summary>
	/// <param name="element">The block to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The block passed is null.</exception>
	public void Add(BlockElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		((IAddChild)this).AddChild((object)element);
	}

	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	/// <param name="value">The child <see cref="T:System.Object" /> that is added.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks.</exception>
	void IAddChild.AddChild(object value)
	{
		if (value is SectionStructure || value is ParagraphStructure || value is FigureStructure || value is ListStructure || value is TableStructure || value is StoryBreak)
		{
			_elementList.Add((BlockElement)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.DocumentStructureUnexpectedParameterType6, value.GetType(), typeof(SectionStructure), typeof(ParagraphStructure), typeof(FigureStructure), typeof(ListStructure), typeof(TableStructure), typeof(StoryBreak)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<BlockElement> IEnumerable<BlockElement>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This method has not been implemented.</summary>
	/// <returns>Always raises <see cref="T:System.NotSupportedException" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<BlockElement>)this).GetEnumerator();
	}
}
