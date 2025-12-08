using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Enums
{
    public enum InventoryDocType
    {
        In = 1,         
        Out = 2,        
        Transfer = 3,   
        Adjustment = 4,  
        Opening = 5     
    }

    public enum InventoryDocumentStatus
    {
        Draft = 1,   
        Posted = 2,
        Canceled = 3 
    }

    public enum InventoryLineType
    {
        In = 1,
        Out = 2
    }
}
