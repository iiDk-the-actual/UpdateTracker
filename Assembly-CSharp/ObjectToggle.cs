using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectToggle : MonoBehaviour
{
	public void Toggle(bool initialState = true)
	{
		if (this._toggled == null)
		{
			if (initialState)
			{
				this.Enable();
				return;
			}
			this.Disable();
			return;
		}
		else
		{
			if (this._toggled.Value)
			{
				this.Disable();
				return;
			}
			this.Enable();
			return;
		}
	}

	public void Enable()
	{
		if (this.objectsToToggle == null)
		{
			return;
		}
		for (int i = 0; i < this.objectsToToggle.Count; i++)
		{
			GameObject gameObject = this.objectsToToggle[i];
			if (!(gameObject == null))
			{
				if (this._ignoreHierarchyState)
				{
					gameObject.SetActive(true);
				}
				else if (!gameObject.activeInHierarchy)
				{
					gameObject.SetActive(true);
				}
			}
		}
		this._toggled = new bool?(true);
	}

	public void Disable()
	{
		if (this.objectsToToggle == null)
		{
			return;
		}
		for (int i = 0; i < this.objectsToToggle.Count; i++)
		{
			GameObject gameObject = this.objectsToToggle[i];
			if (!(gameObject == null))
			{
				if (this._ignoreHierarchyState)
				{
					gameObject.SetActive(false);
				}
				else if (gameObject.activeInHierarchy)
				{
					gameObject.SetActive(false);
				}
			}
		}
		this._toggled = new bool?(false);
	}

	public List<GameObject> objectsToToggle = new List<GameObject>();

	[SerializeField]
	private bool _ignoreHierarchyState;

	[NonSerialized]
	private bool? _toggled;
}
