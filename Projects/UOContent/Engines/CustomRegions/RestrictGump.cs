using System;
using System.Collections;
using Server.Gumps;
using Server.Network;
using Server.Spells;

namespace Server.Engines.CustomRegions;

public abstract class RestrictGump : Gump
{
    public enum RestrictType
    {
        Spells,
        Skills
    }

    private readonly BitArray _restricted;

    private readonly RestrictType _type;

    public RestrictGump(BitArray ba, RestrictType t) : base(50, 50)
    {
        _restricted = ba;
        _type = t;

        Closable = true;
        Draggable = true;
        Resizable = false;

        AddPage(0);

        AddBackground(10, 10, 225, 425, 9380);
        AddLabel(73, 15, 1152, t == RestrictType.Spells ? "Restrict Spells" : "Restrict Skills");
        AddButton(91, 411, 247, 248, 1);

        int itemsThisPage = 0;
        int nextPageNumber = 1;

        object[] array; // = (t == RestrictType.Skills) ? SkillInfo.Table : SpellRegistry.Types;

        if (t == RestrictType.Skills)
        {
            array = SkillInfo.Table;
        }
        else
        {
            array = SpellRegistry.Types;
        }

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                if (itemsThisPage is >= 8 or 0)
                {
                    itemsThisPage = 0;

                    if (nextPageNumber != 1)
                    {
                        AddButton(190, 412, 4005, 4007, 2, GumpButtonType.Page, nextPageNumber);
                        //Forward button -> #2
                    }

                    AddPage(nextPageNumber++);

                    if (nextPageNumber != 2)
                    {
                        AddButton(29, 412, 4014, 4016, 3, GumpButtonType.Page, nextPageNumber - 2);
                        //Back Button -> #3
                    }
                }

                AddCheck(40, 55 + (45 * itemsThisPage), 210, 211, ba[i], i + (t == RestrictType.Spells ? 100 : 500));
                AddLabel(
                    70,
                    55 + (45 * itemsThisPage),
                    0,
                    t == RestrictType.Spells ? ((Type)array[i]).Name : ((SkillInfo)array[i]).Name
                );

                itemsThisPage++;
            }
        }
    }

    public override void OnResponse(NetState sender, RelayInfo info)
    {
        if (info.ButtonID == 1)
        {
            for (int i = 0; i < _restricted.Length; i++)
            {
                _restricted[i] = info.IsSwitched(i + (_type == RestrictType.Spells ? 100 : 500));
                //This way is faster after looking at decompiled BitArray.SetAll( bool )
            }
        }
    }
}

public class SpellRestrictGump : RestrictGump
{
    public SpellRestrictGump(BitArray ba) : base(ba, RestrictType.Spells)
    {
    }
}

public class SkillRestrictGump : RestrictGump
{
    public SkillRestrictGump(BitArray ba) : base(ba, RestrictType.Skills)
    {
    }
}
