using AirPlayHub.Common;
using AirPlayHub.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AirPlayHub.Control
{
    /// <summary>
    /// AirRender.xaml 的交互逻辑
    /// </summary>
    public partial class AirRender : UserControl
    {
        public AirSource airSource { get; set; }

        private D3DImage _d3dImg = null;
        private long _lastUpdateTime = 0;

        private DispatcherTimer _renderTimer = null;

        public AirRender()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            airName.Text = airSource.name;
            sliderVolume.Value = AirPlayWrapper.GetVolume(airSource.session);
            isMute.IsChecked = AirPlayWrapper.IsMute(airSource.session);
            if ((bool)isMute.IsChecked)
            {
                sliderVolume.IsEnabled = false;
            }

            _d3dImg = new D3DImage();
            img.Source = _d3dImg;

            // CompositionTarget.Rendering += CompositionTarget_Rendering;

            _renderTimer = new DispatcherTimer();
            _renderTimer.Tick += _renderTimer_Tick;
            _renderTimer.Interval = TimeSpan.FromMilliseconds(33);// new TimeSpan(0, 0, 0, 0, 33);
            _renderTimer.Start();

            gridControls.Visibility = Visibility.Collapsed;

            mHideFunAreaTimer = new DispatcherTimer();
            mHideFunAreaTimer.Interval = System.TimeSpan.FromMilliseconds(200);
            mHideFunAreaTimer.Tick += MHideFunAreaTimer_Tick;
            mHideFunAreaTimer.Start();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // CompositionTarget.Rendering -= CompositionTarget_Rendering;
            if (_renderTimer != null)
            {
                _renderTimer.Stop();
                _renderTimer = null;
            }
        }

        private void _renderTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                int width = 0;
                int height = 0;
                long updateTime = 0;
                IntPtr pSurface = IntPtr.Zero;

                if (!AirPlayWrapper.GetSurface9(airSource.session, out pSurface, out width, out height, out updateTime))
                {
                    return;
                }

                if (_lastUpdateTime == updateTime || pSurface == IntPtr.Zero)
                {
                    //    return;
                }

                _lastUpdateTime = updateTime;
                if (_d3dImg.IsFrontBufferAvailable)
                {
                    Duration LockDuration = new Duration(TimeSpan.Zero);
                    _d3dImg.Lock();
                    {
                        _d3dImg.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface);
                        _d3dImg.AddDirtyRect(new Int32Rect(0, 0, _d3dImg.PixelWidth, _d3dImg.PixelHeight));
                    }

                    _d3dImg.Unlock();
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void MHideFunAreaTimer_Tick(object sender, object e)
        {
            if (mShowFunArea)
            {
                if (Environment.TickCount - mPointerLastEventTime > 3000)
                {
                    mShowFunArea = false;
                    gridControls.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            try
            {
                int width = 0;
                int height = 0;
                long updateTime = 0;
                IntPtr pSurface = IntPtr.Zero;

                if (!AirPlayWrapper.GetSurface9(airSource.session, out pSurface, out width, out height, out updateTime))
                {
                    return;
                }

                if (_lastUpdateTime == updateTime || pSurface == IntPtr.Zero)
                {
                    //    return;
                }

                _lastUpdateTime = updateTime;
                if (_d3dImg.IsFrontBufferAvailable)
                {
                    Duration LockDuration = new Duration(TimeSpan.Zero);
                    _d3dImg.Lock();
                    {
                        _d3dImg.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface);
                        _d3dImg.AddDirtyRect(new Int32Rect(0, 0, _d3dImg.PixelWidth, _d3dImg.PixelHeight));
                    }

                    _d3dImg.Unlock();
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void isMute_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)isMute.IsChecked)
            {
                AirPlayWrapper.Mute(airSource.session, true);
                sliderVolume.IsEnabled = false;
            }
            else
            {
                AirPlayWrapper.Mute(airSource.session, false);
                sliderVolume.IsEnabled = true;
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AirPlayWrapper.SetVolume(airSource.session, (int)sliderVolume.Value);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_renderTimer != null)
            {
                _renderTimer.Stop();
                _renderTimer = null;
            }

            AirPlayWrapper.Close(airSource.session);
        }

        #region 鼠标/触控

        /// <summary>
        /// 鼠标动作延迟时间
        /// </summary>
        private int mPointerLastEventTime = 0;
        private DispatcherTimer mHideFunAreaTimer = null;

        /// <summary>
        /// 是否显示控制区域
        /// </summary>
        private bool mShowFunArea = false;

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            mPointerLastEventTime = Environment.TickCount;
            if (mShowFunArea == false)
            {
                mShowFunArea = true;
                gridControls.Visibility = Visibility.Visible;
            }
        }

        private void Grid_TouchDown(object sender, TouchEventArgs e)
        {
            mPointerLastEventTime = Environment.TickCount;
            if (mShowFunArea == false)
            {
                mShowFunArea = true;
                gridControls.Visibility = Visibility.Visible;
            }
        }

        private void Grid_TouchMove(object sender, TouchEventArgs e)
        {
            mPointerLastEventTime = Environment.TickCount;
            if (mShowFunArea == false)
            {
                mShowFunArea = true;
                gridControls.Visibility = Visibility.Visible;
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void Grid_TouchLeave(object sender, TouchEventArgs e)
        {

        }

        #endregion 鼠标/触控

        
    }
}
