using MultipleMiniObelisks.Objects;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleMiniObelisks.Multiplayer
{
    internal class ObeliskTeleportRequestMessage
    {
        public MiniObelisk Obelisk { get; set; }
        public long FarmerId { get; set; }

        public ObeliskTeleportRequestMessage()
        {

        }

        public ObeliskTeleportRequestMessage(MiniObelisk obelisk, long farmerId)
        {
            this.Obelisk = obelisk;
            this.FarmerId = farmerId;
        }
    }
}
