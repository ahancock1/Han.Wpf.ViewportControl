// -----------------------------------------------------------------------
//   Copyright (C) 2018 Adam Hancock
//    
//   MainWindow.xaml.cs Licensed under the MIT License. See LICENSE file in
//   the project root for full license information.  
// -----------------------------------------------------------------------

namespace Han.Wpf.ViewportControl.Demo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Annotations;
    using Microsoft.Win32;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ImageSource _image;

        public MainWindow()
        {
            InitializeComponent();

            LoadImageCommand = new RelayCommand(() =>
            {
                var op = new OpenFileDialog
                {
                    Title = "Select an image",
                    Filter = "All Images|*.BMP;*.DIB;*.RLE;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG|" +
                             "BMP Files: (*.BMP; *.DIB; *.RLE)| *.BMP; *.DIB; *.RLE |" +
                             "JPEG Files: (*.JPG; *.JPEG; *.JPE; *.JFIF)| *.JPG; *.JPEG; *.JPE; *.JFIF |" +
                             "GIF Files: (*.GIF) | *.GIF |" +
                             "TIFF Files: (*.TIF; *.TIFF)| *.TIF; *.TIFF |" +
                             "PNG Files: (*.PNG) | *.PNG |" +
                             "All Files | *.* "
                };
                if (op.ShowDialog() == true)
                {
                    Image = new BitmapImage(new Uri(op.FileName));
                }
            });

            DataContext = this;
        }

        public ImageSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public RelayCommand LoadImageCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);

            return true;
        }
    }
}