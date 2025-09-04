using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
