using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that can be lit fy firesource
/// Used for cigarettes, torches, candles, etc.
/// </summary>
public class Flamable : MonoBehaviour, ICheckedInteractable<HandApply>,
	ICheckedInteractable<InventoryApply>, IServerDespawn
{
	/// <summary>
	/// Was this object lit by fire source?
	/// </summary>
	public bool ServerIsLit
	{
		get; private set;
	}

	/// <summary>
	/// Invokes server-side when object was lit by fire source
	/// Fire source can be null
	/// </summary>
	public event System.Action<GameObject> ServerOnWasLit;

	public void ServerPerformInteraction(HandApply interaction)
	{
		ServerTryLight(interaction.UsedObject);
	}

	public void ServerPerformInteraction(InventoryApply interaction)
	{
		ServerTryLight(interaction.UsedObject);
	}

	public bool WillInteract(HandApply interaction, NetworkSide side)
	{
		// standard validation for interaction
		if (!DefaultWillInteract.Default(interaction, side))
		{
			return false;
		}

		return ServerCheckInteraction(interaction, side);
	}

	public bool WillInteract(InventoryApply interaction, NetworkSide side)
	{
		// standard validation for interaction
		if (!DefaultWillInteract.Default(interaction, side))
		{
			return false;
		}

		return ServerCheckInteraction(interaction, side);
	}

	private bool ServerCheckInteraction(Interaction interaction, NetworkSide side)
	{
		if (ServerIsLit)
		{
			return false;
		}

		// check if player want to use some light-source
		if (interaction.UsedObject)
		{
			var lightSource = interaction.UsedObject.GetComponent<FireSource>();
			if (lightSource && lightSource.IsBurning)
			{
				return true;
			}
		}

		return false;
	}

	public void ServerTryLight(GameObject fireSource)
	{
		if (!ServerIsLit)
		{
			ServerIsLit = true;
			ServerOnWasLit?.Invoke(fireSource);
		}
	}

	public void OnDespawnServer(DespawnInfo info)
	{
		ServerIsLit = false;
	}
}
