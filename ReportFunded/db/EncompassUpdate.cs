﻿using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Query;
using EllieMae.Encompass.Reporting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportFunded.db
{
    class EncompassUpdate
    {
        private Session session;
        private DateTime timestamp;
        //private Dictionary<string, int> processors;

        public String run()
        {
            this.timestamp = DateTime.Now;

            //connect
            this.session = Utility.ConnectToServer();
            //this.processors = getProcessors();

            startApplication();

            Console.Out.WriteLine("db update finished!");
            session.End();
            return "";
        }

        private Dictionary<string, int> getProcessors()
        {
            Dictionary<string, int> processorList = new Dictionary<string, int>();
            //query list for mySql
            List<MySqlCommand> queries = new List<MySqlCommand>();
            //String timestamp = 
            db_connect connection = new db_connect();
            connection.connect();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection.getConnection();

            cmd.CommandText = string.Format("SELECT id, name FROM e_processors");

            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                processorList.Add(reader.GetString(1), reader.GetInt32(0));
            }
            reader.Close();

            return processorList;
        }

        private void startApplication()
        {
            Console.Out.WriteLine("Program running...");

            StringFieldCriterion folderCri = new StringFieldCriterion();
            folderCri.FieldName = "Loan.LoanFolder";
            folderCri.Value = "My Pipeline";
            folderCri.MatchType = StringFieldMatchType.Exact;

            QueryCriterion fullQuery = folderCri;

            StringList fields = new StringList();
            fields.Add("Fields.VEND.X263");
            fields.Add("Fields.352");
            fields.Add("Fields.364");
            fields.Add("Fields.37");
            fields.Add("Fields.4000");
            fields.Add("Fields.1109");
            fields.Add("Fields.362");
            fields.Add("Fields.317");
            fields.Add("Fields.362"); //processor
            fields.Add("Fields.317"); //loan officer
            fields.Add("Fields.Log.MS.CurrentMilestone");
            fields.Add("Fields.Log.MS.Date.Started");
            fields.Add("Fields.Log.MS.Date.Submittal");
            fields.Add("Fields.Log.MS.Date.Funding");
            fields.Add("Fields.Log.MS.Date.Purchased");
            fields.Add("Fields.19"); //loan purpose
            fields.Add("Fields.4"); //loan term
            fields.Add("Fields.3"); //interest rate
            fields.Add("Fields.761"); //locked date

            LoanReportCursor results = session.Reports.OpenReportCursor(fields, fullQuery);
            //LoanIdentityList ids = session.Loans.Query(fullQuery);

            int total = results.Count;
            int count = 0;
            Console.Out.WriteLine("Total Files: " + total);


            //query list for mySql
            List<MySqlCommand> queries = new List<MySqlCommand>();
            //String timestamp = 
            db_connect connection = new db_connect();
            connection.connect();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection.getConnection();


            foreach (LoanReportData data in results)
            {
                Dictionary<String, String> map = new Dictionary<String, String>();

                map.Add("investor", data["Fields.VEND.X263"].ToString().ToUpper());
                map.Add("investorNum", data["Fields.352"].ToString().ToUpper());
                //map.Add("timestamp", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                map.Add("b1_lname", data["Fields.37"].ToString().ToUpper());
                map.Add("b1_fname", data["Fields.4000"].ToString().ToUpper());
                map.Add("loanAmt", Convert.ToInt32(data["Fields.1109"]).ToString("C"));
                map.Add("loanNum", data["Fields.364"].ToString());
                map.Add("currentMilestone", data["Fields.Log.MS.CurrentMilestone"].ToString());


                String startedDate = (data["Fields.Log.MS.Date.Started"] == null) ? null : Convert.ToDateTime(data["Fields.Log.MS.Date.Started"]).ToString("yyyy-MM-dd");
                map.Add("startedDate", startedDate);

                String submittalDate = (data["Fields.Log.MS.Date.Submittal"] == null) ? null : Convert.ToDateTime(data["Fields.Log.MS.Date.Submittal"]).ToString("yyyy-MM-dd");
                map.Add("submittalDate", submittalDate);

                String fundedDate = (data["Fields.Log.MS.Date.Funding"] == null) ? null : Convert.ToDateTime(data["Fields.Log.MS.Date.Funding"]).ToString("yyyy-MM-dd");
                map.Add("fundedDate", fundedDate);

                String purchasedDate = (data["Fields.Log.MS.Date.Purchased"] == null) ? null : Convert.ToDateTime(data["Fields.Log.MS.Date.Purchased"]).ToString("yyyy-MM-dd");
                map.Add("purchasedDate", purchasedDate);

                map.Add("processor", data["Fields.362"].ToString());
                map.Add("loanOfficer", data["Fields.317"].ToString());

                /*cmd.CommandText = string.Format(
                    "INSERT INTO loans(guid, investor, investorNum, createdAt, b1_lname, b1_fname, loanAmt, loanNum, fundedDate, processor, loan_officer, purchasedDate) " + 
                    "values(@v1, @v2, @v3, @createdAt, @v4, @v5, @v6, @v7, @v8, @v9, @v10, " + 
                    "@v11, @currMilestone) " +
                    "ON DUPLICATE KEY UPDATE guid=@v1, investor=@v2, investorNum=@v3, updatedAt=@updatedAt, b1_lname=@v4, b1_fname=@v5, "+
                    "loanAmt=@v6, loanNum=@v7, fundedDate=@v8, processor=@v9, loan_officer=@v10, " + 
                    "purchasedDate=@v11"
                    );*/

                var now  = timestamp.ToString("yyyy - MM - dd HH: mm:ss");
          

                StringBuilder insert = new StringBuilder();
                insert.Append("INSERT INTO loans(guid, createdAt ");
                //cols
                foreach(String key in map.Keys)
                {
                    insert.Append(", " + key);
                }
                //values
                insert.Append(") ");
                StringBuilder values = new StringBuilder();
                values.Append("VALUES('" + data.Guid + "', '" + now + "'");
                foreach(String key in map.Keys)
                {
                    if (map[key] != null)
                    {
                        values.Append(", '" + MySqlHelper.EscapeString(" " + map[key]) + "'");
                    }
                    else
                    {
                        values.Append(", NULL");
                    }
                }
                values.Append(") ");
                StringBuilder update2 = new StringBuilder();
                update2.Append("ON DUPLICATE KEY UPDATE updatedAt='" + now + "'");
                foreach(String key in map.Keys)
                {
                    if (map[key] != null)
                    {
                        update2.Append(", " + key + "='" + MySqlHelper.EscapeString(" " + map[key]) + "'");
                    }
                    else
                    {
                        update2.Append(", " + key + "=NULL");
                    }
                }

                //mysql command
                cmd.CommandText = insert.ToString() + values.ToString() + update2.ToString();

               /* cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@v1", data.Guid);
                cmd.Parameters.AddWithValue("@v2", map["investor"]);
                cmd.Parameters.AddWithValue("@v3", map["investorNum"]);
                cmd.Parameters.AddWithValue("@createdAt", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@updatedAt", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@v4", map["b1_lname"]);
                cmd.Parameters.AddWithValue("@v5", map["b1_fname"]);
                cmd.Parameters.AddWithValue("@v6", map["loanAmt"]);
                cmd.Parameters.AddWithValue("@v7", map["loanNum"]);
                cmd.Parameters.AddWithValue("@v8", map["fundedDate"]);
                cmd.Parameters.AddWithValue("@v9", map["processor"]);
                cmd.Parameters.AddWithValue("@v10", map["loanOfficer"]);                    
                cmd.Parameters.AddWithValue("@v11", map["purchasedDate"]);
                cmd.Prepare();*/

                cmd.ExecuteNonQuery();

                //status output
                count++;
                if(count % 10 == 0)
                {
                    Console.Write("\r{0}   ", "writing: " + count + "/" + total);
                }

            }
            Console.Out.WriteLine("");
            results.Close();

            TimeSpan time = DateTime.Now.Subtract(timestamp);

            connection.addLog("db_update", "updated: " + total + " rows in " + 
                String.Format("{0} min {1}.{2} sec"
                    , time.Minutes
                    , time.Seconds
                    , time.Milliseconds) );

            connection.close();

            Console.Out.WriteLine("Finished updating db");



        }
    }
}
