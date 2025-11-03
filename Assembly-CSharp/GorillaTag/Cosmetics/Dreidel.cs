using System;
using CjLib;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class Dreidel : MonoBehaviour
	{
		public bool TrySetIdle()
		{
			if (this.state == Dreidel.State.Idle || this.state == Dreidel.State.FindingSurface || this.state == Dreidel.State.Fallen)
			{
				this.StartIdle();
				return true;
			}
			return false;
		}

		public bool TryCheckForSurfaces()
		{
			if (this.state == Dreidel.State.Idle || this.state == Dreidel.State.FindingSurface)
			{
				this.StartFindingSurfaces();
				return true;
			}
			return false;
		}

		public void Spin()
		{
			this.StartSpin();
		}

		public bool TryGetSpinStartData(out Vector3 surfacePoint, out Vector3 surfaceNormal, out float randomDuration, out Dreidel.Side randomSide, out Dreidel.Variation randomVariation, out double startTime)
		{
			if (this.canStartSpin)
			{
				surfacePoint = this.surfacePlanePoint;
				surfaceNormal = this.surfacePlaneNormal;
				randomDuration = Random.Range(this.spinTimeRange.x, this.spinTimeRange.y);
				randomSide = (Dreidel.Side)Random.Range(0, 4);
				randomVariation = (Dreidel.Variation)Random.Range(0, 5);
				startTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : (-1.0));
				return true;
			}
			surfacePoint = Vector3.zero;
			surfaceNormal = Vector3.zero;
			randomDuration = 0f;
			randomSide = Dreidel.Side.Shin;
			randomVariation = Dreidel.Variation.Tumble;
			startTime = -1.0;
			return false;
		}

		public void SetSpinStartData(Vector3 surfacePoint, Vector3 surfaceNormal, float duration, bool counterClockwise, Dreidel.Side side, Dreidel.Variation variation, double startTime)
		{
			this.surfacePlanePoint = surfacePoint;
			this.surfacePlaneNormal = surfaceNormal;
			this.spinTime = duration;
			this.spinStartTime = startTime;
			this.spinCounterClockwise = counterClockwise;
			this.landingSide = side;
			this.landingVariation = variation;
		}

		private void LateUpdate()
		{
			float deltaTime = Time.deltaTime;
			double num = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
			this.canStartSpin = false;
			switch (this.state)
			{
			default:
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
				this.spinTransform.localRotation = Quaternion.identity;
				this.spinTransform.localPosition = Vector3.zero;
				return;
			case Dreidel.State.FindingSurface:
			{
				float num2 = ((GTPlayer.Instance != null) ? GTPlayer.Instance.scale : 1f);
				Vector3 down = Vector3.down;
				Vector3 vector = base.transform.parent.position - down * 2f * this.surfaceCheckDistance * num2;
				float num3 = (3f * this.surfaceCheckDistance + -this.bottomPointOffset.y) * num2;
				RaycastHit raycastHit;
				if (Physics.Raycast(vector, down, out raycastHit, num3, this.surfaceLayers.value, QueryTriggerInteraction.Ignore) && Vector3.Dot(raycastHit.normal, Vector3.up) > this.surfaceUprightThreshold && Vector3.Dot(raycastHit.normal, this.spinTransform.up) > this.surfaceDreidelAngleThreshold)
				{
					this.canStartSpin = true;
					this.surfacePlanePoint = raycastHit.point;
					this.surfacePlaneNormal = raycastHit.normal;
					this.AlignToSurfacePlane();
					this.groundPointSpring.Reset(this.GetGroundContactPoint(), Vector3.zero);
					this.UpdateSpinTransform();
					return;
				}
				this.canStartSpin = false;
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
				this.spinTransform.localRotation = Quaternion.identity;
				this.spinTransform.localPosition = Vector3.zero;
				return;
			}
			case Dreidel.State.Spinning:
			{
				float num4 = Mathf.Clamp01((float)(num - this.stateStartTime) / this.spinTime);
				this.spinSpeed = Mathf.Lerp(this.spinSpeedStart, this.spinSpeedEnd, num4);
				float num5 = (this.spinCounterClockwise ? (-1f) : 1f);
				this.spinAngle += num5 * this.spinSpeed * 360f * deltaTime;
				float num6 = this.tiltWobble;
				float num7 = Mathf.Sin(this.spinWobbleFrequency * 2f * 3.1415927f * (float)(num - this.stateStartTime));
				float num8 = 0.5f * num7 + 0.5f;
				this.tiltWobble = Mathf.Lerp(this.spinWobbleAmplitudeEndMin * num4, this.spinWobbleAmplitude * num4, num8);
				if (this.landingTiltTarget.y == 0f)
				{
					if (this.landingVariation == Dreidel.Variation.Tumble || this.landingVariation == Dreidel.Variation.Smooth)
					{
						this.tiltFrontBack = Mathf.Sign(this.landingTiltTarget.x) * this.tiltWobble;
					}
					else
					{
						this.tiltFrontBack = Mathf.Sign(this.landingTiltLeadingTarget.x) * this.tiltWobble;
					}
				}
				else if (this.landingVariation == Dreidel.Variation.Tumble || this.landingVariation == Dreidel.Variation.Smooth)
				{
					this.tiltLeftRight = Mathf.Sign(this.landingTiltTarget.y) * this.tiltWobble;
				}
				else
				{
					this.tiltLeftRight = Mathf.Sign(this.landingTiltLeadingTarget.y) * this.tiltWobble;
				}
				float num9 = Mathf.Lerp(this.pathStartTurnRate, this.pathEndTurnRate, num4) + num7 * this.pathTurnRateSinOffset;
				if (this.spinCounterClockwise)
				{
					this.pathDir = Vector3.ProjectOnPlane(Quaternion.AngleAxis(-num9 * deltaTime, Vector3.up) * this.pathDir, Vector3.up);
					this.pathDir.Normalize();
				}
				else
				{
					this.pathDir = Vector3.ProjectOnPlane(Quaternion.AngleAxis(-num9 * deltaTime, Vector3.up) * this.pathDir, Vector3.up);
					this.pathDir.Normalize();
				}
				this.pathOffset += this.pathDir * this.pathMoveSpeed * deltaTime;
				this.AlignToSurfacePlane();
				this.UpdateSpinTransform();
				if (num4 - Mathf.Epsilon >= 1f && this.tiltWobble > 0.9f * this.spinWobbleAmplitude && num6 < this.tiltWobble)
				{
					this.StartFall();
					return;
				}
				break;
			}
			case Dreidel.State.Falling:
			{
				float num10 = this.fallTimeTumble;
				Dreidel.Variation variation = this.landingVariation;
				if (variation <= Dreidel.Variation.Smooth || variation - Dreidel.Variation.Bounce > 2)
				{
					this.spinSpeed = Mathf.MoveTowards(this.spinSpeed, 0f, this.spinSpeedStopRate * deltaTime);
					float num11 = (this.spinCounterClockwise ? (-1f) : 1f);
					this.spinAngle += num11 * this.spinSpeed * 360f * deltaTime;
					float num12 = ((this.landingVariation == Dreidel.Variation.Smooth) ? this.smoothFallFrequency : this.tumbleFallFrontBackFrequency);
					float num13 = ((this.landingVariation == Dreidel.Variation.Smooth) ? this.smoothFallDampingRatio : this.tumbleFallFrontBackDampingRatio);
					float num14 = ((this.landingVariation == Dreidel.Variation.Smooth) ? this.smoothFallFrequency : this.tumbleFallFrequency);
					float num15 = ((this.landingVariation == Dreidel.Variation.Smooth) ? this.smoothFallDampingRatio : this.tumbleFallDampingRatio);
					this.tiltFrontBack = this.tiltFrontBackSpring.TrackDampingRatio(this.landingTiltTarget.x, num12, num13, deltaTime);
					this.tiltLeftRight = this.tiltLeftRightSpring.TrackDampingRatio(this.landingTiltTarget.y, num14, num15, deltaTime);
				}
				else
				{
					bool flag = this.landingVariation != Dreidel.Variation.Bounce;
					bool flag2 = this.landingVariation == Dreidel.Variation.FalseSlowTurn;
					float num16 = (flag ? this.slowTurnSwitchTime : this.bounceFallSwitchTime);
					if (flag)
					{
						num10 = this.fallTimeSlowTurn;
					}
					if (num - this.stateStartTime < (double)num16)
					{
						this.tiltFrontBack = this.tiltFrontBackSpring.TrackDampingRatio(this.landingTiltLeadingTarget.x, this.tumbleFallFrontBackFrequency, this.tumbleFallFrontBackDampingRatio, deltaTime);
						this.tiltLeftRight = this.tiltLeftRightSpring.TrackDampingRatio(this.landingTiltLeadingTarget.y, this.tumbleFallFrequency, this.tumbleFallDampingRatio, deltaTime);
					}
					else
					{
						this.tiltFrontBack = this.tiltFrontBackSpring.TrackDampingRatio(this.landingTiltTarget.x, this.tumbleFallFrontBackFrequency, this.tumbleFallFrontBackDampingRatio, deltaTime);
						if (flag2)
						{
							if (!this.falseTargetReached && Mathf.Abs(this.landingTiltTarget.y - this.tiltLeftRight) > 0.49f)
							{
								this.tiltLeftRight = this.tiltLeftRightSpring.TrackDampingRatio(this.landingTiltTarget.y, this.slowTurnFrequency, this.slowTurnDampingRatio, deltaTime);
							}
							else
							{
								this.falseTargetReached = true;
								this.tiltLeftRight = this.tiltLeftRightSpring.TrackDampingRatio(this.landingTiltLeadingTarget.y, this.tumbleFallFrequency, this.tumbleFallDampingRatio, deltaTime);
							}
						}
						else if (flag && Mathf.Abs(this.landingTiltTarget.y - this.tiltLeftRight) > 0.45f)
						{
							this.tiltLeftRight = this.tiltLeftRightSpring.TrackDampingRatio(this.landingTiltTarget.y, this.slowTurnFrequency, this.slowTurnDampingRatio, deltaTime);
						}
						else
						{
							this.tiltLeftRight = this.tiltLeftRightSpring.TrackDampingRatio(this.landingTiltTarget.y, this.tumbleFallFrequency, this.tumbleFallDampingRatio, deltaTime);
						}
					}
					this.spinSpeed = Mathf.MoveTowards(this.spinSpeed, 0f, this.spinSpeedStopRate * deltaTime);
					float num17 = (this.spinCounterClockwise ? (-1f) : 1f);
					this.spinAngle += num17 * this.spinSpeed * 360f * deltaTime;
				}
				this.AlignToSurfacePlane();
				this.UpdateSpinTransform();
				float num18 = (float)(num - this.stateStartTime);
				if (num18 > num10)
				{
					if (!this.hasLanded)
					{
						this.hasLanded = true;
						if (this.landingSide == Dreidel.Side.Gimel)
						{
							this.gimelConfetti.transform.position = this.spinTransform.position + Vector3.up * this.confettiHeight;
							this.gimelConfetti.gameObject.SetActive(true);
							this.audioSource.GTPlayOneShot(this.gimelConfettiSound, 1f);
						}
					}
					if (num18 > num10 + this.respawnTimeAfterLanding)
					{
						this.StartIdle();
					}
				}
				break;
			}
			case Dreidel.State.Fallen:
				break;
			}
		}

		private void StartIdle()
		{
			this.state = Dreidel.State.Idle;
			this.stateStartTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
			this.canStartSpin = false;
			this.spinAngle = 0f;
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			this.spinTransform.localRotation = Quaternion.identity;
			this.spinTransform.localPosition = Vector3.zero;
			this.tiltFrontBack = 0f;
			this.tiltLeftRight = 0f;
			this.pathOffset = Vector3.zero;
			this.pathDir = Vector3.forward;
			this.gimelConfetti.gameObject.SetActive(false);
			this.groundPointSpring.Reset(this.GetGroundContactPoint(), Vector3.zero);
			this.UpdateSpinTransform();
		}

		private void StartFindingSurfaces()
		{
			this.state = Dreidel.State.FindingSurface;
			this.stateStartTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
			this.canStartSpin = false;
			this.spinAngle = 0f;
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			this.spinTransform.localRotation = Quaternion.identity;
			this.spinTransform.localPosition = Vector3.zero;
			this.tiltFrontBack = 0f;
			this.tiltLeftRight = 0f;
			this.pathOffset = Vector3.zero;
			this.pathDir = Vector3.forward;
			this.gimelConfetti.gameObject.SetActive(false);
			this.groundPointSpring.Reset(this.GetGroundContactPoint(), Vector3.zero);
			this.UpdateSpinTransform();
		}

		private void StartSpin()
		{
			this.state = Dreidel.State.Spinning;
			this.stateStartTime = ((this.spinStartTime > 0.0) ? this.spinStartTime : ((double)Time.time));
			this.canStartSpin = false;
			this.spinSpeed = this.spinSpeedStart;
			this.tiltWobble = 0f;
			this.audioSource.loop = true;
			this.audioSource.clip = this.spinLoopAudio;
			this.audioSource.GTPlay();
			this.gimelConfetti.gameObject.SetActive(false);
			this.AlignToSurfacePlane();
			this.groundPointSpring.Reset(this.GetGroundContactPoint(), Vector3.zero);
			this.UpdateSpinTransform();
			this.pathOffset = Vector3.zero;
			this.pathDir = Vector3.forward;
		}

		private void StartFall()
		{
			this.state = Dreidel.State.Falling;
			this.stateStartTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
			this.canStartSpin = false;
			this.falseTargetReached = false;
			this.hasLanded = false;
			if (this.landingVariation == Dreidel.Variation.FalseSlowTurn)
			{
				if (this.spinCounterClockwise)
				{
					this.GetTiltVectorsForSideWithPrev(this.landingSide, out this.landingTiltLeadingTarget, out this.landingTiltTarget);
				}
				else
				{
					this.GetTiltVectorsForSideWithNext(this.landingSide, out this.landingTiltLeadingTarget, out this.landingTiltTarget);
				}
			}
			else if (this.spinCounterClockwise)
			{
				this.GetTiltVectorsForSideWithNext(this.landingSide, out this.landingTiltTarget, out this.landingTiltLeadingTarget);
			}
			else
			{
				this.GetTiltVectorsForSideWithPrev(this.landingSide, out this.landingTiltTarget, out this.landingTiltLeadingTarget);
			}
			this.spinSpeedSpring.Reset(this.spinSpeed, 0f);
			this.tiltFrontBackSpring.Reset(this.tiltFrontBack, 0f);
			this.tiltLeftRightSpring.Reset(this.tiltLeftRight, 0f);
			this.groundPointSpring.Reset(this.GetGroundContactPoint(), Vector3.zero);
			this.audioSource.loop = false;
			this.audioSource.GTPlayOneShot(this.fallSound, 1f);
			this.gimelConfetti.gameObject.SetActive(false);
		}

		private Vector3 GetGroundContactPoint()
		{
			Vector3 position = this.spinTransform.position;
			this.dreidelCollider.enabled = true;
			Vector3 vector = this.dreidelCollider.ClosestPoint(position - base.transform.up);
			this.dreidelCollider.enabled = false;
			float num = Vector3.Dot(vector - position, this.spinTransform.up);
			if (num > 0f)
			{
				vector -= num * this.spinTransform.up;
			}
			return this.spinTransform.InverseTransformPoint(vector);
		}

		private void GetTiltVectorsForSideWithPrev(Dreidel.Side side, out Vector2 sideTilt, out Vector2 prevSideTilt)
		{
			int num = ((side <= Dreidel.Side.Shin) ? 3 : (side - Dreidel.Side.Hey));
			if (side == Dreidel.Side.Hey || side == Dreidel.Side.Nun)
			{
				sideTilt = this.landingTiltValues[(int)side];
				prevSideTilt = this.landingTiltValues[num];
				prevSideTilt.x = sideTilt.x;
				return;
			}
			prevSideTilt = this.landingTiltValues[num];
			sideTilt = this.landingTiltValues[(int)side];
			sideTilt.x = prevSideTilt.x;
		}

		private void GetTiltVectorsForSideWithNext(Dreidel.Side side, out Vector2 sideTilt, out Vector2 nextSideTilt)
		{
			int num = (int)((side + 1) % Dreidel.Side.Count);
			if (side == Dreidel.Side.Hey || side == Dreidel.Side.Nun)
			{
				sideTilt = this.landingTiltValues[(int)side];
				nextSideTilt = this.landingTiltValues[num];
				nextSideTilt.x = sideTilt.x;
				return;
			}
			nextSideTilt = this.landingTiltValues[num];
			sideTilt = this.landingTiltValues[(int)side];
			sideTilt.x = nextSideTilt.x;
		}

		private void AlignToSurfacePlane()
		{
			Vector3 vector = Vector3.forward;
			if (Vector3.Dot(Vector3.up, this.surfacePlaneNormal) < 0.9999f)
			{
				Vector3 vector2 = Vector3.Cross(this.surfacePlaneNormal, Vector3.up);
				vector = Quaternion.AngleAxis(90f, vector2) * this.surfacePlaneNormal;
			}
			Quaternion quaternion = Quaternion.LookRotation(vector, this.surfacePlaneNormal);
			base.transform.position = this.surfacePlanePoint;
			base.transform.rotation = quaternion;
		}

		private void UpdateSpinTransform()
		{
			Vector3 position = this.spinTransform.position;
			Vector3 groundContactPoint = this.GetGroundContactPoint();
			Vector3 vector = this.groundPointSpring.TrackDampingRatio(groundContactPoint, this.groundTrackingFrequency, this.groundTrackingDampingRatio, Time.deltaTime);
			Vector3 vector2 = this.spinTransform.TransformPoint(vector);
			Quaternion quaternion = Quaternion.AngleAxis(90f * this.tiltLeftRight, Vector3.forward) * Quaternion.AngleAxis(90f * this.tiltFrontBack, Vector3.right);
			this.spinAxis = base.transform.InverseTransformDirection(base.transform.up);
			Quaternion quaternion2 = Quaternion.AngleAxis(this.spinAngle, this.spinAxis);
			this.spinTransform.localRotation = quaternion2 * quaternion;
			Vector3 vector3 = base.transform.InverseTransformVector(Vector3.Dot(position - vector2, base.transform.up) * base.transform.up);
			this.spinTransform.localPosition = vector3 + this.pathOffset;
			this.spinTransform.TransformPoint(this.bottomPointOffset);
		}

		[Header("References")]
		[SerializeField]
		private Transform spinTransform;

		[SerializeField]
		private MeshCollider dreidelCollider;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private AudioClip spinLoopAudio;

		[SerializeField]
		private AudioClip fallSound;

		[SerializeField]
		private AudioClip gimelConfettiSound;

		[SerializeField]
		private ParticleSystem gimelConfetti;

		[Header("Offsets")]
		[SerializeField]
		private Vector3 centerOfMassOffset = Vector3.zero;

		[SerializeField]
		private Vector3 bottomPointOffset = Vector3.zero;

		[SerializeField]
		private Vector2 bodyRect = Vector2.one;

		[SerializeField]
		private float confettiHeight = 0.125f;

		[Header("Surface Detection")]
		[SerializeField]
		private float surfaceCheckDistance = 0.15f;

		[SerializeField]
		private float surfaceUprightThreshold = 0.5f;

		[SerializeField]
		private float surfaceDreidelAngleThreshold = 0.9f;

		[SerializeField]
		private LayerMask surfaceLayers;

		[Header("Spin Paramss")]
		[SerializeField]
		private float spinSpeedStart = 2f;

		[SerializeField]
		private float spinSpeedEnd = 1f;

		[SerializeField]
		private float spinTime = 10f;

		[SerializeField]
		private Vector2 spinTimeRange = new Vector2(7f, 12f);

		[SerializeField]
		private float spinWobbleFrequency = 0.1f;

		[SerializeField]
		private float spinWobbleAmplitude = 0.01f;

		[SerializeField]
		private float spinWobbleAmplitudeEndMin = 0.01f;

		[SerializeField]
		private float tiltFrontBack;

		[SerializeField]
		private float tiltLeftRight;

		[SerializeField]
		private float groundTrackingDampingRatio = 0.9f;

		[SerializeField]
		private float groundTrackingFrequency = 1f;

		[Header("Motion Path")]
		[SerializeField]
		private float pathMoveSpeed = 0.1f;

		[SerializeField]
		private float pathStartTurnRate = 360f;

		[SerializeField]
		private float pathEndTurnRate = 90f;

		[SerializeField]
		private float pathTurnRateSinOffset = 180f;

		[Header("Falling Params")]
		[SerializeField]
		private float spinSpeedStopRate = 1f;

		[SerializeField]
		private float tumbleFallDampingRatio = 0.4f;

		[SerializeField]
		private float tumbleFallFrequency = 6f;

		[SerializeField]
		private float tumbleFallFrontBackDampingRatio = 0.4f;

		[SerializeField]
		private float tumbleFallFrontBackFrequency = 6f;

		[SerializeField]
		private float smoothFallDampingRatio = 0.9f;

		[SerializeField]
		private float smoothFallFrequency = 2f;

		[SerializeField]
		private float slowTurnDampingRatio = 0.9f;

		[SerializeField]
		private float slowTurnFrequency = 2f;

		[SerializeField]
		private float bounceFallSwitchTime = 0.5f;

		[SerializeField]
		private float slowTurnSwitchTime = 0.5f;

		[SerializeField]
		private float respawnTimeAfterLanding = 3f;

		[SerializeField]
		private float fallTimeTumble = 3f;

		[SerializeField]
		private float fallTimeSlowTurn = 5f;

		private Dreidel.State state;

		private double stateStartTime;

		private float spinSpeed;

		private float spinAngle;

		private Vector3 spinAxis = Vector3.up;

		private bool canStartSpin;

		private double spinStartTime = -1.0;

		private float tiltWobble;

		private bool falseTargetReached;

		private bool hasLanded;

		private Vector3 pathOffset = Vector3.zero;

		private Vector3 pathDir = Vector3.forward;

		private Vector3 surfacePlanePoint;

		private Vector3 surfacePlaneNormal;

		private FloatSpring tiltFrontBackSpring;

		private FloatSpring tiltLeftRightSpring;

		private FloatSpring spinSpeedSpring;

		private Vector3Spring groundPointSpring;

		private Vector2[] landingTiltValues = new Vector2[]
		{
			new Vector2(1f, -1f),
			new Vector2(1f, 0f),
			new Vector2(-1f, 1f),
			new Vector2(-1f, 0f)
		};

		private Vector2 landingTiltLeadingTarget = Vector2.zero;

		private Vector2 landingTiltTarget = Vector2.zero;

		[Header("Debug Params")]
		[SerializeField]
		private Dreidel.Side landingSide;

		[SerializeField]
		private Dreidel.Variation landingVariation;

		[SerializeField]
		private bool spinCounterClockwise;

		[SerializeField]
		private bool debugDraw;

		private enum State
		{
			Idle,
			FindingSurface,
			Spinning,
			Falling,
			Fallen
		}

		public enum Side
		{
			Shin,
			Hey,
			Gimel,
			Nun,
			Count
		}

		public enum Variation
		{
			Tumble,
			Smooth,
			Bounce,
			SlowTurn,
			FalseSlowTurn,
			Count
		}
	}
}
