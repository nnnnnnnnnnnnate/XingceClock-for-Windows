using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XingceClock
{
    public class Stage
    {
        public string Name;
        public double Minutes;

        public Stage(string name, double minutes)
        {
            Name = name;
            Minutes = minutes;
        }
    }

    public static class StageStore
    {
        public static string FilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stages.txt");
        }

        public static List<Stage> Load()
        {
            List<Stage> list = new List<Stage>();
            try
            {
                string path = FilePath();
                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path, Encoding.UTF8);
                    foreach (string raw in lines)
                    {
                        string line = (raw == null) ? "" : raw.Trim();
                        if (line.Length == 0) continue;
                        int tab = line.IndexOf('\t');
                        if (tab <= 0) continue;
                        string name = line.Substring(0, tab).Trim();
                        string numStr = line.Substring(tab + 1).Trim();
                        double mins;
                        if (name.Length == 0 || !double.TryParse(numStr,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out mins)) continue;
                        if (mins <= 0) continue;
                        list.Add(new Stage(name, mins));
                    }
                }
            }
            catch { }
            if (list.Count == 0) list = Defaults();
            return list;
        }

        public static void Save(List<Stage> list)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (Stage s in list)
                {
                    sb.Append(s.Name.Replace('\t', ' '));
                    sb.Append('\t');
                    sb.Append(s.Minutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    sb.Append("\r\n");
                }
                File.WriteAllText(FilePath(), sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        public static List<Stage> Defaults()
        {
            List<Stage> list = new List<Stage>();
            list.Add(new Stage("常识判断", 10));
            list.Add(new Stage("言语理解与表达", 30));
            list.Add(new Stage("数量关系", 15));
            list.Add(new Stage("判断推理", 30));
            list.Add(new Stage("资料分析", 30));
            list.Add(new Stage("涂卡检查", 5));
            return list;
        }
    }
}
