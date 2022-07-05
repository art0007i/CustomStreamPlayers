using HarmonyLib;
using NeosModLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrooxEngine;
using BaseX;
using CodeX;

namespace CustomStreamPlayers
{
    public class CustomStreamPlayers : NeosMod
    {
        public override string Name => "CustomStreamPlayers";
        public override string Author => "art0007i";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/art0007i/CustomStreamPlayers/";

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<bool> KEY_DELETE_OLD = new("delete_old", "If true old stream players will be deleted whenever you spawn a new one.", () => true);
        private static ModConfiguration config;
        public override void OnEngineInit()
        {
            config = GetConfiguration();
            Harmony harmony = new Harmony("me.art0007i.CustomStreamPlayers");
            harmony.PatchAll();
            // Register special item using special items lib and store a reference to it
            OurItem = SpecialItemsLib.SpecialItemsLib.RegisterItem(STREAM_PLAYER_TAG);
        }

        private static string STREAM_PLAYER_TAG { get { return "custom_stream_player"; } }

        private static SpecialItemsLib.CustomSpecialItem OurItem;

        // Patch saving items so that newly saved items have the special tag
        // This is done outside the library in case you have a more complex way of determining if an item should be special than just checking if it contains a certain component
        [HarmonyPatch(typeof(SlotHelper), "GenerateTags", new Type[] { typeof(Slot), typeof(HashSet<string>) })]
        class SlotHelper_GenerateTags_Patch
        {
            static void Postfix(Slot slot, HashSet<string> tags)
            {
                if (slot.GetComponent<AudioStreamController>() != null)
                {
                    tags.Add(STREAM_PLAYER_TAG);
                }
            }
        }

        // Patch spawning the special item
        [HarmonyPatch(typeof(AudioStreamSpawner), "OnStartStreaming")]
        class AudioStreamSpawner_OnStartStreaming_Patch
        {
            public static bool Prefix(AudioStreamSpawner __instance, IButton button)
            {
                if (OurItem.Uri == null) return true;
                if (__instance.World != Userspace.UserspaceWorld)
                {
                    return true;
                }
                int deviceIndex = __instance.InputInterface.FindAudioInputIndex(__instance.DeviceName.Value, false, false);
                World _targetWorld = __instance.Engine.WorldManager.FocusedWorld;
                _targetWorld.RunSynchronously(async ()=>
                {
                    if (!_targetWorld.CanSpawnObjects())
                    {
                        NotificationMessage.SpawnTextMessage("Permissions.NotAllowedToSpawn", color.Red, 0.5f, 3f, 0.35f, 0.5f, 0.15f, 0.2f);
                        return;
                    }
                    if (config.GetValue(KEY_DELETE_OLD))
                    {
                        _targetWorld.GetGloballyRegisteredComponents((AudioStreamController c) => c.IsOwnedByLocalUser).ForEach(delegate (AudioStreamController c)
                        {
                            c.Slot.GetObjectRoot().Destroy();
                        });
                    }
                    Slot slot = _targetWorld.RootSlot.LocalUserSpace.AddSlot("Audio Stream", true);

                    // code that loads items from inventory, it's magic
                    CoroutineManager.Manager.Value = _targetWorld.Coroutines;
                    await (default(ToBackground));
                    await slot.LoadObjectAsync(OurItem.Uri);
                    await (default(ToWorld));
                    InventoryItem component = slot.GetComponent<InventoryItem>();
                    if(component != null)
                    {
                        slot = component.Unpack();
                    }
                    slot.PersistentSelf = false;
                    var streamComponent = slot.GetComponentInChildren<AudioStreamController>();

                    UserAudioStream<StereoSample> userAudioStream = streamComponent.Slot.AttachComponent<UserAudioStream<StereoSample>>(true, null);
                    OpusStream<StereoSample> opusStream = _targetWorld.LocalUser.AddStream<OpusStream<StereoSample>>();

                    opusStream.BitRate.Value = MathX.RoundToInt(__instance.BitrateKbps * 1000f);
                    opusStream.ApplicationType.Value = POpusCodec.Enums.OpusApplicationType.Audio;
                    opusStream.MinimumVolume.Value = 0f;
                    opusStream.MinimumBufferDelay.Value = 0.2f;
                    opusStream.BufferSize.Value = 24000;
                    
                    userAudioStream.Persistent = false;
                    userAudioStream.TargetDeviceIndex = new int?(deviceIndex);
                    userAudioStream.Stream.Target = opusStream;
                    userAudioStream.UseFilteredData.Value = false;
                    (AccessTools.Field(typeof(AudioStreamController), "_stream").GetValue(streamComponent) as SyncRef<OpusStream<StereoSample>>).Target = opusStream;
                    var output = (AccessTools.Field(typeof(AudioStreamController), "_audioOutput").GetValue(streamComponent) as SyncRef<AudioOutput>);
                    if(output.Target != null)
                        output.Target.Source.Target = opusStream;

                    slot.PositionInFrontOfUser(float3.Backward);

                });
                button.Slot.CloseModalOverlay();
                return false;
            }
        }
    }
}