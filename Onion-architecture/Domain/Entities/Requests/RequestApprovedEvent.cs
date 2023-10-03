﻿using System;
using Onion_architecture.Domain.BaseObjectsNamespace;

namespace Onion_architecture.Domain.Entities.Requests
{
    public class RequestApprovedEvent : IEvent
    {
        public Guid Id { get; private set; }
        public DateTime Date { get; private set; }
        public Guid RequestId { get; private set; }

        public RequestApprovedEvent(Guid requestId)
        {
            Id = Guid.NewGuid();
            Date = DateTime.UtcNow;
            RequestId = requestId;
        }
    }
}
