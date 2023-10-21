﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.Core
{
    public interface IPublisher
    {
        Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken);
    }
}
