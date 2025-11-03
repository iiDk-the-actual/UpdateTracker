using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ToggleableWearable : MonoBehaviour
{
	protected void Awake()
	{
		this.ownerRig = base.GetComponentInParent<VRRig>();
		if (this.ownerRig == null)
		{
			GorillaTagger componentInParent = base.GetComponentInParent<GorillaTagger>();
			if (componentInParent != null)
			{
				this.ownerRig = componentInParent.offlineVRRig;
				this.ownerIsLocal = this.ownerRig != null;
			}
		}
		if (this.ownerRig == null)
		{
			Debug.LogError("TriggerToggler: Disabling cannot find VRRig.");
			base.enabled = false;
			return;
		}
		foreach (Renderer renderer in this.renderers)
		{
			if (renderer == null)
			{
				Debug.LogError("TriggerToggler: Disabling because a renderer is null.");
				base.enabled = false;
				break;
			}
			renderer.enabled = this.startOn;
		}
		this.hasAudioSource = this.audioSource != null;
		this.assignedSlotBitIndex = (int)this.assignedSlot;
		if (this.oneShot)
		{
			this.toggleCooldownRange.x = this.toggleCooldownRange.x + this.animationTransitionDuration;
			this.toggleCooldownRange.y = this.toggleCooldownRange.y + this.animationTransitionDuration;
		}
	}

	protected void LateUpdate()
	{
		if (this.ownerIsLocal)
		{
			this.toggleCooldownTimer -= Time.deltaTime;
			Transform transform = base.transform;
			if (Physics.OverlapSphereNonAlloc(transform.TransformPoint(this.triggerOffset), this.triggerRadius * transform.lossyScale.x, this.colliders, this.layerMask) > 0 && this.toggleCooldownTimer < 0f)
			{
				XRController componentInParent = this.colliders[0].GetComponentInParent<XRController>();
				if (componentInParent != null)
				{
					this.LocalToggle(componentInParent.controllerNode == XRNode.LeftHand, true, true);
				}
				this.toggleCooldownTimer = Random.Range(this.toggleCooldownRange.x, this.toggleCooldownRange.y);
				this.toggleTimer = 0f;
			}
			if (this.resetTimer > 0f)
			{
				this.toggleTimer += Time.deltaTime;
				if (this.toggleTimer > this.resetTimer && this.startOn != this.isOn)
				{
					this.LocalToggle(false, true, false);
					this.toggleTimer = 0f;
				}
			}
		}
		else
		{
			bool flag = (this.ownerRig.WearablePackedStates & (1 << this.assignedSlotBitIndex)) != 0;
			if (this.isOn != flag)
			{
				this.SharedSetState(flag, true);
			}
		}
		if (this.oneShot)
		{
			if (this.isOn)
			{
				this.progress = Mathf.MoveTowards(this.progress, 1f, Time.deltaTime / this.animationTransitionDuration);
				if (this.progress == 1f)
				{
					if (this.ownerIsLocal)
					{
						this.LocalToggle(false, false, false);
					}
					else
					{
						this.SharedSetState(false, false);
					}
					this.progress = 0f;
				}
			}
		}
		else
		{
			this.progress = Mathf.MoveTowards(this.progress, this.isOn ? 1f : 0f, Time.deltaTime / this.animationTransitionDuration);
		}
		Animator[] array = this.animators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetFloat(ToggleableWearable.animParam_Progress, this.progress);
		}
	}

	private void LocalToggle(bool isLeftHand, bool playAudio, bool playHaptics)
	{
		this.ownerRig.WearablePackedStates ^= 1 << this.assignedSlotBitIndex;
		this.SharedSetState((this.ownerRig.WearablePackedStates & (1 << this.assignedSlotBitIndex)) != 0, playAudio);
		if (playHaptics && GorillaTagger.Instance)
		{
			GorillaTagger.Instance.StartVibration(isLeftHand, this.isOn ? this.turnOnVibrationDuration : this.turnOffVibrationDuration, this.isOn ? this.turnOnVibrationStrength : this.turnOffVibrationStrength);
		}
	}

	private void SharedSetState(bool state, bool playAudio)
	{
		this.isOn = state;
		Renderer[] array = this.renderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = this.isOn;
		}
		if (!playAudio || !this.hasAudioSource)
		{
			return;
		}
		AudioClip audioClip = (this.isOn ? this.toggleOnSound : this.toggleOffSound);
		if (audioClip == null)
		{
			return;
		}
		if (this.oneShot)
		{
			this.audioSource.clip = audioClip;
			this.audioSource.GTPlay();
			return;
		}
		this.audioSource.GTPlayOneShot(audioClip, 1f);
	}

	public Renderer[] renderers;

	public Animator[] animators;

	public float animationTransitionDuration = 1f;

	[Tooltip("Whether the wearable state is toggled on by default.")]
	public bool startOn;

	[Tooltip("AudioSource to play toggle sounds.")]
	public AudioSource audioSource;

	[Tooltip("Sound to play when toggled on.")]
	public AudioClip toggleOnSound;

	[Tooltip("Sound to play when toggled off.")]
	public AudioClip toggleOffSound;

	[Tooltip("Layer to check for trigger sphere collisions.")]
	public LayerMask layerMask;

	[Tooltip("Radius of the trigger sphere.")]
	public float triggerRadius = 0.2f;

	[Tooltip("Position in local space to move the trigger sphere.")]
	public Vector3 triggerOffset = Vector3.zero;

	[Tooltip("This is to determine what bit to change in VRRig.WearablesPackedStates.")]
	public VRRig.WearablePackedStateSlots assignedSlot;

	[Header("Vibration")]
	public float turnOnVibrationDuration = 0.05f;

	public float turnOnVibrationStrength = 0.2f;

	public float turnOffVibrationDuration = 0.05f;

	public float turnOffVibrationStrength = 0.2f;

	private VRRig ownerRig;

	private bool ownerIsLocal;

	private bool isOn;

	[SerializeField]
	private Vector2 toggleCooldownRange = new Vector2(0.2f, 0.2f);

	private bool hasAudioSource;

	private readonly Collider[] colliders = new Collider[1];

	private int framesSinceCooldownAndExitingVolume;

	private float toggleCooldownTimer;

	private int assignedSlotBitIndex;

	private static readonly int animParam_Progress = Animator.StringToHash("Progress");

	private float progress;

	[SerializeField]
	private bool oneShot;

	[SerializeField]
	[Tooltip("Seconds before reverting to its default state, as defined by 'Start On.' A value of 0 or less means never.")]
	private float resetTimer;

	private float toggleTimer;
}
