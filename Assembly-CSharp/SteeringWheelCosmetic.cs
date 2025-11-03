using System;
using UnityEngine;
using UnityEngine.Events;

public class SteeringWheelCosmetic : MonoBehaviour
{
	private void Start()
	{
	}

	public void TryHornHit()
	{
		if (Time.time > this.lastHornTime + this.cooldown)
		{
			this.lastHornTime = Time.time;
			UnityEvent unityEvent = this.onHornHit;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}
	}

	private void Update()
	{
		float z = base.transform.localEulerAngles.z;
		if (Mathf.Abs(Mathf.DeltaAngle(this.lastZAngle, z)) >= this.dramaticTurnThreshold)
		{
			UnityEvent unityEvent = this.onDramaticTurn;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
		}
		this.lastZAngle = z;
	}

	[SerializeField]
	private float cooldown = 1.5f;

	[SerializeField]
	private float dramaticTurnThreshold = 35f;

	[SerializeField]
	private UnityEvent onHornHit;

	[SerializeField]
	private UnityEvent onDramaticTurn;

	private float lastHornTime = -999f;

	private float lastZAngle;
}
