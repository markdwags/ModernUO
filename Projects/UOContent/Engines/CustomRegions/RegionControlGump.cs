using Server.Gumps;
using Server.Network;

namespace Server.Engines.CustomRegions;

public class RegionControlGump : Gump
{
    private readonly RegionControl _controller;

    public RegionControlGump(RegionControl r) : base(25, 50)
    {
        _controller = r;

        Closable = true;
        Draggable = true;
        Resizable = false;

        AddPage(0);
        //x, y, width, high
        AddBackground(23, 32, 412, 186, 9270);
        AddAlphaRegion(19, 29, 418, 193);

        AddLabel(55, 60, 1152, _controller.RegionName);

        AddLabel(75, 90, 1152, "Add Region Area");
        AddButton(55, 92, 0x845, 0x846, 3, GumpButtonType.Reply, 0);

        AddLabel(75, 110, 1152, "Edit Restricted Spells");
        AddButton(55, 112, 0x845, 0x846, 1, GumpButtonType.Reply, 0);

        AddLabel(75, 130, 1152, "Edit Restricted Skills");
        AddButton(55, 132, 0x845, 0x846, 2, GumpButtonType.Reply, 0);

        AddLabel(75, 150, 1152, "Edit Other Properties");
        AddButton(55, 152, 0x845, 0x846, 4, GumpButtonType.Reply, 0);

        AddLabel(75, 170, 1152, "See Region Bounds");
        AddButton(55, 172, 0x845, 0x846, 5, GumpButtonType.Reply, 0);

        AddImage(353, 54, 3953);
        AddImage(353, 180, 3955);
    }

    public override void OnResponse(NetState sender, RelayInfo info)
    {
        if (_controller?.Deleted != false)
        {
            return;
        }

        Mobile m = sender.Mobile;
        string prefix = CommandSystem.Prefix;

        switch (info.ButtonID)
        {
            case 1:
                {
                    m.CloseGump<SpellRestrictGump>();
                    m.SendGump(new SpellRestrictGump(_controller.RestrictedSpells));

                    m.CloseGump<RegionControlGump>();
                    m.SendGump(new RegionControlGump(_controller));
                    break;
                }
            case 2:
                {
                    m.CloseGump<SkillRestrictGump>();
                    m.SendGump(new SkillRestrictGump(_controller.RestrictedSkills));

                    m.CloseGump<RegionControlGump>();
                    m.SendGump(new RegionControlGump(_controller));
                    break;
                }
            case 3:
                {
                    m.CloseGump<RegionControlGump>();
                    m.SendGump(new RegionControlGump(_controller));

                    m.CloseGump<RemoveAreaGump>();
                    m.SendGump(new RemoveAreaGump(_controller));

                    _controller.ChooseArea(m);
                    break;
                }
            case 4:
                {
                    m.SendGump(new PropertiesGump(m, _controller));
                    m.CloseGump<RegionControlGump>();
                    m.SendGump(new RegionControlGump(_controller));
                    m.CloseGump<RemoveAreaGump>();
                    m.SendGump(new RemoveAreaGump(_controller));
                    break;
                }
            case 5:
                {
                    CommandSystem.Handle(m, $"{prefix}RegionBounds");

                    m.CloseGump<RegionControlGump>();
                    m.SendGump(new RegionControlGump(_controller));

                    m.CloseGump<RemoveAreaGump>();
                    m.SendGump(new RemoveAreaGump(_controller));
                    break;
                }
        }
    }
}
