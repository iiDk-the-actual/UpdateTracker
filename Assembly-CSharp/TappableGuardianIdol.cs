using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
public class TappableGuardianIdol : Tappable
{
	public bool isChangingPositions { get; private set; }

	protected override void OnEnable()
	{
		base.OnEnable();
		this._colliderBaseRadius = this.tapCollision.radius;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.isChangingPositions = false;
		this._activationState = -1;
		this.isActivationReady = true;
		this.tapCollision.radius = this._colliderBaseRadius;
	}

	public void OnZoneActiveStateChanged(bool zoneActive)
	{
		this._zoneIsActive = zoneActive;
		this.idolVisualRoot.SetActive(this._zoneIsActive);
	}

	public override void OnTapLocal(float tapStrength, float tapTime, PhotonMessageInfoWrapped info)
	{
		if (info.Sender.IsLocal)
		{
			this.zoneManager.SetScaleCenterPoint(base.transform);
		}
		if (!this.isChangingPositions)
		{
			if (!this.zoneManager.IsZoneValid())
			{
				return;
			}
			RigContainer rigContainer;
			if (PhotonNetwork.LocalPlayer.IsMasterClient && VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
			{
				if (Vector3.Magnitude(rigContainer.Rig.transform.position - base.transform.position) > this.requiredTapDistance + Mathf.Epsilon)
				{
					return;
				}
				this.zoneManager.IdolWasTapped(info.Sender);
			}
			if (!this.zoneManager.IsPlayerGuardian(info.Sender))
			{
				this.tapFX.Play();
			}
		}
	}

	public void SetPosition(Vector3 position)
	{
		base.transform.position = position + new Vector3(0f, this.activeHeight, 0f);
		this.UpdateStageActivatedObjects();
		this._audio.GTPlayOneShot(this._activateSound, this._audio.volume);
		base.StartCoroutine(this.<SetPosition>g__Unshrink|49_0());
	}

	public void MovePositions(Vector3 finalPosition)
	{
		if (this.isChangingPositions)
		{
			return;
		}
		this.transitionPos = finalPosition + this.fallStartOffset;
		this.finalPos = finalPosition;
		base.StartCoroutine(this.TransitionToNextIdol());
	}

	public void UpdateActivationProgress(float rawProgress, bool progressing)
	{
		this.isActivationReady = !progressing;
		if (rawProgress <= 0f && !progressing)
		{
			if (this._activationState >= 0)
			{
				if (this._activationRoutine != null)
				{
					base.StopCoroutine(this._activationRoutine);
					this._activationRoutine = null;
				}
				this.idolMeshRoot.transform.localScale = Vector3.one;
			}
			this._activationState = -1;
			this.UpdateStageActivatedObjects();
			this._audio.GTStop();
			return;
		}
		int num = (int)rawProgress;
		progressing &= this._activationStageSounds.Length > num;
		if (this._activationState == num || !progressing)
		{
			return;
		}
		if (this._activationRoutine != null)
		{
			base.StopCoroutine(this._activationRoutine);
		}
		this._activationRoutine = base.StartCoroutine(this.ShowActivationEffect());
		this._activationState = num;
		this.UpdateStageActivatedObjects();
		TappableGuardianIdol.IdolActivationSound idolActivationSound = this._activationStageSounds[num];
		this._audio.GTPlayOneShot(idolActivationSound.activation, this._audio.volume);
		this._audio.clip = idolActivationSound.loop;
		this._audio.loop = true;
		this._audio.GTPlay();
	}

	public void StartLookingAround()
	{
		if (this._lookRoutine != null)
		{
			base.StopCoroutine(this._lookRoutine);
		}
		this._lookRoutine = base.StartCoroutine(this.DoLookingAround());
	}

	public void StopLookingAround()
	{
		if (this._lookRoutine == null)
		{
			return;
		}
		base.StopCoroutine(this._lookRoutine);
		this._lookRoot.localRotation = Quaternion.identity;
		this._lookRoutine = null;
	}

	private IEnumerator DoLookingAround()
	{
		TappableGuardianIdol.<>c__DisplayClass54_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.nextLookTime = Time.time;
		CS$<>8__locals1._lookDirection = this._lookRoot.rotation;
		yield return null;
		for (;;)
		{
			if (Time.time >= CS$<>8__locals1.nextLookTime)
			{
				this.<DoLookingAround>g__PickLookTarget|54_0(ref CS$<>8__locals1);
			}
			this._lookRoot.rotation = Quaternion.Slerp(this._lookRoot.rotation, CS$<>8__locals1._lookDirection, Time.deltaTime * Mathf.Max(1f, (float)this._activationState * this._baseLookRate));
			yield return null;
		}
		yield break;
	}

	private void UpdateStageActivatedObjects()
	{
		foreach (TappableGuardianIdol.StageActivatedObject stageActivatedObject in this._stageActivatedObjects)
		{
			stageActivatedObject.UpdateActiveState(this._activationState);
		}
	}

	private IEnumerator ShowActivationEffect()
	{
		float bulgeDuration = 1f;
		float lerpVal = 0f;
		while (lerpVal < 1f)
		{
			lerpVal += Time.deltaTime / bulgeDuration;
			float num = Mathf.Lerp(1f, this.bulgeScale, this.bulgeCurve.Evaluate(lerpVal));
			this.idolMeshRoot.transform.localScale = Vector3.one * num;
			this.tapCollision.radius = this._colliderBaseRadius * num;
			yield return null;
		}
		this._activationRoutine = null;
		yield break;
	}

	private IEnumerator TransitionToNextIdol()
	{
		this.isChangingPositions = true;
		this._audio.GTStop();
		if (this.knockbackOnTrigger)
		{
			this.zoneManager.TriggerIdolKnockback();
		}
		if (this.explodeFX)
		{
			ObjectPools.instance.Instantiate(this.explodeFX, base.transform.position, true);
		}
		this.UpdateActivationProgress(-1f, false);
		this.idolMeshRoot.SetActive(false);
		this.tapCollision.enabled = false;
		base.transform.position = this.transitionPos;
		yield return new WaitForSeconds(this.floatDuration);
		this.idolMeshRoot.SetActive(true);
		this.tapCollision.enabled = true;
		if (this.startFallFX)
		{
			ObjectPools.instance.Instantiate(this.startFallFX, this.transitionPos, true);
		}
		this._audio.GTPlayOneShot(this._descentSound, 1f);
		this.trailFX.Play();
		float fall = 0f;
		Vector3 startPos = this.transitionPos;
		Vector3 destinationPos = this.finalPos;
		while (fall < this.fallDuration)
		{
			fall += Time.deltaTime;
			base.transform.position = Vector3.Lerp(startPos, destinationPos, fall / this.fallDuration);
			yield return null;
		}
		base.transform.position = destinationPos;
		this.trailFX.Stop();
		if (this.landedFX)
		{
			ObjectPools.instance.Instantiate(this.landedFX, destinationPos, true);
		}
		if (this.knockbackOnLand)
		{
			this.zoneManager.TriggerIdolKnockback();
		}
		yield return new WaitForSeconds(this.inactiveDuration);
		this._audio.GTPlayOneShot(this._activateSound, this._audio.volume);
		float activateLerp = 0f;
		startPos = this.finalPos;
		destinationPos = this.finalPos + new Vector3(0f, this.activeHeight, 0f);
		AnimationCurve animCurve = AnimationCurves.EaseInOutQuad;
		while (activateLerp < 1f)
		{
			activateLerp = Mathf.Clamp01(activateLerp + Time.deltaTime / this.activationDuration);
			base.transform.position = Vector3.Lerp(startPos, destinationPos, animCurve.Evaluate(activateLerp));
			yield return null;
		}
		if (this.activatedFX)
		{
			ObjectPools.instance.Instantiate(this.activatedFX, base.transform.position, true);
		}
		if (this.knockbackOnActivate)
		{
			this.zoneManager.TriggerIdolKnockback();
		}
		this.isChangingPositions = false;
		yield break;
	}

	private float EaseInOut(float input)
	{
		if (input >= 0.5f)
		{
			return 1f - Mathf.Pow(-2f * input + 2f, 3f) / 2f;
		}
		return 4f * input * input * input;
	}

	[CompilerGenerated]
	private IEnumerator <SetPosition>g__Unshrink|49_0()
	{
		float lerpVal = 0f;
		float growDuration = 0.5f;
		while (lerpVal < 1f)
		{
			lerpVal += Time.deltaTime / growDuration;
			float num = Mathf.Lerp(0f, 1f, AnimationCurves.EaseOutQuad.Evaluate(lerpVal));
			this.idolMeshRoot.transform.localScale = Vector3.one * num;
			this.tapCollision.radius = this._colliderBaseRadius * num;
			yield return null;
		}
		yield break;
	}

	[CompilerGenerated]
	private void <DoLookingAround>g__PickLookTarget|54_0(ref TappableGuardianIdol.<>c__DisplayClass54_0 A_1)
	{
		Transform transform = this.<DoLookingAround>g__GetClosestPlayerPosition|54_2(ref A_1);
		A_1._lookDirection = (transform ? Quaternion.LookRotation(transform.position - this._lookRoot.position) : Quaternion.Euler((float)Random.Range(-15, 15), this._lookRoot.rotation.eulerAngles.y + (float)Random.Range(-45, 45), 0f));
		this.<DoLookingAround>g__SetLookTime|54_1(ref A_1);
	}

	[CompilerGenerated]
	private void <DoLookingAround>g__SetLookTime|54_1(ref TappableGuardianIdol.<>c__DisplayClass54_0 A_1)
	{
		A_1.nextLookTime = Time.time + this._lookInterval / (float)this._activationState * 0.5f + Random.value;
	}

	[CompilerGenerated]
	private Transform <DoLookingAround>g__GetClosestPlayerPosition|54_2(ref TappableGuardianIdol.<>c__DisplayClass54_0 A_1)
	{
		if (Random.value < this._randomLookChance)
		{
			return null;
		}
		Vector3 position = base.transform.position;
		float num = float.MaxValue;
		Transform transform = null;
		foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
		{
			if (!(vrrig == null))
			{
				bool flag = vrrig.OwningNetPlayer == this.zoneManager.CurrentGuardian;
				float num2 = Vector3.SqrMagnitude(vrrig.transform.position - position) * (float)(flag ? 100 : 1);
				if (num2 < num)
				{
					num = num2;
					transform = vrrig.transform;
				}
			}
		}
		return transform;
	}

	[SerializeField]
	private GorillaGuardianZoneManager zoneManager;

	[SerializeField]
	private float floatDuration = 2f;

	[SerializeField]
	private float fallDuration = 1.5f;

	[SerializeField]
	private float inactiveDuration = 2f;

	[SerializeField]
	private float activationDuration = 1f;

	[SerializeField]
	private float activeHeight = 1f;

	[SerializeField]
	private bool knockbackOnTrigger;

	[SerializeField]
	private bool knockbackOnLand = true;

	[SerializeField]
	private bool knockbackOnActivate;

	[SerializeField]
	private Vector3 fallStartOffset = new Vector3(3f, 20f, 3f);

	[SerializeField]
	private ParticleSystem trailFX;

	[SerializeField]
	private ParticleSystem tapFX;

	[SerializeField]
	private GameObject explodeFX;

	[SerializeField]
	private GameObject startFallFX;

	[SerializeField]
	private GameObject landedFX;

	[SerializeField]
	private GameObject activatedFX;

	[SerializeField]
	private SphereCollider tapCollision;

	[SerializeField]
	private GameObject idolVisualRoot;

	[SerializeField]
	private GameObject idolMeshRoot;

	[SerializeField]
	private AnimationCurve bulgeCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f),
		new Keyframe(0.5f, 1f),
		new Keyframe(1f, 0f)
	});

	[SerializeField]
	private float bulgeScale = 1.1f;

	[SerializeField]
	private AudioSource _audio;

	[SerializeField]
	private AudioClip[] _descentSound;

	[SerializeField]
	private AudioClip[] _activateSound;

	[SerializeField]
	private TappableGuardianIdol.IdolActivationSound[] _activationStageSounds;

	[SerializeField]
	private TappableGuardianIdol.StageActivatedObject[] _stageActivatedObjects;

	[Header("Look Around")]
	[SerializeField]
	private Transform _lookRoot;

	[SerializeField]
	private float _lookInterval = 10f;

	[SerializeField]
	private float _baseLookRate = 1f;

	[SerializeField]
	private float _randomLookChance = 0.25f;

	private Coroutine _lookRoutine;

	private Vector3 transitionPos;

	private Vector3 finalPos;

	private int _activationState;

	private Coroutine _activationRoutine;

	private float _colliderBaseRadius;

	private bool _zoneIsActive = true;

	public bool isActivationReady;

	private float requiredTapDistance = 3f;

	[Serializable]
	public struct IdolActivationSound
	{
		public AudioClip activation;

		public AudioClip loop;
	}

	[Serializable]
	public struct StageActivatedObject
	{
		public void UpdateActiveState(int stage)
		{
			bool flag = stage >= this.min && stage <= this.max;
			GameObject[] array = this.objects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(flag);
			}
		}

		public GameObject[] objects;

		public int min;

		public int max;
	}
}
