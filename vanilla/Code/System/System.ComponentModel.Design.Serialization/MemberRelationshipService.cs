using System.Collections.Generic;
using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides the base class for relating one member to another.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class MemberRelationshipService
{
	private struct RelationshipEntry
	{
		internal WeakReference Owner;

		internal MemberDescriptor Member;

		private int hashCode;

		internal RelationshipEntry(MemberRelationship rel)
		{
			Owner = new WeakReference(rel.Owner);
			Member = rel.Member;
			hashCode = ((rel.Owner != null) ? rel.Owner.GetHashCode() : 0);
		}

		public override bool Equals(object o)
		{
			if (o is RelationshipEntry relationshipEntry)
			{
				return this == relationshipEntry;
			}
			return false;
		}

		public static bool operator ==(RelationshipEntry re1, RelationshipEntry re2)
		{
			object obj = (re1.Owner.IsAlive ? re1.Owner.Target : null);
			object obj2 = (re2.Owner.IsAlive ? re2.Owner.Target : null);
			if (obj == obj2)
			{
				return re1.Member.Equals(re2.Member);
			}
			return false;
		}

		public static bool operator !=(RelationshipEntry re1, RelationshipEntry re2)
		{
			return !(re1 == re2);
		}

		public override int GetHashCode()
		{
			return hashCode;
		}
	}

	private Dictionary<RelationshipEntry, RelationshipEntry> _relationships = new Dictionary<RelationshipEntry, RelationshipEntry>();

	/// <summary>Establishes a relationship between a source and target object.</summary>
	/// <returns>The current relationship associated with <paramref name="source" />, or <see cref="F:System.ComponentModel.Design.Serialization.MemberRelationship.Empty" /> if there is no relationship.</returns>
	/// <param name="source">The source relationship. This is the left-hand side of a relationship assignment.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> is empty, or the relationship is not supported by the service.</exception>
	public MemberRelationship this[MemberRelationship source]
	{
		get
		{
			if (source.Owner == null)
			{
				throw new ArgumentNullException("Owner");
			}
			if (source.Member == null)
			{
				throw new ArgumentNullException("Member");
			}
			return GetRelationship(source);
		}
		set
		{
			if (source.Owner == null)
			{
				throw new ArgumentNullException("Owner");
			}
			if (source.Member == null)
			{
				throw new ArgumentNullException("Member");
			}
			SetRelationship(source, value);
		}
	}

	/// <summary>Establishes a relationship between a source and target object.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationship" /> structure encapsulating the relationship between a source and target object, or null if there is no relationship.</returns>
	/// <param name="sourceOwner">The owner of a source relationship.</param>
	/// <param name="sourceMember">The member of a source relationship.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceOwner" /> or <paramref name="sourceMember" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceOwner" /> or <paramref name="sourceMember" /> is empty, or the relationship is not supported by the service.</exception>
	public MemberRelationship this[object sourceOwner, MemberDescriptor sourceMember]
	{
		get
		{
			if (sourceOwner == null)
			{
				throw new ArgumentNullException("sourceOwner");
			}
			if (sourceMember == null)
			{
				throw new ArgumentNullException("sourceMember");
			}
			return GetRelationship(new MemberRelationship(sourceOwner, sourceMember));
		}
		set
		{
			if (sourceOwner == null)
			{
				throw new ArgumentNullException("sourceOwner");
			}
			if (sourceMember == null)
			{
				throw new ArgumentNullException("sourceMember");
			}
			SetRelationship(new MemberRelationship(sourceOwner, sourceMember), value);
		}
	}

	/// <summary>Gets a relationship to the given source relationship.</summary>
	/// <returns>A relationship to <paramref name="source" />, or <see cref="F:System.ComponentModel.Design.Serialization.MemberRelationship.Empty" /> if no relationship exists.</returns>
	/// <param name="source">The source relationship.</param>
	protected virtual MemberRelationship GetRelationship(MemberRelationship source)
	{
		if (_relationships != null && _relationships.TryGetValue(new RelationshipEntry(source), out var value) && value.Owner.IsAlive)
		{
			return new MemberRelationship(value.Owner.Target, value.Member);
		}
		return MemberRelationship.Empty;
	}

	/// <summary>Creates a relationship between the source object and target relationship.</summary>
	/// <param name="source">The source relationship.</param>
	/// <param name="relationship">The relationship to set into the source.</param>
	/// <exception cref="T:System.ArgumentException">The relationship is not supported by the service.</exception>
	protected virtual void SetRelationship(MemberRelationship source, MemberRelationship relationship)
	{
		if (!relationship.IsEmpty && !SupportsRelationship(source, relationship))
		{
			string text = TypeDescriptor.GetComponentName(source.Owner);
			string text2 = TypeDescriptor.GetComponentName(relationship.Owner);
			if (text == null)
			{
				text = source.Owner.ToString();
			}
			if (text2 == null)
			{
				text2 = relationship.Owner.ToString();
			}
			throw new ArgumentException(global::SR.GetString("Relationships between {0}.{1} and {2}.{3} are not supported.", text, source.Member.Name, text2, relationship.Member.Name));
		}
		if (_relationships == null)
		{
			_relationships = new Dictionary<RelationshipEntry, RelationshipEntry>();
		}
		_relationships[new RelationshipEntry(source)] = new RelationshipEntry(relationship);
	}

	/// <summary>Gets a value indicating whether the given relationship is supported.</summary>
	/// <returns>true if a relationship between the given two objects is supported; otherwise, false.</returns>
	/// <param name="source">The source relationship.</param>
	/// <param name="relationship">The relationship to set into the source.</param>
	public abstract bool SupportsRelationship(MemberRelationship source, MemberRelationship relationship);

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.MemberRelationshipService" /> class. </summary>
	protected MemberRelationshipService()
	{
	}
}
