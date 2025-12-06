using System.Collections.Generic;
using System;

public class PlayerData
{
    [Serializable]
    public class PlayersData
    {
        public string id;
        public string name;
        public string role;
        public string team;
    }

    [Serializable]
    public class RoomUpdateData
    {
        public string room;
        public List<PlayersData> players;
    }
}
