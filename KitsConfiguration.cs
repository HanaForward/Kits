using Rocket.API;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace fr34kyn01535.Kits
{
    public class KitsConfiguration : IRocketPluginConfiguration
    {
        public string TableName;

        [XmlArrayItem(ElementName = "Kit")]
        public List<Kit> Kits;
        public int GlobalCooldown;

        public void LoadDefaults()
        {
            TableName = "Kits";

            GlobalCooldown = 10;
            Kits = new List<Kit>() {
                new Kit() { kit = 0 , Count  = 1, Cooldown = 10, Name = "bc", XP = 0,Items = new List<KitItem>() { new KitItem(245, 1) },Conditions = new List<Condition>(){ new Condition("2020/1/1 0:0:0", "2020/1/10 0:0:0")  } },
                new Kit() { kit = 1 , Count  = 5, Cooldown = 10, Name = "new", XP = 0,Items = new List<KitItem>() { new KitItem(245, 1), new KitItem(81, 2), new KitItem(16, 1) }},
                new Kit() { kit = 1 , Cooldown = 10, Name = "force", XP = 0,Money = 30, Vehicle = 57,Items = new List<KitItem>() { new KitItem(112, 1), new KitItem(113, 3), new KitItem(254, 3) }},
                new Kit() { kit = 2 , Cooldown = 10, Name = "food", XP = 200,Money=-20, Items = new List<KitItem>() { new KitItem(109, 1), new KitItem(111, 3), new KitItem(236, 1) }}
            };
        }
    }

    public class Kit
    {
        public Kit() { }

        public uint kit;
        public string Name;
        public ushort? Count;
        public uint? XP = null;
        public decimal? Money = null;
        public ushort? Vehicle = null;

        [XmlArrayItem(ElementName = "Item")]
        public List<KitItem> Items;

        [XmlArrayItem(ElementName = "Condition")]
        public List<Condition> Conditions;

        public int? Cooldown = null;
    }

    public class KitItem
    {

        public KitItem() { }

        public KitItem(ushort itemId, byte amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        [XmlAttribute("id")]
        public ushort ItemId;

        [XmlAttribute("amount")]
        public byte Amount;
    }

    public class Condition
    {
        [XmlAttribute("StartTime")]
        public string StartTime;

        [XmlAttribute("EndTime")]
        public string EndTime;



        public Time GetStartTime()
        {
            Time time = null;
            if (DateTime.TryParse(StartTime, out DateTime result))
            {
                time = new Time();
                time.dateTime = result;
                return time;
            }
            else
            {
                return time;
            }
        }

        public Time GetEndTime()
        {
            Time time = null;
            if (DateTime.TryParse(EndTime, out DateTime result))
            {
                time = new Time();
                time.dateTime = result;
                return time;
            }
            else
            {
                return time;
            }
        }

        public Condition() { }

        public Condition(string StartTime, string EndTime)
        {
            this.StartTime = StartTime;
            this.EndTime = EndTime;
        }

    }

    public class Time
    {
        public DateTime dateTime;
    }


}
