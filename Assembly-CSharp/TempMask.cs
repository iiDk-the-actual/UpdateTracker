using System;
using System.Collections;
using GorillaNetworking;
using UnityEngine;

public class TempMask : MonoBehaviour
{
	private void Awake()
	{
		this.dayOn = new DateTime(this.year, this.month, this.day);
		this.myRig = base.GetComponentInParent<VRRig>();
		if (this.myRig != null && this.myRig.netView.IsMine && !this.myRig.isOfflineVRRig)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.MaskOnDuringDate());
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
	}

	private IEnumerator MaskOnDuringDate()
	{
		for (;;)
		{
			if (GorillaComputer.instance != null && GorillaComputer.instance.startupMillis != 0L)
			{
				this.myDate = new DateTime(GorillaComputer.instance.startupMillis * 10000L + (long)(Time.realtimeSinceStartup * 1000f * 10000f)).Subtract(TimeSpan.FromHours(7.0));
				if (this.myDate.DayOfYear == this.dayOn.DayOfYear)
				{
					if (!this.myRenderer.enabled)
					{
						this.myRenderer.enabled = true;
					}
				}
				else if (this.myRenderer.enabled)
				{
					this.myRenderer.enabled = false;
				}
			}
			yield return new WaitForSeconds(1f);
		}
		yield break;
	}

	public int year;

	public int month;

	public int day;

	public DateTime dayOn;

	public MeshRenderer myRenderer;

	private DateTime myDate;

	private VRRig myRig;
}
