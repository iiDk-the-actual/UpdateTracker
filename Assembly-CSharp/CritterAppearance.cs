using System;
using GorillaExtensions;
using Photon.Pun;

public struct CritterAppearance
{
	public CritterAppearance(string hatName, float size = 1f)
	{
		this.hatName = hatName;
		this.size = size;
	}

	public object[] WriteToRPCData()
	{
		object[] array = new object[] { this.hatName, this.size };
		if (this.hatName == null)
		{
			array[0] = string.Empty;
		}
		if (this.size != 0f)
		{
			array[1] = this.size;
		}
		return array;
	}

	public static int DataLength()
	{
		return 2;
	}

	public static bool ValidateData(object[] data)
	{
		float num;
		return data != null && data.Length == CritterAppearance.DataLength() && CrittersManager.ValidateDataType<float>(data[1], out num) && num >= 0f && !float.IsNaN(num) && !float.IsInfinity(num);
	}

	public static CritterAppearance ReadFromRPCData(object[] data)
	{
		string text;
		if (!CrittersManager.ValidateDataType<string>(data[0], out text))
		{
			return new CritterAppearance(string.Empty, 1f);
		}
		float num;
		if (!CrittersManager.ValidateDataType<float>(data[1], out num))
		{
			return new CritterAppearance(string.Empty, 1f);
		}
		return new CritterAppearance((string)data[0], num.GetFinite());
	}

	public static CritterAppearance ReadFromPhotonStream(PhotonStream data)
	{
		string text = (string)data.ReceiveNext();
		float num = (float)data.ReceiveNext();
		return new CritterAppearance(text, num);
	}

	public override string ToString()
	{
		return string.Format("Size: {0} Hat: {1}", this.size, this.hatName);
	}

	public float size;

	public string hatName;
}
