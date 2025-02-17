using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.U2D;

[Serializable]
[NativeHeader("Runtime/2D/Common/SpriteDataMarshalling.h")]
[NativeHeader("Runtime/2D/Common/SpriteDataAccess.h")]
[NativeType(CodegenOptions.Custom, "ScriptingSpriteBone")]
[RequiredByNativeCode]
[MovedFrom("UnityEngine.Experimental.U2D")]
public struct SpriteBone
{
	[NativeName("name")]
	[SerializeField]
	private string m_Name;

	[NativeName("position")]
	[SerializeField]
	private Vector3 m_Position;

	[NativeName("rotation")]
	[SerializeField]
	private Quaternion m_Rotation;

	[SerializeField]
	[NativeName("length")]
	private float m_Length;

	[NativeName("parentId")]
	[SerializeField]
	private int m_ParentId;

	public string name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
		}
	}

	public Vector3 position
	{
		get
		{
			return m_Position;
		}
		set
		{
			m_Position = value;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return m_Rotation;
		}
		set
		{
			m_Rotation = value;
		}
	}

	public float length
	{
		get
		{
			return m_Length;
		}
		set
		{
			m_Length = value;
		}
	}

	public int parentId
	{
		get
		{
			return m_ParentId;
		}
		set
		{
			m_ParentId = value;
		}
	}
}
