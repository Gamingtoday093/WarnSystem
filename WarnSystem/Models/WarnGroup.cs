﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnSystem.Models;

namespace WarnSystem.Models
{
    public class WarnGroup
    {
        public ulong SteamID { get; set; }
        public List<Warn> Warnings { get; set; }
    }
}
