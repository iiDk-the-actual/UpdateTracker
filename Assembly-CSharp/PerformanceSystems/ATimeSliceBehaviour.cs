using System;
using UnityEngine;

namespace PerformanceSystems
{
	public abstract class ATimeSliceBehaviour : MonoBehaviour, ITimeSlice
	{
		protected void Awake()
		{
			this._timeSliceControllerAsset.AddTimeSliceBehaviour(this);
		}

		protected void OnDestroy()
		{
			this._timeSliceControllerAsset.RemoveTimeSliceBehaviour(this);
		}

		public void SliceUpdate()
		{
			float num = Time.realtimeSinceStartup - this._lastUpdateTime;
			this._lastUpdateTime = Time.realtimeSinceStartup;
			this.SliceUpdateAlways(num);
			if (this._updateIfDisabled || base.gameObject.activeSelf)
			{
				this.SliceUpdate(num);
			}
		}

		public abstract void SliceUpdate(float deltaTime);

		public abstract void SliceUpdateAlways(float deltaTime);

		[SerializeField]
		protected TimeSliceControllerAsset _timeSliceControllerAsset;

		[SerializeField]
		protected bool _updateIfDisabled = true;

		protected float _lastUpdateTime;
	}
}
