using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.ComponentModel;
using Microsoft.VisualBasic;

namespace Demo3
{
    internal class PrintGraph
    {
        static SolidBrush backgroundColor = new SolidBrush(Color.FromArgb(176, 255, 176));
        static SolidBrush white = new SolidBrush(Color.White);
        static Pen stationLine = new Pen(Color.Red, 4);
        static Pen timeLine = new Pen(Color.Green, 1);
        static Pen trainLine = new Pen(Color.Red, 3);
        static Font timeFont = new Font("宋体", 35, FontStyle.Bold);
        static Font stnNameFont = new Font("宋体", 35, FontStyle.Bold);
        static SolidBrush Text = new SolidBrush(Color.Black);
        Image img;
        Graphics g;
        int verEdgeGap = 100;
        int horEdgeGap = 100;
        double secondGap = 0.5;
        int interStnGap = 100;
        int innerStnGap = 20;

        Dictionary<string, int> arrDic = new Dictionary<string, int>();
        Dictionary<string, int> depDic = new Dictionary<string, int>();
        public PrintGraph(DateTime startTime, DateTime endTime)
        {
            int n = DataManager.Stns1.Count;
            this.startTime = startTime;
            int end = (int)Math.Ceiling((endTime - startTime).TotalSeconds / 3600)+startTime.Hour;
            int start = startTime.Hour;
            int horLen = (int)((end - start) * 3600 * secondGap);
            int verLen = (n - 1) * interStnGap + n * innerStnGap;
            int nowHor = verEdgeGap;
            foreach (Station stn in DataManager.Stns1)
            {
                arrDic.Add(stn.StnName, nowHor);
                nowHor += innerStnGap;
                depDic.Add(stn.StnName, nowHor);
                nowHor += interStnGap;
            }
            img = new Bitmap(horLen + 2 * horEdgeGap, 2 * verEdgeGap + verLen);
            g = Graphics.FromImage(img);
            g.FillRectangle(white, 0, 0, horLen + 2 * horEdgeGap, verLen + 2 * verEdgeGap);
            g.FillRectangle(backgroundColor, horEdgeGap, verEdgeGap, horLen, verLen);
            Point arrStart = new Point(horEdgeGap, verEdgeGap);
            Point depStart = new Point(horEdgeGap, verEdgeGap + innerStnGap);
            Point arrEnd = new Point(horEdgeGap + horLen, verEdgeGap);
            Point depEnd = new Point(horEdgeGap + horLen, verEdgeGap + innerStnGap);
            Point stnName = new Point((int)stnNameFont.Size, verEdgeGap);
            int addNum = interStnGap + innerStnGap;
            for (int i = 0; i < n; i++)
            {
                g.DrawLine(stationLine, arrStart, arrEnd);
                g.DrawLine(stationLine, depStart, depEnd);
                g.DrawString(DataManager.Stns1[i].StnName, stnNameFont, Text, stnName);
                arrStart.Y += addNum;
                arrEnd.Y += addNum;
                depStart.Y += addNum;
                depEnd.Y += addNum;
                stnName.Y += addNum;
            }
            DateTime time = startTime.Date + TimeSpan.FromHours(start);
            DateTime _end = startTime.Date + TimeSpan.FromHours(end);
            int xStart = horEdgeGap;
            for (; time <= _end; time = time.AddMinutes(10))
            {
                timeLine.DashStyle = DashStyle.Solid;
                int addX = (int)(Time2int(time) * secondGap);
                Point timeStart = new Point(xStart + addX, verEdgeGap);
                Point timeEnd = new Point(xStart + addX, verEdgeGap + verLen);
                Point timeString = new Point(xStart + addX - (int)(1.5* timeFont.Size), verEdgeGap - (int)(1.5 * timeFont.Size));
                if (time.Minute % 30 == 0)
                {
                    g.DrawString(string.Format("{0}", time.ToString("HH:mm")), timeFont, Text, timeString);
                    timeLine.Width = 3;
                }
                else timeLine.Width = 1;
                g.DrawLine(timeLine, timeStart, timeEnd);
                timeLine.DashStyle = DashStyle.Dash;
                g.DrawLine(timeLine, timeStart, timeEnd);
            }
        }

        public void Draw()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\output"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\output");
            }
            DirectoryInfo Dir = new DirectoryInfo(Environment.CurrentDirectory + "\\output");
            FileSystemInfo[] fileinfo = Dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i.Name == "TimeTable.png")
                {
                    File.Delete(i.FullName);
                }
            }
            foreach (var t in DataManager.Trains)
            {
                DrawTrain(t);
            }
            img.Save(Environment.CurrentDirectory + "\\output\\TrainDiagram.png");
        }
        int cnt = 0;
        void DrawTrain(Train t)
        {
            Action<Station, bool> drawStn = (stn, reverse) =>
            {
                if (t.Arrs.ContainsKey(stn))
                {
                    DateTime arr = t.Arrs[stn];
                    DateTime dep = t.Deps[stn];
                    int xstart = Time2int(arr) + horEdgeGap;
                    int ystart = reverse ? arrDic[stn.StnName] : depDic[stn.StnName];
                    int xend = Time2int(dep) + horEdgeGap;
                    int yend = reverse ? depDic[stn.StnName] : arrDic[stn.StnName];
                    g.DrawLine(trainLine, xstart, ystart, xend, yend);
                    Station nxtStn;
                    if (stn._nextSec != null && t.Arrs.ContainsKey(nxtStn = stn._nextSec.ToStn))
                    {
                        int nx = Time2int(t.Arrs[nxtStn]) + horEdgeGap;
                        int ny = reverse ? arrDic[nxtStn.StnName] : depDic[nxtStn.StnName];
                        g.DrawLine(trainLine, xend, yend, nx, ny);
                    }
                }
            };
            bool reverse = false;
            var stns = DataManager.Stns2;
            foreach (var stn in DataManager.Stns1)
            {
                if (t.Arrs.ContainsKey(stn))
                {
                    stns = DataManager.Stns1;
                    reverse = true;
                    break;
                }
            }
            if (cnt % 4 == 0) trainLine.Color = Color.Red;
            else if (cnt % 4 == 1) trainLine.Color = Color.Violet;
            else if (cnt % 4 == 2) trainLine.Color = Color.Brown;
            else trainLine.Color = Color.DarkRed;
            ++cnt;
            stns.ForEach(stn => drawStn(stn, reverse));

        }

        DateTime startTime;
        int Time2int(DateTime time)
        {
            return (int)(time - startTime).TotalSeconds;
        }
    }

}
