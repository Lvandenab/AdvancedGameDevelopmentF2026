using UnityEngine;

[CreateAssetMenu(fileName = "DungeonSettingsSO", menuName = "Scriptable Objects/DungeonSettingsSO")]
public class DungeonSettingsSO : ScriptableObject
{
    [System.Serializable]
    public class RoomInventoryItem
    {
        public DungeonRoomSO room;
        public int copies;
        public bool isEndRoom;
    }
    //The name for this type of dungeon
    [SerializeField]
    public string dungeonName;
    //The rooms that can be in this dungeon and how many times they can show up
    [SerializeField]
    public RoomInventoryItem[] possibleRooms;
    //The minimum number of rooms in the dungeon
    [SerializeField]
    public int minRoomCount = 3;
    //The maximum number of rooms in the dungeon
    [SerializeField]
    public int maxRoomCount = 6;
    //A reference to the object used to close off open exits
    [SerializeField]
    public Transform cap;
    [SerializeField]
    public Transform BlueDoor;
    [SerializeField]
    public Transform BlueKey;
    /*[SerializeField]
    public Transform YellowDoor;
    [SerializeField]
    public Transform YellowKey;*/
}
