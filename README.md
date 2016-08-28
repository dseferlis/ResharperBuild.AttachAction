# ResharperBuild.AttachAction
Plugin for Resharper's Build/Run feature.

Simulates the Visual Studio attach procedure in a better way by first starting the process and then attaching to it.

[[https://github.com/dseferlis/ResharperBuild.AttachAction/blob/master/Screenshots/Setup.png|alt=Setup]]

The need of this plugin
-----------------------
While developing some modules for a third party application I was unable to F5 directly from Visual Studio, attach the debugger and debug the modules.

This was happening because this application was running under .net 2.0/3.5 and my module was on 4.0. Tried several things to make this work.

First of all of course tried F5 but there was some software licensing error when third party app was starting and not able to continue.

Then added an app.exe.config making the app start under 2.0/3.5 using:
~~~
<startup>
    <supportedRuntime version="v2.0.50727"/>
</startup>
~~~
Problem with this was that debugger was never actualy attached. No breakpoint was hitting.

So the only way I was able to debug was to use VS "Attach to Process.." after launching the app manually and selecting the "Managed (v4.6, v4.5, v4.0)" Code Type.

For some time used a great VS extension called [ReAttach](https://github.com/erlandranvinge/ReAttach) from Erland Ranvinge, which helped minimize some keystrokes.
But still, I had to launch the app myself.

The solution
------------
Started thinking to edit ReAttach source in order to also launch the app, but the enthusiasm with Resharper kept me back!

After reading the [ReSharper DevGuide](https://www.jetbrains.com/help/resharper/sdk/README.html) for some hours there were not much about the Build feature.
So I made a question on [stackoverflow](http://stackoverflow.com/questions/39105401/resharper-run-configurations) where the answer of [Matt Ellis](https://twitter.com/citizenmatt) was really detailed and helpful!
After much digging inside Jetbrains assemblies made this plugin.

I hope this will help some of you and also some will contribute to it.

Also some special thanks to [Julien Lebosquain](https://github.com/MrJul) for project setup after examining his [ReSharper.EnhancedTooltip](https://github.com/MrJul/ReSharper.EnhancedTooltip).