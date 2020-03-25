using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// AI brain for mice
/// used to get hunted by Runtime and squeak
/// </summary>
public class CatAI : MobAI
{
	public float mouseDmg = 70f;
	private string catName;
	private string capCatName;
	private float timeForNextRandomAction;
	private float timeWaiting;
	private bool isLayingDown = false;

	private ConeOfSight coneOfSight;
	private LayerMask mouseMask;

	protected override void Awake()
	{
		base.Awake();
		catName = mobName.ToLower();
		capCatName = char.ToUpper(catName[0]) + catName.Substring(1);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		mouseMask = LayerMask.GetMask("NPC");
		coneOfSight = GetComponent<ConeOfSight>();
	}

	public void Update()//FIXME 
	{
		var mice = AnyMiceNearby();
		if (mice != null)
		{
			ChatBubbleManager.ShowAChatBubble(
				gameObject.transform,
				$"FOUND: {mice}"
			);
		}
	}

	protected override void AIStartServer()
	{
		followingStopped.AddListener(OnFollowingStopped);
	}

	protected override void UpdateMe()
	{
		if (health.IsDead || health.IsCrit || health.IsCardiacArrest) return;

		base.UpdateMe();
		MonitorExtras();
	}
	
	void MonitorExtras()
	{
		return; //FIXME
		if (IsPerformingTask || isLayingDown) return;

		timeWaiting += Time.deltaTime;
		if (timeWaiting > timeForNextRandomAction)
		{
			timeWaiting = 0f;
			timeForNextRandomAction = Random.Range(8f,30f);

			DoRandomAction();
		}
	}
	
	protected override void ResetBehaviours()
	{
		base.ResetBehaviours();
		if (isLayingDown) StopLayingDown();
	}

	public override void OnPetted(GameObject performer)
	{
		int randAction = Random.Range(1,6);
		switch (randAction)
		{
			case 1:
				Purr(performer);
				break;
			case 2:
				Meow(performer);
				break;
			case 3:
				StartCoroutine(ChaseTail(Random.Range(1,5)));
				break;
			case 4:
				StartCoroutine(LayDown(Random.Range(10,15)));
				break;
			case 5:
				StartFleeing(performer.transform, 5f);
				break;
		}
	}

	protected override void OnAttackReceived(GameObject damagedBy)
	{
		Hiss(damagedBy);
		FleeFromAttacker(damagedBy, 10F);
	}

	void OnFollowingStopped()
	{
		BeginExploring(MobExplore.Target.mice, 10f);
	}

	MouseAI AnyMiceNearby()
	{
		var hits = coneOfSight.GetObjectsInSight(mouseMask, dirSprites.CurrentFacingDirection, 10f, 20);
		return hits.FirstOrDefault(m => m.gameObject.GetComponent<MouseAI>() != null)?.gameObject.GetComponent<MouseAI>();
	}

	public override void HuntMouse(MouseAI mouse)
	{
		mouse.gameObject.GetComponent<SimpleAnimal>().ApplyDamage(
			gameObject,
			mouseDmg, 
			AttackType.Melee,
			DamageType.Brute);

		Hiss(mouse.gameObject);
		FollowTarget(mouse.gameObject.transform, 5f);
	}

	IEnumerator ChaseTail(int times)
	{
		var timesSpun = 0;
		Chat.AddLocalMsgToChat($"{capCatName} start chasing its own tail!", gameObject.transform.position, gameObject);

		while (timesSpun <= times)
		{
			for (int spriteDir = 1; spriteDir < 5; spriteDir++)
			{
				dirSprites.DoManualChange(spriteDir);
				yield return WaitFor.Seconds(0.3f);
			}

			timesSpun++;
		}

		yield return WaitFor.EndOfFrame;
	}

	private void Purr(GameObject purred = null)
	{
		//TODO play purr sound
		
		if (purred != null)
		{
			Chat.AddActionMsgToChat(
				purred,
				$"{capCatName} purrs at you!", 
				$"{capCatName} purrs at {purred.ExpensiveName()}");
		}
		else
		{
			Chat.AddActionMsgToChat(gameObject, $"{capCatName} purrs!", $"{capCatName} purrs!");
		}
	}

	private void Meow(GameObject meowed = null)
	{
		//TODO play meow sound
		
		if (meowed != null)
		{
			Chat.AddActionMsgToChat(
				meowed,
				$"{capCatName} meows at you!",
				$"{capCatName} meows at {meowed.ExpensiveName()}");
		}
		else
		{
			Chat.AddActionMsgToChat(gameObject, $"{capCatName} meows!", $"{capCatName} meows!");
		}
	}

	private void Hiss(GameObject hissed = null)
	{
		//TODO play hiss sound
		
		if (hissed != null)
		{
			Chat.AddActionMsgToChat(
				hissed,
				$"{capCatName} hisses at you!", 
				$"{capCatName} hisses at {hissed.ExpensiveName()}");
		}
		else
		{
			Chat.AddActionMsgToChat(gameObject, $"{capCatName} hisses!", $"{capCatName} hisses!");
		}
	}

	private void LickPaws()
	{
		Chat.AddActionMsgToChat(
			gameObject,
			$"{capCatName} start licking its paws!",
			$"{capCatName} start licking its paws!");
	}

	IEnumerator LayDown(int cycles)
	{
		isLayingDown = true;
		//TODO animate layingdown and wagging tail

		StopLayingDown();
		yield break;
	}

	private void StopLayingDown()
	{
		isLayingDown = false;
	}

	private void DoRandomAction()
	{
		// More likely to hunt mouses if nearby!
		var posibbleMouse = AnyMiceNearby();
		if (posibbleMouse != null)
		{

			BeginExploring(MobExplore.Target.mice, 3f);
			return;
		}

		int randAction = Random.Range(1,6);

		switch (randAction)
		{
			case 1:
				Purr();
				break;
			case 2:
				Meow();
				break;
			case 3:
				BeginExploring(MobExplore.Target.mice, 3f);
				break;
			case 4:
				LickPaws();
				break;
			case 5:
				StartCoroutine(ChaseTail(Random.Range(1,4)));
				break;
			// case 6:
			//	 StartCoroutine(LayDown(1));
			//	 break;
		}
	}
}