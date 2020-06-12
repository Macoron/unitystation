using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingProcessVendor : HackingProcessBase
{
	public NetTabType NetTabType = NetTabType.HackingPanel;

	public override bool WillInteract(HandApply interaction, NetworkSide side)
	{
		if (!DefaultWillInteract.Default(interaction, side)) return false;
		if (interaction.TargetObject != gameObject) return false;

		if (Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Screwdriver))
		{
			return true;
		}

		return WiresExposed;
	}

	public override void ClientPredictInteraction(HandApply interaction)
	{
	}

	public override void ServerRollbackClient(HandApply interaction) { }

	public override void ServerPerformInteraction(HandApply interaction)
	{
		// Does player want to open/close panel with a screwdriver?
		if (interaction.HandObject != null && interaction.Performer != null)
		{
			if (Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Screwdriver))
			{
				// Toggle maintance panel
				Chat.AddExamineMsgFromServer(interaction.Performer,
					"You " + (WiresExposed ? "close" : "open") + " the vending machine maintenance panel");
				ToggleWiresExposed();
			}
		}
	}

	public override void ServerLinkHackingNodes()
	{
	}

	public override void OnDespawnServer(DespawnInfo info)
	{
		NetworkTabManager.Instance.RemoveTab(gameObject, NetTabType);
	}
}
