using System;
using UnityEngine;

public class ScalerUpper : MonoBehaviour
{
	private void Update()
	{
		for (int i = 0; i < this.target.Length; i++)
		{
			this.target[i].transform.localScale = Vector3.one * this.scaleCurve.Evaluate(this.t);
		}
		this.t += Time.deltaTime;
	}

	private void OnEnable()
	{
		this.t = 0f;
	}

	private void OnDisable()
	{
		for (int i = 0; i < this.target.Length; i++)
		{
			this.target[i].transform.localScale = Vector3.one;
		}
	}

	[SerializeField]
	private Transform[] target;

	[SerializeField]
	private AnimationCurve scaleCurve;

	private float t;
}
