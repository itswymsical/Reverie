using Reverie.Common.Items.Types;
using Reverie.Common.UI.FlowerSatchel;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Common.UI.RelicUI;

public class RelicSystem : ModSystem
{
    private UserInterface relicInterface;
    private RelicUIState relicUI;

    public override void Load()
    {
        if (!Main.dedServ)
        {
            relicUI = new RelicUIState();
            relicUI.Activate();
            relicInterface = new UserInterface();
            relicInterface.SetState(relicUI);
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (relicInterface?.CurrentState != null)
        {
            relicInterface.Update(gameTime);
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (inventoryIndex != -1)
        {
            layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                "Reverie: Relic Slot",
                delegate
                {
                    if (Main.playerInventory)
                    {
                        relicInterface?.Draw(Main.spriteBatch, new GameTime());
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}

internal class RelicUIState : UIState
{
    private RelicSlotUI relicSlot;
    private bool slotInitialized;

    public override void OnInitialize()
    {
        Width = Height = StyleDimension.Fill;
    }

    public override void Update(GameTime gameTime)
    {
        if (!Main.playerInventory)
        {
            if (slotInitialized)
            {
                RemoveAllChildren();
                slotInitialized = false;
            }
            return;
        }

        if (!slotInitialized)
        {
            var relicPlayer = Main.LocalPlayer.GetModPlayer<RelicPlayer>();
            relicSlot = new RelicSlotUI(relicPlayer.relicSlot, 0)
            {
                Left = new StyleDimension(16, 0),
                Top = new StyleDimension(Main.screenHeight / 3.25f, 0),
                Width = StyleDimension.FromPixels(52),
                Height = StyleDimension.FromPixels(52)
            };
            Append(relicSlot);
            slotInitialized = true;
        }

        base.Update(gameTime);
    }
}

internal class RelicSlotUI : BasicItemSlot
{
    public RelicSlotUI(Item[] items, int index)
        : base(items, index, ItemSlot.Context.InventoryItem, 1f)
    {
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var player = Main.LocalPlayer;
        var relicPlayer = player?.GetModPlayer<RelicPlayer>();

        if (relicPlayer == null) return;

        DrawSlot(spriteBatch);

        ref var item = ref _items[_index];

        if (!item.IsAir)
        {
            DrawItemOnly(spriteBatch, ref item);
        }

        if (IsMouseHovering)
            SlotLogic(ref item);

        UpdateEquippedRelic(relicPlayer);
    }

    protected override void SlotLogic(ref Item item)
    {
        Main.LocalPlayer.mouseInterface = true;

        if (item.IsAir)
        {
            Main.hoverItemName = "Relic Slot";
            Main.mouseText = true;
        }
        else
        {
            ItemSlot.MouseHover(ref item, _context);
        }

        if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            var heldItem = Main.mouseItem;

            if (heldItem.IsAir || ValidItemForSlot(heldItem))
            {
                ItemSlot.LeftClick(ref item, _context);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
    }

    private void DrawSlot(SpriteBatch spriteBatch)
    {
        var center = GetDimensions().Center();
        Main.spriteBatch.Draw(
            ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}RelicUI/RelicSlot").Value,
            new Rectangle((int)center.X - 24, (int)center.Y - 24, 52, 52),
            Color.White
        );
    }

    private void DrawItemOnly(SpriteBatch spriteBatch, ref Item item)
    {
        var oldScale = Main.inventoryScale;
        Main.inventoryScale = Scale;

        var position = GetDimensions().ToRectangle().TopLeft();
        var texture = TextureAssets.Item[item.type].Value;
        var frame = Main.itemAnimations[item.type] != null
            ? Main.itemAnimations[item.type].GetFrame(texture)
            : texture.Frame();

        var drawPos = position + new Vector2(28) - frame.Size() * Scale * 0.5f;
        var color = item.GetAlpha(Color.White);

        spriteBatch.Draw(texture, drawPos, frame, color, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

        if (item.stack > 1)
        {
            Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value,
                item.stack.ToString(), drawPos.X + 10, drawPos.Y + 26,
                Color.White, Color.Black, Vector2.Zero, Scale);
        }

        Main.inventoryScale = oldScale;
    }

    private void UpdateEquippedRelic(RelicPlayer relicPlayer)
    {
        var item = _items[_index];

        if (!item.IsAir && item.ModItem is RelicItem relic)
        {
            relicPlayer.CurrentRelic = relic;
        }
        else
        {
            relicPlayer.CurrentRelic = null;
        }
    }

    public override bool ValidItemForSlot(Item item)
    {
        return item.ModItem is RelicItem;
    }
}

public class RelicPlayer : ModPlayer
{
    public RelicItem CurrentRelic { get; set; }
    public Item[] relicSlot = new Item[1];

    // Percentage of player XP that goes to relic (0.0 to 1.0)
    public virtual float RelicXPShare => 0.25f;

    public override void Initialize()
    {
        relicSlot[0] = new Item();
        CurrentRelic = null;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["relicSlot"] = relicSlot[0];
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("relicSlot"))
        {
            relicSlot[0] = tag.Get<Item>("relicSlot");
        }

        if (!relicSlot[0].IsAir && relicSlot[0].ModItem is RelicItem relic)
        {
            CurrentRelic = relic;
        }
    }

    public override void PostUpdateEquips()
    {
        if (CurrentRelic != null && !relicSlot[0].IsAir)
        {
            CurrentRelic.UpdateRelicEffects(Player);
        }
    }

    public override void OnEnterWorld()
    {
        if (!relicSlot[0].IsAir && relicSlot[0].ModItem is RelicItem relic)
        {
            CurrentRelic = relic;
        }
        else
        {
            CurrentRelic = null;
        }
    }

    public void ShareExperienceWithRelic(int playerXP)
    {
        if (CurrentRelic == null || !CurrentRelic.CanGainXP()) return;

        // Calculate relic XP based on share percentage
        int relicXP = (int)(playerXP * RelicXPShare);

        if (relicXP > 0)
        {
            var oldLevel = CurrentRelic.RelicLevel;
            CurrentRelic.AddXP(relicXP);

            // Play sound if relic leveled up
            if (CurrentRelic.RelicLevel > oldLevel)
            {
                SoundEngine.PlaySound(SoundID.Item4, Player.position);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Main.NewText($"Your {relicSlot[0].Name} reached level {CurrentRelic.RelicLevel}!", Color.Gold);
                }
            }
        }
    }
}