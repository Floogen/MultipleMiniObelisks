using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MultipleMiniObelisks.Objects
{
    public class MiniObelisk
    {
        public string LocationName { get; set; }
        public Vector2 Tile { get; set; }
        public string CustomName { get; set; }

        public MiniObelisk()
        {

        }

        public MiniObelisk(string locationName, Vector2 tile, string customName)
        {
            this.LocationName = locationName;
            this.Tile = tile;
            this.CustomName = customName;
        }
    }
}
