/* 
 * Copyright 2015-2017 Takekazu Omi
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.IO;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
}


