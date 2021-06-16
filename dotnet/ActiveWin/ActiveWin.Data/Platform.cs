using System;
using System.Collections.Generic;

#nullable disable

namespace ActiveWin.Data
{
    public partial class Platform
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string CreatedAt { get; set; }
    }
}
