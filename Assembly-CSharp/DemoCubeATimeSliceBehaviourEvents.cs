using System;
using PerformanceSystems;
using UnityEngine;

public class DemoCubeATimeSliceBehaviourEvents : TimeSliceLodBehaviour
{
	protected new void Awake()
	{
		base.Awake();
		this._renderer = base.GetComponent<Renderer>();
	}

	public override void SliceUpdate(float deltaTime)
	{
		float num = 0f;
		for (int i = 0; i < this._iterationsOfExpensiveOp; i++)
		{
			num += Mathf.Sqrt((float)i * deltaTime);
		}
	}

	public void OnLod0Enter()
	{
		this._renderer.material = this._red;
		base.gameObject.SetActive(true);
	}

	public void OnLod1Enter()
	{
		this._renderer.material = this._green;
		base.gameObject.SetActive(true);
	}

	public void OnLodExit()
	{
		base.gameObject.SetActive(false);
	}

	[SerializeField]
	private int _iterationsOfExpensiveOp = 200;

	[SerializeField]
	private Material _red;

	[SerializeField]
	private Material _green;

	private Renderer _renderer;
}
