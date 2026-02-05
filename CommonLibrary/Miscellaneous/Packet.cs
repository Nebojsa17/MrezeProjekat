using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Miscellaneous
{

    public enum PacketType { DISCARD, PLAYENEMY, PLAYCARD, HAND, INILINES };

    [Serializable]
    public class Packet
    {
        public PacketType Vrsta { get; set; }
        public object Sadrzaj { get; set; }

        public Packet(PacketType vrsta, object sadrzaj)
        {
            Vrsta = vrsta;
            Sadrzaj = sadrzaj;
        }
    }
}
