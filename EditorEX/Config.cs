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

        public virtual void Changed()
        {
            Updated?.Invoke(this);
        }
    }
}
