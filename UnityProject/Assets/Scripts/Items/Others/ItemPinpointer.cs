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

	private ItemsSprites newSprites = new ItemsSprites();
	private Pickupable pick;
	private float timeElapsedSprite = 0;
	private float timeElapsedIcon = 0;
	public float timeWait = 1;

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

	private void Start()
	{
		var NukeDisks = FindObjectsOfType<NukeDiskScript>();

		foreach (var nukeDisk in NukeDisks)
		{
			if (nukeDisk == null) continue;

			if(!nukeDisk.secondaryNukeDisk)
			{
				objectToTrack =  nukeDisk.gameObject;
				break;
			}
		}
	}

	private void OnEnable()
	{
		UpdateManager.Add(CallbackType.UPDATE, UpdateMe);
		EnsureInit();
	}
	void OnDisable()
	{

		UpdateManager.Remove(CallbackType.UPDATE, UpdateMe);

	}

	private void ChangeAngleofSprite()
	{
		// find direction to target
		var dirToTarget = objectToTrack.AssumedWorldPosServer() - gameObject.AssumedWorldPosServer();

		// check if they have same position
		if (dirToTarget == Vector3.zero)
		{
			//ServerChangeSpriteVariant(0);
			ServerChangeSpriteSheetVariant(DIRECT_SHEET);
			return;
		}

		// set distance sprite (animation blink intensity)
		UpdateDistanceSprite(dirToTarget);

		// get angle between direction to object and north and update arrow
		float angle = Vector2.SignedAngle(Vector2.up, dirToTarget);
		AngleUpdate(angle);
	}
	private void UpdateDistanceSprite(Vector3 moveDirection)
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
	private void AngleUpdate(float angle)
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


	private void EnsureInit()
	{
		pick = GetComponent<Pickupable>();
		spriteHandler = rendererSprite.GetComponent<SpriteHandler>();
	}

	public void ServerPerformInteraction(HandActivate interaction)
	{
			if(objectToTrack == null)
			{
				objectToTrack = FindObjectOfType<NukeDiskScript>().gameObject;
			}
			isOn = !isOn;
			ServerChangeSpriteVariant(0);
			ServerChangeSpriteSheetVariant(0);
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


	protected virtual void UpdateMe()
	{
		if (isServer)
		{

			timeElapsedSprite += Time.deltaTime;
			if (timeElapsedSprite > timeWait)
			{
				if (isOn)
				{
					ChangeAngleofSprite();
				}
				timeElapsedSprite = 0;
			}
		}
		else
		{
			timeElapsedIcon += Time.deltaTime;
			if (timeElapsedIcon > 0.2f)
			{
				timeElapsedIcon = 0;
			}
		}
	}
}
