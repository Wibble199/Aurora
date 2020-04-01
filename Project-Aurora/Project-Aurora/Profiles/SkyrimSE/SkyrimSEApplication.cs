namespace Aurora.Profiles.SkyrimSE {

    public class SkyrimSEApplication : Application {

        public SkyrimSEApplication() : base(new LightEventConfig {
            Name = "Skyrim Special Edition",
            ID = "skyrimse",
            AppID = "489830",
            ProcessNames = new[] { "SkyrimSE.exe" },
            ProfileType = typeof(SkyrimSEProfile),
            OverviewControlType = typeof(Control_SkyrimSE),
            GameStateType = typeof(GSI.GameState_SkyrimSE),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/skyrim_se.png"
        }) { }
    }
}
