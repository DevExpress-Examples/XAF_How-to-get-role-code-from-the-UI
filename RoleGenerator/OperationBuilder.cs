using DevExpress.ExpressApp.Security;
using System;
using System.Collections.Generic;

namespace RoleGeneratorSpace
{
    [Flags]
    public enum Operations
    {
        None = 0,
        Read = 1,
        Write = 2,
        Create = 4,
        Delete = 8,
        Navigate = 16,
        FullObjectAccess = Read | Write | Delete | Navigate,
        Full = FullObjectAccess | Create,
        ReadOnly = Read | Navigate,
        CRUD = Create | Read | Write | Delete,
        ReadWrite = Read | Write
    }

    public class OperationBuilder
    {
        private Operations operationMask = Operations.None;
        private string operationString = string.Empty;
        private Dictionary<Operations, string> operationsDictionary;

        public OperationBuilder()
        {
            operationsDictionary = new Dictionary<Operations, string>
            {
                { Operations.Read, nameof(SecurityOperations.Read) },
                { Operations.Write, nameof(SecurityOperations.Write) },
                { Operations.Create, nameof(SecurityOperations.Create) },
                { Operations.Delete, nameof(SecurityOperations.Delete) },
                { Operations.Navigate, nameof(SecurityOperations.Navigate) },
                { Operations.FullObjectAccess, nameof(SecurityOperations.FullObjectAccess) },
                { Operations.Full, nameof(SecurityOperations.FullAccess) },
                { Operations.ReadOnly, nameof(SecurityOperations.ReadOnlyAccess) },
                { Operations.CRUD, nameof(SecurityOperations.CRUDAccess) },
                { Operations.ReadWrite, nameof(SecurityOperations.ReadWriteAccess) }
            };
        }
        public void AddOperation(Operations operation)
        {
            operationMask |= operation;
            operationString += SecurityOperations.Delimiter + Enum.GetName(typeof(Operations), operation);
        }
        public string GetOperations()
        {
            string result = string.Empty;
            if (operationString != string.Empty)
            {
                if (operationsDictionary.TryGetValue(operationMask, out result))
                {
                    result = typeof(SecurityOperations).Name + '.' + result;
                }
                else
                {
                    result = '"' + operationString.Substring(1) + '"';
                }
            }
            return result;
        }
    }
}
