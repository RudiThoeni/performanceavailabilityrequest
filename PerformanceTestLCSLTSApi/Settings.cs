using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceTestLCSLTSApi
{
    public class Settings
    {
        private readonly IConfiguration configuration;
        private readonly LTSCredentials ltsCredentials;
        private readonly LCSCredentials lcsCredentials;
        
        public Settings(IConfiguration configuration)
        {
            this.configuration = configuration;
            var ltsapi = this.configuration.GetSection("LTSApi");
            this.ltsCredentials = new LTSCredentials(ltsapi.GetValue<string>("serviceurl", ""), ltsapi.GetValue<string>("username", ""), ltsapi.GetValue<string>("password", ""), ltsapi.GetValue<string>("xltsclientid", ""));

            var lcs = this.configuration.GetSection("LcsConfig");
            this.lcsCredentials = new LTSCredentials(lcs.GetValue<string>("serviceurl", ""), lcs.GetValue<string>("username", ""), lcs.GetValue<string>("password", ""), lcs.GetValue<string>("messagepassword", ""));
        }

        public LTSCredentials LtsCredentials => this.ltsCredentials;
        public LCSCredentials LcsCredentials => this.lcsCredentials;
    }

    public class LTSCredentials
    {
        public LTSCredentials(string serviceurl, string username, string password, string ltsclientid)
        {
            this.serviceurl = serviceurl;
            this.ltsclientid = ltsclientid;
            this.username = username;
            this.password = password;            
        }

        public string serviceurl { get; set; }
        public string ltsclientid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }


    public class LCSCredentials
    {
        public LCSCredentials(string serviceurl, string username, string password, string messagepassword)
        {
            this.serviceurl = serviceurl;
            this.messagepassword = messagepassword;
            this.username = username;
            this.password = password;            
        }

        public string serviceurl { get; set; }
        public string messagepassword { get; set; }
        public string username { get; set; }
        public string password { get; set; }        
    }

}
