namespace Aurora.Profiles.SkyrimSE.GSI.Nodes {

    public class SkillsNode : AutoJsonNode<SkillsNode> {

        public float Alchemy;
        public float Alteration;
        public float Archery;
        public float Block;
        public float Conjuration;
        public float Destruction;
        public float Enchanting;
        public float HeavyArmor;
        public float Illusion;
        public float LightArmor;
        public float Lockpicking;
        public float OneHanded;
        public float Pickpocket;
        public float Restoration;
        public float Smithing;
        public float Sneak;
        public float Speech;
        public float TwoHanded;

        internal SkillsNode(string json) : base(json) { }
    }
}
