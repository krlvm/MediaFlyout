/**
 * MIT License
 * 
 * Copyright (c) 2016 minami_SC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MediaFlyout.Styles;

namespace MediaFlyout
{
    public class ThemeDictionary : ResourceDictionary, INotifyPropertyChanged
    {
        private string themeName;
        public string ThemeName
        {
            get { return themeName; }
            set { themeName = value; OnPropertyChanged(); }
        }

        public new Uri Source
        {
            get { return base.Source; }
            set { base.Source = value; OnPropertyChanged(); }
        }

        #region
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    public class ThemeCollection : ObservableCollection<ThemeDictionary>
    {
        private IList<ThemeDictionary> _previousList;

        public ThemeCollection()
        {
            _previousList = new List<ThemeDictionary>();
            CollectionChanged += ThemeCollection_CollectionChanged;
        }

        private void ThemeCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in this._previousList)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
                _previousList.Clear();
            }


            if (e.OldItems != null)
            {
                foreach (ThemeDictionary item in e.OldItems)
                {
                    _previousList.Remove(item);
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (ThemeDictionary item in e.NewItems)
                {
                    _previousList.Add(item);
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var args = new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(args);
        }
    }

    public class ResourceDictionaryEx : ResourceDictionary
    {
        public ThemeCollection ThemeDictionaries { get; set; } = new ThemeCollection();

        public ResourceDictionaryEx()
        {
            ThemeDictionaries.CollectionChanged += ThemeDictionaries_CollectionChanged;
            ThemeChanged += ResourceDictionaryEx_ThemeChanged;
        }

        private void ThemeDictionaries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ChangeTheme();
        }

        private void ResourceDictionaryEx_ThemeChanged(object sender, EventArgs e)
        {
            ChangeTheme();
        }

        public void ChangeTheme()
        {
            switch (theme)
            {
                case ColorScheme.Light:
                    ChangeTheme(ColorScheme.Light.ToString());
                    break;
                case ColorScheme.Dark:
                default:
                    ChangeTheme(ColorScheme.Dark.ToString());
                    break;
            }
        }

        private void ChangeTheme(string themeName)
        {
            MergedDictionaries.Clear();
            var theme = ThemeDictionaries.OfType<ThemeDictionary>().FirstOrDefault(o => o.ThemeName == themeName);
            if (theme != null)
            {
                MergedDictionaries.Add(theme);
            }
        }

        private static ColorScheme? theme;
        public static ColorScheme? Theme
        {
            get { return theme; }
            set
            {
                if (theme == value) return;
                theme = value; 
                ThemeChanged?.Invoke(null, null);
            }
        }

        public static event EventHandler<EventArgs> ThemeChanged;
    }
}