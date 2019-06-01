using Aurora.Profiles;
using Aurora.Utils;
using Castle.DynamicProxy;
using System.ComponentModel;

namespace Aurora.Settings {

    public interface ICanNotifyPropertyChanged {
        void NotifyPropertyChanged(string propertyName);
    }

    [HasProxyInterceptors(typeof(NotifyChangedInterceptor))]
    public class AutoNotifyPropertyChanged<TSelf> : ICanNotifyPropertyChanged, INotifyPropertyChanged where TSelf : AutoNotifyPropertyChanged<TSelf> {

        public event PropertyChangedEventHandler PropertyChanged;

        public static TSelf Create() =>
            Global.ProxyGenerator.CreateClassProxy<TSelf>(new NotifyChangedInterceptor());

        public void NotifyPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public class NotifyChangedInterceptor : IInterceptor {
        public void Intercept(IInvocation invocation) {

            // Run the normal method
            invocation.Proceed();

            // If the method was a setter (indicated by being called "set_xxx")...
            if (invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("set_") && invocation.InvocationTarget is ICanNotifyPropertyChanged target) {
                // Call the notify property changed event
                target.NotifyPropertyChanged(invocation.Method.Name.Substring(4));

                // If the set value is a INotifyPropertyChanged or a INotifyCollectionChanged, add a listener to the new value to bubble the event to the invocation target
            }
        }
    }
}
