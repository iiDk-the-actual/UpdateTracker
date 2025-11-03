using System;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Events;

public class PartyHornTransferableObject : TransferrableObject
{
	internal override void OnEnable()
	{
		base.OnEnable();
		this.localHead = GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform;
		this.InitToDefault();
	}

	internal override void OnDisable()
	{
		base.OnDisable();
	}

	public override void ResetToDefaultState()
	{
		base.ResetToDefaultState();
		this.InitToDefault();
	}

	protected Vector3 CalcMouthPiecePos()
	{
		if (!this.mouthPiece)
		{
			return base.transform.position + this.mouthPieceZOffset * base.transform.forward;
		}
		return this.mouthPiece.position;
	}

	protected override void LateUpdateLocal()
	{
		base.LateUpdateLocal();
		if (!base.InHand())
		{
			return;
		}
		if (this.itemState != TransferrableObject.ItemStates.State0)
		{
			return;
		}
		if (!GorillaParent.hasInstance)
		{
			return;
		}
		Transform transform = base.transform;
		Vector3 vector = this.CalcMouthPiecePos();
		float num = this.mouthPieceRadius * this.mouthPieceRadius * GTPlayer.Instance.scale * GTPlayer.Instance.scale;
		bool flag = (this.localHead.TransformPoint(this.mouthOffset) - vector).sqrMagnitude < num;
		if (this.soundActivated && PhotonNetwork.InRoom)
		{
			bool flag2;
			if (flag)
			{
				GorillaTagger instance = GorillaTagger.Instance;
				if (instance == null)
				{
					flag2 = false;
				}
				else
				{
					Recorder myRecorder = instance.myRecorder;
					bool? flag3 = ((myRecorder != null) ? new bool?(myRecorder.IsCurrentlyTransmitting) : null);
					bool flag4 = true;
					flag2 = (flag3.GetValueOrDefault() == flag4) & (flag3 != null);
				}
			}
			else
			{
				flag2 = false;
			}
			flag = flag2;
		}
		for (int i = 0; i < GorillaParent.instance.vrrigs.Count; i++)
		{
			VRRig vrrig = GorillaParent.instance.vrrigs[i];
			if (vrrig.head == null || vrrig.head.rigTarget == null || flag)
			{
				break;
			}
			flag = (vrrig.head.rigTarget.transform.TransformPoint(this.mouthOffset) - vector).sqrMagnitude < num;
			if (this.soundActivated)
			{
				bool flag5;
				if (flag)
				{
					RigContainer rigContainer = vrrig.rigContainer;
					if (rigContainer == null)
					{
						flag5 = false;
					}
					else
					{
						PhotonVoiceView voice = rigContainer.Voice;
						bool? flag3 = ((voice != null) ? new bool?(voice.IsSpeaking) : null);
						bool flag4 = true;
						flag5 = (flag3.GetValueOrDefault() == flag4) & (flag3 != null);
					}
				}
				else
				{
					flag5 = false;
				}
				flag = flag5;
			}
		}
		this.itemState = (flag ? TransferrableObject.ItemStates.State1 : this.itemState);
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		if (TransferrableObject.ItemStates.State1 != this.itemState)
		{
			return;
		}
		if (!this.localWasActivated)
		{
			if (this.effectsGameObject)
			{
				this.effectsGameObject.SetActive(true);
			}
			this.cooldownRemaining = this.cooldown;
			this.localWasActivated = true;
			UnityEvent onCooldownStart = this.OnCooldownStart;
			if (onCooldownStart != null)
			{
				onCooldownStart.Invoke();
			}
		}
		this.cooldownRemaining -= Time.deltaTime;
		if (this.cooldownRemaining <= 0f)
		{
			this.InitToDefault();
		}
	}

	private void InitToDefault()
	{
		this.itemState = TransferrableObject.ItemStates.State0;
		if (this.effectsGameObject)
		{
			this.effectsGameObject.SetActive(false);
		}
		this.cooldownRemaining = this.cooldown;
		this.localWasActivated = false;
		UnityEvent onCooldownReset = this.OnCooldownReset;
		if (onCooldownReset == null)
		{
			return;
		}
		onCooldownReset.Invoke();
	}

	[Tooltip("This GameObject will activate when held to any gorilla's mouth.")]
	public GameObject effectsGameObject;

	public float cooldown = 2f;

	public float mouthPieceZOffset = -0.18f;

	public float mouthPieceRadius = 0.05f;

	public Transform mouthPiece;

	public Vector3 mouthOffset = new Vector3(0f, 0.02f, 0.17f);

	public bool soundActivated;

	public UnityEvent OnCooldownStart;

	public UnityEvent OnCooldownReset;

	private float cooldownRemaining;

	private Transform localHead;

	private PartyHornTransferableObject.PartyHornState partyHornStateLastFrame;

	private bool localWasActivated;

	private enum PartyHornState
	{
		None = 1,
		CoolingDown
	}
}
