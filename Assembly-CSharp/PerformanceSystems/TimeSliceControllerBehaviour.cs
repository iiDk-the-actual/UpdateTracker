using System;
using UnityEngine;

namespace PerformanceSystems
{
	public class TimeSliceControllerBehaviour : MonoBehaviour
	{
		private void Awake()
		{
			this._timeSliceControllerAsset.InitializeReferenceTransformWithMainCam();
		}

		private void Update()
		{
			this._timeSliceControllerAsset.Update();
		}

		[SerializeField]
		private TimeSliceControllerAsset _timeSliceControllerAsset;
	}
}
