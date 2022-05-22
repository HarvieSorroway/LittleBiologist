using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LittleBiologist.LBio_Const;

namespace LittleBiologist.LBio_Navigations
{
    public class LBio_NavigationCore
    {
        public static SearchState searchState;
        public static Dictionary<int, List<NaviNode>> SolvesForDest = new Dictionary<int, List<NaviNode>>();

        static Region _currentRegion;
        public static Region currentRegion
        {
            get => _currentRegion;
            set
            {
                if (_currentRegion != value)
                {
                    SolvesForDest.Clear();
                    _currentRegion = value;
                }
            }
        }

        public class NaviNode
        {
            public NaviNode(AbstractRoom thisRoom,NaviNode forwardNode = null)
            {
                this.thisRoomIndex = thisRoom.world.abstractRooms.IndexOf(thisRoom);
                this.forwardNode = forwardNode;
                connections = thisRoom.connections;
            }

            public int NodeLength
            {
                get
                {
                    if(forwardNode == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return forwardNode.NodeLength + 1;
                    }
                }
            }

            public Vector2 ShortCutPos = new Vector2(-10000, -10000);
            public int thisRoomIndex;
            public int[] connections;
            public NaviNode forwardNode;

            public Vector2 GetShortCut(Room currentRoom)
            {
                if(ShortCutPos.x == -10000)
                {
                    if(currentRoom.abstractRoom == currentRoom.world.abstractRooms[thisRoomIndex])
                    {
                        foreach (var shortcut in currentRoom.shortcuts)
                        {
                            if (currentRoom.WhichRoomDoesThisExitLeadTo(shortcut.DestTile) != null && currentRoom.WhichRoomDoesThisExitLeadTo(shortcut.DestTile).index == forwardNode.thisRoomIndex)
                            {
                                ShortCutPos = (shortcut.StartTile.ToVector2() * 20f);
                            }
                        }
                    }
                }
                return ShortCutPos;
            }
        }
        
        public static IEnumerator GetAllSolveInRegion(RoomCamera roomCamera)
        {
            World world = roomCamera.room.world;

            searchState = SearchState.Searching;

            foreach(var newDest in world.abstractRooms)
            {
                if (SolvesForDest.ContainsKey(newDest.index))
                {
                    continue;
                }
                List<NaviNode> newSolve = new List<NaviNode>();
                List<NaviNode> nodesToUpdate = new List<NaviNode>();
                List<AbstractRoom> searchedRooms = new List<AbstractRoom>();
                NaviNode rootNode = new NaviNode(newDest);

                newSolve.Add(rootNode);
                searchedRooms.Add(newDest);
                nodesToUpdate.Add(rootNode);

                while(nodesToUpdate.Count > 0)
                {
                    NaviNode currentNode = nodesToUpdate.Pop();
                    AbstractRoom currentRoom = world.GetAbstractRoom(currentNode.thisRoomIndex);
                    searchedRooms.Add(currentRoom);

                    foreach(var connection in currentRoom.connections)
                    {
                        AbstractRoom nextRoom = world.GetAbstractRoom(connection);

                        if(nextRoom == null)
                        {
                            Log("Navi-Seaerch", "Meet null room");
                            continue;
                        }
                        if (searchedRooms.Contains(nextRoom))
                        {
                            Log("Navi-Seaerch", "Run into room that already searched");
                            continue;
                        }

                        NaviNode newNode = new NaviNode(nextRoom, currentNode);

                        if (!nextRoom.name.Contains("GATE"))
                        {
                            nodesToUpdate.Add(newNode);
                        }
                        newSolve.Add(newNode);

                        yield return null;
                    }
                }

                SolvesForDest.Add(newDest.index, newSolve);
            }

            searchState = SearchState.SearchFinish;
            yield break;
        }

        public enum SearchState
        {
            Searching,
            SearchFinish
        }
    }
}
