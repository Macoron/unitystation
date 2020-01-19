using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component which allows an object to broke if integrity levels goes down bellow certain value
/// It's also changes current sprite, if integrity changed
/// </summary>
[RequireComponent(typeof(Integrity))]
public class Breakable : NetworkBehaviour
{
	[SerializeField]
	private bool isInitialyBroken;

	[SyncVar(hook = nameof(SyncIsBroken))]
	private bool isBroken;

	/// <summary>
	/// Is item broken?
	/// </summary>
	public bool IsBroken => isBroken;

	private void SyncIsBroken(bool newValue)
	{
		this.isBroken = newValue;
	}
}
