using System;
using Zenject;

namespace EditorEX.Managers
{
    internal class EnvironmentGameObjectManager : IInitializable, IDisposable
    {
        private const string SPECTROGRAM_GROUPID = "Spectrogram";

        private readonly Config _config;
        private readonly EnvironmentGameObjectGroupManager _environmentGameObjectGroupManager;

        public EnvironmentGameObjectManager(Config config, EnvironmentGameObjectGroupManager environmentGameObjectGroupManager)
        {
            _config = config;
            _environmentGameObjectGroupManager = environmentGameObjectGroupManager;
        }

        public void Initialize()
        {
            _config.Updated += Config_Updated;
            _environmentGameObjectGroupManager.Add<Spectrogram>(SPECTROGRAM_GROUPID);
            _environmentGameObjectGroupManager.SetVisible(SPECTROGRAM_GROUPID, !_config.HideEnvironmentSpectrograms);
        }

        public void Dispose()
        {
            _config.Updated -= Config_Updated;
            _environmentGameObjectGroupManager.SetVisible(SPECTROGRAM_GROUPID, true); // Reset to default state.
        }

        private void Config_Updated(Config config)
        {
            _environmentGameObjectGroupManager.SetVisible(SPECTROGRAM_GROUPID, !config.HideEnvironmentSpectrograms);
        }
    }
}
