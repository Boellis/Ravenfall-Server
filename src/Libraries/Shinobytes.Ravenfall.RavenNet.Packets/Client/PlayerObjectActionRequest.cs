﻿namespace Shinobytes.Ravenfall.RavenNet.Packets.Client
{
    public class PlayerObjectActionRequest
    {
        public const short OpCode = 7;
        public int ObjectServerId { get; set; }
        public int ActionId { get; set; }
        public int ParameterId { get; set; }
    }
}
