using System;
using UnityEngine;
using UnityEngine.Events;

namespace PerformanceSystems
{
	public class TimeSliceLodBehaviour : ATimeSliceBehaviour, ILod
	{
		public Vector3 Position
		{
			get
			{
				return this._transform.position;
			}
		}

		public float[] LodRanges
		{
			get
			{
				return this._lodRanges;
			}
		}

		public UnityEvent[] OnLodRangeEvents
		{
			get
			{
				return this._onLodRangeEvents;
			}
		}

		public UnityEvent OnCulledEvent
		{
			get
			{
				return this._onCulledEvent;
			}
		}

		public int CurrentLod
		{
			get
			{
				return this._currentLod;
			}
		}

		protected void Start()
		{
			this._updateIfDisabled = true;
			this._transform = base.transform;
		}

		protected void SetLod(int newLod)
		{
			if (newLod == this._currentLod)
			{
				return;
			}
			this._currentLod = newLod;
			if (newLod < this._onLodRangeEvents.Length)
			{
				this._onLodRangeEvents[newLod].Invoke();
				return;
			}
			if (newLod == this._onLodRangeEvents.Length)
			{
				this._onCulledEvent.Invoke();
				return;
			}
			Debug.LogWarning(string.Format("No event for LOD [{0}]", newLod), this);
		}

		public void UpdateLod(Vector3 refPos)
		{
			Vector3 position = this._transform.position;
			float num = Vector3.Distance(refPos, position);
			for (int i = 0; i < this._lodRanges.Length; i++)
			{
				float num2 = this._lodRanges[i];
				if (num <= num2)
				{
					this.SetLod(i);
					return;
				}
			}
			this.SetLod(this._lodRanges.Length);
		}

		public override void SliceUpdate(float deltaTime)
		{
		}

		public override void SliceUpdateAlways(float deltaTime)
		{
			this.UpdateLod(this._timeSliceControllerAsset.ReferenceTransform.position);
		}

		[Space]
		[SerializeField]
		protected int _currentLod = -1;

		[SerializeField]
		protected float[] _lodRanges;

		[Space]
		[SerializeField]
		protected UnityEvent[] _onLodRangeEvents;

		[Space]
		[SerializeField]
		protected UnityEvent _onCulledEvent;

		protected Transform _transform;
	}
}
