using System;
using GorillaExtensions;
using GorillaLocomotion;
using GT_CustomMapSupportRuntime;
using UnityEngine;

public class ForceVolume : MonoBehaviour, IGorillaSliceableSimple
{
	private void Awake()
	{
		this.volume = base.GetComponent<Collider>();
		this.audioState = ForceVolume.AudioState.None;
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		if (this.audioSource && this.audioSource != null && !this.audioSource.isPlaying && this.audioSource.enabled)
		{
			this.audioSource.enabled = false;
		}
	}

	private bool TriggerFilter(Collider other, out Rigidbody rb, out Transform xf)
	{
		rb = null;
		xf = null;
		if (other.gameObject == GorillaTagger.Instance.headCollider.gameObject)
		{
			rb = GorillaTagger.Instance.GetComponent<Rigidbody>();
			xf = GorillaTagger.Instance.headCollider.GetComponent<Transform>();
		}
		return rb != null && xf != null;
	}

	public void OnTriggerEnter(Collider other)
	{
		Rigidbody rigidbody = null;
		Transform transform = null;
		if (!this.TriggerFilter(other, out rigidbody, out transform))
		{
			return;
		}
		if (this.enterClip == null)
		{
			return;
		}
		if (this.audioSource)
		{
			this.audioSource.enabled = true;
			this.audioSource.GTPlayOneShot(this.enterClip, 1f);
			this.audioState = ForceVolume.AudioState.Enter;
		}
		this.enterPos = transform.position;
	}

	public void OnTriggerExit(Collider other)
	{
		Rigidbody rigidbody = null;
		Transform transform = null;
		if (!this.TriggerFilter(other, out rigidbody, out transform))
		{
			return;
		}
		if (this.audioSource)
		{
			this.audioSource.enabled = true;
			this.audioSource.GTPlayOneShot(this.exitClip, 1f);
			this.audioState = ForceVolume.AudioState.None;
		}
	}

	public void OnTriggerStay(Collider other)
	{
		Rigidbody rigidbody = null;
		Transform transform = null;
		if (!this.TriggerFilter(other, out rigidbody, out transform))
		{
			return;
		}
		if (this.audioSource && !this.audioSource.isPlaying)
		{
			ForceVolume.AudioState audioState = this.audioState;
			if (audioState != ForceVolume.AudioState.Enter)
			{
				if (audioState == ForceVolume.AudioState.Loop)
				{
					if (this.loopClip != null)
					{
						this.audioSource.enabled = true;
						this.audioSource.GTPlayOneShot(this.loopClip, 1f);
					}
					this.audioState = ForceVolume.AudioState.Loop;
				}
			}
			else
			{
				if (this.loopCresendoClip != null)
				{
					this.audioSource.enabled = true;
					this.audioSource.GTPlayOneShot(this.loopCresendoClip, 1f);
				}
				this.audioState = ForceVolume.AudioState.Crescendo;
			}
		}
		if (this.disableGrip)
		{
			GTPlayer.Instance.SetMaximumSlipThisFrame();
		}
		VRRig.LocalRig.BreakHandLinks();
		SizeManager sizeManager = null;
		if (this.scaleWithSize)
		{
			sizeManager = rigidbody.GetComponent<SizeManager>();
		}
		Vector3 vector = rigidbody.linearVelocity;
		if (this.scaleWithSize && sizeManager)
		{
			vector /= sizeManager.currentScale;
		}
		Vector3 vector2 = Vector3.Dot(transform.position - base.transform.position, base.transform.up) * base.transform.up;
		Vector3 vector3 = base.transform.position + vector2 - transform.position;
		float num = vector3.magnitude + 0.0001f;
		Vector3 vector4 = vector3 / num;
		float num2 = Vector3.Dot(vector, vector4);
		float num3 = this.accel;
		if (this.maxDepth > -1f)
		{
			float num4 = Vector3.Dot(transform.position - this.enterPos, vector4);
			float num5 = this.maxDepth - num4;
			float num6 = 0f;
			if (num5 > 0.0001f)
			{
				num6 = num2 * num2 / num5;
			}
			num3 = Mathf.Max(this.accel, num6);
		}
		float deltaTime = Time.deltaTime;
		Vector3 vector5 = base.transform.up * num3 * deltaTime;
		vector += vector5;
		Vector3 vector6 = Mathf.Min(Vector3.Dot(vector, base.transform.up), this.maxSpeed) * base.transform.up;
		Vector3 vector7 = Vector3.Dot(vector, base.transform.right) * base.transform.right;
		Vector3 vector8 = Vector3.Dot(vector, base.transform.forward) * base.transform.forward;
		float num7 = 1f;
		float num8 = 1f;
		if (this.dampenLateralVelocity)
		{
			num7 = 1f - this.dampenXVelPerc * 0.01f * deltaTime;
			num8 = 1f - this.dampenZVelPerc * 0.01f * deltaTime;
		}
		vector = vector6 + num7 * vector7 + num8 * vector8;
		if (this.applyPullToCenterAcceleration && this.pullToCenterAccel > 0f && this.pullToCenterMaxSpeed > 0f)
		{
			vector -= num2 * vector4;
			if (num > this.pullTOCenterMinDistance)
			{
				num2 += this.pullToCenterAccel * deltaTime;
				float num9 = Mathf.Min(this.pullToCenterMaxSpeed, num / deltaTime);
				num2 = Mathf.Min(num2, num9);
			}
			else
			{
				num2 = 0f;
			}
			vector += num2 * vector4;
			if (vector.magnitude > 0.0001f)
			{
				Vector3 vector9 = Vector3.Cross(base.transform.up, vector4);
				float magnitude = vector9.magnitude;
				if (magnitude > 0.0001f)
				{
					vector9 /= magnitude;
					num2 = Vector3.Dot(vector, vector9);
					vector -= num2 * vector9;
					num2 -= this.pullToCenterAccel * deltaTime;
					num2 = Mathf.Max(0f, num2);
					vector += num2 * vector9;
				}
			}
		}
		if (this.scaleWithSize && sizeManager)
		{
			vector *= sizeManager.currentScale;
		}
		rigidbody.linearVelocity = vector;
	}

	public void OnDrawGizmosSelected()
	{
		base.GetComponents<Collider>();
		Gizmos.color = Color.magenta;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(this.pullTOCenterMinDistance / base.transform.lossyScale.x, 1f, this.pullTOCenterMinDistance / base.transform.lossyScale.z));
	}

	public void SetPropertiesFromPlaceholder(ForceVolumeProperties properties, AudioSource volumeAudioSource, Collider colliderVolume)
	{
		this.accel = properties.accel;
		this.maxDepth = properties.maxDepth;
		this.maxSpeed = properties.maxSpeed;
		this.disableGrip = properties.disableGrip;
		this.dampenLateralVelocity = properties.dampenLateralVelocity;
		this.dampenXVelPerc = properties.dampenXVel;
		this.dampenZVelPerc = properties.dampenZVel;
		this.applyPullToCenterAcceleration = properties.applyPullToCenterAcceleration;
		this.pullToCenterAccel = properties.pullToCenterAccel;
		this.pullToCenterMaxSpeed = properties.pullToCenterMaxSpeed;
		this.pullTOCenterMinDistance = properties.pullToCenterMinDistance;
		this.enterClip = properties.enterClip;
		this.exitClip = properties.exitClip;
		this.loopClip = properties.loopClip;
		this.loopCresendoClip = properties.loopCrescendoClip;
		if (volumeAudioSource.IsNotNull())
		{
			this.audioSource = volumeAudioSource;
		}
		if (colliderVolume.IsNotNull())
		{
			this.volume = colliderVolume;
		}
	}

	[SerializeField]
	public bool scaleWithSize = true;

	[SerializeField]
	private float accel;

	[SerializeField]
	private float maxDepth = -1f;

	[SerializeField]
	private float maxSpeed;

	[SerializeField]
	private bool disableGrip;

	[SerializeField]
	private bool dampenLateralVelocity = true;

	[SerializeField]
	private float dampenXVelPerc;

	[SerializeField]
	private float dampenZVelPerc;

	[SerializeField]
	private bool applyPullToCenterAcceleration = true;

	[SerializeField]
	private float pullToCenterAccel;

	[SerializeField]
	private float pullToCenterMaxSpeed;

	[SerializeField]
	private float pullTOCenterMinDistance = 0.1f;

	private Collider volume;

	public AudioClip enterClip;

	public AudioClip exitClip;

	public AudioClip loopClip;

	public AudioClip loopCresendoClip;

	public AudioSource audioSource;

	private Vector3 enterPos;

	private ForceVolume.AudioState audioState;

	private enum AudioState
	{
		None,
		Enter,
		Crescendo,
		Loop,
		Exit
	}
}
