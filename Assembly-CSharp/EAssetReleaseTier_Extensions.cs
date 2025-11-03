using System;

public static class EAssetReleaseTier_Extensions
{
	public static bool ShouldIncludeInBuild(this EAssetReleaseTier assetTier, EBuildReleaseTier buildTier)
	{
		return assetTier != EAssetReleaseTier.Disabled && assetTier <= (EAssetReleaseTier)buildTier;
	}
}
