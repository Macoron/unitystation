using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Distributes all ProximityObject to their chunks by their positions
/// Server side only
/// </summary>
public class ProximityManager : MonoBehaviour
{
	public static ProximityManager Instance { get; private set; }

	/// <summary>
	/// All existing chunks by their position
	/// </summary>
	private Dictionary<Vector2Int, ProximityChunk> chunks
		= new Dictionary<Vector2Int, ProximityChunk>();

	/// <summary>
	/// All registered ProximityObject that can be tracked
	/// </summary>
	private HashSet<ProximityObject> allObjects
		= new HashSet<ProximityObject>();

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (CustomNetworkManager.IsServer)
		{
			UpdateChunks();
		}

	}

	private void UpdateChunks()
	{
		var destroyed = new List<ProximityObject>();

		foreach (var obj in allObjects)
		{
			if (!obj)
			{
				// looks like object was destroyed, 
				// but didn't called OnObjectDestroyed
				destroyed.Add(obj);
				continue;
			}

			var oldPos = obj.ChunkPos;
			var objPos = obj.transform.position;

			// Does object moved outside current chunk?
			var newPos = GetChunkPosition(objPos);
			if (oldPos != newPos)
			{
				if (!chunks.ContainsKey(oldPos))
				{
					Debug.LogError($"{obj} has chunk position {oldPos}, " +
						"but ChunkSyncManager doesn't have such chunk!");
					return;
				}

				// Move object to other chunk
				var oldChunk = chunks[oldPos];
				oldChunk.UnregisterObject(obj);
				var newChunk = GetChunk(newPos);
				newChunk.RegisterObject(obj);

				obj.ChunkPos = newPos;
			}
		}

		// Destroy all invalid objects
		foreach (var obj in destroyed)
		{
			UnregisterObject(obj);
		}
	}

	public void RegisterObject(ProximityObject obj)
	{
		// Add object to watchlist
		if (allObjects.Contains(obj))
		{
			Logger.LogError($"{obj} is already registered in ChunkSyncManager. " +
				"Did you called Start() twice?", Category.Proximity);
			return;
		}
		allObjects.Add(obj);

		// Add object to chunk
		var chunk = GetChunk(obj);
		chunk.RegisterObject(obj);

		// Save chunk pos on object
		obj.ChunkPos = chunk.ChunkPos;
	}

	public void UnregisterObject(ProximityObject obj)
	{
		// Remove object from watchlist
		if (!allObjects.Contains(obj))
		{
			Logger.LogError($"{obj} is not registerd in ChunkSyncManager. " +
				"Didn't it recive Start() before?", Category.Proximity);
			return;
		}
		allObjects.Remove(obj);

		// Get chunk
		var chunkPos = obj.ChunkPos;

		// Delete object from chunk
		var chunk = GetChunk(chunkPos);
		chunk.UnregisterObject(obj);

		// if chunk became empty - delete it
		if (chunk.Count() == 0)
		{
			chunks.Remove(chunkPos);
		}
	}

	public HashSet<ProximityObject> GetVisibleObjects(Vector3 worldPosition, float radius)
	{
		var chunkPos = GetChunkPosition(worldPosition);
		var chunk = GetChunk(chunkPos);

		return chunk.ObjectsInside;
	}

	/// <summary>
	/// Get chunk for ProximityObject by transform.position
	/// If no chunk with such pos exists - new one will be created
	/// </summary>
	private ProximityChunk GetChunk(ProximityObject obj)
	{
		// Assign object to initial chunk
		var objPos = obj.transform.position;
		var chunkPos = GetChunkPosition(objPos);

		var chunk = GetChunk(chunkPos);
		return chunk;
	}

	/// <summary>
	/// Get chunk by chunkPos
	/// If no chunk with such pos exists - new one will be created
	/// </summary>
	private ProximityChunk GetChunk(Vector2Int chunkPos)
	{
		// Need to get chunk in dictionary
		ProximityChunk chunk;
		if (!chunks.ContainsKey(chunkPos))
		{
			// chunk not exist - create new one
			chunk = new ProximityChunk(chunkPos);
			chunks.Add(chunkPos, chunk);
		}
		else
		{
			// chunk with such position already exist
			chunk = chunks[chunkPos];
		}

		return chunk;
	}

	/// <summary>
	/// Transforms world position to chunk position
	/// </summary>
	public static Vector2Int GetChunkPosition(Vector3 pos)
	{
		var size = ProximityChunk.Size;
		int chunkX = (int)(pos.x >= 0 ? pos.x / size : pos.x / size - 1);
		int chunkY = (int)(pos.y >= 0 ? pos.y / size : pos.y / size - 1);
		return new Vector2Int(chunkX, chunkY);
	}
}
