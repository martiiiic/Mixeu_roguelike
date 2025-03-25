using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallChangeForCustomRooms : MonoBehaviour
{
    [System.Serializable]
    public class TileVariation
    {
        public TileBase[] originalTiles;
        public TileBase[] replacementTiles;
    }

    [System.Serializable]
    public class RoomTypeWallSet
    {
        public string roomTag;
        public TileVariation[] tileVariations;
    }

    [Header("Room Type Wall Sets")]
    public RoomTypeWallSet[] roomWallSets;

    public void ApplyWallChangesForRoomType(string roomTag)
    {
        // Find all tilemaps in the entire scene
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();

        // Find the matching room wall set
        RoomTypeWallSet selectedRoomSet = System.Array.Find(roomWallSets, set => set.roomTag == roomTag);

        if (selectedRoomSet != null)
        {
            // Filter tilemaps to only those in the specific room
            Tilemap[] roomTilemaps = Array.FindAll(allTilemaps, tilemap =>
                tilemap.transform.parent.CompareTag("Room") // Assumes parent object is tagged with room type
            );

            foreach (var tilemap in roomTilemaps)
            {
                ApplyRoomSpecificTileVariations(tilemap, selectedRoomSet);
            }
        }
    }

    private void ApplyRoomSpecificTileVariations(Tilemap tilemap, RoomTypeWallSet roomSet)
    {
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                TileBase originalTile = tilemap.GetTile(tilePosition);

                if (originalTile != null)
                {
                    // Find a matching tile variation
                    TileVariation matchingVariation = System.Array.Find(roomSet.tileVariations,
                        variation => System.Array.Exists(variation.originalTiles, tile => tile == originalTile));

                    // Replace the tile if a matching variation is found
                    if (matchingVariation != null && matchingVariation.replacementTiles.Length > 0)
                    {
                        // Randomly select a replacement tile
                        TileBase replacementTile = matchingVariation.replacementTiles[UnityEngine.Random.Range(0, matchingVariation.replacementTiles.Length)];
                        tilemap.SetTile(tilePosition, replacementTile);
                    }
                }
            }
        }
    }
    public void ChangeWallsForStore()
    {
        ApplyWallChangesForRoomType("Store");
    }
}