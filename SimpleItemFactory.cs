using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDVX3
{
    public static class SimpleItemFactory
    {
        public static SimpleObject CreateItem(Spawnpoint location, string modItemId, int quality = 0)
        {
            SimpleObject item = new SimpleObject(location.Position, modItemId);
            item.parentSheetIndex = SDVX3Mod.itemID; //make this modItemId dependant later
            item.placeholderId = SDVX3Mod.itemID;

            item.canBeGrabbed = true;
            item.canBeSetDown = false;
            item.Type = "Basic";
            item.readyForHarvest = true;
            item.category = SimpleObject.artisanGoodsCategory;
            item.quality = quality;
            item.displayName = GetDisplayName(modItemId);
            item.isSpawnedObject = true;
            return item;
        }

        public static string GetDisplayName(string modItemId)
        {
            return "McTestyface";
        }

        public static string GetDescription(string modItemId)
        {
            return "An SDVX3 mod object.";
        }
    }
}
