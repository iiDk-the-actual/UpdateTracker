using System;

internal class PropHuntPools_Callbacks
{
	internal void ListenForZoneChanged()
	{
		if (PropHuntPools_Callbacks._isListeningForZoneChanged)
		{
			return;
		}
		ZoneManagement.OnZoneChange += this._OnZoneChanged;
	}

	private void _OnZoneChanged(ZoneData[] zoneDatas)
	{
		if (VRRigCache.Instance == null || VRRigCache.Instance.localRig == null || VRRigCache.Instance.localRig.Rig == null || VRRigCache.Instance.localRig.Rig.zoneEntity.currentZone != GTZone.bayou)
		{
			return;
		}
		PropHuntPools_Callbacks._isListeningForZoneChanged = false;
		ZoneManagement.OnZoneChange -= this._OnZoneChanged;
		PropHuntPools.OnLocalPlayerEnteredBayou();
	}

	private const string preLog = "PropHuntPools_Callbacks: ";

	private const string preLogEd = "(editor only log) PropHuntPools_Callbacks: ";

	private const string preLogBeta = "(beta only log) PropHuntPools_Callbacks: ";

	private const string preErr = "ERROR!!!  PropHuntPools_Callbacks: ";

	private const string preErrEd = "ERROR!!!  (editor only log) PropHuntPools_Callbacks: ";

	private const string preErrBeta = "ERROR!!!  (beta only log) PropHuntPools_Callbacks: ";

	internal static readonly PropHuntPools_Callbacks instance = new PropHuntPools_Callbacks();

	private static bool _isListeningForZoneChanged;
}
