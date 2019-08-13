using System;
using Newtonsoft.Json.Linq;

namespace Aurora.Profiles {
    public class GameStateIgnoreAttribute : Attribute
    { }

    public class RangeAttribute : Attribute
    {
        public int Start { get; set; }

        public int End { get; set; }

        public RangeAttribute(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    /// <summary>
    /// A class representing various information retaining to the game.
    /// </summary>
    public interface IGameState {
        JObject _ParsedData { get; set; }
        string json { get; set; }
        string GetNode(string name);
    }

    public class GameState<T> : StringProperty<T>, IGameState where T : GameState<T>
    {
        private static LocalPCInformation _localpcinfo;

        /// <summary>
        /// Information about the local system
        /// </summary>
        public LocalPCInformation LocalPCInfo
        {
            get
            {
                if (_localpcinfo == null)
                    _localpcinfo = new LocalPCInformation();

                return _localpcinfo;
            }
        }

        public JObject _ParsedData { get; set; }
        public string json { get; set; }

        /// <summary>
        /// Creates a default GameState instance.
        /// </summary>
        public GameState() : base()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState(string json_data) : base()
        {
            if (String.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            json = json_data;
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json_data);
        }

        /// <summary>
        /// A copy constructor, creates a GameState instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState(IGameState other_state) : base()
        {
            _ParsedData = other_state._ParsedData;
            json = other_state.json;
        }

        public String GetNode(string name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(name, out value))
                return value.ToString();
            else
                return "";
        }

        /// <summary>
        /// Displays the JSON, representative of the GameState data
        /// </summary>
        /// <returns>JSON String</returns>
        public override string ToString()
        {
            return json;
        }
    }

    public class GameState : GameState<GameState>
    {
        public GameState() : base() { }
        public GameState(IGameState gs) : base(gs) { }
        public GameState(string json) : base(json) { }
    }
}
