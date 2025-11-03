using System;
using UnityEngine;

public class SetStateConditional : StateMachineBehaviour
{
	private void OnValidate()
	{
		this._setToID = this.setToState;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!this._didSetup)
		{
			this.parentAnimator = animator;
			this.Setup(animator, stateInfo, layerIndex);
			this._didSetup = true;
		}
		this._sinceEnter = TimeSince.Now();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.delay > 0f && !this._sinceEnter.HasElapsed(this.delay, true))
		{
			return;
		}
		if (!this.CanSetState(animator, stateInfo, layerIndex))
		{
			return;
		}
		animator.Play(this._setToID);
	}

	protected virtual void Setup(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	protected virtual bool CanSetState(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		return true;
	}

	public Animator parentAnimator;

	public string setToState;

	[SerializeField]
	private AnimStateHash _setToID;

	public float delay = 1f;

	protected TimeSince _sinceEnter;

	[NonSerialized]
	private bool _didSetup;
}
