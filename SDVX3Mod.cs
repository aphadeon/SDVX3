using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using StardewValley.Monsters;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace SDVX3
{
    public class SDVX3Mod : Mod
    {
        internal static SDVX3Mod instance;
        public static Texture2D texture;
        public const int itemID = 921;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            this.Monitor.Log("SDVX3 is loaded.");
            //always create paths like so: string path = Path.Combine(helper.DirectoryPath, "assets", "asset.xnb");

            //step -1: install code patches
            Patcher.InstallPatches();

            //step zero: try to add an item
            //both Game1.objectInformation and Game1.objectSpriteSheet are loaded by Game1.TranslateFields
            //add base object information
            Game1.objectInformation.Add(itemID, "CustomItem / 375 / -300 / Basic - 26 / CustomItem / A custom item that isn't loaded properly.");
            //load our custom texture
            texture = helper.Content.Load<Texture2D>(Path.Combine("Content", "test.png"));


            //step one: let's create a deliberate forage spot every day
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;

            //debug printy stuff for coords/map id
            ControlEvents.KeyReleased += ControlEvents_KeyReleased;

            //step two: let's attempt to create a custom crop
            this.Monitor.Log("Texture loaded: " + texture.Width);

            //step three: hookup custom serializer for SimpleItems
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
        }

        private void ControlEvents_KeyReleased(object sender, EventArgsKeyPressed e)
        {
            //debug, press V to max friendship with all aquaintences
            if(e.KeyPressed == Microsoft.Xna.Framework.Input.Keys.V)
            {
                int maxFriend = 10 * 250;
                foreach (NPC current in Utility.getAllCharacters())
                {
                    if (Game1.player.friendships.ContainsKey(current.name))
                    {
                        Game1.player.friendships[current.name][0] = maxFriend;
                    } else
                    {
                        Game1.player.friendships.Add(current.name, new int[4]);
                        Game1.player.friendships[current.name][0] = maxFriend;
                    }
                }
            }

            //debug, press Z to get location/position
            if (e.KeyPressed == Microsoft.Xna.Framework.Input.Keys.Z)
            {
                this.Monitor.Log("Map id: " + Game1.player.currentLocation.Map.Id);
                this.Monitor.Log("Map name: " + Game1.player.currentLocation.Name);
                //tiles are 64px, per Game1.tileSize
                this.Monitor.Log("Map position: " + Game1.player.position.X + " (" + (int)(Game1.player.position.X / Game1.tileSize) + "), " + Game1.player.position.Y + " (" + (int)(Game1.player.position.Y / Game1.tileSize) + ")");
            }
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            //a couple forage tests - these happen every day for now

            //try to spawn a forage in haley's house, just outside her room by the dresser:
            //HaleyHouse - 6, 15
            SpawnSimpleItem("HaleyHouse", 6, 15, "testObject", 0);

            //spawn another item on the farm for easy testing
            SpawnSimpleItem("Farm", 61, 15, "testObject", 4);

            //SpawnForageLike(Game1.getLocationFromName("HaleyHouse"), new Vector2(6, 15), "testItem", 4);
            //SpawnForageLike(Game1.getLocationFromName("Farm"), new Vector2(61, 15), "testItem", 0);
        }

        public bool SpawnSimpleItem(string location, int x, int y, string modItemId, int quality, bool destructive = false)
        {
            Spawnpoint spawn = new Spawnpoint(location, x, y);
            if (!spawn.isClear())
            {
                if (!destructive) return false;
                spawn.AttemptToClear();
            }
            SimpleObject item = SimpleItemFactory.CreateItem(spawn, modItemId, quality);
            spawn.PlaceItem(item);
            return true;
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            SaveEvents.AfterSave -= SaveEvents_AfterSave;
            SaveEvents.BeforeSave -= SaveEvents_BeforeSave;
            SaveEvents.AfterReturnToTitle -= SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            LoadAndAdd();
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            SaveAndRemove();
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterSave += SaveEvents_AfterSave;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
            LoadAndAdd();
        }

        //custom stuff crashes the serializer.
        //soooo. we serialize it ourselves, and remove it from play prior to saving.
        public void SaveAndRemove()
        {
            //check the world for placed items
            foreach (GameLocation location in Game1.locations)
            {
                //check placed objects
                Vector2[] keys = location.objects.Keys.ToArray<Vector2>();
                for(int ik = 0; ik < keys.Length; ik++)
                {
                    Vector2 key = keys[ik];
                    if (location.objects[key] is SimpleObject)
                    {
                        location.objects[key] = (location.objects[key] as SimpleObject).Pack();
                    }
                    else if (location.objects[key] is Chest)
                    {
                        for (int i = 0; i < (location.objects[key] as Chest).items.Count; i++)
                        {
                            if ((location.objects[key] as Chest).items[i] is SimpleObject)
                            {
                                (location.objects[key] as Chest).items[i] = ((location.objects[key] as Chest).items[i] as SimpleObject).Pack();
                            }
                        }
                    }
                }
                if (location is StardewValley.Locations.BuildableGameLocation bgl)
                {
                    foreach (StardewValley.Buildings.Building building in bgl.buildings)
                    {
                        if (building.indoors is GameLocation bl)
                        {
                            if (bl.objects is SerializableDictionary<Vector2, StardewValley.Object>)
                            {
                                keys = bl.objects.Keys.ToArray<Vector2>();
                                for (int ik = 0; ik < keys.Length; ik++)
                                {
                                    Vector2 key = keys[ik];
                                    if (location.objects[key] is SimpleObject)
                                    {
                                        location.objects[key] = (location.objects[key] as SimpleObject).Pack();
                                    }
                                    else if (location.objects[key] is Chest)
                                    {
                                        for (int i = 0; i < (location.objects[key] as Chest).items.Count; i++)
                                        {
                                            if ((location.objects[key] as Chest).items[i] is SimpleObject)
                                            {
                                                (location.objects[key] as Chest).items[i] = ((location.objects[key] as Chest).items[i] as SimpleObject).Pack();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (building is JunimoHut)
                        {
                            for (int i = 0; i < (building as JunimoHut).output.items.Count; i++) {
                                if((building as JunimoHut).output.items[i] is SimpleObject)
                                {
                                    (building as JunimoHut).output.items[i] = ((building as JunimoHut).output.items[i] as SimpleObject).Pack();
                                }
                            }
                        }

                        if (building is Mill)
                        {
                            for (int i = 0; i < (building as Mill).output.items.Count; i++)
                            {
                                if ((building as Mill).output.items[i] is SimpleObject)
                                {
                                    (building as Mill).output.items[i] = ((building as Mill).output.items[i] as SimpleObject).Pack();
                                }
                            }
                            for (int i = 0; i < (building as Mill).input.items.Count; i++)
                            {
                                if ((building as Mill).input.items[i] is SimpleObject)
                                {
                                    (building as Mill).input.items[i] = ((building as Mill).input.items[i] as SimpleObject).Pack();
                                }
                            }
                        }
                    }
                }
            }

            //check various inventories
            for(int i = 0; i < Game1.player.items.Count; i++)
            {
                if(Game1.player.items[i] is SimpleObject)
                {
                    Game1.player.items[i] = (Game1.player.items[i] as SimpleObject).Pack();
                }
            }
            for (int i = 0; i < (Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items.Count; i++)
            {
                if ((Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items[i] is SimpleObject)
                {
                    Game1.player.items[i] = ((Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items[i] as SimpleObject).Pack();
                }
            }
            for (int i = 0; i < (Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell.Count; i++)
            {
                if ((Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell[i] is SimpleObject)
                {
                    Game1.player.items[i] = ((Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell[i] as SimpleObject).Pack();
                }
            }
        }


        //now we reconstitute our simpleitem class from saved placeholder objects
        public void LoadAndAdd()
        {
            //check the world for placed items
            foreach (GameLocation location in Game1.locations)
            {
                //check placed objects
                Vector2[] keys = location.objects.Keys.ToArray<Vector2>();
                for (int ik = 0; ik < keys.Length; ik++)
                {
                    Vector2 key = keys[ik];
                    if (location.objects[key].name.Contains("SDVX3.SimpleObjects"))
                    {
                        location.objects[key] = SimpleObject.Unpack(location.objects[key]);
                    }
                    else if (location.objects[key] is Chest)
                    {
                        for (int i = 0; i < (location.objects[key] as Chest).items.Count; i++)
                        {
                            if (((location.objects[key] as Chest).items[i] is StardewValley.Object) && (((location.objects[key] as Chest).items[i] as StardewValley.Object).name.Contains("SDVX3.SimpleObjects")))
                            {
                                (location.objects[key] as Chest).items[i] = SimpleObject.Unpack((location.objects[key] as Chest).items[i] as StardewValley.Object);
                            }
                        }
                    }
                }
                if (location is StardewValley.Locations.BuildableGameLocation bgl)
                {
                    foreach (StardewValley.Buildings.Building building in bgl.buildings)
                    {
                        if (building.indoors is GameLocation bl)
                        {
                            if (bl.objects is SerializableDictionary<Vector2, StardewValley.Object>)
                            {
                                keys = bl.objects.Keys.ToArray<Vector2>();
                                for (int ik = 0; ik < keys.Length; ik++)
                                {
                                    Vector2 key = keys[ik];
                                    if (location.objects[key].name.Contains("SDVX3.SimpleObjects"))
                                    {
                                        location.objects[key] = SimpleObject.Unpack(location.objects[key]);
                                    }
                                    else if (location.objects[key] is Chest)
                                    {
                                        for (int i = 0; i < (location.objects[key] as Chest).items.Count; i++)
                                        {
                                            if (((location.objects[key] as Chest).items[i] is StardewValley.Object) && (((location.objects[key] as Chest).items[i] as StardewValley.Object).name.Contains("SDVX3.SimpleObjects")))
                                            {
                                                (location.objects[key] as Chest).items[i] = SimpleObject.Unpack((location.objects[key] as Chest).items[i] as StardewValley.Object);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (building is JunimoHut)
                        {
                            for (int i = 0; i < (building as JunimoHut).output.items.Count; i++)
                            {
                                if (((building as JunimoHut).output.items[i] is StardewValley.Object) && ((building as JunimoHut).output.items[i] as StardewValley.Object).name.Contains("SDVX3.SimpleObjects"))
                                {
                                    (building as JunimoHut).output.items[i] = SimpleObject.Unpack((building as JunimoHut).output.items[i] as StardewValley.Object);
                                }
                            }
                        }

                        if (building is Mill)
                        {
                            for (int i = 0; i < (building as Mill).output.items.Count; i++)
                            {
                                if (((building as Mill).output.items[i] is StardewValley.Object) && ((building as Mill).output.items[i] as StardewValley.Object).name.Contains("SDVX3.SimpleObjects"))
                                {
                                    (building as Mill).output.items[i] = SimpleObject.Unpack((building as Mill).output.items[i] as StardewValley.Object);
                                }
                            }
                            for (int i = 0; i < (building as Mill).input.items.Count; i++)
                            {
                                if (((building as Mill).input.items[i] is StardewValley.Object) && ((building as Mill).input.items[i] as StardewValley.Object).name.Contains("SDVX3.SimpleObjects"))
                                {
                                    (building as Mill).input.items[i] = SimpleObject.Unpack((building as Mill).input.items[i] as StardewValley.Object);
                                }
                            }
                        }
                    }
                }
            }

            //check various inventories
            for (int i = 0; i < Game1.player.items.Count; i++)
            {
                if (Game1.player.items[i] is StardewValley.Object && (Game1.player.items[i] as StardewValley.Object).name.Contains("SDVX3.SimpleObjects"))
                {
                    Game1.player.items[i] = SimpleObject.Unpack(Game1.player.items[i] as StardewValley.Object);
                }
            }
            for (int i = 0; i < (Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items.Count; i++)
            {
                if ((Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items[i] is StardewValley.Object && ((Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items[i] as StardewValley.Object).name.Contains("SDVX3.SimpleItems"))
                {
                    (Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items[i] = SimpleObject.Unpack((Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items[i] as StardewValley.Object);
                }
            }
            for (int i = 0; i < (Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell.Count; i++)
            {
                if ((Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell[i] is StardewValley.Object && ((Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell[i] as StardewValley.Object).name.Contains("SDVX3.SimpleItems"))
                {
                    (Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell[i] = SimpleObject.Unpack((Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell[i] as StardewValley.Object);
                }
            }

        }
    }
}
