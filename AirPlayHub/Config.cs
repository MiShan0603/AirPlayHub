using AirPlayHub.Wrapper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirPlayHub
{
    internal class Config
    {
        #region 单例
        private static Config _instance = null;

        private Config()
        {
            Init();
        }

        //此方法是获得本类实例的唯一全局访问点
        public static Config Instance()
        {
            //若实例不存在，则new一个新实例,否则返回已有的实例
            if (_instance == null)
            {
                _instance = new Config();
            }

            return _instance;
        }
        #endregion 单例

        
        private const string _keyName = @"HKEY_CURRENT_USER\SOFTWARE\AirPlayHub";

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name != value)
                {
                    _name = value;

                    try
                    {
                        Registry.SetValue(_keyName, "Name", _name);
                    }
                    catch (Exception ex)
                    {
                        Win32Fun.OutputDebugString(ex.ToString());
                    }
                }
            }
        }

        private string _pinCode;
        public string PinCode
        {
            get
            {
                return _pinCode;
            }

            set
            {
                if (_pinCode != value)
                {
                    _pinCode = value;

                    try
                    {
                        Registry.SetValue(_keyName, "PinCode", _pinCode);
                    }
                    catch (Exception ex)
                    {
                        Win32Fun.OutputDebugString(ex.ToString());
                    }
                }
            }
        }

        private bool _startWithWindow = false;
        public bool StartWithWindow
        {
            get
            {
                return _startWithWindow;
            }

            set 
            {
                if (_startWithWindow != value)
                {
                    _startWithWindow = value;

                    try
                    {
                        Registry.SetValue(_keyName, "StartWithWindow", _startWithWindow ? 1: 0);
                    }
                    catch (Exception ex)
                    {
                        Win32Fun.OutputDebugString(ex.ToString());
                    }
                }
            }
        }

        private void Init()
        {
            string name = @"AirPlayHub[" + Environment.MachineName + "]";
            object regObj = Registry.GetValue(_keyName, "Name", name);
            if (regObj == null)
            {
                _name = name;
            }
            else
            {
                _name = (string)regObj;
            }

            regObj = Registry.GetValue(_keyName, "PinCode", "");
            if (regObj == null)
            {
                _pinCode = "";
            }
            else
            {
                _pinCode = (string)regObj;
            }

            regObj = Registry.GetValue(_keyName, "StartWithWindow", 0);
            if (regObj == null)
            {
                _startWithWindow = false;
            }
            else
            {
                _startWithWindow = ((int)regObj == 1);
            }
        }

    }
}
