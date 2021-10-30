using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalGameLauncher {
    class Constants : Application {

        /// <summary>
        /// Core game info
        /// </summary>
        public static readonly string GAME_TITLE = "UO VnV";
        public static readonly string LAUNCHER_NAME = "UOVnV";

        /// <summary>
        /// Paths & urls
        /// </summary>
        public static string DESTINATION_PATH { get { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\",""); } }
        public static string GAME_EXECUTABLE_PATH 
        {
            get
            {
                return Path.Combine(Properties.Settings.Default.ClientLocation);
            }
        }
        public static string GAME_CSV_PATH 
        { 
            get 
            { 
               return Path.Combine(Properties.Settings.Default.DestinationPath, "UOVnV.csv"); 
            } 
        }
        public static readonly string DOWNLOAD_CSV_PATH = "https://uovnv.nyc3.digitaloceanspaces.com/UO%20VnV/UOVnV.csv";

        public static readonly string VERSION_URL = "https://uovnv.com/serverfiles/version.txt";
        public static readonly string APPLICATION_ICON_URL = "https://uovnv.com/serverfiles/cropped-Head2-300x210.ico"; // Needs to be .ico
        public static readonly string LOGO_URL = "https://uovnv.com/serverfiles/cropped-Head2-300x210.png"; // Ideally around 283x75
        public static readonly string BACKGROUND_URL = "https://uovnv.com/serverfiles/cropped-Head2.png";
        public static readonly string PATCH_NOTES_URL = "https://uovnv.com/serverfiles/UO%20VnV/updates.xml";
        public static readonly string CLIENT_DOWNLOAD_URL = "https://uovnv.com/serverfiles/UO VnV.zip";

        /// <summary>
        /// Navigation bar buttons
        /// </summary>
        public static readonly string NAVBAR_BUTTON_1_TEXT = "Website";
        public static readonly string NAVBAR_BUTTON_1_URL = "https://uovnv.com";
        public static readonly string NAVBAR_BUTTON_2_TEXT = "Wiki";
        public static readonly string NAVBAR_BUTTON_2_URL = "https://uovnv.com/wiki";
        public static readonly string NAVBAR_BUTTON_3_TEXT = "Community";
        public static readonly string NAVBAR_BUTTON_3_URL = "https://uovnv.com/forum";
        public static readonly string NAVBAR_BUTTON_4_TEXT = "Support";
        public static readonly string NAVBAR_BUTTON_4_URL = "https://github.com/";
        public static readonly string NAVBAR_BUTTON_5_TEXT = "Discord";
        public static readonly string NAVBAR_BUTTON_5_URL = "https://github.com/";

        // Modify this array if you're adding or removing a button
        public static readonly string[] NAVBAR_BUTTON_TEXT_ARRAY = { NAVBAR_BUTTON_1_TEXT, NAVBAR_BUTTON_2_TEXT, NAVBAR_BUTTON_3_TEXT };//,
                                                                    //NAVBAR_BUTTON_4_TEXT, NAVBAR_BUTTON_5_TEXT };

        /// <summary>
        /// Settings
        /// </summary>
        public static bool SHOW_VERSION_TEXT = true;
        public static bool AUTOMATICALLY_BEGIN_UPDATING = false;
        public static bool AUTOMATICALLY_LAUNCH_GAME_AFTER_UPDATING = false;
        public static bool SHOW_ERROR_BOX_IF_PATCH_NOTES_DOWNLOAD_FAILS = true;

    }
}
