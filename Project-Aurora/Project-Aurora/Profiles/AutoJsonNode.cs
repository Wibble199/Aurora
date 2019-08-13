using Castle.DynamicProxy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles {

    /// <summary>
    /// A specialised <see cref="Node{TClass}"/> that will intercept any calls to properties and fetch them on-demand from the JSON.
    /// Will not serialize JSON until it is requested, meaning that any properties the user does not use are not serialized.
    /// </summary>
    public class AutoJsonNode<TSelf> : Node<TSelf>, IAutoJsonNode where TSelf : Node<TSelf> {
        public Dictionary<string, object> PropValues { get; } = new Dictionary<string, object>();

        public JObject JObject { get => _ParsedData; set => _ParsedData = value; }

        public void SetJObject(string json) {
            PropValues.Clear();
            JObject = JObject.Parse(json);
        }

        public object ReadValue(string name) {
            // Check the property exists
            if (!PropertyLookup.TryGetValue(name, out var member))
                return null;

            // Check the token exists in the JSON
            var token = JObject.GetValue(name, StringComparison.InvariantCultureIgnoreCase);
            if (token == null)
                return null;

            // If the property type is a Node<T> type, instantiate and setup lazy loading for this
            if (typeof(IAutoJsonNode).IsAssignableFrom(member.MemberType) && token is JObject jObj) {
                var node = (IAutoJsonNode)Global.ProxyGenerator.CreateClassProxy(member.MemberType, new LazyJsonNodeInterceptor());
                node.JObject = jObj;
                return node;
            }

            // If the type is IList<Node<T>>, create a collection of lazy nodes
            var iList = member.MemberType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            var iListT = iList?.GetGenericArguments()[0];
            if (iList != null && typeof(IAutoJsonNode).IsAssignableFrom(iListT) && token is JArray jArr) {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(iListT));
                foreach (var item in jArr.Where(x => x is JObject)) {
                    var node = (IAutoJsonNode)Global.ProxyGenerator.CreateClassProxy(iListT, new LazyJsonNodeInterceptor());
                    node.JObject = (JObject)item;
                    list.Add(node);
                }
                return list;
            }

            // If the target type is neither Node<T> nor IList<Node<T>>, convert it to the relevant object.
            return JObject.GetValue(name).ToObject(member.MemberType);
        }
    }

    /// <summary>
    /// Defines the typeless interface for the AutoJsonNode.
    /// </summary>
    interface IAutoJsonNode {
        Dictionary<string, object> PropValues { get; }
        object ReadValue(string name);
        JObject JObject { get; set; }
        void SetJObject(string json);
    }


    /// <summary>
    /// Interceptor that intercepts calls to the GameState variables and attempts to get them from the parsed JSON data. Once fetched, they
    /// will be stored and subsequent calls the getter will return the stored value. Will mean that any non-needed properties will not be
    /// evaluated.
    /// </summary>
    class LazyJsonNodeInterceptor : IInterceptor {
        public void Intercept(IInvocation invocation) {

            invocation.Proceed();

            var propName = invocation.Method.Name.Substring(4);
            if (invocation.InvocationTarget is IAutoJsonNode n && invocation.Method.IsSpecialName) {
                if (invocation.Method.Name.StartsWith("get_")) {
                    var @value = n.PropValues.ContainsKey(propName) ? n.PropValues[propName] : (n.PropValues[propName] = n.ReadValue(propName));
                    if (@value != null)
                        invocation.ReturnValue = @value;
                    // If the value was null, e.g. the desired property was not set in the JSON object, we don't override the return value. This means the value from the default getter is returned,
                    // which will gracefully handle value types not being able to be set to null and will also allow for specifying default values in the node class.
                } else if (invocation.Method.Name.StartsWith("set_"))
                    n.PropValues[propName] = invocation.Arguments[0];
            }
        }
    }
}
