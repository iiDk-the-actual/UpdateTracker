using System;
using UnityEngine;

namespace GorillaLocomotion
{
	public sealed class Playspace : MonoBehaviour
	{
		private void Awake()
		{
			this._sqrSphereRadius = this._sphereRadius * this._sphereRadius;
			this._sqrSnapToThreshold = this._snapToThreshold * this._snapToThreshold;
		}

		private void Update()
		{
			Vector3 vector = this._localGorillaHead.transform.position - base.transform.position;
			float sqrMagnitude = vector.sqrMagnitude;
			if (GTPlayer.Instance.enableHoverMode || GTPlayer.Instance.isClimbing || vector.sqrMagnitude > this._sqrSnapToThreshold)
			{
				base.transform.position = this._localGorillaHead.transform.position;
				return;
			}
			Vector3 normalized = vector.normalized;
			vector = this.GetChaseSpeed() * Time.deltaTime * normalized;
			base.transform.position = ((vector.sqrMagnitude > sqrMagnitude) ? this._localGorillaHead.transform.position : (base.transform.position + vector));
			if ((this._localGorillaHead.transform.position - base.transform.position).sqrMagnitude > this._sqrSphereRadius)
			{
				this._localGorillaHead.transform.position = base.transform.position + this._sphereRadius * normalized;
			}
		}

		private float GetChaseSpeed()
		{
			return this._defaultChaseSpeed;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(base.transform.position, this._sphereRadius);
		}

		[SerializeField]
		private GameObject _localGorillaHead;

		[SerializeField]
		private float _sphereRadius;

		private float _sqrSphereRadius;

		[SerializeField]
		private float _defaultChaseSpeed;

		[SerializeField]
		private float _snapToThreshold;

		private float _sqrSnapToThreshold;
	}
}
