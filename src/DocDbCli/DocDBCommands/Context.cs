using System;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace DocDB
{
    public class Context
    {
        public string EndPoint { get; set; }

        // TODO take care here is not secure
        public string AuthorizationKey { get; set; }

        public string DatabaseName { get; set; }
        public string DataCollectionName { get; set; }
        public FeedOptions FeedOptions { get; set; }
        public ConnectionPolicy ConnectionPolicy { get; set; }

        [JsonIgnore]
        public int Verbose { get; set; }

        [JsonIgnore]
        public bool Help { get; set; }

        public Context()
        {
            ConnectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp,
                RequestTimeout = new TimeSpan(1, 0, 0),
                MaxConnectionLimit = 1000,
                RetryOptions = new RetryOptions
                {
                    MaxRetryAttemptsOnThrottledRequests = 10,
                    MaxRetryWaitTimeInSeconds = 60
                }
            };
            FeedOptions = new FeedOptions {EnableCrossPartitionQuery = true};
        }

        private static string GetDotFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".docdbcli");
        }

        public void Apply(Context context)
        {
            EndPoint = context.EndPoint ?? EndPoint;
            AuthorizationKey = context.AuthorizationKey ?? AuthorizationKey;
            DatabaseName = context.DatabaseName ?? DatabaseName;
            DataCollectionName = context.DataCollectionName ?? DataCollectionName;
            // always overwrite
            Verbose = context.Verbose;
            Help = context.Help;
        }

        public static Context ReadFromFile(string path = null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    path = GetDotFilePath();
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    return JsonConvert.DeserializeObject<Context>(json);
                }
            }
            catch (Exception e)
            {
                Log.Warning(e, ".docdbcli deserialize error");
            }
            return new Context();
        }

        public void WriteToFile(string path = null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    path = GetDotFilePath();
                var json = JsonConvert.SerializeObject(this, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        Converters = new JsonConverter[] {new StringEnumConverter()}
                    });
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Log.Warning(e, ".docdbcli serialize error");
            }
        }
    }
 
    //public class ShouldSerializeContractResolver : DefaultContractResolver
    //    {
    //        public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();
    //        public static readonly string[] PropertyNames = { };

    //    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    //    {
    //        JsonProperty property = base.CreateProperty(member, memberSerialization);

    //        if (property.DeclaringType == typeof(Employee) && property.PropertyName == "Manager")
    //        {
    //            property.ShouldSerialize =
    //                instance =>
    //                {
    //                    Employee e = (Employee)instance;
    //                    return e.Manager != e;
    //                };
    //    }

    //    return property;
    //}
}


