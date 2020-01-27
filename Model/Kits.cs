using SqlSugar;
using System;

namespace fr34kyn01535.Kits.Model
{
    class Kits
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public uint id { get; set; }
        public uint kit { get; set; }
        public uint player { get; set; }
        public ushort count { get; set; }
        public DateTime updated_at { get; set; }

        public Kits()
        {

        }
        public Kits(uint players,uint kit)
        {
            this.player = players;
            this.kit = kit;
            this.count = 0;
            updated_at = DateTime.Now;
        }

    }
}
