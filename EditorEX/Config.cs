using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace EditorEX
{
    internal class Config
    {
        public static Config Instance { get; set; }

#pragma warning disable CS8632
        public event Action<Config>? Updated;
#pragma warning restore CS8632

        public virtual bool HideEnvironmentSpectrograms { get; set; } = true;
        public virtual bool UseEnvironmentColors { get; set; } = true;

        public virtual void Changed()
        {
            Updated?.Invoke(this);
        }
    }
}
