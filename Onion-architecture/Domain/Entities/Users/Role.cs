﻿using System;

namespace Onion_architecture.Domain.Entities.Users
{
    public class Role
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public Role(string name)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
