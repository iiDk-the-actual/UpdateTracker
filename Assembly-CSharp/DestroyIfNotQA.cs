using System;
using UnityEngine;

public class DestroyIfNotQA : MonoBehaviour
{
	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}
}
