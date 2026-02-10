using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RainmeterLayoutManager.Models
{
    /// <summary>
    /// View model for a skin and its variables in the layout binding dialog.
    /// </summary>
    public class SkinViewModel : INotifyPropertyChanged
    {
        public string SkinName { get; set; } = string.Empty;
        public ObservableCollection<VariableViewModel> Variables { get; set; } = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// View model for a single INI variable.
    /// </summary>
    public class VariableViewModel : INotifyPropertyChanged
    {
        private string _key = string.Empty;
        private string _value = string.Empty;
        private string _defaultValue = string.Empty;

        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The default value from the INI file (used for placeholder display).
        /// </summary>
        public string DefaultValue
        {
            get => _defaultValue;
            set
            {
                if (_defaultValue != value)
                {
                    _defaultValue = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PlaceholderText));
                }
            }
        }

        /// <summary>
        /// Placeholder text to show in the TextBox when no override is set.
        /// </summary>
        public string PlaceholderText => string.IsNullOrEmpty(_defaultValue)
            ? string.Empty : _defaultValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
