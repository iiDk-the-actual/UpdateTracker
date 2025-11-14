using System;
using System.Collections.Generic;
using UnityEngine;

namespace PerformanceSystems
{
	[CreateAssetMenu(menuName = "PerformanceTools/TimeSlicer/TimeSliceController", fileName = "TimeSliceController")]
	public class TimeSliceControllerAsset : ScriptableObject
	{
		public Transform ReferenceTransform
		{
			get
			{
				return this._referenceTransform;
			}
		}

		private void RemovePendingObjects()
		{
			this._currentTimeSliceBehaviours.FastRemove(this._timeSliceBehavioursToRemove);
			this._timeSliceBehavioursToRemove.Clear();
		}

		private void AddPendingObjects()
		{
			foreach (ITimeSlice timeSlice in this._timeSliceBehavioursToAdd)
			{
				if (!this._currentTimeSliceBehaviours.Contains(timeSlice))
				{
					this._currentTimeSliceBehaviours.Add(timeSlice);
				}
			}
			this._timeSliceBehavioursToAdd.Clear();
		}

		private void UpdateCurrentSliceObjects()
		{
			int count = this._currentTimeSliceBehaviours.Count;
			if (count == 0)
			{
				return;
			}
			int num = Mathf.Max(1, this._timeSlices);
			this._sliceSize = Mathf.CeilToInt((float)count / (float)num);
			if (this._sliceSize <= 0)
			{
				this._sliceSize = 1;
			}
			int num2 = this._sliceSize * this._currentSlice;
			if (num2 >= count)
			{
				num2 = Mathf.Max(0, count - this._sliceSize);
			}
			int num3 = Mathf.Min(this._sliceSize, count - num2);
			if (num3 <= 0)
			{
				return;
			}
			for (int i = 0; i < num3; i++)
			{
				int num4 = num2 + i;
				if (num4 < 0 || num4 >= this._currentTimeSliceBehaviours.Count)
				{
					break;
				}
				ITimeSlice timeSlice = this._currentTimeSliceBehaviours[num4];
				if (timeSlice != null)
				{
					timeSlice.SliceUpdate();
				}
			}
		}

		public void SetRefTransform(Transform refTransform)
		{
			this._referenceTransform = refTransform;
			this._isActive = this._referenceTransform != null;
		}

		public void AddTimeSliceBehaviour(ITimeSlice timeSlice)
		{
			if (this._currentTimeSliceBehaviours.Contains(timeSlice))
			{
				return;
			}
			this._timeSliceBehavioursToAdd.Add(timeSlice);
		}

		public void RemoveTimeSliceBehaviour(ITimeSlice timeSlice)
		{
			if (!this._currentTimeSliceBehaviours.Contains(timeSlice))
			{
				this._timeSliceBehavioursToRemove.Remove(timeSlice);
				return;
			}
			this._timeSliceBehavioursToRemove.Add(timeSlice);
		}

		public void Update()
		{
			this.InitializeReferenceTransformWithMainCam();
			if (!this._isActive)
			{
				return;
			}
			if (this._currentSlice == 0)
			{
				this.RemovePendingObjects();
				this.AddPendingObjects();
			}
			this.UpdateCurrentSliceObjects();
			this._currentSlice = (this._currentSlice + 1) % Mathf.Max(1, this._timeSlices);
		}

		public void InitializeReferenceTransformWithMainCam()
		{
			if (this._referenceTransform == null)
			{
				Camera main = Camera.main;
				this._referenceTransform = ((main != null) ? main.transform : null);
			}
			this._isActive = this._referenceTransform != null;
		}

		private void OnDisable()
		{
			this.ClearAsset();
		}

		public void ClearAsset()
		{
			this._currentTimeSliceBehaviours.Clear();
			this._timeSliceBehavioursToAdd.Clear();
			this._timeSliceBehavioursToRemove.Clear();
			this._referenceTransform = null;
		}

		private readonly List<ITimeSlice> _currentTimeSliceBehaviours = new List<ITimeSlice>();

		private readonly HashSet<ITimeSlice> _timeSliceBehavioursToAdd = new HashSet<ITimeSlice>();

		private readonly HashSet<ITimeSlice> _timeSliceBehavioursToRemove = new HashSet<ITimeSlice>();

		private Transform _referenceTransform;

		[Range(1f, 150f)]
		[SerializeField]
		private int _timeSlices = 1;

		private int _currentSlice;

		private bool _isActive;

		private int _sliceSize;
	}
}
