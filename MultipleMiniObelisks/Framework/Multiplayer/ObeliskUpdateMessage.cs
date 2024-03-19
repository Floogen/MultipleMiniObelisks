using MultipleMiniObelisks.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleMiniObelisks.Multiplayer
{
    internal class ObeliskUpdateMessage
    {
        public MiniObelisk Obelisk { get; set; }

        public ObeliskUpdateMessage()
        {

        }

        public ObeliskUpdateMessage(MiniObelisk obelisk)
        {
            this.Obelisk = obelisk;
        }
    }
}
