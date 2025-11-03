using System;

[Serializable]
internal struct SIUpgradeBasedGeneric<T>
{
	public bool TryGetActiveValue(SIUpgradeSet withUpgrades, out T out_value)
	{
		out_value = default(T);
		bool flag = false;
		for (int i = 0; i < this.entries.Length; i++)
		{
			if (this.entries[i].IsActive(withUpgrades))
			{
				flag = true;
				out_value = this.entries[i].value;
			}
		}
		return flag;
	}

	public SIUpgradeBasedGenericEntry<T>[] entries;
}
