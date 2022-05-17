using Server.Gumps;
using Server.Network;

namespace Server.Engines.CustomRegions;

public class RemoveAreaGump : Gump
{
    private readonly RegionControl _control;

    public RemoveAreaGump(RegionControl regionControl) : base(25, 250)
    {
        _control = regionControl;

        Closable = true;
        Draggable = true;
        Resizable = false;

        AddPage(0);
        AddBackground(23, 32, 412, 256, 9270);
        AddAlphaRegion(19, 29, 418, 263);

        AddLabel(186, 45, 1152, "REMOVE AREA");

        int itemsThisPage = 0;
        int nextPageNumber = 1;

        if (regionControl.RegionArea != null)
        {
            for (int i = 0; i < regionControl.RegionArea.Length; i++)
            {
                Rectangle3D rect = regionControl.RegionArea[i];

                if (itemsThisPage is >= 8 or 0)
                {
                    itemsThisPage = 0;

                    if (nextPageNumber != 1)
                    {
                        AddButton(393, 45, 4007, 4009, 0, GumpButtonType.Page, nextPageNumber);
                    }

                    AddPage(nextPageNumber++);

                    if (nextPageNumber != 2)
                    {
                        AddButton(35, 45, 4014, 4016, 1, GumpButtonType.Page, nextPageNumber - 2);
                    }
                }

                AddButton(70, 75 + 25 * itemsThisPage, 4017, 4019, 100 + i, GumpButtonType.Reply, 0);

                AddLabel(
                    116,
                    77 + 25 * itemsThisPage,
                    1152,
                    $"({rect.Start.X}, {rect.Start.Y}, {rect.Start.Z})"
                );


                AddLabel(232, 78 + 25 * itemsThisPage, 1152, "<-->");

                //AddLabel(294, 77 + 25*i, 0, "(9876, 5432)");
                AddLabel(
                    294,
                    77 + 25 * itemsThisPage,
                    1152,
                    $"({rect.End.X}, {rect.End.Y}, {rect.End.Z})"
                );

                itemsThisPage++;
            }
        }
    }

    public override void OnResponse(NetState sender, RelayInfo info)
    {
        Mobile from = sender.Mobile;

        if (from == null)
        {
            return;
        }

        if (info.ButtonID >= 100)
        {
            _control.RemoveArea(info.ButtonID - 100, from);

            sender.Mobile.CloseGump<RemoveAreaGump>();
            sender.Mobile.SendGump(new RemoveAreaGump(_control));
        }
    }
}
