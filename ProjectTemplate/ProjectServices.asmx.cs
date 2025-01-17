using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace ProjectTemplate
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]

    public class ProjectServices : System.Web.Services.WebService
    {
        ////////////////////////////////////////////////////////////////////////
        ///replace the values of these variables with your database credentials
        ////////////////////////////////////////////////////////////////////////
        private string dbID = "cis440template";
        private string dbPass = "!!Cis440";
        private string dbName = "cis440template";
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        ///call this method anywhere that you need the connection string!
        ////////////////////////////////////////////////////////////////////////
        private string getConString() {
            return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass;
        }
        ////////////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////////////
        //don't forget to include this decoration above each method that you want
        //to be exposed as a web service!
        [WebMethod(EnableSession = true)]
        /////////////////////////////////////////////////////////////////////////
        public string TestConnection()
        {
            try
            {
                string testQuery = "select * from test";

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                ////////////////////////////////////////////////////////////////////////
                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                string sqlSelect = "SELECT * from USERS";


                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                //a data adapter acts like a bridge between our command object and 
                //the data we are trying to get back and put in a table object
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);

                DataTable table = new DataTable();
                sqlDa.Fill(table);
                return "Success!";
            }
            catch (Exception e)
            {
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }

        [WebMethod(EnableSession = true)] //NOTICE: gotta enable session on each individual method
        public bool LogOn(string email, string password)
        {
            //we return this flag to tell them if they logged in or not
            bool success = false;

            //our connection string comes from our web.config file like we talked about earlier
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
            //NOTICE: we added admin to what we pull, so that we can store it along with the id in the session
            string sqlSelect = "SELECT USER_ID FROM USERS WHERE email= @emailVal and password = @passVal";

            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@emailVal", HttpUtility.UrlDecode(email));
            sqlCommand.Parameters.AddWithValue("@passVal", HttpUtility.UrlDecode(password));

            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            Account currentAccount;
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account
            if (sqlDt.Rows.Count > 0)
            {
                //if we found an account, store the id and admin status in the session
                //so we can check those values later on other method calls to see if they 
                //are 1) logged in at all, and 2) and admin or not
                Session["USER_ID"] = sqlDt.Rows[0]["USER_ID"];
                //currentAccount = (new Account
                //(
                  //  sqlDt.Rows[0]["USER_ID"].ToString(),
                  //  sqlDt.Rows[0]["PASSWORD"].ToString(),
                  //  sqlDt.Rows[0]["FIRST_NAME"].ToString(),
                  //  sqlDt.Rows[0]["LAST_NAME"].ToString(),
                  //  sqlDt.Rows[0]["EMAIL"].ToString(),
                   // sqlDt.Rows[0]["USERNAME"].ToString()
               // ));

                success = true;
            }
            //return the result!
            return success;
        }

        [WebMethod(EnableSession = true)]
        public string CreateUser(string firstName, string lastName, string email, string userName, string password)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string insertUser = "insert into USERS" +
                "(USER_NAME, PASSWORD, FIRST_NAME, LAST_NAME, EMAIL)" +
                " VALUES " +
                 "(@username,@password, @firstName, @lastName,@email);" +
                "SELECT FIRST_NAME FROM USERS ORDER BY JOIN_DATE DESC LIMIT 1;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(insertUser, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@username", HttpUtility.UrlDecode(userName));
            sqlCommand.Parameters.AddWithValue("@password", HttpUtility.UrlDecode(password));
            sqlCommand.Parameters.AddWithValue("@firstName", HttpUtility.UrlDecode(firstName));
            sqlCommand.Parameters.AddWithValue("@lastName", HttpUtility.UrlDecode(lastName));
            sqlCommand.Parameters.AddWithValue("@email", HttpUtility.UrlDecode(email));


            string accountID;

            sqlConnection.Open();
            try
            {
                accountID = Convert.ToString(sqlCommand.ExecuteScalar());
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return accountID.ToString();
        }


        [WebMethod(EnableSession = true)]
        public string AddArt(string userEmail, string url, string title, string description, string price)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string getUser = "SELECT USER_ID FROM USERS WHERE USER_NAME = @userEmail";
            string insertArt = "insert into ART" +
                "(USER_ID, TITLE, DESCRIPTION, PRICE, URL)" +
                " VALUES " +
                 "(@userID, @title, @description, @price, @url);" +
                "SELECT TITLE FROM ART ORDER BY POST_DATE DESC LIMIT 1;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand getUserCommand = new MySqlCommand(getUser, sqlConnection);
            MySqlCommand insertArtCommand = new MySqlCommand(insertArt, sqlConnection);

            getUserCommand.Parameters.AddWithValue("@userEmail", HttpUtility.UrlDecode(userEmail));

            insertArtCommand.Parameters.AddWithValue("@url", HttpUtility.UrlDecode(url));
            insertArtCommand.Parameters.AddWithValue("@title", HttpUtility.UrlDecode(title));
            insertArtCommand.Parameters.AddWithValue("@description", HttpUtility.UrlDecode(description));
            insertArtCommand.Parameters.AddWithValue("@price", HttpUtility.UrlDecode(price));

            int userID;
            string returnTitle;

            sqlConnection.Open();
            try
            {
                userID = Convert.ToInt32(getUserCommand.ExecuteScalar());
                insertArtCommand.Parameters.AddWithValue("@userID", userID);
                try
                {
                    returnTitle = Convert.ToString(insertArtCommand.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

            }
            catch (Exception ex)
            {
                return "Wrong email address";
            }
            sqlConnection.Close();
            return returnTitle;

        }

        [WebMethod(EnableSession = true)]
        public string onLoadHome()
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            List<String> columnData = new List<String>();

            using (sqlConnection)
            {
                sqlConnection.Open();
                string query = "SELECT URL FROM ART";
                using (MySqlCommand command = new MySqlCommand(query, sqlConnection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columnData.Add(reader.GetString(0));
                        }
                    }
                }
            } //end using connection

            string jsonArt = Newtonsoft.Json.JsonConvert.SerializeObject(columnData);
            sqlConnection.Close();
            return jsonArt;
        }

        [WebMethod(EnableSession = true)]
        public string GetArtist(string url)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;

            string getArtistSQL = "SELECT CONCAT(FIRST_NAME, \" \", LAST_NAME) " +
                                        "FROM USERS U JOIN ART A ON A.USER_ID = U.USER_ID " +
                                        "WHERE URL = @inURL";
            string artist;

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand getArtistCommand = new MySqlCommand(getArtistSQL, sqlConnection);
            getArtistCommand.Parameters.AddWithValue("@inURL", HttpUtility.UrlDecode(url));

            sqlConnection.Open();
            try
            {
                artist = Convert.ToString(getArtistCommand.ExecuteScalar());
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            sqlConnection.Close();
            string a = Newtonsoft.Json.JsonConvert.SerializeObject(artist);
            return a;
        }

        ////EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
        //[WebMethod]
        //public Account[] GetAccounts()
        //{
        //    //check out the return type.  It's an array of Account objects.  You can look at our custom Account class in this solution to see that it's 
        //    //just a container for public class-level variables.  It's a simple container that asp.net will have no trouble converting into json.  When we return
        //    //sets of information, it's a good idea to create a custom container class to represent instances (or rows) of that information, and then return an array of those objects.  
        //    //Keeps everything simple.

        //    //LOGIC: get all the active accounts and return them!

        //    DataTable sqlDt = new DataTable("accounts");

        //    string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
        //    string sqlSelect = "select USER_ID, FIRST_NAME, LAST_NAME FROM USERS ";

        //    MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //    //gonna use this to fill a data table
        //    MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
        //    //filling the data table
        //    sqlDa.Fill(sqlDt);

        //    //loop through each row in the dataset, creating instances
        //    //of our container class Account.  Fill each acciount with
        //    //data from the rows, then dump them in a list.
        //    List<Account> accounts = new List<Account>();
        //    for (int i = 0; i < sqlDt.Rows.Count; i++)
        //    {
        //        accounts.Add(new Account
        //        {
        //            id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
        //            userId = sqlDt.Rows[i]["user_id"].ToString(),
        //            password = sqlDt.Rows[i]["pass"].ToString(),
        //            firstName = sqlDt.Rows[i]["firstname"].ToString(),
        //            lastName = sqlDt.Rows[i]["lastname"].ToString(),
        //            email = sqlDt.Rows[i]["email"].ToString()
        //        });
        //    }
        //    //convert the list of accounts to an array and return!
        //    return accounts.ToArray();
        //}
















    } //end class

} //end namespace
