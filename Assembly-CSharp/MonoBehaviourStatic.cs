using System;
using UnityEngine;

public class MonoBehaviourStatic<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance
	{
		get
		{
			return MonoBehaviourStatic<T>.gInstance;
		}
	}

	protected void Awake()
	{
		if (MonoBehaviourStatic<T>.gInstance && MonoBehaviourStatic<T>.gInstance != this)
		{
			Object.Destroy(this);
		}
		MonoBehaviourStatic<T>.gInstance = this as T;
		this.OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	protected static T gInstance;
}
