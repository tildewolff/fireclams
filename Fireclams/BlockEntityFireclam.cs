using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Fireclams;

public class BlockEntityFireclam : BlockEntity
{
    private double readyAtHour;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (api.Side == EnumAppSide.Server) RegisterGameTickListener(CheckGrowth, 1000);
    }

    public void StartGrowing()
    {
        readyAtHour = Api.World.Calendar.TotalHours + 24;
        MarkDirty();
    }

    public void Reset()
    {
        readyAtHour = 0;
        MarkDirty();
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        base.GetBlockInfo(forPlayer, dsc);
        if (Block is not BlockFireclam clam || clam.Variant["stage"] != "processing" || readyAtHour <= 0) return;

        int minutesLeft = (int)Math.Ceiling(Math.Max(0, readyAtHour - Api.World.Calendar.TotalHours) * 60);
        dsc.AppendLine(Lang.Get("fireclams:clam-time-remaining", minutesLeft / 60, minutesLeft % 60));
    }

    private void CheckGrowth(float dt)
    {
        if (readyAtHour <= 0 || Api.World.Calendar.TotalHours < readyAtHour) return;
        if (Api.World.BlockAccessor.GetBlock(Pos) is not BlockFireclam clam || clam.Variant["stage"] != "processing") return;

        string shell = clam.Variant["shell"];
        string metal = clam.Variant["metal"];
        Block ready = Api.World.GetBlock(new AssetLocation("fireclams", $"kalluclam-{shell}-ready-{metal}"));
        if (ready == null) return;

        readyAtHour = 0;
        Api.World.BlockAccessor.ExchangeBlock(ready.BlockId, Pos);
        MarkDirty();
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetDouble("readyAtHour", readyAtHour);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        readyAtHour = tree.GetDouble("readyAtHour");
    }
}
