using System;
using System.Collections.Generic;
using UnityEngine;

public class GREnemy : MonoBehaviour, IGameEntityComponent
{
	public void OnEntityInit()
	{
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	public static void HideRenderers(List<Renderer> renderers, bool hide)
	{
		if (renderers == null)
		{
			return;
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].enabled = !hide;
			}
		}
	}

	public static void HideObjects(List<GameObject> objects, bool hide)
	{
		if (objects == null)
		{
			return;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i] != null)
			{
				objects[i].SetActive(!hide);
			}
		}
	}
}
