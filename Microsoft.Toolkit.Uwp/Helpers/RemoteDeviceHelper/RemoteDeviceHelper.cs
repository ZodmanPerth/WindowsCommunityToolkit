﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System.RemoteSystems;

namespace Microsoft.Toolkit.Uwp.Helpers
{
    /// <summary>
    /// Helper to List Remote Devices that are accessible
    /// </summary>
    public class RemoteDeviceHelper
    {
        /// <summary>
        /// Gets a List of All Remote Systems based on Selection Filter
        /// </summary>
        public ObservableCollection<RemoteSystem> RemoteSystems { get; private set; }

        private RemoteSystemWatcher _remoteSystemWatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteDeviceHelper"/> class.
        /// </summary>
        public RemoteDeviceHelper()
        {
            RemoteSystems = new ObservableCollection<RemoteSystem>();
            GenerateSystems();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteDeviceHelper"/> class.
        /// </summary>
        public RemoteDeviceHelper(List<IRemoteSystemFilter> filter)
        {
            RemoteSystems = new ObservableCollection<RemoteSystem>();
            GenerateSystemsWithFilterAsync(filter);
        }

        /// <summary>
        /// Initiate Enumeration
        /// </summary>
        public void GenerateSystems()
        {
            GenerateSystemsWithFilterAsync(null);
        }

        /// <summary>
        /// Initiate Enumeration with specific RemoteSystemStatusType
        /// </summary>
        private void GenerateSystemsByFilters(RemoteSystemStatusType remoteSystemStatusType, RemoteSystemAuthorizationKind remoteSystemAuthorizationKind, RemoteSystemDiscoveryType remoteSystemDiscoveryType)
        {
            var remoteSystemStatusTypeFilter = new RemoteSystemStatusTypeFilter(remoteSystemStatusType);
            var remoteSystemDiscoveryTypeFilter = new RemoteSystemDiscoveryTypeFilter(remoteSystemDiscoveryType);
            var remoteSystemAuthorizationKindFilter = new RemoteSystemAuthorizationKindFilter(remoteSystemAuthorizationKind);
            var filters = new List<IRemoteSystemFilter>();
            if (remoteSystemStatusTypeFilter != null)
            {
                filters.Add(remoteSystemStatusTypeFilter);
            }

            if (remoteSystemDiscoveryTypeFilter != null)
            {
                filters.Add(remoteSystemDiscoveryTypeFilter);
            }

            if (remoteSystemAuthorizationKindFilter != null)
            {
                filters.Add(remoteSystemAuthorizationKindFilter);
            }

            GenerateSystemsWithFilterAsync(filters);
        }

        /// <summary>
        /// Initiate Enumeration with specific RemoteSysemKind with Filters
        /// </summary>
        private async void GenerateSystemsWithFilterAsync(List<IRemoteSystemFilter> filter)
        {
            var accessStatus = await RemoteSystem.RequestAccessAsync();
            if (accessStatus == RemoteSystemAccessStatus.Allowed)
            {
                _remoteSystemWatcher = filter != null ? RemoteSystem.CreateWatcher(filter) : RemoteSystem.CreateWatcher();
                _remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcher_RemoteSystemAdded;
                _remoteSystemWatcher.RemoteSystemRemoved += RemoteSystemWatcher_RemoteSystemRemoved;
                _remoteSystemWatcher.RemoteSystemUpdated += RemoteSystemWatcher_RemoteSystemUpdated;
                _remoteSystemWatcher.EnumerationCompleted += RemoteSystemWatcher_EnumerationCompleted;
                _remoteSystemWatcher.Start();
            }
        }

        private void RemoteSystemWatcher_EnumerationCompleted(RemoteSystemWatcher sender, RemoteSystemEnumerationCompletedEventArgs args)
        {
            _remoteSystemWatcher.Stop();
        }

        private async void RemoteSystemWatcher_RemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                RemoteSystems.Remove(RemoteSystems.First(a => a.Id == args.RemoteSystem.Id));
                RemoteSystems.Add(args.RemoteSystem);
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                RemoteSystems.Remove(RemoteSystems.First(a => a.Id == args.RemoteSystemId));
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                RemoteSystems.Add(args.RemoteSystem);
            });
        }
    }
}
