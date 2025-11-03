using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomAnimationSelector : MonoBehaviour, IGorillaSliceableSimple
{
	private void Awake()
	{
		this.animator = base.GetComponent<Animator>();
		this.animationTrigger = Animator.StringToHash(this.animationTriggerName);
		this.animationSelect = Animator.StringToHash(this.animationSelectName);
	}

	public void OnEnable()
	{
		if (this.animator != null)
		{
			GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
			this.lastSliceUpdateTime = Time.time;
		}
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void SliceUpdate()
	{
		float num = Time.time - this.lastSliceUpdateTime;
		this.lastSliceUpdateTime = Time.time;
		float num2 = 1f - Mathf.Exp(-this.animationChancePerSecond * num);
		if (Random.value < num2)
		{
			float num3 = Time.time - (float)((int)Time.time);
			this.animator.SetFloat(this.animationSelect, num3);
			this.animator.SetTrigger(this.animationTrigger);
		}
	}

	[SerializeField]
	private string animationTriggerName;

	private int animationTrigger;

	[SerializeField]
	private string animationSelectName;

	private int animationSelect;

	[Range(0f, 1f)]
	[SerializeField]
	private float animationChancePerSecond = 0.33f;

	private Animator animator;

	private float lastSliceUpdateTime;
}
