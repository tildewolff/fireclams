using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Fireclams;

public class FireclamsModSystem : ModSystem
{
    internal static ClamHarvestProgress HarvestProgress { get; private set; }

    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("Fireclam", typeof(BlockFireclam));
        api.RegisterBlockEntityClass("Fireclam", typeof(BlockEntityFireclam));
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        HarvestProgress = new ClamHarvestProgress(api);
    }

    public override void Dispose()
    {
        HarvestProgress?.Dispose();
        HarvestProgress = null;
    }
}
