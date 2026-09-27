using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Fireclams;

internal sealed class ClamHarvestProgress : IDisposable
{
    private readonly ICoreClientAPI api;
    private readonly long tickListenerId;
    private IProgressBar progressBar;
    private BlockPos targetPos;

    public ClamHarvestProgress(ICoreClientAPI api)
    {
        this.api = api;
        tickListenerId = api.Event.RegisterGameTickListener(CheckHarvest, 50);
    }

    public void Show(float fraction, BlockPos position)
    {
        if (targetPos == null || !targetPos.Equals(position)) targetPos = position.Copy();

        ModSystemProgressBar bars = api.ModLoader.GetModSystem<ModSystemProgressBar>();
        if (bars == null) return;

        progressBar ??= bars.AddProgressbar();
        progressBar.Progress = Math.Clamp(fraction, 0, 1);
    }

    public void Hide()
    {
        if (progressBar != null)
        {
            api.ModLoader.GetModSystem<ModSystemProgressBar>()?.RemoveProgressbar(progressBar);
        }
        progressBar = null;
        targetPos = null;
        api.World?.Player?.Entity?.StopAnimation("knifecut");
    }

    private void CheckHarvest(float dt)
    {
        if (targetPos == null) return;

        IClientPlayer player = api.World?.Player;
        BlockPos currentTarget = player?.CurrentBlockSelection?.Position;
        if (!api.Input.InWorldMouseButton.Right
            || currentTarget == null || !currentTarget.Equals(targetPos)
            || player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible?.Tool != EnumTool.Knife
            || api.World.BlockAccessor.GetBlock(targetPos) is not BlockFireclam clam
            || clam.Variant["stage"] != "ready")
        {
            Hide();
        }
    }

    public void Dispose()
    {
        Hide();
        api.Event.UnregisterGameTickListener(tickListenerId);
    }
}
