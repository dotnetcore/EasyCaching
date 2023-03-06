// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using dotnet_etcd.helper;

using Etcdserverpb;

using Google.Protobuf;

using Grpc.Core;

using static Mvccpb.Event.Types;

namespace dotnet_etcd
{
    /// <summary>
    /// WatchEvent class is used for retrieval of minimal
    /// data from watch events on etcd.
    /// </summary>
    public class WatchEvent
    {
        /// <summary>
        /// etcd Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// etcd value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// etcd watch event type (PUT,DELETE etc.)
        /// </summary>
        public EventType Type { get; set; }

    }

    public partial class EtcdClient
    {
        #region Watch Key

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the watch response to the method provided.
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(WatchRequest request, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(new WatchRequest[1] { request }, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the watch response to the methods provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(WatchRequest request, Action<WatchResponse>[] methods,
            Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(new WatchRequest[1] { request }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the keys according to the specified watch requests and
        /// passes the watch response to the method provided.
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(WatchRequest[] requests, Action<WatchResponse> method,
            Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(requests, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch requests and
        /// passes the watch response to the methods provided. 
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(WatchRequest[] requests, Action<WatchResponse>[] methods,
            Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcdAsync(async (connection) =>
                                                            {
                                                                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                                                                    connection._watchClient.Watch(headers, deadline, cancellationToken))
                                                                {
                                                                    Task watcherTask = Task.Run(async () =>
                                                                    {
                                                                        while (await watcher.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                        {
                                                                            WatchResponse update = watcher.ResponseStream.Current;
                                                                            foreach (Action<WatchResponse> method in methods)
                                                                            {
                                                                                method(update);
                                                                            }
                                                                        }
                                                                    }, cancellationToken);

                                                                    foreach (WatchRequest request in requests)
                                                                    {
                                                                        await watcher.RequestStream.WriteAsync(request).ConfigureAwait(false);
                                                                    }

                                                                    await watcher.RequestStream.CompleteAsync().ConfigureAwait(false);
                                                                    await watcherTask.ConfigureAwait(false);
                                                                }
                                                            });

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the method provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(WatchRequest request, Action<WatchEvent[]> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(new WatchRequest[1] { request }, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the methods provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(WatchRequest request, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(new WatchRequest[1] { request }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the method provided. 
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(WatchRequest[] requests, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(requests, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch requests and
        /// passes the minimal watch event data to the methods provided. 
        /// </summary>
        /// <param name="requests">Watch Request containing keys to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(WatchRequest[] requests, Action<WatchEvent[]>[] methods,
            Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcdAsync(async (connection) =>
                                                            {
                                                                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                                                                    connection._watchClient.Watch(headers, deadline, cancellationToken))
                                                                {
                                                                    Task watcherTask = Task.Run(async () =>
                                                                    {
                                                                        while (await watcher.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                        {
                                                                            WatchResponse update = watcher.ResponseStream.Current;
                                                                            foreach (Action<WatchEvent[]> method in methods)
                                                                            {
                                                                                method(update.Events.Select(i =>
                                                                                    {
                                                                                        return new WatchEvent
                                                                                        {
                                                                                            Key = i.Kv.Key.ToStringUtf8(),
                                                                                            Value = i.Kv.Value.ToStringUtf8(),
                                                                                            Type = i.Type
                                                                                        };
                                                                                    }).ToArray()
                                                                                );
                                                                            }
                                                                        }
                                                                    }, cancellationToken);

                                                                    foreach (WatchRequest request in requests)
                                                                    {
                                                                        await watcher.RequestStream.WriteAsync(request).ConfigureAwait(false);
                                                                    }

                                                                    await watcher.RequestStream.CompleteAsync().ConfigureAwait(false);
                                                                    await watcherTask.ConfigureAwait(false);
                                                                }
                                                            });


        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the watch response to the method provided.
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(WatchRequest request, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(new WatchRequest[1] { request }, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the watch response to the methods provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(WatchRequest request, Action<WatchResponse>[] methods,
            Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(new WatchRequest[1] { request }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the keys according to the specified watch requests and
        /// passes the watch response to the method provided.
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(WatchRequest[] requests, Action<WatchResponse> method,
            Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(requests, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch requests and
        /// passes the watch response to the methods provided. 
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(WatchRequest[] requests, Action<WatchResponse>[] methods,
            Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => AsyncHelper.RunSync(async () => await WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken).ConfigureAwait(false));

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the method provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(WatchRequest request, Action<WatchEvent[]> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(new WatchRequest[1] { request }, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the methods provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(WatchRequest request, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(new WatchRequest[1] { request }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the method provided. 
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(WatchRequest[] requests, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(requests, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches a key according to the specified watch requests and
        /// passes the minimal watch event data to the methods provided. 
        /// </summary>
        /// <param name="requests">Watch Request containing keys to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(WatchRequest[] requests, Action<WatchEvent[]>[] methods,
            Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => AsyncHelper.RunSync(async () => await WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken).ConfigureAwait(false));

        /// <summary>
        /// Watches the specified key and passes the watch response to the method provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string key, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(new string[1] { key }, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key and passes the watch response to the methods provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string key, Action<WatchResponse>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(new string[1] { key }, methods, headers, deadline, cancellationToken);


        /// <summary>
        /// Watches the specified keys and passes the watch response to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string[] keys, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(keys, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified keys and passes the watch response to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string[] keys, Action<WatchResponse>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string key in keys)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = ByteString.CopyFromUtf8(key),
                        ProgressNotify = true,
                        PrevKv = true
                    }
                };
                requests.Add(request);
            }

            Watch(requests.ToArray(), methods, headers, deadline, cancellationToken);
        }


        /// <summary>
        /// Watches the specified key and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string key, Action<WatchEvent[]> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(new string[1] { key }, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key and passes the minimal watch events data to the methods provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string key, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(new string[1] { key }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified keys and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string[] keys, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
            CancellationToken cancellationToken = default) => Watch(keys, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified keys and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string[] keys, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string key in keys)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = ByteString.CopyFromUtf8(key)
                    }
                };
                requests.Add(request);
            }

            Watch(requests.ToArray(), methods, headers, deadline, cancellationToken);
        }


        /// <summary>
        /// Watches the specified key and passes the watch response to the method provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(string key, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(new string[1] { key }, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key and passes the watch response to the methods provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(string key, Action<WatchResponse>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(new string[1] { key }, methods, headers, deadline, cancellationToken);


        /// <summary>
        /// Watches the specified keys and passes the watch response to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(string[] keys, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(keys, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified keys and passes the watch response to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(string[] keys, Action<WatchResponse>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string key in keys)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = ByteString.CopyFromUtf8(key),
                        ProgressNotify = true,
                        PrevKv = true
                    }
                };
                requests.Add(request);
            }

            return WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken);
        }


        /// <summary>
        /// Watches the specified key and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(string key, Action<WatchEvent[]> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(new string[1] { key }, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key and passes the minimal watch events data to the methods provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(string key, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(new string[1] { key }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified keys and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(string[] keys, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchAsync(keys, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified keys and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchAsync(string[] keys, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string key in keys)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = ByteString.CopyFromUtf8(key),
                        ProgressNotify = true,
                        PrevKv = true
                    }
                };
                requests.Add(request);
            }

            return WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken);
        }

        #endregion

        #region Watch Range of keys

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchRangeAsync(string path, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRangeAsync(new string[1] { path }, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the watch response to the methods provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchRangeAsync(string path, Action<WatchResponse>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRangeAsync(new string[1] { path }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchRangeAsync(string[] paths, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRangeAsync(paths, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchRangeAsync(string[] paths, Action<WatchResponse>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string path in paths)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = GetStringByteForRangeRequests(path),
                        RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(path)),
                        ProgressNotify = true,
                        PrevKv = true
                    }
                };
                requests.Add(request);
            }

            return WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchRangeAsync(string path, Action<WatchEvent[]> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRangeAsync(new string[1] { path }, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the methods provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchRangeAsync(string path, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRangeAsync(new string[1] { path }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchRangeAsync(string[] paths, Action<WatchEvent[]> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRangeAsync(paths, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public Task WatchRangeAsync(string[] paths, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string path in paths)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = GetStringByteForRangeRequests(path),
                        RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(path)),
                        ProgressNotify = true,
                        PrevKv = true
                    }
                };
                requests.Add(request);
            }

            return WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string path, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRange(new string[1] { path }, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the watch response to the methods provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string path, Action<WatchResponse>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRange(new string[1] { path }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string[] paths, Action<WatchResponse> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRange(paths, new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string[] paths, Action<WatchResponse>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string path in paths)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = GetStringByteForRangeRequests(path),
                        RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(path)),
                        ProgressNotify = true,
                        PrevKv = true
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string path, Action<WatchEvent[]> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRange(new string[1] { path }, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the methods provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string path, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRange(new string[1] { path }, methods, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string[] paths, Action<WatchEvent[]> method, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => WatchRange(paths, new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string[] paths, Action<WatchEvent[]>[] methods, Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string path in paths)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = GetStringByteForRangeRequests(path),
                        RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(path)),
                        ProgressNotify = true,
                        PrevKv = true
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken).ConfigureAwait(false));
        }

        #endregion
    }
}
