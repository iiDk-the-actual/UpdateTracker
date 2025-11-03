using System;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderScaleParticles : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this.useLossyScale)
			{
				this.setScaleNextFrame = true;
				this.enableFrame = Time.frameCount;
			}
		}

		private void LateUpdate()
		{
			if (this.setScaleNextFrame && Time.frameCount > this.enableFrame)
			{
				if (this.useLossyScale)
				{
					this.SetScale(base.transform.lossyScale.x);
				}
				this.setScaleNextFrame = false;
			}
		}

		private void OnDisable()
		{
			if (this.useLossyScale)
			{
				this.RevertScale();
			}
		}

		public void SetScale(float inScale)
		{
			bool isPlaying = this.system.isPlaying;
			if (isPlaying)
			{
				this.system.Stop();
				this.system.Clear();
			}
			if (Mathf.Approximately(inScale, this.scale))
			{
				if (this.autoPlay || isPlaying)
				{
					this.system.Play(true);
				}
				return;
			}
			this.scale = inScale;
			this.RevertScale();
			if (Mathf.Approximately(this.scale, 1f))
			{
				if (this.autoPlay || isPlaying)
				{
					this.system.Play(true);
				}
				return;
			}
			ParticleSystem.MainModule main = this.system.main;
			this.gravityMod = main.gravityModifierMultiplier;
			main.gravityModifierMultiplier = this.gravityMod * this.scale;
			if (main.startSize3D)
			{
				ParticleSystem.MinMaxCurve startSizeX = main.startSizeX;
				this.sizeCurveXCache = main.startSizeX;
				this.ScaleCurve(ref startSizeX, this.scale);
				main.startSizeX = startSizeX;
				ParticleSystem.MinMaxCurve startSizeY = main.startSizeY;
				this.sizeCurveYCache = main.startSizeY;
				this.ScaleCurve(ref startSizeY, this.scale);
				main.startSizeY = startSizeY;
				ParticleSystem.MinMaxCurve startSizeZ = main.startSizeZ;
				this.sizeCurveZCache = main.startSizeZ;
				this.ScaleCurve(ref startSizeZ, this.scale);
				main.startSizeZ = startSizeZ;
			}
			else
			{
				ParticleSystem.MinMaxCurve startSize = main.startSize;
				this.sizeCurveCache = main.startSize;
				this.ScaleCurve(ref startSize, this.scale);
				main.startSize = startSize;
			}
			ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
			this.speedCurveCache = main.startSpeed;
			this.ScaleCurve(ref startSpeed, this.scale);
			main.startSpeed = startSpeed;
			if (this.scaleShape)
			{
				ParticleSystem.ShapeModule shape = this.system.shape;
				this.shapeScale = shape.scale;
				shape.scale = this.shapeScale * this.scale;
			}
			if (this.scaleVelocityLifetime)
			{
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = this.system.velocityOverLifetime;
				this.lifetimeVelocityX = velocityOverLifetime.x;
				this.lifetimeVelocityY = velocityOverLifetime.y;
				this.lifetimeVelocityZ = velocityOverLifetime.z;
				ParticleSystem.MinMaxCurve minMaxCurve = velocityOverLifetime.x;
				this.ScaleCurve(ref minMaxCurve, this.scale);
				velocityOverLifetime.x = minMaxCurve;
				minMaxCurve = velocityOverLifetime.y;
				this.ScaleCurve(ref minMaxCurve, this.scale);
				velocityOverLifetime.y = minMaxCurve;
				minMaxCurve = velocityOverLifetime.z;
				this.ScaleCurve(ref minMaxCurve, this.scale);
				velocityOverLifetime.z = minMaxCurve;
			}
			if (this.scaleVelocityLimitLifetime)
			{
				ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = this.system.limitVelocityOverLifetime;
				this.limitMultiplier = limitVelocityOverLifetime.limitMultiplier;
				limitVelocityOverLifetime.limitMultiplier = this.limitMultiplier * this.scale;
			}
			if (this.scaleForceOverLife)
			{
				ParticleSystem.ForceOverLifetimeModule forceOverLifetime = this.system.forceOverLifetime;
				this.forceX = forceOverLifetime.x;
				this.forceY = forceOverLifetime.y;
				this.forceZ = forceOverLifetime.z;
				ParticleSystem.MinMaxCurve minMaxCurve2 = forceOverLifetime.x;
				this.ScaleCurve(ref minMaxCurve2, this.scale);
				forceOverLifetime.x = minMaxCurve2;
				minMaxCurve2 = forceOverLifetime.y;
				this.ScaleCurve(ref minMaxCurve2, this.scale);
				forceOverLifetime.y = minMaxCurve2;
				minMaxCurve2 = forceOverLifetime.z;
				this.ScaleCurve(ref minMaxCurve2, this.scale);
				forceOverLifetime.z = minMaxCurve2;
			}
			if (this.autoPlay || isPlaying)
			{
				this.system.Play(true);
			}
			this.shouldRevert = true;
		}

		private void ScaleCurve(ref ParticleSystem.MinMaxCurve curve, float scale)
		{
			switch (curve.mode)
			{
			case ParticleSystemCurveMode.Constant:
				curve.constant *= scale;
				return;
			case ParticleSystemCurveMode.Curve:
			case ParticleSystemCurveMode.TwoCurves:
				curve.curveMultiplier *= scale;
				return;
			case ParticleSystemCurveMode.TwoConstants:
				curve.constantMin *= scale;
				curve.constantMax *= scale;
				return;
			default:
				return;
			}
		}

		public void RevertScale()
		{
			if (!this.shouldRevert)
			{
				return;
			}
			ParticleSystem.MainModule main = this.system.main;
			main.gravityModifierMultiplier = this.gravityMod;
			main.startSpeed = this.speedCurveCache;
			if (main.startSize3D)
			{
				main.startSizeX = this.sizeCurveXCache;
				main.startSizeY = this.sizeCurveYCache;
				main.startSizeZ = this.sizeCurveZCache;
			}
			else
			{
				main.startSize = this.sizeCurveCache;
			}
			if (this.scaleShape)
			{
				this.system.shape.scale = this.shapeScale;
			}
			if (this.scaleVelocityLifetime)
			{
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = this.system.velocityOverLifetime;
				velocityOverLifetime.x = this.lifetimeVelocityX;
				velocityOverLifetime.y = this.lifetimeVelocityY;
				velocityOverLifetime.z = this.lifetimeVelocityZ;
			}
			if (this.scaleVelocityLimitLifetime)
			{
				this.system.limitVelocityOverLifetime.limitMultiplier = this.limitMultiplier;
			}
			if (this.scaleForceOverLife)
			{
				ParticleSystem.ForceOverLifetimeModule forceOverLifetime = this.system.forceOverLifetime;
				forceOverLifetime.x = this.forceX;
				forceOverLifetime.y = this.forceY;
				forceOverLifetime.z = this.forceZ;
			}
			this.scale = 1f;
			this.shouldRevert = false;
		}

		private float scale = 1f;

		[Tooltip("Scale particles on enable using lossy scale")]
		[SerializeField]
		private bool useLossyScale;

		[Tooltip("Play particles after scaling")]
		[SerializeField]
		private bool autoPlay;

		[SerializeField]
		private ParticleSystem system;

		[SerializeField]
		private bool scaleShape;

		[SerializeField]
		private bool scaleVelocityLifetime;

		[SerializeField]
		private bool scaleVelocityLimitLifetime;

		[SerializeField]
		private bool scaleForceOverLife;

		private float gravityMod = 1f;

		private ParticleSystem.MinMaxCurve speedCurveCache;

		private ParticleSystem.MinMaxCurve sizeCurveCache;

		private ParticleSystem.MinMaxCurve sizeCurveXCache;

		private ParticleSystem.MinMaxCurve sizeCurveYCache;

		private ParticleSystem.MinMaxCurve sizeCurveZCache;

		private ParticleSystem.MinMaxCurve forceX;

		private ParticleSystem.MinMaxCurve forceY;

		private ParticleSystem.MinMaxCurve forceZ;

		private Vector3 shapeScale = Vector3.one;

		private ParticleSystem.MinMaxCurve lifetimeVelocityX;

		private ParticleSystem.MinMaxCurve lifetimeVelocityY;

		private ParticleSystem.MinMaxCurve lifetimeVelocityZ;

		private float limitMultiplier = 1f;

		private bool shouldRevert;

		private bool setScaleNextFrame;

		private int enableFrame;
	}
}
