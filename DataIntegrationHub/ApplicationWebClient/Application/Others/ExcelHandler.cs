using ApplicationWebClient.DIS;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace ApplicationWebClient.Application.Others
{
    public static class ExcelHandler
    {

        public static ExcelPackage CreateStatistics(String[] channels, DIS.QueryTimeSpan span, DIS.Record[] records)
        {
            List<Record> lrecords = records.ToList();
            ExcelPackage excel = new ExcelPackage();
            var hasTemp = channels.Contains("T");
            var hasPres = channels.Contains("P");
            var hasHumi = channels.Contains("H");


            #region Content
            var ws = excel.Workbook.Worksheets.Add("Content");
            ws.Cells["A1"].Value = "IdRecord";
            ws.Cells["B1"].Value = "NodeId";
            ws.Cells["C1"].Value = "Channel";
            ws.Cells["D1"].Value = "Value";
            ws.Cells["E1"].Value = "Date created";
            ws.Cells["A1:E1"].Style.Font.Bold = true;

            for (int i = 0; i < records.Length; i++)
            {
                int j = i + 2;
                ws.Cells["A" + j].Value = records[i].IdRecord;
                ws.Cells["B" + j].Value = records[i].NodeId;
                ws.Cells["C" + j].Value = records[i].Channel;
                ws.Cells["D" + j].Value = records[i].Value;
                ws.Cells["E" + j].Value = records[i].DateCreated.ToShortDateString() + " " + records[i].DateCreated.ToShortTimeString();
            }
            #endregion

            #region Statistics
            ws = excel.Workbook.Worksheets.Add("Statistics");

            int row = 0;
            ws.Cells["A" + ++row].Value = "Statistics " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            using (ExcelRange r = ws.Cells["A" + row + ":W" + row])
            {
                r.Merge = true;
                r.Style.Font.SetFromFont(new Font("Arial", 22, FontStyle.Italic));
                r.Style.Font.Color.SetColor(Color.White);
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }

            #region Tables
            ws.Cells["A" + ++row].Value = "Ammount of records";
            using (ExcelRange r = ws.Cells["A" + row + ":B" + row])
            {
                r.Merge = true;
                r.Style.Font.SetFromFont(new Font("Arial", 22, FontStyle.Italic));
                r.Style.Font.Color.SetColor(Color.White);
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }

            int tcount = lrecords.Where(r => r.Channel == "T").Count();
            int pcount = lrecords.Where(r => r.Channel == "P").Count();
            int hcount = lrecords.Where(r => r.Channel == "H").Count();

            ws.Cells["A" + ++row].Value = "Channel";
            ws.Cells["B" + row].Value = "Value";
            if (hasTemp) ws.Cells["A" + ++row].Value = "Temperature";
            if (hasTemp) ws.Cells["B" + row].Value = tcount;
            if (hasPres) ws.Cells["A" + ++row].Value = "Pressure";
            if (hasPres) ws.Cells["B" + row].Value = pcount;
            if (hasHumi) ws.Cells["A" + ++row].Value = "Humidity";
            if (hasHumi) ws.Cells["B" + row].Value = hcount;

            // Temperature
            row++;
            ws.Cells["A" + ++row].Value = "Temperature";
            using (ExcelRange r = ws.Cells["A" + row + ":B" + row])
            {
                r.Merge = true;
                r.Style.Font.SetFromFont(new Font("Arial", 22, FontStyle.Italic));
                r.Style.Font.Color.SetColor(Color.White);
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }

            var tmin = lrecords.Where(r => r.Channel == "T").Select(r => (float?)r.Value).Min();
            var tavg = lrecords.Where(r => r.Channel == "T").Select(r => (float?)r.Value).Average();
            var tmax = lrecords.Where(r => r.Channel == "T").Select(r => (float?)r.Value).Max();


            ws.Cells["A" + ++row].Value = "Temperature";
            ws.Cells["B" + row].Value = "Value";
            if (hasTemp) ws.Cells["A" + ++row].Value = "Minimum";
            if (hasTemp) ws.Cells["B" + row].Value = tmin;
            if (hasTemp) ws.Cells["A" + ++row].Value = "Average";
            if (hasTemp) ws.Cells["B" + row].Value = tavg;
            if (hasTemp) ws.Cells["A" + ++row].Value = "Maximum";
            if (hasTemp) ws.Cells["B" + row].Value = tmax;

            // Pressure
            row++;
            ws.Cells["A" + ++row].Value = "Pressure";
            using (ExcelRange r = ws.Cells["A" + row + ":B" + row])
            {
                r.Merge = true;
                r.Style.Font.SetFromFont(new Font("Arial", 22, FontStyle.Italic));
                r.Style.Font.Color.SetColor(Color.White);
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }

            var pmin = lrecords.Where(r => r.Channel == "P").Select(r => (float?)r.Value).Min();
            var pavg = lrecords.Where(r => r.Channel == "P").Select(r => (float?)r.Value).Average();
            var pmax = lrecords.Where(r => r.Channel == "P").Select(r => (float?)r.Value).Max();


            ws.Cells["A" + ++row].Value = "Pressure";
            ws.Cells["B" + row].Value = "Value";
            if (hasPres) ws.Cells["A" + ++row].Value = "Minimum";
            if (hasPres) ws.Cells["B" + row].Value = pmin;
            if (hasPres) ws.Cells["A" + ++row].Value = "Average";
            if (hasPres) ws.Cells["B" + row].Value = pavg;
            if (hasPres) ws.Cells["A" + ++row].Value = "Maximum";
            if (hasPres) ws.Cells["B" + row].Value = pmax;

            // Humidity
            row++;
            ws.Cells["A" + ++row].Value = "Humidity";
            using (ExcelRange r = ws.Cells["A" + row + ":B" + row])
            {
                r.Merge = true;
                r.Style.Font.SetFromFont(new Font("Arial", 22, FontStyle.Italic));
                r.Style.Font.Color.SetColor(Color.White);
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }

            var hmin = lrecords.Where(r => r.Channel == "H").Select(r => (float?)r.Value).Min();
            var havg = lrecords.Where(r => r.Channel == "H").Select(r => (float?)r.Value).Average();
            var hmax = lrecords.Where(r => r.Channel == "H").Select(r => (float?)r.Value).Max();

            ws.Cells["A" + ++row].Value = "Humidity";
            ws.Cells["B" + row].Value = "Value";
            if (hasHumi) ws.Cells["A" + ++row].Value = "Minimum";
            if (hasHumi) ws.Cells["B" + row].Value = hmin;
            if (hasHumi) ws.Cells["A" + ++row].Value = "Average";
            if (hasHumi) ws.Cells["B" + row].Value = havg;
            if (hasHumi) ws.Cells["A" + ++row].Value = "Maximum";
            if (hasHumi) ws.Cells["B" + row].Value = hmax;
            #endregion

            var pieChart = ws.Drawings.AddChart("crtRecordsCount", eChartType.PieExploded3D) as ExcelPieChart;
            //Set top left corner to row 1 column 2
            pieChart.SetPosition(2, 0, 3, 0);
            pieChart.SetSize(600, 400);
            pieChart.Series.Add(ExcelRange.GetAddress(4, 2, (hasTemp ? 2 : 0) + (hasPres ? 2 : 0) + (hasHumi ? 2 : 0), 2), ExcelRange.GetAddress(4, 1, 6, 1));
            pieChart.Title.Text = "Records Count";
            //Set datalabels and remove the legend
            pieChart.DataLabel.ShowCategory = true;
            pieChart.DataLabel.ShowPercent = false;
            pieChart.DataLabel.ShowLeaderLines = true;
            pieChart.Legend.Remove();

            if (hasTemp)
            {
                var tbarChart = ws.Drawings.AddChart("crtTemperatureInfo", eChartType.BarClustered) as ExcelBarChart;
                //Set top left corner to row 1 column 2
                tbarChart.SetPosition(10, 0, 3, 0);
                tbarChart.SetSize(600, 400);
                tbarChart.Series.Add(ExcelRange.GetAddress(10, 2, 10, 2), ExcelRange.GetAddress(10, 1, 10, 1));
                tbarChart.Series.Add(ExcelRange.GetAddress(16, 2, 16, 2), ExcelRange.GetAddress(16, 1, 16, 1));
                tbarChart.Series.Add(ExcelRange.GetAddress(22, 2, 22, 2), ExcelRange.GetAddress(22, 1, 22, 1));
                tbarChart.Title.Text = "Temperature Statistics";
                //Set datalabels and remove the legend
                tbarChart.DataLabel.ShowCategory = true;
                tbarChart.DataLabel.ShowPercent = false;
                tbarChart.DataLabel.ShowLeaderLines = true;
                tbarChart.Legend.Remove();
            }
            #endregion

            return excel;
        }

        private static void CollectGarbage(params object[] garbage)
        {
            for (int i = 0; i < garbage.Length; i++)
            {
                try
                {
                    Marshal.ReleaseComObject(garbage[i]);
                    garbage[i] = null;
                }
                catch { }
            }
            GC.Collect();
        }
    }
}