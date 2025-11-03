using System;
using System.Collections.Generic;
using CjLib;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using UnityEngine;
using UnityEngine.XR;

public class TransferrableBall : TransferrableObject
{
	public override void TriggeredLateUpdate()
	{
		base.TriggeredLateUpdate();
		if (Time.time - this.hitSoundSpamLastHitTime > this.hitSoundSpamCooldownResetTime)
		{
			this.hitSoundSpamCount = 0;
		}
		bool flag = false;
		bool flag2 = false;
		float num = 1f;
		bool flag3 = this.leftHandOverlapping;
		bool flag4 = this.rightHandOverlapping;
		GTPlayer instance = GTPlayer.Instance;
		bool flag5 = false;
		foreach (KeyValuePair<GorillaHandClimber, int> keyValuePair in this.handClimberMap)
		{
			if (keyValuePair.Value > 0)
			{
				flag2 = true;
				Vector3 vector = Vector3.zero;
				bool flag6 = keyValuePair.Key.xrNode == XRNode.LeftHand;
				Vector3 position = instance.GetHandFollower(flag6).position;
				Quaternion rotation = instance.GetHandFollower(flag6).rotation;
				Transform handFollower = instance.GetHandFollower(flag6);
				Vector3 vector2;
				Vector3 vector3;
				if (flag6)
				{
					float num2;
					this.leftHandOverlapping = this.CheckCollisionWithHand(position, rotation, rotation * Vector3.right, out vector2, out vector3, out num2);
					if (this.leftHandOverlapping)
					{
						vector = instance.GetHandVelocityTracker(flag6).GetAverageVelocity(true, 0.15f, false);
					}
					else if ((position - base.transform.position).sqrMagnitude > num * num)
					{
						this.handClimberMap[keyValuePair.Key] = 0;
						continue;
					}
				}
				else
				{
					float num2;
					this.rightHandOverlapping = this.CheckCollisionWithHand(position, rotation, rotation * -Vector3.right, out vector2, out vector3, out num2);
					if (this.rightHandOverlapping)
					{
						vector = instance.GetHandVelocityTracker(flag6).GetAverageVelocity(true, 0.15f, false);
					}
					else if ((position - base.transform.position).sqrMagnitude > num * num)
					{
						this.handClimberMap[keyValuePair.Key] = 0;
						continue;
					}
				}
				if ((this.leftHandOverlapping || this.rightHandOverlapping) && (this.currentState == TransferrableObject.PositionState.None || this.currentState == TransferrableObject.PositionState.Dropped))
				{
					if (this.applyFrictionHolding)
					{
						if (flag6 && this.leftHandOverlapping)
						{
							if (!flag3)
							{
								Vector3 normalized = (handFollower.position - base.transform.position).normalized;
								Vector3 vector4 = normalized * this.ballRadius + base.transform.position;
								this.frictionHoldLocalPosLeft = base.transform.InverseTransformPoint(vector4);
								this.frictionHoldLocalRotLeft = Quaternion.LookRotation(normalized, handFollower.forward);
							}
							Vector3 vector5 = base.transform.TransformPoint(this.frictionHoldLocalPosLeft);
							this.frictionHoldLocalRotLeft = Quaternion.LookRotation(vector5 - base.transform.position, handFollower.forward);
							if (this.debugDraw)
							{
								Quaternion quaternion = this.frictionHoldLocalRotLeft * Quaternion.AngleAxis(90f, Vector3.right);
								DebugUtil.DrawRect(vector5, quaternion, new Vector2(0.08f, 0.15f), Color.green, false, DebugUtil.Style.Wireframe);
								Vector3 normalized2 = (instance.GetHandFollower(flag6).position - base.transform.position).normalized;
								Vector3 vector6 = normalized2 * this.ballRadius + base.transform.position;
								Quaternion quaternion2 = Quaternion.LookRotation(normalized2, handFollower.forward) * Quaternion.AngleAxis(90f, Vector3.right);
								DebugUtil.DrawRect(vector6, quaternion2, new Vector2(0.08f, 0.15f), Color.yellow, false, DebugUtil.Style.Wireframe);
							}
						}
						else if (!flag6 && this.rightHandOverlapping)
						{
							if (!flag4)
							{
								Vector3 normalized3 = (handFollower.position - base.transform.position).normalized;
								Vector3 vector7 = normalized3 * this.ballRadius + base.transform.position;
								this.frictionHoldLocalPosRight = base.transform.InverseTransformPoint(vector7);
								this.frictionHoldLocalRotRight = Quaternion.LookRotation(normalized3, handFollower.forward);
							}
							Vector3 vector8 = base.transform.TransformPoint(this.frictionHoldLocalPosRight);
							this.frictionHoldLocalRotRight = Quaternion.LookRotation(vector8 - base.transform.position, handFollower.forward);
							if (this.debugDraw)
							{
								Quaternion quaternion3 = this.frictionHoldLocalRotRight * Quaternion.AngleAxis(90f, Vector3.right);
								DebugUtil.DrawRect(vector8, quaternion3, new Vector2(0.08f, 0.15f), Color.green, false, DebugUtil.Style.Wireframe);
								Vector3 normalized4 = (handFollower.position - base.transform.position).normalized;
								Vector3 vector9 = normalized4 * this.ballRadius + base.transform.position;
								Quaternion quaternion4 = Quaternion.LookRotation(normalized4, handFollower.forward) * Quaternion.AngleAxis(90f, Vector3.right);
								DebugUtil.DrawRect(vector9, quaternion4, new Vector2(0.08f, 0.15f), Color.yellow, false, DebugUtil.Style.Wireframe);
							}
						}
					}
					bool flag7 = (flag6 && this.leftHandOverlapping && !flag3) || (!flag6 && this.rightHandOverlapping && !flag4);
					if (!flag5 && flag7)
					{
						Vector3 position2 = handFollower.position;
						float magnitude = vector.magnitude;
						Vector3 vector10 = vector / magnitude;
						Vector3 vector11 = -(position2 - base.transform.position).normalized;
						Vector3 vector12 = (vector10 + vector11) * 0.5f;
						flag5 = this.ApplyHit(position2, vector12, magnitude);
					}
					if (!flag5)
					{
						Vector3 position3 = handFollower.position;
						Vector3 vector13 = position3 - base.transform.position;
						float magnitude2 = vector13.magnitude;
						float num3 = this.ballRadius - vector13.magnitude;
						if (num3 > 0f)
						{
							Vector3 vector14 = -(vector13 / magnitude2) * num3;
							this.rigidbodyInstance.AddForce(-(vector13 / magnitude2) * this.depenetrationSpeed * Time.deltaTime * this.rigidbodyInstance.mass, ForceMode.Impulse);
							if (this.collisionContactsCount == 0)
							{
								this.rigidbodyInstance.MovePosition(base.transform.position + vector14 * this.depenetrationBias);
							}
							if (this.debugDraw)
							{
								DebugUtil.DrawLine(position3, position3 - vector14, Color.magenta, false);
							}
						}
					}
					if (this.debugDraw)
					{
						DebugUtil.DrawSphere(vector2, 0.01f, 6, 6, Color.green, true, DebugUtil.Style.SolidColor);
						DebugUtil.DrawArrow(vector2, vector2 - vector3 * 0.05f, 0.01f, Color.green, true, DebugUtil.Style.Wireframe);
					}
				}
				flag = flag || this.leftHandOverlapping || this.rightHandOverlapping;
			}
		}
		bool flag8 = this.headOverlapping;
		this.headOverlapping = false;
		if (this.allowHeadButting && !flag5 && this.playerHeadCollider != null)
		{
			Vector3 vector15;
			Vector3 vector16;
			float num4;
			this.headOverlapping = this.CheckCollisionWithHead(this.playerHeadCollider, out vector15, out vector16, out num4);
			Vector3 averagedVelocity = instance.AveragedVelocity;
			float magnitude3 = averagedVelocity.magnitude;
			if (this.headOverlapping && !flag8 && (double)magnitude3 > 0.0001)
			{
				Vector3 vector17 = averagedVelocity / magnitude3;
				flag5 = this.ApplyHit(vector15, vector17, magnitude3 * this.headButtHitMultiplier);
			}
			else if ((this.playerHeadCollider.transform.position - base.transform.position).sqrMagnitude > num * num)
			{
				this.playerHeadCollider = null;
			}
		}
		if (this.debugDraw && this.onGround)
		{
			DebugUtil.DrawLine(this.groundContact.point, this.groundContact.point + this.groundContact.normal * 0.2f, Color.yellow, false);
			DebugUtil.DrawRect(this.groundContact.point, Quaternion.LookRotation(this.groundContact.normal) * Quaternion.AngleAxis(90f, Vector3.right), Vector2.one * 0.2f, Color.yellow, false, DebugUtil.Style.Wireframe);
		}
		if (flag2 && this.debugDraw)
		{
			DebugUtil.DrawSphereTripleCircles(base.transform.position, this.ballRadius, 16, flag ? Color.green : Color.white, true, DebugUtil.Style.Wireframe);
			for (int i = 0; i < this.collisionContactsCount; i++)
			{
				ContactPoint contactPoint = this.collisionContacts[i];
				DebugUtil.DrawArrow(contactPoint.point, contactPoint.point + contactPoint.normal * 0.2f, 0.02f, Color.red, false, DebugUtil.Style.Wireframe);
			}
		}
	}

	private void TakeOwnershipAndEnablePhysics()
	{
		this.currentState = TransferrableObject.PositionState.Dropped;
		this.rigidbodyInstance.isKinematic = false;
		if (this.worldShareableInstance != null)
		{
			if (!this.worldShareableInstance.guard.isTrulyMine)
			{
				this.worldShareableInstance.guard.RequestOwnershipImmediately(delegate
				{
				});
			}
			this.worldShareableInstance.transferableObjectState = this.currentState;
		}
	}

	private bool CheckCollisionWithHand(Vector3 handCenter, Quaternion handRotation, Vector3 palmForward, out Vector3 hitPoint, out Vector3 hitNormal, out float penetrationDist)
	{
		Vector3 position = base.transform.position;
		bool flag = false;
		hitPoint = position;
		hitNormal = Vector3.forward;
		penetrationDist = 0f;
		Vector3 vector = position - handCenter;
		Vector3 vector2 = position - Vector3.Dot(vector, palmForward) * palmForward;
		Vector3 vector3 = vector2;
		if ((vector2 - handCenter).sqrMagnitude > this.handRadius * this.handRadius)
		{
			vector3 = handCenter + (vector2 - handCenter).normalized * this.handRadius;
		}
		if ((vector3 - position).sqrMagnitude < this.ballRadius * this.ballRadius)
		{
			flag = true;
			hitNormal = (position - vector3).normalized;
			hitPoint = position - hitNormal * this.ballRadius;
			penetrationDist = this.ballRadius - (vector3 - position).magnitude;
		}
		if (this.debugDraw)
		{
			Color color = (flag ? Color.green : Color.white);
			DebugUtil.DrawCircle(handCenter, handRotation * Quaternion.AngleAxis(90f, Vector3.forward), this.handRadius, 16, color, true, DebugUtil.Style.Wireframe);
			DebugUtil.DrawArrow(handCenter, handCenter + palmForward * 0.05f, 0.01f, color, true, DebugUtil.Style.Wireframe);
		}
		return flag;
	}

	private bool CheckCollisionWithHead(SphereCollider headCollider, out Vector3 hitPoint, out Vector3 hitNormal, out float penetrationDist)
	{
		Vector3 vector = base.transform.position - headCollider.transform.position;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = this.ballRadius + this.headButtRadius;
		if (sqrMagnitude < num * num)
		{
			float num2 = Mathf.Sqrt(sqrMagnitude);
			hitNormal = vector / num2;
			penetrationDist = num - num2;
			hitPoint = headCollider.transform.position + hitNormal * this.headButtRadius;
			return true;
		}
		hitNormal = Vector3.forward;
		hitPoint = Vector3.zero;
		penetrationDist = 0f;
		return false;
	}

	private bool ApplyHit(Vector3 hitPoint, Vector3 hitDir, float hitSpeed)
	{
		bool flag = false;
		this.TakeOwnershipAndEnablePhysics();
		float num = 0f;
		Vector3 vector = Vector3.zero;
		if (hitSpeed > 0.0001f)
		{
			float num2 = Vector3.Dot(this.rigidbodyInstance.linearVelocity, hitDir);
			float num3 = hitSpeed - num2;
			if (num3 > 0f)
			{
				num = num3;
				vector = num * hitDir;
			}
		}
		Vector3 normalized = (hitPoint - base.transform.position).normalized;
		float num4 = Vector3.Dot(this.rigidbodyInstance.linearVelocity, -normalized);
		if (num4 < 0f)
		{
			float num5 = Mathf.Lerp(this.reflectOffHandAmountOutputMinMax.x, this.reflectOffHandAmountOutputMinMax.y, Mathf.InverseLerp(this.reflectOffHandSpeedInputMinMax.x, this.reflectOffHandSpeedInputMinMax.y, num4));
			this.rigidbodyInstance.linearVelocity = num5 * Vector3.Reflect(this.rigidbodyInstance.linearVelocity, -normalized);
		}
		if (num > this.hitSpeedThreshold)
		{
			flag = true;
			float num6 = this.hitMultiplierCurve.Evaluate(Mathf.InverseLerp(this.hitSpeedToHitMultiplierMinMax.x, this.hitSpeedToHitMultiplierMinMax.y, num));
			if (this.onGround)
			{
				if (Vector3.Dot(vector, this.groundContact.normal) < 0f)
				{
					vector = Vector3.Reflect(vector, this.groundContact.normal);
				}
				Vector3 vector2 = vector / num;
				if (Vector3.Dot(vector2, this.groundContact.normal) < 0.707f)
				{
					vector = num * (vector2 + this.groundContact.normal) * 0.5f;
				}
			}
			this.rigidbodyInstance.AddForce(Vector3.ClampMagnitude(vector * num6, this.maxHitSpeed) * this.rigidbodyInstance.mass, ForceMode.Impulse);
			Vector3 vector3 = hitDir * hitSpeed - Vector3.Dot(hitDir * hitSpeed, normalized) * normalized;
			Vector3 normalized2 = Vector3.Cross(normalized, vector3).normalized;
			float num7 = Vector3.Dot(this.rigidbodyInstance.angularVelocity, normalized2);
			float num8 = vector3.magnitude / this.ballRadius - num7;
			if (num8 > 0f)
			{
				this.rigidbodyInstance.AddTorque(num6 * this.hitTorqueMultiplier * num8 * normalized2, ForceMode.VelocityChange);
			}
		}
		this.PlayHitSound(num * this.handHitAudioMultiplier);
		return flag;
	}

	private void PlayHitSound(float hitSpeed)
	{
		float num = Mathf.InverseLerp(this.hitSpeedToAudioMinMax.x, this.hitSpeedToAudioMinMax.y, hitSpeed);
		float num2 = Mathf.Lerp(this.hitSoundVolumeMinMax.x, this.hitSoundVolumeMinMax.y, num);
		float num3 = Mathf.Lerp(this.hitSoundPitchMinMax.x, this.hitSoundPitchMinMax.y, num);
		if (this.hitSoundBank != null && this.hitSoundSpamCount < this.hitSoundSpamLimit)
		{
			this.hitSoundSpamLastHitTime = Time.time;
			this.hitSoundSpamCount++;
			this.hitSoundBank.Play(new float?(num2), new float?(num3));
		}
	}

	private void FixedUpdate()
	{
		this.collisionContactsCount = 0;
		this.onGround = false;
		this.rigidbodyInstance.AddForce(-Physics.gravity * this.gravityCounterAmount * this.rigidbodyInstance.mass, ForceMode.Force);
	}

	private void OnTriggerEnter(Collider other)
	{
		GorillaHandClimber component = other.GetComponent<GorillaHandClimber>();
		if (!(component != null))
		{
			if (other.CompareTag(this.gorillaHeadTriggerTag))
			{
				this.playerHeadCollider = other as SphereCollider;
			}
			return;
		}
		int num;
		if (this.handClimberMap.TryGetValue(component, out num))
		{
			this.handClimberMap[component] = Mathf.Min(num + 1, 2);
			return;
		}
		this.handClimberMap.Add(component, 1);
	}

	private void OnTriggerExit(Collider other)
	{
		GorillaHandClimber component = other.GetComponent<GorillaHandClimber>();
		if (component != null)
		{
			int num;
			if (this.handClimberMap.TryGetValue(component, out num))
			{
				this.handClimberMap[component] = Mathf.Max(num - 1, 0);
				return;
			}
		}
		else if (other.CompareTag(this.gorillaHeadTriggerTag))
		{
			this.playerHeadCollider = null;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		this.PlayHitSound(collision.relativeVelocity.magnitude);
	}

	private void OnCollisionStay(Collision collision)
	{
		this.collisionContactsCount = collision.GetContacts(this.collisionContacts);
		float num = -1f;
		for (int i = 0; i < this.collisionContactsCount; i++)
		{
			float num2 = Vector3.Dot(this.collisionContacts[i].normal, Vector3.up);
			if (num2 > num)
			{
				this.groundContact = this.collisionContacts[i];
				num = num2;
			}
		}
		float num3 = 0.5f;
		this.onGround = num > num3;
	}

	[Header("Transferrable Ball")]
	public float ballRadius = 0.1f;

	public float depenetrationSpeed = 5f;

	[Range(0f, 1f)]
	public float hitSpeedThreshold = 0.8f;

	public float maxHitSpeed = 10f;

	public Vector2 hitSpeedToHitMultiplierMinMax = Vector2.one;

	public AnimationCurve hitMultiplierCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float hitTorqueMultiplier = 0.5f;

	public float reflectOffHandAmount = 0.5f;

	public float minHitSpeedThreshold = 0.2f;

	public float surfaceGripDistance = 0.02f;

	public Vector2 reflectOffHandSpeedInputMinMax = Vector2.one;

	public Vector2 reflectOffHandAmountOutputMinMax = Vector2.one;

	public SoundBankPlayer hitSoundBank;

	public Vector2 hitSpeedToAudioMinMax = Vector2.one;

	public float handHitAudioMultiplier = 2f;

	public Vector2 hitSoundPitchMinMax = Vector2.one;

	public Vector2 hitSoundVolumeMinMax = Vector2.one;

	public bool allowHeadButting = true;

	public float headButtRadius = 0.1f;

	public float headButtHitMultiplier = 1.5f;

	public float gravityCounterAmount;

	public bool debugDraw;

	private Dictionary<GorillaHandClimber, int> handClimberMap = new Dictionary<GorillaHandClimber, int>();

	private SphereCollider playerHeadCollider;

	private ContactPoint[] collisionContacts = new ContactPoint[8];

	private int collisionContactsCount;

	private float handRadius = 0.1f;

	private float depenetrationBias = 1f;

	private bool leftHandOverlapping;

	private bool rightHandOverlapping;

	private bool headOverlapping;

	private bool onGround;

	private ContactPoint groundContact;

	private bool applyFrictionHolding;

	private Vector3 frictionHoldLocalPosLeft;

	private Quaternion frictionHoldLocalRotLeft;

	private Vector3 frictionHoldLocalPosRight;

	private Quaternion frictionHoldLocalRotRight;

	private float hitSoundSpamLastHitTime;

	private int hitSoundSpamCount;

	private int hitSoundSpamLimit = 5;

	private float hitSoundSpamCooldownResetTime = 0.2f;

	private string gorillaHeadTriggerTag = "PlayerHeadTrigger";
}
