using System.Runtime.Serialization;

namespace System.Xaml;

/// <summary>The exception that is thrown when a XAML writer attempts to write a value for a duplicate member into the same object node. </summary>
[Serializable]
public class XamlDuplicateMemberException : XamlException
{
	/// <summary>Gets or sets the XAML member identifier for the property to report as a duplicate.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlMember" /> object (XAML member identifier) to report.</returns>
	public XamlMember DuplicateMember { get; set; }

	/// <summary>Gets or sets the XAML type identifier to report as the parent type.</summary>
	/// <returns>The <see cref="T:System.Xaml.XamlType" /> object (XAML type identifier) to report as the parent type.</returns>
	public XamlType ParentType { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDuplicateMemberException" /> class with a system-supplied message that describes the error.</summary>
	public XamlDuplicateMemberException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDuplicateMemberException" /> class with the relevant member and type information to report.</summary>
	/// <param name="member">The XAML member identifier to report.</param>
	/// <param name="type">The XAML type identifier to report as the parent type.</param>
	public XamlDuplicateMemberException(XamlMember member, XamlType type)
		: base(System.SR.Format(System.SR.DuplicateMemberSet, (member != null) ? member.Name : null, (type != null) ? type.Name : null))
	{
		DuplicateMember = member;
		ParentType = type;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDuplicateMemberException" /> class with a specified message that describes the error.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
	public XamlDuplicateMemberException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDuplicateMemberException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public XamlDuplicateMemberException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDuplicateMemberException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	protected XamlDuplicateMemberException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		ArgumentNullException.ThrowIfNull(info, "info");
		DuplicateMember = (XamlMember)info.GetValue("DuplicateMember", typeof(XamlMember));
		ParentType = (XamlType)info.GetValue("ParentType", typeof(XamlType));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDuplicateMemberException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		ArgumentNullException.ThrowIfNull(info, "info");
		info.AddValue("DuplicateMember", DuplicateMember);
		info.AddValue("ParentType", ParentType);
		base.GetObjectData(info, context);
	}
}
