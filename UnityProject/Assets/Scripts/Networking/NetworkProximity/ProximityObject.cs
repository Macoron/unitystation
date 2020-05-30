using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This component get tracked by ProximityManager
/// </summary>
public class ProximityObject : NetworkBehaviour
{
	public Vector2Int ChunkPos { get; set; }

	private HashSet<NetworkConnection> proposedObservers
		= new HashSet<NetworkConnection>();
	private bool isDirty;

	[ServerCallback]
	private void Start()
	{
		// Register this object in ProximityManager
		// Now ProximityManager will update ChunkPos of this object
		ProximityManager.Instance?.RegisterObject(this);
	}


	[ServerCallback]
	private void OnDestroy()
	{
		// Unregister this object in ProximityManager
		ProximityManager.Instance?.UnregisterObject(this);
	}

	[ServerCallback]
	private void Update()
	{
		if (isDirty)
		{
			// rebuild observers if set changed
			netIdentity.RebuildObservers(false);
			isDirty = false;
		}
	}

	/// <summary>
	/// Add player to the list of observers of this object
	/// </summary>
	/// <param name="connectionToClient">Client that want to become observer of this object</param>
	[Server]
	public void AddObserver(NetworkConnection connectionToClient)
	{
		if (!proposedObservers.Contains(connectionToClient))
		{
			proposedObservers.Add(connectionToClient);
			isDirty = true;
		}
	}

	/// <summary>
	/// Remove player from the list of observers of this object
	/// </summary>
	/// <param name="connectionToClient">Client that no longer want to observe this object state</param>
	[Server]
	public void RemoveObserver(NetworkConnection connectionToClient)
	{
		if (proposedObservers.Contains(connectionToClient))
		{
			proposedObservers.Remove(connectionToClient);
			isDirty = true;
		}
	}


	public override bool OnCheckObserver(NetworkConnection conn)
	{
		// This method is called on the SERVER, when a NEW player enters the game.

		// TODO: Returning false will not spawn this object on client until it became visible
		// It would be ideal for game perfomance, but many of our logic based on messages
		// So if message came to client, but won't find reciver it will create error and desync
		// In future it should be fixed by migrating message and RPC logic to SyncVars
		return true;
	}

	///<inheritdoc/>
	public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
	{
		// add all proposed observers to mirror observers set
		foreach (var obs in proposedObservers)
			observers.Add(obs);

		// clear proposed set - it will be updated next frame
		proposedObservers.Clear();

		// need to return true to override mirror build-in logic 
		return true;
	}
}
