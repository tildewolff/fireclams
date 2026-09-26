using Vintagestory.API.Common;

namespace Fireclams;

public class FireclamsModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("Fireclam", typeof(BlockFireclam));
        api.RegisterBlockEntityClass("Fireclam", typeof(BlockEntityFireclam));
    }
}
