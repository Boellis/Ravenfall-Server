﻿namespace Shinobytes.Ravenfall.RavenNet.Packets.Client
{
    public class PlayerPlayerActionRequest
    {
        public const short OpCode = 32;
        public int PlayerId { get; set; }
        public int ActionId { get; set; }
        public int ParameterId { get; set; }
    }
}
