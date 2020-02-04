using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    class ElectricityTest : PlayModeTest
	{
		private PlayerSync player;

		/// <summary>
		/// This test checks outpost station generator behaviour
		/// Spawned CE turn it on and off
		/// </summary>
		/// <returns></returns>
		[UnityTest]
        public IEnumerator ElectricityGeneratorTest()
		{
			// Load lobby scene
			var lobbyScenePath = "Assets/Scenes/OutpostStation.unity";
			EditorSceneManager.OpenScene(lobbyScenePath, OpenSceneMode.Single);

			// Enter play mode
			yield return new EnterPlayMode();

			yield return PlayMode(SkipRoundWaiting());

			yield return PlayMode(ClickButton("CHIEF_ENGINEER"));

			// Move Chief Engineer to Engine Room
			player = PlayerManager.LocalPlayer.GetComponent<PlayerSync>();
			yield return PlayMode(Move(10, MoveAction.MoveUp));
			yield return PlayMode(Settle());
			yield return PlayMode(Move(3, MoveAction.MoveUp));

			// turn generator on/off
			var generator = GetAtRelative<PowerGenerator>(Orientation.Right);
			InteractionUtils.RequestInteract(HandApply.ByLocalPlayer(generator.gameObject), generator);
			yield return PlayMode(new WaitForSeconds(3f));
			InteractionUtils.RequestInteract(HandApply.ByLocalPlayer(generator.gameObject), generator);
			yield return PlayMode(new WaitForSeconds(3f));

			// Exit play mode
			yield return new ExitPlayMode();

		}

		#region PlayerInteraction
		T GetAtRelative<T>(Orientation orientation) where T : MonoBehaviour
		{
			return MatrixManager.GetAt<T>(player.ClientPosition + Vector3Int.FloorToInt(orientation.Vector), true).First();
		}

		IEnumerator Move(int repeat, params MoveAction[] moves)
		{
			for (int i = 0; i < repeat; i++)
			{
				yield return new WaitUntil(() => TryMove(moves));
			}
		}

		IEnumerator Move(params MoveAction[] moves)
		{
			return new WaitUntil(() => TryMove(moves));
		}

		bool TryMove(params MoveAction[] moves)
		{
			int[] intMoves = Array.ConvertAll(moves, value => (int)value);
			return player.DoAction(new PlayerAction() { moveActions = intMoves});
		}

		#endregion PlayerInteraction
	}
}
