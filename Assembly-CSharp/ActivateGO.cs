using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ActivateGO : MonoBehaviour
{
	private void OnEnable()
	{
		this.active = PlayerPrefFlags.Check(this.flag);
		this.SetGOsActive(0);
		PlayerPrefFlags.OnFlagChange = (Action<PlayerPrefFlags.Flag, bool>)Delegate.Combine(PlayerPrefFlags.OnFlagChange, new Action<PlayerPrefFlags.Flag, bool>(this.OnFlagChange));
	}

	private void OnDisable()
	{
		PlayerPrefFlags.OnFlagChange = (Action<PlayerPrefFlags.Flag, bool>)Delegate.Remove(PlayerPrefFlags.OnFlagChange, new Action<PlayerPrefFlags.Flag, bool>(this.OnFlagChange));
	}

	private void OnDestroy()
	{
		PlayerPrefFlags.OnFlagChange = (Action<PlayerPrefFlags.Flag, bool>)Delegate.Remove(PlayerPrefFlags.OnFlagChange, new Action<PlayerPrefFlags.Flag, bool>(this.OnFlagChange));
	}

	public void OnFlagChange(PlayerPrefFlags.Flag f, bool value)
	{
		if (f != this.flag)
		{
			return;
		}
		this.active = value;
		this.SetGOsActive(this.flashes);
	}

	private async void SetGOsActive(int fls)
	{
		if (!this.flashing)
		{
			List<Renderer> renderers = new List<Renderer>();
			renderers.AddRange(this.targetGO.GetComponentsInChildren<MeshRenderer>(true));
			renderers.AddRange(this.targetGO.GetComponentsInChildren<SkinnedMeshRenderer>(true));
			for (int i = 0; i < fls; i++)
			{
				this.flashing = true;
				this.toggle(renderers, this.active);
				await Task.Delay(150);
				this.toggle(renderers, !this.active);
				await Task.Delay(100);
			}
			this.toggle(renderers, this.active);
			this.flashing = false;
		}
	}

	private void toggle(List<Renderer> renderers, bool state)
	{
		for (int i = 0; i < renderers.Count; i++)
		{
			if ((this.layerMask.value & (1 << renderers[i].gameObject.layer)) != 0)
			{
				renderers[i].forceRenderingOff = !state;
			}
		}
	}

	[SerializeField]
	private GameObject targetGO;

	[SerializeField]
	private PlayerPrefFlags.Flag flag;

	[SerializeField]
	private int flashes;

	[SerializeField]
	private LayerMask layerMask;

	private bool active;

	private bool flashing;
}
