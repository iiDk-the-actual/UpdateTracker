using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class RCHelicopter : RCVehicle
	{
		protected override void AuthorityBeginDocked()
		{
			base.AuthorityBeginDocked();
			this.turnRate = 0f;
			this.verticalPropeller.localRotation = this.verticalPropellerBaseRotation;
			this.turnPropeller.localRotation = this.turnPropellerBaseRotation;
			if (this.connectedRemote == null)
			{
				base.gameObject.SetActive(false);
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.verticalPropellerBaseRotation = this.verticalPropeller.localRotation;
			this.turnPropellerBaseRotation = this.turnPropeller.localRotation;
			this.ascendAccel = this.maxAscendSpeed / this.ascendAccelTime;
			this.turnAccel = this.maxTurnRate / this.turnAccelTime;
			this.horizontalAccel = this.maxHorizontalSpeed / this.horizontalAccelTime;
		}

		protected override void SharedUpdate(float dt)
		{
			if (this.localState == RCVehicle.State.Mobilized)
			{
				float num = Mathf.Lerp(this.mainPropellerSpinRateRange.x, this.mainPropellerSpinRateRange.y, this.activeInput.trigger);
				this.verticalPropeller.Rotate(new Vector3(0f, num * dt, 0f), Space.Self);
				this.turnPropeller.Rotate(new Vector3(this.activeInput.joystick.x * this.backPropellerSpinRate * dt, 0f, 0f), Space.Self);
			}
		}

		private void FixedUpdate()
		{
			if (!base.HasLocalAuthority || this.localState != RCVehicle.State.Mobilized)
			{
				return;
			}
			float fixedDeltaTime = Time.fixedDeltaTime;
			Vector3 linearVelocity = this.rb.linearVelocity;
			float magnitude = linearVelocity.magnitude;
			float num = this.activeInput.joystick.x * this.maxTurnRate;
			this.turnRate = Mathf.MoveTowards(this.turnRate, num, this.turnAccel * fixedDeltaTime);
			float num2 = this.activeInput.joystick.y * this.maxHorizontalSpeed;
			float num3 = Mathf.Sign(this.activeInput.joystick.y) * Mathf.Lerp(0f, this.maxHorizontalTiltAngle, Mathf.Abs(this.activeInput.joystick.y));
			base.transform.rotation = Quaternion.Euler(new Vector3(num3, this.turnAccel, 0f));
			float num4 = Mathf.Abs(num2);
			Vector3 normalized = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up).normalized;
			float num5 = Vector3.Dot(normalized, linearVelocity);
			if (num4 > 0.01f && ((num2 > 0f && num2 > num5) || (num2 < 0f && num2 < num5)))
			{
				this.rb.AddForce(normalized * Mathf.Sign(num2) * this.horizontalAccel * fixedDeltaTime * this.rb.mass, ForceMode.Force);
			}
			float num6 = this.activeInput.trigger * this.maxAscendSpeed;
			if (num6 > 0.01f && linearVelocity.y < num6)
			{
				this.rb.AddForce(Vector3.up * this.ascendAccel * this.rb.mass, ForceMode.Force);
			}
			if (this.rb.useGravity)
			{
				this.rb.AddForce(-Physics.gravity * this.gravityCompensation * this.rb.mass, ForceMode.Force);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.isTrigger && base.HasLocalAuthority && this.localState == RCVehicle.State.Mobilized)
			{
				this.AuthorityBeginCrash();
			}
		}

		[SerializeField]
		private float maxAscendSpeed = 6f;

		[SerializeField]
		private float ascendAccelTime = 3f;

		[SerializeField]
		private float gravityCompensation = 0.5f;

		[SerializeField]
		private float maxTurnRate = 90f;

		[SerializeField]
		private float turnAccelTime = 0.75f;

		[SerializeField]
		private float maxHorizontalSpeed = 6f;

		[SerializeField]
		private float horizontalAccelTime = 2f;

		[SerializeField]
		private float maxHorizontalTiltAngle = 45f;

		[SerializeField]
		private Vector2 mainPropellerSpinRateRange = new Vector2(3f, 15f);

		[SerializeField]
		private float backPropellerSpinRate = 5f;

		[SerializeField]
		private Transform verticalPropeller;

		[SerializeField]
		private Transform turnPropeller;

		private Quaternion verticalPropellerBaseRotation;

		private Quaternion turnPropellerBaseRotation;

		private float turnRate;

		private float ascendAccel;

		private float turnAccel;

		private float horizontalAccel;
	}
}
