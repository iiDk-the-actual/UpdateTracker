using System;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class ScenePreparer : MonoBehaviour
{
	protected void Awake()
	{
		bool flag = false;
		GameObject[] array = this.betaEnableObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(flag);
		}
		array = this.betaDisableObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(!flag);
		}
	}

	public OVRManager ovrManager;

	public GameObject[] betaDisableObjects;

	public GameObject[] betaEnableObjects;
}
