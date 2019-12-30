using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
  public class OrcBomber : BaseCreature
  {
    private DateTime m_NextBomb;
    private int m_Thrown;

    [Constructible]
    public OrcBomber() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
    {
      Body = 182;
      BaseSoundID = 0x45A;

      SetStr(147, 215);
      SetDex(91, 115);
      SetInt(61, 85);

      SetHits(95, 123);

      SetDamage(1, 8);

      SetDamageType(ResistanceType.Physical, 75);
      SetDamageType(ResistanceType.Fire, 25);

      SetResistance(ResistanceType.Physical, 25, 35);
      SetResistance(ResistanceType.Fire, 30, 40);
      SetResistance(ResistanceType.Cold, 15, 25);
      SetResistance(ResistanceType.Poison, 15, 20);
      SetResistance(ResistanceType.Energy, 25, 30);

      SetSkill(SkillName.MagicResist, 70.1, 85.0);
      SetSkill(SkillName.Swords, 60.1, 85.0);
      SetSkill(SkillName.Tactics, 75.1, 90.0);
      SetSkill(SkillName.Wrestling, 60.1, 85.0);

      Fame = 2500;
      Karma = -2500;

      VirtualArmor = 30;

      PackItem(new SulfurousAsh(Utility.RandomMinMax(6, 10)));
      PackItem(new MandrakeRoot(Utility.RandomMinMax(6, 10)));
      PackItem(new BlackPearl(Utility.RandomMinMax(6, 10)));
      PackItem(new MortarPestle());
      PackItem(new LesserExplosionPotion());

      if (0.2 > Utility.RandomDouble())
        PackItem(new BolaBall());
    }

    public OrcBomber(Serial serial) : base(serial)
    {
    }

    public override string CorpseName => "an orcish corpse";
    public override InhumanSpeech SpeechType => InhumanSpeech.Orc;

    public override string DefaultName => "an orc bomber";

    public override bool CanRummageCorpses => true;

    public override OppositionGroup OppositionGroup => OppositionGroup.SavagesAndOrcs;

    public override void GenerateLoot()
    {
      AddLoot(LootPack.Average);
      AddLoot(LootPack.Meager);
    }

    public override bool IsEnemy(Mobile m)
    {
      if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
        return false;

      return base.IsEnemy(m);
    }

    public override void AggressiveAction(Mobile aggressor, bool criminal)
    {
      base.AggressiveAction(aggressor, criminal);

      Item item = aggressor.FindItemOnLayer(Layer.Helm);

      if (item is OrcishKinMask)
      {
        AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
        item.Delete();
        aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
        aggressor.PlaySound(0x307);
      }
    }

    public override void OnActionCombat()
    {
      Mobile combatant = Combatant;

      if (combatant?.Deleted != false || combatant.Map != Map || !InRange(combatant, 12) ||
          !CanBeHarmful(combatant) || !InLOS(combatant))
        return;

      if (DateTime.UtcNow >= m_NextBomb)
      {
        ThrowBomb(combatant);

        m_Thrown++;

        if (0.75 >= Utility.RandomDouble() && m_Thrown % 2 == 1) // 75% chance to quickly throw another bomb
          m_NextBomb = DateTime.UtcNow + TimeSpan.FromSeconds(3.0);
        else
          m_NextBomb = DateTime.UtcNow + TimeSpan.FromSeconds(5.0 + 10.0 * Utility.RandomDouble()); // 5-15 seconds
      }
    }

    public void ThrowBomb(Mobile m)
    {
      DoHarmful(m);

      MovingParticles(m, 0x1C19, 1, 0, false, true, 0, 0, 9502, 6014, 0x11D, EffectLayer.Waist, 0);

      new InternalTimer(m, this).Start();
    }

    public override void Serialize(IGenericWriter writer)
    {
      base.Serialize(writer);
      writer.Write(0);
    }

    public override void Deserialize(IGenericReader reader)
    {
      base.Deserialize(reader);
      int version = reader.ReadInt();
    }

    private class InternalTimer : Timer
    {
      private Mobile m_Mobile, m_From;

      public InternalTimer(Mobile m, Mobile from) : base(TimeSpan.FromSeconds(1.0))
      {
        m_Mobile = m;
        m_From = from;
        Priority = TimerPriority.TwoFiftyMS;
      }

      protected override void OnTick()
      {
        m_Mobile.PlaySound(0x11D);
        AOS.Damage(m_Mobile, m_From, Utility.RandomMinMax(10, 20), 0, 100, 0, 0, 0);
      }
    }
  }
}
