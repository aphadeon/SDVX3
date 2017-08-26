using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Xml.Serialization;

namespace SDVX3
{
    // Wraps our already custom items in a custom subclass for more control over their functionality.
    [Serializable]
    public class SimpleObject : StardewValley.Object
    {
        public int placeholderId;
        public string ModItemId { get; set; } //cannot contain "::"
        public bool drawShadow = true;

        [XmlIgnore]
        public override string DisplayName
        {
            get
            {
                return SimpleItemFactory.GetDisplayName(ModItemId);
            }
            set
            {
                this.displayName = value;
            }
        }

        public SimpleObject() : base()
        {
            ModItemId = "";
            bigCraftable = false; //not supported
        } 

        //use this one - vanilla item is wood
        public SimpleObject(Vector2 tileLocation, string modItemId) : base(tileLocation, SDVX3Mod.itemID)
        {
            ModItemId = modItemId;
        }

        public override Color getCategoryColor(){
            return base.getCategoryColor();
        }

        public override string getCategoryName() { return base.getCategoryName(); }

        public override string getDescription() { return SimpleItemFactory.GetDescription(ModItemId); }

        public override bool canBePlacedInWater() { return false; }

        public override bool isPlaceable() { return false; }

        public virtual Rectangle GetSpriteSourceRect()
        {
            return new Rectangle(0, 0, 16, 16);
        }

        public override bool isActionable(StardewValley.Farmer who)
        {
            return this.checkForAction(who, true);
        }

        public override bool isPassable()
        {
            return true;
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            if (this.lightSource != null && this.isOn)
            {
                Game1.currentLightSources.Add(this.lightSource);
            }
            if (this.heldObject != null && this.heldObject.lightSource != null)
            {
                Game1.currentLightSources.Add(this.heldObject.lightSource);
            }
            if (this.shakeTimer > 0)
            {
                this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
                if (this.shakeTimer <= 0)
                {
                    this.health = 10;
                }
            }
            if (this.parentSheetIndex == 590 && Game1.random.NextDouble() < 0.01)
            {
                this.shakeTimer = 100;
            }
            /* errors
            if (this.bigCraftable && this.name.Equals("Slime Ball"))
            {
                this.parentSheetIndex = 56 + (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0);
            }
            */
        }

        public override bool canBeGivenAsGift()
        {
            return false;
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (!justCheckingForActivity && who != null && who.currentLocation.isObjectAt(who.getTileX(), who.getTileY() - 1) && who.currentLocation.isObjectAt(who.getTileX(), who.getTileY() + 1) && who.currentLocation.isObjectAt(who.getTileX() + 1, who.getTileY()) && who.currentLocation.isObjectAt(who.getTileX() - 1, who.getTileY()))
            {
                this.performToolAction(null);
            }
            if (justCheckingForActivity)
            {
                return true;
            } else
            {
                //perform a basic pickup
                if (who.IsMainPlayer && who.addItemToInventoryBool(this, true))
                {
                    who.currentLocation.objects.Remove(this.tileLocation);
                    Game1.playSound("coin");
                    tileLocation = Vector2.Zero;
                    return true;
                } else
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588", new object[0]));
                    return false;
                }
            }
        }

        public override Item getOne()
        {
            SimpleObject si = SimpleObject.Unpack(Pack());
            si.stack = 1;
            return si;
        }

        public virtual TemporaryAnimatedSprite getTemporarySpriteForAnimation(StardewValley.Farmer f)
        {
            var tas = new TemporaryAnimatedSprite(SDVX3Mod.texture, GetSpriteSourceRect(), 2500f, 1, 0, f.position + new Vector2(0f, (float)(-(float)Game1.tileSize * 2 + 4)), false, false, 1f, 0f, Color.White, (float)Game1.pixelZoom, 0, 0, 0);
            tas.motion = new Vector2(0f, -0.1f);
            return tas;
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            spriteBatch.Draw(SDVX3Mod.texture, objectPosition, GetSpriteSourceRect(), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 2) / 10000f));
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            if (drawShadow)
            {
                spriteBatch.Draw(Game1.shadowTexture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize * 3 / 4)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.5f, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            }
            spriteBatch.Draw(SDVX3Mod.texture, location + new Vector2((float)((int)((float)(Game1.tileSize / 2) * scaleSize)), (float)((int)((float)(Game1.tileSize / 2) * scaleSize))), GetSpriteSourceRect(), Color.White * transparency, 0f, new Vector2(8f, 8f) * scaleSize, (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);
            if (drawStackNumber && this.maximumStackSize() > 1 && (double)scaleSize > 0.3 && this.Stack != 2147483647 && this.Stack > 1)
            {
                Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, (float)Game1.tileSize - 18f * scaleSize + 2f), 3f * scaleSize, 1f, Color.White);
            }
            if (drawStackNumber && this.quality > 0)
            {
                float num = (this.quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * 3.1415926535897931 / 512.0) + 1f) * 0.05f);
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, (float)(Game1.tileSize - 12) + num), new Microsoft.Xna.Framework.Rectangle?((this.quality < 4) ? new Microsoft.Xna.Framework.Rectangle(338 + (this.quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8)), Color.White * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + num), SpriteEffects.None, layerDepth);
            }
            if (this.category == -22 && this.scale.Y < 1f)
            {
                spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)location.X, (int)(location.Y + (float)(Game1.tileSize - 2 * Game1.pixelZoom) * scaleSize), (int)((float)Game1.tileSize * scaleSize * this.scale.Y), (int)((float)(2 * Game1.pixelZoom) * scaleSize)), Utility.getRedToGreenLerpColor(this.scale.Y));
            }
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (!Game1.eventUp || (Game1.CurrentEvent != null && !Game1.CurrentEvent.isTileWalkedOn(x, y)))
            {
                if (drawShadow)
                {
                    spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize + Game1.tileSize / 2), (float)(y * Game1.tileSize + Game1.tileSize * 4 / 5 + Game1.pixelZoom))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)this.getBoundingBox(new Vector2((float)x, (float)y)).Bottom / 15000f);
                }
                Texture2D arg_6EE_1 = SDVX3Mod.texture;
                Vector2 arg_6EE_2 = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize + Game1.tileSize / 2 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * Game1.tileSize + Game1.tileSize / 2 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))));
                Microsoft.Xna.Framework.Rectangle? arg_6EE_3 = GetSpriteSourceRect();
                Color arg_6EE_4 = Color.White * alpha;
                float arg_6EE_5 = 0f;
                Vector2 arg_6EE_6 = new Vector2(8f, 8f);
                Vector2 arg_67B_0 = this.scale;
                spriteBatch.Draw(arg_6EE_1, arg_6EE_2, arg_6EE_3, arg_6EE_4, arg_6EE_5, arg_6EE_6, (this.scale.Y > 1f) ? this.getScale().Y : ((float)Game1.pixelZoom), this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.isPassable() ? this.getBoundingBox(new Vector2((float)x, (float)y)).Top : this.getBoundingBox(new Vector2((float)x, (float)y)).Bottom) / 10000f);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1f)
        {
            if (!Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn(xNonTile / Game1.tileSize, yNonTile / Game1.tileSize))
            {
                if (drawShadow)
                {
                    spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + Game1.tileSize / 2), (float)(yNonTile + Game1.tileSize * 4 / 5 + Game1.pixelZoom))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, layerDepth);
                }
                Texture2D arg_39B_1 = SDVX3Mod.texture;
                Vector2 arg_39B_2 = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + Game1.tileSize / 2 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(yNonTile + Game1.tileSize / 2 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))));
                Microsoft.Xna.Framework.Rectangle? arg_39B_3 = GetSpriteSourceRect();
                Color arg_39B_4 = Color.White * alpha;
                float arg_39B_5 = 0f;
                Vector2 arg_39B_6 = new Vector2(8f, 8f);
                Vector2 arg_367_0 = this.scale;
                spriteBatch.Draw(arg_39B_1, arg_39B_2, arg_39B_3, arg_39B_4, arg_39B_5, arg_39B_6, (this.scale.Y > 1f) ? this.getScale().Y : ((float)Game1.pixelZoom), this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
            }
        }

        /* todo? possibly?
        public override void drawAsProp(SpriteBatch b)
        {

        }
        */

        //return something to have it packed into the serialization along with ModItemId
        //can't use '::' in the metadata string, as it's used to tokenise the serialized data
        public virtual string GetMetadata()
        {
            return "";
        }

        //this is the inverse of the above, restore the data from the serialized string
        public virtual void ParseMetadata(string meta)
        {

        }

        //generate a vanilla copy holding all the vanilla variables
        //effectively a copy constructor for a vanilla object, operating on this SimpleItem instance.
        //this is what gets serialized in its place.
        public StardewValley.Object Pack()
        {
            StardewValley.Object o = new StardewValley.Object(placeholderId, 1, false);
            o.showNextIndex = showNextIndex;
            o.minutesUntilReady = minutesUntilReady;
            o.flipped = flipped;
            o.hasBeenPickedUpByFarmer = hasBeenPickedUpByFarmer;
            o.isRecipe = isRecipe;
            o.isLamp = isLamp;
            o.heldObject = heldObject; //we will need to revisit this if we make machines that can accept SimpleItems
            o.boundingBox = boundingBox;
            o.preservedParentSheetIndex = preservedParentSheetIndex;
            o.lightSource = lightSource;
            o.shakeTimer = shakeTimer;
            o.internalSound = internalSound;
            o.preserve = preserve;
            o.honeyType = honeyType;
            o.displayName = displayName;
            o.readyForHarvest = readyForHarvest;
            o.scale = scale;
            o.setIndoors = setIndoors;
            o.parentSheetIndex = parentSheetIndex;
            o.bigCraftable = bigCraftable;
            o.tileLocation = tileLocation;
            o.setOutdoors = setOutdoors;
            o.name = name;
            o.type = type;
            o.canBeSetDown = canBeSetDown;
            o.canBeGrabbed = canBeGrabbed;
            o.isHoedirt = isHoedirt;
            o.owner = owner;
            o.questItem = questItem;
            o.isOn = isOn;
            o.fragility = fragility;
            o.price = price;
            o.edibility = edibility;
            o.isSpawnedObject = isSpawnedObject;
            o.stack = stack;
            o.quality = quality;
            o.setHealth(getHealth());
            o.category = category;
            o.specialVariable = specialVariable;
            o.specialItem = specialItem;
            o.hasBeenInInventory = hasBeenInInventory;

            //override the substitute's id
            o.parentSheetIndex = placeholderId;
            //o.preservedParentSheetIndex = placeholderId;

            //pack our secret sauce
            if (String.IsNullOrEmpty(name)) name = "";
            o.name = name.Replace("::", "_") + "::SDVX3.SimpleObjects." + ModItemId + "::" + GetMetadata();

            o.bigCraftable = false; //not supported

            //return the result
            SDVX3Mod.instance.Monitor.Log("Packed a SimpleObject: " + ModItemId);
            return o;
        }

        //inverse of pack. we take a vanilla object and create a new SimpleItem from it.
        public static SimpleObject Unpack(StardewValley.Object o)
        {
            SimpleObject i = new SimpleObject();
            i.showNextIndex = o.showNextIndex;
            i.minutesUntilReady = o.minutesUntilReady;
            i.flipped = o.flipped;
            i.hasBeenPickedUpByFarmer = o.hasBeenPickedUpByFarmer;
            i.isRecipe = o.isRecipe;
            i.isLamp = o.isLamp;
            i.heldObject = o.heldObject; //we will need to revisit this if we make machines that can accept SimpleItems
            i.boundingBox = o.boundingBox;
            i.preservedParentSheetIndex = o.preservedParentSheetIndex;
            i.lightSource = o.lightSource;
            i.shakeTimer = o.shakeTimer;
            i.internalSound = o.internalSound;
            i.preserve = o.preserve;
            i.honeyType = o.honeyType;
            i.displayName = o.displayName;
            i.readyForHarvest = o.readyForHarvest;
            i.scale = o.scale;
            i.setIndoors = o.setIndoors;
            i.parentSheetIndex = o.parentSheetIndex;
            i.bigCraftable = o.bigCraftable;
            i.tileLocation = o.tileLocation;
            i.setOutdoors = o.setOutdoors;
            i.name = o.name;
            i.type = o.type;
            i.canBeSetDown = o.canBeSetDown;
            i.canBeGrabbed = o.canBeGrabbed;
            i.isHoedirt = o.isHoedirt;
            i.owner = o.owner;
            i.questItem = o.questItem;
            i.isOn = o.isOn;
            i.fragility = o.fragility;
            i.price = o.price;
            i.edibility = o.edibility;
            i.isSpawnedObject = o.isSpawnedObject;
            i.stack = o.stack;
            i.quality = o.quality;
            i.setHealth(o.getHealth());
            i.category = o.category;
            i.specialVariable = o.specialVariable;
            i.specialItem = o.specialItem;
            i.hasBeenInInventory = o.hasBeenInInventory;

            //override the substitute's id
            i.parentSheetIndex = SDVX3Mod.itemID;

            //unpack our secret sauce
            string[] meta = o.name.Split(new string[] { "::" }, StringSplitOptions.None);
            i.name = meta[0];
            i.ModItemId = meta[1].Split('.')[2];
            if(meta.Length > 2) i.ParseMetadata(meta[2]);;

            i.bigCraftable = false; //not supported

            //return the result
            SDVX3Mod.instance.Monitor.Log("Unpacked a SimpleObject: " + i.ModItemId);

            return i;
        }
    }
}
