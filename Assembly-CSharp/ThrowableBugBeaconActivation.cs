using System;
using System.Collections;
using UnityEngine;

public class ThrowableBugBeaconActivation : MonoBehaviour
{
	private void Awake()
	{
		this.tbb = base.GetComponent<ThrowableBugBeacon>();
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.SendSignals());
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
	}

	private IEnumerator SendSignals()
	{
		uint count = 0U;
		while (this.signalCount == 0U || count < this.signalCount)
		{
			yield return new WaitForSeconds(Random.Range(this.minCallTime, this.maxCallTime));
			switch (this.mode)
			{
			case ThrowableBugBeaconActivation.ActivationMode.CALL:
				this.tbb.Call();
				break;
			case ThrowableBugBeaconActivation.ActivationMode.DISMISS:
				this.tbb.Dismiss();
				break;
			case ThrowableBugBeaconActivation.ActivationMode.LOCK:
				this.tbb.Lock();
				break;
			}
			uint num = count;
			count = num + 1U;
		}
		yield break;
	}

	[SerializeField]
	private float minCallTime = 1f;

	[SerializeField]
	private float maxCallTime = 5f;

	[SerializeField]
	private uint signalCount;

	[SerializeField]
	private ThrowableBugBeaconActivation.ActivationMode mode;

	private ThrowableBugBeacon tbb;

	private enum ActivationMode
	{
		CALL,
		DISMISS,
		LOCK
	}
}
