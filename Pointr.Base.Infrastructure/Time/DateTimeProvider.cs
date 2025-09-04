using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pointr.Base.Application.Interfaces;

namespace Pointr.Base.Infrastructure.Time;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow
    {
        get { return DateTime.UtcNow; }
    }
}
