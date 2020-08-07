using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple explosive timer activator
/// </summary>
public class ExplosiveTimer : NetworkBehaviour, IExplosiveActivation, IServerDespawn
{
	[TooltipAttribute("If the fuse is precise or has a degree of error equal to fuselength / 4")]
	public bool unstableFuse = false;
	[TooltipAttribute("fuse timer in seconds")]
	public float fuseLength = 3;
	[TooltipAttribute("Sound to play when timer is armed")]
	public string activationSound = "armbomb";

	// events for explosive
	public event System.Action<bool> OnExplosiveArmChangedClient;
	public event System.Action OnExplosiveBoom;

	[SyncVar(hook = nameof(UpdateTimerHook))]
	private bool isTimerRunning = false;

	public bool IsExplosiveArmed => isTimerRunning;

	private void UpdateTimerHook(bool wasTimerRunning, bool timerRunning)
	{
		this.isTimerRunning = timerRunning;
		OnExplosiveArmChangedClient?.Invoke(timerRunning);
	}

	public void ArmExplosive()
	{
		if (!isTimerRunning)
		{
			isTimerRunning = true;
			StartCoroutine(TimeExplode());
			PlayPinSFX();
		}
	}

	public void DisarmExplosive()
	{
		if (isTimerRunning)
		{
			isTimerRunning = false;
			StopCoroutine(TimeExplode());
		}
	}

	private IEnumerator TimeExplode()
	{
		float timeLeft = fuseLength;
		if (unstableFuse)
		{
			float fuseVariation = fuseLength / 4;
			timeLeft = Random.Range(fuseLength - fuseVariation, fuseLength + fuseVariation);
		}

		yield return WaitFor.Seconds(timeLeft);

		// Is timer still running? Was it interupted?
		if (isTimerRunning)
		{
			OnExplosiveBoom?.Invoke();
		}
	}

	public void OnDespawnServer(DespawnInfo info)
	{
		isTimerRunning = false;
	}

	private void PlayPinSFX()
	{
		var position = gameObject.AssumedWorldPosServer();
		SoundManager.PlayNetworkedAtPos(activationSound, position);
	}
}
