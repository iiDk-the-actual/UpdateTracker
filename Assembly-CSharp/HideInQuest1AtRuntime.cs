using System;
using GorillaNetworking;
using UnityEngine;

public class HideInQuest1AtRuntime : MonoBehaviour
{
	private void OnEnable()
	{
		if (PlayFabAuthenticator.instance != null && "Quest1" == PlayFabAuthenticator.instance.platform.ToString())
		{
			Object.Destroy(base.gameObject);
		}
	}
}
