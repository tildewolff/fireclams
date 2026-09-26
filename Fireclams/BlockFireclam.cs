using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Fireclams;

public class BlockFireclam : Block
{
    private const float HarvestSeconds = 1.25f;
    private static readonly HashSet<string> Metals = new(StringComparer.Ordinal)
    {
        "gold", "silver", "electrum", "blackbronze", "nickel",
        "meteoriciron", "copper", "uranium"
    };

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (blockSel == null) return false;

        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (Variant["stage"] == "empty" && TryGetInput(slot, out string pearlType))
        {
            if (world.Side == EnumAppSide.Server && HasRequiredLiquid(world, blockSel.Position)
                && world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                BlockEntityFireclam entity = world.BlockAccessor.GetBlockEntity<BlockEntityFireclam>(blockSel.Position);
                Block next = GetState(world, "processing", pearlType);
                if (entity != null && next != null)
                {
                    entity.StartGrowing();
                    world.BlockAccessor.ExchangeBlock(next.BlockId, blockSel.Position);
                    if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                    {
                        slot.TakeOut(1);
                        slot.MarkDirty();
                    }
                }
            }
            return true;
        }

        return Variant["stage"] == "ready" && IsKnife(slot);
    }

    public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (Variant["stage"] != "ready" || !IsKnife(byPlayer.InventoryManager.ActiveHotbarSlot)) return false;
        return secondsUsed < HarvestSeconds;
    }

    public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (secondsUsed < HarvestSeconds || blockSel == null || Variant["stage"] != "ready") return;
        if (!IsKnife(byPlayer.InventoryManager.ActiveHotbarSlot)) return;
        if (world.Side == EnumAppSide.Server && world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
        {
            Harvest(world, byPlayer, blockSel.Position);
        }
    }

    private void Harvest(IWorldAccessor world, IPlayer player, BlockPos pos)
    {
        if (world.BlockAccessor.GetBlock(pos).BlockId != BlockId) return;
        string pearlType = Variant["shell"] == "clay" ? Variant["metal"] : "pearl";
        Item pearl = world.GetItem(new AssetLocation("fireclams", "nurrupearl-" + pearlType));
        if (pearl == null) return;

        ItemStack stack = new(pearl);
        if (!player.InventoryManager.TryGiveItemstack(stack, true))
        {
            world.SpawnItemEntity(stack, pos);
        }

        if (world.Rand.NextDouble() < 0.5)
        {
            world.BlockAccessor.SetBlock(0, pos, BlockLayersAccess.Solid);
        }
        else
        {
            Block empty = GetState(world, "empty", "pearl");
            if (empty != null)
            {
                world.BlockAccessor.ExchangeBlock(empty.BlockId, pos);
                world.BlockAccessor.GetBlockEntity<BlockEntityFireclam>(pos)?.Reset();
            }
        }
    }

    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        Block empty = GetState(world, "empty", "pearl");
        return empty == null ? Array.Empty<ItemStack>() : new[] { new ItemStack(empty) };
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        Block empty = GetState(world, "empty", "pearl");
        return new ItemStack(empty ?? this);
    }

    private Block GetState(IWorldAccessor world, string stage, string metal)
    {
        return world.GetBlock(new AssetLocation("fireclams", $"kalluclam-{Variant["shell"]}-{stage}-{metal}"));
    }

    public override bool DisplacesLiquids(IBlockAccessor blockAccess, BlockPos pos) => false;

    public bool HasRequiredLiquid(IWorldAccessor world, BlockPos pos)
    {
        Block fluid = world.BlockAccessor.GetBlock(pos, BlockLayersAccess.Fluid);
        EnumBlockMaterial required = Variant["shell"] == "clay" ? EnumBlockMaterial.Lava : EnumBlockMaterial.Water;
        return fluid.IsLiquid() && fluid.BlockMaterial == required;
    }

    private bool TryGetInput(ItemSlot slot, out string pearlType)
    {
        if (Variant["shell"] == "clay") return TryGetMetal(slot, out pearlType);

        pearlType = "pearl";
        return slot?.Itemstack?.Block?.BlockMaterial == EnumBlockMaterial.Sand;
    }

    private static bool TryGetMetal(ItemSlot slot, out string metal)
    {
        metal = "";
        string code = slot?.Itemstack?.Collectible?.Code?.ToString() ?? "";
        const string prefix = "game:metalbit-";
        if (!code.StartsWith(prefix, StringComparison.Ordinal)) return false;
        metal = code[prefix.Length..];
        return Metals.Contains(metal);
    }

    private static bool IsKnife(ItemSlot slot) => slot?.Itemstack?.Collectible?.Tool == EnumTool.Knife;
}
