using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRenderVelocityMapper : MonoBehaviour
{
	private void Awake()
	{
		this._lr = base.GetComponent<LineRenderer>();
		this._lr.useWorldSpace = true;
	}

	private void LateUpdate()
	{
		if (this.velocityEstimator == null)
		{
			return;
		}
		this._lr.SetPosition(0, this.velocityEstimator.transform.position);
		if (this.velocityEstimator.linearVelocity.sqrMagnitude > 0.1f)
		{
			this._lr.SetPosition(1, this.velocityEstimator.transform.position + this.velocityEstimator.linearVelocity.normalized * 0.2f);
			return;
		}
		this._lr.SetPosition(1, this.velocityEstimator.transform.position);
	}

	[SerializeField]
	private GorillaVelocityEstimator velocityEstimator;

	private LineRenderer _lr;
}
