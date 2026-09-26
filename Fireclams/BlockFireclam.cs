using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Fireclams;

public class BlockFireclam : Block
{
    private const float HarvestSeconds = 1.25f;
    private static readonly string[] MetalOptions =
    {
        "gold", "silver", "electrum", "blackbronze", "nickel",
        "meteoriciron", "copper", "uranium"
    };
    private static readonly HashSet<string> Metals = new(MetalOptions, StringComparer.Ordinal);
    private WorldInteraction[] addInteraction;

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (blockSel == null) return false;

        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (Variant["stage"] == "empty" && TryGetInput(slot, out string pearlType))
        {
            if (world.Side == EnumAppSide.Server && HasRequiredMedium(world, blockSel.Position)
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
        Block empty = GetInventoryBlock(world);
        return empty == null ? Array.Empty<ItemStack>() : new[] { new ItemStack(empty) };
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        Block empty = GetInventoryBlock(world);
        return new ItemStack(empty ?? this);
    }

    private Block GetInventoryBlock(IWorldAccessor world)
    {
        return world.GetBlock(new AssetLocation("fireclams", $"kalluclamrot-{Variant["shell"]}-empty-pearl-north"));
    }

    internal Block GetState(IWorldAccessor world, string stage, string metal)
    {
        string shell = Variant["shell"];
        string code = Variant.TryGetValue("side", out string side)
            ? $"kalluclamrot-{shell}-{stage}-{metal}-{side}"
            : $"kalluclam-{shell}-{stage}-{metal}";
        return world.GetBlock(new AssetLocation("fireclams", code));
    }

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        // Existing saves may still contain the original, directionless clam items.
        if (!Variant.ContainsKey("side"))
        {
            Block oriented = GetInventoryBlock(world);
            if (oriented is BlockFireclam orientedClam)
            {
                return orientedClam.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            }
        }
        return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
    }

    public override bool DisplacesLiquids(IBlockAccessor blockAccess, BlockPos pos) => Variant["shell"] == "clay";

    public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref string failureCode)
    {
        if (Variant["shell"] == "clay")
        {
            Block fluid = world.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Fluid);
            if (fluid.IsLiquid() && fluid.LiquidCode == "lava")
            {
                failureCode = "generic";
                return false;
            }
        }
        return base.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode);
    }

    public bool HasRequiredMedium(IWorldAccessor world, BlockPos pos)
    {
        if (Variant["shell"] != "clay")
        {
            Block saltwater = world.BlockAccessor.GetBlock(pos, BlockLayersAccess.Fluid);
            return saltwater.IsLiquid() && saltwater.LiquidCode == "saltwater";
        }

        if (world.BlockAccessor.GetBlock(pos, BlockLayersAccess.Fluid).IsLiquid()) return false;
        foreach (BlockFacing face in BlockFacing.ALLFACES)
        {
            BlockPos checkPos = pos.AddCopy(face);
            for (int distance = 1; distance <= 2; distance++)
            {
                if (IsLavaAt(world, checkPos)) return true;
                checkPos = checkPos.AddCopy(face);
            }
        }

        foreach (BlockFacing face in BlockFacing.HORIZONTALS)
        {
            BlockPos side = pos.AddCopy(face);
            if (IsLavaAt(world, side.AddCopy(0, 1, 0)) || IsLavaAt(world, side.AddCopy(0, -1, 0))) return true;
        }
        return false;
    }

    private static bool IsLavaAt(IWorldAccessor world, BlockPos pos)
    {
        Block fluid = world.BlockAccessor.GetBlock(pos, BlockLayersAccess.Fluid);
        return fluid.IsLiquid() && fluid.LiquidCode == "lava";
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
    {
        if (Variant["stage"] != "empty") return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        if (addInteraction != null) return addInteraction;

        List<ItemStack> options = new();
        bool clay = Variant["shell"] == "clay";
        if (clay)
        {
            foreach (string metal in MetalOptions)
            {
                Item bit = world.GetItem(new AssetLocation("game", "metalbit-" + metal));
                if (bit != null) options.Add(new ItemStack(bit));
            }
        }
        else
        {
            foreach (Block block in world.Blocks)
            {
                if (block?.Code != null && block.BlockMaterial == EnumBlockMaterial.Sand)
                {
                    options.Add(new ItemStack(block));
                }
            }
        }

        addInteraction = new[]
        {
            new WorldInteraction
            {
                ActionLangCode = clay ? "fireclams:blockhelp-add-metalbit" : "fireclams:blockhelp-add-sand",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = options.ToArray()
            }
        };
        return addInteraction;
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
