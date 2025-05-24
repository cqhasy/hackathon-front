using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fiycraft.scripts.main
{
    internal class PlayerItems
    {
        public PlayerItems()
        {
            CurrentItemsCD = ItemsCD
                .Select(x => x.Key)
                .ToDictionary<string, string, double>(x => x, _ => 0);
        }

        public Dictionary<string, bool> usableItems = new Dictionary<string, bool>
        {
            { "Dash", true },
            { "SlowMode", true },
            { "Shield", true },
        };
        public Dictionary<string, double> CurrentItemsCD;
        public Dictionary<string, double> ItemsCD = new Dictionary<string, double>
        {
            { "Dash", 5 },
            { "SlowMode", 5 },
            { "Shield", 5 },
        };

        public bool ItemUsable(string item)
        {
            if (usableItems.TryGetValue(item, out var result))
                return CurrentItemsCD[item] == 0;
            return false;
        }

        public bool UseItemIfItUsable(string item)
        {
            var result = ItemUsable(item);
            if (result)
                CurrentItemsCD[item] = ItemsCD[item];
            return result;
        }
    }
}
