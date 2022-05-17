using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using Server.Spells;

namespace Server.Engines.CustomRegions;

public class CustomRegion : GuardedRegion
{
    private readonly RegionControl _controller;
    private Timer _movePlayerTimer;

    public CustomRegion(RegionControl control) : base(control.RegionName, control.Map, control.RegionPriority, control.RegionArea)
    {
        Disabled = !control.IsGuarded;
        Music = control.Music;

        _controller = control;
    }

    public override void OnDeath(Mobile m)
    {
        if (m is { Deleted: false })
        {
            if (m is PlayerMobile && _controller.NoPlayerItemDrop)
            {
                if (m.Female)
                {
                    m.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
                    m.Body = 403;
                    m.Hidden = true;
                }
                else
                {
                    m.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
                    m.Body = 402;
                    m.Hidden = true;
                }

                m.Hidden = false;
            }
            else if (!(m is PlayerMobile) && _controller.NoNPCItemDrop)
            {
                if (m.Female)
                {
                    m.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
                    m.Body = 403;
                    m.Hidden = true;
                }
                else
                {
                    m.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
                    m.Body = 402;
                    m.Hidden = true;
                }

                m.Hidden = false;
            }

            // Start a 1 second timer
            // The Timer will check if they need moving, corpse deleting etc.
            _movePlayerTimer = new MovePlayerTimer(m, _controller);
            _movePlayerTimer.Start();
        }
    }

    private class MovePlayerTimer : Timer
    {
        private readonly Mobile _mobile;
        private readonly RegionControl _controller;

        public MovePlayerTimer(Mobile mobile, RegionControl controller) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
        {
            _mobile = mobile;
            _controller = controller;
        }

        protected override void OnTick()
        {
            // Empties the corpse and places items on ground
            if (_mobile is PlayerMobile)
            {
                if (_controller.EmptyPlayerCorpse)
                {
                    if (_mobile is { Corpse: { } })
                    {
                        List<Item> corpseItems = new List<Item>(_mobile.Corpse.Items);

                        foreach (Item item in corpseItems)
                        {
                            if (item.Layer != Layer.Bank && item.Layer != Layer.Backpack &&
                                item.Layer != Layer.Hair && item.Layer != Layer.FacialHair &&
                                item.Layer != Layer.Mount)
                            {
                                if (item.LootType != LootType.Blessed)
                                {
                                    item.MoveToWorld(_mobile.Corpse.Location, _mobile.Corpse.Map);
                                }
                            }
                        }
                    }
                }
            }
            else if (_controller.EmptyNPCCorpse)
            {
                if (_mobile is { Corpse: { } })
                {
                    List<Item> corpseItems = new List<Item>(_mobile.Corpse.Items);

                    foreach (Item item in corpseItems)
                    {
                        if (item.Layer != Layer.Bank && item.Layer != Layer.Backpack &&
                            item.Layer != Layer.Hair && item.Layer != Layer.FacialHair &&
                            item.Layer != Layer.Mount)
                        {
                            if ((item.LootType != LootType.Blessed))
                            {
                                item.MoveToWorld(_mobile.Corpse.Location, _mobile.Corpse.Map);
                            }
                        }
                    }
                }
            }

            Mobile newNpc = null;

            // Resurrects Players
            if (_mobile is PlayerMobile)
            {
                if (_controller.ResPlayerOnDeath)
                {
                    if (_mobile != null)
                    {
                        _mobile.Resurrect();
                        _mobile.SendMessage("You have been Resurrected");
                    }
                }
            }
            else if (_controller.ResNPCOnDeath)
            {
                if (_mobile is { Corpse: { } })
                {
                    Type type = _mobile.GetType();
                    newNpc = Activator.CreateInstance(type) as Mobile;

                    if (newNpc != null)
                    {
                        newNpc.Location = _mobile.Corpse.Location;
                        newNpc.Map = _mobile.Corpse.Map;
                    }
                }
            }

            // Deletes the corpse
            if (_mobile is PlayerMobile)
            {
                if (_controller.DeletePlayerCorpse)
                {
                    if (_mobile != null && _mobile.Corpse != null)
                    {
                        _mobile.Corpse.Delete();
                    }
                }
            }
            else if (_controller.DeleteNPCCorpse)
            {
                if (_mobile is { Corpse: { } })
                {
                    _mobile.Corpse.Delete();
                }
            }

            // Move Mobiles
            if (_mobile is PlayerMobile)
            {
                if (_controller.MovePlayerOnDeath)
                {
                    if (_mobile != null)
                    {
                        _mobile.Map = _controller.MovePlayerToMap;
                        _mobile.Location = _controller.MovePlayerToLoc;
                    }
                }
            }
            else if (_controller.MoveNPCOnDeath)
            {
                if (newNpc != null)
                {
                    newNpc.Map = _controller.MoveNpcToMap;
                    newNpc.Location = _controller.MoveNpcToLoc;
                }
            }

            Stop();
        }
    }

    public override bool IsDisabled()
    {
        if (!_controller.IsGuarded != Disabled)
        {
            _controller.IsGuarded = !Disabled;
        }

        return Disabled;
    }

    public override bool AllowBeneficial(Mobile from, Mobile target)
    {
        if ((!_controller.AllowBenefitPlayer && target is PlayerMobile) || (!_controller.AllowBenefitNPC && target is BaseCreature))
        {
            from.SendMessage("You cannot perform beneficial acts on your target.");
            return false;
        }

        return base.AllowBeneficial(from, target);
    }

    public override bool AllowHarmful(Mobile from, Mobile target)
    {
        if ((!_controller.AllowHarmPlayer && target is PlayerMobile) || (!_controller.AllowHarmNPC && target is BaseCreature))
        {
            from.SendMessage("You cannot perform harmful acts on your target.");
            return false;
        }

        return base.AllowHarmful(from, target);
    }

    public override bool AllowHousing(Mobile from, Point3D p) => _controller.AllowHousing;

    public override bool AllowSpawn() => _controller.AllowSpawn;

    public override bool CanUseStuckMenu(Mobile m)
    {
        if (!_controller.CanUseStuckMenu)
        {
            m.SendMessage("You cannot use the Stuck menu here.");
        }

        return _controller.CanUseStuckMenu;
    }

    public override bool OnDamage(Mobile m, ref int Damage)
    {
        if (!_controller.CanBeDamaged)
        {
            m.SendMessage("You cannot be damaged here.");
        }

        return _controller.CanBeDamaged;
    }

    public override bool OnResurrect(Mobile m)
    {
        if (!_controller.CanResurrect && m.AccessLevel == AccessLevel.Player)
        {
            m.SendMessage("You cannot resurrect here.");
        }

        return _controller.CanResurrect;
    }

    public override bool OnBeginSpellCast(Mobile from, ISpell s)
    {
        if (from.AccessLevel == AccessLevel.Player)
        {
            bool restricted = _controller.IsRestrictedSpell(s);
            if (restricted)
            {
                from.SendMessage("You cannot cast that spell here.");
                return false;
            }

            if (!_controller.CanMountEthereal && ((Spell)s).Info.Name == "Ethereal Mount")
            {
                from.SendMessage("You cannot mount your ethereal here.");
                return false;
            }
        }

        return true; //Let users customize spells, not rely on weather it's guarded or not.
    }

    public override bool OnDecay(Item item) => _controller.ItemDecay;

    public override bool OnHeal(Mobile m, ref int Heal)
    {
        if (!_controller.CanHeal)
        {
            m.SendMessage("You cannot be healed here.");
        }

        return _controller.CanHeal;
    }

    public override bool OnSkillUse(Mobile m, int skill)
    {
        bool restricted = _controller.IsRestrictedSkill(skill);

        if (restricted && m.AccessLevel == AccessLevel.Player)
        {
            m.SendMessage("You cannot use that skill here.");
            return false;
        }

        return base.OnSkillUse(m, skill);
    }

    public override void OnExit(Mobile m)
    {
        if (_controller.ShowExitMessage)
        {
            m.SendMessage("You have left {0}", Name);
        }

        base.OnExit(m);
    }

    public override void OnEnter(Mobile m)
    {
        if (_controller.ShowEnterMessage)
        {
            m.SendMessage("You have entered {0}", Name);
        }

        base.OnEnter(m);
    }

    public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
    {
        if (!_controller.CanEnter && !Contains(oldLocation))
        {
            m.SendMessage("You cannot enter this area.");
            return false;
        }

        return true;
    }

    public override TimeSpan GetLogoutDelay(Mobile m)
    {
        if (m.AccessLevel == AccessLevel.Player)
            return _controller.PlayerLogoutDelay;

        return base.GetLogoutDelay(m);
    }

    public override bool OnDoubleClick(Mobile m, object o)
    {
        if (o is BasePotion && !_controller.CanUsePotions)
        {
            m.SendMessage("You cannot drink potions here.");
            return false;
        }

        if (o is Corpse corpse)
        {
            bool canLoot;

            if (corpse.Owner == m)
            {
                canLoot = _controller.CanLootOwnCorpse;
            }
            else if (corpse.Owner is PlayerMobile)
            {
                canLoot = _controller.CanLootPlayerCorpse;
            }
            else
            {
                canLoot = _controller.CanLootNPCCorpse;
            }

            if (!canLoot)
            {
                m.SendMessage("You cannot loot that corpse here.");
            }

            if (m.AccessLevel >= AccessLevel.GameMaster && !canLoot)
            {
                m.SendMessage("This is not able to be looted but you are able to open that with your access.");
                return true;
            }

            return canLoot;
        }

        return base.OnDoubleClick(m, o);
    }

    public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
    {
        if (_controller.LightLevel >= 0)
        {
            global = _controller.LightLevel;
        }
        else
        {
            base.AlterLightLevel(m, ref global, ref personal);
        }
    }
}
