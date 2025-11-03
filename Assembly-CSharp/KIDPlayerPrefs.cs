using System;

public class KIDPlayerPrefs
{
	public const string SESSION_ID_PREFIX_PLAYER_PREF = "kIDSessionID-";

	public const string SESSION_ETAG_PLAYER_PREF = "kIDSessionETAG-";

	public const string SESSION_CHANGED_PLAYER_PREF = "kIDSessionUpdated-";

	private const string KID_PERMISSIONS_CSV = "kid-permission-csv";

	private const string KID_DEFAULT_PERMISSIONS_CSV = "kid-default-permission-csv";

	private const string KID_PERMISSIONS_ENABLED_KEY = "-enabled";

	private const string KID_PERMISSIONS_MANAGED_BY_KEY = "-managed-by";

	private const string KID_EMAIL_KEY = "k-id_EmailAddress";
}
