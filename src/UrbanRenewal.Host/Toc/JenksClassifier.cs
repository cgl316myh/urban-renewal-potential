using System;
using System.Collections.Generic;

namespace UrbanRenewal.Host
{
    /// <summary>
    /// Jenks 自然断裂点分级（采样后一维数组）。
    /// </summary>
    public static class JenksClassifier
    {
        /// <summary>
        /// 计算自然断裂断点。返回长度为 classCount+1 的升序断点（含最小值与最大值）。
        /// </summary>
        public static double[] ComputeBreaks(IList<double> values, int classCount)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("分级数据为空。");
            }

            if (classCount < 2)
            {
                classCount = 2;
            }

            List<double> data = new List<double>(values.Count);
            for (int i = 0; i < values.Count; i++)
            {
                double v = values[i];
                if (!double.IsNaN(v) && !double.IsInfinity(v))
                {
                    data.Add(v);
                }
            }

            if (data.Count == 0)
            {
                throw new ArgumentException("没有有效数值用于分级。");
            }

            data.Sort();

            if (classCount > data.Count)
            {
                classCount = data.Count;
            }

            if (classCount < 2)
            {
                return new double[] { data[0], data[data.Count - 1] };
            }

            // 等间距退化保护：数值几乎相同
            if (Math.Abs(data[data.Count - 1] - data[0]) < 1e-12)
            {
                double[] same = new double[classCount + 1];
                for (int i = 0; i <= classCount; i++)
                {
                    same[i] = data[0];
                }
                return same;
            }

            int n = data.Count;
            double[,] mat1 = new double[n + 1, classCount + 1];
            double[,] mat2 = new double[n + 1, classCount + 1];

            for (int i = 1; i <= classCount; i++)
            {
                mat1[1, i] = 1;
                mat2[1, i] = 0;
                for (int j = 2; j <= n; j++)
                {
                    mat1[j, i] = 0;
                    mat2[j, i] = double.MaxValue;
                }
            }

            double vVar = 0;
            for (int l = 2; l <= n; l++)
            {
                double s1 = 0;
                double s2 = 0;
                double w = 0;

                for (int m = 1; m <= l; m++)
                {
                    int i3 = l - m + 1;
                    double val = data[i3 - 1];
                    s2 += val * val;
                    s1 += val;
                    w += 1;
                    vVar = s2 - (s1 * s1) / w;
                    int i4 = i3 - 1;
                    if (i4 != 0)
                    {
                        for (int j = 2; j <= classCount; j++)
                        {
                            if (mat2[l, j] >= (vVar + mat2[i4, j - 1]))
                            {
                                mat1[l, j] = i3;
                                mat2[l, j] = vVar + mat2[i4, j - 1];
                            }
                        }
                    }
                }

                mat1[l, 1] = 1;
                mat2[l, 1] = vVar;
            }

            double[] kclass = new double[classCount + 1];
            kclass[0] = data[0];
            kclass[classCount] = data[n - 1];

            int k = n;
            for (int j = classCount; j >= 2; j--)
            {
                int id = (int)mat1[k, j] - 2;
                if (id < 0)
                {
                    id = 0;
                }
                kclass[j - 1] = data[id];
                k = (int)mat1[k, j] - 1;
                if (k < 1)
                {
                    k = 1;
                }
            }

            return kclass;
        }

        /// <summary>
        /// 等间距断点。
        /// </summary>
        public static double[] ComputeEqualIntervalBreaks(IList<double> values, int classCount)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("分级数据为空。");
            }

            if (classCount < 2)
            {
                classCount = 2;
            }

            double min = double.MaxValue;
            double max = double.MinValue;
            int count = 0;

            for (int i = 0; i < values.Count; i++)
            {
                double v = values[i];
                if (double.IsNaN(v) || double.IsInfinity(v))
                {
                    continue;
                }
                if (v < min) min = v;
                if (v > max) max = v;
                count++;
            }

            if (count == 0)
            {
                throw new ArgumentException("没有有效数值用于分级。");
            }

            double[] breaks = new double[classCount + 1];
            breaks[0] = min;
            breaks[classCount] = max;

            if (Math.Abs(max - min) < 1e-12)
            {
                for (int i = 1; i < classCount; i++)
                {
                    breaks[i] = min;
                }
                return breaks;
            }

            double step = (max - min) / classCount;
            for (int i = 1; i < classCount; i++)
            {
                breaks[i] = min + step * i;
            }

            return breaks;
        }

        /// <summary>
        /// 将分值映射为等级：1=最优（高分），classCount=较差。非有效返回 0。
        /// </summary>
        public static int ScoreToGradeBestFirst(double score, double[] breaks)
        {
            if (breaks == null || breaks.Length < 2)
            {
                return 0;
            }

            int classCount = breaks.Length - 1;

            // 从低到高找类别，再反转为“优在前”
            int classFromLow = 0;
            for (int i = 0; i < classCount; i++)
            {
                if (i == classCount - 1)
                {
                    if (score >= breaks[i] - 1e-12)
                    {
                        classFromLow = i;
                    }
                }
                else if (score >= breaks[i] - 1e-12 && score < breaks[i + 1] - 1e-12)
                {
                    classFromLow = i;
                    break;
                }
                else if (score >= breaks[i + 1] - 1e-12)
                {
                    classFromLow = i + 1;
                }
            }

            return classCount - classFromLow;
        }
    }
}
