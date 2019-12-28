﻿using Aurora.Profiles.Generic;

namespace Aurora.Profiles.Osu.GSI {

    public class GameState_Osu : GameState<GameState_Osu> {

        private ProviderNode provider;
        public ProviderNode Provider => provider ?? (provider = new ProviderNode(_ParsedData["provider"]?.ToString() ?? ""));

        private GameNode game;
        public GameNode Game => game ?? (game = new GameNode(_ParsedData["game"]?.ToString() ?? ""));

        public GameState_Osu() : base() { }
        public GameState_Osu(string JSONstring) : base(JSONstring) { }
        public GameState_Osu(IGameState other) : base(other) { }
    }

    public class GameNode : Node<GameNode> {

        public OsuStatus StatusEnum;
        public string Status => StatusEnum.ToString(); // Only here for legacy reasons - don't wanna break any profiles that may now depend on this
        public OsuPlayMode PlayMode;
        public float HP;
        public float Accuracy;
        public int Combo;
        public int Count50;
        public int Count100;
        public int Count200;
        public int Count300;
        public int CountKatu;
        public int CountGeki;
        public int CountMiss;

        internal GameNode(string json) : base(json) {
            StatusEnum = GetEnum<OsuStatus>("status");
            PlayMode = GetEnum<OsuPlayMode>("playMode");
            HP = GetFloat("hp");
            Accuracy = GetFloat("accuracy");
            Combo = GetInt("combo");
            Count50 = GetInt("count50");
            Count100 = GetInt("count100");
            Count200 = GetInt("count200");
            Count300 = GetInt("count300");
            CountKatu = GetInt("countKatu");
            CountGeki = GetInt("countGeki");
            CountMiss = GetInt("countMiss");
        }
    }
}
