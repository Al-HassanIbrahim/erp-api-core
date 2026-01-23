using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Core;

namespace ERPSystem.Application.Interfaces
{
    public interface IJwtTokenService
    {
        AuthResponse CreateToken(Guid userId,int companyId, string email, string[] roles);
    }
}
