using System;
using UnityEngine;

public class GorillaBallWall : MonoBehaviour
{
	private void Awake()
	{
		if (GorillaBallWall.instance == null)
		{
			GorillaBallWall.instance = this;
			return;
		}
		if (GorillaBallWall.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
	}

	[OnEnterPlay_SetNull]
	public static volatile GorillaBallWall instance;
}
