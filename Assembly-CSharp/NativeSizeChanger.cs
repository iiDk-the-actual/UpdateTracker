using System;
using GorillaLocomotion;
using UnityEngine;

public class NativeSizeChanger : MonoBehaviour
{
	public void Activate(NativeSizeChangerSettings settings)
	{
		settings.WorldPosition = base.transform.position;
		settings.ActivationTime = Time.time;
		GTPlayer.Instance.SetNativeScale(settings);
	}
}
