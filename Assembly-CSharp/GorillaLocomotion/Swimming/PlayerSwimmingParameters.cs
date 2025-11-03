using System;
using UnityEngine;

namespace GorillaLocomotion.Swimming
{
	[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerSwimmingParameters", order = 1)]
	public class PlayerSwimmingParameters : ScriptableObject
	{
		[Header("Base Settings")]
		public float floatingWaterLevelBelowHead = 0.6f;

		public float buoyancyFadeDist = 0.3f;

		public bool extendBouyancyFromSpeed;

		public float buoyancyExtensionDecayHalflife = 0.2f;

		public float baseUnderWaterDampingHalfLife = 0.25f;

		public float swimUnderWaterDampingHalfLife = 1.1f;

		public AnimationCurve speedToBouyancyExtension = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public Vector2 speedToBouyancyExtensionMinMax = Vector2.zero;

		public float swimmingVelocityOutOfWaterDrainRate = 3f;

		[Range(0f, 1f)]
		public float underwaterJumpsAsSwimVelocityFactor = 1f;

		[Range(0f, 1f)]
		public float swimmingHapticsStrength = 0.5f;

		[Header("Surface Jumping")]
		public bool allowWaterSurfaceJumps;

		public float waterSurfaceJumpHandSpeedThreshold = 1f;

		public float waterSurfaceJumpAmount;

		public float waterSurfaceJumpMaxSpeed = 1f;

		public AnimationCurve waterSurfaceJumpPalmFacingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public AnimationCurve waterSurfaceJumpHandVelocityFacingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Header("Diving")]
		public bool applyDiveSteering;

		public bool applyDiveDampingMultiplier;

		public float diveDampingMultiplier = 1f;

		[Tooltip("In degrees")]
		public float maxDiveSteerAnglePerStep = 1f;

		public float diveVelocityAveragingWindow = 0.1f;

		public bool applyDiveSwimVelocityConversion;

		[Tooltip("In meters per second")]
		public float diveSwimVelocityConversionRate = 3f;

		public float diveMaxSwimVelocityConversion = 3f;

		public bool reduceDiveSteeringBelowVelocityPlane;

		public float reduceDiveSteeringBelowPlaneFadeStartDist = 0.4f;

		public float reduceDiveSteeringBelowPlaneFadeEndDist = 0.55f;

		public AnimationCurve palmFacingToRedirectAmount = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public Vector2 palmFacingToRedirectAmountMinMax = Vector2.zero;

		public AnimationCurve swimSpeedToRedirectAmount = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public Vector2 swimSpeedToRedirectAmountMinMax = Vector2.zero;

		public AnimationCurve swimSpeedToMaxRedirectAngle = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public Vector2 swimSpeedToMaxRedirectAngleMinMax = Vector2.zero;

		public AnimationCurve handSpeedToRedirectAmount = AnimationCurve.Linear(0f, 1f, 1f, 0f);

		public Vector2 handSpeedToRedirectAmountMinMax = Vector2.zero;

		public AnimationCurve handAccelToRedirectAmount = AnimationCurve.Linear(0f, 1f, 1f, 0f);

		public Vector2 handAccelToRedirectAmountMinMax = Vector2.zero;

		public AnimationCurve nonDiveDampingHapticsAmount = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public Vector2 nonDiveDampingHapticsAmountMinMax = Vector2.zero;
	}
}
