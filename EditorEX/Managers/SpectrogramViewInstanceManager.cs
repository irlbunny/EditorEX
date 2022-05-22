using EditorEX.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.Managers
{
    internal class SpectrogramViewInstanceManager
    {
        public static SpectrogramView Instance { get; private set; }

        public SpectrogramViewInstanceManager(SpectrogramView spectrogramView)
        {
            Instance = spectrogramView;
        }
    }
}
