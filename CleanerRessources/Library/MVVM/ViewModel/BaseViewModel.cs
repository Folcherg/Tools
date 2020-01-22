using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Library.MVVM.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private Dictionary<string, object> __storage = new Dictionary<string, object>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="_default">Default value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns></returns>
        protected T Get<T>(T _default, [CallerMemberName] string propertyName = null)
            => Get(() => _default, propertyName);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="_default">Default value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns></returns>
        protected T Get<T>(Func<T> _default = null, [CallerMemberName] string propertyName = null)
        {
            if (!__storage.ContainsKey(propertyName))
                __storage[propertyName] = _default == null ? default(T) : _default();
            //return _default == null ? default(T) : _default();

            return (T)__storage[propertyName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool Set<T>(T value, [CallerMemberName] string propertyName = null)
            => Set(value, (Action)null, propertyName);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="onPropertyChanged"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool Set<T>(T value, Action onPropertyChanged, [CallerMemberName] string propertyName = null)
            => Set(value, (o, n) => onPropertyChanged?.Invoke(), propertyName);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="onPropertyChanged"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool Set<T>(T value, Action<T, T> onPropertyChanged, [CallerMemberName] string propertyName = null)
        {
            if (!__storage.ContainsKey(propertyName) || !Equals(__storage[propertyName], value))
            {
                var oldValue = !__storage.ContainsKey(propertyName) ? default(T) : (T)__storage[propertyName];
                __storage[propertyName] = value;

                onPropertyChanged?.Invoke(oldValue, value);
                ChangedProperty(propertyName);

                return true;
            }
            return false;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void ChangedProperty([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
