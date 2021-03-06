﻿using Microsoft.Xna.Framework;
using MultipleMiniObelisks.Objects;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleMiniObelisks.Multiplayer
{
    internal class ObeliskTeleportStatusMessage
    {
        public string DestinationName { get; set; }
        public Vector2 DestinationTile { get; set; }
        public long FarmerId { get; set; }
        public bool DoTeleport { get; set; }

        public ObeliskTeleportStatusMessage()
        {

        }

        public ObeliskTeleportStatusMessage(string destinationName, Vector2 destinationTile, long farmerId, bool doTeleport)
        {
            this.DestinationName = destinationName;
            this.DestinationTile = destinationTile;
            this.FarmerId = farmerId;
            this.DoTeleport = doTeleport;
        }
    }
}
