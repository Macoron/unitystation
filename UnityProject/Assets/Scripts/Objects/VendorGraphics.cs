using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vendor))]
public class VendorGraphics : MonoBehaviour
{
	public const int POWERED_SPRITE = 0;
	public const int OFF_SPRITE = 1;
	public const int BROKEN_SPRITE = 2;

	public SpriteHandler SpriteHandler;
	public GameObject ScreenGlow;

	private Vendor vendor;
	private HackingProcessVendor hackingVendor;

	private void Awake()
	{
		vendor = GetComponent<Vendor>();
		hackingVendor = GetComponent<HackingProcessVendor>();

		vendor?.OnVendorStateUpdatedClient.AddListener(OnStateUpdatedClient);

	}

	/// <summary>
	/// Changes vendor graphics client-side
	/// </summary>
	private void OnStateUpdatedClient(VendorState state)
	{
		int spriteIndex = POWERED_SPRITE;
		bool isGlowing = true;

		switch (state)
		{
			case VendorState.POWERED:
				spriteIndex = POWERED_SPRITE;
				isGlowing = true;
				break;
			case VendorState.OFF:
				spriteIndex = OFF_SPRITE;
				isGlowing = false;
				break;
			case VendorState.BROKEN:
				spriteIndex = BROKEN_SPRITE;
				isGlowing = false;
				break;
		}

		SpriteHandler?.ChangeSprite(spriteIndex);
		ScreenGlow?.gameObject.SetActive(isGlowing);
	}
}
