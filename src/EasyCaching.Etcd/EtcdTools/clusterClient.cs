// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

using Etcdserverpb;

namespace dotnet_etcd
{
    public partial class EtcdClient
    {
        /// <summary>
        /// MemberAdd adds a member into the cluster
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public MemberAddResponse MemberAdd(MemberAddRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._clusterClient
                                                                            .MemberAdd(request, headers, deadline, cancellationToken));

        /// <summary>
        /// MemberAddAsync adds a member into the cluster in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<MemberAddResponse> MemberAddAsync(MemberAddRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._clusterClient
                                                                            .MemberAddAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// MemberRemove removes an existing member from the cluster
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public MemberRemoveResponse MemberRemove(MemberRemoveRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._clusterClient
                                                                            .MemberRemove(request, headers, deadline, cancellationToken));

        /// <summary>
        /// MemberRemoveAsync removes an existing member from the cluster in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<MemberRemoveResponse> MemberRemoveAsync(MemberRemoveRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._clusterClient
                                                                            .MemberRemoveAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// MemberUpdate updates the member configuration
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public MemberUpdateResponse MemberUpdate(MemberUpdateRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._clusterClient
                                                                            .MemberUpdate(request, headers, deadline, cancellationToken));

        /// <summary>
        /// MemberUpdateAsync updates the member configuration in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<MemberUpdateResponse> MemberUpdateAsync(MemberUpdateRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._clusterClient
                                                                            .MemberUpdateAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// MemberList lists all the members in the cluster
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public MemberListResponse MemberList(MemberListRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._clusterClient
                                                                            .MemberList(request, headers, deadline, cancellationToken));

        /// <summary>
        /// MemberListAsync lists all the members in the cluster in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<MemberListResponse> MemberListAsync(MemberListRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._clusterClient
                                                                            .MemberListAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);
    }
}
