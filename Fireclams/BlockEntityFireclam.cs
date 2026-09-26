using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Fireclams;

public class BlockEntityFireclam : BlockEntity
{
    private double readyAtHour;
    private double pausedAtHour;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (api.Side == EnumAppSide.Server) RegisterGameTickListener(CheckGrowth, 1000);
    }

    public void StartGrowing()
    {
        readyAtHour = Api.World.Calendar.TotalHours + 24;
        pausedAtHour = 0;
        MarkDirty();
    }

    public void Reset()
    {
        readyAtHour = 0;
        pausedAtHour = 0;
        MarkDirty();
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        base.GetBlockInfo(forPlayer, dsc);
        if (Block is not BlockFireclam clam) return;
        string stage = clam.Variant["stage"];
        if ((stage == "empty" || stage == "processing") && !clam.HasRequiredLiquid(Api.World, Pos))
        {
            dsc.AppendLine(Lang.Get(clam.Variant["shell"] == "clay" ? "fireclams:needs-lava" : "fireclams:needs-water"));
        }
        if (stage != "processing" || readyAtHour <= 0) return;

        double currentHour = pausedAtHour > 0 ? pausedAtHour : Api.World.Calendar.TotalHours;
        int minutesLeft = (int)Math.Ceiling(Math.Max(0, readyAtHour - currentHour) * 60);
        dsc.AppendLine(Lang.Get("fireclams:clam-time-remaining", minutesLeft / 60, minutesLeft % 60));
    }

    private void CheckGrowth(float dt)
    {
        if (Api.World.BlockAccessor.GetBlock(Pos) is not BlockFireclam clam) return;
        string stage = clam.Variant["stage"];
        string shell = clam.Variant["shell"];

        // Keep existing worlds usable if a non-clay clam was already growing a metal pearl.
        if (shell != "clay" && stage != "empty" && clam.Variant["metal"] != "pearl")
        {
            Block regular = Api.World.GetBlock(new AssetLocation("fireclams", $"kalluclam-{shell}-{stage}-pearl"));
            if (regular is BlockFireclam regularClam)
            {
                Api.World.BlockAccessor.ExchangeBlock(regular.BlockId, Pos);
                clam = regularClam;
            }
        }

        if (readyAtHour <= 0) return;
        if (stage != "processing") return;

        double now = Api.World.Calendar.TotalHours;
        if (!clam.HasRequiredLiquid(Api.World, Pos))
        {
            if (pausedAtHour == 0)
            {
                pausedAtHour = now;
                MarkDirty();
            }
            return;
        }

        if (pausedAtHour > 0)
        {
            readyAtHour += now - pausedAtHour;
            pausedAtHour = 0;
            MarkDirty();
        }
        if (now < readyAtHour) return;

        string metal = clam.Variant["metal"];
        Block ready = Api.World.GetBlock(new AssetLocation("fireclams", $"kalluclam-{shell}-ready-{metal}"));
        if (ready == null) return;

        readyAtHour = 0;
        pausedAtHour = 0;
        Api.World.BlockAccessor.ExchangeBlock(ready.BlockId, Pos);
        MarkDirty();
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetDouble("readyAtHour", readyAtHour);
        tree.SetDouble("pausedAtHour", pausedAtHour);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        readyAtHour = tree.GetDouble("readyAtHour");
        pausedAtHour = tree.GetDouble("pausedAtHour");
    }
}
