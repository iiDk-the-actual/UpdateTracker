using System;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTagScripts.Builder
{
	public class RecyclerForceVolume : MonoBehaviour
	{
		private void Awake()
		{
			this.volume = base.GetComponent<Collider>();
			this.hasWindFX = this.windEffectRenderer != null;
			if (this.hasWindFX)
			{
				this.windEffectRenderer.enabled = false;
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
			this.enterPos = transform.position;
			ObjectPools.instance.Instantiate(this.windSFX, this.enterPos, true);
			if (this.hasWindFX)
			{
				this.windEffectRenderer.transform.position = base.transform.position + Vector3.Dot(this.enterPos - base.transform.position, base.transform.right) * base.transform.right;
				this.windEffectRenderer.enabled = true;
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
			if (this.hasWindFX)
			{
				this.windEffectRenderer.enabled = false;
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
			Vector3 vector2 = Vector3.Dot(base.transform.position - transform.position, base.transform.up) * base.transform.up;
			float num = vector2.magnitude + 0.0001f;
			Vector3 vector3 = vector2 / num;
			float num2 = Vector3.Dot(vector, vector3);
			float num3 = this.accel;
			if (this.maxDepth > -1f)
			{
				float num4 = Vector3.Dot(transform.position - this.enterPos, vector3);
				float num5 = this.maxDepth - num4;
				float num6 = 0f;
				if (num5 > 0.0001f)
				{
					num6 = num2 * num2 / num5;
				}
				num3 = Mathf.Max(this.accel, num6);
			}
			float deltaTime = Time.deltaTime;
			Vector3 vector4 = base.transform.forward * num3 * deltaTime;
			vector += vector4;
			Vector3 vector5 = Vector3.Dot(vector, base.transform.up) * base.transform.up;
			Vector3 vector6 = Vector3.Dot(vector, base.transform.right) * base.transform.right;
			Vector3 vector7 = Mathf.Clamp(Vector3.Dot(vector, base.transform.forward), -1f * this.maxSpeed, this.maxSpeed) * base.transform.forward;
			float num7 = 1f;
			float num8 = 1f;
			if (this.dampenLateralVelocity)
			{
				num7 = 1f - this.dampenXVelPerc * 0.01f * deltaTime;
				num8 = 1f - this.dampenYVelPerc * 0.01f * deltaTime;
			}
			vector = num8 * vector5 + num7 * vector6 + vector7;
			if (this.applyPullToCenterAcceleration && this.pullToCenterAccel > 0f && this.pullToCenterMaxSpeed > 0f)
			{
				vector -= num2 * vector3;
				if (num > this.pullTOCenterMinDistance)
				{
					num2 += this.pullToCenterAccel * deltaTime;
					float num9 = Mathf.Min(this.pullToCenterMaxSpeed, num / deltaTime);
					num2 = Mathf.Clamp(num2, -1f * num9, num9);
				}
				else
				{
					num2 = 0f;
				}
				vector += num2 * vector3;
			}
			if (this.scaleWithSize && sizeManager)
			{
				vector *= sizeManager.currentScale;
			}
			rigidbody.linearVelocity = vector;
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

		[FormerlySerializedAs("dampenZVelPerc")]
		[SerializeField]
		private float dampenYVelPerc;

		[SerializeField]
		private bool applyPullToCenterAcceleration = true;

		[SerializeField]
		private float pullToCenterAccel;

		[SerializeField]
		private float pullToCenterMaxSpeed;

		[SerializeField]
		private float pullTOCenterMinDistance = 0.1f;

		private Collider volume;

		public GameObject windSFX;

		[SerializeField]
		private MeshRenderer windEffectRenderer;

		private bool hasWindFX;

		private Vector3 enterPos;
	}
}
