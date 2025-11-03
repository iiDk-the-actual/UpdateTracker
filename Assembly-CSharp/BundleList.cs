using System;

internal class BundleList
{
	public void FromJson(string jsonString)
	{
		this.data = JSonHelper.FromJson<BundleData>(jsonString);
		if (this.data.Length == 0)
		{
			return;
		}
		this.activeBundleIdx = 0;
		int num = this.data[0].majorVersion;
		int num2 = this.data[0].minorVersion;
		int num3 = this.data[0].minorVersion2;
		int gameMajorVersion = NetworkSystemConfig.GameMajorVersion;
		int gameMinorVersion = NetworkSystemConfig.GameMinorVersion;
		int gameMinorVersion2 = NetworkSystemConfig.GameMinorVersion2;
		for (int i = 1; i < this.data.Length; i++)
		{
			this.data[i].isActive = false;
			int num4 = gameMajorVersion * 1000000 + gameMinorVersion * 1000 + gameMinorVersion2;
			int num5 = this.data[i].majorVersion * 1000000 + this.data[i].minorVersion * 1000 + this.data[i].minorVersion2;
			if (num4 >= num5 && this.data[i].majorVersion >= num && this.data[i].minorVersion >= num2 && this.data[i].minorVersion2 >= num3)
			{
				this.activeBundleIdx = i;
				num = this.data[i].majorVersion;
				num2 = this.data[i].minorVersion;
				num3 = this.data[i].minorVersion2;
				break;
			}
		}
		this.data[this.activeBundleIdx].isActive = true;
	}

	public bool HasSku(string skuName, out int idx)
	{
		if (this.data == null)
		{
			idx = -1;
			return false;
		}
		for (int i = 0; i < this.data.Length; i++)
		{
			if (this.data[i].skuName == skuName)
			{
				idx = i;
				return true;
			}
		}
		idx = -1;
		return false;
	}

	public BundleData ActiveBundle()
	{
		return this.data[this.activeBundleIdx];
	}

	private int activeBundleIdx;

	public BundleData[] data;
}
