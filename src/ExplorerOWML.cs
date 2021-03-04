#if OWML
using OWML;
using OWML.Common;
using OWML.ModHelper;
using Harmony;
using OWML.Utils;

namespace UnityExplorer
{
    public class ExplorerOWML : ModBehaviour
    {
        public static HarmonyInstance HarmonyInstance;
        public static IModHelper Helper;


        public override void Configure(IModConfig config)
        {
            Helper = ModHelper;
        }

        internal void Start()
        {
            Helper.Console.WriteLine("Start NomaiExplorer");

            //Using the Mod Loader the Mod Loader? Why not?
            HarmonyInstance = Helper.HarmonyHelper.GetValue<HarmonyInstance>("_harmony");


            Helper.Console.WriteLine("Initializing explorer");
            new ExplorerCore();
            ExplorerCore.OnLogError += (message) => Helper.Console.WriteLine(message, MessageType.Error);
            ExplorerCore.OnLogMessage += (message) => Helper.Console.WriteLine(message, MessageType.Info);
            ExplorerCore.OnLogWarning += (message) => Helper.Console.WriteLine(message, MessageType.Warning);
        }

        internal void Update()
        {
            ExplorerCore.Update();
        }
	}
}
#endif