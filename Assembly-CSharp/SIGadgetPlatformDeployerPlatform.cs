using System;
using UnityEngine;

public class SIGadgetPlatformDeployerPlatform : MonoBehaviour, ISIGameDeployable
{
	public void ApplyUpgrades(SIUpgradeSet upgrades)
	{
		bool flag = upgrades.Contains(SIUpgradeType.Platform_Duration);
		float num = (flag ? this.extendedDuration : this.defaultDuration);
		this.timeToDie = Time.time + num;
		this.extendedDurationFrame.SetActive(flag);
		this.checkBounds = new Bounds(this.activeCollider.center, this.activeCollider.size);
		Vector3 size = this.checkBounds.size;
		Vector3 lossyScale = this.activeCollider.transform.lossyScale;
		size.x *= lossyScale.x;
		size.y *= lossyScale.y;
		size.z *= lossyScale.z;
		this.checkBounds.size = size;
		this.checkOffset = this.activeCollider.transform.position;
		this.checkRot = this.activeCollider.transform.rotation;
		this.CheckHeadOverlap();
	}

	public void CheckHeadOverlap()
	{
		if (this.activeCollider == null)
		{
			return;
		}
		Vector3 position = GorillaTagger.Instance.headCollider.transform.position;
		float num = GorillaTagger.Instance.headCollider.radius * GorillaTagger.Instance.headCollider.transform.lossyScale.x;
		Vector3 vector = Quaternion.Inverse(this.checkRot) * (position - this.checkOffset);
		if (Vector3.Magnitude(this.checkBounds.ClosestPoint(vector) - vector) < num)
		{
			this.isOverlappingHead = true;
			this.activeCollider.enabled = false;
			return;
		}
		this.isOverlappingHead = false;
		this.activeCollider.enabled = true;
	}

	private void LateUpdate()
	{
		if (Time.time > this.timeToDie)
		{
			ObjectPools.instance.Destroy(base.gameObject);
			return;
		}
		if (this.isOverlappingHead)
		{
			this.CheckHeadOverlap();
		}
	}

	private void OnDisable()
	{
		Action onDisabled = this.OnDisabled;
		if (onDisabled != null)
		{
			onDisabled();
		}
		this.OnDisabled = null;
	}

	[SerializeField]
	private GameObject extendedDurationFrame;

	[SerializeField]
	private float defaultDuration = 10f;

	[SerializeField]
	private float extendedDuration = 20f;

	[SerializeField]
	private BoxCollider activeCollider;

	private bool isOverlappingHead;

	private float timeToDie = -1f;

	private Bounds checkBounds;

	private Vector3 checkOffset;

	private Quaternion checkRot;

	public Action OnDisabled;
}
