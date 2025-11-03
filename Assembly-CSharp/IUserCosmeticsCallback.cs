using System;

internal interface IUserCosmeticsCallback
{
	bool OnGetUserCosmetics(string cosmetics);

	bool PendingUpdate { get; set; }
}
