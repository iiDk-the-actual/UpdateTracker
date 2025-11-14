using System;
using PerformanceSystems;
using UnityEngine;

public class DemoSpheresRotate : TimeSliceLodBehaviour
{
	public void OnLod0Enter()
	{
		this._renderer.material = this._red;
		this.SwapToTimeSlicer(0);
		base.gameObject.SetActive(true);
	}

	public void OnLod1Enter()
	{
		this._renderer.material = this._green;
		this.SwapToTimeSlicer(1);
		base.gameObject.SetActive(true);
	}

	public void OnLod2Enter()
	{
		this._renderer.material = this._black;
		this.SwapToTimeSlicer(2);
		base.gameObject.SetActive(true);
	}

	public void OnLodExit()
	{
		base.gameObject.SetActive(false);
	}

	public override void SliceUpdate(float deltaTime)
	{
		base.transform.Rotate(Vector3.up * this._rotationSpeed * deltaTime);
	}

	private void SwapToTimeSlicer(int index)
	{
		if (this._timeSliceControllerAssets[index] == this._timeSliceControllerAsset)
		{
			return;
		}
		this._timeSliceControllerAsset.RemoveTimeSliceBehaviour(this);
		this._timeSliceControllerAsset = this._timeSliceControllerAssets[index];
		this._timeSliceControllerAsset.AddTimeSliceBehaviour(this);
	}

	[SerializeField]
	private TimeSliceControllerAsset[] _timeSliceControllerAssets;

	[SerializeField]
	private float _rotationSpeed = 10f;

	[SerializeField]
	private Material _red;

	[SerializeField]
	private Material _green;

	[SerializeField]
	private Material _black;

	[SerializeField]
	private Renderer _renderer;
}
