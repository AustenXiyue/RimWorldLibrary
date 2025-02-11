using System.Windows.Markup;

namespace System.Windows.Controls.Primitives;

/// <summary>
///   <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> is used to describe the position of an item that is managed by <see cref="T:System.Windows.Controls.ItemContainerGenerator" />.</summary>
public struct GeneratorPosition
{
	private int _index;

	private int _offset;

	/// <summary>Gets or sets the <see cref="T:System.Int32" /> index that is relative to the generated (realized) items.</summary>
	/// <returns>An <see cref="T:System.Int32" /> index that is relative to the generated (realized) items.</returns>
	public int Index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Int32" /> offset that is relative to the ungenerated (unrealized) items near the indexed item.</summary>
	/// <returns>An <see cref="T:System.Int32" /> offset that is relative to the ungenerated (unrealized) items near the indexed item.</returns>
	public int Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
		}
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> with the specified index and offset.</summary>
	/// <param name="index">An <see cref="T:System.Int32" /> index that is relative to the generated (realized) items. -1 is a special value that refers to a fictitious item at the beginning or the end of the items list.</param>
	/// <param name="offset">An <see cref="T:System.Int32" /> offset that is relative to the ungenerated (unrealized) items near the indexed item. An offset of 0 refers to the indexed element itself, an offset 1 refers to the next ungenerated (unrealized) item, and an offset of -1 refers to the previous item.</param>
	public GeneratorPosition(int index, int offset)
	{
		_index = index;
		_offset = offset;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />.</summary>
	/// <returns>The hash code for this <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />.</returns>
	public override int GetHashCode()
	{
		return _index.GetHashCode() + _offset.GetHashCode();
	}

	/// <summary>Returns a string representation of this instance of <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />.</summary>
	/// <returns>A string representation of this instance of <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /></returns>
	public override string ToString()
	{
		return "GeneratorPosition (" + _index.ToString(TypeConverterHelper.InvariantEnglishUS) + "," + _offset.ToString(TypeConverterHelper.InvariantEnglishUS) + ")";
	}

	/// <summary>Compares the specified instance and the current instance of <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> for value equality.</summary>
	/// <returns>true if <paramref name="o" /> and this instance of <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> have the same values.</returns>
	/// <param name="o">The <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> instance to compare.</param>
	public override bool Equals(object o)
	{
		if (o is GeneratorPosition generatorPosition)
		{
			if (_index == generatorPosition._index)
			{
				return _offset == generatorPosition._offset;
			}
			return false;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> objects for value equality.</summary>
	/// <returns>true if the two objects are equal; otherwise, false.</returns>
	/// <param name="gp1">The first instance to compare.</param>
	/// <param name="gp2">The second instance to compare.</param>
	public static bool operator ==(GeneratorPosition gp1, GeneratorPosition gp2)
	{
		if (gp1._index == gp2._index)
		{
			return gp1._offset == gp2._offset;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> objects for value inequality.</summary>
	/// <returns>true if the values are not equal; otherwise, false.</returns>
	/// <param name="gp1">The first instance to compare.</param>
	/// <param name="gp2">The second instance to compare.</param>
	public static bool operator !=(GeneratorPosition gp1, GeneratorPosition gp2)
	{
		return !(gp1 == gp2);
	}
}
