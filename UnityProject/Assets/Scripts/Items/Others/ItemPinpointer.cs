using System;
using System.Collections;
using System.Collections.Generic;
using Atmospherics;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System.Linq;

public class ItemPinpointer : NetworkBehaviour, IInteractable<HandActivate>
{
	public GameObject rendererSprite;
	private GameObject objectToTrack;
	private SpriteHandler spriteHandler;

	private float timeElapsedSprite = 0;
	private float timeElapsedIcon = 0;

	[SyncVar(hook = nameof(CleintIsOnChanged))]
	private bool isOn = false;

	public float maxMagnitude = 80;
	public float mediumMagnitude = 40;
	public float closeMagnitude = 10;

	private const int SOUTH_SPRITE = 0;
	private const int NORTH_SPRITE = 1;
	private const int EAST_SPRITE = 2;
	private const int WEST_SPRITE = 3;
	private const int SE_SPRITE = 4;
	private const int SW_SPRITE = 5;
	private const int NE_SPRITE = 6;
	private const int NW_SPRITE = 7;

	private const int FAR_SHEET = 0;
	private const int MEDIUM_SHEET = 1;
	private const int CLOSE_SHEET = 2;
	private const int DIRECT_SHEET = 3;
	private const int ERROR_SHEET = 4;

	private void Start()
	{
		if (rendererSprite)
		{
			spriteHandler = rendererSprite.GetComponent<SpriteHandler>();
		}

		spriteHandler?.gameObject.SetActive(isOn);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		FindTargetItem();
		UpdateManager.Add(ServerUpdate, 1f);
	}

	private void OnDisable()
	{
		if (isServer)
		{
			UpdateManager.Remove(CallbackType.UPDATE, ServerUpdate);
		}
	}

	[Server]
	public virtual void FindTargetItem()
	{
		var NukeDisks = FindObjectsOfType<NukeDiskScript>();
		foreach (var nukeDisk in NukeDisks)
		{
			if (nukeDisk == null) continue;

			if (!nukeDisk.secondaryNukeDisk)
			{
				objectToTrack = nukeDisk.gameObject;
				break;
			}
		}
	}

	[Server]
	private void ServerUpdateSprites()
	{
		// check if target still valid
		if (!objectToTrack)
		{
			ServerChangeSpriteSheetVariant(ERROR_SHEET);
			return;
		}

		// find direction to target
		var dirToTarget = objectToTrack.AssumedWorldPosServer() - gameObject.AssumedWorldPosServer();

		// check if they have same position
		if (dirToTarget == Vector3.zero)
		{
			ServerChangeSpriteSheetVariant(DIRECT_SHEET);
			return;
		}

		// set distance sprite (animation blink intensity)
		ServerUpdateDistanceSprite(dirToTarget);

		// get angle between direction to object and north and update arrow
		float angle = Vector2.SignedAngle(Vector2.up, dirToTarget);
		ServerUpdateAngleSprite(angle);
	}

	[Server]
	private void ServerUpdateDistanceSprite(Vector3 moveDirection)
	{
		if (moveDirection.magnitude > mediumMagnitude)
		{
			ServerChangeSpriteSheetVariant(FAR_SHEET);
		}
		else if (moveDirection.magnitude > closeMagnitude)
		{
			ServerChangeSpriteSheetVariant(MEDIUM_SHEET);
		}
		else
		{
			ServerChangeSpriteSheetVariant(CLOSE_SHEET);
		}
	}

	[Server]
	private void ServerUpdateAngleSprite(float angle)
	{
		// moving clockwise
		switch (angle)
		{
			case 0f:
				ServerChangeSpriteVariant(NORTH_SPRITE);
				return;
			case -90.0f:
				ServerChangeSpriteVariant(EAST_SPRITE);
				return;
			case 180.0f:
				ServerChangeSpriteVariant(SOUTH_SPRITE);
				return;
			case 90.0f:
				ServerChangeSpriteVariant(WEST_SPRITE);
				return;
			default:
				break;
		}

		// based on orientation above
		if(angle < 0.0f && angle > -90.0f)
		{
			ServerChangeSpriteVariant(NE_SPRITE);
			return;
		}
		if (angle > 0.0f && angle < 90.0f)
		{
			ServerChangeSpriteVariant(NW_SPRITE);
			return;
		}
		if (angle > 90.0f && angle < 180.0f)
		{
			ServerChangeSpriteVariant(SW_SPRITE);
			return;
		}
		if (angle < -90.0f)
		{
			ServerChangeSpriteVariant(SE_SPRITE);
			return;
		}
	}

	[Server]
	public void ServerPerformInteraction(HandActivate interaction)
	{
		isOn = !isOn;
	}

	[Server]
	private void ServerChangeSpriteSheetVariant(int newSheetVar)
	{
		spriteHandler?.ChangeSprite(newSheetVar);
	}

	[Server]
	private void ServerChangeSpriteVariant(int newVar)
	{
		spriteHandler?.ChangeSpriteVariant(newVar);
	}

	[Client]
	private void CleintIsOnChanged(bool oldVal, bool isOn)
	{
		spriteHandler?.gameObject.SetActive(isOn);

		if (isOn)
		{
			// need to force update texture
			spriteHandler?.PushTexture();
		}
	}

	[Server]
	protected virtual void ServerUpdate()
	{
		if (isOn)
		{
			ServerUpdateSprites();
		}
	}
}
