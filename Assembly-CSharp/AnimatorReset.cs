using System;
using UnityEngine;

public class AnimatorReset : MonoBehaviour
{
	public void Reset()
	{
		if (!this.target)
		{
			return;
		}
		this.target.Rebind();
		this.target.Update(0f);
	}

	private void OnEnable()
	{
		if (this.onEnable)
		{
			this.Reset();
		}
	}

	private void OnDisable()
	{
		if (this.onDisable)
		{
			this.Reset();
		}
	}

	public Animator target;

	public bool onEnable;

	public bool onDisable = true;
}
