﻿using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Query;
using EllieMae.Encompass.Reporting;
using ReportFunded;
using System;
using System.Collections.Generic;

public class NotCTCReport
{

    private Session session;
    private List<Row> report;

    public NotCTCReport()
    {
        this.report = new List<Row>();
    }

    public String run()
    {
        //connect
        session = Utility.ConnectToServer();

        DateTime timestamp = DateTime.Now;

        String text = "<html><head>";
        text += "<style>table,th,td{text-align:center;border:1px solid grey;border-collapse:collapse;padding:.5em;font-size:.9em;}.small{font-size:.7em;}</style>";
        text += "</head><body>";

        text += startApplication();

        text += "<div class='small'>*Data Sources from Encompass. If report is incorrect, please update information in Encompass.* </div>";
        text += "<div class='small'>Report completed in: " + DateTime.Now.Subtract(timestamp).ToString(@"ss\.fff") + " seconds</div> </body></html>";

        Console.Out.WriteLine("Report ready!");
        session.End();
        return text;
    }
    private String startApplication()
    {
        Console.Out.WriteLine("Program running...");

        String text = "";


        DateFieldCriterion cri = new DateFieldCriterion();
        cri.FieldName = "Fields.Log.MS.Date.Clear to Close";
        cri.Value = DateFieldCriterion.EmptyDate;
        cri.MatchType = OrdinalFieldMatchType.Equals;

        DateFieldCriterion cri2 = new DateFieldCriterion();
        cri2.FieldName = "Fields.Log.MS.Date.Started";
        cri2.Value = DateTime.Today.AddDays(-60);  //last 60 days
        cri2.MatchType = OrdinalFieldMatchType.GreaterThanOrEquals;

        StringFieldCriterion folderCri = new StringFieldCriterion();
        folderCri.FieldName = "Loan.LoanFolder";
        folderCri.Value = "My Pipeline";
        folderCri.MatchType = StringFieldMatchType.Exact;

        QueryCriterion fullQuery = folderCri.And(cri.And(cri2));

        StringList fields = new StringList();
        Row row = new Row();
        row.setHeader(true);
        
        row.add("Milestone");
        fields.Add("Fields.Log.MS.CurrentMilestone");
        
        row.add("Date Started");
        fields.Add("Fields.Log.MS.Date.Started");
        
        row.add("Date Submitted");
        fields.Add("Fields.Log.MS.Date.Submittal");
        
        row.add("Loan #");
        fields.Add("Fields.364");

        row.add("Borrower Name");
        fields.Add("Fields.4002");
        fields.Add("Fields.4000");
        
        row.add("Address");
        fields.Add("Fields.11");

        row.add("Loan Amount");
        fields.Add("Fields.1109");

        row.add("Purpose");
        fields.Add("Fields.19");

        row.add("Term");
        fields.Add("Fields.4");

        row.add("Rate");
        fields.Add("Fields.3");

        row.add("Locked Date");
        fields.Add("Fields.761");

        row.add("Processor");
        fields.Add("Fields.362");

        row.add("Loan Officer");
        fields.Add("Fields.317");
      
        report.Add(row);


        SortCriterionList sortOrder = new SortCriterionList();
        sortOrder.Add(new SortCriterion("Fields.Log.MS.Date.Started",SortOrder.Ascending));

        LoanReportCursor results = session.Reports.OpenReportCursor(fields, fullQuery, sortOrder);

        Console.Out.WriteLine(results.ToString());

        int count = results.Count;
        Console.Out.WriteLine("Total Files Not CTC " + ": " + count);

        text += "Total Files Not CTC last 60 days: <b>" + count + "</b><br/><br/>";

        
        //iterate through query and format
        foreach (LoanReportData data in results)
        {

            Row line = new Row();
            line.add(data["Fields.Log.MS.CurrentMilestone"].ToString());
            line.add(Convert.ToDateTime(data["Fields.Log.MS.Date.Started"]).ToShortDateString());
            line.add(Utility.toShortDate(data["Fields.Log.MS.Date.Submittal"]));
            line.add(data["Fields.364"].ToString());
            line.add(data["Fields.4002"].ToString().ToUpper()+", "+ data["Fields.4000"].ToString().ToUpper());
            line.add(data["Fields.11"].ToString().ToUpper());
            line.add(Convert.ToInt32(data["Fields.1109"]).ToString("C"));
            line.add(data["Fields.19"].ToString());
            line.add(Convert.ToInt32(data["Fields.4"]).ToString());
            line.add(Convert.ToDouble(data["Fields.3"]).ToString("F3"));
            line.add(Utility.toShortDate(data["Fields.761"]));
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
            foreach (String col in row.getRow())
            {
                if (row.isHeader())
                {
                    text += "<th>" + col + "</th>";
                }
                else
                {
                    text += "<td>" + col + "</td>";
                }
                if (Program.debug)
                {
                    Console.Out.Write(col + "\t");
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
