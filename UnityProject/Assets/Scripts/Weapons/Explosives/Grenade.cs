using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Light2D;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

/// <summary>
/// This grenade simply spawns prefabs explosion
/// </summary>
public class Grenade : BaseGrenade
{
	[Tooltip("Explosion effect prefab, which creates when timer ends")]
	public Explosion explosionPrefab;

	public override void Explode()
	{
		if (isServer)
		{
			// Get data from grenade before despawning
			var explosionMatrix = registerItem.Matrix;
			var worldPos = objectBehaviour.AssumedWorldPositionServer();

			// Despawn grenade
			Despawn.ServerSingle(gameObject);

			// Explosion here
			var explosionGO = Instantiate(explosionPrefab, explosionMatrix.transform);
			explosionGO.transform.position = worldPos;
			explosionGO.Explode(explosionMatrix);
		}
	}
}