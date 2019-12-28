using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Generic {

    public class ProviderNode : Node<ProviderNode> {

        public string Name;
        public int AppID;

        public ProviderNode(string json) : base(json) {
            Name = GetString("name");
            AppID = GetInt("appid");
        }
    }
}
