using System;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderSpeedBooster : MonoBehaviour
	{
		private void Awake()
		{
			this.volume = base.GetComponent<Collider>();
			this.windRenderer.enabled = false;
			this.boosting = false;
		}

		private void LateUpdate()
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

		private void CheckTableZone()
		{
			if (this.hasCheckedZone)
			{
				return;
			}
			BuilderTable builderTable;
			if (BuilderTable.TryGetBuilderTableForZone(GorillaTagger.Instance.offlineVRRig.zoneEntity.currentZone, out builderTable))
			{
				this.ignoreMonkeScale = !builderTable.isTableMutable;
			}
			this.hasCheckedZone = true;
		}

		public void OnTriggerEnter(Collider other)
		{
			Rigidbody rigidbody = null;
			Transform transform = null;
			if (!this.TriggerFilter(other, out rigidbody, out transform))
			{
				return;
			}
			this.CheckTableZone();
			if (!this.ignoreMonkeScale && (double)GorillaTagger.Instance.offlineVRRig.scaleFactor > 0.99)
			{
				return;
			}
			this.positiveForce = Vector3.Dot(base.transform.up, rigidbody.linearVelocity) > 0f;
			if (this.positiveForce)
			{
				this.windRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
			}
			else
			{
				this.windRenderer.transform.localRotation = Quaternion.Euler(0f, 180f, -90f);
			}
			this.windRenderer.enabled = true;
			this.enterPos = transform.position;
			if (!this.boosting)
			{
				this.boosting = true;
				this.enterTime = Time.timeAsDouble;
			}
		}

		public void OnTriggerExit(Collider other)
		{
			Rigidbody rigidbody = null;
			Transform transform = null;
			if (!this.TriggerFilter(other, out rigidbody, out transform))
			{
				return;
			}
			this.windRenderer.enabled = false;
			this.CheckTableZone();
			if (!this.ignoreMonkeScale && (double)GorillaTagger.Instance.offlineVRRig.scaleFactor > 0.99)
			{
				return;
			}
			if (this.boosting && this.audioSource)
			{
				this.audioSource.enabled = true;
				this.audioSource.Stop();
				this.audioSource.GTPlayOneShot(this.exitClip, 1f);
			}
			this.boosting = false;
		}

		public void OnTriggerStay(Collider other)
		{
			if (!this.boosting)
			{
				return;
			}
			Rigidbody rigidbody = null;
			Transform transform = null;
			if (!this.TriggerFilter(other, out rigidbody, out transform))
			{
				return;
			}
			if (!this.ignoreMonkeScale && (double)GorillaTagger.Instance.offlineVRRig.scaleFactor > 0.99)
			{
				this.OnTriggerExit(other);
				return;
			}
			if (Time.timeAsDouble > this.enterTime + (double)this.maxBoostDuration)
			{
				this.OnTriggerExit(other);
				return;
			}
			if (this.disableGrip)
			{
				GTPlayer.Instance.SetMaximumSlipThisFrame();
			}
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
			if (!this.positiveForce)
			{
				vector5 *= -1f;
			}
			vector += vector5;
			if ((double)Vector3.Dot(vector5, Vector3.down) <= 0.1)
			{
				vector += Vector3.up * this.addedWorldUpVelocity * deltaTime;
			}
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

		[SerializeField]
		private float addedWorldUpVelocity = 10f;

		[SerializeField]
		private float maxBoostDuration = 2f;

		private bool boosting;

		private double enterTime;

		private Collider volume;

		public AudioClip exitClip;

		public AudioSource audioSource;

		public MeshRenderer windRenderer;

		private Vector3 enterPos;

		private bool positiveForce = true;

		private bool ignoreMonkeScale;

		private bool hasCheckedZone;
	}
}
