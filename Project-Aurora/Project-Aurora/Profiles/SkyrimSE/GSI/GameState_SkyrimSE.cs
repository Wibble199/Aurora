using Aurora.Profiles.SkyrimSE.GSI.Nodes;

namespace Aurora.Profiles.SkyrimSE.GSI {

    public class GameState_SkyrimSE : GameState<GameState_SkyrimSE> {

        public PlayerNode Player => NodeFor<PlayerNode>("Player");

        public GameState_SkyrimSE() : base() { }
        public GameState_SkyrimSE(string json) : base(json) { }
        public GameState_SkyrimSE(IGameState other) : base(other) { }
    }
}
