using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TronMarshall
{
    class BotListEntry
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public BotListEntry(string name, Type type)
        {
            Name = name;
            Type = type;
        }
        
    }
}
