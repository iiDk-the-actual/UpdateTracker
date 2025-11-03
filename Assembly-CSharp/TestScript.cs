using System;
using GorillaTag;
using UnityEngine;

[GTStripGameObjectFromBuild("!QATESTING")]
public class TestScript : MonoBehaviour
{
	public int callbackOrder
	{
		get
		{
			return 0;
		}
	}

	public GameObject testDelete;
}
