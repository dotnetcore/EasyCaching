// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using dotnet_etcd.multiplexer;

using Etcdserverpb;

using Google.Protobuf;

using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient
    {
        private const string rangeEndString = "\x00";
        /// <summary>
        /// Converts RangeResponse to Dictionary
        /// </summary>
        /// <returns>IDictionary corresponding the RangeResponse</returns>
        /// <param name="resp">RangeResponse received from etcd server</param>
        private static IDictionary<string, string> RangeRespondToDictionary(RangeResponse resp)
        {
            Dictionary<string, string> resDictionary = new Dictionary<string, string>();
            foreach (Mvccpb.KeyValue kv in resp.Kvs)
            {
                resDictionary.Add(kv.Key.ToStringUtf8(), kv.Value.ToStringUtf8());
            }

            return resDictionary;
        }

        /// <summary>
        /// Gets the range end for prefix
        /// </summary>
        /// <returns>The range end for prefix</returns>
        /// <param name="prefixKey">Prefix key</param>
        public static string GetRangeEnd(string prefixKey)
        {
            if (prefixKey.Length == 0)
            {
                return rangeEndString;
            }

            StringBuilder rangeEnd = new StringBuilder(prefixKey);
            rangeEnd[index: rangeEnd.Length - 1] = ++rangeEnd[rangeEnd.Length - 1];
            return rangeEnd.ToString();
        }

        /// <summary>
        /// Gets the byte string for range requests
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ByteString GetStringByteForRangeRequests(string key) => key.Length == 0 ? ByteString.CopyFrom(0) : ByteString.CopyFromUtf8(key);

        /// <summary>
        /// Generic helper for performing actions an a connection.
        /// Gets the connection from the <seealso cref="Balancer"/>
        /// Also implements a retry mechanism if the calling methods returns an <seealso cref="RpcException"/> with the <seealso cref="StatusCode"/> <seealso cref="StatusCode.Unavailable"/>
        /// </summary>
        /// <typeparam name="TResponse">The type of the response that is returned from the call to etcd</typeparam>
        /// <param name="etcdCallFunc">The function to perform actions with the <seealso cref="Connection"/> object</param>
        /// <returns>The response from the the <paramref name="etcdCallFunc"/></returns>
        private TResponse CallEtcd<TResponse>(Func<Connection, TResponse> etcdCallFunc) => etcdCallFunc.Invoke(_connection);

        /// <summary>
        /// Generic helper for performing actions an a connection.
        /// Gets the connection from the <seealso cref="Balancer"/>
        /// Also implements a retry mechanism if the calling methods returns an <seealso cref="RpcException"/> with the <seealso cref="StatusCode"/> <seealso cref="StatusCode.Unavailable"/>
        /// </summary>
        /// <typeparam name="TResponse">The type of the response that is returned from the call to etcd</typeparam>
        /// <param name="etcdCallFunc">The function to perform actions with the <seealso cref="Connection"/> object</param>
        /// <returns>The response from the the <paramref name="etcdCallFunc"/></returns>
        private Task<TResponse> CallEtcdAsync<TResponse>(Func<Connection, Task<TResponse>> etcdCallFunc) => etcdCallFunc.Invoke(_connection);

        /// <summary>
        /// Generic helper for performing actions an a connection.
        /// Gets the connection from the <seealso cref="Balancer"/>
        /// Also implements a retry mechanism if the calling methods returns an <seealso cref="RpcException"/> with the <seealso cref="StatusCode"/> <seealso cref="StatusCode.Unavailable"/>
        /// </summary>
        /// <param name="etcdCallFunc">The function to perform actions with the <seealso cref="Connection"/> object</param>
        /// <returns>The response from the the <paramref name="etcdCallFunc"/></returns>
        private Task CallEtcdAsync(Func<Connection, Task> etcdCallFunc) => etcdCallFunc.Invoke(_connection);
    }
}
