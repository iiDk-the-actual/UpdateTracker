using System;
using UnityEngine;

public class GTDisableStaticOnAwake : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.isStatic = false;
		Object.Destroy(this);
	}
}
