Thank you for buying my VRVRTK.Weapons asset pack!

There are a couple things I couldn't automate, that you need to do to get started:

1. Ensure you have the current version of SteamVR in your current project.
2. Add the "VRWControl" and SteamVR's Player prefab into your scene. The Player prefab is found in SteamVR->InteractionSystem->Core->Prefabs.
3. Add my "VRTK.Weapon.cs" script to the object you want to use as your VRTK.Weapon.
4. In the inspector, you will find all necessary settings and a "Build VRTK.Weapon" button to walk you through the construction of your gun.
5. Add the HealthManager script to anything intended to take shots, to manage health/destruction of objects. It can also be used to call functions from any object you specify, allowing much more flexibility.
6. For dynamic impacts, click the "Set up Impact Profile" button. This will create a new asset under VRVRTK.Weapons/Impact Profiles. Drag and drop your impact prefabs on to the default slot if you don't need dynamic
	impacts. Then, for each impact, assign it a material and an impact prefab (or multiple, if you want different variations). Then, simply drag the impact profile onto the Impact Profile slot on the VRTK.Weapon.
	NOTE: VRW does not currently come with any impact prefabs. Plenty available on the asset store, though :)
7. Buttons are re-mappable through VRWControl. (Note: Because of the way SteamVR enumerates button inputs, things may not appear correct at first glance. For example, if you select "K_E Button_Steam VR_Trigger"
	it will automatically change to "K_E Button_Axis 1." This is normal and works as intended.)
NOTE: If the test scene is not working properly, make sure you have clicked the "Set up layers and tags" button on a VRTK.Weapon first.

If you run into any problems, I have a few walkthrough videos up on my channel: https://www.youtube.com/channel/UChC2f8-8uX5E0BBYBrLP3Gw

If you are still having issues, you can reach me at slayd7@gmail.com or on Twitter @Slayd7. Also, we have a Discord now: https://discord.gg/SPG8fch Come stop by, ask questions, chat! I'm usually online, and it's
	the easiest way to get real-time help.

If you have the time, leaving me an honest review would really help me out. The only thing I ask is that if you're having an issue with VRW, please contact me before leaving a negative review, and give me a 
	chance to make it right!

Thanks, and enjoy!
-Brad

----------------------------------------------
	Common problems
----------------------------------------------

In the test scene, I am not able to push layer and tag information through the Asset Store. To fix this, select the VRTK.Weapons and hit the "Set Up Layers and Tags" button. If you don't, you will have errors when you 
	hit play.

I did not want to force collider positions on the controllers, so I did not put any on the controllers. If you're in VRTK mode, make sure you have colliders on your controllers and that they are triggers. If you
	don't, you will be unable to pick up or interact with the VRTK.Weapons.

----------------------------------------------
	Random things of note
----------------------------------------------

VRTK.Weapons chamber accurately, depending on how quickly your bolt makes a full cycle. So, for example: If you have an automatic VRTK.Weapon with a 0.1 fire rate (10 rounds per second) but your bolt's slide speed is slower
	than that (5 slide speed = 5 frames back, 5 frames forward, 10 frames total, running at 90fps would make that 10/90 seconds to chamber a new round, or 0.1111 seconds) the VRTK.Weapon may fail to fire - because it
	isn't cycling the bolt fast enough! So either adjust your slide speed or your fire rate. Same goes for Autofire Proj and Burst fire.

Bolt-action style rifles are possible - simply change the "Rotate Until" option to 1, and set your rotation positions. You will not have to set up slide max and min limits, but you WILL still need to set up Linear 
	Drive start and end positions under the "Slide max and min limits" menu.

