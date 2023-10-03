﻿using System;
using System.Collections.Generic;

namespace Onion_architecture.Domain.Entities.Workflows
{
    public class Workflow
    {
        public Guid WorkflowTemplateId { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyList<WorkflowStep> Steps { get; private set; }

        public Workflow(Guid workflowTemplateId, string name, List<WorkflowStep> steps)
        {
            WorkflowTemplateId = workflowTemplateId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }
    }
}
