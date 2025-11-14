using System;
using UnityEngine;

public class DestroyOnDisabled : MonoBehaviour
{
	private void OnDisable()
	{
		Object.Destroy(base.gameObject);
	}
}
