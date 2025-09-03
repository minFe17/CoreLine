using System;

public static class PauseControl
{
    public static bool IsPaused { get; private set; }
    public static event Action<bool> OnChanged;

    public static void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;
        OnChanged?.Invoke(paused);
    }
}
