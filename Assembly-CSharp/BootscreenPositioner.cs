using System;
using UnityEngine;

public class BootscreenPositioner : MonoBehaviour
{
	private void Awake()
	{
		base.transform.position = this._pov.position;
		base.transform.rotation = Quaternion.Euler(0f, this._pov.rotation.eulerAngles.y, 0f);
	}

	private void LateUpdate()
	{
		if (Vector3.Distance(base.transform.position, this._pov.position) > this._distanceThreshold)
		{
			base.transform.position = this._pov.position;
		}
		if (Mathf.Abs(this._pov.rotation.eulerAngles.y - base.transform.rotation.eulerAngles.y) > this._rotationThreshold)
		{
			base.transform.rotation = Quaternion.Euler(0f, this._pov.rotation.eulerAngles.y, 0f);
		}
	}

	[SerializeField]
	private Transform _pov;

	[SerializeField]
	private float _distanceThreshold;

	[SerializeField]
	private float _rotationThreshold;
}
