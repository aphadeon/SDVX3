using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDVX3
{
    public class Spawnpoint
    {
        public GameLocation Location { get; set; }
        public Vector2 Position { get; set; }

        public Spawnpoint(string location, int x, int y)
        {
            Location = Game1.getLocationFromName(location);
            Position = new Vector2(x, y);
        }

        public void PlaceItem(SimpleObject item)
        {
            Location.objects.Add(Position, item);
        }

        public bool isClear()
        {
            if (Location.objects.ContainsKey(Position)) return false;
            if (!Location.isTileLocationTotallyClearAndPlaceable(Position)) return false;
            return true;
        }

        public void AttemptToClear()
        {
            Location.objects.Remove(Position);
        }
    }
}
