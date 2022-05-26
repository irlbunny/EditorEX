using System;
using Zenject;

namespace EditorEX.Managers
{
    internal class SpectrogramManager : IInitializable, IDisposable
    {
        private const string GROUPID = "Spectrogram";

        private readonly Config _config;
        private readonly EnvironmentGameObjectGroupManager _environmentGameObjectGroupManager;

        public SpectrogramManager(Config config, EnvironmentGameObjectGroupManager environmentGameObjectGroupManager)
        {
            _config = config;
            _environmentGameObjectGroupManager = environmentGameObjectGroupManager;
            _environmentGameObjectGroupManager.Add<Spectrogram>(GROUPID);
        }

        public void Initialize()
        {
            _config.Updated += Config_Updated;

            SetVisible(!_config.HideEnvironmentSpectrograms);
        }

        public void Dispose()
        {
            _config.Updated -= Config_Updated;

            SetVisible(true); // Reset to default state.
        }

        private void Config_Updated(Config config)
        {
            SetVisible(!config.HideEnvironmentSpectrograms);
        }

        public void SetVisible(bool visible)
        {
            _environmentGameObjectGroupManager.SetVisible(GROUPID, visible);
        }
    }
}
