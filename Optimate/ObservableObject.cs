using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Optimate
{
    public abstract class ObservableObject : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool SuppressNotification = false;

        protected void RaisePropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            if (!SuppressNotification)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == null)
            {
                return null;
            }    
            if (this._errors.ContainsKey(propertyName))
                return this._errors[propertyName];
            return null;
        }

        public bool HasErrors
        {
            get { return (this._errors.Count > 0); }
        }

        public bool HasError(string propertyName)
        {
            if (this._errors.ContainsKey(propertyName))
                return true;
            else
                return false;
        }

        public void AddError(string propertyName, string error)
        {
            // Add error to list
            this._errors[propertyName] = new List<string>() { error };
            this.NotifyErrorsChanged(propertyName);
        }

        public void ClearErrors(string propertyName)
        {
            // Clear errors
            if (this._errors.ContainsKey(propertyName))
            {
                this._errors.Remove(propertyName);
            }
            this.NotifyErrorsChanged(propertyName);
        }

        public void RemoveError(string propertyName)
        {
            // remove error
            if (this._errors.ContainsKey(propertyName))
                this._errors.Remove(propertyName);
            this.NotifyErrorsChanged(propertyName);
        }

        public void NotifyErrorsChanged(string propertyName)
        {
            // Notify
            if (this.ErrorsChanged != null)
                this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

    }
}
