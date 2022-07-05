# CustomStreamPlayers

Requires [SpecialItemsLib](https://github.com/art0007i/SpecialItemsLib)!<br>
A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that allows you to select a custom audio stream player from your inventory and have it spawn whenever you stream audio.<br>
To create a custom stream player just attach the `AudioStreamController` component and save the item, whenever you favorite it and spawn a stream the `_stream` field will have the OpusStream which you can then use for anything you want. Optionally you can fill the `_audioOutput` field which will set the source of the audio output to your OpusStream.<br>
Because of how the game generates stream players spawning and saving the default stream player will break the player.<br>This is why I created a 'fixed' default stream player and a couple of other example players in this public folder `neosrec:///U-art0007i/R-b6abfeab-4a16-423d-9192-1402ef7bc671`.

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
1. Place [CustomStreamPlayers.dll](https://github.com/art0007i/CustomStreamPlayers/releases/latest/download/CustomStreamPlayers.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.
