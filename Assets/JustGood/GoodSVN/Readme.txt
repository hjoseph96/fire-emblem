Good SVN
--------------------------------------


Authors
-------
Jesse Graupmann
Tim Graupmann


Audience
--------

Package contents are targeted towards artists and programmers that want cross-platform-native SVN support.


Compatibility
-------------

This project is targeted for Unity 3.5.7 or better.


Contents
--------

This package contains full source for the Good SVN package.


Menu Items
----------

Window->Just Good Design->Open Good SVN - The Good SVN Panel can be accessed via the menu.

Window->Just Good Design->Links->Good SVN - Opens the Good SVN Product Page

Window->Just Good Design->Links->Tools->CrossOver - CrossOver allows you to run Windows Apps on MacOS

Window->Just Good Design->Links->Tools->Putty - Putty has pageant to store your SVN SSH keys

Window->Just Good Design->Links->Tools->TortoiseSVN - Opens the TortoiseSVN version control product page

Window->Just Good Design->Links->Tools->Winmerge - Is great and free version comparison tool


Mac Platform
------------

Good SVN has a hook available using TortoiseSVN on Mac which requires CrossOver office.

Launch the CrosssOver application, and select "Manage Bottles".

Create a new 'WinXP' bottle named 'TortoiseSVN' by clicking the plus button.

This same bottle will hold several installs, which is not the default option when

installing new software with crossover.

Close the "Manage Bottles" dialog and in the Configure menu select "Install Software".

Near the top of the installer, if given the choice, choose "Always Update".

Enter "Putty" into the Selectan application to install.

Expand "Commuity Supported Applications" where you'll find "Putty".

In the "Will install into a new winxp bottle 'Putty', change that to the existing 'TortoiseSVN'.

Install "TortoiseSVN" and "WinMerge" in the same way to that same 'TortoiseSVN' bottle.

If desired you can repeat the same process and create separate bottles for SVN 1.6 and 1.7 that

can live side by side. Mac by default has SVN 1.6 command-line support. And this CrossOver setup

is the fastest way to add support for SVN 1.7 without a bunch of command line hackery.

You can access the preferences in the standard preferences with the Unity->Preferences menu item.


Windows
-------

On Windows, you simply need to install the TortoiseSVN installer with the command-line tools.

You can access the preferences in the standard preferences with the Edit->Preferences menu item.


Preferences
-----------

You'll find 'G|SVN' in the standard preferences.

If the SVN Type is set to TortoiseSVN, additional options appear.

On Windows, you can set the path to TortoiseProc.exe which is a command-line tool within the

TortoiseSVN. It requires that the command-line tools for TortoiseSVN were installed.

On Mac, you need to enter the Bottle Name which TortoiseSVN is installed. The preferences

default to "TortoiseSVN". Here is were you could choose between SVN 1.6 and 1.7 using side

by side installs.


Good SVN Panel
--------------

Good SVN has areas divided out into "Assets", "Project Settings", "Files, and "Commands".

"Assets" has common functions:

> Update - Runs SVN Update in the Assets folder

> Commit - Runs SVN Commit in the Assets folder

> Add - Runs SVN Add in the Assets folder

"Project Settings" has common functions:

> Update - Runs SVN Update in the ProjectSettings folder

> Commit - Runs SVN Commit in the ProjectSettings folder

> Add - Runs SVN Add in the ProjectSettings folder

"File" does operations to folders and files selected in the project view:

> Update - Runs SVN Update on the selected file(s)

> Commit - Runs SVN Commit on the selected file(s)

> Add - Runs SVN Add on the selected file(s)

> Remove - Runs SVN Remove on the selected file(s)

> Lock - Locks the selected file(s)

> Unlock - Unlock the selected file(s)

> Diff - Runs SVN diff on the selected file(s)

> Rename - Renames on the selected file(s) including a meta file popup

> Edit Conflicts - Opens edit conflicts on the selected file(s)

"Commands" has common functions:

> Revert - Runs SVN revert on the selected file(s)

> Status - Runs SVN status on the selected file(s)

> Log - Runs SVN log on the selected file(s)

> Browser - Opens the repro browser on the selected file(s)

> CleanUp - Runs an SVN cleanup at the root level


Support
-------

You can send comments/questions to unity@justgooddesign.com and please post in the forums to share

how you like this product or to make feature requests.

http://forum.unity3d.com/threads/169759-Good-SVN-Native-SVN-Support-in-Unity-on-Mac-Windows


Change Log
----------

1.0 Initial launch of product

1.1 Removed editor dependency on Litjson

1.2 Removed dependency on litjson. Added screenshots. Updated icons. Updated version

1.3 Made project compatible with Good GIT