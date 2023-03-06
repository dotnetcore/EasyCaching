// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Etcdserverpb;

using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient
    {
        /// <summary>
        /// LeaseGrant creates a lease which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public LeaseGrantResponse LeaseGrant(LeaseGrantRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._leaseClient
                                                                            .LeaseGrant(request, headers, deadline, cancellationToken));

        /// <summary>
        /// LeaseGrant creates a lease in async which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<LeaseGrantResponse> LeaseGrantAsync(LeaseGrantRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._leaseClient
                                                                            .LeaseGrantAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// LeaseRevoke revokes a lease. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public LeaseRevokeResponse LeaseRevoke(LeaseRevokeRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._leaseClient
                                                                            .LeaseRevoke(request, headers, deadline, cancellationToken));

        /// <summary>
        /// LeaseRevoke revokes a lease in async. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<LeaseRevokeResponse> LeaseRevokeAsync(LeaseRevokeRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._leaseClient
                                                                            .LeaseRevokeAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="leaseId"></param>
        /// <param name="cancellationToken"></param>
        public async Task LeaseKeepAlive(long leaseId, CancellationToken cancellationToken) => await CallEtcdAsync(async (connection) =>
                                                                                             {
                                                                                                 using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                                                                                                     connection._leaseClient.LeaseKeepAlive(cancellationToken: cancellationToken))
                                                                                                 {
                                                                                                     LeaseKeepAliveRequest request = new LeaseKeepAliveRequest()
                                                                                                     {
                                                                                                         ID = leaseId
                                                                                                     };

                                                                                                     while (true)
                                                                                                     {
                                                                                                         cancellationToken.ThrowIfCancellationRequested();

                                                                                                         await leaser.RequestStream.WriteAsync(request).ConfigureAwait(false);
                                                                                                         if (!await leaser.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                                                         {
                                                                                                             await leaser.RequestStream.CompleteAsync().ConfigureAwait(false);
                                                                                                             throw new EndOfStreamException();
                                                                                                         }

                                                                                                         LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                                                                                         if (update.ID != leaseId || update.TTL == 0) // expired
                                                                                                         {
                                                                                                             await leaser.RequestStream.CompleteAsync().ConfigureAwait(false);
                                                                                                             return;
                                                                                                         }

                                                                                                         await Task.Delay(TimeSpan.FromMilliseconds(update.TTL * 1000 / 3), cancellationToken).ConfigureAwait(false);
                                                                                                     }
                                                                                                 }
                                                                                             }).ConfigureAwait(false);

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="method"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse> method,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null) => await CallEtcdAsync(async (connection) =>
                                                                                     {
                                                                                         using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                                                                                             connection._leaseClient
                                                                                                 .LeaseKeepAlive(headers, cancellationToken: cancellationToken))
                                                                                         {
                                                                                             Task leaserTask = Task.Run(async () =>
                                                                                             {
                                                                                                 while (await leaser.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                                                 {
                                                                                                     LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                                                                                     method(update);
                                                                                                 }
                                                                                             }, cancellationToken);

                                                                                             await leaser.RequestStream.WriteAsync(request).ConfigureAwait(false);
                                                                                             await leaser.RequestStream.CompleteAsync().ConfigureAwait(false);
                                                                                             await leaserTask.ConfigureAwait(false);
                                                                                         }
                                                                                     }).ConfigureAwait(false);

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="methods"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse>[] methods,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null) => await CallEtcdAsync(async (connection) =>
                                                                                     {
                                                                                         using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                                                                                             connection._leaseClient
                                                                                                 .LeaseKeepAlive(headers, cancellationToken: cancellationToken))
                                                                                         {
                                                                                             Task leaserTask = Task.Run(async () =>
                                                                                             {
                                                                                                 while (await leaser.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                                                 {
                                                                                                     LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                                                                                     foreach (Action<LeaseKeepAliveResponse> method in methods)
                                                                                                     {
                                                                                                         method(update);
                                                                                                     }

                                                                                                 }
                                                                                             }, cancellationToken);

                                                                                             await leaser.RequestStream.WriteAsync(request).ConfigureAwait(false);
                                                                                             await leaser.RequestStream.CompleteAsync().ConfigureAwait(false);
                                                                                             await leaserTask.ConfigureAwait(false);
                                                                                         }
                                                                                     }).ConfigureAwait(false);


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="method"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse> method,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null) => await CallEtcdAsync(async (connection) =>
                                                                                     {
                                                                                         using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                                                                                             connection._leaseClient
                                                                                                 .LeaseKeepAlive(headers, cancellationToken: cancellationToken))
                                                                                         {
                                                                                             Task leaserTask = Task.Run(async () =>
                                                                                             {
                                                                                                 while (await leaser.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                                                 {
                                                                                                     LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                                                                                     method(update);
                                                                                                 }
                                                                                             }, cancellationToken);

                                                                                             foreach (LeaseKeepAliveRequest request in requests)
                                                                                             {
                                                                                                 await leaser.RequestStream.WriteAsync(request).ConfigureAwait(false);
                                                                                             }

                                                                                             await leaser.RequestStream.CompleteAsync().ConfigureAwait(false);
                                                                                             await leaserTask.ConfigureAwait(false);
                                                                                         }
                                                                                     }).ConfigureAwait(false);

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="methods"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse>[] methods,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null, DateTime? deadline = null) => await CallEtcdAsync(async (connection) =>
                                                                                                                {
                                                                                                                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                                                                                                                        connection._leaseClient
                                                                                                                            .LeaseKeepAlive(headers, deadline, cancellationToken))
                                                                                                                    {
                                                                                                                        Task leaserTask = Task.Run(async () =>
                                                                                                                        {
                                                                                                                            while (await leaser.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                                                                            {
                                                                                                                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                                                                                                                foreach (Action<LeaseKeepAliveResponse> method in methods)
                                                                                                                                {
                                                                                                                                    method(update);
                                                                                                                                }

                                                                                                                            }
                                                                                                                        }, cancellationToken);

                                                                                                                        foreach (LeaseKeepAliveRequest request in requests)
                                                                                                                        {
                                                                                                                            await leaser.RequestStream.WriteAsync(request).ConfigureAwait(false);
                                                                                                                        }

                                                                                                                        await leaser.RequestStream.CompleteAsync().ConfigureAwait(false);
                                                                                                                        await leaserTask.ConfigureAwait(false);
                                                                                                                    }
                                                                                                                }).ConfigureAwait(false);

        /// <summary>
        /// LeaseTimeToLive retrieves lease information.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public LeaseTimeToLiveResponse LeaseTimeToLive(LeaseTimeToLiveRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._leaseClient
                                                                            .LeaseTimeToLive(request, headers, deadline, cancellationToken));

        /// <summary>
        /// LeaseTimeToLive retrieves lease information.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The call object.</returns>
        public async Task<LeaseTimeToLiveResponse> LeaseTimeToLiveAsync(LeaseTimeToLiveRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._leaseClient
                                                                            .LeaseTimeToLiveAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);
    }
}
