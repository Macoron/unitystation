using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CandleState
{
	public SpriteDataSO litState;
	public SpriteDataSO unlitState;
}

public class Candle : MonoBehaviour
{
	public SpriteHandler spriteHandler;
	public CandleState[] spriteStates;

	private Flamable flamable = null;
	private FireSource fireSource = null;

	[SerializeField]
	[Tooltip("Time for candle to burn-out in seconds")]
	private float maxBurningTimeSeconds = 2000;
	private float burningTimeSeconds;

	private void Awake()
	{
		flamable = GetComponent<Flamable>();
		fireSource = GetComponent<FireSource>();
	}
}
