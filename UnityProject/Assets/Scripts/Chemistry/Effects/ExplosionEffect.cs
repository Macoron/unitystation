using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chemistry.Effects
{
	[CreateAssetMenu(fileName = "reaction", menuName = "ScriptableObjects/Chemistry/Effect/Explosion")]
	public class ExplosionEffect : Effect
	{
		public float explosionRadiusMultiplayer = 4f;
		public float explosionDamageMultiplayer = 1f;
		public GameObject explosionPrefab;

		public override void Apply(MonoBehaviour sender, float amount)
		{
			var pushPull = sender.GetComponent<PushPull>();
			if (!pushPull)
			{
				return;
			}

			var worldPos = pushPull.AssumedWorldPositionServer();
			var explosionMatrix = pushPull.registerTile.Matrix;


			if (explosionPrefab)
			{
				var explosionGO = Instantiate(explosionPrefab, explosionMatrix.transform);
				var explosion = explosionGO.GetComponent<Explosion>();
				if (explosion)
				{
					explosion.transform.position = worldPos;

					explosion.damage = (int)(explosionDamageMultiplayer * amount);
					explosion.minDamage = explosion.damage;
					explosion.radius = 1f + explosionRadiusMultiplayer * amount;

					explosion.Explode(explosionMatrix);
				}


			}
		}
	}
}

