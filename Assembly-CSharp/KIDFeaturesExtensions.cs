using System;

public static class KIDFeaturesExtensions
{
	public static string ToStandardisedString(this EKIDFeatures feature)
	{
		switch (feature)
		{
		case EKIDFeatures.Multiplayer:
			return "multiplayer";
		case EKIDFeatures.Custom_Nametags:
			return "custom-username";
		case EKIDFeatures.Voice_Chat:
			return "voice-chat";
		case EKIDFeatures.Mods:
			return "mods";
		case EKIDFeatures.Groups:
			return "join-groups";
		default:
			return feature.ToString();
		}
	}

	public static EKIDFeatures? FromString(string name)
	{
		string text = name.ToLower();
		if (text == "voice-chat")
		{
			return new EKIDFeatures?(EKIDFeatures.Voice_Chat);
		}
		if (text == "custom-username")
		{
			return new EKIDFeatures?(EKIDFeatures.Custom_Nametags);
		}
		if (text == "multiplayer")
		{
			return new EKIDFeatures?(EKIDFeatures.Multiplayer);
		}
		if (text == "mods")
		{
			return new EKIDFeatures?(EKIDFeatures.Mods);
		}
		if (!(text == "join-groups"))
		{
			return null;
		}
		return new EKIDFeatures?(EKIDFeatures.Groups);
	}

	public static bool TryGetFromString(string name, out EKIDFeatures result)
	{
		EKIDFeatures? ekidfeatures = KIDFeaturesExtensions.FromString(name);
		if (ekidfeatures != null)
		{
			result = ekidfeatures.Value;
			return true;
		}
		result = EKIDFeatures.Voice_Chat;
		return false;
	}
}
