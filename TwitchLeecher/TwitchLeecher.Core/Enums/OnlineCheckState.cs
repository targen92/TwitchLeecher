namespace TwitchLeecher.Core.Enums
{
    public enum OnlineCheckState
    {
        CheckNoState = 0x00,
        CheckOff = 0x01,
        CheckOn = 0x02,
        CheckOnOff = 0x0F,
        CheckNow = 0x10,
        CheckEnd = 0x20,
        Download = 0x30,
        Wait = 0x40,
        CheckCurState = 0xF0,
    }
}