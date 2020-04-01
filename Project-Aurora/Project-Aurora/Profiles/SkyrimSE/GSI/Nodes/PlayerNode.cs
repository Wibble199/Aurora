namespace Aurora.Profiles.SkyrimSE.GSI.Nodes {

    public class PlayerNode : AutoJsonNode<PlayerNode> {

        public int Level;
        public float Health;
        public float HealthMax;
        public float Stamina;
        public float StaminaMax;
        public float Magicka;
        public float MagickaMax;

        public SkillsNode Skills => NodeFor<SkillsNode>("Skills");

        internal PlayerNode(string json) : base(json) { }
    }
}
