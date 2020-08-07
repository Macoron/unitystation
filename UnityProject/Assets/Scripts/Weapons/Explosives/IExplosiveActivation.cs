/// <summary>
/// Interface for different explosives activators
/// Timers, remote devices, voice recognition, etc.
/// </summary>
public interface IExplosiveActivation
{
	bool IsExplosiveArmed { get; }

	void ArmExplosive();
	void DisarmExplosive();

	/// <summary>
	/// Invokes client-side when explosive activator armed/disarmed
	/// </summary>
	event System.Action<bool> OnExplosiveArmChangedClient;

	/// <summary>
	/// Invokes server-side when explosive need to blown-up
	/// </summary>
	event System.Action OnExplosiveBoom;
}