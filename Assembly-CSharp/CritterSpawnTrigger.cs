using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class CritterSpawnTrigger : MonoBehaviour
{
	private ValueDropdownList<int> GetCritterTypeList()
	{
		return new ValueDropdownList<int>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!CrittersManager.instance.LocalAuthority())
		{
			return;
		}
		if (Time.realtimeSinceStartup < this._nextSpawnTime)
		{
			return;
		}
		CrittersActor componentInParent = other.GetComponentInParent<CrittersActor>();
		if (!componentInParent)
		{
			return;
		}
		if (componentInParent.crittersActorType != this.triggerActorType)
		{
			return;
		}
		if (this.requiredSubObjectIndex >= 0 && componentInParent.subObjectIndex != this.requiredSubObjectIndex)
		{
			return;
		}
		if (!string.IsNullOrEmpty(this.triggerActorName) && !componentInParent.GetActorSubtype().Contains(this.triggerActorName))
		{
			return;
		}
		CrittersManager.instance.DespawnActor(componentInParent);
		CrittersManager.instance.SpawnCritter(this.critterType, this.spawnPoint.position, this.spawnPoint.rotation);
		this._nextSpawnTime = Time.realtimeSinceStartup + this.triggerCooldown;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine(base.transform.position, this.spawnPoint.position);
		Gizmos.DrawWireSphere(this.spawnPoint.position, 0.1f);
	}

	[Header("Trigger Settings")]
	[SerializeField]
	private CrittersActor.CrittersActorType triggerActorType;

	[SerializeField]
	private int requiredSubObjectIndex = -1;

	[SerializeField]
	private string triggerActorName;

	[SerializeField]
	private float triggerCooldown = 1f;

	[Header("Spawn Settings")]
	[SerializeField]
	private Transform spawnPoint;

	[SerializeField]
	private int critterType;

	private float _nextSpawnTime;
}
