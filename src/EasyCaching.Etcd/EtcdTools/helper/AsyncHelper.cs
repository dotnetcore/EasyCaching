// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet_etcd.helper
{
    /// <summary>
    /// Helps with running async code in sync methods
    /// </summary>
    /// <remarks>Based on the AsyncHelper of the Microsoft.AspNet.Identity project (https://web.archive.org/web/20200411071640/https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs)</remarks>
    public static class AsyncHelper
    {
        private static readonly TaskFactory MyTaskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Runs the provided async function provided in <paramref name="func"/> in a synchronous way
        /// </summary>
        /// <param name="func">A call to the async method</param>
        /// <example>
        /// <code>
        /// AsyncHelper.RunSync(SomeFunctionAsync)
        /// </code>
        /// or
        /// <code>
        /// AsyncHelper.RunSync(async () => await SomeFunctionAsync())
        /// </code>
        /// </example>
        public static void RunSync(Func<Task> func)
        {
            CultureInfo cultureUi = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            MyTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }
    }
}
