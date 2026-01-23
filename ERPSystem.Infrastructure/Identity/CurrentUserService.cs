using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ERPSystem.Infrastructure.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _http = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var value = _http.HttpContext?.User?.FindFirstValue("sub");
                return Guid.TryParse(value, out var id) ? id : Guid.Empty;
            }
        }

        public int CompanyId
        {
            get
            {
                var value = _http.HttpContext?.User?.FindFirstValue("companyId");
                return int.TryParse(value, out var companyId) ? companyId : 0;
            }
        }
    }
}
