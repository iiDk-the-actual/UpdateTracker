using System;
using UnityEngine;
using UnityEngine.Events;

public class RadialBoundsTrigger : MonoBehaviour
{
	public void TestOverlap()
	{
		this.TestOverlap(this._raiseEvents);
	}

	public void TestOverlap(bool raiseEvents)
	{
		if (!this.object1 || !this.object2)
		{
			this._overlapping = false;
			this._timeOverlapStarted = -1f;
			this._timeOverlapStopped = -1f;
			this._timeSpentInOverlap = 0f;
			return;
		}
		float time = Time.time;
		float num = this.object1.radius + this.object2.radius;
		bool flag = (this.object2.center - this.object1.center).sqrMagnitude <= num * num;
		if (this._overlapping && flag)
		{
			this._overlapping = true;
			this._timeSpentInOverlap = time - this._timeOverlapStarted;
			if (raiseEvents)
			{
				UnityEvent<RadialBounds, float> onOverlapStay = this.object1.onOverlapStay;
				if (onOverlapStay != null)
				{
					onOverlapStay.Invoke(this.object2, this._timeSpentInOverlap);
				}
				UnityEvent<RadialBounds, float> onOverlapStay2 = this.object2.onOverlapStay;
				if (onOverlapStay2 == null)
				{
					return;
				}
				onOverlapStay2.Invoke(this.object1, this._timeSpentInOverlap);
				return;
			}
		}
		else if (!this._overlapping && flag)
		{
			if (time - this._timeOverlapStopped < this.hysteresis)
			{
				return;
			}
			this._overlapping = true;
			this._timeOverlapStarted = time;
			this._timeOverlapStopped = -1f;
			this._timeSpentInOverlap = 0f;
			if (raiseEvents)
			{
				UnityEvent<RadialBounds> onOverlapEnter = this.object1.onOverlapEnter;
				if (onOverlapEnter != null)
				{
					onOverlapEnter.Invoke(this.object2);
				}
				UnityEvent<RadialBounds> onOverlapEnter2 = this.object2.onOverlapEnter;
				if (onOverlapEnter2 == null)
				{
					return;
				}
				onOverlapEnter2.Invoke(this.object1);
				return;
			}
		}
		else if (!flag && this._overlapping)
		{
			this._overlapping = false;
			this._timeOverlapStarted = -1f;
			this._timeOverlapStopped = time;
			this._timeSpentInOverlap = 0f;
			if (raiseEvents)
			{
				UnityEvent<RadialBounds> onOverlapExit = this.object1.onOverlapExit;
				if (onOverlapExit != null)
				{
					onOverlapExit.Invoke(this.object2);
				}
				UnityEvent<RadialBounds> onOverlapExit2 = this.object2.onOverlapExit;
				if (onOverlapExit2 == null)
				{
					return;
				}
				onOverlapExit2.Invoke(this.object1);
			}
		}
	}

	private void FixedUpdate()
	{
		this.TestOverlap();
	}

	private void OnDisable()
	{
		if (this._raiseEvents && this.object1 && this.object2 && this._overlapping)
		{
			UnityEvent<RadialBounds> onOverlapExit = this.object1.onOverlapExit;
			if (onOverlapExit != null)
			{
				onOverlapExit.Invoke(this.object2);
			}
			UnityEvent<RadialBounds> onOverlapExit2 = this.object2.onOverlapExit;
			if (onOverlapExit2 != null)
			{
				onOverlapExit2.Invoke(this.object1);
			}
		}
		this._timeOverlapStarted = -1f;
		this._timeSpentInOverlap = 0f;
		this._overlapping = false;
	}

	[SerializeField]
	private Id32 _triggerID;

	[Space]
	public RadialBounds object1 = new RadialBounds();

	[Space]
	public RadialBounds object2 = new RadialBounds();

	[Space]
	public float hysteresis = 0.5f;

	[SerializeField]
	private bool _raiseEvents = true;

	[Space]
	private bool _overlapping;

	private float _timeSpentInOverlap;

	[Space]
	private float _timeOverlapStarted;

	private float _timeOverlapStopped;
}
