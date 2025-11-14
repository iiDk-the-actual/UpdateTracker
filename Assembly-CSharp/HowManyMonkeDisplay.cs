using System;
using TMPro;
using UnityEngine;

public class HowManyMonkeDisplay : MonoBehaviour, IGorillaSliceableSimple
{
	public void OnEnable()
	{
		this.currValue = (this.nextValue = HowManyMonke.ThisMany);
		this.text.text = this.currValue.ToString("N0");
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		HowManyMonke.OnCheck = (Action<int>)Delegate.Remove(HowManyMonke.OnCheck, new Action<int>(this.HowManyMonke_OnCheck));
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	private void OnDestroy()
	{
		HowManyMonke.OnCheck = (Action<int>)Delegate.Remove(HowManyMonke.OnCheck, new Action<int>(this.HowManyMonke_OnCheck));
	}

	private void HowManyMonke_OnCheck(int thisMany)
	{
		this.currValue = this.nextValue;
		this.nextValue = thisMany;
		this.checkTime = Time.time;
	}

	public void SliceUpdate()
	{
		float num = Mathf.Lerp((float)this.currValue, (float)this.nextValue, (Time.time - this.checkTime) / HowManyMonke.RecheckDelay);
		this.text.text = num.ToString("N0");
		this.particleSystem.emission.rateOverTime = this.particleSystemRateToCount.Evaluate(num);
		float sqrMagnitude = (VRRig.LocalRig.transform.position - base.transform.position).sqrMagnitude;
		if (this.observable && sqrMagnitude > this.observableDistance)
		{
			this.observable = false;
			HowManyMonke.OnCheck = (Action<int>)Delegate.Remove(HowManyMonke.OnCheck, new Action<int>(this.HowManyMonke_OnCheck));
			if (this.observableActive)
			{
				this.observableActive.SetActive(this.observable);
				return;
			}
		}
		else if (!this.observable && sqrMagnitude < this.observableDistance)
		{
			this.observable = true;
			HowManyMonke.OnCheck = (Action<int>)Delegate.Combine(HowManyMonke.OnCheck, new Action<int>(this.HowManyMonke_OnCheck));
			if (this.observableActive)
			{
				this.observableActive.SetActive(this.observable);
			}
		}
	}

	[SerializeField]
	private TMP_Text text;

	[SerializeField]
	private float observableDistance = 100f;

	[SerializeField]
	private GameObject observableActive;

	[SerializeField]
	private ParticleSystem particleSystem;

	[SerializeField]
	private AnimationCurve particleSystemRateToCount;

	private bool observable;

	private int currValue;

	private int nextValue;

	private float checkTime;
}
