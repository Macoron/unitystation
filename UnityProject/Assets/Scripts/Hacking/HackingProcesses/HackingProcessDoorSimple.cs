using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This handles opening a network tab internally, and hence, doesn't require a HasNetworkTab component. Don't add one!
/// </summary>
public class HackingProcessDoorSimple : HackingProcessBase
{
	public NetTabType NetTabType = NetTabType.HackingPanel;

	[SerializeField]
	[Tooltip("The name that comes up when you interact with the object.")]
	private string doorName = "airlock";

	[SerializeField]
	[Tooltip("SpriteRender which is toggled on when the door panel is exposed.")]
	private SpriteRenderer hackPanelOverlay = null;

	[SerializeField]
	private Sprite hackPanelSprite = null;

	private DoorController controller;
	public DoorController Controller
	{
		get
		{
			if (!controller)
			{
				controller = GetComponent<DoorController>();
			}
			return controller;
		}
	}

	private InteractableDoor intDoor;
	public InteractableDoor IntDoor
	{
		get
		{
			if (!intDoor)
			{
				intDoor = GetComponent<InteractableDoor>();
			}
			return intDoor;
		}
	}

	public override bool WillInteract(HandApply interaction, NetworkSide side)
	{
		if (!DefaultWillInteract.Default(interaction, side)) return false;
		if (interaction.TargetObject != gameObject) return false;
		if (IntDoor == null || !IntDoor.allowInput) return false;

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
		//Do specific things when the wires are exposed.
		if (WiresExposed)
		{
			if (interaction.HandObject == null && interaction.Performer != null)
			{
				TabUpdateMessage.Send(interaction.Performer, gameObject, NetTabType, TabAction.Open);
				IntDoor.StartInputCoolDown();
				return;
			}
		}

		if (Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Screwdriver))
		{
			if (Controller != null)
			{
				Chat.AddExamineMsgFromServer(interaction.Performer,
					"You " + (WiresExposed ? "close" : "open") + " the " + doorName + "'s maintenance panel");
				ServerTryTogglePanel();
			}

			IntDoor.StartInputCoolDown();
			return;
		}
	}

	public override void ServerAddInputMethods()
	{
		Controller.LinkHackNodes();
	}

	public void ServerTryTogglePanel()
	{
		if (!Controller.isPerformingAction)
		{
			ToggleWiresExposed();
		}
	}

	protected override void OnWiresExposed()
	{
		if (hackPanelOverlay != null && hackPanelSprite != null)
		{
			hackPanelOverlay.sprite = hackPanelSprite;
		}
	}

	protected override void OnWiresHidden()
	{
		if (hackPanelOverlay != null)
		{
			hackPanelOverlay.sprite = null;
		}
	}

	public override void OnDespawnServer(DespawnInfo info)
	{
		NetworkTabManager.Instance.RemoveTab(gameObject, NetTabType);
	}

}
