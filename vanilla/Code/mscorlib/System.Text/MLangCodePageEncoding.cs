using System.Runtime.Serialization;
using System.Security;

namespace System.Text;

[Serializable]
internal sealed class MLangCodePageEncoding : ISerializable, IObjectReference
{
	[Serializable]
	internal sealed class MLangEncoder : ISerializable, IObjectReference
	{
		[NonSerialized]
		private Encoding realEncoding;

		internal MLangEncoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			realEncoding = (Encoding)info.GetValue("m_encoding", typeof(Encoding));
		}

		[SecurityCritical]
		public object GetRealObject(StreamingContext context)
		{
			return realEncoding.GetEncoder();
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new ArgumentException(Environment.GetResourceString("Internal error in the runtime."));
		}
	}

	[Serializable]
	internal sealed class MLangDecoder : ISerializable, IObjectReference
	{
		[NonSerialized]
		private Encoding realEncoding;

		internal MLangDecoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			realEncoding = (Encoding)info.GetValue("m_encoding", typeof(Encoding));
		}

		[SecurityCritical]
		public object GetRealObject(StreamingContext context)
		{
			return realEncoding.GetDecoder();
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new ArgumentException(Environment.GetResourceString("Internal error in the runtime."));
		}
	}

	[NonSerialized]
	private int m_codePage;

	[NonSerialized]
	private bool m_isReadOnly;

	[NonSerialized]
	private bool m_deserializedFromEverett;

	[NonSerialized]
	private EncoderFallback encoderFallback;

	[NonSerialized]
	private DecoderFallback decoderFallback;

	[NonSerialized]
	private Encoding realEncoding;

	internal MLangCodePageEncoding(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		m_codePage = (int)info.GetValue("m_codePage", typeof(int));
		try
		{
			m_isReadOnly = (bool)info.GetValue("m_isReadOnly", typeof(bool));
			encoderFallback = (EncoderFallback)info.GetValue("encoderFallback", typeof(EncoderFallback));
			decoderFallback = (DecoderFallback)info.GetValue("decoderFallback", typeof(DecoderFallback));
		}
		catch (SerializationException)
		{
			m_deserializedFromEverett = true;
			m_isReadOnly = true;
		}
	}

	[SecurityCritical]
	public object GetRealObject(StreamingContext context)
	{
		realEncoding = Encoding.GetEncoding(m_codePage);
		if (!m_deserializedFromEverett && !m_isReadOnly)
		{
			realEncoding = (Encoding)realEncoding.Clone();
			realEncoding.EncoderFallback = encoderFallback;
			realEncoding.DecoderFallback = decoderFallback;
		}
		return realEncoding;
	}

	[SecurityCritical]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new ArgumentException(Environment.GetResourceString("Internal error in the runtime."));
	}
}
