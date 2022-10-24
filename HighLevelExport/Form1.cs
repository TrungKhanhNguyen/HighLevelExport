using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Elasticsearch.Net;
using HighLevelExport.Helper;
using HighLevelExport.Models;
using MySql.Data;
using MySql.Data.MySqlClient;
using Nest;

namespace HighLevelExport
{
    public partial class Form1 : Form
    {
        private MySqlConnection connection;
        private DBHelper helper = new DBHelper();
        private string connectionString = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private List<ExportObject> getInterceptInfo()
        {
            
            connectionString = helper.getConnectionString();
            connection = new MySqlConnection(connectionString);
            var listObject = new List<ExportObject>();
            try
            {
                connection.Open();
                string sql = "select metadata from intellego_events.document where id = '51041346' and type=19";
                var cmd = new MySqlCommand(sql, connection);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {

                    //var tempObj = new ExportObject
                    //{
                    //    CaseName = rdr.GetString(0),
                    //    InterceptName = rdr.GetString(1),
                    //    InterceptId = rdr.GetString(2)
                    //};


                    var testmetada = rdr.GetString(0);
                    var indexmeta = testmetada.IndexOf("452-");
                    var teststrng = testmetada.Substring(indexmeta, 20);
                    //listObject.Add(tempObj);
                }
                connection.Close();
                return listObject;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message + connectionString);
                connection.Close();
                return null;
                //Insert to DB Failed
            }
        }

        private string getQuery(string interceptid, string fromDate, string endDate)
        {
            var query = @"{
            ""size"": 1000000,
            ""query"": {
                ""bool"": {
                    ""must"": [
                        {
                        ""bool"": {
                            ""must"": [
                                {
                                ""match"": {
                                    ""intercept"": ""intercepid""
                                }
                            }
                        ]
                    }
                    },
                {
                        ""bool"": {
                            ""must"": [
                                {
                                ""range"": {
                                    ""eventDate"": {
                                        ""from"": ""fromDate"",
                                        ""to"": ""endDate""
                                    }
                                }
                            }
                        ]
                    }
                    }
            ]
        }
            },
    ""docvalue_fields"": [
        {
                ""field"": ""eventDate"",
            ""format"": ""date_time""
        }
    ]
}";
            var returnQuery = query.Replace("intercepid", interceptid).Replace("fromDate",fromDate).Replace("endDate", endDate);
            return returnQuery;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var listObj = getInterceptInfo();
            var nodes = new Uri[]
            {
                new Uri("http://localhost:9200/"),
            };
            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming();
            //var elasticClient = new ElasticClient(connectionSettings);
            var elasticClient = new ElasticClient(connectionSettings.DefaultIndex("intellego"));
            var startTime = new DateTime(2022,09,15,0,0,0).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endTime = new DateTime(2022, 09, 15, 8, 0, 0).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var tempQuery = getQuery("4945",startTime, endTime);

            var searchResult = elasticClient.LowLevel.Search<SearchResponse<ElasticObject>>(tempQuery);
            var kkk = searchResult.Total;
            foreach (var item in searchResult.Hits)
            {
                var articleIds = item.Id;
                var source = item.Source;
                var sss = item.Fields["eventDate"].As<DateTime[]>().First();
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var tempStr = "\u90000452-42-10235-15u000uifhdjfdf";
            var indexS = tempStr.IndexOf("452-");
            var sw = tempStr.Substring(indexS, 5);

            var sss = "452-01-13006-0\0\0\0\v84";
            var indexofzero = sss.IndexOf("\0");
            var sdsd = sss.Remove(indexofzero);

            //getInterceptInfo();
        }
    }
}

