// <copyright file="UserRemovedFromIncubatorIntegrationEvent.cs" company="LinaSys">
// Copyright (c) LinaSys. All rights reserved.
// </copyright>

using System;
using MediatR;

namespace LinaSys.Shared.Application.IntegrationEvents.Auth
{
    /// <summary>
    /// Integration event raised when a user is removed from an incubator.
    /// Used by Auth domain to update incubator access read models.
    /// </summary>
    public record UserRemovedFromIncubatorIntegrationEvent(
        string UserId,
        long IncubatorId,
        string Reason,
        DateTime OccurredAt) : INotification;
}