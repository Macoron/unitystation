using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Light2D;
using UnityEngine;

// Note: Judging the "lighting" sprite sheet it seems that light source can have many disabled states.
// At this point i just want to do a basic setup for an obvious extension, so only On / Off states are actually implemented
// and for other states is just a state and sprite assignment.
internal enum LightState
{
	None = 0,

	On,
	Off,

	// Placeholder states, i assume naming would change.
	MissingBulb,
	Dirty,
	Broken,

	TypeCount,
}
/// <summary>
/// Light source, such as a light bar. Note that for wall protrusion lights such as light tubes / light bars,
/// LightSwitch automatically sets their RelatedAPC if it's looking in their general direction
/// </summary>
[ExecuteInEditMode]
public class LightSource : ObjectTrigger
{
	private const LightState InitialState = LightState.Off;

	private readonly Dictionary<LightState, Sprite> mSpriteDictionary = new Dictionary<LightState, Sprite>((int)LightState.TypeCount);
	[Header("Light sprite")]
	public Sprite OnSprite;
	public Sprite OffSprite;
	public Sprite MissingBulbSprite;
	public Sprite BrokenSprite;
	public Sprite DirtySprite;
	[Tooltip("Rays effect sprite, which enabled when light is on")]
	public SpriteRenderer LightFXRenderer;

	[Header("Generates itself if this is null:")]
	public GameObject mLightRendererObject;
	private LightState mState;
	private SpriteRenderer Renderer;
	private float fullIntensityVoltage = 240;
	public float Resistance = 1200;
	private bool tempStateCache;
	private float _intensity;
	/// <summary>
	/// Current intensity of the lights, automatically clamps and updates sprites when set
	/// </summary>
	private float Intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			value = Mathf.Clamp(value, 0, 1);
			if (_intensity != value)
			{
				_intensity = value;
				OnIntensityChange();
			}
		}
	}

	///Note that for wall protrusion lights such as light tubes / light bars,
	// LightSwitch automatically sets this if it's looking in their general direction
	public APC RelatedAPC;
	public LightSwitch relatedLightSwitch;
	public Color customColor; //Leave null if you want default light color.

	// For network sync reliability.
	private bool waitToCheckState;

	private LightState State
	{
		get
		{
			return mState;
		}

		set
		{
			if (mState == value)
				return;

			mState = value;

			OnStateChange(value);
		}
	}
	public override void Trigger(bool iState)
	{
		// Leo Note: Some sync magic happening here. Decided not to touch it.
		tempStateCache = iState;

		if (waitToCheckState)
		{
			return;
		}

		if (Renderer == null)
		{
			waitToCheckState = true;
			if ( this != null )
			{
				StartCoroutine(WaitToTryAgain());
			}
			return;
		}
		else
		{
			State = iState ? LightState.On : LightState.Off;
		}
	}

	//this is the method broadcast invoked by LightSwitch to tell this light source what switch is driving it.
	//it is buggy and unreliable especially when client joins on a rotated matrix, client and server do not agree
	//on which switch owns which light
	public void Received(LightSwitchData Received)
	{
		//Logger.Log (Received.LightSwitchTrigger.ToString() + " < LightSwitchTrigger" + Received.RelatedAPC.ToString() + " < APC" + Received.state.ToString() + " < state" );
		tempStateCache = Received.state;

		if (waitToCheckState)
		{
			return;
		}
		if (Received.LightSwitch == relatedLightSwitch || relatedLightSwitch == null)
		{
			if (relatedLightSwitch == null)
			{
				relatedLightSwitch = Received.LightSwitch;
			}
			if (Received.RelatedAPC != null)
			{
				RelatedAPC = Received.RelatedAPC;
				{
					if (State == LightState.On)
					{
						if (!RelatedAPC.ConnectedSwitchesAndLights[relatedLightSwitch].Contains(this))
						{
							RelatedAPC.ConnectedSwitchesAndLights[relatedLightSwitch].Add(this);
						}

					}
				}
			}
			else if (relatedLightSwitch.SelfPowered)
			{
				if (State == LightState.On)
				{
					if (!relatedLightSwitch.SelfPowerLights.Contains(this))
					{
						relatedLightSwitch.SelfPowerLights.Add(this);
					}

				}
			}

			if (Renderer == null)
			{
				waitToCheckState = true;
				StartCoroutine(WaitToTryAgain());
				return;
			}
			else
			{
				State = Received.state ? LightState.On : LightState.Off;
			}
		}


	}

	//broadcast target - invoked so lights can register themselves with this APC in
	//LightSwitch.DetectLightsAndAction. There must be a better way to do this that doesn't rely
	//on broadcasting.
	public void EmergencyLight(LightSwitchData Received)
	{
		if (gameObject.tag == "EmergencyLight")
		{
			var emergLightAnim = gameObject.GetComponent<EmergencyLightAnimator>();
			if (emergLightAnim != null)
			{
				Received.RelatedAPC.ConnectEmergencyLight(emergLightAnim);

			}
		}

	}
	private void OnIntensityChange()
	{
		//we were getting an NRE here internally in GetComponent so this checks if the object lifetime
		//is up according to Unity
		if (this == null) return;
		var lightSprites = GetComponentInChildren<LightSprite>();
		if (lightSprites)
		{
			lightSprites.Color.a = Intensity;
		}
	}
	private void OnStateChange(LightState newState)
	{
		// Assign state appropriate sprite to the LightSourceObject.
		if (mSpriteDictionary.ContainsKey(newState) )
		{
			var newSprite = mSpriteDictionary[newState];
			if (newSprite)
			{
				Renderer.sprite = newSprite;
			}
			else
			{
				Logger.LogWarningFormat("{0} lighting missing sprite for an {1} state", Category.Lighting, gameObject.name, newState);
			}
		}

		// Switch Light renderer.
		if (mLightRendererObject != null)
			mLightRendererObject.SetActive(newState == LightState.On);

		// Switch light FX
		if (LightFXRenderer != null)
			LightFXRenderer.gameObject.SetActive(newState == LightState.On);
	}

	public void PowerLightIntensityUpdate(float Voltage)
	{
		if (State == LightState.Off)
		{
			//RelatedAPC.ListOfLights.Remove(this);
			//RelatedAPC = null;
		}
		else
		{
			// Intensity clamped between 0 and 1, and sprite updated automatically with custom get set
			Intensity = Voltage / fullIntensityVoltage;
		}
	}

	private void Awake()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Renderer = GetComponentInChildren<SpriteRenderer>();

		if (mLightRendererObject == null)
		{
			mLightRendererObject = LightSpriteBuilder.BuildDefault(gameObject, new Color(0, 0, 0, 0), 12);
		}

		ExtractLightSprites();

		State = InitialState;

		GetComponent<Integrity>().OnWillDestroyServer.AddListener(OnWillDestroyServer);
	}

	private void OnWillDestroyServer(DestructionInfo arg0)
	{
		Spawn.ServerPrefab("GlassShard", gameObject.TileWorldPosition().To3Int(), transform.parent, count: 2,
			scatterRadius: Spawn.DefaultScatterRadius, cancelIfImpassable: true);
	}

	void Update()
	{
		if (!Application.isPlaying)
		{
			if (gameObject.tag == "EmergencyLight")
			{
				if (RelatedAPC == null)
				{

					Logger.LogError("EmergencyLight is missing APC reference, at " + transform.position, Category.Electrical);
					RelatedAPC.Current = 1; //so It will bring up an error, you can go to click on to go to the actual object with the missing reference
				}
			}
			return;
		}
	}

	void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Color _color;

		if (customColor == new Color(0, 0, 0, 0))
		{
			_color = new Color(0.7264151f, 0.7264151f, 0.7264151f, 0.8f);
		}
		else
		{
			_color = customColor;
		}

		mLightRendererObject.GetComponent<LightSprite>().Color = _color;
	}

	private void ExtractLightSprites()
	{
		mSpriteDictionary.Add(LightState.On, OnSprite);
		mSpriteDictionary.Add(LightState.Off, OffSprite);
		mSpriteDictionary.Add(LightState.MissingBulb, MissingBulbSprite);
		mSpriteDictionary.Add(LightState.Dirty, DirtySprite);
		mSpriteDictionary.Add(LightState.Broken, BrokenSprite);
	}

	// Handle sync failure.
	private IEnumerator WaitToTryAgain()
	{
		yield return WaitFor.Seconds(0.2f);
		if (Renderer == null)
		{
			Renderer = GetComponentInChildren<SpriteRenderer>();
			if (Renderer != null)
			{
				State = tempStateCache ? LightState.On : LightState.Off;
				if (mLightRendererObject != null)
				{
					mLightRendererObject.SetActive(tempStateCache);
				}
			}
			else
			{
				Logger.LogWarning("LightSource still failing Renderer sync", Category.Lighting);
			}
		}
		else
		{
			State = tempStateCache ? LightState.On : LightState.Off;
			if (mLightRendererObject != null)
			{
				mLightRendererObject.SetActive(tempStateCache);
			}
		}
		waitToCheckState = false;
	}
}