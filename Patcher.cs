using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using Microsoft.Xna.Framework;

namespace SDVX3
{
    class Patcher
    {
        public static void InstallPatches()
        {
            var harmony = HarmonyInstance.Create("avarisc.sdvx3");
            harmony.PatchAll(Assembly.GetCallingAssembly());
        }

        //patch: Farmer.showHoldingItem for SimpleItem support
        class Patch_Farmer_ShowHoldingItem
        {
            [HarmonyTargetMethod]
            static MethodInfo getTargetMethod()
            {
                return typeof(StardewValley.Farmer).GetMethod("showHoldingItem");
            }

            [HarmonyPrefix]
            public static bool showHoldingItemPrefix(ref StardewValley.Farmer who)
            {
                if(who.mostRecentlyGrabbedItem is SimpleObject)
                {

                    who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(SDVX3Mod.texture, (who.mostRecentlyGrabbedItem as SimpleObject).GetSpriteSourceRect(), 2500f, 1, 0, who.position + new Vector2(0f, (float)(-(float)Game1.tileSize * 3 + 4)), false, false)
                    {
                        motion = new Vector2(0f, -0.1f),
                        scale = (float)Game1.pixelZoom,
                        layerDepth = 1f
                    });
                    who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(10, who.position + new Vector2((float)(Game1.tileSize / 2), (float)(-(float)Game1.tileSize * 3 / 2)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                    {
                        motion = new Vector2(0f, -0.1f)
                    });

                    return false; //we have handled it ourselves
                } else
                {
                    return true; //run the original method
                }
            }
        }
    }
}
