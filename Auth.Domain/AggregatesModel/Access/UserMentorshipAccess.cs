// <copyright file="UserMentorshipAccess.cs" company="LinaSys">
// Copyright (c) LinaSys. All rights reserved.
// </copyright>

using System;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Auth.Domain.AggregatesModel.Access
{
    /// <summary>
    /// Read model for tracking mentorship relationships.
    /// Synchronized via integration events from BusinessIncubator domain.
    /// </summary>
    public class UserMentorshipAccess : Entity
    {
        /// <summary>
        /// Gets the mentor user identifier.
        /// </summary>
        public string MentorUserId { get; private set; }

        /// <summary>
        /// Gets the starter user identifier.
        /// </summary>
        public string StarterUserId { get; private set; }

        /// <summary>
        /// Gets the project identifier.
        /// </summary>
        public long ProjectId { get; private set; }

        /// <summary>
        /// Gets the incubator identifier.
        /// </summary>
        public long IncubatorId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the mentorship is active.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets the date when the mentorship was assigned.
        /// </summary>
        public DateTime AssignedAt { get; private set; }

        /// <summary>
        /// Gets the date when the mentorship ended (if applicable).
        /// </summary>
        public DateTime? EndedAt { get; private set; }

        /// <summary>
        /// Gets the timestamp of the last synchronization.
        /// </summary>
        public DateTime LastSyncedAt { get; private set; }

        /// <summary>
        /// Creates a new instance of UserMentorshipAccess.
        /// </summary>
        /// <param name="mentorUserId">The mentor user identifier.</param>
        /// <param name="starterUserId">The starter user identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="incubatorId">The incubator identifier.</param>
        /// <param name="assignedAt">The assignment timestamp.</param>
        /// <returns>A new UserMentorshipAccess instance.</returns>
        public static UserMentorshipAccess Create(
            string mentorUserId,
            string starterUserId,
            long projectId,
            long incubatorId,
            DateTime assignedAt)
        {
            if (string.IsNullOrWhiteSpace(mentorUserId))
            {
                throw new ArgumentException("El identificador del mentor no puede estar vacío.", nameof(mentorUserId));
            }

            if (string.IsNullOrWhiteSpace(starterUserId))
            {
                throw new ArgumentException("El identificador del emprendedor no puede estar vacío.", nameof(starterUserId));
            }

            if (projectId <= 0)
            {
                throw new ArgumentException("El identificador del proyecto debe ser mayor que cero.", nameof(projectId));
            }

            if (incubatorId <= 0)
            {
                throw new ArgumentException("El identificador de la incubadora debe ser mayor que cero.", nameof(incubatorId));
            }

            return new UserMentorshipAccess
            {
                MentorUserId = mentorUserId,
                StarterUserId = starterUserId,
                ProjectId = projectId,
                IncubatorId = incubatorId,
                IsActive = true,
                AssignedAt = assignedAt,
                LastSyncedAt = assignedAt,
            };
        }

        /// <summary>
        /// Ends the mentorship relationship.
        /// </summary>
        /// <param name="endedAt">The end timestamp.</param>
        public void End(DateTime endedAt)
        {
            IsActive = false;
            EndedAt = endedAt;
            LastSyncedAt = endedAt;
        }

        /// <summary>
        /// Reactivates the mentorship relationship.
        /// </summary>
        /// <param name="reactivatedAt">The reactivation timestamp.</param>
        public void Reactivate(DateTime reactivatedAt)
        {
            IsActive = true;
            EndedAt = null;
            AssignedAt = reactivatedAt;
            LastSyncedAt = reactivatedAt;
        }
    }
}