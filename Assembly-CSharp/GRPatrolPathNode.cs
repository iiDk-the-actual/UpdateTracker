using System;
using UnityEngine;

public class GRPatrolPathNode : MonoBehaviour
{
	public void OnDrawGizmosSelected()
	{
		if (base.transform.parent == null)
		{
			return;
		}
		GRPatrolPath component = base.transform.parent.GetComponent<GRPatrolPath>();
		if (component == null)
		{
			return;
		}
		component.OnDrawGizmosSelected();
	}
}
