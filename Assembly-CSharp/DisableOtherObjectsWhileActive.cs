using System;
using UnityEngine;

public class DisableOtherObjectsWhileActive : MonoBehaviour
{
	private void OnEnable()
	{
		this.SetAllActive(false);
	}

	private void OnDisable()
	{
		this.SetAllActive(true);
	}

	private void SetAllActive(bool active)
	{
		foreach (GameObject gameObject in this.otherObjects)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(active);
			}
		}
		foreach (XSceneRef xsceneRef in this.otherXSceneObjects)
		{
			GameObject gameObject2;
			if (xsceneRef.TryResolve(out gameObject2))
			{
				gameObject2.SetActive(active);
			}
		}
	}

	public GameObject[] otherObjects;

	public XSceneRef[] otherXSceneObjects;
}
