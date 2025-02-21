using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace App.Security.Requirements
{
    public class ArticleUpdateRequirement : IAuthorizationRequirement
    {
        public ArticleUpdateRequirement(int year = 2025, int month = 1, int date = 1)
        {
            Year = year;
            Month = month;
            Date = date;
        }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Date { get; set; }
    }
}