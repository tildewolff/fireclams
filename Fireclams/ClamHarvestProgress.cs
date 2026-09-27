using System;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace Fireclams;

internal sealed class ClamHarvestProgress : IDisposable
{
    private readonly ICoreClientAPI api;
    private IProgressBar progressBar;

    public ClamHarvestProgress(ICoreClientAPI api)
    {
        this.api = api;
    }

    public void Show(float fraction)
    {
        ModSystemProgressBar bars = api.ModLoader.GetModSystem<ModSystemProgressBar>();
        if (bars == null) return;

        progressBar ??= bars.AddProgressbar();
        progressBar.Progress = Math.Clamp(fraction, 0, 1);
    }

    public void Hide()
    {
        if (progressBar == null) return;

        api.ModLoader.GetModSystem<ModSystemProgressBar>()?.RemoveProgressbar(progressBar);
        progressBar = null;
    }

    public void Dispose() => Hide();
}
