using System;
using System.Collections;
using UnityEngine;

public class HideFirstFrame : MonoBehaviour
{
	private void Awake()
	{
		this._cam = base.GetComponent<Camera>();
		this._farClipPlane = this._cam.farClipPlane;
		this._cam.farClipPlane = this._cam.nearClipPlane + 0.1f;
	}

	public IEnumerator Start()
	{
		int num;
		for (int i = 0; i < this._frameDelay; i = num + 1)
		{
			yield return null;
			num = i;
		}
		this._cam.farClipPlane = this._farClipPlane;
		yield break;
	}

	[SerializeField]
	private int _frameDelay = 1;

	private Camera _cam;

	private float _farClipPlane;
}
