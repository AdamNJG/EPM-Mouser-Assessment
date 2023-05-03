using EPM.Mouser.Interview.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMP.Mouser.Inverview.Application.Exceptions
{
    public class InvalidRequestException : Exception
    {
        public ErrorReason? errorReason;
        public InvalidRequestException(string message, ErrorReason? errorReason = null) : base(message) 
        {
            this.errorReason = errorReason ?? null;
        }
    }
}
