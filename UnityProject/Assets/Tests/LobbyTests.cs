using System;
using System.Collections;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace Tests
{
	class LobbyTests : PlayModeTest
	{
		/// <summary>
		/// This test starts local game from lobby screen, join as Assistant and exit game
		/// </summary>
		/// <returns></returns>
		[UnityTest]
		public IEnumerator StartsGameFromLobby()
		{
			// Load lobby scene
			var lobbyScenePath = "Assets/Scenes/Lobby.unity";
			EditorSceneManager.OpenScene(lobbyScenePath, OpenSceneMode.Single);

			// Enter play mode
			yield return new EnterPlayMode();

			// Set game in local mode and skip login
			yield return PlayMode(SkipLogin());

			// Start new game
			yield return PlayMode(ClickButton("StartGameButton"));

			// Skip 30 seconds round timer waiting
			yield return PlayMode(SkipRoundWaiting());

			// Choose assistant job
			yield return PlayMode(ClickButton("ASSISTANT"));
			yield return PlayMode(new WaitForSeconds(5f));

			// Close current round and return to main menu
			GUI_IngameMenu.Instance.isTest = true;
			GUI_IngameMenu.Instance.OpenMenuPanel();
			yield return PlayMode(ClickButton("ExitButton"));
			yield return PlayMode(DoActionWaitSceneUnload(ClickButton("Button1")));

			// Exit play mode
			yield return new ExitPlayMode();
		}
	}
}