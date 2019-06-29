using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Settings;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.Generic_Application
{
    public class Event_GenericApplication : GameEvent_Generic
    {
        public Event_GenericApplication()
        {
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            GenericApplicationProfile settings = (GenericApplicationProfile)this.Application.Profile;

            //Scripts
            //this.Application.UpdateEffectScripts(layers);

            foreach (var layer in settings.Layers.Reverse().ToArray())
            {
                if (layer.Enabled)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, Application profile)
        {
            this.Application = profile;

            UpdateLights(frame);
        }
    }
}
