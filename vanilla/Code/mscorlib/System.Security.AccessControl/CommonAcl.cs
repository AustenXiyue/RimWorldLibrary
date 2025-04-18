using System.Security.Principal;
using Unity;

namespace System.Security.AccessControl;

/// <summary>Represents an access control list (ACL) and is the base class for the <see cref="T:System.Security.AccessControl.DiscretionaryAcl" /> and <see cref="T:System.Security.AccessControl.SystemAcl" /> classes.</summary>
public abstract class CommonAcl : GenericAcl
{
	internal delegate bool RemoveAcesCallback<T>(T ace);

	private const int default_capacity = 10;

	private bool is_aefa;

	private bool is_canonical;

	private bool is_container;

	private bool is_ds;

	internal RawAcl raw_acl;

	/// <summary>Gets the length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object. This length should be used before marshaling the access control list (ACL) into a binary array by using the <see cref="M:System.Security.AccessControl.CommonAcl.GetBinaryForm" /> method.</summary>
	/// <returns>The length, in bytes, of the binary representation of the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object.</returns>
	public sealed override int BinaryLength => raw_acl.BinaryLength;

	/// <summary>Gets the number of access control entries (ACEs) in the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object.</summary>
	/// <returns>The number of ACEs in the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object.</returns>
	public sealed override int Count => raw_acl.Count;

	/// <summary>Gets a Boolean value that specifies whether the access control entries (ACEs) in the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object are in canonical order.</summary>
	/// <returns>true if the ACEs in the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object are in canonical order; otherwise, false.</returns>
	public bool IsCanonical => is_canonical;

	/// <summary>Sets whether the <see cref="T:System.Security.AccessControl.CommonAcl" /> object is a container. </summary>
	/// <returns>true if the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object is a container.</returns>
	public bool IsContainer => is_container;

	/// <summary>Sets whether the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object is a directory object access control list (ACL).</summary>
	/// <returns>true if the current <see cref="T:System.Security.AccessControl.CommonAcl" /> object is a directory object ACL.</returns>
	public bool IsDS => is_ds;

	internal bool IsAefa
	{
		get
		{
			return is_aefa;
		}
		set
		{
			is_aefa = value;
		}
	}

	/// <summary>Gets the revision level of the <see cref="T:System.Security.AccessControl.CommonAcl" />.</summary>
	/// <returns>A byte value that specifies the revision level of the <see cref="T:System.Security.AccessControl.CommonAcl" />.</returns>
	public sealed override byte Revision => raw_acl.Revision;

	/// <summary>Gets or sets the <see cref="T:System.Security.AccessControl.CommonAce" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.CommonAce" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Security.AccessControl.CommonAce" /> to get or set.</param>
	public sealed override GenericAce this[int index]
	{
		get
		{
			return CopyAce(raw_acl[index]);
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	internal CommonAcl(bool isContainer, bool isDS, RawAcl rawAcl)
	{
		if (rawAcl == null)
		{
			rawAcl = new RawAcl(isDS ? GenericAcl.AclRevisionDS : GenericAcl.AclRevision, 10);
		}
		else
		{
			byte[] binaryForm = new byte[rawAcl.BinaryLength];
			rawAcl.GetBinaryForm(binaryForm, 0);
			rawAcl = new RawAcl(binaryForm, 0);
		}
		Init(isContainer, isDS, rawAcl);
	}

	internal CommonAcl(bool isContainer, bool isDS, byte revision, int capacity)
	{
		Init(isContainer, isDS, new RawAcl(revision, capacity));
	}

	internal CommonAcl(bool isContainer, bool isDS, int capacity)
		: this(isContainer, isDS, isDS ? GenericAcl.AclRevisionDS : GenericAcl.AclRevision, capacity)
	{
	}

	private void Init(bool isContainer, bool isDS, RawAcl rawAcl)
	{
		is_container = isContainer;
		is_ds = isDS;
		raw_acl = rawAcl;
		CanonicalizeAndClearAefa();
	}

	/// <summary>Marshals the contents of the <see cref="T:System.Security.AccessControl.CommonAcl" /> object into the specified byte array beginning at the specified offset.</summary>
	/// <param name="binaryForm">The byte array into which the contents of the <see cref="T:System.Security.AccessControl.CommonAcl" /> is marshaled.</param>
	/// <param name="offset">The offset at which to start marshaling.</param>
	public sealed override void GetBinaryForm(byte[] binaryForm, int offset)
	{
		raw_acl.GetBinaryForm(binaryForm, offset);
	}

	/// <summary>Removes all access control entries (ACEs) contained by this <see cref="T:System.Security.AccessControl.CommonAcl" /> object that are associated with the specified <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</summary>
	/// <param name="sid">The <see cref="T:System.Security.Principal.SecurityIdentifier" /> object to check for.</param>
	public void Purge(SecurityIdentifier sid)
	{
		RequireCanonicity();
		RemoveAces((KnownAce ace) => ace.SecurityIdentifier == sid);
	}

	/// <summary>Removes all inherited access control entries (ACEs) from this <see cref="T:System.Security.AccessControl.CommonAcl" /> object.</summary>
	public void RemoveInheritedAces()
	{
		RequireCanonicity();
		RemoveAces((GenericAce ace) => ace.IsInherited);
	}

	internal void RequireCanonicity()
	{
		if (!IsCanonical)
		{
			throw new InvalidOperationException("ACL is not canonical.");
		}
	}

	internal void CanonicalizeAndClearAefa()
	{
		RemoveAces<GenericAce>(IsAceMeaningless);
		is_canonical = TestCanonicity();
		if (IsCanonical)
		{
			ApplyCanonicalSortToExplicitAces();
			MergeExplicitAces();
		}
		IsAefa = false;
	}

	internal virtual bool IsAceMeaningless(GenericAce ace)
	{
		AceFlags aceFlags = ace.AceFlags;
		KnownAce knownAce = ace as KnownAce;
		if (knownAce != null)
		{
			if (knownAce.AccessMask == 0)
			{
				return true;
			}
			if ((aceFlags & AceFlags.InheritOnly) != 0)
			{
				if (knownAce is ObjectAce)
				{
					return true;
				}
				if (!IsContainer)
				{
					return true;
				}
				if ((aceFlags & (AceFlags.ObjectInherit | AceFlags.ContainerInherit)) == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool TestCanonicity()
	{
		AceEnumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!(enumerator.Current is QualifiedAce))
			{
				return false;
			}
		}
		bool flag = false;
		enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (((QualifiedAce)enumerator.Current).IsInherited)
			{
				flag = true;
			}
			else if (flag)
			{
				return false;
			}
		}
		bool flag2 = false;
		enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			QualifiedAce qualifiedAce = (QualifiedAce)enumerator.Current;
			if (qualifiedAce.IsInherited)
			{
				break;
			}
			if (qualifiedAce.AceQualifier == AceQualifier.AccessAllowed)
			{
				flag2 = true;
			}
			else if (AceQualifier.AccessDenied == qualifiedAce.AceQualifier && flag2)
			{
				return false;
			}
		}
		return true;
	}

	internal int GetCanonicalExplicitDenyAceCount()
	{
		int i;
		for (i = 0; i < Count && !raw_acl[i].IsInherited; i++)
		{
			QualifiedAce qualifiedAce = raw_acl[i] as QualifiedAce;
			if (qualifiedAce == null || qualifiedAce.AceQualifier != AceQualifier.AccessDenied)
			{
				break;
			}
		}
		return i;
	}

	internal int GetCanonicalExplicitAceCount()
	{
		int i;
		for (i = 0; i < Count && !raw_acl[i].IsInherited; i++)
		{
		}
		return i;
	}

	private void MergeExplicitAces()
	{
		int num = GetCanonicalExplicitAceCount();
		int num2 = 0;
		while (num2 < num - 1)
		{
			GenericAce genericAce = MergeExplicitAcePair(raw_acl[num2], raw_acl[num2 + 1]);
			if (null != genericAce)
			{
				raw_acl[num2] = genericAce;
				raw_acl.RemoveAce(num2 + 1);
				num--;
			}
			else
			{
				num2++;
			}
		}
	}

	private GenericAce MergeExplicitAcePair(GenericAce ace1, GenericAce ace2)
	{
		QualifiedAce qualifiedAce = ace1 as QualifiedAce;
		QualifiedAce qualifiedAce2 = ace2 as QualifiedAce;
		if (!(null != qualifiedAce) || !(null != qualifiedAce2))
		{
			return null;
		}
		if (qualifiedAce.AceQualifier != qualifiedAce2.AceQualifier)
		{
			return null;
		}
		if (!(qualifiedAce.SecurityIdentifier == qualifiedAce2.SecurityIdentifier))
		{
			return null;
		}
		AceFlags aceFlags = qualifiedAce.AceFlags;
		AceFlags aceFlags2 = qualifiedAce2.AceFlags;
		int accessMask = qualifiedAce.AccessMask;
		int accessMask2 = qualifiedAce2.AccessMask;
		if (!IsContainer)
		{
			aceFlags &= ~AceFlags.InheritanceFlags;
			aceFlags2 &= ~AceFlags.InheritanceFlags;
		}
		AceFlags aceFlags3;
		int accessMask3;
		if (aceFlags != aceFlags2)
		{
			if (accessMask != accessMask2)
			{
				return null;
			}
			if ((aceFlags & ~(AceFlags.ObjectInherit | AceFlags.ContainerInherit)) == (aceFlags2 & ~(AceFlags.ObjectInherit | AceFlags.ContainerInherit)))
			{
				aceFlags3 = aceFlags | aceFlags2;
				accessMask3 = accessMask;
			}
			else
			{
				if ((aceFlags & ~AceFlags.AuditFlags) != (aceFlags2 & ~AceFlags.AuditFlags))
				{
					return null;
				}
				aceFlags3 = aceFlags | aceFlags2;
				accessMask3 = accessMask;
			}
		}
		else
		{
			aceFlags3 = aceFlags;
			accessMask3 = accessMask | accessMask2;
		}
		CommonAce commonAce = ace1 as CommonAce;
		CommonAce commonAce2 = ace2 as CommonAce;
		if (null != commonAce && null != commonAce2)
		{
			return new CommonAce(aceFlags3, commonAce.AceQualifier, accessMask3, commonAce.SecurityIdentifier, commonAce.IsCallback, commonAce.GetOpaque());
		}
		ObjectAce objectAce = ace1 as ObjectAce;
		ObjectAce objectAce2 = ace2 as ObjectAce;
		if (null != objectAce && null != objectAce2)
		{
			GetObjectAceTypeGuids(objectAce, out var type, out var inheritedType);
			GetObjectAceTypeGuids(objectAce2, out var type2, out var inheritedType2);
			if (type == type2 && inheritedType == inheritedType2)
			{
				return new ObjectAce(aceFlags3, objectAce.AceQualifier, accessMask3, objectAce.SecurityIdentifier, objectAce.ObjectAceFlags, objectAce.ObjectAceType, objectAce.InheritedObjectAceType, objectAce.IsCallback, objectAce.GetOpaque());
			}
		}
		return null;
	}

	private static void GetObjectAceTypeGuids(ObjectAce ace, out Guid type, out Guid inheritedType)
	{
		type = Guid.Empty;
		inheritedType = Guid.Empty;
		if ((ace.ObjectAceFlags & ObjectAceFlags.ObjectAceTypePresent) != 0)
		{
			type = ace.ObjectAceType;
		}
		if ((ace.ObjectAceFlags & ObjectAceFlags.InheritedObjectAceTypePresent) != 0)
		{
			inheritedType = ace.InheritedObjectAceType;
		}
	}

	internal abstract void ApplyCanonicalSortToExplicitAces();

	internal void ApplyCanonicalSortToExplicitAces(int start, int count)
	{
		for (int i = start + 1; i < start + count; i++)
		{
			KnownAce knownAce = (KnownAce)raw_acl[i];
			SecurityIdentifier securityIdentifier = knownAce.SecurityIdentifier;
			int num = i;
			while (num > start && ((KnownAce)raw_acl[num - 1]).SecurityIdentifier.CompareTo(securityIdentifier) > 0)
			{
				raw_acl[num] = raw_acl[num - 1];
				num--;
			}
			raw_acl[num] = knownAce;
		}
	}

	internal override string GetSddlForm(ControlFlags sdFlags, bool isDacl)
	{
		return raw_acl.GetSddlForm(sdFlags, isDacl);
	}

	internal void RemoveAces<T>(RemoveAcesCallback<T> callback) where T : GenericAce
	{
		int num = 0;
		while (num < raw_acl.Count)
		{
			if (raw_acl[num] is T && callback((T)raw_acl[num]))
			{
				raw_acl.RemoveAce(num);
			}
			else
			{
				num++;
			}
		}
	}

	internal void AddAce(AceQualifier aceQualifier, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
	{
		QualifiedAce newAce = AddAceGetQualifiedAce(aceQualifier, sid, accessMask, inheritanceFlags, propagationFlags, auditFlags);
		AddAce(newAce);
	}

	internal void AddAce(AceQualifier aceQualifier, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
	{
		QualifiedAce newAce = AddAceGetQualifiedAce(aceQualifier, sid, accessMask, inheritanceFlags, propagationFlags, auditFlags, objectFlags, objectType, inheritedObjectType);
		AddAce(newAce);
	}

	private QualifiedAce AddAceGetQualifiedAce(AceQualifier aceQualifier, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
	{
		if (!IsDS)
		{
			throw new InvalidOperationException("For this overload, IsDS must be true.");
		}
		if (objectFlags == ObjectAceFlags.None)
		{
			return AddAceGetQualifiedAce(aceQualifier, sid, accessMask, inheritanceFlags, propagationFlags, auditFlags);
		}
		return new ObjectAce(GetAceFlags(inheritanceFlags, propagationFlags, auditFlags), aceQualifier, accessMask, sid, objectFlags, objectType, inheritedObjectType, isCallback: false, null);
	}

	private QualifiedAce AddAceGetQualifiedAce(AceQualifier aceQualifier, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
	{
		return new CommonAce(GetAceFlags(inheritanceFlags, propagationFlags, auditFlags), aceQualifier, accessMask, sid, isCallback: false, null);
	}

	private void AddAce(QualifiedAce newAce)
	{
		RequireCanonicity();
		int aceInsertPosition = GetAceInsertPosition(newAce.AceQualifier);
		raw_acl.InsertAce(aceInsertPosition, CopyAce(newAce));
		CanonicalizeAndClearAefa();
	}

	private static GenericAce CopyAce(GenericAce ace)
	{
		byte[] binaryForm = new byte[ace.BinaryLength];
		ace.GetBinaryForm(binaryForm, 0);
		return GenericAce.CreateFromBinaryForm(binaryForm, 0);
	}

	internal abstract int GetAceInsertPosition(AceQualifier aceQualifier);

	private AceFlags GetAceFlags(InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
	{
		if (inheritanceFlags != 0 && !IsContainer)
		{
			throw new ArgumentException("Flags only work with containers.", "inheritanceFlags");
		}
		if (inheritanceFlags == InheritanceFlags.None && propagationFlags != 0)
		{
			throw new ArgumentException("Propagation flags need inheritance flags.", "propagationFlags");
		}
		AceFlags aceFlags = AceFlags.None;
		if ((InheritanceFlags.ContainerInherit & inheritanceFlags) != 0)
		{
			aceFlags |= AceFlags.ContainerInherit;
		}
		if ((InheritanceFlags.ObjectInherit & inheritanceFlags) != 0)
		{
			aceFlags |= AceFlags.ObjectInherit;
		}
		if ((PropagationFlags.InheritOnly & propagationFlags) != 0)
		{
			aceFlags |= AceFlags.InheritOnly;
		}
		if ((PropagationFlags.NoPropagateInherit & propagationFlags) != 0)
		{
			aceFlags |= AceFlags.NoPropagateInherit;
		}
		if ((AuditFlags.Success & auditFlags) != 0)
		{
			aceFlags |= AceFlags.SuccessfulAccess;
		}
		if ((AuditFlags.Failure & auditFlags) != 0)
		{
			aceFlags |= AceFlags.FailedAccess;
		}
		return aceFlags;
	}

	internal void RemoveAceSpecific(AceQualifier aceQualifier, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
	{
		RequireCanonicity();
		RemoveAces(delegate(CommonAce ace)
		{
			if (ace.AccessMask != accessMask)
			{
				return false;
			}
			if (ace.AceQualifier != aceQualifier)
			{
				return false;
			}
			if (ace.SecurityIdentifier != sid)
			{
				return false;
			}
			if (ace.InheritanceFlags != inheritanceFlags)
			{
				return false;
			}
			if (inheritanceFlags != 0 && ace.PropagationFlags != propagationFlags)
			{
				return false;
			}
			return (ace.AuditFlags == auditFlags) ? true : false;
		});
		CanonicalizeAndClearAefa();
	}

	internal void RemoveAceSpecific(AceQualifier aceQualifier, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
	{
		if (!IsDS)
		{
			throw new InvalidOperationException("For this overload, IsDS must be true.");
		}
		if (objectFlags == ObjectAceFlags.None)
		{
			RemoveAceSpecific(aceQualifier, sid, accessMask, inheritanceFlags, propagationFlags, auditFlags);
			return;
		}
		RequireCanonicity();
		RemoveAces(delegate(ObjectAce ace)
		{
			if (ace.AccessMask != accessMask)
			{
				return false;
			}
			if (ace.AceQualifier != aceQualifier)
			{
				return false;
			}
			if (ace.SecurityIdentifier != sid)
			{
				return false;
			}
			if (ace.InheritanceFlags != inheritanceFlags)
			{
				return false;
			}
			if (inheritanceFlags != 0 && ace.PropagationFlags != propagationFlags)
			{
				return false;
			}
			if (ace.AuditFlags != auditFlags)
			{
				return false;
			}
			if (ace.ObjectAceFlags != objectFlags)
			{
				return false;
			}
			if ((objectFlags & ObjectAceFlags.ObjectAceTypePresent) != 0 && ace.ObjectAceType != objectType)
			{
				return false;
			}
			return ((objectFlags & ObjectAceFlags.InheritedObjectAceTypePresent) == 0 || !(ace.InheritedObjectAceType != objectType)) ? true : false;
		});
		CanonicalizeAndClearAefa();
	}

	internal void SetAce(AceQualifier aceQualifier, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
	{
		QualifiedAce ace = AddAceGetQualifiedAce(aceQualifier, sid, accessMask, inheritanceFlags, propagationFlags, auditFlags);
		SetAce(ace);
	}

	internal void SetAce(AceQualifier aceQualifier, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
	{
		QualifiedAce ace = AddAceGetQualifiedAce(aceQualifier, sid, accessMask, inheritanceFlags, propagationFlags, auditFlags, objectFlags, objectType, inheritedObjectType);
		SetAce(ace);
	}

	private void SetAce(QualifiedAce newAce)
	{
		RequireCanonicity();
		RemoveAces((QualifiedAce oldAce) => oldAce.AceQualifier == newAce.AceQualifier && oldAce.SecurityIdentifier == newAce.SecurityIdentifier);
		CanonicalizeAndClearAefa();
		AddAce(newAce);
	}

	internal CommonAcl()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
