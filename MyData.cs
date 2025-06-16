using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 平差作业
{
    class MyData
    {
        public string DDnumber { get; set; } // 端点号（如 1-3）
        public double high { get; set; } // 观测高差
        public double distance { get; set; } // 测段距离
        public double number { get; set; } // 序号

        public MyData(string ddNumber, double high, double distance, double number)
        {
            DDnumber = ddNumber;
            this.high = high;
            this.distance = distance;
            this.number = number;
        }
    }
}
