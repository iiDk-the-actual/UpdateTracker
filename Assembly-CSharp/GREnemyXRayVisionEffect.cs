using System;
using UnityEngine;

public class GREnemyXRayVisionEffect : MonoBehaviour
{
	public void Awake()
	{
	}

	private void Start()
	{
		base.InvokeRepeating("UpdateEffect", 0f, 0.5f);
	}

	private bool ShouldShowEffect()
	{
		return GRPlayer.GetLocal().HasXRayVision();
	}

	private void UpdateEffect()
	{
		this.enemyXRayEffect.SetActive(this.ShouldShowEffect());
	}

	public GameObject enemyXRayEffect;
}
