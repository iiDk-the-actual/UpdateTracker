using System;
using System.Threading.Tasks;
using UnityEngine;

public class AnimationPauser : StateMachineBehaviour
{
	public override async void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		this._animPauseDuration = Random.Range(this._minTimeBetweenAnims, this._maxTimeBetweenAnims);
		await Task.Delay(this._animPauseDuration * 1000);
		animator.SetTrigger(AnimationPauser.Restart_Anim_Name);
	}

	[SerializeField]
	private int _maxTimeBetweenAnims = 5;

	[SerializeField]
	private int _minTimeBetweenAnims = 1;

	private int _animPauseDuration;

	private static readonly string Restart_Anim_Name = "RestartAnim";
}
