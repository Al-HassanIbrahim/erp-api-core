using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Enums
{
    public enum LeadStatus
    {
        New = 1,
        Contacted = 2,
        Qualified = 3,
        Proposal = 4,
        Negotiation = 5,
        Won = 6,
        Lost = 7
    }
    public enum  LeadSource
    {
        Website = 1,
        Referral = 2,
        ColdCall = 3,
        SocialMedia = 4,
        Email = 5,
        Event = 6,
        LinkedIn = 7
    }

    public enum  DealStatus
    {
        New = 1,
        Qualified = 2,
        Proposal = 3,
        Negotiation = 4,
    }
}
