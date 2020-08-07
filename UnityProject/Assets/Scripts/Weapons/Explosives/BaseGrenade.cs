using Mirror;
using System.Collections;
using UnityEngine;

/// <summary>
/// Generic grenade base
/// </summary>
public class BaseGrenade : NetworkBehaviour, IServerDespawn, IPredictedInteractable<HandActivate>
{
	protected const int DEFAULT_SPRITE = 0;
	protected const int ARMED_SPRITE = 1;

	[Tooltip("Used for inventory animation")]
	public Pickupable pickupable;
	[Tooltip("SpriteHandler used for blinking animation")]
	public SpriteHandler spriteHandler;

	protected RegisterItem registerItem;
	protected ObjectBehaviour objectBehaviour;

	protected IExplosiveActivation explosiveActivation;

	//whether this object has exploded
	protected bool hasExploded;

	private void Start()
	{
		registerItem = GetComponent<RegisterItem>();
		objectBehaviour = GetComponent<ObjectBehaviour>();

		// subscribe to bomb arm/disarm events
		explosiveActivation = GetComponent<IExplosiveActivation>();
		if (explosiveActivation != null)
		{
			explosiveActivation.OnExplosiveArmChangedClient += OnExplosiveArmChanged;
			explosiveActivation.OnExplosiveBoom += Explode;
		}

		// Set grenade to locked state by default
		UpdateSprite(DEFAULT_SPRITE);
	}

	public void ClientPredictInteraction(HandActivate interaction)
	{
		// Toggle the throw action after activation
		UIManager.Action.Throw();
	}

	public void ServerRollbackClient(HandActivate interaction)
	{
	}

	public virtual void ServerPerformInteraction(HandActivate interaction)
	{
		// arm explosive when player clicked on it
		if (explosiveActivation != null)
		{
			if (!explosiveActivation.IsExplosiveArmed)
			{
				explosiveActivation?.ArmExplosive();
			}
		}
	}

	protected virtual void OnExplosiveArmChanged(bool isArmed)
	{
		if (isArmed)
		{
			UpdateSprite(ARMED_SPRITE);
			StartCoroutine(AnimateSpriteInHands());
		}
		else
		{
			UpdateSprite(DEFAULT_SPRITE);
			StopCoroutine(AnimateSpriteInHands());
		}
	}

	public virtual void Explode()
	{
		if (hasExploded)
		{
			return;
		}
		hasExploded = true;

		// all explosion logic should be implemented in inhereted classes
	}

	/// <summary>
	/// This coroutines make sure that sprite in hands is animated
	/// TODO: replace this with more general aproach for animated icons
	/// </summary>
	/// <returns></returns>
	private IEnumerator AnimateSpriteInHands()
	{
		while (explosiveActivation.IsExplosiveArmed && !hasExploded)
		{
			pickupable.RefreshUISlotImage();
			yield return null;
		}
	}

	protected void UpdateSprite(int sprite)
	{
		// Update sprite in game
		spriteHandler?.ChangeSprite(sprite);
	}

	public void OnDespawnServer(DespawnInfo info)
	{
		// Set grenade to locked state by default
		UpdateSprite(DEFAULT_SPRITE);
		// Reset grenade timer
		hasExploded = false;
	}
}