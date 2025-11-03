using System;
using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
	public void TriggerAttackVFX()
	{
		this.fxAttack.SetActive(false);
		this.fxAttack.SetActive(true);
	}

	public GameObject fxAttack;
}
