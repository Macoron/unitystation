using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Base class for smokable cigarette
/// </summary>
public class Cigarette : MonoBehaviour
{
	private const int DEFAULT_SPRITE = 0;
	private const int LIT_SPRITE = 1;

	[SerializeField]
	[Tooltip("Object to spawn after cigarette burnt")]
	private GameObject buttPrefab = null;

	[SerializeField]
	[Tooltip("Time after cigarette will destroy and spawn butt")]
	private float smokeTimeSeconds = 180;

	public SpriteHandler spriteHandler = null;
	private FireSource fireSource = null;
	private Pickupable pickupable = null;
	private Flamable flamable = null;

	private void Awake()
	{
		pickupable = GetComponent<Pickupable>();
		fireSource = GetComponent<FireSource>();

		flamable = GetComponent<Flamable>();
		if (flamable)
		{
			flamable.ServerOnWasLit += (source) =>
			{
				ServerChangeLit(true);
			};
		}
	}

	private void ServerChangeLit(bool isLitNow)
	{
		// TODO: add support for in-hand and clothing animation
		// update cigarette sprite to lit state
		if (spriteHandler)
		{
			var newSpriteID = isLitNow ? LIT_SPRITE : DEFAULT_SPRITE;
			spriteHandler.ChangeSprite(newSpriteID);
		}

		// toggle flame from cigarette
		if (fireSource)
		{
			fireSource.IsBurning = isLitNow;
		}

		StartCoroutine(FireRoutine());
	}

	public void OnDespawnServer(DespawnInfo info)
	{
		ServerChangeLit(false);
	}

	private void Burn()
	{
		var worldPos = gameObject.AssumedWorldPosServer();
		var tr = gameObject.transform.parent;
		var rotation = RandomUtils.RandomRotatation2D();

		// Print burn out message if in players inventory 
		if (pickupable && pickupable.ItemSlot != null)
		{
			var player = pickupable.ItemSlot.Player;
			if (player)
			{
				Chat.AddExamineMsgFromServer(player.gameObject,
					$"Your {gameObject.ExpensiveName()} goes out.");
			}
		}

		// Despawn cigarette
		Despawn.ServerSingle(gameObject);
		// Spawn cigarette butt
		Spawn.ServerPrefab(buttPrefab, worldPos, tr, rotation);
	}

	private IEnumerator FireRoutine()
	{
		// wait until cigarette will burn
		yield return new WaitForSeconds(smokeTimeSeconds);
		// despawn cigarette and spawn burn
		Burn();
	}
}
