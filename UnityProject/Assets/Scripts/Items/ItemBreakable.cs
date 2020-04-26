using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBreakable : MonoBehaviour
{
	[Tooltip("Damage applied to THIS item integrity when player hit other object")]
	public float damageOnHit;

	[Tooltip("Prefab to spawn when object was destroyed")]
	public GameObject brokenItem;

	[Tooltip("Sound to play when object was destroyed")]
	public string soundOnBreak;

	private CustomNetTransform netTransform;
	private Integrity integrity;
	private Pickupable pickupable;

	private void Awake()
	{
		netTransform = GetComponent<CustomNetTransform>();
		if (netTransform)
		{
			// when player threw this item and hit something
			netTransform.OnThrowEnd.AddListener((info) => AddDamage());
		}

		integrity = GetComponent<Integrity>();
		if (integrity)
		{
			// when this item destroyed for any reason
			integrity.OnWillDestroyServer.AddListener((info) => OnWillDestroy());
		}

		pickupable = GetComponent<Pickupable>();
	}

	/// <summary>
	/// Apply damageOnHit to this item integrity
	/// </summary>
	public void AddDamage()
	{
		if (damageOnHit > 0 && integrity)
		{
			integrity.ApplyDamage(damageOnHit, AttackType.Melee, DamageType.Brute);
		}
	}

	/// <summary>
	/// Called just before item will be destroyed
	/// Plays sound and spawn broken prefab
	/// </summary>
	private void OnWillDestroy()
	{
		// Play sound if avaliable
		if (!string.IsNullOrEmpty(soundOnBreak))
		{
			SoundManager.PlayNetworkedAtPos(soundOnBreak,
				gameObject.AssumedWorldPosServer());
		}

		// spawn broken object next (if avaliable)
		SpawnBrokenItem();
	}

	private void SpawnBrokenItem()
	{
		if (!brokenItem)
		{
			return;
		}

		// spawn broken item
		var spawnResult = Spawn.ServerPrefab(brokenItem,
			worldPosition: gameObject.AssumedWorldPosServer(),
			localRotation: transform.localRotation);

		// check if spawn worked right
		if (spawnResult.Successful)
		{
			var brokenGO = spawnResult.GameObject;
			var brokenPickupable = brokenGO.GetComponent<Pickupable>();

			// check if object can be added to inventory
			if (pickupable && brokenPickupable)
			{
				// check if old item was in inventory
				var slot = pickupable.ItemSlot;
				if (slot != null)
				{
					// now replace old item by new one
					// use drop because Integrity script will despawn object anyway
					Inventory.ServerAdd(brokenPickupable, slot,
						ReplacementStrategy.DropOther);
				}
			}
		}
	}
}