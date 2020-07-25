﻿using GameServer.Managers;
using GameServer.Processors;
using RavenfallServer.Providers;
using RavenNest.BusinessLogic.Data;
using Shinobytes.Ravenfall.Data.Entities;
using Shinobytes.Ravenfall.RavenNet.Models;
using System;

public abstract class SkillObjectAction : EntityAction
{
    private readonly string skillName;
    private readonly int actionTime;
    
    private readonly IPlayerStatsProvider statsProvider;
    private readonly IPlayerInventoryProvider inventoryProvider;
    private readonly Random random = new Random();

    protected readonly IGameData GameData;
    protected readonly IWorldProcessor World;
    protected readonly IGameSessionManager Sessions;
    protected event EventHandler<AfterActionEventArgs> AfterAction;

    protected SkillObjectAction(
        int id,
        string name,
        string skillName,
        int actionTime,
        IGameData gameData,
        IWorldProcessor worldProcessor,
        IGameSessionManager gameSessionManager,
        IPlayerStatsProvider statsProvider,
        IPlayerInventoryProvider inventoryProvider)
        : base(id, name)
    {
        this.skillName = skillName;
        this.actionTime = actionTime;
        this.GameData = gameData;
        this.World = worldProcessor;
        this.Sessions = gameSessionManager;
        this.statsProvider = statsProvider;
        this.inventoryProvider = inventoryProvider;
    }

    public override bool Invoke(Player player, IEntity entity, int parameterId)
    {
        if (!(entity is GameObjectInstance obj))
        {
            return false;
        }

        var session = Sessions.Get(player);

        // if we are already interacting with this object
        // ignore it.
        if (session.Objects.HasAcquiredLock(obj, player))
        {
            return false;
        }

        if (!session.Objects.AcquireLock(obj, player))
        {
            return false;
        }

        var gobj = GameData.GetGameObject(obj.ObjectId);
        if (gobj.InteractItemType > 0)
        {
            var inventory = inventoryProvider.GetInventory(player.Id);
            var requiredItem = inventory.GetItemOfType(gobj.InteractItemType);
            if (requiredItem == null || requiredItem.Amount < 1)
            {
                return false;
            }

            var reqItem = GameData.GetItem(requiredItem.ItemId);
            if (reqItem.Equippable)
            {
                World.SetItemEquipState(player, reqItem, true);
            }
        }

        StartAnimation(player, obj);
        return true;
    }

    protected bool HandleObjectTick(Player player, GameObjectInstance obj, TimeSpan totalTime, TimeSpan deltaTime)
    {
        var session = Sessions.Get(player);
        if (!session.Objects.HasAcquiredLock(obj, player))
        {
            StopAnimation(player, obj);
            return true;
        }

        var skill = statsProvider.GetStatByName(player.Id, skillName);
        var chance = 0.05f + skill.EffectiveLevel * 0.05f;
        if (random.NextDouble() <= chance)
        {
            StartAnimation(player, obj);
            return false;
        }
        var gobj = GameData.GetGameObject(obj.ObjectId);
        var levelsGaiend = skill.AddExperience(gobj.Experience);
        var itemDrops = session.Objects.GetItemDrops(obj);

        foreach (var itemDrop in itemDrops)
        {
            if (random.NextDouble() > itemDrop.DropChance)
                continue;

            World.AddPlayerItem(player, GameData.GetItem(itemDrop.ItemId));
        }

        World.UpdatePlayerStat(player, skill);

        if (levelsGaiend > 0)
        {
            World.PlayerStatLevelUp(player, skill, levelsGaiend);
        }

        StopAnimation(player, obj);
        if (AfterAction != null)
        {
            AfterAction?.Invoke(this, new AfterActionEventArgs(player, obj));
        }
        return true;
    }

    protected void StartAnimation(Player player, GameObjectInstance obj)
    {
        World.PlayAnimation(player, skillName, true, true);
        World.SetEntityTimeout(actionTime, player, obj, HandleObjectTick);
    }

    protected void StopAnimation(Player player, GameObjectInstance obj)
    {
        var session = Sessions.Get(player);
        var gobj = GameData.GetGameObject(obj.ObjectId);
        var inventory = inventoryProvider.GetInventory(player.Id);
        var requiredItem = inventory.GetItemOfType(gobj.InteractItemType);
        if (requiredItem != null)
        {
            var reqItem = GameData.GetItem(requiredItem.ItemId);
            if (reqItem.Equippable)
            {
                World.SetItemEquipState(player, reqItem, false);
            }
            if (reqItem.Consumable)
            {
                World.RemovePlayerItem(player, reqItem);
            }
            World.PlayAnimation(player, skillName, false);
        }

        session.Objects.ReleaseLocks(player);
    }
}
