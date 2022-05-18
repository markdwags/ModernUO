using System;
using System.Collections.Generic;
using ModernUO.Serialization;
using Server.Collections;
using Server.Spells;

namespace Server.Engines.CustomRegions;

/*[Flags]
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
}*/

[SerializationGenerator(0, false)]
public partial class RegionControl : Item
{
    public static List<RegionControl> AllControls { get; private set; } = new();

    #region Region Toggles

    [SerializableField(0)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _allowBenefitPlayer;

    [SerializableField(1)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _allowHarmPlayer;

    [SerializableField(2)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _allowHousing;

    [SerializableField(3)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _allowSpawn;

    [SerializableField(4)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canBeDamaged;

    [SerializableField(5)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canMountEthereal;

    [SerializableField(6)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canEnter;

    [SerializableField(7)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canHeal;

    [SerializableField(8)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canResurrect;

    [SerializableField(9)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canUseStuckMenu;

    [SerializableField(10)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _itemDecay;

    [SerializableField(11)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _allowBenefitNPC;

    [SerializableField(12)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _allowHarmNPC;

    [SerializableField(13)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _showEnterMessage;

    [SerializableField(14)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _showExitMessage;

    [SerializableField(15)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canLootPlayerCorpse;

    [SerializableField(16)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canLootNPCCorpse;

    [SerializableField(17)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canLootOwnCorpse;

    [SerializableField(18)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _canUsePotions;

    private bool _isGuarded;

    [SerializableField(19)]
    [CommandProperty(AccessLevel.GameMaster)]
    public bool IsGuarded
    {
        get => _isGuarded;
        set
        {
            _isGuarded = value;

            if (_region != null)
            {
                _region.Disabled = !value;
            }

            Timer.DelayCall(TimeSpan.FromSeconds(2.0), UpdateRegion);

            this.MarkDirty();
        }
    }

    [SerializableField(20)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _emptyNpcCorpse;

    [SerializableField(21)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _emptyPlayerCorpse;

    [SerializableField(22)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _deleteNpcCorpse;

    [SerializableField(23)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _deletePlayerCorpse;

    [SerializableField(24)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _resNpcOnDeath;

    [SerializableField(25)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _resPlayerOnDeath;

    public bool _moveNpcOnDeath;

    [SerializableField(26)]
    [CommandProperty(AccessLevel.GameMaster)]
    public bool MoveNpcOnDeath
    {
        get => _moveNpcOnDeath;
        set
        {
            if (MoveNpcToMap == null || MoveNpcToMap == Map.Internal || MoveNpcToLoc == Point3D.Zero)
            {
                _moveNpcOnDeath = false;
            }
            else
            {
                _moveNpcOnDeath = value;
            }

            this.MarkDirty();
        }
    }

    private bool _movePlayerOnDeath;

    [SerializableField(27)]
    [CommandProperty(AccessLevel.GameMaster)]
    public bool MovePlayerOnDeath
    {
        get => _movePlayerOnDeath;
        set
        {
            if (MovePlayerToMap == null || MovePlayerToMap == Map.Internal || MovePlayerToLoc == Point3D.Zero)
            {
                _movePlayerOnDeath = false;
            }
            else
            {
                _movePlayerOnDeath = value;
            }

            this.MarkDirty();
        }
    }

    [SerializableField(28)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _noPlayerItemDrop;


    [SerializableField(29)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private bool _noNpcItemDrop;

    [SerializableField(30)]
    [CommandProperty(AccessLevel.GameMaster)]
    public bool LoginRelocation
    {
        get => _loginRelocation;
        set
        {
            if (LoginRelocationMap == null || LoginRelocationMap == Map.Internal || LoginRelocationLoc == Point3D.Zero)
            {
                _loginRelocation = false;
            }
            else
            {
                _loginRelocation = value;
            }

            this.MarkDirty();
        }
    }

    [SerializableField(31)]
    [CommandProperty(AccessLevel.GameMaster)]
    public Point3D LoginRelocationLoc
    {
        get => _loginRelocationLoc;
        set
        {
            if (value != Point3D.Zero)
            {
                _loginRelocationLoc = value;
            }
            else
            {
                _loginRelocation = false;
            }

            this.MarkDirty();
        }
    }

    [SerializableField(32)]
    [CommandProperty(AccessLevel.GameMaster)]
    public Map LoginRelocationMap
    {
        get => _loginRelocationMap;
        set
        {
            if (value != Map.Internal)
            {
                _loginRelocationMap = value;
            }
            else
            {
                _loginRelocation = false;
            }

            this.MarkDirty();
        }
    }

    # endregion


    #region Region Restrictions

    [SerializableField(33)]
    private BitArray _restrictedSpells;

    [SerializableField(34)]
    private BitArray _restrictedSkills;

    # endregion


    # region Region Related Objects

    private CustomRegion _region;

    public CustomRegion Region => _region;

    [SerializableField(35)]
    [SerializableFieldAttr("[CommandProperty(AccessLevel.GameMaster)]")]
    private Rectangle3D[] _regionArea;

    # endregion


    # region Control Properties

    private bool _active = true;

    [SerializableField(36)]
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

            this.MarkDirty();
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

    private bool _loginRelocation;
    private Point3D _loginRelocationLoc;
    private Map _loginRelocationMap;

    [SerializableField(37)]
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

            this.MarkDirty();
        }
    }

    [SerializableField(38)]
    [CommandProperty(AccessLevel.GameMaster)]
    public int RegionPriority
    {
        get => _regionPriority;
        set
        {
            _regionPriority = value;
            UpdateRegion();

            this.MarkDirty();
        }
    }

    [SerializableField(39)]
    [CommandProperty(AccessLevel.GameMaster)]
    public MusicName Music
    {
        get => _music;
        set
        {
            _music = value;
            UpdateRegion();

            this.MarkDirty();
        }
    }

    [SerializableField(40)]
    [CommandProperty(AccessLevel.GameMaster)]
    public TimeSpan PlayerLogoutDelay
    {
        get => _playerLogoutDelay;
        set
        {
            _playerLogoutDelay = value;
            UpdateRegion();

            this.MarkDirty();
        }
    }

    [SerializableField(41)]
    [CommandProperty(AccessLevel.GameMaster)]
    public int LightLevel
    {
        get => _lightLevel;
        set
        {
            _lightLevel = value;
            UpdateRegion();

            this.MarkDirty();
        }
    }

    [SerializableField(42)]
    [CommandProperty(AccessLevel.GameMaster)]
    public Map MoveNpcToMap
    {
        get => _moveNpcToMap;
        set
        {
            _moveNpcToMap = value != Map.Internal ? value : null;

            this.MarkDirty();
        }
    }

    [SerializableField(43)]
    [CommandProperty(AccessLevel.GameMaster)]
    public Point3D MoveNpcToLoc
    {
        get => _moveNpcToLoc;
        set
        {
            _moveNpcToLoc = value != Point3D.Zero ? value : Point3D.Zero;

            this.MarkDirty();
        }
    }

    [SerializableField(44)]
    [CommandProperty(AccessLevel.GameMaster)]
    public Map MovePlayerToMap
    {
        get => _movePlayerToMap;
        set
        {
            _movePlayerToMap = value != Map.Internal ? value : null;

            this.MarkDirty();
        }
    }

    [SerializableField(45)]
    [CommandProperty(AccessLevel.GameMaster)]
    public Point3D MovePlayerToLoc
    {
        get => _movePlayerToLoc;
        set
        {
            _movePlayerToLoc = value != Point3D.Zero ? value : Point3D.Zero;

            this.MarkDirty();
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
            if (_regionArea is { Length: > 0 })
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

    /*public static void WriteBitArray(IGenericWriter writer, BitArray ba)
    {
        writer.Write(ba.Length);

        for (int i = 0; i < ba.Length; i++)
        {
            writer.Write(ba[i]);
        }
    }*/

    /*public static BitArray ReadBitArray(IGenericReader reader)
    {
        int size = reader.ReadInt();

        BitArray newBA = new BitArray(size);

        for (int i = 0; i < size; i++)
        {
            newBA[i] = reader.ReadBool();
        }

        return newBA;
    }*/


    /*public static void WriteRect3DArray(IGenericWriter writer, Rectangle3D[] ary)
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
    }*/

    # endregion

    [AfterDeserialization]
    private void AfterDeserialization()
    {
        AllControls.Add(this);

        if (RegionNameTaken(_regionName))
        {
            _regionName = FindNewName(_regionName);
        }

        UpdateRegion();
    }
}
