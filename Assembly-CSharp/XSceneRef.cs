using System;
using UnityEngine;

[Serializable]
public struct XSceneRef
{
	public bool TryResolve(out XSceneRefTarget result)
	{
		if (this.TargetID == 0)
		{
			result = null;
			return true;
		}
		if (this.didCache && this.cached != null)
		{
			result = this.cached;
			return true;
		}
		XSceneRefTarget xsceneRefTarget;
		if (!XSceneRefGlobalHub.TryResolve(this.TargetScene, this.TargetID, out xsceneRefTarget))
		{
			result = null;
			return false;
		}
		this.cached = xsceneRefTarget;
		this.didCache = true;
		result = xsceneRefTarget;
		return true;
	}

	public bool TryResolve(out GameObject result)
	{
		XSceneRefTarget xsceneRefTarget;
		if (this.TryResolve(out xsceneRefTarget))
		{
			result = ((xsceneRefTarget == null) ? null : xsceneRefTarget.gameObject);
			return true;
		}
		result = null;
		return false;
	}

	public bool TryResolve<T>(out T result) where T : Component
	{
		XSceneRefTarget xsceneRefTarget;
		if (this.TryResolve(out xsceneRefTarget))
		{
			result = ((xsceneRefTarget == null) ? default(T) : xsceneRefTarget.GetComponent<T>());
			return true;
		}
		result = default(T);
		return false;
	}

	public void AddCallbackOnLoad(Action callback)
	{
		this.TargetScene.AddCallbackOnSceneLoad(callback);
	}

	public void RemoveCallbackOnLoad(Action callback)
	{
		this.TargetScene.RemoveCallbackOnSceneLoad(callback);
	}

	public void AddCallbackOnUnload(Action callback)
	{
		this.TargetScene.AddCallbackOnSceneUnload(callback);
	}

	public void RemoveCallbackOnUnload(Action callback)
	{
		this.TargetScene.RemoveCallbackOnSceneUnload(callback);
	}

	public SceneIndex TargetScene;

	public int TargetID;

	private XSceneRefTarget cached;

	private bool didCache;
}
