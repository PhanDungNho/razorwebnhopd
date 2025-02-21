using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace App.Security.Requirements
{
    public class GenZRequirement : IAuthorizationRequirement
    {
        public GenZRequirement(int formYear = 1997, int toYear = 2012)
        {
            FromYear = formYear;
            ToYear = toYear;
        }

        public int FromYear { get; set; }
        public int ToYear { get; set; }
    }
}