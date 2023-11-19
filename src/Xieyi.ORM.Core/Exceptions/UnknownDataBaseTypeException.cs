using System;

namespace Xieyi.ORM.Core.Exceptions
{
    public class UnknownDataBaseTypeException : ApplicationException
    {
        public UnknownDataBaseTypeException(string message) : base(message)
        {
        }
    }
}