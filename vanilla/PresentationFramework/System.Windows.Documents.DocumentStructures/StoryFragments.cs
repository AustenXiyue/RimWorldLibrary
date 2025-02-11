using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a set of one or more <see cref="T:System.Windows.Documents.DocumentStructures.StoryFragment" /> elements.</summary>
[ContentProperty("StoryFragmentList")]
public class StoryFragments : IAddChild, IEnumerable<StoryFragment>, IEnumerable
{
	private List<StoryFragment> _elementList;

	internal List<StoryFragment> StoryFragmentList => _elementList;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.StoryFragments" /> class. </summary>
	public StoryFragments()
	{
		_elementList = new List<StoryFragment>();
	}

	/// <summary>Adds a <see cref="T:System.Windows.Documents.DocumentStructures.StoryFragment" /> to the <see cref="T:System.Windows.Documents.DocumentStructures.StoryFragments" /> collection.</summary>
	/// <param name="storyFragment">The <see cref="T:System.Windows.Documents.DocumentStructures.StoryFragment" /> to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Windows.Documents.DocumentStructures.StoryFragment" /> is null.</exception>
	public void Add(StoryFragment storyFragment)
	{
		if (storyFragment == null)
		{
			throw new ArgumentNullException("storyFragment");
		}
		((IAddChild)this).AddChild((object)storyFragment);
	}

	/// <summary>Adds a child object to the <see cref="T:System.Windows.Documents.DocumentStructures.StoryFragments" />.</summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value is StoryFragment)
		{
			_elementList.Add((StoryFragment)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(StoryFragment)), "value");
	}

	/// <summary>Adds the text content of a node to the object.</summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<StoryFragment> IEnumerable<StoryFragment>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This API is not implemented.</summary>
	/// <returns>This API is not implemented.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<StoryFragment>)this).GetEnumerator();
	}
}
