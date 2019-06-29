using Aurora.EffectsEngine;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.Desktop {
    public class Event_Desktop : LightEvent
    {
        public Event_Desktop() : base()
        {
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> getQueue(IEnumerable<Layer> src) {
                return new Queue<EffectLayer>(src
                    .Where(l => l.Enabled)
                    .Reverse()
                    .Select(l => l.Render(_game_state))
                );
            }
            var layers = getQueue(Application.Profile.Layers);
            var overlayLayers = getQueue(Application.Profile.OverlayLayers);

            //Scripts before interactive and shortcut assistant layers
            //ProfilesManager.DesktopProfile.UpdateEffectScripts(layers);

            frame.AddLayers(layers.ToArray());
            frame.AddOverlayLayers(overlayLayers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state)
        {
            
        }

        public new bool IsEnabled
        {
            get { return true; }
        }
    }
}
