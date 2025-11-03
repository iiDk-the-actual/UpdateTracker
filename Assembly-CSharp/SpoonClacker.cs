using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class SpoonClacker : MonoBehaviour
{
	private void Awake()
	{
		this.Setup();
	}

	private void Setup()
	{
		JointLimits limits = this.hingeJoint.limits;
		this.hingeMin = limits.min;
		this.hingeMax = limits.max;
	}

	private void Update()
	{
		if (!this.transferObject)
		{
			return;
		}
		TransferrableObject.PositionState currentState = this.transferObject.currentState;
		if (currentState != TransferrableObject.PositionState.InLeftHand && currentState != TransferrableObject.PositionState.InRightHand)
		{
			return;
		}
		float num = MathUtils.Linear(this.hingeJoint.angle, this.hingeMin, this.hingeMax, 0f, 1f);
		float num2 = (this.invertOut ? (1f - num) : num) * 100f;
		this.skinnedMesh.SetBlendShapeWeight(this.targetBlendShape, num2);
		if (!this._lockMin && num <= this.minThreshold)
		{
			this.OnHitMin.Invoke();
			this._lockMin = true;
		}
		else if (!this._lockMax && num >= 1f - this.maxThreshold)
		{
			this.OnHitMax.Invoke();
			this._lockMax = true;
			if (this._sincelastHit.HasElapsed(this.multiHitCutoff, true))
			{
				this.soundsSingle.Play();
			}
			else
			{
				this.soundsMulti.Play();
			}
		}
		if (this._lockMin && num > this.minThreshold * this.hysterisisFactor)
		{
			this._lockMin = false;
		}
		if (this._lockMax && num < 1f - this.maxThreshold * this.hysterisisFactor)
		{
			this._lockMax = false;
		}
	}

	public TransferrableObject transferObject;

	public SkinnedMeshRenderer skinnedMesh;

	public HingeJoint hingeJoint;

	public int targetBlendShape;

	public float hingeMin;

	public float hingeMax;

	public bool invertOut;

	public float minThreshold = 0.01f;

	public float maxThreshold = 0.01f;

	public float hysterisisFactor = 4f;

	public UnityEvent OnHitMin;

	public UnityEvent OnHitMax;

	private bool _lockMin;

	private bool _lockMax;

	public SoundBankPlayer soundsSingle;

	public SoundBankPlayer soundsMulti;

	private TimeSince _sincelastHit;

	[FormerlySerializedAs("multiHitInterval")]
	public float multiHitCutoff = 0.1f;
}
