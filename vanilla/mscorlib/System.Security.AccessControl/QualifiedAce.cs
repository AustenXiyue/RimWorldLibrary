using Unity;

namespace System.Security.AccessControl;

/// <summary>Represents an Access Control Entry (ACE) that contains a qualifier. The qualifier, represented by an <see cref="T:System.Security.AccessControl.AceQualifier" /> object, specifies whether the ACE allows access, denies access, causes system audits, or causes system alarms. The <see cref="T:System.Security.AccessControl.QualifiedAce" /> class is the abstract base class for the <see cref="T:System.Security.AccessControl.CommonAce" /> and <see cref="T:System.Security.AccessControl.ObjectAce" /> classes.</summary>
public abstract class QualifiedAce : KnownAce
{
	private byte[] opaque;

	/// <summary>Gets a value that specifies whether the ACE allows access, denies access, causes system audits, or causes system alarms.</summary>
	/// <returns>A value that specifies whether the ACE allows access, denies access, causes system audits, or causes system alarms.</returns>
	public AceQualifier AceQualifier
	{
		get
		{
			switch (base.AceType)
			{
			case AceType.AccessAllowed:
			case AceType.AccessAllowedCompound:
			case AceType.AccessAllowedObject:
			case AceType.AccessAllowedCallback:
			case AceType.AccessAllowedCallbackObject:
				return AceQualifier.AccessAllowed;
			case AceType.AccessDenied:
			case AceType.AccessDeniedObject:
			case AceType.AccessDeniedCallback:
			case AceType.AccessDeniedCallbackObject:
				return AceQualifier.AccessDenied;
			case AceType.SystemAlarm:
			case AceType.SystemAlarmObject:
			case AceType.SystemAlarmCallback:
			case AceType.SystemAlarmCallbackObject:
				return AceQualifier.SystemAlarm;
			case AceType.SystemAudit:
			case AceType.SystemAuditObject:
			case AceType.SystemAuditCallback:
			case AceType.SystemAuditCallbackObject:
				return AceQualifier.SystemAudit;
			default:
				throw new ArgumentException("Unrecognised ACE type: " + base.AceType);
			}
		}
	}

	/// <summary>Specifies whether this <see cref="T:System.Security.AccessControl.QualifiedAce" /> object contains callback data.</summary>
	/// <returns>true if this <see cref="T:System.Security.AccessControl.QualifiedAce" /> object contains callback data; otherwise, false.</returns>
	public bool IsCallback
	{
		get
		{
			if (base.AceType != AceType.AccessAllowedCallback && base.AceType != AceType.AccessAllowedCallbackObject && base.AceType != AceType.AccessDeniedCallback && base.AceType != AceType.AccessDeniedCallbackObject && base.AceType != AceType.SystemAlarmCallback && base.AceType != AceType.SystemAlarmCallbackObject && base.AceType != AceType.SystemAuditCallback)
			{
				return base.AceType == AceType.SystemAuditCallbackObject;
			}
			return true;
		}
	}

	/// <summary>Gets the length of the opaque callback data associated with this <see cref="T:System.Security.AccessControl.QualifiedAce" /> object. This property is valid only for callback Access Control Entries (ACEs).</summary>
	/// <returns>The length of the opaque callback data.</returns>
	public int OpaqueLength
	{
		get
		{
			if (opaque == null)
			{
				return 0;
			}
			return opaque.Length;
		}
	}

	internal QualifiedAce(AceType type, AceFlags flags, byte[] opaque)
		: base(type, flags)
	{
		SetOpaque(opaque);
	}

	internal QualifiedAce(byte[] binaryForm, int offset)
		: base(binaryForm, offset)
	{
	}

	/// <summary>Returns the opaque callback data associated with this <see cref="T:System.Security.AccessControl.QualifiedAce" /> object. </summary>
	/// <returns>An array of byte values that represents the opaque callback data associated with this <see cref="T:System.Security.AccessControl.QualifiedAce" /> object.</returns>
	public byte[] GetOpaque()
	{
		if (opaque == null)
		{
			return null;
		}
		return (byte[])opaque.Clone();
	}

	/// <summary>Sets the opaque callback data associated with this <see cref="T:System.Security.AccessControl.QualifiedAce" /> object.</summary>
	/// <param name="opaque">An array of byte values that represents the opaque callback data for this <see cref="T:System.Security.AccessControl.QualifiedAce" /> object.</param>
	public void SetOpaque(byte[] opaque)
	{
		if (opaque == null)
		{
			this.opaque = null;
		}
		else
		{
			this.opaque = (byte[])opaque.Clone();
		}
	}

	internal QualifiedAce()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
