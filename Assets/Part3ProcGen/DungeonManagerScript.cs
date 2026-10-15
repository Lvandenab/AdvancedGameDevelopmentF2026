using System.Collections.Generic;
using UnityEngine;

public class DungeonManagerScript : MonoBehaviour
{
    //A class to keep track of the exits currently open in the dungeon
    private class RoomExits
    {
        public Transform edge;
        public string keyword;

        //Constructor for the class, we send it an edge and a keyword as its 
        // initial value
        public RoomExits(Transform edge, string keyword)
        {
            this.keyword = keyword;
            this.edge = edge;
        }
    }

    [SerializeField]
    private DungeonSettingsSO settings;

    //The deck of rooms to spawn
    private List<DungeonRoomSO> deck = new();
    private List<DungeonRoomSO> enddeck = new();
    //Rooms currently spawned
    private List<RoomScript> currentRooms = new();
    //This is a dynamic list to keep track of which exits are open in the dungeon
    private List<RoomExits> availableExits = new();

    //The layers that can be overlapped during dungeon generation
    public LayerMask overlapLayerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        int roomCount = Random.Range(settings.minRoomCount,
            settings.maxRoomCount);
        DungeonRoomSO newRoom;
        RoomScript currentRoom;

        //Stock the deck with the cards we can have
        RestockDeck();
        //Shuffle
        ShuffleDeck();
        //Get a new room
        Draw(out newRoom);

        //Spawn the room at 0,0,0 with no rotation
        //Store a reference in "currentRoom"
        //We'll use it to keep track of new rooms
        currentRoom = Instantiate(newRoom.prefab, 
            Vector3.zero, Quaternion.identity);

        //For each exit in the room we just spawned
        for (int n = 0; n < currentRoom.doors.Length; n++) 
        {
            //Add it to the list of available exits
            availableExits.Add(new RoomExits(
                currentRoom.doors[n], currentRoom.colorKeywords[n].keyword));
        }

        //Repeat for as many room as we want
        for (int i = 1; i < roomCount; i++)
        {
            //If we couldn't draw a room
            if (!Draw(out newRoom))
            {
                //Shuffle
                RestockDeck();
                //Draw again
                Draw(out newRoom);
            }

            //Debug.Log(currentRoom.GetEntityId());
            SpawnRoom(newRoom, ref currentRoom);
            //Debug.Log(currentRoom.GetEntityId());
        }
        if (enddeck.Count>0)
        {
            int randIndex = Random.Range(0, enddeck.Count);
            DungeonRoomSO endRoom = enddeck[randIndex];

            RoomExits actualChosenExit = SpawnRoom(endRoom, ref currentRoom);
            
            if (currentRoom != null && actualChosenExit != null && settings.BlueDoor !=null)
            {
                Transform doorInstance = Instantiate(settings.BlueDoor);
                doorInstance.position = actualChosenExit.edge.position;
                doorInstance.forward = -actualChosenExit.edge.forward;

                availableExits.Remove(actualChosenExit);
            }
        }

        //We close the exits with a cap
        CloseOffExits();
    }
    RoomExits SpawnRoom(DungeonRoomSO newRoom, ref RoomScript currentRoom)
    {
        RoomExits currentRoomExit;

        int exitInd = 0;

        //Spawn the next room
        // more details to come
        currentRoom = Instantiate(newRoom.prefab,
            Vector3.zero, Quaternion.identity);

        //We make a list to be able to shuffle the exits in the coming room
        List<RoomExits> newRoomExits = new();

        //Then we fill it with the exits in the room we spawned
        for (int n= 0; n < currentRoom.doors.Length; n++)
        {
            newRoomExits.Add(new RoomExits(currentRoom.doors[n], 
                currentRoom.colorKeywords[n].keyword));
        }

        //We shuffle the list
        RoomExits temp;
        for (int i = 0; i < newRoomExits.Count; i++)
        {
            int rand = Random.Range(0, newRoomExits.Count);
            temp = newRoomExits[rand];
            newRoomExits[rand] = newRoomExits[i];
            newRoomExits[i] = temp;
        }

        //For each exit...
        foreach (var exit in newRoomExits)
        {
            //If we can find an available exit with the same keyword
            if ((currentRoomExit = availableExits.Find
                (x => x.keyword == exit.keyword)) != null)
            {
                Debug.Log(string.Format("Attempting to connect {0} to {1}.",
                    exit.keyword, currentRoomExit.keyword));


                //Get a reference to the selected exit
                Transform selectedExit = newRoomExits[exitInd].edge;
                Debug.Log(currentRoom.doors[exitInd].name + " " + exitInd);
                //Parent room to exit
                selectedExit.SetParent(null);
                currentRoom.transform.SetParent(selectedExit);
                //Align door with available exit and make them point to each other
                selectedExit.position = currentRoomExit.edge.position;
                selectedExit.forward = -currentRoomExit.edge.forward;

                //If the room is colliding with another room already on the map, we try to find another exit
                if (CheckCollisions(currentRoomExit.edge.GetComponentInParent<RoomScript>(), currentRoom))
                {
                    Debug.Log("Space was blocked!");
                    //Try another iteration of the loop
                    continue;
                }

                //Remove the exit we used
                newRoomExits.RemoveAt(exitInd);

                //Add the new exits to the available exits
                foreach (var e in newRoomExits)
                {
                    availableExits.Add(e);
                }
                Debug.Log(availableExits.Count);

                //Shuffle the list of available exits
                for (int j = 0; j < availableExits.Count; j++)
                {
                    int rand = Random.Range(0, availableExits.Count);
                    temp = availableExits[j];
                    availableExits[j] = availableExits[rand];
                    availableExits[rand] = temp;
                }

                //Remove the exit we just connected to
                availableExits.Remove(currentRoomExit);

                return currentRoomExit;
            }
            exitInd++;
        }

        Debug.Log("Couldn't find an available exit for the spawned room.");
        //Destroy room created
        Destroy(currentRoom.gameObject);
        return null;
    }

    //
    public void ShuffleDeck()
    {
        DungeonRoomSO temp;

        for (int i = 0; i < deck.Count; i++)
        {
            //choose a random position on the deck
            int rand = Random.Range(0, deck.Count);
            //Swap this element with that one
            temp = deck[rand];
            deck[rand] = deck[i];
            deck[i] = temp;
        }
    }

    //
    public void RestockDeck()
    {
        //Clear the deck
        deck.Clear();
        enddeck.Clear();

        //For each item in the dungeon settings
        foreach (DungeonSettingsSO.RoomInventoryItem item
            in settings.possibleRooms)
        {

            //Add that many "cards" of that type to the deck
            for (int i = 0; i < item.copies; i++)
            {
                if (item.isEndRoom)
                {
                    enddeck.Add(item.room);
                }
                else
                {
                    deck.Add(item.room);
                }
                    
            }
        }
    }

    //
    private void CloseOffExits()
    {
        //For each exit
        foreach (var exit in availableExits)
        {
            //Instantiate a cap
            Transform cap = Instantiate(settings.cap);
            //Position it and rotate it so it closes off the exit
            cap.position = exit.edge.position;
            cap.forward = -exit.edge.forward;
        }
        //Clear the list
        availableExits.Clear();
    }

    /// <summary>
    /// A function that removes a card from the deck and gives it back.
    /// </summary>
    /// <param name="room">Where the drawn "card" will go.</param>
    /// <returns>Whether the draw was succesful.</returns>
    public bool Draw(out DungeonRoomSO room)
    {
        //Set a default value for room
        room = null;

        //If the deck isn't empty
        if (deck.Count > 0)
        {
            //Put the first card in the room variable
            room = deck[0];
            //Remove it from the deck
            deck.RemoveAt(0);
            //Return true
            return true;
        }

        //Return false if the deck is empty
        return false;
    }


    /// <summary>
    /// A function that checks if a newly spawned piece is colliding with something else.
    /// </summary>
    /// <param name="connectingTo">The room that it's connecting to. We ignore it.</param>
    /// <param name="newPiece">The room that just spawned.</param>
    /// <returns></returns>
    private bool CheckCollisions(RoomScript connectingTo, RoomScript newPiece)
    {
        //Disable collider
        newPiece.colliderCheck.enabled = false;
        connectingTo.colliderCheck.enabled = false;

        //The list of colliders overlapping with the newly placed piece.
        //We pass to the function the center, and its size divided by 2 because it's asking for the extents.
        Collider[] collisions = Physics.OverlapBox(
            newPiece.colliderCheck.bounds.center, //The center
            newPiece.colliderCheck.bounds.size / 2,  //The size of the overlap check
            newPiece.transform.rotation, //The rotation of the overlap check
            overlapLayerMask //The layers we're ignoring in this step
            );

        //Enable collider
        newPiece.colliderCheck.enabled = true;
        connectingTo.colliderCheck.enabled = true;

        //If there's any collisions, return true
        return collisions.Length > 0;



        //if (collisions.Length == 0 //If there's no elements, we're good. If there are...
        //    || (collisions.Length == 1 && //If it's only one element
        //    collisions[0].transform == connectingTo.colliderCheck.transform)) //And that element is the door we're trying to connect 
        //    return false; //Then we're safe, we're not hitting anything.

        ////If there's more than one element, then there has to be another room blocking the way
        //return true;
    }
}
