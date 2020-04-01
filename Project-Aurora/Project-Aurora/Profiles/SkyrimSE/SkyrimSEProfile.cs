using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.ObjectModel;
using System.Drawing;
using static Aurora.Devices.DeviceKeys;

namespace Aurora.Profiles.SkyrimSE {

    public class SkyrimSEProfile : ApplicationProfile {
        public override void Reset() {
            base.Reset();
            Layers = new ObservableCollection<Layer> {
                new Layer("Magicka Bar", new PercentLayerHandler {
                    Properties = new PercentLayerHandlerProperties {
                        _VariablePath = "Player/Magicka",
                        _MaxVariablePath = "Player/MagickaMax",
                        _PrimaryColor = Color.FromArgb(30, 100, 215),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(F1, F2, F3, F4)
                    }
                }),
                new Layer("Health Bar", new PercentLayerHandler {
                    Properties = new PercentLayerHandlerProperties {
                        _VariablePath = "Player/Health",
                        _MaxVariablePath = "Player/HealthMax",
                        _PrimaryColor = Color.FromArgb(255, 0, 0),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(F5, F6, F7, F8)
                    }
                }),
                new Layer("Stamina Bar", new PercentLayerHandler {
                    Properties = new PercentLayerHandlerProperties {
                        _VariablePath = "Player/Stamina",
                        _MaxVariablePath = "Player/StaminaMax",
                        _PrimaryColor = Color.FromArgb(40, 170, 70),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(F9, F10, F11, F12)
                    }
                })
            };
        }
    }
}
