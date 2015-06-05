@ECHO OFF
CLS

REM This is an example script to show how to use the Help Library Manager Launcher to remove an MS Help Viewer
REM file.  You can use this as an example for creating a script to run from your product's uninstaller.

REM NOTE: If not executed from within the same folder as the executable, a full path is required on the
REM executable.

IF "%1%"=="H2" GOTO HelpViewer2
IF "%1%"=="h2" GOTO HelpViewer2
IF "%1%"=="H21" GOTO HelpViewer21
IF "%1%"=="h21" GOTO HelpViewer21

REM Help Viewer 1.0
HelpLibraryManagerLauncher.exe /product "VS" /version "100" /locale en-us /uninstall /silent /vendor "Beatsleigher (originally created by Regaw-Leinad [AKA Dan Wager])" /productName "AndroidLib" /mediaBookList "AndroidLib - Beatsleigher Edition"

GOTO Exit

:HelpViewer2

REM Help Viewer 2.0
HelpLibraryManagerLauncher.exe /viewerVersion 2.0 /catalogName VisualStudio11 /locale en-us /wait 0 /operation uninstall /vendor "Beatsleigher (originally created by Regaw-Leinad [AKA Dan Wager])" /productName "AndroidLib" /bookList "AndroidLib - Beatsleigher Edition"

GOTO Exit

:HelpViewer21

REM Help Viewer 2.1
HelpLibraryManagerLauncher.exe /viewerVersion 2.1 /catalogName VisualStudio11 /locale en-us /wait 0 /operation uninstall /vendor "Beatsleigher (originally created by Regaw-Leinad [AKA Dan Wager])" /productName "AndroidLib" /bookList "AndroidLib - Beatsleigher Edition"

:Exit
