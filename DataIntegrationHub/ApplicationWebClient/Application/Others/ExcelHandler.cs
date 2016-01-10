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
            using (ExcelRange r = ws.Cells["A" + row + ":N" + row])
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
            ws.Cells["B" + row].Value = "Ammount";
            ws.Cells["A" + ++row].Value = "Temperature";
            ws.Cells["B" + row].Value = tcount;
            ws.Cells["A" + ++row].Value = "Pressure";
            ws.Cells["B" + row].Value = pcount;
            ws.Cells["A" + ++row].Value = "Humidity";
            ws.Cells["B" + row].Value = hcount;

            row++;
            ws.Cells["A" + ++row].Value = "Minimum Values";
            using (ExcelRange r = ws.Cells["A" + row + ":B" + row])
            {
                r.Merge = true;
                r.Style.Font.SetFromFont(new Font("Arial", 22, FontStyle.Italic));
                r.Style.Font.Color.SetColor(Color.White);
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }

            float tmin = lrecords.Where(r => r.Channel == "T").Min(r => r.Value);
            float pmin = lrecords.Where(r => r.Channel == "P").Min(r => r.Value);
            float hmin = lrecords.Where(r => r.Channel == "H").Min(r => r.Value);
            ws.Cells["A" + ++row].Value = "Channel";
            ws.Cells["B" + row].Value = "Minimum";
            ws.Cells["A" + ++row].Value = "Temperature";
            ws.Cells["B" + row].Value = tmin;
            ws.Cells["A" + ++row].Value = "Pressure";
            ws.Cells["B" + row].Value = pmin;
            ws.Cells["A" + ++row].Value = "Humidity";
            ws.Cells["B" + row].Value = hmin;

            row++;
            ws.Cells["A" + ++row].Value = "Average Values";
            using (ExcelRange r = ws.Cells["A" + row + ":B" + row])
            {
                r.Merge = true;
                r.Style.Font.SetFromFont(new Font("Arial", 22, FontStyle.Italic));
                r.Style.Font.Color.SetColor(Color.White);
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }


            float tavg = lrecords.Where(r => r.Channel == "T").Average(r => r.Value);
            float pavg = lrecords.Where(r => r.Channel == "P").Average(r => r.Value);
            float havg = lrecords.Where(r => r.Channel == "H").Average(r => r.Value);
            ws.Cells["A" + ++row].Value = "Channel";
            ws.Cells["B" + row].Value = "Average";
            ws.Cells["A" + ++row].Value = "Temperature";
            ws.Cells["B" + row].Value = tavg;
            ws.Cells["A" + ++row].Value = "Pressure";
            ws.Cells["B" + row].Value = pavg;
            ws.Cells["A" + ++row].Value = "Humidity";
            ws.Cells["B" + row].Value = havg;

            row++;
            ws.Cells["A" + ++row].Value = "Max Values";
            using (ExcelRange r = ws.Cells["A" + row + ":B" + row])
            {
                r.Merge = true;
                r.Style.Font.SetFromFont(new Font("Arial", 22, FontStyle.Italic));
                r.Style.Font.Color.SetColor(Color.White);
                r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }


            float tmax = lrecords.Where(r => r.Channel == "T").Max(r => r.Value);
            float pmax = lrecords.Where(r => r.Channel == "P").Max(r => r.Value);
            float hmax = lrecords.Where(r => r.Channel == "H").Max(r => r.Value);
            ws.Cells["A" + ++row].Value = "Channel";
            ws.Cells["B" + row].Value = "Max";
            ws.Cells["A" + ++row].Value = "Temperature";
            ws.Cells["B" + row].Value = tmax;
            ws.Cells["A" + ++row].Value = "Pressure";
            ws.Cells["B" + row].Value = pmax;
            ws.Cells["A" + ++row].Value = "Humidity";
            ws.Cells["B" + row].Value = hmax;
            #endregion

            var pieChart = ws.Drawings.AddChart("crtRecordsCount", eChartType.PieExploded3D) as ExcelPieChart;
            //Set top left corner to row 1 column 2
            pieChart.SetPosition(1, 0, 2, 0);
            pieChart.SetSize(400, 400);
            pieChart.Series.Add(ExcelRange.GetAddress(4, 2, 6, 2), ExcelRange.GetAddress(4, 1, 6, 1));
            pieChart.Title.Text = "Records Count";
            //Set datalabels and remove the legend
            pieChart.DataLabel.ShowCategory = true;
            pieChart.DataLabel.ShowPercent = true;
            pieChart.DataLabel.ShowLeaderLines = true;
            pieChart.Legend.Remove();

            using (ExcelRange r = ws.Cells["A1:N" + row])
            {
                r.Style.WrapText = false;
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