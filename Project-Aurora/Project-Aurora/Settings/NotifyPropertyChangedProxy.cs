using Aurora.Profiles;
using Aurora.Utils;
using Castle.DynamicProxy;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Aurora.Settings {
    
    /// <summary>
    /// Interceptor for any <see cref="ICanNotifyPropertyChanged"/> classes which will automatically invoke the PropertyChanged event though
    /// the exposed <see cref="ICanNotifyPropertyChanged.NotifyPropertyChanged(string)"/> method whenever any of the virtual properties
    /// have their setters called.
    /// To use this, all properties should be marked with the virtual modifier.
    /// </summary>
    public class NotifyChangedInterceptor : IInterceptor {

        public void Intercept(IInvocation invocation) {

            // Run the normal method
            invocation.Proceed();

            // If the method was a setter (indicated by being called "set_xxx")...
            if (invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("set_") && invocation.InvocationTarget is ICanNotifyPropertyChanged target) {
                // Call the notify property changed event
                var propName = invocation.Method.Name.Substring(4);
                target.NotifyPropertyChanged(propName);

                // If the set value is a INotifyPropertyChanged or a INotifyCollectionChanged, add a listener to the new value to bubble the event to the invocation target
                var propType = invocation.Method.GetParameters()[0].ParameterType; // Setters will have a single param. The value may be null so we get the typedef from the method metainfo
                var propVal = invocation.Arguments[0];

                if (propVal != null && typeof(INotifyPropertyChanged).IsAssignableFrom(propType))
                    ((INotifyPropertyChanged)propVal).PropertyChanged += (sender, e) => target.NotifyPropertyChanged(propName);

                if (propVal != null && typeof(INotifyCollectionChanged).IsAssignableFrom(propType))
                    ((INotifyCollectionChanged)propVal).CollectionChanged += (sender, e) => target.NotifyPropertyChanged(propName);
            }
        }
    }


    /// <summary>
    /// Can be extended by classes to provide automatic <see cref="INotifyPropertyChanged.PropertyChanged"/> invocation. Properties declared
    /// in classes extending this should be marked as virtual so they can be intercepted by Castle. Whenever the setter for any of these
    /// properties is called, it will invoke PropertyChanged automatically.
    /// </summary>
    /// <typeparam name="TSelf">The self type, allowing the Create method to return the relevant class type.</typeparam>
    [HasProxyInterceptors(typeof(NotifyChangedInterceptor))]
    public class AutoNotifyPropertyChanged<TSelf> : ICanNotifyPropertyChanged, INotifyPropertyChanged where TSelf : AutoNotifyPropertyChanged<TSelf> {

        /// <summary>Event that is called whenever any of the properties in this class are changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Creates a new instance of this class using the ProxyGenerator. This method should be used instead of `new`.</summary>
        public static TSelf Create() =>
            Global.ProxyGenerator.CreateClassProxy<TSelf>(new NotifyChangedInterceptor());

        /// <summary>Method that allows external invocation of the <see cref="PropertyChanged"/> event (so it can be utilised by the interceptor).</summary>
        public void NotifyPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    /// <summary>
    /// Implementing classes should expose the ability to invoke their <see cref="INotifyPropertyChanged.PropertyChanged"/> event though the
    /// use of the NotifyPropertyChanged method. This allows it to be utilised by systems such as the automatic notify proxy interceptor.
    /// </summary>
    public interface ICanNotifyPropertyChanged : INotifyPropertyChanged {
        void NotifyPropertyChanged(string propertyName);
    }
}
