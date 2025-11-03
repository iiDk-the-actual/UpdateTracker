using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class DicePhysics : MonoBehaviour
	{
		public int GetRandomSide()
		{
			DicePhysics.DiceType diceType = this.diceType;
			if (diceType != DicePhysics.DiceType.D6)
			{
				if (this.forceLandingSide)
				{
					return Mathf.Clamp(this.forcedLandingSide, 1, 20);
				}
				int num;
				if (this.CheckCosmeticRollOverride(out num))
				{
					return Mathf.Clamp(num, 1, 20);
				}
				return Random.Range(1, 21);
			}
			else
			{
				if (this.forceLandingSide)
				{
					return Mathf.Clamp(this.forcedLandingSide, 1, 6);
				}
				int num2;
				if (this.CheckCosmeticRollOverride(out num2))
				{
					return Mathf.Clamp(num2, 1, 6);
				}
				return Random.Range(1, 7);
			}
		}

		public Vector3 GetSideDirection(int side)
		{
			DicePhysics.DiceType diceType = this.diceType;
			if (diceType != DicePhysics.DiceType.D6)
			{
				int num = Mathf.Clamp(side - 1, 0, 19);
				return this.d20SideDirections[num];
			}
			int num2 = Mathf.Clamp(side - 1, 0, 5);
			return this.d6SideDirections[num2];
		}

		public void StartThrow(DiceHoldable holdable, Vector3 startPosition, Vector3 velocity, float playerScale, int side, double startTime)
		{
			this.holdableParent = holdable;
			base.transform.parent = null;
			base.transform.position = startPosition;
			base.transform.localScale = Vector3.one * playerScale;
			this.rb.isKinematic = false;
			this.rb.useGravity = true;
			this.rb.linearVelocity = velocity;
			if (!this.allowPickupFromGround && this.interactionPoint != null)
			{
				this.interactionPoint.enabled = false;
			}
			this.throwStartTime = ((startTime > 0.0) ? startTime : ((double)Time.time));
			this.throwSettledTime = -1.0;
			this.scale = playerScale;
			this.landingSide = Mathf.Clamp(side, 1, 20);
			this.prevVelocity = Vector3.zero;
			velocity = Vector3.zero;
			base.enabled = true;
		}

		public void EndThrow()
		{
			this.rb.isKinematic = true;
			this.rb.linearVelocity = Vector3.zero;
			if (this.holdableParent != null)
			{
				base.transform.parent = this.holdableParent.transform;
			}
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			base.transform.localScale = Vector3.one;
			this.scale = 1f;
			this.throwStartTime = -1.0;
			if (this.interactionPoint != null)
			{
				this.interactionPoint.enabled = true;
			}
			this.onRollFinished.Invoke();
			base.enabled = false;
		}

		private void FixedUpdate()
		{
			double num = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
			float num2 = (float)(num - this.throwStartTime);
			RaycastHit raycastHit;
			if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 0.1f * this.scale, this.surfaceLayers.value, QueryTriggerInteraction.Ignore))
			{
				Vector3 normal = raycastHit.normal;
				Vector3 sideDirection = this.GetSideDirection(this.landingSide);
				Vector3 vector = base.transform.rotation * sideDirection;
				Vector3 normalized = Vector3.Cross(vector, normal).normalized;
				float num3 = Vector3.SignedAngle(vector, normal, normalized);
				float num4 = Mathf.Sign(num3) * this.angleDeltaVsStrengthCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(num3) / 180f));
				float num5 = this.landingTimeVsStrengthCurve.Evaluate(Mathf.Clamp01(num2 / this.landingTime));
				float magnitude = this.rb.linearVelocity.magnitude;
				float num6 = Mathf.Clamp01(1f - Mathf.Min(magnitude, 1f));
				float num7 = Mathf.Max(num5, num6);
				Vector3 vector2 = this.strength * (num7 * num4 * normalized) - this.damping * this.rb.angularVelocity;
				this.rb.AddTorque(vector2, ForceMode.Acceleration);
				if (!this.rb.isKinematic && magnitude < 0.01f && num3 < 2f)
				{
					this.rb.isKinematic = true;
					this.throwSettledTime = num;
					this.InvokeLandingEffects(this.landingSide);
				}
				else if (!this.rb.isKinematic && num2 > this.landingTime)
				{
					this.rb.isKinematic = true;
					this.throwSettledTime = num;
					base.transform.rotation = Quaternion.FromToRotation(vector, normal) * base.transform.rotation;
					this.InvokeLandingEffects(this.landingSide);
				}
			}
			if (num2 > this.landingTime + this.postLandingTime || (this.rb.isKinematic && (float)(num - this.throwSettledTime) > this.postLandingTime))
			{
				this.EndThrow();
			}
			this.prevVelocity = this.velocity;
			this.velocity = this.rb.linearVelocity;
		}

		private void OnCollisionEnter(Collision collision)
		{
			float magnitude = collision.impulse.magnitude;
			if (magnitude > 0.001f)
			{
				Vector3 vector = Vector3.Reflect(this.prevVelocity, collision.impulse / magnitude);
				this.rb.linearVelocity = vector * this.bounceAmplification;
			}
		}

		private void InvokeLandingEffects(int side)
		{
			DicePhysics.DiceType diceType = this.diceType;
			if (diceType != DicePhysics.DiceType.D6)
			{
				if (side == 20)
				{
					this.onBestRoll.Invoke();
					return;
				}
				if (side == 1)
				{
					this.onWorstRoll.Invoke();
					return;
				}
			}
			else
			{
				if (side == 6)
				{
					this.onBestRoll.Invoke();
					return;
				}
				if (side == 1)
				{
					this.onWorstRoll.Invoke();
				}
			}
		}

		private bool CheckCosmeticRollOverride(out int rollSide)
		{
			if (this.cosmeticRollOverrides.Length != 0)
			{
				if (this.cachedLocalRig == null)
				{
					RigContainer rigContainer;
					if (PhotonNetwork.InRoom && VRRigCache.Instance.TryGetVrrig(PhotonNetwork.LocalPlayer, out rigContainer) && rigContainer.Rig != null)
					{
						this.cachedLocalRig = rigContainer.Rig;
					}
					else
					{
						this.cachedLocalRig = GorillaTagger.Instance.offlineVRRig;
					}
				}
				if (this.cachedLocalRig != null)
				{
					int num = -1;
					for (int i = 0; i < this.cosmeticRollOverrides.Length; i++)
					{
						if (this.cosmeticRollOverrides[i].cosmeticName != null && this.cachedLocalRig.cosmeticSet != null && this.cachedLocalRig.cosmeticSet.HasItem(this.cosmeticRollOverrides[i].cosmeticName) && (!this.cosmeticRollOverrides[i].requireHolding || (EquipmentInteractor.instance.leftHandHeldEquipment != null && EquipmentInteractor.instance.leftHandHeldEquipment.name == this.cosmeticRollOverrides[i].cosmeticName) || (EquipmentInteractor.instance.rightHandHeldEquipment != null && EquipmentInteractor.instance.rightHandHeldEquipment.name == this.cosmeticRollOverrides[i].cosmeticName)) && this.cosmeticRollOverrides[i].landingSide > num)
						{
							num = this.cosmeticRollOverrides[i].landingSide;
						}
					}
					if (num > 0)
					{
						rollSide = num;
						return true;
					}
				}
			}
			rollSide = 0;
			return false;
		}

		[SerializeField]
		private DicePhysics.DiceType diceType = DicePhysics.DiceType.D20;

		[SerializeField]
		private float landingTime = 5f;

		[SerializeField]
		private float postLandingTime = 2f;

		[SerializeField]
		private LayerMask surfaceLayers;

		[SerializeField]
		private AnimationCurve angleDeltaVsStrengthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private AnimationCurve landingTimeVsStrengthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private float strength = 1f;

		[SerializeField]
		private float damping = 0.5f;

		[SerializeField]
		private bool forceLandingSide;

		[SerializeField]
		private int forcedLandingSide = 20;

		[SerializeField]
		private bool allowPickupFromGround = true;

		[SerializeField]
		private float bounceAmplification = 1f;

		[SerializeField]
		private DicePhysics.CosmeticRollOverride[] cosmeticRollOverrides;

		[SerializeField]
		private UnityEvent onBestRoll;

		[SerializeField]
		private UnityEvent onWorstRoll;

		[SerializeField]
		private UnityEvent onRollFinished;

		[SerializeField]
		private Rigidbody rb;

		[SerializeField]
		private InteractionPoint interactionPoint;

		private VRRig cachedLocalRig;

		private DiceHoldable holdableParent;

		private double throwStartTime = -1.0;

		private double throwSettledTime = -1.0;

		private int landingSide;

		private float scale;

		private Vector3 prevVelocity = Vector3.zero;

		private Vector3 velocity = Vector3.zero;

		private const float a = 38.833332f;

		private const float b = 77.66666f;

		private Vector3[] d20SideDirections = new Vector3[]
		{
			Quaternion.AngleAxis(144f, Vector3.up) * Quaternion.AngleAxis(38.833332f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(324f, -Vector3.up) * Quaternion.AngleAxis(38.833332f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(288f, Vector3.up) * Quaternion.AngleAxis(38.833332f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(180f, -Vector3.up) * Quaternion.AngleAxis(38.833332f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(252f, -Vector3.up) * Quaternion.AngleAxis(77.66666f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(108f, -Vector3.up) * Quaternion.AngleAxis(77.66666f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(72f, Vector3.up) * Quaternion.AngleAxis(38.833332f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(36f, -Vector3.up) * Quaternion.AngleAxis(77.66666f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(216f, Vector3.up) * Quaternion.AngleAxis(77.66666f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(0f, Vector3.up) * Quaternion.AngleAxis(77.66666f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(180f, -Vector3.up) * Quaternion.AngleAxis(77.66666f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(324f, -Vector3.up) * Quaternion.AngleAxis(77.66666f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(144f, Vector3.up) * Quaternion.AngleAxis(77.66666f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(108f, -Vector3.up) * Quaternion.AngleAxis(38.833332f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(72f, Vector3.up) * Quaternion.AngleAxis(77.66666f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(288f, Vector3.up) * Quaternion.AngleAxis(77.66666f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(0f, Vector3.up) * Quaternion.AngleAxis(38.833332f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(252f, -Vector3.up) * Quaternion.AngleAxis(38.833332f, -Vector3.forward) * Vector3.up,
			Quaternion.AngleAxis(216f, Vector3.up) * Quaternion.AngleAxis(38.833332f, Vector3.forward) * -Vector3.up,
			Quaternion.AngleAxis(36f, -Vector3.up) * Quaternion.AngleAxis(38.833332f, -Vector3.forward) * Vector3.up
		};

		private Vector3[] d6SideDirections = new Vector3[]
		{
			new Vector3(0f, -1f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(0f, 0f, -1f),
			new Vector3(0f, 0f, 1f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 1f, 0f)
		};

		private enum DiceType
		{
			D6,
			D20
		}

		[Serializable]
		private struct CosmeticRollOverride
		{
			public string cosmeticName;

			public int landingSide;

			public bool requireHolding;
		}
	}
}
