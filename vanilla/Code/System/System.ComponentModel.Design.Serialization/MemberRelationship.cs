namespace System.ComponentModel.Design.Serialization;

/// <summary>Represents a single relationship between an object and a member.</summary>
public struct MemberRelationship
{
	private object _owner;

	private MemberDescriptor _member;

	/// <summary>Represents the empty member relationship. This field is read-only.</summary>
	public static readonly MemberRelationship Empty;

	/// <summary>Gets a value indicating whether this relationship is equal to the <see cref="F:System.ComponentModel.Design.Serialization.MemberRelationship.Empty" /> relationship. </summary>
	/// <returns>true if this relationship is equal to the <see cref="F:System.ComponentModel.Design.Serialization.MemberRelationship.Empty" /> relationship; otherwise, false.</returns>
	public bool IsEmpty => _owner == null;

	/// <summary>Gets the related member.</summary>
	/// <returns>The member that is passed in to the <see cref="M:System.ComponentModel.Design.Serialization.MemberRelationship.#ctor(System.Object,System.ComponentModel.MemberDescriptor)" />.</returns>
	public MemberDescriptor Member => _member;

	/// <summary>Gets the owning object.</summary>
	/// <returns>The owning object that is passed in to the <see cref="M:System.ComponentModel.Design.Serialization.MemberRelationship.#ctor(System.Object,System.ComponentModel.MemberDescriptor)" />.</returns>
	public object Owner => _owner;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> class. </summary>
	/// <param name="owner">The object that owns <paramref name="member" />.</param>
	/// <param name="member">The member which is to be related to <paramref name="owner" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="owner" /> or <paramref name="member" /> is null.</exception>
	public MemberRelationship(object owner, MemberDescriptor member)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		_owner = owner;
		_member = member;
	}

	/// <summary>Determines whether two <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> instances are equal.</summary>
	/// <returns>true if the specified <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> is equal to the current <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> to compare with the current <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" />.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is MemberRelationship memberRelationship))
		{
			return false;
		}
		if (memberRelationship.Owner == Owner)
		{
			return memberRelationship.Member == Member;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" />.</returns>
	public override int GetHashCode()
	{
		if (_owner == null)
		{
			return base.GetHashCode();
		}
		return _owner.GetHashCode() ^ _member.GetHashCode();
	}

	/// <summary>Tests whether two specified <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structures are equivalent.</summary>
	/// <returns>This operator returns true if the two <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structures are equal; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structure that is to the left of the equality operator.</param>
	/// <param name="right">The <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structure that is to the right of the equality operator.</param>
	public static bool operator ==(MemberRelationship left, MemberRelationship right)
	{
		if (left.Owner == right.Owner)
		{
			return left.Member == right.Member;
		}
		return false;
	}

	/// <summary>Tests whether two specified <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structures are different.</summary>
	/// <returns>This operator returns true if the two <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structures are different; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structure that is to the left of the inequality operator.</param>
	/// <param name="right">The <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structure that is to the right of the inequality operator.</param>
	public static bool operator !=(MemberRelationship left, MemberRelationship right)
	{
		return !(left == right);
	}
}
