using System;
using UnityEngine.Scripting;

namespace UnityEngine;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[RequiredByNativeCode]
public class RuntimeInitializeOnLoadMethodAttribute : PreserveAttribute
{
	private RuntimeInitializeLoadType m_LoadType;

	public RuntimeInitializeLoadType loadType
	{
		get
		{
			return m_LoadType;
		}
		private set
		{
			m_LoadType = value;
		}
	}

	public RuntimeInitializeOnLoadMethodAttribute()
	{
		loadType = RuntimeInitializeLoadType.AfterSceneLoad;
	}

	public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType)
	{
		this.loadType = loadType;
	}
}
