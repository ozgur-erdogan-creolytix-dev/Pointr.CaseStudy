﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Domain.DomainEvents.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
