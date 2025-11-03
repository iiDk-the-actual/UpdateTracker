using System;
using UnityEngine;

public class GorillaUIParent : MonoBehaviour
{
	private void Awake()
	{
		if (GorillaUIParent.instance == null)
		{
			GorillaUIParent.instance = this;
			return;
		}
		if (GorillaUIParent.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	[OnEnterPlay_SetNull]
	public static volatile GorillaUIParent instance;
}
