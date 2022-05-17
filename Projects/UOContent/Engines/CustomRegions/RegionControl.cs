using System;
using System.Collections;
using System.Collections.Generic;
using Server.Spells;

namespace Server.Engines.CustomRegions;

[Flags]
public enum RegionFlag : uint
{
    None = 0x00000000,
    AllowBenefitPlayer = 0x00000001,
    AllowHarmPlayer = 0x00000002,
    AllowHousing = 0x00000004,
    AllowSpawn = 0x00000008,

    CanBeDamaged = 0x00000010,
    CanHeal = 0x00000020,
    CanResurrect = 0x00000040,
    CanUseStuckMenu = 0x00000080,
    ItemDecay = 0x00000100,

    ShowEnterMessage = 0x00000200,
    ShowExitMessage = 0x00000400,

    AllowBenefitNpc = 0x00000800,
    AllowHarmNpc = 0x00001000,

    CanMountEthereal = 0x000002000,

    CanEnter = 0x000004000,

    CanLootPlayerCorpse = 0x000008000,
    CanLootNpcCorpse = 0x000010000,

    CanLootOwnCorpse = 0x000020000,

    CanUsePotions = 0x000040000,

    IsGuarded = 0x000080000,

    EmptyNpcCorpse = 0x000400000,
    EmptyPlayerCorpse = 0x000800000,
    DeleteNpcCorpse = 0x001000000,
    DeletePlayerCorpse = 0x002000000,
    ResNpcOnDeath = 0x004000000,
    ResPlayerOnDeath = 0x008000000,
    MoveNpcOnDeath = 0x010000000,
    MovePlayerOnDeath = 0x020000000,

    NoPlayerItemDrop = 0x040000000,
    NoNPCItemDrop = 0x080000000
}

public class RegionControl : Item
{
    public static List<RegionControl> AllControls { get; private set; } = new();

    #region Region Flags

    public RegionFlag Flags { get; set; }

    public bool GetFlag(RegionFlag flag) => (Flags & flag) != 0;

    public void SetFlag(RegionFlag flag, bool value)
    {
        if (value)
        {
            Flags |= flag;
        }
        else
        {
            Flags &= ~flag;
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowBenefitPlayer
    {
        get => GetFlag(RegionFlag.AllowBenefitPlayer);
        set => SetFlag(RegionFlag.AllowBenefitPlayer, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowHarmPlayer
    {
        get => GetFlag(RegionFlag.AllowHarmPlayer);
        set => SetFlag(RegionFlag.AllowHarmPlayer, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowHousing
    {
        get => GetFlag(RegionFlag.AllowHousing);
        set => SetFlag(RegionFlag.AllowHousing, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowSpawn
    {
        get => GetFlag(RegionFlag.AllowSpawn);
        set => SetFlag(RegionFlag.AllowSpawn, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanBeDamaged
    {
        get => GetFlag(RegionFlag.CanBeDamaged);
        set => SetFlag(RegionFlag.CanBeDamaged, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanMountEthereal
    {
        get => GetFlag(RegionFlag.CanMountEthereal);
        set => SetFlag(RegionFlag.CanMountEthereal, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanEnter
    {
        get => GetFlag(RegionFlag.CanEnter);
        set => SetFlag(RegionFlag.CanEnter, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanHeal
    {
        get => GetFlag(RegionFlag.CanHeal);
        set => SetFlag(RegionFlag.CanHeal, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanResurrect
    {
        get => GetFlag(RegionFlag.CanResurrect);
        set => SetFlag(RegionFlag.CanResurrect, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanUseStuckMenu
    {
        get => GetFlag(RegionFlag.CanUseStuckMenu);
        set => SetFlag(RegionFlag.CanUseStuckMenu, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool ItemDecay
    {
        get => GetFlag(RegionFlag.ItemDecay);
        set => SetFlag(RegionFlag.ItemDecay, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowBenefitNPC
    {
        get => GetFlag(RegionFlag.AllowBenefitNpc);
        set => SetFlag(RegionFlag.AllowBenefitNpc, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowHarmNPC
    {
        get => GetFlag(RegionFlag.AllowHarmNpc);
        set => SetFlag(RegionFlag.AllowHarmNpc, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool ShowEnterMessage
    {
        get => GetFlag(RegionFlag.ShowEnterMessage);
        set => SetFlag(RegionFlag.ShowEnterMessage, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool ShowExitMessage
    {
        get => GetFlag(RegionFlag.ShowExitMessage);
        set => SetFlag(RegionFlag.ShowExitMessage, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanLootPlayerCorpse
    {
        get => GetFlag(RegionFlag.CanLootPlayerCorpse);
        set => SetFlag(RegionFlag.CanLootPlayerCorpse, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanLootNPCCorpse
    {
        get => GetFlag(RegionFlag.CanLootNpcCorpse);
        set => SetFlag(RegionFlag.CanLootNpcCorpse, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanLootOwnCorpse
    {
        get => GetFlag(RegionFlag.CanLootOwnCorpse);
        set => SetFlag(RegionFlag.CanLootOwnCorpse, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CanUsePotions
    {
        get => GetFlag(RegionFlag.CanUsePotions);
        set => SetFlag(RegionFlag.CanUsePotions, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool IsGuarded
    {
        get => GetFlag(RegionFlag.IsGuarded);
        set
        {
            SetFlag(RegionFlag.IsGuarded, value);
            if (_region != null)
            {
                _region.Disabled = !value;
            }

            Timer.DelayCall(TimeSpan.FromSeconds(2.0), UpdateRegion);
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool EmptyNpcCorpse
    {
        get => GetFlag(RegionFlag.EmptyNpcCorpse);
        set => SetFlag(RegionFlag.EmptyNpcCorpse, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool EmptyPlayerCorpse
    {
        get => GetFlag(RegionFlag.EmptyPlayerCorpse);
        set => SetFlag(RegionFlag.EmptyPlayerCorpse, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool DeleteNpcCorpse
    {
        get => GetFlag(RegionFlag.DeleteNpcCorpse);
        set => SetFlag(RegionFlag.DeleteNpcCorpse, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool DeletePlayerCorpse
    {
        get => GetFlag(RegionFlag.DeletePlayerCorpse);
        set => SetFlag(RegionFlag.DeletePlayerCorpse, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool ResNpcOnDeath
    {
        get => GetFlag(RegionFlag.ResNpcOnDeath);
        set => SetFlag(RegionFlag.ResNpcOnDeath, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool ResPlayerOnDeath
    {
        get => GetFlag(RegionFlag.ResPlayerOnDeath);
        set => SetFlag(RegionFlag.ResPlayerOnDeath, value);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool MoveNpcOnDeath
    {
        get => GetFlag(RegionFlag.MoveNpcOnDeath);
        set
        {
            if (MoveNpcToMap == null || MoveNpcToMap == Map.Internal || MoveNpcToLoc == Point3D.Zero)
            {
                SetFlag(RegionFlag.MoveNpcOnDeath, false);
            }
            else
            {
                SetFlag(RegionFlag.MoveNpcOnDeath, value);
            }
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool MovePlayerOnDeath
    {
        get => GetFlag(RegionFlag.MovePlayerOnDeath);
        set
        {
            if (MovePlayerToMap == null || MovePlayerToMap == Map.Internal || MovePlayerToLoc == Point3D.Zero)
            {
                SetFlag(RegionFlag.MovePlayerOnDeath, false);
            }
            else
            {
                SetFlag(RegionFlag.MovePlayerOnDeath, value);
            }
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool NoPlayerItemDrop
    {
        get => GetFlag(RegionFlag.NoPlayerItemDrop);
        set => SetFlag(RegionFlag.NoPlayerItemDrop, value);
    }


    [CommandProperty(AccessLevel.GameMaster)]
    public bool NoNPCItemDrop
    {
        get => GetFlag(RegionFlag.NoNPCItemDrop);
        set => SetFlag(RegionFlag.NoNPCItemDrop, value);
    }

    # endregion


    #region Region Restrictions

    private BitArray _restrictedSpells;
    private BitArray _restrictedSkills;

    public BitArray RestrictedSpells => _restrictedSpells;

    public BitArray RestrictedSkills => _restrictedSkills;

    # endregion


    # region Region Related Objects

    private CustomRegion _region;
    private Rectangle3D[] _regionArea;

    public CustomRegion Region => _region;

    [CommandProperty(AccessLevel.GameMaster)]
    public Rectangle3D[] RegionArea
    {
        get => _regionArea;
        set => _regionArea = value;
    }

    # endregion


    # region Control Properties

    private bool _active = true;

    [CommandProperty(AccessLevel.GameMaster)]
    public bool Active
    {
        get => _active;
        set
        {
            if (_active != value)
            {
                _active = value;
                UpdateRegion();
            }
        }
    }

    # endregion


    # region Region Properties

    private string _regionName;
    private int _regionPriority;
    private MusicName _music;
    private TimeSpan _playerLogoutDelay;
    private int _lightLevel;

    private Map _moveNpcToMap;
    private Point3D _moveNpcToLoc;
    private Map _movePlayerToMap;
    private Point3D _movePlayerToLoc;

    [CommandProperty(AccessLevel.GameMaster)]
    public string RegionName
    {
        get => _regionName;
        set
        {
            if (Map != null && !RegionNameTaken(value))
            {
                _regionName = value;
            }
            else if (Map != null)
            {
                Console.WriteLine("RegionName not changed for {0}, {1} already has a Region with the name of {2}", this, Map, value);
            }
            else if (Map == null)
            {
                Console.WriteLine("RegionName not changed for {0} to {1}, it's Map value was null", this, value);
            }

            UpdateRegion();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int RegionPriority
    {
        get => _regionPriority;
        set
        {
            _regionPriority = value;
            UpdateRegion();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public MusicName Music
    {
        get => _music;
        set
        {
            _music = value;
            UpdateRegion();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public TimeSpan PlayerLogoutDelay
    {
        get => _playerLogoutDelay;
        set
        {
            _playerLogoutDelay = value;
            UpdateRegion();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int LightLevel
    {
        get => _lightLevel;
        set
        {
            _lightLevel = value;
            UpdateRegion();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Map MoveNpcToMap
    {
        get => _moveNpcToMap;
        set
        {
            if (value != Map.Internal)
            {
                _moveNpcToMap = value;
            }
            else
            {
                SetFlag(RegionFlag.MoveNpcOnDeath, false);
            }
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Point3D MoveNpcToLoc
    {
        get => _moveNpcToLoc;
        set
        {
            if (value != Point3D.Zero)
            {
                _moveNpcToLoc = value;
            }
            else
            {
                SetFlag(RegionFlag.MoveNpcOnDeath, false);
            }
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Map MovePlayerToMap
    {
        get => _movePlayerToMap;
        set
        {
            if (value != Map.Internal)
            {
                _movePlayerToMap = value;
            }
            else
            {
                SetFlag(RegionFlag.MovePlayerOnDeath, false);
            }
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Point3D MovePlayerToLoc
    {
        get => _movePlayerToLoc;
        set
        {
            if (value != Point3D.Zero)
            {
                _movePlayerToLoc = value;
            }
            else
            {
                SetFlag(RegionFlag.MovePlayerOnDeath, false);
            }
        }
    }

    # endregion

    [Constructible]
    public RegionControl() : base(5609)
    {
        Visible = false;
        Movable = false;
        Name = "Region Controller";

        AllControls ??= new List<RegionControl>();
        AllControls.Add(this);

        _regionName = FindNewName("Custom Region");
        _regionPriority = Server.Region.DefaultPriority;

        _restrictedSpells = new BitArray(SpellRegistry.Types.Length);
        _restrictedSkills = new BitArray(SkillInfo.Table.Length);
    }

    [Constructible]
    public RegionControl(Rectangle2D rect) : base(5609)
    {
        Visible = false;
        Movable = false;
        Name = "Region Controller";

        AllControls ??= new List<RegionControl>();
        AllControls.Add(this);

        _regionName = FindNewName("Custom Region");
        _regionPriority = Server.Region.DefaultPriority;

        _restrictedSpells = new BitArray(SpellRegistry.Types.Length);
        _restrictedSkills = new BitArray(SkillInfo.Table.Length);

        Rectangle3D newRect = Server.Region.ConvertTo3D(rect);
        DoChooseArea(null, Map, newRect.Start, newRect.End);

        UpdateRegion();
    }

    [Constructible]
    public RegionControl(Rectangle3D rect) : base(5609)
    {
        Visible = false;
        Movable = false;
        Name = "Region Controller";

        AllControls ??= new List<RegionControl>();
        AllControls.Add(this);

        _regionName = FindNewName("Custom Region");
        _regionPriority = Server.Region.DefaultPriority;

        _restrictedSpells = new BitArray(SpellRegistry.Types.Length);
        _restrictedSkills = new BitArray(SkillInfo.Table.Length);

        DoChooseArea(null, Map, rect.Start, rect.End);

        UpdateRegion();
    }

    [Constructible]
    public RegionControl(Rectangle2D[] rects) : base(5609)
    {
        Visible = false;
        Movable = false;
        Name = "Region Controller";

        AllControls ??= new List<RegionControl>();
        AllControls.Add(this);

        _regionName = FindNewName("Custom Region");
        _regionPriority = Server.Region.DefaultPriority;

        _restrictedSpells = new BitArray(SpellRegistry.Types.Length);
        _restrictedSkills = new BitArray(SkillInfo.Table.Length);

        foreach (Rectangle2D rect2d in rects)
        {
            Rectangle3D newRect = Server.Region.ConvertTo3D(rect2d);
            DoChooseArea(null, Map, newRect.Start, newRect.End);
        }

        UpdateRegion();
    }

    [Constructible]
    public RegionControl(Rectangle3D[] rects) : base(5609)
    {
        Visible = false;
        Movable = false;
        Name = "Region Controller";

        AllControls ??= new List<RegionControl>();
        AllControls.Add(this);

        _regionName = FindNewName("Custom Region");
        _regionPriority = Server.Region.DefaultPriority;

        _restrictedSpells = new BitArray(SpellRegistry.Types.Length);
        _restrictedSkills = new BitArray(SkillInfo.Table.Length);

        foreach (Rectangle3D rect3d in rects)
        {
            DoChooseArea(null, Map, rect3d.Start, rect3d.End);
        }

        UpdateRegion();
    }

    public RegionControl(Serial serial) : base(serial)
    {
    }


    #region Control Special Voids

    public bool RegionNameTaken(string testName)
    {
        if (AllControls != null)
        {
            foreach (RegionControl control in AllControls)
            {
                if (control.RegionName == testName && control != this)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public string FindNewName(string oldName)
    {
        int i = 1;

        string newName = oldName;
        while (RegionNameTaken(newName))
        {
            newName = oldName;
            newName += $" {i}";
            i++;
        }

        return newName;
    }

    public void UpdateRegion()
    {
        _region?.Unregister();

        if (Map != null && Active)
        {
            if (RegionArea != null && RegionArea.Length > 0)
            {
                _region = new CustomRegion(this);
                _region.Register();
            }
            else
            {
                _region = null;
            }
        }
        else
        {
            _region = null;
        }
    }

    public void RemoveArea(int index, Mobile from)
    {
        try
        {
            List<Rectangle3D> rects = new List<Rectangle3D>();

            foreach (Rectangle3D rect in _regionArea)
            {
                rects.Add(rect);
            }

            rects.RemoveAt(index);
            _regionArea = rects.ToArray();

            UpdateRegion();
            from.SendMessage("Area Removed!");
        }
        catch
        {
            from.SendMessage("Removing of Area Failed!");
        }
    }

    public static int GetRegistryNumber(ISpell s)
    {
        Type[] t = SpellRegistry.Types;

        for (int i = 0; i < t.Length; i++)
        {
            if (s.GetType() == t[i])
            {
                return i;
            }
        }

        return -1;
    }


    public bool IsRestrictedSpell(ISpell s)
    {
        if (_restrictedSpells.Length != SpellRegistry.Types.Length)
        {
            _restrictedSpells = new BitArray(SpellRegistry.Types.Length);

            for (int i = 0; i < _restrictedSpells.Length; i++)
            {
                _restrictedSpells[i] = false;
            }
        }

        int regNum = GetRegistryNumber(s);


        if (regNum < 0) //Happens with unregistered Spells
        {
            return false;
        }

        return _restrictedSpells[regNum];
    }

    public bool IsRestrictedSkill(int skill)
    {
        if (_restrictedSkills.Length != SkillInfo.Table.Length)
        {
            _restrictedSkills = new BitArray(SkillInfo.Table.Length);

            for (int i = 0; i < _restrictedSkills.Length; i++)
            {
                _restrictedSkills[i] = false;
            }
        }

        if (skill < 0)
        {
            return false;
        }

        return _restrictedSkills[skill];
    }

    public void ChooseArea(Mobile m)
    {
        BoundingBoxPicker.Begin(m, (map, start, end) => CustomRegion_Callback(m, map, start, end));
    }

    public void CustomRegion_Callback(Mobile from, Map map, Point3D start, Point3D end)
    {
        DoChooseArea(from, map, start, end);
    }

    public void DoChooseArea(Mobile from, Map map, Point3D start, Point3D end)
    {
        List<Rectangle3D> areas = new List<Rectangle3D>();

        if (_regionArea != null)
        {
            foreach (Rectangle3D rect in _regionArea)
            {
                areas.Add(rect);
            }
        }

        if (start.Z == end.Z || start.Z < end.Z)
        {
            if (start.Z != Server.Region.MinZ)
            {
                start.Z = Server.Region.MinZ;
            }

            if (end.Z != Server.Region.MaxZ)
            {
                end.Z = Server.Region.MaxZ;
            }
        }
        else
        {
            if (start.Z != Server.Region.MaxZ)
            {
                start.Z = Server.Region.MaxZ;
            }

            if (end.Z != Server.Region.MinZ)
            {
                end.Z = Server.Region.MinZ;
            }
        }

        Rectangle3D newRect = new Rectangle3D(start, end);
        areas.Add(newRect);

        _regionArea = areas.ToArray();

        UpdateRegion();
        from.CloseGump<RegionControlGump>();
        from.SendGump(new RegionControlGump(this));
        from.CloseGump<RemoveAreaGump>();
        from.SendGump(new RemoveAreaGump(this));
    }

    # endregion


    #region Control Overrides

    public override void OnDoubleClick(Mobile m)
    {
        if (m.AccessLevel >= AccessLevel.GameMaster)
        {
            if (_restrictedSpells.Length != SpellRegistry.Types.Length)
            {
                _restrictedSpells = new BitArray(SpellRegistry.Types.Length);

                for (int i = 0; i < _restrictedSpells.Length; i++)
                {
                    _restrictedSpells[i] = false;
                }

                m.SendMessage("Resetting all restricted Spells due to Spell change");
            }

            if (_restrictedSkills.Length != SkillInfo.Table.Length)
            {
                _restrictedSkills = new BitArray(SkillInfo.Table.Length);

                for (int i = 0; i < _restrictedSkills.Length; i++)
                {
                    _restrictedSkills[i] = false;
                }

                m.SendMessage("Resetting all restricted Skills due to Skill change");
            }

            m.CloseGump<RegionControlGump>();
            m.SendGump(new RegionControlGump(this));
            m.SendMessage("Don't forget to props this object for more options!");
            m.CloseGump<RemoveAreaGump>();
            m.SendGump(new RemoveAreaGump(this));
        }
    }

    public override void OnMapChange()
    {
        UpdateRegion();
        base.OnMapChange();
    }

    public override void OnDelete()
    {
        _region?.Unregister();

        AllControls?.Remove(this);

        base.OnDelete();
    }

    # endregion


    #region Ser/Deser Helpers

    public static void WriteBitArray(IGenericWriter writer, BitArray ba)
    {
        writer.Write(ba.Length);

        for (int i = 0; i < ba.Length; i++)
        {
            writer.Write(ba[i]);
        }
    }

    public static BitArray ReadBitArray(IGenericReader reader)
    {
        int size = reader.ReadInt();

        BitArray newBA = new BitArray(size);

        for (int i = 0; i < size; i++)
        {
            newBA[i] = reader.ReadBool();
        }

        return newBA;
    }


    public static void WriteRect3DArray(IGenericWriter writer, Rectangle3D[] ary)
    {
        if (ary == null)
        {
            writer.Write(0);
            return;
        }

        writer.Write(ary.Length);

        for (int i = 0; i < ary.Length; i++)
        {
            Rectangle3D rect = ((Rectangle3D)ary[i]);
            writer.Write((Point3D)rect.Start);
            writer.Write((Point3D)rect.End);
        }
    }

    public static List<Rectangle2D> ReadRect2DArray(IGenericReader reader)
    {
        int size = reader.ReadInt();
        List<Rectangle2D> newAry = new List<Rectangle2D>();

        for (int i = 0; i < size; i++)
        {
            newAry.Add(reader.ReadRect2D());
        }

        return newAry;
    }

    public static Rectangle3D[] ReadRect3DArray(IGenericReader reader)
    {
        int size = reader.ReadInt();
        List<Rectangle3D> newAry = new List<Rectangle3D>();

        for (int i = 0; i < size; i++)
        {
            Point3D start = reader.ReadPoint3D();
            Point3D end = reader.ReadPoint3D();
            newAry.Add(new Rectangle3D(start, end));
        }

        return newAry.ToArray();
    }

    # endregion


    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)0); // version

        WriteRect3DArray(writer, _regionArea);

        writer.Write((int)Flags);

        WriteBitArray(writer, _restrictedSpells);
        WriteBitArray(writer, _restrictedSkills);

        writer.Write((bool)_active);

        writer.Write((string)_regionName);
        writer.Write((int)_regionPriority);
        writer.Write((int)_music);
        writer.Write((TimeSpan)_playerLogoutDelay);
        writer.Write((int)_lightLevel);

        writer.Write((Map)_moveNpcToMap);
        writer.Write((Point3D)_moveNpcToLoc);
        writer.Write((Map)_movePlayerToMap);
        writer.Write((Point3D)_movePlayerToLoc);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();

        _regionArea = ReadRect3DArray(reader);

        Flags = (RegionFlag)reader.ReadInt();

        _restrictedSpells = ReadBitArray(reader);
        _restrictedSkills = ReadBitArray(reader);

        _active = reader.ReadBool();

        _regionName = reader.ReadString();
        _regionPriority = reader.ReadInt();
        _music = (MusicName)reader.ReadInt();
        _playerLogoutDelay = reader.ReadTimeSpan();
        _lightLevel = reader.ReadInt();

        _moveNpcToMap = reader.ReadMap();
        _moveNpcToLoc = reader.ReadPoint3D();
        _movePlayerToMap = reader.ReadMap();
        _movePlayerToLoc = reader.ReadPoint3D();

        AllControls.Add(this);

        if (RegionNameTaken(_regionName))
        {
            _regionName = FindNewName(_regionName);
        }

        UpdateRegion();
    }
}
