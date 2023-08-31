using AirPlayHub.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirPlayHub.Common
{
    public class AirRect
    {
        public AirRect()
        {
            left = top = width = height = 0.0;
        }

        public double left { get; set; }
        public double top { get; set; }
        public double width { get; set; }
        public double height { get; set; }

        public static bool operator ==(AirRect left, AirRect right)
        {
            return (left.left == right.left && left.top == right.top && left.width == right.width && left.height == right.height);
        }

        public static bool operator !=(AirRect left, AirRect right)
        {
            return !(left == right);
        }

    }

    public class AirSource
    {
        public AirSource(string session, string name)
        {
            this.session = session;
            this.name = name;

            this.showRect = new AirRect();
        }

        public string session { get; set; }

        public string name { get; set; }

        public string address { get; set; }

        public int width { get; set; }

        public int height { get; set; }

        public int state { get; set; }

        public bool mute
        {
            get
            {
                return mute;
            }

            set
            {
                bool curVal = mute;
                if (curVal != value)
                {
                    AirPlayWrapper.Mute(session, value);

                    mute = value;
                }
            }
        }

        // 这个参数这里没有意义
        public long updateTime { get; set; }

        public AirRect showRect { get; set; }

    }
}
