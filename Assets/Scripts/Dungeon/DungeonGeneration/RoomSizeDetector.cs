using UnityEngine;
using System.Collections.Generic;

public enum RoomSize
{
    Corridor,  // Narrow passage
    Small,     // 1x1 (10x10 units)
    Large      // 2x2 (20x20 units)
}

public class RoomSizeDetector : MonoBehaviour
{
    [SerializeField] private LayerMask roomLayerMask;

    // Dimensions for different room sizes
    private const float SMALL_ROOM_WIDTH = 10f;
    private const float LARGE_ROOM_WIDTH = 20f;
    private const float CORRIDOR_MAX_WIDTH = 5f;

    // Cache room size results
    private Dictionary<GameObject, RoomSize> roomSizeCache = new Dictionary<GameObject, RoomSize>();

    public RoomSize GetRoomSize(GameObject room)
    {
        // Check cache first
        if (roomSizeCache.TryGetValue(room, out RoomSize cachedSize))
        {
            return cachedSize;
        }

        // Get room's tag to identify predefined types
        string roomTag = room.tag;

        // Special case for corridor rooms that are tagged
        if (roomTag == "Corridor")
        {
            roomSizeCache[room] = RoomSize.Corridor;
            return RoomSize.Corridor;
        }

        // Check room dimensions by looking at collider
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        if (roomCollider != null)
        {
            // Get bounds of the room
            Bounds bounds = roomCollider.bounds;
            float width = bounds.size.x;
            float height = bounds.size.y;

            // Use the longest dimension for comparison
            float maxDimension = Mathf.Max(width, height);

            // Determine room size based on dimensions
            RoomSize detectedSize;
            if (maxDimension <= CORRIDOR_MAX_WIDTH)
            {
                detectedSize = RoomSize.Corridor;
            }
            else if (maxDimension <= SMALL_ROOM_WIDTH * 1.2f)
            {
                detectedSize = RoomSize.Small;
            }
            else
            {
                detectedSize = RoomSize.Large;
            }

            // Cache the result
            roomSizeCache[room] = detectedSize;
            return detectedSize;
        }

        string roomName = room.name.ToLower();
        if (roomName.Contains("large") || roomName.Contains("2x2"))
        {
            roomSizeCache[room] = RoomSize.Large;
            return RoomSize.Large;
        }
        else if (roomName.Contains("corridor") || roomName.Contains("hallway") || roomName.Contains("passage"))
        {
            roomSizeCache[room] = RoomSize.Corridor;
            return RoomSize.Corridor;
        }

        roomSizeCache[room] = RoomSize.Small;
        return RoomSize.Small;
    }

    public bool CanPlaceSpecialRoomInRoom(GameObject room)
    {
        RoomSize size = GetRoomSize(room);

        // Only small (1x1) rooms can have special rooms
        return size == RoomSize.Small;
    }

    public bool CanPlaceEnemiesInRoom(GameObject room)
    {
        RoomSize size = GetRoomSize(room);

        return size != RoomSize.Corridor;
    }

    public void ClearCache()
    {
        roomSizeCache.Clear();
    }
}