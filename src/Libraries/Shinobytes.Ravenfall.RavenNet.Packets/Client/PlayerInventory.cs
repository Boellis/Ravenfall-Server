﻿using Shinobytes.Ravenfall.RavenNet.Models;
using System.Collections.Generic;

namespace Shinobytes.Ravenfall.RavenNet.Packets.Client
{
    public class PlayerInventory
    {
        public const short OpCode = 26;
        public int PlayerId { get; set; }
        public int[] ItemId { get; set; }
        public long[] Amount { get; set; }
        public long Coins { get; set; }
        public static PlayerInventory Create(Player player, IReadOnlyList<InventoryItem> items)
        {
            var itemIds = new int[items.Count];
            var amounts = new long[items.Count];
            for (var i = 0; i < items.Count; ++i)
            {
                itemIds[i] = items[i].ItemId;
                amounts[i] = items[i].Amount;
            }

            return new PlayerInventory
            {
                PlayerId = player.Id,
                Amount = amounts,
                ItemId = itemIds,
                Coins = player.Coins
            };
        }
    }
}
