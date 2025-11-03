using System;
using GorillaLocomotion;
using UnityEngine;

public class SIGameEntityStealthVisibility : MonoBehaviour
{
	private void OnEnable()
	{
		this.revealRange = Mathf.Min(this.revealRange, this.hideRange);
	}

	private void OnDisable()
	{
		this.SetVisibility(true);
	}

	private void LateUpdate()
	{
		Vector3 position = GTPlayer.Instance.transform.position;
		float num = Vector3.SqrMagnitude(base.transform.position - position);
		if (this.isStealthed && num < this.revealRange * this.revealRange)
		{
			this.SetVisibility(true);
			return;
		}
		if (!this.isStealthed && num > this.hideRange * this.hideRange)
		{
			this.SetVisibility(false);
		}
	}

	private void SetVisibility(bool visible)
	{
		this.isStealthed = !visible;
		for (int i = 0; i < this.stealthedComponents.Length; i++)
		{
			this.stealthedComponents[i].enabled = visible;
		}
	}

	[SerializeField]
	private Renderer[] stealthedComponents;

	[SerializeField]
	private float revealRange = 5f;

	[SerializeField]
	private float hideRange = 8f;

	private bool isStealthed;
}
