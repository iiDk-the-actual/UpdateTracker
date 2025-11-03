using System;
using CustomMapSupport;
using GorillaExtensions;
using GorillaLocomotion.Climbing;
using GorillaLocomotion.Gameplay;
using GT_CustomMapSupportRuntime;
using UnityEngine;

public class CustomMapsGorillaZipline : GorillaZipline
{
	public bool GenerateZipline(global::CustomMapSupport.BezierSpline splineRef)
	{
		this.spline = base.GetComponent<global::BezierSpline>();
		if (this.spline.IsNull())
		{
			return false;
		}
		this.spline.BuildSplineFromPoints(splineRef.GetControlPoints(), this.ConvertControlPointModes(splineRef.GetControlPointModes()), splineRef.Loop);
		if (this.segmentsRoot == null)
		{
			return false;
		}
		this.ziplineDistance = 0f;
		float num = 0f;
		int num2 = 0;
		Transform transform = null;
		while (num < 1f)
		{
			float num3 = this.segmentDistance;
			if (num2 == 0)
			{
				num3 /= 2f;
			}
			base.FindTFromDistance(ref num, num3, 5000);
			if (num < 1f || this.spline.Loop)
			{
				Vector3 point = this.spline.GetPoint(num);
				GameObject gameObject = Object.Instantiate<GameObject>(this.segmentPrefab);
				gameObject.transform.SetParent(this.segmentsRoot);
				gameObject.transform.position = point;
				gameObject.transform.LookAt(point + this.spline.GetDirection(num));
				gameObject.transform.position -= gameObject.transform.forward * 0.5f;
				if (num2 > 0)
				{
					transform.LookAt(gameObject.transform);
				}
				gameObject.GetComponent<GorillaClimbableRef>().climb = this.slideHelper;
				this.ziplineDistance += this.segmentDistance;
				transform = gameObject.transform;
			}
			num2++;
		}
		return true;
	}

	private global::BezierControlPointMode[] ConvertControlPointModes(global::CustomMapSupport.BezierControlPointMode[] refModes)
	{
		global::BezierControlPointMode[] array = new global::BezierControlPointMode[refModes.Length];
		for (int i = 0; i < refModes.Length; i++)
		{
			switch (refModes[i])
			{
			case global::CustomMapSupport.BezierControlPointMode.Free:
				array[i] = global::BezierControlPointMode.Free;
				break;
			case global::CustomMapSupport.BezierControlPointMode.Aligned:
				array[i] = global::BezierControlPointMode.Aligned;
				break;
			case global::CustomMapSupport.BezierControlPointMode.Mirrored:
				array[i] = global::BezierControlPointMode.Mirrored;
				break;
			}
		}
		return array;
	}

	protected override void Start()
	{
		GorillaClimbable slideHelper = this.slideHelper;
		slideHelper.onBeforeClimb = (Action<GorillaHandClimber, GorillaClimbableRef>)Delegate.Combine(slideHelper.onBeforeClimb, new Action<GorillaHandClimber, GorillaClimbableRef>(base.OnBeforeClimb));
	}

	public void Init(GTObjectPlaceholder ziplinePlaceholder)
	{
		if (ziplinePlaceholder.PlaceholderObject != GTObject.ZipLine)
		{
			return;
		}
		this.segmentDistance = ziplinePlaceholder.ziplineSegmentGenerationOffset;
		this.spline = base.gameObject.GetComponent<global::BezierSpline>();
		if (this.spline == null)
		{
			this.spline = base.gameObject.AddComponent<global::BezierSpline>();
		}
		this.spline.BuildSplineFromPoints(ziplinePlaceholder.spline.GetControlPoints(), this.ConvertControlPointModes(ziplinePlaceholder.spline.GetControlPointModes()), ziplinePlaceholder.spline.Loop);
		for (int i = 0; i < ziplinePlaceholder.ziplineSegments.Count; i++)
		{
			ziplinePlaceholder.ziplineSegments[i].transform.SetParent(this.segmentsRoot, true);
			ziplinePlaceholder.ziplineSegments[i].AddComponent<GorillaClimbableRef>().climb = this.slideHelper;
			this.ziplineDistance += this.segmentDistance;
		}
	}
}
