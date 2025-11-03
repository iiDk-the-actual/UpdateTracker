using System;
using System.Collections;
using UnityEngine;

public class SlowCameraUpdate : MonoBehaviour
{
	public void Awake()
	{
		this.frameRate = 30f;
		this.timeToNextFrame = 1f / this.frameRate;
		this.myCamera = base.GetComponent<Camera>();
	}

	public void OnEnable()
	{
		base.StartCoroutine(this.UpdateMirror());
	}

	public void OnDisable()
	{
		base.StopAllCoroutines();
	}

	public IEnumerator UpdateMirror()
	{
		for (;;)
		{
			if (base.gameObject.activeSelf)
			{
				Debug.Log("rendering camera!");
				this.myCamera.Render();
			}
			yield return new WaitForSeconds(this.timeToNextFrame);
		}
		yield break;
	}

	private Camera myCamera;

	private float frameRate;

	private float timeToNextFrame;
}
