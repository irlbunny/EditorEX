using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace EditorEX
{
    internal class Config
    {
#pragma warning disable CS8632
        public event Action<Config>? Updated;
#pragma warning restore CS8632

        public virtual bool HideEnvironmentSpectrograms { get; set; } = true;

        public virtual bool UseColorScheme { get; set; } = true;

        public virtual bool ShowSpectrogram { get; set; } = true;

        public virtual float SpectrogramXOffset { get; set; } = 1f;
        public virtual float SpectrogramWidth { get; set; } = .7f;

        public virtual void Changed()
        {
            Updated?.Invoke(this);
        }
    }
}
