﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public record DomainEvent<TAggregateId>(
        IEvent Data,
        IEventMetadata<TAggregateId> Metadata
    );
}
