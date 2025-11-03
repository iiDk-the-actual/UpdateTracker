using System;
using UnityEngine;

public class Campfire : MonoBehaviour, IGorillaSliceableSimple
{
	private void Start()
	{
		this.lastAngleBottom = 0f;
		this.lastAngleMiddle = 0f;
		this.lastAngleTop = 0f;
		this.perlinBottom = (float)Random.Range(0, 100);
		this.perlinMiddle = (float)Random.Range(200, 300);
		this.perlinTop = (float)Random.Range(400, 500);
		this.startingRotationBottom = this.baseFire.localEulerAngles.x;
		this.startingRotationMiddle = this.middleFire.localEulerAngles.x;
		this.startingRotationTop = this.topFire.localEulerAngles.x;
		this.tempVec = new Vector3(0f, 0f, 0f);
		this.mergedBottom = false;
		this.mergedMiddle = false;
		this.mergedTop = false;
		this.wasActive = false;
		this.lastTime = Time.time;
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
		if (BetterDayNightManager.instance == null)
		{
			return;
		}
		if ((this.isActive[BetterDayNightManager.instance.currentTimeIndex] && BetterDayNightManager.instance.CurrentWeather() != BetterDayNightManager.WeatherType.Raining) || this.overrideDayNight == 1)
		{
			if (!this.wasActive)
			{
				this.wasActive = true;
				this.mergedBottom = false;
				this.mergedMiddle = false;
				this.mergedTop = false;
				Color.RGBToHSV(this.mat.color, out this.h, out this.s, out this.v);
				this.mat.color = Color.HSVToRGB(this.h, this.s, 1f);
			}
			this.Flap(ref this.perlinBottom, this.perlinStepBottom, ref this.lastAngleBottom, ref this.baseFire, this.bottomRange, this.baseMultiplier, ref this.mergedBottom);
			this.Flap(ref this.perlinMiddle, this.perlinStepMiddle, ref this.lastAngleMiddle, ref this.middleFire, this.middleRange, this.middleMultiplier, ref this.mergedMiddle);
			this.Flap(ref this.perlinTop, this.perlinStepTop, ref this.lastAngleTop, ref this.topFire, this.topRange, this.topMultiplier, ref this.mergedTop);
		}
		else
		{
			if (this.wasActive)
			{
				this.wasActive = false;
				this.mergedBottom = false;
				this.mergedMiddle = false;
				this.mergedTop = false;
				Color.RGBToHSV(this.mat.color, out this.h, out this.s, out this.v);
				this.mat.color = Color.HSVToRGB(this.h, this.s, 0.25f);
			}
			this.ReturnToOff(ref this.baseFire, this.startingRotationBottom, ref this.mergedBottom);
			this.ReturnToOff(ref this.middleFire, this.startingRotationMiddle, ref this.mergedMiddle);
			this.ReturnToOff(ref this.topFire, this.startingRotationTop, ref this.mergedTop);
		}
		this.lastTime = Time.time;
	}

	private void Flap(ref float perlinValue, float perlinStep, ref float lastAngle, ref Transform flameTransform, float range, float multiplier, ref bool isMerged)
	{
		perlinValue += perlinStep;
		lastAngle += (Time.time - this.lastTime) * Mathf.PerlinNoise(perlinValue, 0f);
		this.tempVec.x = range * Mathf.Sin(lastAngle * multiplier);
		if (Mathf.Abs(this.tempVec.x - flameTransform.localEulerAngles.x) > 180f)
		{
			if (this.tempVec.x > flameTransform.localEulerAngles.x)
			{
				this.tempVec.x = this.tempVec.x - 360f;
			}
			else
			{
				this.tempVec.x = this.tempVec.x + 360f;
			}
		}
		if (isMerged)
		{
			flameTransform.localEulerAngles = this.tempVec;
			return;
		}
		if (Mathf.Abs(flameTransform.localEulerAngles.x - this.tempVec.x) < 1f)
		{
			isMerged = true;
			flameTransform.localEulerAngles = this.tempVec;
			return;
		}
		this.tempVec.x = (this.tempVec.x - flameTransform.localEulerAngles.x) * this.slerp + flameTransform.localEulerAngles.x;
		flameTransform.localEulerAngles = this.tempVec;
	}

	private void ReturnToOff(ref Transform startTransform, float targetAngle, ref bool isMerged)
	{
		this.tempVec.x = targetAngle;
		if (Mathf.Abs(this.tempVec.x - startTransform.localEulerAngles.x) > 180f)
		{
			if (this.tempVec.x > startTransform.localEulerAngles.x)
			{
				this.tempVec.x = this.tempVec.x - 360f;
			}
			else
			{
				this.tempVec.x = this.tempVec.x + 360f;
			}
		}
		if (!isMerged)
		{
			if (Mathf.Abs(startTransform.localEulerAngles.x - targetAngle) < 1f)
			{
				isMerged = true;
				return;
			}
			this.tempVec.x = (this.tempVec.x - startTransform.localEulerAngles.x) * this.slerp + startTransform.localEulerAngles.x;
			startTransform.localEulerAngles = this.tempVec;
		}
	}

	public Transform baseFire;

	public Transform middleFire;

	public Transform topFire;

	public float baseMultiplier;

	public float middleMultiplier;

	public float topMultiplier;

	public float bottomRange;

	public float middleRange;

	public float topRange;

	private float lastAngleBottom;

	private float lastAngleMiddle;

	private float lastAngleTop;

	public float perlinStepBottom;

	public float perlinStepMiddle;

	public float perlinStepTop;

	private float perlinBottom;

	private float perlinMiddle;

	private float perlinTop;

	public float startingRotationBottom;

	public float startingRotationMiddle;

	public float startingRotationTop;

	public float slerp = 0.01f;

	private bool mergedBottom;

	private bool mergedMiddle;

	private bool mergedTop;

	public string lastTimeOfDay;

	public Material mat;

	private float h;

	private float s;

	private float v;

	public int overrideDayNight;

	private Vector3 tempVec;

	public bool[] isActive;

	public bool wasActive;

	private float lastTime;
}
