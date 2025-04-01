using System.Runtime.Serialization;
using System.Security;
using Unity;

namespace System.Runtime.CompilerServices;

/// <summary>Wraps an exception that does not derive from the <see cref="T:System.Exception" /> class. This class cannot be inherited.</summary>
[Serializable]
public sealed class RuntimeWrappedException : Exception
{
	private object m_wrappedException;

	/// <summary>Gets the object that was wrapped by the <see cref="T:System.Runtime.CompilerServices.RuntimeWrappedException" /> object.</summary>
	/// <returns>The object that was wrapped by the <see cref="T:System.Runtime.CompilerServices.RuntimeWrappedException" /> object.</returns>
	public object WrappedException => m_wrappedException;

	private RuntimeWrappedException(object thrownObject)
		: base(Environment.GetResourceString("An object that does not derive from System.Exception has been wrapped in a RuntimeWrappedException."))
	{
		SetErrorCode(-2146233026);
		m_wrappedException = thrownObject;
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with information about the exception.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null.</exception>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("WrappedException", m_wrappedException, typeof(object));
	}

	internal RuntimeWrappedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		m_wrappedException = info.GetValue("WrappedException", typeof(object));
	}

	internal RuntimeWrappedException()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
