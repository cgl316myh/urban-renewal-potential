using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace UrbanRenewal.Model
{
    /// <summary>动力性缓冲赋分规则（得分≤0 的环在分析时跳过）。</summary>
    public class BufferScoreRules
    {
        public BufferScoreRules()
        {
            MetroPreset = "Original";
            MetroMulti = MultiRingRule.Create(new double[] { 300, 600, 1000 }, new int[] { 4, 3, 2 });
            MetroSingle = MultiRingRule.Create(new double[] { 300, 600, 1000 }, new int[] { 3, 2, 1 });
            Cbd = SingleRingRule.Create(1000, 3);
            TrafficFacility = SingleRingRule.Create(300, 1);
            EcoCorridor = SingleRingRule.Create(500, 2);
            OpenSpace = SingleRingRule.Create(500, 2);
            Green = SingleRingRule.Create(300, 1);
            PublicService = SingleRingRule.Create(1000, 2);
            Convenience = SingleRingRule.Create(300, 1);
            Commercial = SingleRingRule.Create(1000, 1);
        }

        /// <summary>Original / A / B / C / Custom</summary>
        [XmlAttribute("metroPreset")]
        public string MetroPreset { get; set; }

        [XmlElement("MetroMulti")]
        public MultiRingRule MetroMulti { get; set; }

        [XmlElement("MetroSingle")]
        public MultiRingRule MetroSingle { get; set; }

        [XmlElement("CBD")]
        public SingleRingRule Cbd { get; set; }

        [XmlElement("TrafficFacility")]
        public SingleRingRule TrafficFacility { get; set; }

        [XmlElement("EcoCorridor")]
        public SingleRingRule EcoCorridor { get; set; }

        [XmlElement("OpenSpace")]
        public SingleRingRule OpenSpace { get; set; }

        [XmlElement("Green")]
        public SingleRingRule Green { get; set; }

        [XmlElement("PublicService")]
        public SingleRingRule PublicService { get; set; }

        [XmlElement("Convenience")]
        public SingleRingRule Convenience { get; set; }

        [XmlElement("Commercial")]
        public SingleRingRule Commercial { get; set; }

        /// <summary>现状代码默认（地铁偏强）。</summary>
        public static BufferScoreRules CreateOriginal()
        {
            return new BufferScoreRules();
        }

        /// <summary>方案 A 温和：整体降 1 档。</summary>
        public static BufferScoreRules CreatePresetA()
        {
            BufferScoreRules r = CreateOriginal();
            r.MetroPreset = "A";
            r.MetroMulti = MultiRingRule.Create(new double[] { 300, 600, 1000 }, new int[] { 3, 2, 1 });
            r.MetroSingle = MultiRingRule.Create(new double[] { 300, 600, 1000 }, new int[] { 2, 1, 1 });
            return r;
        }

        /// <summary>方案 B 推荐：地铁作加分项，外环可取消（得分 0）。</summary>
        public static BufferScoreRules CreatePresetB()
        {
            BufferScoreRules r = CreateOriginal();
            r.MetroPreset = "B";
            r.MetroMulti = MultiRingRule.Create(new double[] { 300, 600, 1000 }, new int[] { 2, 1, 1 });
            r.MetroSingle = MultiRingRule.Create(new double[] { 300, 600 }, new int[] { 2, 1 });
            return r;
        }

        /// <summary>方案 C 激进：缩小半径 + 降分。</summary>
        public static BufferScoreRules CreatePresetC()
        {
            BufferScoreRules r = CreateOriginal();
            r.MetroPreset = "C";
            r.MetroMulti = MultiRingRule.Create(new double[] { 300, 600 }, new int[] { 2, 1 });
            r.MetroSingle = MultiRingRule.Create(new double[] { 300, 600 }, new int[] { 1, 1 });
            return r;
        }

        public static BufferScoreRules FromPresetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return CreateOriginal();
            }
            string key = name.Trim();
            if (string.Equals(key, "A", StringComparison.OrdinalIgnoreCase)
                || key.IndexOf("温和", StringComparison.Ordinal) >= 0)
            {
                return CreatePresetA();
            }
            if (string.Equals(key, "B", StringComparison.OrdinalIgnoreCase)
                || key.IndexOf("推荐", StringComparison.Ordinal) >= 0)
            {
                return CreatePresetB();
            }
            if (string.Equals(key, "C", StringComparison.OrdinalIgnoreCase)
                || key.IndexOf("激进", StringComparison.Ordinal) >= 0)
            {
                return CreatePresetC();
            }
            return CreateOriginal();
        }

        public BufferScoreRules Clone()
        {
            BufferScoreRules c = new BufferScoreRules();
            c.MetroPreset = MetroPreset;
            c.MetroMulti = MetroMulti != null ? MetroMulti.Clone() : null;
            c.MetroSingle = MetroSingle != null ? MetroSingle.Clone() : null;
            c.Cbd = Cbd != null ? Cbd.Clone() : null;
            c.TrafficFacility = TrafficFacility != null ? TrafficFacility.Clone() : null;
            c.EcoCorridor = EcoCorridor != null ? EcoCorridor.Clone() : null;
            c.OpenSpace = OpenSpace != null ? OpenSpace.Clone() : null;
            c.Green = Green != null ? Green.Clone() : null;
            c.PublicService = PublicService != null ? PublicService.Clone() : null;
            c.Convenience = Convenience != null ? Convenience.Clone() : null;
            c.Commercial = Commercial != null ? Commercial.Clone() : null;
            return c;
        }

        public string DescribeMetro()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("预设=").Append(MetroPreset ?? "Original");
            sb.Append("; 多线=").Append(MetroMulti != null ? MetroMulti.ToDisplay() : "-");
            sb.Append("; 单线=").Append(MetroSingle != null ? MetroSingle.ToDisplay() : "-");
            return sb.ToString();
        }
    }

    public class MultiRingRule
    {
        [XmlAttribute("distances")]
        public string Distances { get; set; }

        [XmlAttribute("scores")]
        public string Scores { get; set; }

        public static MultiRingRule Create(double[] distances, int[] scores)
        {
            MultiRingRule r = new MultiRingRule();
            r.Distances = JoinDoubles(distances);
            r.Scores = JoinInts(scores);
            return r;
        }

        public MultiRingRule Clone()
        {
            MultiRingRule c = new MultiRingRule();
            c.Distances = Distances;
            c.Scores = Scores;
            return c;
        }

        public void GetActiveRings(out double[] distances, out int[] scores)
        {
            double[] dAll = ParseDoubles(Distances);
            int[] sAll = ParseInts(Scores);
            List<double> d = new List<double>();
            List<int> s = new List<int>();
            int n = Math.Min(dAll.Length, sAll.Length);
            for (int i = 0; i < n; i++)
            {
                if (dAll[i] > 0 && sAll[i] > 0)
                {
                    d.Add(dAll[i]);
                    s.Add(sAll[i]);
                }
            }
            distances = d.ToArray();
            scores = s.ToArray();
        }

        public string ToDisplay()
        {
            double[] d;
            int[] s;
            GetActiveRings(out d, out s);
            if (d.Length == 0)
            {
                return "（无有效环）";
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < d.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(" / ");
                }
                sb.Append(d[i].ToString("0.##", CultureInfo.InvariantCulture))
                    .Append("m→")
                    .Append(s[i].ToString(CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        private static string JoinDoubles(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(values[i].ToString("0.##", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        private static string JoinInts(int[] values)
        {
            if (values == null || values.Length == 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(values[i].ToString(CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        private static double[] ParseDoubles(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new double[0];
            }
            string[] parts = text.Split(new char[] { ',', ';', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<double> list = new List<double>();
            for (int i = 0; i < parts.Length; i++)
            {
                double v;
                if (double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v)
                    || double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out v))
                {
                    list.Add(v);
                }
            }
            return list.ToArray();
        }

        private static int[] ParseInts(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new int[0];
            }
            string[] parts = text.Split(new char[] { ',', ';', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> list = new List<int>();
            for (int i = 0; i < parts.Length; i++)
            {
                int v;
                if (int.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v)
                    || int.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out v))
                {
                    list.Add(v);
                }
            }
            return list.ToArray();
        }
    }

    public class SingleRingRule
    {
        [XmlAttribute("distance")]
        public double Distance { get; set; }

        [XmlAttribute("score")]
        public int Score { get; set; }

        public static SingleRingRule Create(double distance, int score)
        {
            SingleRingRule r = new SingleRingRule();
            r.Distance = distance;
            r.Score = score;
            return r;
        }

        public SingleRingRule Clone()
        {
            return Create(Distance, Score);
        }

        public bool IsActive
        {
            get { return Distance > 0 && Score > 0; }
        }

        public string ToDisplay()
        {
            if (!IsActive)
            {
                return "（关闭）";
            }
            return Distance.ToString("0.##", CultureInfo.InvariantCulture) + "m→" + Score.ToString(CultureInfo.InvariantCulture);
        }
    }
}
