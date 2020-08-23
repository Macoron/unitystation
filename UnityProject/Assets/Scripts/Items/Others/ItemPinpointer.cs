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
			UpdateManager.Remove(CallbackType.PERIODIC_UPDATE, ServerUpdate);
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
			ServerChangeSpriteSheetAndVariant(ERROR_SHEET, 0);
			return;
		}

		// find direction to target
		var dirToTarget = objectToTrack.AssumedWorldPosServer() - gameObject.AssumedWorldPosServer();

		// check if they have same position
		if (dirToTarget == Vector3.zero)
		{
			ServerChangeSpriteSheetAndVariant(DIRECT_SHEET, 0);
			return;
		}

		// update distance sprite (animation blink intensity and color)
		var spriteID = ServerGetDistanceSprite(dirToTarget);

		// get angle between direction to object and north and update arrow
		float angle = Vector2.SignedAngle(Vector2.up, dirToTarget);
		int varID = ServerUpdateAngleSprite(angle);

		ServerChangeSpriteSheetAndVariant(spriteID, varID);
	}

	[Server]
	private int ServerGetDistanceSprite(Vector3 moveDirection)
	{
		if (moveDirection.magnitude > mediumMagnitude)
		{
			return FAR_SHEET;
		}
		else if (moveDirection.magnitude > closeMagnitude)
		{
			return MEDIUM_SHEET;
		}
		else
		{
			return CLOSE_SHEET;
		}
	}

	[Server]
	private int ServerUpdateAngleSprite(float angle)
	{
		// TODO: rewrite with something like this
		// https://stackoverflow.com/questions/35104991/relative-cardinal-direction-of-two-coordinates

		// moving counter-clockwise
		switch (angle)
		{
			case 0f:
				return NORTH_SPRITE;
			case -90.0f:
				return EAST_SPRITE;
			case 180.0f:
				return SOUTH_SPRITE;
			case 90.0f:
				return WEST_SPRITE;
			default:
				break;
		}

		// based on orientation above
		if(angle < 0.0f && angle > -90.0f)
		{
			return NE_SPRITE;
		}
		if (angle > 0.0f && angle < 90.0f)
		{
			return NW_SPRITE;
		}
		if (angle > 90.0f && angle < 180.0f)
		{
			return SW_SPRITE;
		}
		if (angle < -90.0f)
		{
			return SE_SPRITE;
		}

		return -1;
	}

	[Server]
	public void ServerPerformInteraction(HandActivate interaction)
	{
		var newIsOn = !isOn;
		if (newIsOn)
		{
			// first update sprites before showing graphics
			// this will make sure that playe will see correct direction
			ServerUpdateSprites();
		}

		// next toggle graphics
		isOn = newIsOn;
	}

	[Server]
	private void ServerChangeSpriteSheetAndVariant(int newSheet, int newVar)
	{
		spriteHandler?.ChangeCatalogueAndVariant(newSheet, newVar);
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
