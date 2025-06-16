using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 平差作业
{
    class Matrix
    {
        // 矩阵转置
        public static double[,] Transpose(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] result = new double[cols, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }
            return result;
        }

        // 矩阵乘法
        public static double[,] Multiply(double[,] a, double[,] b)
        {
            int rowsA = a.GetLength(0);
            int colsA = a.GetLength(1);
            int colsB = b.GetLength(1);
            double[,] result = new double[rowsA, colsB];

            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < colsA; k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return result;
        }

        // 矩阵乘向量
        public static double[] Multiply(double[,] matrix, double[] vector)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[] result = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                result[i] = 0;
                for (int j = 0; j < cols; j++)
                {
                    result[i] += matrix[i, j] * vector[j];
                }
            }
            return result;
        }

        

        // 矩阵求逆（高斯-约旦消元法）
        public static double[,] Inverse(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] augmented = new double[n, 2 * n];

            // 构建增广矩阵 [A|I]
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = matrix[i, j];
                    augmented[i, j + n] = (i == j) ? 1.0 : 0.0;
                }
            }

            // 高斯-约旦消元
            for (int i = 0; i < n; i++)
            {
                // 寻找主元
                double pivot = augmented[i, i];
                int pivotRow = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(augmented[j, i]) > Math.Abs(pivot))
                    {
                        pivot = augmented[j, i];
                        pivotRow = j;
                    }
                }

                // 交换行
                if (pivotRow != i)
                {
                    for (int j = 0; j < 2 * n; j++)
                    {
                        double temp = augmented[i, j];
                        augmented[i, j] = augmented[pivotRow, j];
                        augmented[pivotRow, j] = temp;
                    }
                }

                // 归一化主行
                for (int j = 0; j < 2 * n; j++)
                {
                    augmented[i, j] /= pivot;
                }

                // 消元
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        double factor = augmented[j, i];
                        for (int k = 0; k < 2 * n; k++)
                        {
                            augmented[j, k] -= factor * augmented[i, k];
                        }
                    }
                }
            }

            // 提取逆矩阵
            double[,] result = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = augmented[i, j + n];
                }
            }

            return result;
        }

    }
}