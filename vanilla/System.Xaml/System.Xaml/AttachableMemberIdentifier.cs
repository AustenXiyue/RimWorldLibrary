namespace System.Xaml;

/// <summary>Provides a XAML type system identifier representation for attachable members. The identifier structure parallels the <paramref name="declaringType" />.<paramref name="memberName" /> string form for attachable member usage.</summary>
public class AttachableMemberIdentifier : IEquatable<AttachableMemberIdentifier>
{
	private readonly Type declaringType;

	private readonly string memberName;

	/// <summary>Gets or sets the <paramref name="memberName" /> component value of the <see cref="T:System.Xaml.AttachableMemberIdentifier" />.</summary>
	/// <returns>The <paramref name="memberName" /> component value of the <see cref="T:System.Xaml.AttachableMemberIdentifier" />.</returns>
	public string MemberName => memberName;

	/// <summary>Gets or sets the <paramref name="declaringType" /> component value of the <see cref="T:System.Xaml.AttachableMemberIdentifier" />.</summary>
	/// <returns>The <paramref name="declaringType" /> component value of the <see cref="T:System.Xaml.AttachableMemberIdentifier" />.</returns>
	public Type DeclaringType => declaringType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.AttachableMemberIdentifier" /> class.</summary>
	/// <param name="declaringType">The <paramref name="declaringType" /> component of the identifier, which should match the name of the declaring <see cref="P:System.Xaml.XamlMember.Type" />.</param>
	/// <param name="memberName">The <paramref name="memberName" /> component of the identifier, which should match the <see cref="P:System.Xaml.XamlMember.Name" />.</param>
	public AttachableMemberIdentifier(Type declaringType, string memberName)
	{
		this.declaringType = declaringType;
		this.memberName = memberName;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Xaml.AttachableMemberIdentifier" /> objects have different values. </summary>
	/// <returns>true if the value of <paramref name="left" /> differs from the value of <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">An <see cref="T:System.Xaml.AttachableMemberIdentifier" />, or null.</param>
	/// <param name="right">An <see cref="T:System.Xaml.AttachableMemberIdentifier" />, or null.</param>
	public static bool operator !=(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
	{
		return !(left == right);
	}

	/// <summary>Determines whether two specified <see cref="T:System.Xaml.AttachableMemberIdentifier" /> objects have the same value. </summary>
	/// <returns>true if the value of <paramref name="left" /> is the same as the value of <paramref name="right" />; otherwise, false. </returns>
	/// <param name="left">An <see cref="T:System.Xaml.AttachableMemberIdentifier" />, or null.</param>
	/// <param name="right">An <see cref="T:System.Xaml.AttachableMemberIdentifier" />, or null.</param>
	public static bool operator ==(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	/// <summary>Determines whether this instance of <see cref="T:System.Xaml.AttachableMemberIdentifier" /> and a specified object have the same value.</summary>
	/// <returns>true if <paramref name="obj" /> is an <see cref="T:System.Xaml.AttachableMemberIdentifier" /> and if its value is the same as this instance; otherwise, false. </returns>
	/// <param name="obj">The object to compare with the current <see cref="T:System.Xaml.AttachableMemberIdentifier" />. </param>
	public override bool Equals(object obj)
	{
		return Equals(obj as AttachableMemberIdentifier);
	}

	/// <summary>Determines whether this instance and another specified <see cref="T:System.Xaml.AttachableMemberIdentifier" /> object have the same value.</summary>
	/// <returns>true if the objects have the same value; otherwise, false.</returns>
	/// <param name="other">The <see cref="T:System.Xaml.AttachableMemberIdentifier" /> to compare with the current <see cref="T:System.Xaml.AttachableMemberIdentifier" />. </param>
	public bool Equals(AttachableMemberIdentifier other)
	{
		if (other == null)
		{
			return false;
		}
		if (declaringType == other.declaringType)
		{
			return memberName == other.memberName;
		}
		return false;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Xaml.AttachableMemberIdentifier" />. </summary>
	/// <returns>An integer hash code. </returns>
	public override int GetHashCode()
	{
		int num = ((!(declaringType == null)) ? declaringType.GetHashCode() : 0);
		int num2 = ((memberName != null) ? memberName.GetHashCode() : 0);
		return ((num << 5) + num) ^ num2;
	}

	/// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Xaml.AttachableMemberIdentifier" />. </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Xaml.AttachableMemberIdentifier" />. </returns>
	public override string ToString()
	{
		if (declaringType == null)
		{
			return memberName;
		}
		return declaringType.ToString() + "." + memberName;
	}
}
