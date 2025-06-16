using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 平差作业
{
    class  MyAlog
    {
        // 输入数据
        public static List<MyData> data = new List<MyData>(); // 测段数据
        public static List<MyPoint> YZPoints = new List<MyPoint>(); // 已知点
        public int SumPoint { get; set; } // 总点数

        // 平差计算核心参数
        private double[,] A; // 误差方程系数矩阵
        private double[] L; // 误差方程常数项
        private double[,] P; // 权矩阵
        private double[,] N; // 法方程系数矩阵 (N = A^T * P * A)
        private double[] W; // 法方程常数项 (W = A^T * P * L)
        private double[] X; // 参数改正数
        private double[] V; // 观测值改正数

        // 平差结果
        public double H3, H4, H5; // 3、4、5号点平差后高程
        public double[] AdjustedHeights; // 观测高差平差值
        public double UnitWeightSigma; // 单位权中误差
        public double[] PointSigma; // 待定点高程中误差（3、4、5号点对应索引 0、1、2）

        /// <summary>
        /// 执行平差计算并返回结果报告
        /// </summary>
        public string PrintBG()
        {
            // 执行平差计算
            DoAdjustment();

            // 生成结果报告
            return GetAdjustmentReport();
        }

        /// <summary>
        /// 执行平差计算
        /// </summary>
        private void DoAdjustment()
        {
            // 1. 初始化基础参数
            int n = data.Count; // 观测值数量（图中 n=7）
            int t = 3; // 必要观测数（3、4、5号点三个未知点，t=3）
            int u = n - t; // 多余观测数

            // 2. 构造误差方程
            BuildErrorEquation(n, t);

            // 3. 定权（10km为单位权观测）
            BuildWeightMatrix(n);

            // 4. 组成并解法方程
            BuildNormalEquation(n, t);
            SolveNormalEquation(t);

            // 5. 计算改正数、平差值
            ComputeCorrectionsAndAdjustedValues(n, t);

            // 6. 精度评定
            ComputePrecision(n, t, u);
        }

        /// <summary>
        /// 构造误差方程
        /// </summary>
        private void BuildErrorEquation(int n, int t)
        {
            A = new double[n, t];
            L = new double[n];

            // 获取已知点高程
            double H1 = YZPoints.First(p => p.id == "1").high;
            double H2 = YZPoints.First(p => p.id == "2").high;

            // 计算参数近似值（从已知点推算）
            double X3_approx = H1 + data[0].high; // 1-3测段近似高程
            double X4_approx = H1 + data[1].high; // 1-4测段近似高程
            double X5_approx = H2 + data[6].high; // 5-2测段近似高程

            // 按水准网路线，逐个列误差方程（对应7个观测值）
            // 测段1: 1-3
            A[0, 0] = 1;
            L[0] = X3_approx - data[0].high - H1;

            // 测段2: 1-4
            A[1, 1] = 1;
            L[1] = X4_approx - data[1].high - H1;

            // 测段3: 2-3
            A[2, 0] = 1;
            L[2] = X3_approx - data[2].high - H2;

            // 测段4: 2-4
            A[3, 1] = 1;
            L[3] = X4_approx - data[3].high - H2;

            // 测段5: 3-4
            A[4, 1] = 1;
            A[4, 0] = -1;
            L[4] = (X4_approx - X3_approx) - data[4].high;

            // 测段6: 3-5
            A[5, 2] = 1;
            A[5, 0] = -1;
            L[5] = (X5_approx - X3_approx) - data[5].high;

            // 测段7: 5-2
            A[6, 2] = -1;
            L[6] = (H2 - X5_approx) - data[6].high;
        }

        /// <summary>
        /// 定权（按10km为单位权观测，Pi = 10 / Si）
        /// </summary>
        private void BuildWeightMatrix(int n)
        {
            P = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                P[i, i] = 10.0 / data[i].distance;
            }
        }

        /// <summary>
        /// 组成法方程 N*X = W
        /// </summary>
        private void BuildNormalEquation(int n, int t)
        {
            double[,] A_T = Matrix.Transpose(A);
            double[,] P_A = Matrix.Multiply(P, A);
            N = Matrix.Multiply(A_T, P_A);

            double[] P_L = Matrix.Multiply(P, L);
            W = Matrix.Multiply(A_T, P_L);
        }

        /// <summary>
        /// 解法方程（求参数改正数 X）
        /// </summary>
        private void SolveNormalEquation(int t)
        {
            double[,] N_Inv = Matrix.Inverse(N);
            X = Matrix.Multiply(N_Inv, W);
        }

        /// <summary>
        /// 计算观测值改正数、平差值，以及待定点高程
        /// </summary>
        private void ComputeCorrectionsAndAdjustedValues(int n, int t)
        {
            // 计算观测值改正数 V = A*X - L
            V = new double[n];
            double[] A_X = Matrix.Multiply(A, X);
            for (int i = 0; i < n; i++)
            {
                V[i] = A_X[i] - L[i];
            }

            // 计算待定点高程平差值
            double H1 = YZPoints.First(p => p.id == "1").high;
            double X3_approx = H1 + data[0].high;
            double X4_approx = H1 + data[1].high;
            double X5_approx = YZPoints.First(p => p.id == "2").high + data[6].high;

            H3 = X3_approx + X[0];
            H4 = X4_approx + X[1];
            H5 = X5_approx + X[2];

            // 计算观测高差平差值
            AdjustedHeights = new double[n];
            for (int i = 0; i < n; i++)
            {
                AdjustedHeights[i] = data[i].high + V[i];
            }
        }

        /// <summary>
        /// 精度评定
        /// </summary>
        private void ComputePrecision(int n, int t, int u)
        {
            // 单位权中误差
            double VPV = 0;
            for (int i = 0; i < n; i++)
            {
                VPV += V[i] * P[i, i] * V[i];
            }
            UnitWeightSigma = Math.Sqrt(VPV / u);

            // 待定点高程中误差
            double[,] N_Inv = Matrix.Inverse(N);
            PointSigma = new double[t];
            for (int i = 0; i < t; i++)
            {
                PointSigma[i] = UnitWeightSigma * Math.Sqrt(N_Inv[i, i]);
            }
        }

        /// <summary>
        /// 输出平差报告
        /// </summary>
        private string GetAdjustmentReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("===== 水准网间接平差计算报告 =====");
            sb.AppendLine($"计算日期：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("\n--- 已知数据 ---");
            sb.AppendLine($"总点数: {SumPoint}");
            sb.AppendLine($"已知点1高程：H1 = {YZPoints.First(p => p.id == "1").high:F3} m");
            sb.AppendLine($"已知点2高程：H2 = {YZPoints.First(p => p.id == "2").high:F3} m");

            sb.AppendLine("\n--- 平差结果 ---");
            sb.AppendLine($"3号点高程：H3 = {H3:F3} m");
            sb.AppendLine($"4号点高程：H4 = {H4:F3} m");
            sb.AppendLine($"5号点高程：H5 = {H5:F3} m");

            sb.AppendLine("\n--- 观测高差平差值 ---");
            for (int i = 0; i < AdjustedHeights.Length; i++)
            {
                sb.AppendLine($"测段{(int)data[i].number}（{data[i].DDnumber}）：{AdjustedHeights[i]:F3} m");
            }

            sb.AppendLine("\n--- 观测值改正数 ---");
            for (int i = 0; i < V.Length; i++)
            {
                sb.AppendLine($"测段{(int)data[i].number}改正数：v{i + 1} = {V[i]:F4} m");
            }

            sb.AppendLine("\n--- 精度评定 ---");
            sb.AppendLine($"单位权中误差：σ0 = {UnitWeightSigma:F4}");
            sb.AppendLine($"3号点高程中误差：σ3 = {PointSigma[0]:F4} m");
            sb.AppendLine($"4号点高程中误差：σ4 = {PointSigma[1]:F4} m");
            sb.AppendLine($"5号点高程中误差：σ5 = {PointSigma[2]:F4} m");

            return sb.ToString();
        }
    }
}