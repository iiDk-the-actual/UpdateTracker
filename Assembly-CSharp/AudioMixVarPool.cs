using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New AudioMixVarPool", menuName = "ScriptableObjects/AudioMixVarPool", order = 0)]
public class AudioMixVarPool : ScriptableObject
{
	public bool Rent(out AudioMixVar mixVar)
	{
		for (int i = 0; i < this._vars.Length; i++)
		{
			if (!this._vars[i].taken)
			{
				this._vars[i].taken = true;
				mixVar = this._vars[i];
				return true;
			}
		}
		mixVar = null;
		return false;
	}

	public void Return(AudioMixVar mixVar)
	{
		if (mixVar == null)
		{
			return;
		}
		int num = this._vars.IndexOfRef(mixVar);
		if (num == -1)
		{
			return;
		}
		this._vars[num].taken = false;
	}

	[SerializeField]
	private AudioMixVar[] _vars = new AudioMixVar[0];
}
