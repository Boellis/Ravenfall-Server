﻿using System;

namespace Shinobytes.Ravenfall.DataModels.Legacy
{

    public class ServerLogs
    {
        public long Id { get; set; }
        public ServerLogSeverity Severity { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }
}