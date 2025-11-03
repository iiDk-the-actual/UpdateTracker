using System;
using UnityEngine;

public class PeriodicFoodTopUpper : MonoBehaviour
{
	private void Awake()
	{
		this.food = base.GetComponentInParent<CrittersFood>();
	}

	private void Update()
	{
		if (!CrittersManager.instance.LocalAuthority())
		{
			return;
		}
		if (!this.waitingToRefill && this.food.currentFood == 0f)
		{
			this.waitingToRefill = true;
			this.timeFoodEmpty = Time.time;
		}
		if (this.waitingToRefill && Time.time > this.timeFoodEmpty + this.waitToRefill)
		{
			this.waitingToRefill = false;
			this.food.Initialize();
		}
	}

	private CrittersFood food;

	private float timeFoodEmpty;

	private bool waitingToRefill;

	public float waitToRefill = 10f;

	public GameObject foodObject;
}
