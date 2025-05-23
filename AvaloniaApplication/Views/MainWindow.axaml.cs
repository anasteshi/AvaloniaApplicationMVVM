using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaApplication.Services;
using AvaloniaApplication.ViewModels;
using NWaves.Signals;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia.Interactivity;

namespace AvaloniaApplication.Views
{
    public partial class MainWindow : Window
    {
        #region Private Members

        /// <summary>
        /// The main view model of this view
        /// </summary>
        private ViewModelBase mViewModel => (ViewModelBase)DataContext;

        private readonly Control mChannelConfigPopup;
        private readonly Control mChannelConfigButton;
        private readonly Control mMainGrid;
        private readonly Control mVolumeContainer;
        private readonly Control mVolumeBar;

        /// <summary>
        /// The timeout timer to detect when auto-sizing has finished firing
        /// </summary>
        private readonly Timer mSizingTimer;

        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <exception cref="Exception">Throws if named controls cannot be found</exception>
        public MainWindow()
        {
            InitializeComponent();

            mSizingTimer = new Timer((t) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Update the desired size
                    UpdateSizes();
                });
            });

            // Gather the named controls
            mChannelConfigButton = this.FindControl<Control>("ChannelConfigurationButton") ?? throw new Exception("Cannot find Channel Configuration Button by name");
            mChannelConfigPopup = this.FindControl<Control>("ChannelConfigurationPopup") ?? throw new Exception("Cannot find Channel Configuration Popup by name");
            mMainGrid = this.FindControl<Control>("MainGrid") ?? throw new Exception("Cannot find Main Grid by name");
            mVolumeContainer = this.FindControl<Control>("VolumeContainer") ?? throw new Exception("Cannot find Volume Container by name");
            mVolumeBar = this.FindControl<Control>("VolumeBar") ?? throw new Exception("Cannot find Volume Bar by name");
        }
        
        #endregion

        /// <summary>
        /// Updates the application window/control sizes dynamically
        /// </summary>
        private void UpdateSizes()
        {
            mViewModel.VolumeContainerHeight = mVolumeContainer.Bounds.Height;
            mViewModel.VolumeBarHeight = mVolumeBar.Bounds.Height;
        }

        /// <summary>
        /// Run on-load initialization code
        /// </summary>
        protected override async void OnLoaded()
        {
            await mViewModel.LoadCommand.ExecuteAsync(null);
            
            base.OnLoaded();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            mSizingTimer.Change(100, int.MaxValue);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Get relative position of button, in relation to main grid
                var position = mChannelConfigButton.TranslatePoint(new Point(), mMainGrid) ??
                               throw new Exception("Cannot get TranslatePoint from Configuration Button");

                // Set margin of popup so it appears bottom left of button
                mChannelConfigPopup.Margin = new Thickness(
                    position.X,
                    0,
                    0,
                    mMainGrid.Bounds.Height - position.Y - mChannelConfigButton.Bounds.Height);
            });
        }

        private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
            => mViewModel.ChannelConfigurationButtonPressedCommand.Execute(null);
    }
}