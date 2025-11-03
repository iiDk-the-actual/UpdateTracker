using System;
using UnityEngine;

public class DisableGameObjectDelayed : MonoBehaviour
{
	private void OnEnable()
	{
		this.enabledTime = Time.time;
	}

	private void Update()
	{
		if (Time.time > this.enabledTime + this.delayTime)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void EnableAndResetTimer()
	{
		base.gameObject.SetActive(true);
		this.OnEnable();
	}

	public float delayTime = 1f;

	public float enabledTime;
}
