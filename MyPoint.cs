using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 平差作业
{
    class MyPoint
    {
        public string id { get; set; } // 点号
        public double high { get; set; } // 高程

        public MyPoint(string id, double high)
        {
            this.id = id;
            this.high = high;
        }
    }
}
