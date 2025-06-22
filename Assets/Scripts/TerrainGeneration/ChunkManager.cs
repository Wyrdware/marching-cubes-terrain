using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField] 
    private GameObject chunkPrefab;
    [SerializeField]
    private int chunkSize = 10;
    [SerializeField]
    private int renderDistance = 3;

    [SerializeField]
    private ScalarField scalarField;

    private Dictionary<Vector3Int, GameObject> chunks = new();
    private HashSet<Vector3Int> loadedChunks = new();


    
    // Update is called once per frame
    void Update()
    {
        HashSet<Vector3Int> chunksInRange = GetChunksInRange();
        HashSet<Vector3Int> chunksToRemove = new(loadedChunks);
        HashSet<Vector3Int> chunksToAdd = new(chunksInRange);


        chunksToRemove.ExceptWith(chunksInRange);
        chunksToAdd.ExceptWith(loadedChunks);

        AddChunks(chunksToAdd);
        //RemoveChunks(chunksToRemove);


    }
    public void AddChunks(HashSet<Vector3Int> chunksToAdd)
    {
        foreach (var chunk in chunksToAdd)
        {
            GameObject newChunk = Instantiate(chunkPrefab, ((Vector3)chunk) * (float)chunkSize, Quaternion.identity);
            chunks.Add(chunk, newChunk);
            newChunk.GetComponent<Chunk>().Generate(chunkSize, scalarField);
            loadedChunks.Add(chunk);
            return;// Only add the first chunk each frame
        }
        
    }
    public void RemoveChunks(HashSet<Vector3Int> chunksToRemove)
    {
        foreach(var chunk in chunksToRemove)
        {
            Destroy(chunks[chunk]);
            chunks.Remove(chunk); 
            loadedChunks.Remove(chunk);

        }
    }

    public HashSet<Vector3Int> GetChunksInRange()
    {
        HashSet<Vector3Int> chunksInRange = new();
        Vector3Int playerChunk = new Vector3Int((int)(player.position.x/chunkSize), (int)(player.position.y/chunkSize), (int)(player.position.z/chunkSize));
        for(int x = -renderDistance; x<=renderDistance; x++)
        {
            
            for (int y = -renderDistance; y<= renderDistance; y++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    Vector3Int chunkoffset= new Vector3Int(x,y,z);

                    chunksInRange.Add( chunkoffset + playerChunk);
                    
                }
            }
        }
        
        return chunksInRange;
    }
}
