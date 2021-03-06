﻿using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Query;
using EllieMae.Encompass.Reporting;
using System;
using System.Collections.Generic;
using log4net;

namespace ReportFunded.Reports
{
    class FundedReport
    {

        //private Session session;
        private static readonly ILog log = LogManager.GetLogger(typeof(FundedReport));
        private List<Row> report;

        public FundedReport()
        {
            this.report = new List<Row>();
        }

        public String run()
        {

            DateTime timestamp = DateTime.Now;
            
            String html = HtmlReport.getHeader();
            html += "<body>";

            html += startApplication();

            html += HtmlReport.getFooter(timestamp);

            Console.Out.WriteLine("Report ready!");
            return html;
        }
        private String startApplication()
        {
            log.Info("Program running...");

            String text = "";

            DateFieldCriterion cri = new DateFieldCriterion();
            cri.FieldName = "Fields.Log.MS.Date.Funding";
            cri.Value = DateTime.Now;
            cri.Precision = DateFieldMatchPrecision.Day;

            QueryCriterion fullQuery = cri;

            StringList fields = new StringList();
            fields.Add("Fields.VEND.X263");
            fields.Add("Fields.352");
            fields.Add("Fields.364");
            fields.Add("Fields.37");
            fields.Add("Fields.4000");
            fields.Add("Fields.1109");
            fields.Add("Fields.362");
            fields.Add("Fields.317");

            LoanReportCursor results = Program.mySession.getSession().Reports.OpenReportCursor(fields, fullQuery);

            //LoanIdentityList ids = session.Loans.Query(fullQuery);

            int count = results.Count;
            log.Info("Total Files Funded " + cri.Value.ToShortDateString() + ": " + count);

            text += "Total Files Funded on " + cri.Value.ToShortDateString() + ": <b>" + count + "</b><br/><br/>";

            //headers
            Row row = new Row();
            row.setHeader(true);
            row.add("Investor");
            row.add("Inv #");
            row.add("Loan #");
            row.add("Borrower Name");
            row.add("Loan Amount");
            row.add("Processor");
            row.add("Loan Officer");
            report.Add(row);


            foreach (LoanReportData data in results)
            {
                Row line = new Row();

                line.add(data["Fields.VEND.X263"].ToString());
                line.add(data["Fields.352"].ToString());
                line.add(data["Fields.364"].ToString());
                line.add(data["Fields.37"].ToString().ToUpper() + ", " + data["Fields.4000"].ToString().ToUpper());

                line.add(Convert.ToInt32(data["Fields.1109"]).ToString("C"));
                line.add(data["Fields.362"].ToString());
                line.add(data["Fields.317"].ToString());

                report.Add(line);
                Console.Out.Write("."); //status bar
            }
            Console.Out.WriteLine("");
            results.Close();


            text += formatReport(report);

            return text;




        }


        private String formatReport(List<Row> report)
        {
            String text = "<table border='1'>";
            foreach (Row row in report)
            {
                row.toString();
                text += "<tr>";
                foreach (Col col in row.getRow())
                {
                    if (row.isHeader())
                    {
                        text += "<th>" + col.toString() + "</th>";
                    }
                    else
                    {
                        text += "<td>" + col.toString() + "</td>";
                    }
                }
                if (Program.debug)
                {
                    Console.Out.WriteLine("");
                }
                text += "</tr>";
            }
            text += "</table>";
            return text;
        }
    }
}