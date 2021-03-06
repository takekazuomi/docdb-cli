﻿/* 
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
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Documents;

namespace DocDB
{
    internal static class Extentions
    {
        public static string ToJoinedString(this NameValueCollection nvc, string separator, string valueMark)
        {
            return string.Join(separator, nvc.Cast<string>().Select(s => string.Format("{0}{1}{2}", s, valueMark, nvc[s])).ToArray());
        }
        public static string Dump<T>(this FeedResponse<T> feed)
        {
            return feed.ResponseHeaders.ToJoinedString("\n\t", " : ");
        }

        public static string[] DumpValue<T>(this ResourceResponse<T> response) where T : Resource, new()
        {
            var result = Enumerable.Empty<string>();
            if (response.ResponseHeaders != null)
            {
                result = response.ResponseHeaders.AllKeys.Select(
                    key => string.Format("\t{0}={1}", key, response.ResponseHeaders[key]));
            }

            var properties = response.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
                .Select(info => string.Format("{0}={1}", info.Name, info.GetValue(response)?.ToString()));

            return properties.Concat(result).ToArray();
        }
    }
}
