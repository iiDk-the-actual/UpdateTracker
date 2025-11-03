using System;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;

public class SizeManager : MonoBehaviour
{
	public float currentScale
	{
		get
		{
			if (this.targetRig != null)
			{
				return this.targetRig.ScaleMultiplier;
			}
			if (this.targetPlayer != null)
			{
				return this.targetPlayer.ScaleMultiplier;
			}
			return 1f;
		}
	}

	public int currentSizeLayerMaskValue
	{
		get
		{
			if (this.targetPlayer)
			{
				return this.targetPlayer.sizeLayerMask;
			}
			if (this.targetRig)
			{
				return this.targetRig.SizeLayerMask;
			}
			return 1;
		}
		set
		{
			if (this.targetPlayer)
			{
				this.targetPlayer.sizeLayerMask = value;
				if (this.targetRig != null)
				{
					this.targetRig.SizeLayerMask = value;
					return;
				}
			}
			else if (this.targetRig)
			{
				this.targetRig.SizeLayerMask = value;
			}
		}
	}

	private void OnDisable()
	{
		this.touchingChangers.Clear();
		this.currentSizeLayerMaskValue = 1;
		SizeManagerManager.UnregisterSM(this);
	}

	private void OnEnable()
	{
		SizeManagerManager.RegisterSM(this);
	}

	private void CollectLineRenderers(GameObject obj)
	{
		this.lineRenderers = obj.GetComponentsInChildren<LineRenderer>(true);
		int num = this.lineRenderers.Length;
		foreach (LineRenderer lineRenderer in this.lineRenderers)
		{
			this.initLineScalar.Add(lineRenderer.widthMultiplier);
		}
	}

	public void BuildInitialize()
	{
		this.rate = 650f;
		if (this.targetRig != null)
		{
			this.CollectLineRenderers(this.targetRig.gameObject);
		}
		else if (this.targetPlayer != null)
		{
			this.CollectLineRenderers(GorillaTagger.Instance.offlineVRRig.gameObject);
		}
		this.mainCameraTransform = Camera.main.transform;
		if (this.targetPlayer != null)
		{
			this.myType = SizeManager.SizeChangerType.LocalOffline;
		}
		else if (this.targetRig != null && !this.targetRig.isOfflineVRRig && this.targetRig.netView != null && this.targetRig.netView.Owner != NetworkSystem.Instance.LocalPlayer)
		{
			this.myType = SizeManager.SizeChangerType.OtherOnline;
		}
		else
		{
			this.myType = SizeManager.SizeChangerType.LocalOnline;
		}
		this.buildInitialized = true;
	}

	private void Awake()
	{
		if (!this.buildInitialized)
		{
			this.BuildInitialize();
		}
		SizeManagerManager.RegisterSM(this);
	}

	public void InvokeFixedUpdate()
	{
		float num = 1f;
		SizeChanger sizeChanger = this.ControllingChanger(this.targetRig.transform);
		switch (this.myType)
		{
		case SizeManager.SizeChangerType.LocalOffline:
			num = this.ScaleFromChanger(sizeChanger, this.mainCameraTransform, Time.fixedDeltaTime);
			this.targetPlayer.SetScaleMultiplier((num == 1f) ? this.SizeOverTime(num, 0.33f, Time.fixedDeltaTime) : num);
			break;
		case SizeManager.SizeChangerType.LocalOnline:
			num = this.ScaleFromChanger(sizeChanger, this.targetRig.transform, Time.fixedDeltaTime);
			this.targetRig.ScaleMultiplier = ((num == 1f) ? this.SizeOverTime(num, 0.33f, Time.fixedDeltaTime) : num);
			break;
		case SizeManager.SizeChangerType.OtherOnline:
			num = this.ScaleFromChanger(sizeChanger, this.targetRig.transform, Time.fixedDeltaTime);
			this.targetRig.ScaleMultiplier = ((num == 1f) ? this.SizeOverTime(num, 0.33f, Time.fixedDeltaTime) : num);
			break;
		}
		if (num != this.lastScale)
		{
			for (int i = 0; i < this.lineRenderers.Length; i++)
			{
				this.lineRenderers[i].widthMultiplier = num * this.initLineScalar[i];
			}
			Vector3 vector;
			if (sizeChanger != null && sizeChanger.TryGetScaleCenterPoint(out vector))
			{
				if (this.myType == SizeManager.SizeChangerType.LocalOffline)
				{
					this.targetPlayer.ScaleAwayFromPoint(this.lastScale, num, vector);
				}
				else if (this.myType == SizeManager.SizeChangerType.LocalOnline)
				{
					GTPlayer.Instance.ScaleAwayFromPoint(this.lastScale, num, vector);
				}
			}
			if (this.myType == SizeManager.SizeChangerType.LocalOffline)
			{
				this.CheckSizeChangeEvents(num);
			}
		}
		this.lastScale = num;
	}

	private SizeChanger ControllingChanger(Transform t)
	{
		for (int i = this.touchingChangers.Count - 1; i >= 0; i--)
		{
			SizeChanger sizeChanger = this.touchingChangers[i];
			if (!(sizeChanger == null) && sizeChanger.gameObject.activeInHierarchy && (sizeChanger.SizeLayerMask & this.currentSizeLayerMaskValue) != 0 && (sizeChanger.alwaysControlWhenEntered || (sizeChanger.ClosestPoint(t.position) - t.position).magnitude < this.magnitudeThreshold))
			{
				return sizeChanger;
			}
		}
		return null;
	}

	private float ScaleFromChanger(SizeChanger sC, Transform t, float deltaTime)
	{
		if (sC == null)
		{
			return 1f;
		}
		SizeChanger.ChangerType changerType = sC.MyType;
		if (changerType == SizeChanger.ChangerType.Static)
		{
			return this.SizeOverTime(sC.MinScale, sC.StaticEasing, deltaTime);
		}
		if (changerType == SizeChanger.ChangerType.Continuous)
		{
			Vector3 vector = Vector3.Project(t.position - sC.StartPos.position, sC.EndPos.position - sC.StartPos.position);
			return Mathf.Clamp(sC.MaxScale - vector.magnitude / (sC.StartPos.position - sC.EndPos.position).magnitude * (sC.MaxScale - sC.MinScale), sC.MinScale, sC.MaxScale);
		}
		return 1f;
	}

	private float SizeOverTime(float targetSize, float easing, float deltaTime)
	{
		if (easing <= 0f || Mathf.Abs(this.targetRig.ScaleMultiplier - targetSize) < 0.05f)
		{
			return targetSize;
		}
		return Mathf.MoveTowards(this.targetRig.ScaleMultiplier, targetSize, deltaTime / easing);
	}

	private void CheckSizeChangeEvents(float newSize)
	{
		if (newSize < this.smallThreshold)
		{
			if (!this.isSmall)
			{
				this.isSmall = true;
				this.isLarge = false;
				PlayerGameEvents.MiscEvent("SizeSmall", 1);
				return;
			}
		}
		else if (newSize > this.largeThreshold)
		{
			if (!this.isLarge)
			{
				this.isLarge = true;
				this.isSmall = false;
				PlayerGameEvents.MiscEvent("SizeLarge", 1);
				return;
			}
		}
		else
		{
			this.isLarge = false;
			this.isSmall = false;
		}
	}

	public List<SizeChanger> touchingChangers;

	private LineRenderer[] lineRenderers;

	private List<float> initLineScalar = new List<float>();

	public VRRig targetRig;

	public GTPlayer targetPlayer;

	public float magnitudeThreshold = 0.01f;

	public float rate = 650f;

	public Transform mainCameraTransform;

	public SizeManager.SizeChangerType myType;

	public float lastScale;

	private bool buildInitialized;

	private const float returnToNormalEasing = 0.33f;

	private float smallThreshold = 0.6f;

	private float largeThreshold = 1.5f;

	private bool isSmall;

	private bool isLarge;

	public enum SizeChangerType
	{
		LocalOffline,
		LocalOnline,
		OtherOnline
	}
}
