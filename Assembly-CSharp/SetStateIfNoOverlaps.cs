using System;
using UnityEngine;

public class SetStateIfNoOverlaps : SetStateConditional
{
	protected override void Setup(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this._volume = animator.GetComponent<VolumeCast>();
	}

	protected override bool CanSetState(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		bool flag = this._volume.CheckOverlaps();
		if (flag)
		{
			this._sinceEnter = 0f;
		}
		return !flag;
	}

	public VolumeCast _volume;
}
