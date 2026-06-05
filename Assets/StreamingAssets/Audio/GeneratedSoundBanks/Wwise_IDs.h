/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_BGM = 3126765036U;
        static const AkUniqueID PLAY_BUTTONPRESS = 2652178615U;
        static const AkUniqueID PLAY_CLICKUI = 4257816096U;
        static const AkUniqueID PLAY_DIE = 3092915528U;
        static const AkUniqueID PLAY_ENDLEVEL = 4083890705U;
        static const AkUniqueID PLAY_JUMPING = 2756694246U;
        static const AkUniqueID PLAY_LANDING = 2323405115U;
        static const AkUniqueID PLAY_REWIND = 869704911U;
        static const AkUniqueID PLAY_ROLLING = 1834004125U;
        static const AkUniqueID PLAY_SPAWN = 1012143543U;
        static const AkUniqueID STOP_BGM = 1073466678U;
        static const AkUniqueID STOP_REWIND = 564679353U;
        static const AkUniqueID STOP_ROLLING = 2567045767U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace DEFAULT
        {
            static const AkUniqueID GROUP = 782826392U;

            namespace STATE
            {
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace DEFAULT

    } // namespace STATES

    namespace SWITCHES
    {
        namespace BGM
        {
            static const AkUniqueID GROUP = 412724365U;

            namespace SWITCH
            {
                static const AkUniqueID CHAPTER1 = 1183684777U;
                static const AkUniqueID CHAPTER2 = 1183684778U;
                static const AkUniqueID CHAPTER3 = 1183684779U;
                static const AkUniqueID CHAPTER4 = 1183684780U;
                static const AkUniqueID CHAPTER5 = 1183684781U;
                static const AkUniqueID MAINMENU = 3604647259U;
            } // namespace SWITCH
        } // namespace BGM

    } // namespace SWITCHES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID SPEED = 640949982U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID DEFAULT = 782826392U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID EFFECTS = 1942696649U;
        static const AkUniqueID MASTER_AUDIO_BUS = 3803692087U;
        static const AkUniqueID MUSIC = 3991942870U;
    } // namespace BUSSES

    namespace AUX_BUSSES
    {
        static const AkUniqueID REVERB = 348963605U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
