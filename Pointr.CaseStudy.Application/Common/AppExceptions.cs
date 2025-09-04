using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.CaseStudy.Application.Common;

public class NotFoundAppException : Exception
{
    public NotFoundAppException(string message)
        : base(message) { }
}

public class ValidationAppException : Exception
{
    public ValidationAppException(string message)
        : base(message) { }
}

public class ConcurrencyAppException : Exception
{
    public ConcurrencyAppException(string message, Exception? inner = null)
        : base(message, inner) { }
}
