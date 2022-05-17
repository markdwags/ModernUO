using Server;
using System;
using System.Collections;
using Server.Items;
using Server.Spells;
using Server.Mobiles;

namespace Server.Regions
{
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
                        if (_mobile != null && _mobile.Corpse != null)
                        {
                            ArrayList corpseitems = new ArrayList(_mobile.Corpse.Items);

                            foreach (Item item in corpseitems)
                            {
                                if ((item.Layer != Layer.Bank) && (item.Layer != Layer.Backpack) &&
                                    (item.Layer != Layer.Hair) && (item.Layer != Layer.FacialHair) &&
                                    (item.Layer != Layer.Mount))
                                {
                                    if ((item.LootType != LootType.Blessed))
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
                    if (_mobile != null && _mobile.Corpse != null)
                    {
                        ArrayList corpseitems = new ArrayList(_mobile.Corpse.Items);

                        foreach (Item item in corpseitems)
                        {
                            if ((item.Layer != Layer.Bank) && (item.Layer != Layer.Backpack) && (item.Layer != Layer.Hair) &&
                                (item.Layer != Layer.FacialHair) && (item.Layer != Layer.Mount))
                            {
                                if ((item.LootType != LootType.Blessed))
                                {
                                    item.MoveToWorld(_mobile.Corpse.Location, _mobile.Corpse.Map);
                                }
                            }
                        }
                    }
                }

                Mobile newnpc = null;

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
                    if (_mobile != null && _mobile.Corpse != null)
                    {
                        Type type = _mobile.GetType();
                        newnpc = Activator.CreateInstance(type) as Mobile;
                        if (newnpc != null)
                        {
                            newnpc.Location = _mobile.Corpse.Location;
                            newnpc.Map = _mobile.Corpse.Map;
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
                    if (_mobile != null && _mobile.Corpse != null)
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
                    if (newnpc != null)
                    {
                        newnpc.Map = _controller.MoveNPCToMap;
                        newnpc.Location = _controller.MoveNPCToLoc;
                    }
                }

                Stop();
            }
        }

        public override bool IsDisabled()
        {
            if (!_controller.IsGuarded != Disabled)
                _controller.IsGuarded = !Disabled;

            return Disabled;
        }

        public override bool AllowBeneficial(Mobile from, Mobile target)
        {
            if ((!_controller.AllowBenefitPlayer && target is PlayerMobile) ||
                (!_controller.AllowBenefitNPC && target is BaseCreature))
            {
                from.SendMessage("You cannot perform benificial acts on your target.");
                return false;
            }

            return base.AllowBeneficial(from, target);
        }

        public override bool AllowHarmful(Mobile from, Mobile target)
        {
            if ((!_controller.AllowHarmPlayer && target is PlayerMobile) ||
                (!_controller.AllowHarmNPC && target is BaseCreature))
            {
                from.SendMessage("You cannot perform harmful acts on your target.");
                return false;
            }

            return base.AllowHarmful(from, target);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return _controller.AllowHousing;
        }

        public override bool AllowSpawn()
        {
            return _controller.AllowSpawn;
        }

        public override bool CanUseStuckMenu(Mobile m)
        {
            if (!_controller.CanUseStuckMenu)
                m.SendMessage("You cannot use the Stuck menu here.");
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
            if (!_controller.CanRessurect && m.AccessLevel == AccessLevel.Player)
                m.SendMessage("You cannot ressurect here.");
            return _controller.CanRessurect;
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

                //if ( s is EtherealSpell && !CanMountEthereal ) Grr, EthereealSpell is private :<
                if (!_controller.CanMountEthereal &&
                    ((Spell)s).Info.Name == "Ethereal Mount") //Hafta check with a name compare of the string to see if ethy
                {
                    from.SendMessage("You cannot mount your ethereal here.");
                    return false;
                }
            }

            //Console.WriteLine( m_Controller.GetRegistryNumber( s ) );

            //return base.OnBeginSpellCast( from, s );
            return true; //Let users customize spells, not rely on weather it's guarded or not.
        }

        public override bool OnDecay(Item item)
        {
            return _controller.ItemDecay;
        }

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
                m.SendMessage("You have left {0}", this.Name);

            base.OnExit(m);
        }

        public override void OnEnter(Mobile m)
        {
            if (_controller.ShowEnterMessage)
                m.SendMessage("You have entered {0}", this.Name);

            base.OnEnter(m);
        }

        public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
        {
            if (!_controller.CanEnter && !this.Contains(oldLocation))
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

            if (o is Corpse)
            {
                Corpse c = (Corpse)o;

                bool canLoot;

                if (c.Owner == m)
                    canLoot = _controller.CanLootOwnCorpse;
                else if (c.Owner is PlayerMobile)
                    canLoot = _controller.CanLootPlayerCorpse;
                else
                    canLoot = _controller.CanLootNPCCorpse;

                if (!canLoot)
                    m.SendMessage("You cannot loot that corpse here.");

                if (m.AccessLevel >= AccessLevel.GameMaster && !canLoot)
                {
                    m.SendMessage("This is unlootable but you are able to open that with your Godly powers.");
                    return true;
                }

                return canLoot;
            }

            return base.OnDoubleClick(m, o);
        }

        public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
        {
            if (_controller.LightLevel >= 0)
                global = _controller.LightLevel;
            else
                base.AlterLightLevel(m, ref global, ref personal);
        }

        /*public override bool CheckAccessibility(Item item, Mobile from)
        {

            if (item is BasePotion && !m_Controller.CanUsePotions)
            {
                from.SendMessage("You cannot drink potions here.");
                return false;
            }

            if (item is Corpse)
            {
                Corpse c = item as Corpse;

                bool canLoot;

                if (c.Owner == from)
                    canLoot = m_Controller.CanLootOwnCorpse;
                else if (c.Owner is PlayerMobile)
                    canLoot = m_Controller.CanLootPlayerCorpse;
                else
                    canLoot = m_Controller.CanLootNPCCorpse;

                if (!canLoot)
                    from.SendMessage("You cannot loot that corpse here.");

                if (from.AccessLevel >= AccessLevel.GameMaster && !canLoot)
                {
                    from.SendMessage("This is unlootable but you are able to open that with your Godly powers.");
                    return true;
                }

                return canLoot;
            }

            return base.CheckAccessibility(item, from);
        }*/
    }
}
